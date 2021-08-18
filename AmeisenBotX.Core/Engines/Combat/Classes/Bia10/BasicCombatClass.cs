using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Engines.Combat.Helpers;
using AmeisenBotX.Core.Engines.Combat.Helpers.Aura;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Bia10
{
    public abstract class BasicCombatClassBia10 : ICombatClass
    {
        #region Shaman
        protected const string ancestralSpiritSpell = "Ancestral Spirit";
        protected const string chainHealSpell = "Chain Heal";
        protected const string chainLightningSpell = "Chain Lightning";
        protected const string earthlivingBuff = "Earthliving ";
        protected const string earthlivingWeaponSpell = "Earthliving Weapon";
        protected const string earthShieldSpell = "Earth Shield";
        protected const string earthShockSpell = "Earth Shock";
        protected const string elementalMasterySpell = "Elemental Mastery";
        protected const string feralSpiritSpell = "Feral Spirit";
        protected const string flameShockSpell = "Flame Shock";
        protected const string flametongueBuff = "Flametongue ";
        protected const string flametongueWeaponSpell = "Flametongue Weapon";
        protected const string flametoungueBuff = "Flametongue ";
        protected const string flametoungueWeaponSpell = "Flametongue Weapon";
        protected const string healingWaveSpell = "Healing Wave";
        protected const string heroismSpell = "Heroism";
        protected const string hexSpell = "Hex";
        protected const string lavaBurstSpell = "Lava Burst";
        protected const string lavaLashSpell = "Lava Lash";
        protected const string lesserHealingWaveSpell = "Lesser Healing Wave";
        protected const string lightningBoltSpell = "Lightning Bolt";
        protected const string lightningShieldSpell = "Lightning Shield";
        protected const string maelstromWeaponSpell = "Mealstrom Weapon";
        protected const string riptideSpell = "Riptide";
        protected const string shamanisticRageSpell = "Shamanistic Rage";
        protected const string stormstrikeSpell = "Stormstrike";
        protected const string thunderstormSpell = "Thunderstorm";
        protected const string tidalForceSpell = "Tidal Force";
        protected const string waterShieldSpell = "Water Shield";
        protected const string windfuryBuff = "Windfury";
        protected const string windfuryWeaponSpell = "Windfury Weapon";
        protected const string windShearSpell = "Wind Shear";
        #endregion Shaman

        #region Racials
        protected const string berserkingSpell = "Berserking"; // Troll
        protected const string bloodFurySpell = "Blood Fury";  // Orc
        #endregion Racials

        private const float MAX_ANGLE = MathF.PI * 2.0f;

        private readonly int[] useableHealingItems = {
            // potions
            118, 929, 1710, 2938, 3928, 4596, 5509, 13446, 22829, 33447,
            // healthstones
            5509, 5510, 5511, 5512, 9421, 19013, 22103, 36889, 36892,
        };

        private readonly int[] useableManaItems = {
            // potions
            2245, 3385, 3827, 6149, 13443, 13444, 33448, 22832,
        };

        protected BasicCombatClassBia10(AmeisenBotInterfaces bot, AmeisenBotFsm stateMachine)
        {
            Bot = bot;
            StateMachine = stateMachine;

            SpellAbortFunctions = new List<Func<bool>>();

            CooldownManager = new CooldownManager(Bot.Character.SpellBook.Spells);
            RessurrectionTargets = new Dictionary<string, DateTime>();

            //TargetProviderDps = new TargetManager(new DpsTargetSelectionLogic(Bot), TimeSpan.FromMilliseconds(250));
            //TargetProviderTank = new TargetManager(new TankTargetSelectionLogic(Bot), TimeSpan.FromMilliseconds(250));
            //TargetProviderHeal = new TargetManager(new HealTargetSelectionLogic(Bot), TimeSpan.FromMilliseconds(250));

            MyAuraManager = new AuraManager(Bot);
            TargetAuraManager = new AuraManager(Bot);
            GroupAuraManager = new GroupAuraManager(Bot);

            InterruptManager = new InterruptManager();

            EventCheckFacing = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
            EventAutoAttack = new TimegatedEvent(TimeSpan.FromMilliseconds(500));

            C = new Dictionary<string, dynamic>
            {
                { "HealthItemThreshold", 30.0 },
                { "ManaItemThreshold", 30.0 }
            };

            //Load spells, would be nice to have proper SpellManager for extra spell info :/
            KnownSpells = new Dictionary<string, WoWSpell>();
            var lightingBoltData = new WoWSpell(30, 0, TimeSpan.FromSeconds(1.5), WoWSpell.WoWSpellSchool.Nature);
            KnownSpells.Add(lightningBoltSpell, lightingBoltData);
        }

        public Dictionary<string, WoWSpell> KnownSpells { get; set; }

        public class WoWSpell
        {
            [Flags]
            public enum WoWSpellSchool
            {
                None = 0,
                Physical = 1,
                Holy = 2,
                Fire = 4,
                Nature = 8,
                Frost = 16,
                Shadow = 32,
                Arcane = 64,
            }

            public Guid Id;
            public int MaxRange;
            public int MinRange;
            public TimeSpan Cooldown;
            public TimeSpan CastTime;
            public WoWSpellSchool SchoolType; 

            public WoWSpell(int maxRange, int minRange, TimeSpan castTime, WoWSpellSchool schoolType)
            {
                Id = new Guid();
                MaxRange = maxRange;
                MinRange = minRange;
                CastTime = castTime;
                SchoolType = schoolType;
            }
        }

        public string Author { get; } = "Bia10";
        public IEnumerable<int> BlacklistedTargetDisplayIds { get => TargetProviderDps.BlacklistedTargets; set => TargetProviderDps.BlacklistedTargets = value; }
        public Dictionary<string, dynamic> C { get; set; }
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
        private AmeisenBotFsm StateMachine { get; }

        protected bool IsInSpellRange(IWowUnit unit, string spellName)
        {
            if (string.IsNullOrEmpty(spellName)) return false;
            if (unit == null) return false;

            var spell = KnownSpells[spellName]; // would be nice: SpellManager.KnownSpells[spellName]
            if (spell.MinRange == 0 && spell.MaxRange == 0 || spell.MaxRange == 0)
                return Bot.Player.IsInMeleeRange(unit);

            double distance = Bot.Player.Position.GetDistance(unit.Position);
            return distance >= spell.MinRange && distance <= spell.MaxRange - 1.0;
        }

        public virtual void AttackTarget()
        {
            var target = Bot.Target;
            if (target == null) return;

            if (Bot.Player.Position.GetDistance(target.Position) <= 3.0f) // this will likely buggout with facing check, endless loop of turning on mob position
            {
                Bot.Wow.StopClickToMove();
                Bot.Movement.Reset();
                Bot.Wow.InteractWithUnit(target.BaseAddress);
            }
            else if (IsMelee)
                Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);
            else if (IsInSpellRange(target, lightningBoltSpell))
                TryCastSpell(lightningBoltSpell, target.Guid);
            else if (!IsInSpellRange(target, lightningBoltSpell) // try closer location
                     || !Bot.Wow.IsInLineOfSight(Bot.Player.Position, target.Position))
                Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);
        }

        public virtual void Execute()
        {
            if (Bot.Player.IsCasting)
            {
                if (!Bot.Objects.IsTargetInLineOfSight || SpellAbortFunctions.Any(e => e()))
                    Bot.Wow.StopCasting();

                return;
            }

            if (Bot.Target != null && EventCheckFacing.Run())
                CheckFacing(Bot.Target);

            // Update Priority Units
            // --------------------------- >
            if (StateMachine.CurrentState.Key == BotState.Dungeon
                && Bot.Dungeon != null
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
                return;

            if (Bot.Target != null
                && TargetAuraManager.Tick(Bot.Target.Auras))
                return;

            if (InterruptManager.Tick(Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, IsMelee ? 5.0f : 30.0f)))
                return;

            // Useable items, potions, etc.
            // ---------------------------- >
            if (Bot.Player.HealthPercentage < C["HealthItemThreshold"])
            {
                var healthItem = Bot.Character.Inventory.Items.FirstOrDefault(e =>
                    useableHealingItems.Contains(e.Id));

                if (healthItem != null)
                {
                    Bot.Wow.UseItemByName(healthItem.Name);
                }
            }

            if (Bot.Player.ManaPercentage < C["ManaItemThreshold"])
            {
                var manaItem = Bot.Character.Inventory.Items.FirstOrDefault(e =>
                    useableManaItems.Contains(e.Id));

                if (manaItem != null)
                    Bot.Wow.UseItemByName(manaItem.Name);
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
                && (Bot.Player.Race == WowRace.Draenei && TryCastSpell("Gift of the Naaru", 0)
                    || Bot.Player.Race == WowRace.Dwarf && TryCastSpell("Stoneform", 0)))
            {
                return;
            }
        }

        public virtual void Load(Dictionary<string, JsonElement> objects)
        {
            if (objects.ContainsKey("Configureables")) { C = objects["Configureables"].ToDyn(); }
        }

        public virtual void OutOfCombatExecute()
        {
            if (Bot.Player.IsCasting)
            {
                if (!Bot.Objects.IsTargetInLineOfSight || SpellAbortFunctions.Any(e => e()))
                    Bot.Wow.StopCasting();

                return;
            }

            if (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Food") && Bot.Player.HealthPercentage < 100.0
                || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Drink") && Bot.Player.ManaPercentage < 100.0)
                return;

            if (MyAuraManager.Tick(Bot.Player.Auras) || GroupAuraManager.Tick())
                return;
        }

        public virtual Dictionary<string, object> Save() =>
            new() { { "Configureables", C } };

        public override string ToString() => 
            $"[{WowClass}] [{Role}] {DisplayName} ({Author})";

        protected bool CheckForWeaponEnchantment(WowEquipmentSlot slot, string enchantmentName, string spellToCastEnchantment)
        {
            if (!Bot.Character.Equipment.Items.ContainsKey(slot)) return false;
            var itemId = Bot.Character.Equipment.Items[slot].Id;
            if (itemId <= 0) return false;

            var item = Bot.Objects.WowObjects.OfType<IWowItem>().FirstOrDefault(e => e.EntryId == itemId);
            return item != null && !item.GetEnchantmentStrings().Any(e => e.Contains(enchantmentName, StringComparison.OrdinalIgnoreCase))
                                && TryCastSpell(spellToCastEnchantment, 0, true);
        }

        protected bool HandleDeadPartymembers(string spellName) //TODO: mess refactor
        {
            var spell = Bot.Character.SpellBook.GetSpellByName(spellName);

            if (spell != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && spell.Costs < Bot.Player.Mana)
            {
                var groupPlayers = Bot.Objects.Partymembers
                    .OfType<IWowPlayer>()
                    .Where(e => e.IsDead);

                if (groupPlayers.Any())
                {
                    var player = groupPlayers.FirstOrDefault(e => Bot.Db.GetUnitName(e, out var name) && !RessurrectionTargets.ContainsKey(name) || RessurrectionTargets[name] < DateTime.Now);

                    if (player != null)
                    {
                        if (Bot.Db.GetUnitName(player, out var name))
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

        protected bool TryCastSpell(string spellName, ulong guid, bool needsResource = false, int currentResourceAmount = 0, bool forceTargetSwitch = false)
        {
            if (!Bot.Character.SpellBook.IsSpellKnown(spellName) // spell not found
                || guid != 0 && guid == Bot.Wow.PlayerGuid       // target is us
                || !Bot.Objects.IsTargetInLineOfSight)           // not in los
                return false;

            if (!ValidateTarget(guid, out var target, out var needToSwitchTarget)) // target invalid
                return false;

            if (currentResourceAmount == 0)
                currentResourceAmount = Bot.Player.Mana;

            var isTargetMyself = guid is 0 or 2;
            var spell = Bot.Character.SpellBook.GetSpellByName(spellName);

            if (spell == null || CooldownManager.IsSpellOnCooldown(spellName)
                              || needsResource && spell.Costs >= currentResourceAmount 
                              || target != null && !IsInSpellRange(target, spell.Name)) 
                return false;

            if (!isTargetMyself && (needToSwitchTarget || forceTargetSwitch))
                Bot.Wow.ChangeTarget(guid);

            if (!isTargetMyself && target != null
                                && !BotMath.IsFacing(Bot.Player.Position, Bot.Player.Rotation, target.Position))
                Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);

            if (spell.CastTime > 0)
            {
                // stop pending movement if we cast something
                Bot.Movement.PreventMovement(TimeSpan.FromMilliseconds(spell.CastTime));
                CheckFacing(target);
            }

            LastSpellCast = DateTime.UtcNow;
            return CastSpell(spellName, isTargetMyself);
        }

        private bool CastSpell(string spellName, bool castOnSelf) //TODO: mess refactor
        {
            // spits out stuff like this "1;300" (1 or 0 whether the cast was successful or not);(the cooldown in ms)
            if (Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:3}},{{v:4}}=GetSpellCooldown(\"{spellName}\"){{v:2}}=({{v:3}}+{{v:4}}-GetTime())*1000;if {{v:2}}<=0 then {{v:2}}=0;CastSpellByName(\"{spellName}\"{(castOnSelf ? ", \"player\"" : string.Empty)}){{v:5}},{{v:6}}=GetSpellCooldown(\"{spellName}\"){{v:1}}=({{v:5}}+{{v:6}}-GetTime())*1000;{{v:0}}=\"1;\"..{{v:1}} else {{v:0}}=\"0;\"..{{v:2}} end"), out var result))
            {
                if (result.Length < 3)
                    return false;

                var parts = result.Split(";", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                    return false;
                
                // replace comma with dot in the cooldown
                if (parts[1].Contains(',', StringComparison.OrdinalIgnoreCase))
                    parts[1] = parts[1].Replace(',', '.');

                if (int.TryParse(parts[0], out var castSuccessful)
                    && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var cooldown))
                {
                    cooldown = Math.Max(cooldown, 0);
                    CooldownManager.SetSpellCooldown(spellName, (int)cooldown);

                    if (castSuccessful == 1)
                    {
                        AmeisenLogger.I.Log("CombatClass", $"[{DisplayName}]: Casting Spell \"{spellName}\" on \"{Bot.Target?.Guid}\"", LogLevel.Verbose);
                        IsWanding = IsWanding && spellName == "Shoot";
                        return true;
                    }

                    AmeisenLogger.I.Log("CombatClass", $"[{DisplayName}]: Spell \"{spellName}\" is on cooldown for \"{cooldown}\"ms", LogLevel.Verbose);
                    return false;
                }
            }

            return false;
        }

        private void CheckFacing(IWowObject target)
        {
            if (target == null || target.Guid == Bot.Wow.PlayerGuid)
                return;

            var facingAngle = BotMath.GetFacingAngle(Bot.Player.Position, target.Position);
            var angleDiff = facingAngle - Bot.Player.Rotation;

            if (angleDiff < 0)
                angleDiff += MAX_ANGLE;
            else if (angleDiff > MAX_ANGLE)
                angleDiff -= MAX_ANGLE;

            if (angleDiff > 1.0)
                Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);
        }

        private bool ValidateTarget(ulong guid, out IWowUnit target, out bool needToSwitchTargets)
        {
            if (guid == 0)
            {
                target = Bot.Player;
                needToSwitchTargets = false;
                return true;
            }
            if (guid == Bot.Wow.TargetGuid)
            {
                target = Bot.Target;
                needToSwitchTargets = false;
                return true;
            }

            target = Bot.GetWowObjectByGuid<IWowUnit>(guid);
            needToSwitchTargets = true;
            return target != null;
        }
    }
}