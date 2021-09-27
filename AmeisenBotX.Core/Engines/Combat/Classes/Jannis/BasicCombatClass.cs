using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Combat.Helpers;
using AmeisenBotX.Core.Engines.Combat.Helpers.Aura;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public abstract class BasicCombatClass : ICombatClass
    {
        private const float MAX_ANGLE = MathF.PI * 2.0f;

        private readonly int[] useableHealingItems = new int[]
        {
            // potions
            118, 929, 1710, 2938, 3928, 4596, 5509, 13446, 22829, 33447,
            // healthstones
            5509, 5510, 5511, 5512, 9421, 19013, 22103, 36889, 36892,
        };

        private readonly int[] useableManaItems = new int[]
        {
            // potions
            2245, 3385, 3827, 6149, 13443, 13444, 33448, 22832,
        };

        protected BasicCombatClass(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            SpellAbortFunctions = new();

            CooldownManager = new(Bot.Character.SpellBook.Spells);
            RessurrectionTargets = new();

            TargetProviderDps = new TargetManager(new DpsTargetSelectionLogic(Bot), TimeSpan.FromMilliseconds(250));
            TargetProviderTank = new TargetManager(new TankTargetSelectionLogic(Bot), TimeSpan.FromMilliseconds(250));
            TargetProviderHeal = new TargetManager(new HealTargetSelectionLogic(Bot), TimeSpan.FromMilliseconds(250));

            MyAuraManager = new(Bot);
            TargetAuraManager = new(Bot);
            GroupAuraManager = new(Bot);

            InterruptManager = new();

            EventCheckFacing = new(TimeSpan.FromMilliseconds(500));
            EventAutoAttack = new(TimeSpan.FromMilliseconds(500));

            Configurables = new()
            {
                { "HealthItemThreshold", 30.0 },
                { "ManaItemThreshold", 30.0 }
            };
        }

        public string Author { get; } = "Jannis";

        public IEnumerable<int> BlacklistedTargetDisplayIds { get => TargetProviderDps.BlacklistedTargets; set => TargetProviderDps.BlacklistedTargets = value; }

        public Dictionary<string, dynamic> Configurables { get; set; }

        public CooldownManager CooldownManager { get; private set; }

        public abstract string Description { get; }

        public abstract string DisplayName { get; }

        public TimegatedEvent EventAutoAttack { get; private set; }

        public TimegatedEvent EventCheckFacing { get; set; }

        public GroupAuraManager GroupAuraManager { get; private set; }

        public bool HandlesFacing => true;

        public abstract bool HandlesMovement { get; }

        public InterruptManager InterruptManager { get; private set; }

        public abstract bool IsMelee { get; }

        public bool IsWanding { get; private set; }

        public abstract IItemComparator ItemComparator { get; set; }

        public AuraManager MyAuraManager { get; private set; }

        public IEnumerable<int> PriorityTargetDisplayIds { get => TargetProviderDps.PriorityTargets; set => TargetProviderDps.PriorityTargets = value; }

        public Dictionary<string, DateTime> RessurrectionTargets { get; private set; }

        public abstract WowRole Role { get; }

        public abstract TalentTree Talents { get; }

        public AuraManager TargetAuraManager { get; private set; }

        public ITargetProvider TargetProviderDps { get; private set; }

        public ITargetProvider TargetProviderHeal { get; private set; }

        public ITargetProvider TargetProviderTank { get; private set; }

        public abstract bool UseAutoAttacks { get; }

        public abstract string Version { get; }

        public abstract bool WalkBehindEnemy { get; }

        public abstract WowClass WowClass { get; }

        protected AmeisenBotInterfaces Bot { get; }

        protected DateTime LastSpellCast { get; private set; }

        protected List<Func<bool>> SpellAbortFunctions { get; }

        public virtual void AttackTarget()
        {
            IWowUnit target = Bot.Target;

            if (target == null)
            {
                return;
            }

            if (Bot.Player.Position.GetDistance(target.Position) <= 3.0f)
            {
                Bot.Wow.StopClickToMove();
                Bot.Movement.Reset();
                Bot.Wow.InteractWithUnit(target.BaseAddress);
            }
            else
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);
            }
        }

        public virtual void Execute()
        {
            if (Bot.Player.IsCasting)
            {
                if (!Bot.Objects.IsTargetInLineOfSight || SpellAbortFunctions.Any(e => e()))
                {
                    Bot.Wow.StopCasting();
                }

                return;
            }

            if (Bot.Target != null && EventCheckFacing.Run())
            {
                CheckFacing(Bot.Target);
            }

            // Update Priority Units
            // --------------------------- >
            if (Bot.Dungeon.Profile != null
                && Bot.Dungeon.Profile.PriorityUnits != null
                && Bot.Dungeon.Profile.PriorityUnits.Count > 0)
            {
                TargetProviderDps.PriorityTargets = Bot.Dungeon.Profile.PriorityUnits;
            }

            // Autoattacks
            // --------------------------- >
            if (UseAutoAttacks)
            {
                IsWanding = Bot.Character.SpellBook.IsSpellKnown("Shoot")
                    && Bot.Character.Equipment.Items.ContainsKey(WowEquipmentSlot.INVSLOT_RANGED)
                    && (WowClass == WowClass.Priest || WowClass == WowClass.Mage || WowClass == WowClass.Warlock)
                    && (IsWanding || TryCastSpell("Shoot", Bot.Wow.TargetGuid));

                if (!IsWanding
                    && EventAutoAttack.Run()
                    && !Bot.Player.IsAutoAttacking
                    && Bot.Player.IsInMeleeRange(Bot.Target))
                {
                    Bot.Wow.StartAutoAttack();
                }
            }

            // Buffs, Debuffs, Interrupts
            // --------------------------- >
            if (MyAuraManager.Tick(Bot.Player.Auras)
                || GroupAuraManager.Tick())
            {
                return;
            }

            if (Bot.Target != null
                && TargetAuraManager.Tick(Bot.Target.Auras))
            {
                return;
            }

            if (InterruptManager.Tick(Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, IsMelee ? 5.0f : 30.0f)))
            {
                return;
            }

            // Useable items, potions, etc.
            // ---------------------------- >
            if (Bot.Player.HealthPercentage < Configurables["HealthItemThreshold"])
            {
                IWowInventoryItem healthItem = Bot.Character.Inventory.Items.FirstOrDefault(e => useableHealingItems.Contains(e.Id));

                if (healthItem != null)
                {
                    Bot.Wow.UseItemByName(healthItem.Name);
                }
            }

            if (Bot.Player.ManaPercentage < Configurables["ManaItemThreshold"])
            {
                IWowInventoryItem manaItem = Bot.Character.Inventory.Items.FirstOrDefault(e => useableManaItems.Contains(e.Id));

                if (manaItem != null)
                {
                    Bot.Wow.UseItemByName(manaItem.Name);
                }
            }

            // Race abilities
            // -------------- >
            if (Bot.Player.Race == WowRace.Human
                && (Bot.Player.IsDazed
                    || Bot.Player.IsFleeing
                    || Bot.Player.IsInfluenced
                    || Bot.Player.IsPossessed)
                && TryCastSpell("Every Man for Himself", 0))
            {
                return;
            }

            if (Bot.Player.HealthPercentage < 50.0
                && ((Bot.Player.Race == WowRace.Draenei && TryCastSpell("Gift of the Naaru", 0))
                    || (Bot.Player.Race == WowRace.Dwarf && TryCastSpell("Stoneform", 0))))
            {
                return;
            }
        }

        public virtual void Load(Dictionary<string, JsonElement> objects)
        {
            if (objects.ContainsKey("Configureables")) { Configurables = objects["Configureables"].ToDyn(); }
        }

        public virtual void OutOfCombatExecute()
        {
            if (Bot.Player.IsCasting)
            {
                if (!Bot.Objects.IsTargetInLineOfSight || SpellAbortFunctions.Any(e => e()))
                {
                    Bot.Wow.StopCasting();
                }

                return;
            }

            if ((Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Food") && Bot.Player.HealthPercentage < 100.0)
                || (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Drink") && Bot.Player.ManaPercentage < 100.0))
            {
                return;
            }

            if (MyAuraManager.Tick(Bot.Player.Auras)
                || GroupAuraManager.Tick())
            {
                return;
            }
        }

        public virtual Dictionary<string, object> Save()
        {
            return new() { { "Configureables", Configurables } };
        }

        public override string ToString()
        {
            return $"[{WowClass}] [{Role}] {DisplayName} ({Author})";
        }

        protected bool CheckForWeaponEnchantment(WowEquipmentSlot slot, string enchantmentName, string spellToCastEnchantment)
        {
            if (Bot.Character.Equipment.Items.ContainsKey(slot))
            {
                int itemId = Bot.Character.Equipment.Items[slot].Id;

                if (itemId > 0)
                {
                    IWowItem item = Bot.Objects.WowObjects.OfType<IWowItem>().FirstOrDefault(e => e.EntryId == itemId);

                    if (item != null
                        && !item.GetEnchantmentStrings().Any(e => e.Contains(enchantmentName, StringComparison.OrdinalIgnoreCase))
                        && TryCastSpell(spellToCastEnchantment, 0, true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected string GetFolder()
        {
            return GetType().FullName.ToLower().Replace("ameisenbotx.core.engines.", string.Empty);
        }

        protected bool HandleDeadPartymembers(string spellName)
        {
            Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

            if (spell != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && spell.Costs < Bot.Player.Mana)
            {
                IEnumerable<IWowPlayer> groupPlayers = Bot.Objects.Partymembers
                    .OfType<IWowPlayer>()
                    .Where(e => e.IsDead);

                if (groupPlayers.Any())
                {
                    IWowPlayer player = groupPlayers.FirstOrDefault(e => Bot.Db.GetUnitName(e, out string name) && !RessurrectionTargets.ContainsKey(name) || RessurrectionTargets[name] < DateTime.Now);

                    if (player != null)
                    {
                        if (Bot.Db.GetUnitName(player, out string name))
                        {
                            if (!RessurrectionTargets.ContainsKey(name))
                            {
                                RessurrectionTargets.Add(name, DateTime.Now + TimeSpan.FromSeconds(10));
                                return TryCastSpell(spellName, player.Guid, true);
                            }

                            if (RessurrectionTargets[name] < DateTime.Now)
                            {
                                return TryCastSpell(spellName, player.Guid, true);
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        protected bool IsInRange(Spell spell, IWowUnit wowUnit)
        {
            if ((spell.MinRange == 0 && spell.MaxRange == 0) || spell.MaxRange == 0)
            {
                return Bot.Player.IsInMeleeRange(wowUnit);
            }

            double distance = Bot.Player.Position.GetDistance(wowUnit.Position);
            return distance >= spell.MinRange && distance <= spell.MaxRange - 1.0;
        }

        protected bool SelectTarget(ITargetProvider targetProvider)
        {
            if (targetProvider.Get(out IEnumerable<IWowUnit> targetToTarget))
            {
                if (targetToTarget != null && targetToTarget.Any())
                {
                    ulong guid = targetToTarget.First().Guid;

                    if (Bot.Player.TargetGuid != guid)
                    {
                        Bot.Wow.ChangeTarget(guid);
                        Bot.Objects.Player.Update(Bot.Memory, Bot.Wow.Offsets);
                    }
                }
            }

            return Bot.Target != null
                && IWowUnit.IsValidUnit(Bot.Target)
                && !Bot.Target.IsDead;
        }

        protected bool TryCastAoeSpell(string spellName, ulong guid, bool needsResource = false, int currentResourceAmount = 0, bool forceTargetSwitch = false)
        {
            if (TryCastSpell(spellName, guid, needsResource, currentResourceAmount, forceTargetSwitch))
            {
                if (ValidateTarget(guid, out IWowUnit target, out bool _))
                {
                    Bot.Wow.ClickOnTerrain(target.Position);
                    LastSpellCast = DateTime.UtcNow;
                    return true;
                }
            }

            return false;
        }

        protected bool TryCastAoeSpellDk(string spellName, ulong guid, bool needsRuneenergy = false, bool needsBloodrune = false, bool needsFrostrune = false, bool needsUnholyrune = false, bool forceTargetSwitch = false)
        {
            if (TryCastSpellDk(spellName, guid, needsRuneenergy, needsBloodrune, needsFrostrune, needsUnholyrune, forceTargetSwitch))
            {
                if (ValidateTarget(guid, out IWowUnit target, out bool _))
                {
                    Bot.Wow.ClickOnTerrain(target.Position);
                    LastSpellCast = DateTime.UtcNow;
                    return true;
                }
            }

            return false;
        }

        protected bool TryCastSpell(string spellName, ulong guid, bool needsResource = false, int currentResourceAmount = 0, bool forceTargetSwitch = false)
        {
            if (!Bot.Character.SpellBook.IsSpellKnown(spellName) || ((guid != 0 && guid != Bot.Wow.PlayerGuid) && !Bot.Objects.IsTargetInLineOfSight)) { return false; }

            if (ValidateTarget(guid, out IWowUnit target, out bool needToSwitchTarget))
            {
                if (currentResourceAmount == 0)
                {
                    currentResourceAmount = Bot.Player.Class switch
                    {
                        WowClass.Deathknight => Bot.Player.Runeenergy,
                        WowClass.Rogue => Bot.Player.Energy,
                        WowClass.Warrior => Bot.Player.Rage,
                        _ => Bot.Player.Mana,
                    };
                }

                bool isTargetMyself = guid == 0;
                Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                if (spell != null
                    && !CooldownManager.IsSpellOnCooldown(spellName)
                    && (!needsResource || spell.Costs < currentResourceAmount)
                    && (target == null || IsInRange(spell, target)))
                {
                    if (!isTargetMyself && (needToSwitchTarget || forceTargetSwitch))
                    {
                        Bot.Wow.ChangeTarget(guid);
                    }

                    if (!isTargetMyself
                        && !BotMath.IsFacing(Bot.Player.Position, Bot.Player.Rotation, target.Position))
                    {
                        Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);
                    }

                    if (spell.CastTime > 0)
                    {
                        // stop pending movement if we cast something
                        Bot.Movement.StopMovement();
                        Bot.Movement.PreventMovement(TimeSpan.FromMilliseconds(spell.CastTime));
                        CheckFacing(target);
                    }

                    LastSpellCast = DateTime.UtcNow;
                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        protected bool TryCastSpellDk(string spellName, ulong guid, bool needsRuneenergy = false, bool needsBloodrune = false, bool needsFrostrune = false, bool needsUnholyrune = false, bool forceTargetSwitch = false)
        {
            if (!Bot.Character.SpellBook.IsSpellKnown(spellName) || ((guid != 0 && guid != Bot.Wow.PlayerGuid) && !Bot.Objects.IsTargetInLineOfSight)) { return false; }

            if (ValidateTarget(guid, out IWowUnit target, out bool needToSwitchTarget))
            {
                bool isTargetMyself = guid == 0;
                Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);
                Dictionary<int, int> runes = Bot.Wow.GetRunesReady();

                if (spell != null
                    && !CooldownManager.IsSpellOnCooldown(spellName)
                    && (!needsRuneenergy || spell.Costs < Bot.Player.Runeenergy)
                    && (!needsBloodrune || (runes[(int)WowRuneType.Blood] > 0 || runes[(int)WowRuneType.Death] > 0))
                    && (!needsFrostrune || (runes[(int)WowRuneType.Frost] > 0 || runes[(int)WowRuneType.Death] > 0))
                    && (!needsUnholyrune || (runes[(int)WowRuneType.Unholy] > 0 || runes[(int)WowRuneType.Death] > 0))
                    && (target == null || IsInRange(spell, target)))
                {
                    if (!isTargetMyself && (needToSwitchTarget || forceTargetSwitch))
                    {
                        Bot.Wow.ChangeTarget(guid);
                    }

                    if (!isTargetMyself
                        && !BotMath.IsFacing(Bot.Player.Position, Bot.Player.Rotation, target.Position))
                    {
                        Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);
                    }

                    if (spell.CastTime > 0)
                    {
                        // stop pending movement if we cast something
                        Bot.Movement.StopMovement();
                        Bot.Movement.PreventMovement(TimeSpan.FromMilliseconds(spell.CastTime));
                        CheckFacing(target);
                    }

                    LastSpellCast = DateTime.UtcNow;
                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        protected bool TryCastSpellRogue(string spellName, ulong guid, bool needsEnergy = false, bool needsCombopoints = false, int requiredCombopoints = 1, bool forceTargetSwitch = false)
        {
            if (!Bot.Character.SpellBook.IsSpellKnown(spellName) || ((guid != 0 && guid != Bot.Wow.PlayerGuid) && !Bot.Objects.IsTargetInLineOfSight)) { return false; }

            if (ValidateTarget(guid, out IWowUnit target, out bool needToSwitchTarget))
            {
                bool isTargetMyself = guid == 0;
                Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                if (spell != null
                    && !CooldownManager.IsSpellOnCooldown(spellName)
                    && (!needsEnergy || spell.Costs < Bot.Player.Energy)
                    && (!needsCombopoints || Bot.Player.ComboPoints >= requiredCombopoints)
                    && (target == null || IsInRange(spell, target)))
                {
                    if (!isTargetMyself && (needToSwitchTarget || forceTargetSwitch))
                    {
                        Bot.Wow.ChangeTarget(guid);
                    }

                    if (!isTargetMyself
                        && !BotMath.IsFacing(Bot.Player.Position, Bot.Player.Rotation, target.Position))
                    {
                        Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);
                    }

                    if (spell.CastTime > 0)
                    {
                        // stop pending movement if we cast something
                        Bot.Movement.StopMovement();
                        Bot.Movement.PreventMovement(TimeSpan.FromMilliseconds(spell.CastTime));
                        CheckFacing(target);
                    }

                    LastSpellCast = DateTime.UtcNow;
                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        protected bool TryCastSpellWarrior(string spellName, string requiredStance, ulong guid, bool needsResource = false, int currentResourceAmount = 0, bool forceTargetSwitch = false)
        {
            if (!Bot.Character.SpellBook.IsSpellKnown(spellName) || ((guid != 0 && guid != Bot.Wow.PlayerGuid) && !Bot.Objects.IsTargetInLineOfSight)) { return false; }

            if (ValidateTarget(guid, out IWowUnit target, out bool needToSwitchTarget))
            {
                if (currentResourceAmount == 0)
                {
                    currentResourceAmount = Bot.Player.Rage;
                }

                bool isTargetMyself = guid == 0;
                Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                if (spell != null
                    && !CooldownManager.IsSpellOnCooldown(spellName)
                    && (!needsResource || spell.Costs < currentResourceAmount)
                    && (target == null || IsInRange(spell, target)))
                {
                    if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == requiredStance)
                        && Bot.Character.SpellBook.IsSpellKnown(requiredStance)
                        && !CooldownManager.IsSpellOnCooldown(requiredStance))
                    {
                        CastSpell(requiredStance, true);
                    }

                    if (!isTargetMyself
                        && !BotMath.IsFacing(Bot.Player.Position, Bot.Player.Rotation, target.Position))
                    {
                        Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);
                    }

                    if (!isTargetMyself && (needToSwitchTarget || forceTargetSwitch))
                    {
                        Bot.Wow.ChangeTarget(guid);
                    }

                    if (spell.CastTime > 0)
                    {
                        // stop pending movement if we cast something
                        Bot.Movement.StopMovement();
                        Bot.Movement.PreventMovement(TimeSpan.FromMilliseconds(spell.CastTime));
                        CheckFacing(target);
                    }

                    LastSpellCast = DateTime.UtcNow;
                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        private bool CastSpell(string spellName, bool castOnSelf)
        {
            // spits out stuff like this "1;300" (1 or 0 whether the cast was successful or not);(the cooldown in ms)
            if (Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:3}},{{v:4}}=GetSpellCooldown(\"{spellName}\"){{v:2}}=({{v:3}}+{{v:4}}-GetTime())*1000;if {{v:2}}<=0 then {{v:2}}=0;CastSpellByName(\"{spellName}\"{(castOnSelf ? ", \"player\"" : string.Empty)}){{v:5}},{{v:6}}=GetSpellCooldown(\"{spellName}\"){{v:1}}=({{v:5}}+{{v:6}}-GetTime())*1000;{{v:0}}=\"1;\"..{{v:1}} else {{v:0}}=\"0;\"..{{v:2}} end"), out string result))
            {
                if (result.Length < 3)
                {
                    return false;
                }

                string[] parts = result.Split(";", StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 2)
                {
                    return false;
                }

                // replace comma with dot in the cooldown
                if (parts[1].Contains(',', StringComparison.OrdinalIgnoreCase))
                {
                    parts[1] = parts[1].Replace(',', '.');
                }

                if (int.TryParse(parts[0], out int castSuccessful)
                    && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double cooldown))
                {
                    cooldown = Math.Max(cooldown, 0);
                    CooldownManager.SetSpellCooldown(spellName, (int)cooldown);

                    if (castSuccessful == 1)
                    {
                        AmeisenLogger.I.Log("CombatClass", $"[{DisplayName}]: Casting Spell \"{spellName}\" on \"{Bot.Target?.Guid}\"", LogLevel.Verbose);
                        IsWanding = IsWanding && spellName == "Shoot";
                        return true;
                    }
                    else
                    {
                        AmeisenLogger.I.Log("CombatClass", $"[{DisplayName}]: Spell \"{spellName}\" is on cooldown for \"{cooldown}\"ms", LogLevel.Verbose);
                        return false;
                    }
                }
            }

            return false;
        }

        private void CheckFacing(IWowUnit target)
        {
            if (target == null || target.Guid == Bot.Wow.PlayerGuid)
            {
                return;
            }

            float facingAngle = BotMath.GetFacingAngle(Bot.Player.Position, target.Position);
            float angleDiff = facingAngle - Bot.Player.Rotation;

            if (angleDiff < 0)
            {
                angleDiff += MAX_ANGLE;
            }
            else if (angleDiff > MAX_ANGLE)
            {
                angleDiff -= MAX_ANGLE;
            }

            if (angleDiff > 1.0)
            {
                Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);
            }
        }

        private bool ValidateTarget(ulong guid, out IWowUnit target, out bool needToSwitchTargets)
        {
            if (guid == 0)
            {
                target = Bot.Player;
                needToSwitchTargets = false;
                return true;
            }
            else if (guid == Bot.Wow.TargetGuid)
            {
                target = Bot.Target;
                needToSwitchTargets = false;
                return true;
            }
            else
            {
                target = Bot.GetWowObjectByGuid<IWowUnit>(guid);
                needToSwitchTargets = true;
                return target != null;
            }
        }
    }
}