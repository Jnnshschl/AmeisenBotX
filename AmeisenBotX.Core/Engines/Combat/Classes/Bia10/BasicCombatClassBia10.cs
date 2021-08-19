using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Engines.Combat.Helpers;
using AmeisenBotX.Core.Engines.Combat.Helpers.Aura;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets;
using AmeisenBotX.Core.Engines.Movement.Enums;
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
        protected BasicCombatClassBia10(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            SpellAbortFunctions = new List<Func<bool>>();

            CooldownManager = new CooldownManager(Bot.Character.SpellBook.Spells);
            ResurrectionTargets = new Dictionary<string, DateTime>();

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
            KnownSpells.Add(DataConstants.ShamanSpells.LightningBolt, lightingBoltData);
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
        public Dictionary<string, DateTime> ResurrectionTargets { get; private set; }
        public abstract WowRole Role { get; }
        public abstract TalentTree Talents { get; }
        public AuraManager TargetAuraManager { get; private set; }
        public ITargetProvider TargetProviderDps { get; private set; }
        public abstract bool UseAutoAttacks { get; }
        public abstract string Version { get; }
        public abstract bool WalkBehindEnemy { get; }
        public abstract WowClass WowClass { get; }
        protected AmeisenBotInterfaces Bot { get; }
        protected DateTime LastSpellCast { get; private set; }
        protected List<Func<bool>> SpellAbortFunctions { get; }

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

            if (IsMelee)
                Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);
            else if (IsInSpellRange(target, DataConstants.ShamanSpells.LightningBolt))
                TryCastSpell(DataConstants.ShamanSpells.LightningBolt, target.Guid);
            else if (!IsInSpellRange(target, DataConstants.ShamanSpells.LightningBolt) // try closer location
                     || !Bot.Wow.IsInLineOfSight(Bot.Player.Position, target.Position))
            {
                var distanceToTarget = Bot.Player.DistanceTo(target.Position);
                var heightDifference = target.Position.Z - Bot.Player.Position.Z;

                // a mere workaround, some calculation of slope would be nice
                if (heightDifference <= 10 && distanceToTarget >= 30)
                    Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);

                //Bot.Wow.ClearTarget();
                //Todo: add to blacklist for a while
            }
        }

        public virtual void Execute() //TODO: refactor
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
            if (Bot.Dungeon != null
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
                    DataConstants.usableHealingItems.Contains(e.Id));

                if (healthItem != null)
                {
                    Bot.Wow.UseItemByName(healthItem.Name);
                }
            }

            if (Bot.Player.ManaPercentage < C["ManaItemThreshold"])
            {
                var manaItem = Bot.Character.Inventory.Items.FirstOrDefault(e =>
                    DataConstants.usableManaItems.Contains(e.Id));

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

        protected bool HandleDeadPartyMembers(string spellName)
        {
            var spell = Bot.Character.SpellBook.GetSpellByName(spellName);
            if (spell == null || CooldownManager.IsSpellOnCooldown(spellName)
                              || spell.Costs >= Bot.Player.Mana)
                return false;

            var groupPlayers = Bot.Objects.Partymembers
                .OfType<IWowPlayer>()
                .Where(e => e.Health == 0)
                .ToList();

            if (!groupPlayers.Any()) return false;

            var player = groupPlayers.FirstOrDefault(e => Bot.Db.GetUnitName(e, out var name)
                && !ResurrectionTargets.ContainsKey(name) || ResurrectionTargets[name] < DateTime.Now);

            if (player == null) return false;
            if (!Bot.Db.GetUnitName(player, out var name)) return false;

            if (ResurrectionTargets.ContainsKey(name))
                return ResurrectionTargets[name] >= DateTime.Now || TryCastSpell(spellName, player.Guid, true);

            ResurrectionTargets.Add(name, DateTime.Now + TimeSpan.FromSeconds(10));
            return TryCastSpell(spellName, player.Guid, true);

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

        private bool CastSpell(string spellName, bool castOnSelf)
        {
            // spits out stuff like this "1;300" (1 or 0 whether the cast was successful or not);(the cooldown in ms)
            if (!Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua(
                    DataConstants.GetCastSpellString(spellName, castOnSelf)), out var result))
                return false;

            if (result.Length < 3) return false;

            var parts = result.Split(";", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return false;

            // replace comma with dot in the cooldown
            if (parts[1].Contains(',', StringComparison.OrdinalIgnoreCase))
                parts[1] = parts[1].Replace(',', '.');

            if (!int.TryParse(parts[0], out var castSuccessful)
                || !double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var cooldown))
                return false;

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

        private void CheckFacing(IWowObject target)
        {
            if (target == null || target.Guid == Bot.Wow.PlayerGuid)
                return;

            var facingAngle = BotMath.GetFacingAngle(Bot.Player.Position, target.Position);
            var angleDiff = facingAngle - Bot.Player.Rotation;

            switch (angleDiff)
            {
                case < 0:
                    angleDiff += DataConstants.MAX_ANGLE;
                    break;
                case > DataConstants.MAX_ANGLE:
                    angleDiff -= DataConstants.MAX_ANGLE;
                    break;
            }

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