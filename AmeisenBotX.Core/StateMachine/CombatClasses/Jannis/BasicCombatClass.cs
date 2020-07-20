using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.States;
using AmeisenBotX.Core.Statemachine.Utils;
using AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public abstract class BasicCombatClass : ICombatClass
    {
        protected BasicCombatClass(WowInterface wowInterface, AmeisenBotStateMachine stateMachine)
        {
            WowInterface = wowInterface;
            StateMachine = stateMachine;

            CooldownManager = new CooldownManager(WowInterface.CharacterManager.SpellBook.Spells);
            RessurrectionTargets = new Dictionary<string, DateTime>();

            ITargetSelectionLogic targetSelectionLogic = Role switch
            {
                CombatClassRole.Dps => targetSelectionLogic = new DpsTargetSelectionLogic(wowInterface),
                CombatClassRole.Heal => targetSelectionLogic = new HealTargetSelectionLogic(wowInterface),
                CombatClassRole.Tank => targetSelectionLogic = new TankTargetSelectionLogic(wowInterface),
                _ => null,
            };

            TargetManager = new TargetManager(targetSelectionLogic, TimeSpan.FromMilliseconds(250));

            Spells = new Dictionary<string, Spell>();
            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                Spells.Clear();

                foreach (Spell spell in WowInterface.CharacterManager.SpellBook.Spells.OrderBy(e => e.Rank).GroupBy(e => e.Name).Select(e => e.First()))
                {
                    if (!Spells.ContainsKey(spell.Name))
                    {
                        Spells.Add(spell.Name, spell);
                    }
                }
            };

            MyAuraManager = new AuraManager
            (
                TimeSpan.Zero,
                () => WowInterface.ObjectManager.Player?.Auras
            );

            TargetAuraManager = new AuraManager
            (
                TimeSpan.Zero,
                () => WowInterface.ObjectManager.Target?.Auras
            );

            GroupAuraManager = new GroupAuraManager(WowInterface);

            TargetInterruptManager = new InterruptManager(new List<WowUnit>() { WowInterface.ObjectManager.Target }, null);

            NearInterruptUnitsEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(250));
            UpdatePriorityUnits = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));
            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));
        }

        public abstract string Author { get; }

        public TimegatedEvent AutoAttackEvent { get; private set; }

        public abstract WowClass Class { get; }

        public abstract Dictionary<string, dynamic> Configureables { get; set; }

        public CooldownManager CooldownManager { get; private set; }

        public abstract string Description { get; }

        public DispellBuffsFunction DispellBuffsFunction { get; private set; }

        public DispellDebuffsFunction DispellDebuffsFunction { get; private set; }

        public abstract string Displayname { get; }

        public GroupAuraManager GroupAuraManager { get; private set; }

        public abstract bool HandlesMovement { get; }

        public double HealingItemHealthThreshold { get; set; } = 30.0;

        public double HealingItemManaThreshold { get; set; } = 30.0;

        public abstract bool IsMelee { get; }

        public abstract IWowItemComparator ItemComparator { get; set; }

        public AuraManager MyAuraManager { get; private set; }

        public TimegatedEvent NearInterruptUnitsEvent { get; set; }

        public List<string> PriorityTargets { get => TargetManager.PriorityTargets; set => TargetManager.PriorityTargets = value; }

        public Dictionary<string, DateTime> RessurrectionTargets { get; private set; }

        public abstract CombatClassRole Role { get; }

        public Dictionary<string, Spell> Spells { get; protected set; }

        public abstract TalentTree Talents { get; }

        public AuraManager TargetAuraManager { get; private set; }

        public bool TargetInLineOfSight { get; set; }

        public InterruptManager TargetInterruptManager { get; private set; }

        public TargetManager TargetManager { get; private set; }

        public TimegatedEvent UpdatePriorityUnits { get; set; }

        public abstract bool UseAutoAttacks { get; }

        public bool UseDefaultTargetSelection { get; protected set; } = true;

        public abstract string Version { get; }

        public abstract bool WalkBehindEnemy { get; }

        protected WowInterface WowInterface { get; }

        private AmeisenBotStateMachine StateMachine { get; }

        public void Execute()
        {
            if (WowInterface.ObjectManager.Player.IsCasting) { return; }

            // Update Priority Units
            // --------------------------- >

            if (UpdatePriorityUnits.Run())
            {
                if (StateMachine.CurrentState.Key == BotState.Dungeon
                    && WowInterface.DungeonEngine != null
                    && WowInterface.DungeonEngine.Profile.PriorityUnits != null
                    && WowInterface.DungeonEngine.Profile.PriorityUnits.Count > 0)
                {
                    TargetManager.PriorityTargets = WowInterface.DungeonEngine.Profile.PriorityUnits.ToList();
                }
            }

            // Target selection
            // --------------------------- >

            if (UseDefaultTargetSelection)
            {
                if (TargetManager.GetUnitToTarget(out List<WowUnit> targetToTarget))
                {
                    ulong guid = targetToTarget.First().Guid;

                    if (WowInterface.ObjectManager.Player.TargetGuid != guid)
                    {
                        WowInterface.HookManager.TargetGuid(guid);
                        WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Player);
                    }
                }

                if (WowInterface.ObjectManager.Target == null
                    || WowInterface.ObjectManager.Target.IsDead
                    || !BotUtils.IsValidUnit(WowInterface.ObjectManager.Target))
                {
                    return;
                }
            }

            // Autoattacks
            // --------------------------- >

            if (UseAutoAttacks
                && !WowInterface.ObjectManager.Player.IsAutoAttacking
                && AutoAttackEvent.Run()
                && WowInterface.ObjectManager.Player.IsInMeleeRange(WowInterface.ObjectManager.Target))
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            // Interrupting
            // --------------------------- >

            if (NearInterruptUnitsEvent.Run())
            {
                TargetInterruptManager.UnitsToWatch = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, IsMelee ? 5.0 : 30.0).ToList();
            }

            // Buffs, Debuffs
            // --------------------------- >

            if (MyAuraManager.Tick()
                || GroupAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick())
            {
                return;
            }

            // Useable items, potions, etc.
            // ---------------------------- >

            int[] useableHealingItems = new int[]
            {
                // potions
                118, 929, 1710, 2938, 3928, 4596, 5509, 13446, 22829, 33447,
                // healthstones
                5509, 5510, 5511, 5512, 9421, 19013, 22103, 36889, 36892,
            };

            if (WowInterface.ObjectManager.Player.HealthPercentage < HealingItemHealthThreshold)
            {
                IWowItem healthstone = WowInterface.CharacterManager.Inventory.Items.FirstOrDefault(e => useableHealingItems.Contains(e.Id));

                if (healthstone != null)
                {
                    WowInterface.HookManager.UseItemByName(healthstone.Name);
                }
            }

            int[] useableManaItems = new int[]
            {
                // potions
                2245, 3385, 3827, 6149, 13443, 13444, 33448,
            };

            if (WowInterface.ObjectManager.Player.ManaPercentage < HealingItemManaThreshold)
            {
                IWowItem healthstone = WowInterface.CharacterManager.Inventory.Items.FirstOrDefault(e => useableHealingItems.Contains(e.Id));

                if (healthstone != null)
                {
                    WowInterface.HookManager.UseItemByName(healthstone.Name);
                }
            }

            // Race abilities
            // -------------- >

            if (WowInterface.ObjectManager.Player.Race == WowRace.Dwarf
                && WowInterface.ObjectManager.Player.HealthPercentage < 50
                && CastSpellIfPossible("Stoneform", 0))
            {
                return;
            }

            if (WowInterface.ObjectManager.TargetGuid != 0)
            {
                ExecuteCC();
            }
        }

        public abstract void ExecuteCC();

        public abstract void OutOfCombatExecute();

        public override string ToString()
        {
            return $"[{Class}] [{Role}] {Displayname} ({Author})";
        }

        protected bool CastSpellIfPossible(string spellName, ulong guid, bool needsResource = false, int currentResourceAmount = 0, bool forceTargetSwitch = false)
        {
            if (!DoIKnowSpell(spellName)) { return false; }

            if (GetValidTarget(guid, out WowUnit target))
            {
                if (currentResourceAmount == 0)
                {
                    currentResourceAmount = WowInterface.ObjectManager.Player.Class switch
                    {
                        WowClass.Deathknight => WowInterface.ObjectManager.Player.Runeenergy,
                        WowClass.Rogue => WowInterface.ObjectManager.Player.Energy,
                        WowClass.Warrior => WowInterface.ObjectManager.Player.Rage,
                        _ => WowInterface.ObjectManager.Player.Mana,
                    };
                }

                bool isTargetMyself = target != null && target.Guid == WowInterface.ObjectManager.PlayerGuid;

                if (!isTargetMyself && !TargetInLineOfSight)
                {
                    return false;
                }

                if (Spells[spellName] != null
                    && !CooldownManager.IsSpellOnCooldown(spellName)
                    && (!needsResource || Spells[spellName].Costs < currentResourceAmount)
                    && (target == null || IsInRange(Spells[spellName], target)))
                {
                    HandleTargetSelection(guid, forceTargetSwitch, isTargetMyself);

                    if (Spells[spellName].CastTime > 0)
                    {
                        // stop pending movement if we cast something
                        WowInterface.MovementEngine.Reset();
                        WowInterface.HookManager.StopClickToMoveIfActive();

                        CheckFacing(target);
                    }

                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        protected bool CastSpellIfPossibleDk(string spellName, ulong guid, bool needsRuneenergy = false, bool needsBloodrune = false, bool needsFrostrune = false, bool needsUnholyrune = false, bool forceTargetSwitch = false)
        {
            if (!DoIKnowSpell(spellName)) { return false; }

            if (GetValidTarget(guid, out WowUnit target))
            {
                bool isTargetMyself = target != null && target.Guid == WowInterface.ObjectManager.PlayerGuid;

                if (!isTargetMyself && !TargetInLineOfSight)
                {
                    return false;
                }

                Dictionary<RuneType, int> runes = WowInterface.HookManager.GetRunesReady();

                if (Spells[spellName] != null
                    && !CooldownManager.IsSpellOnCooldown(spellName)
                    && (!needsRuneenergy || Spells[spellName].Costs < WowInterface.ObjectManager.Player.Runeenergy)
                    && (!needsBloodrune || (runes[RuneType.Blood] > 0 || runes[RuneType.Death] > 0))
                    && (!needsFrostrune || (runes[RuneType.Frost] > 0 || runes[RuneType.Death] > 0))
                    && (!needsUnholyrune || (runes[RuneType.Unholy] > 0 || runes[RuneType.Death] > 0))
                    && (target == null || IsInRange(Spells[spellName], target)))
                {
                    HandleTargetSelection(guid, forceTargetSwitch, isTargetMyself);

                    if (Spells[spellName].CastTime > 0)
                    {
                        // stop pending movement if we cast something
                        WowInterface.MovementEngine.Reset();
                        WowInterface.HookManager.StopClickToMoveIfActive();

                        CheckFacing(target);
                    }

                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        protected bool CastSpellIfPossibleRogue(string spellName, ulong guid, bool needsEnergy = false, bool needsCombopoints = false, int requiredCombopoints = 1, bool forceTargetSwitch = false)
        {
            if (!DoIKnowSpell(spellName)) { return false; }

            if (GetValidTarget(guid, out WowUnit target))
            {
                bool isTargetMyself = target != null && target.Guid == WowInterface.ObjectManager.PlayerGuid;

                if (!isTargetMyself && !TargetInLineOfSight)
                {
                    return false;
                }

                if (Spells[spellName] != null
                    && !CooldownManager.IsSpellOnCooldown(spellName)
                    && (!needsEnergy || Spells[spellName].Costs < WowInterface.ObjectManager.Player.Energy)
                    && (!needsCombopoints || WowInterface.ObjectManager.Player.ComboPoints >= requiredCombopoints)
                    && (target == null || IsInRange(Spells[spellName], target)))
                {
                    HandleTargetSelection(guid, forceTargetSwitch, isTargetMyself);

                    if (Spells[spellName].CastTime > 0)
                    {
                        // stop pending movement if we cast something
                        WowInterface.MovementEngine.Reset();
                        WowInterface.HookManager.StopClickToMoveIfActive();

                        CheckFacing(target);
                    }

                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        protected bool CastSpellIfPossibleWarrior(string spellName, string requiredStance, ulong guid, bool needsResource = false, int currentResourceAmount = 0, bool forceTargetSwitch = false)
        {
            if (!DoIKnowSpell(spellName)) { return false; }

            if (GetValidTarget(guid, out WowUnit target))
            {
                if (currentResourceAmount == 0)
                {
                    currentResourceAmount = WowInterface.ObjectManager.Player.Class switch
                    {
                        WowClass.Deathknight => WowInterface.ObjectManager.Player.Runeenergy,
                        WowClass.Rogue => WowInterface.ObjectManager.Player.Energy,
                        WowClass.Warrior => WowInterface.ObjectManager.Player.Rage,
                        _ => WowInterface.ObjectManager.Player.Mana,
                    };
                }

                bool isTargetMyself = target != null && target.Guid == WowInterface.ObjectManager.PlayerGuid;

                if (!isTargetMyself && !TargetInLineOfSight)
                {
                    return false;
                }

                if (Spells[spellName] != null
                    && !CooldownManager.IsSpellOnCooldown(spellName)
                    && (!needsResource || Spells[spellName].Costs < currentResourceAmount)
                    && (target == null || IsInRange(Spells[spellName], target)))
                {
                    if (!WowInterface.ObjectManager.Player.HasBuffByName(requiredStance)
                        && Spells[requiredStance] != null
                        && !CooldownManager.IsSpellOnCooldown(requiredStance))
                    {
                        CastSpell(requiredStance, true);
                    }

                    HandleTargetSelection(guid, forceTargetSwitch, isTargetMyself);

                    if (Spells[spellName].CastTime > 0)
                    {
                        // stop pending movement if we cast something
                        WowInterface.MovementEngine.Reset();
                        WowInterface.HookManager.StopClickToMoveIfActive();

                        CheckFacing(target);
                    }

                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        protected bool CheckForWeaponEnchantment(EquipmentSlot slot, string enchantmentName, string spellToCastEnchantment)
        {
            if (WowInterface.CharacterManager.Equipment.Items.ContainsKey(slot))
            {
                int itemId = WowInterface.CharacterManager.Equipment.Items[slot].Id;

                if (itemId > 0)
                {
                    WowItem item = WowInterface.ObjectManager.WowObjects.OfType<WowItem>().FirstOrDefault(e => e.EntryId == itemId);

                    if (item != null
                        && !item.GetEnchantmentStrings().Any(e => e.Contains(enchantmentName))
                        && CastSpellIfPossible(spellToCastEnchantment, 0, true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool HandleDeadPartymembers(string SpellName)
        {
            if (!Spells.ContainsKey(SpellName))
            {
                Spells.Add(SpellName, WowInterface.CharacterManager.SpellBook.GetSpellByName(SpellName));
            }

            if (Spells[SpellName] != null
                && !CooldownManager.IsSpellOnCooldown(SpellName)
                && Spells[SpellName].Costs < WowInterface.ObjectManager.Player.Mana)
            {
                IEnumerable<WowPlayer> players = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>();
                List<WowPlayer> groupPlayers = players.Where(e => e.IsDead && e.Health == 0 && WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

                if (groupPlayers.Count > 0)
                {
                    WowPlayer player = groupPlayers.FirstOrDefault(e => !RessurrectionTargets.ContainsKey(e.Name) || RessurrectionTargets[e.Name] < DateTime.Now);

                    if (player != null)
                    {
                        if (!RessurrectionTargets.ContainsKey(player.Name))
                        {
                            RessurrectionTargets.Add(player.Name, DateTime.Now + TimeSpan.FromSeconds(8));
                            return false;
                        }

                        if (RessurrectionTargets[player.Name] < DateTime.Now)
                        {
                            return CastSpellIfPossible(SpellName, player.Guid, true);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private bool CastSpell(string spellName, bool castOnSelf)
        {
            // spits out stuff like this "1;300" (1 or 0 wether the cast was successful or not);(the cooldown in ms)
            if (WowInterface.HookManager.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:3}},{{v:4}}=GetSpellCooldown(\"{spellName}\"){{v:2}}=({{v:3}}+{{v:4}}-GetTime())*1000;if {{v:2}}<=0 then {{v:2}}=0;CastSpellByName(\"{spellName}\"{(castOnSelf ? ", \"player\"" : string.Empty)}){{v:5}},{{v:6}}=GetSpellCooldown(\"{spellName}\"){{v:1}}=({{v:5}}+{{v:6}}-GetTime())*1000;{{v:0}}=\"1;\"..{{v:1}} else {{v:0}}=\"0;\"..{{v:2}} end"), out string result))
            {
                if (result.Length < 3) return false;

                string[] parts = result.Split(";", StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 2) return false;

                // replace comma with dot in the cooldown
                if (parts[1].Contains(',')) parts[1] = parts[1].Replace(',', '.');

                if (int.TryParse(parts[0], out int castSuccessful)
                    && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double cooldown))
                {
                    cooldown = Math.Max(Math.Round(cooldown), 0);
                    CooldownManager.SetSpellCooldown(spellName, (int)cooldown);

                    if (castSuccessful == 1)
                    {
                        AmeisenLogger.Instance.Log("CombatClass", $"[{Displayname}]: Casting Spell \"{spellName}\" on \"{WowInterface.ObjectManager.Target?.Name}\"", LogLevel.Verbose);
                        return true;
                    }
                    else
                    {
                        AmeisenLogger.Instance.Log("CombatClass", $"[{Displayname}]: Spell \"{spellName}\" is on cooldown for \"{cooldown}\"", LogLevel.Verbose);
                        return false;
                    }
                }
            }

            return false;
        }

        private void CheckFacing(WowUnit target)
        {
            float facingAngle = BotMath.GetFacingAngle2D(WowInterface.ObjectManager.Player.Position, target.Position);
            float angleDiff = facingAngle - WowInterface.ObjectManager.Player.Rotation;
            float maxAngle = (float)(Math.PI * 2);

            if (angleDiff < 0)
            {
                angleDiff += maxAngle;
            }

            if (angleDiff > maxAngle)
            {
                angleDiff -= maxAngle;
            }

            if (angleDiff > 1.5)
            {
                WowInterface.HookManager.FacePosition(WowInterface.ObjectManager.Player, target.Position);
            }
        }

        private bool DoIKnowSpell(string spellName)
        {
            if (!Spells.ContainsKey(spellName))
            {
                Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                if (spell != null)
                {
                    Spells.Add(spellName, spell);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private bool GetValidTarget(ulong guid, out WowUnit target)
        {
            target = guid == 0 ? WowInterface.ObjectManager.Player : WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(guid);
            return target != null;
        }

        private void HandleTargetSelection(ulong guid, bool forceTargetSwitch, bool isTargetMyself)
        {
            if (guid != 0 && WowInterface.ObjectManager.TargetGuid != guid)
            {
                // we dont need to switch target when casting spell on self
                if (forceTargetSwitch || !isTargetMyself)
                {
                    WowInterface.HookManager.TargetGuid(guid);
                }
            }
        }

        private bool IsInRange(Spell spell, WowUnit wowUnit)
        {
            if ((spell.MinRange == 0 && spell.MaxRange == 0) || spell.MaxRange == 0)
            {
                return WowInterface.ObjectManager.Player.IsInMeleeRange(wowUnit);
            }

            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(wowUnit.Position);
            return distance >= spell.MinRange && distance <= spell.MaxRange;
        }
    }
}