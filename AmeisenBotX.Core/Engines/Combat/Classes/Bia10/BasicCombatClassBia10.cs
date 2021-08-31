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
using AmeisenBotX.Wow335a.Constants;
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
            ResurrectionTargets = new Dictionary<string, DateTime>();

            CooldownManager = new CooldownManager(Bot.Character.SpellBook.Spells);
            InterruptManager = new InterruptManager();
            MyAuraManager = new AuraManager(Bot);
            TargetAuraManager = new AuraManager(Bot);
            GroupAuraManager = new GroupAuraManager(Bot);

            EventCheckFacing = new TimegatedEvent(TimeSpan.FromMilliseconds(500));

            Configurables = new Dictionary<string, dynamic>
            {
                { "HealthItemThreshold", 30.0 },
                { "ManaItemThreshold", 30.0 }
            };

            //Load spells, would be nice to have proper SpellManager for extra spell info :/
            KnownSpells = new Dictionary<string, WoWSpell>();

            var healingWaveData = new WoWSpell(40, 0, TimeSpan.FromSeconds(1.5), WoWSpell.WoWSpellSchool.Nature);
            KnownSpells.Add(Shaman335a.HealingWave, healingWaveData);
            var lightingBoltData = new WoWSpell(30, 0, TimeSpan.FromSeconds(1.5), WoWSpell.WoWSpellSchool.Nature);
            KnownSpells.Add(Shaman335a.LightningBolt, lightingBoltData);
            var earthShockData = new WoWSpell(25, 0, TimeSpan.FromSeconds(6), WoWSpell.WoWSpellSchool.Nature);
            KnownSpells.Add(Shaman335a.EarthShock, earthShockData);
            var rockBitterData = new WoWSpell(0, 0, TimeSpan.FromSeconds(0), WoWSpell.WoWSpellSchool.Nature);
            KnownSpells.Add(Shaman335a.RockbiterWeapon, rockBitterData);
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

        public string Author => "Bia10";
        public abstract string Description { get; }
        public abstract string DisplayName { get; }
        public IEnumerable<int> BlacklistedTargetDisplayIds { get; set; }
        public IEnumerable<int> PriorityTargetDisplayIds { get; set; }
        public Dictionary<string, dynamic> Configurables { get; set; }
        public CooldownManager CooldownManager { get; private set; }
        public TimegatedEvent EventCheckFacing { get; set; }
        public GroupAuraManager GroupAuraManager { get; private set; }
        public bool HandlesFacing => true;
        public abstract bool HandlesMovement { get; }
        public InterruptManager InterruptManager { get; private set; }
        public abstract bool IsMelee { get; }
        public abstract IItemComparator ItemComparator { get; set; }
        public AuraManager MyAuraManager { get; private set; }
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
        private DateTime LastGCD { get; set; }
        private double GCDTime { get; set; }

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

            /* TODO: either IsInMeleeRange or IsAutoAttacking is bugged investigate
            if (Bot.Player.IsInMeleeRange(Bot.Target) && !Bot.Player.IsAutoAttacking)
            {
                if (Bot.Player.IsCasting)
                {
                    Bot.Wow.StopCasting();
                    Bot.Wow.StartAutoAttack();
                    return;
                }
                Bot.Wow.StartAutoAttack();
            }*/

            if (!IsInSpellRange(target, Shaman335a.LightningBolt)
                || !Bot.Wow.IsInLineOfSight(Bot.Player.Position, target.Position))
                Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);
        }

        public virtual void Execute()
        {
            if (Bot.Player.IsCasting && (!Bot.Objects.IsTargetInLineOfSight
                                         || SpellAbortFunctions.Any(e => e())))
            {
                Bot.Wow.StopCasting();
                return;
            }

            if (Bot.Target != null && EventCheckFacing.Run())
                CheckFacing(Bot.Target);

            AttackTarget();

            // Buffs, Debuffs, Interrupts
            // --------------------------- >
            if (MyAuraManager.Tick(Bot.Player.Auras) || GroupAuraManager.Tick())
                return;
            if (Bot.Target != null && TargetAuraManager.Tick(Bot.Target.Auras))
                return;
            if (InterruptManager.Tick(Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, IsMelee ? 5.0f : 30.0f)))
                return;

            // Race abilities
            // -------------- >
            switch (Bot.Player.Race)
            {
                case WowRace.Human:
                    if (Bot.Player.IsDazed || Bot.Player.IsFleeing || Bot.Player.IsInfluenced || Bot.Player.IsPossessed)
                        if (ValidateSpell(Racials335a.EveryManForHimself))
                            TryCastSpell(Racials335a.EveryManForHimself, Bot.Player.Guid);
                    break;
                case WowRace.Orc:
                    break;
                case WowRace.Dwarf:
                    if (Bot.Player.HealthPercentage < 50.0)
                        if (ValidateSpell(Racials335a.Stoneform))
                            TryCastSpell(Racials335a.Stoneform, Bot.Player.Guid);
                    break;
                case WowRace.Nightelf:
                    break;
                case WowRace.Undead:
                    break;
                case WowRace.Tauren:
                    if (Bot.Player.HealthPercentage < 50.0
                        && Bot.GetEnemiesInCombatWithMe<IWowUnit>(Bot.Player.Position, 10).Count() > 2)
                        if (ValidateSpell(Racials335a.WarStomp))
                            TryCastSpell(Racials335a.WarStomp, Bot.Player.Guid);
                    break;
                case WowRace.Gnome:
                    break;
                case WowRace.Troll:
                    if (Bot.Player.ManaPercentage > 45.0
                        && Bot.GetEnemiesInCombatWithMe<IWowUnit>(Bot.Player.Position, 10).Count() > 2)
                        if (ValidateSpell(Racials335a.Berserking))
                            TryCastSpell(Racials335a.Berserking, Bot.Player.Guid);
                    break;
                case WowRace.Bloodelf:
                    break;
                case WowRace.Draenei:
                    if (Bot.Player.HealthPercentage < 50.0)
                        if (ValidateSpell(Racials335a.GiftOfTheNaaru))
                            TryCastSpell(Racials335a.GiftOfTheNaaru, Bot.Player.Guid);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
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
                    Bot.Wow.StopCasting();
                return;
            }

            if (MyAuraManager.Tick(Bot.Player.Auras) || GroupAuraManager.Tick())
                return;
        }

        public virtual Dictionary<string, object> Save() =>
            new() { { "Configureables", Configurables } };

        public override string ToString() =>
            $"[{WowClass}] [{Role}] {DisplayName} ({Author})";

        protected bool CheckForWeaponEnchantment(WowEquipmentSlot slot, string enchantmentName, string spellToCastEnchantment)
        {
            if (!Bot.Character.Equipment.Items.ContainsKey(slot))
                return false;

            var itemId = Bot.Character.Equipment.Items[slot].Id;
            if (itemId <= 0) return false;

            var item = Bot.Objects.WowObjects.OfType<IWowItem>().FirstOrDefault(e => e.EntryId == itemId);
            if (item == null) return false;

            var enchantNameClean = enchantmentName.Split(" ", 2)[0];
            return !item.GetEnchantmentStrings().Any(e => e.Contains(enchantNameClean, StringComparison.OrdinalIgnoreCase))
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

        protected string SelectSpell(out ulong targetGuid)
        {
            if (Bot.Player.HealthPercentage < DataConstants.HealSelfPercentage
                && ValidateSpell(Shaman335a.HealingWave))
            {
                targetGuid = Bot.Player.Guid;
                return Shaman335a.HealingWave;
            }
            if (IsInSpellRange(Bot.Target, Shaman335a.EarthShock)
                && ValidateSpell(Shaman335a.EarthShock))
            {
                targetGuid = Bot.Target.Guid;
                return Shaman335a.EarthShock;
            }
            if (IsInSpellRange(Bot.Target, Shaman335a.LightningBolt)
                && ValidateSpell(Shaman335a.LightningBolt))
            {
                targetGuid = Bot.Target.Guid;
                return Shaman335a.LightningBolt;
            }

            targetGuid = 9999999;
            return string.Empty;
        }

        private bool ValidateSpell(string spellName)
        {
            if (!Bot.Character.SpellBook.IsSpellKnown(spellName) || !Bot.Objects.IsTargetInLineOfSight)
                return false;
            if (CooldownManager.IsSpellOnCooldown(spellName) || IsGCD())
                return false;

            return !Bot.Player.IsCasting;
        }

        private void SetGCD(double gcdInSec)
        {
            GCDTime = gcdInSec;
            LastGCD = DateTime.Now;
        }

        private bool IsGCD() => DateTime.Now.Subtract(LastGCD).TotalSeconds < GCDTime;

        protected bool TryCastSpell(string spellName, ulong guid, bool needsResource = false)
        {
            var spell = Bot.Character.SpellBook.GetSpellByName(spellName);

            if (spell == null) return false;
            if (needsResource && spell.Costs > Bot.Player.Mana) return false;
            if (!ValidateTarget(guid, out var target, out var needToSwitchTarget)) return false;
            if (target != null && !IsInSpellRange(target, spellName)) return false;

            var isTargetMyself = guid == Bot.Player.Guid;
            if (!isTargetMyself && needToSwitchTarget)
                Bot.Wow.ChangeTarget(guid);

            if (!isTargetMyself && target != null && !BotMath.IsFacing(Bot.Player.Position, Bot.Player.Rotation, target.Position))
                Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);

            if (spell.CastTime > 0)
            {
                // stop pending movement if we cast something
                Bot.Movement.PreventMovement(TimeSpan.FromMilliseconds(spell.CastTime));
                CheckFacing(target);
            }

            if (!CastSpell(spellName, isTargetMyself)) return false;

            SetGCD(1.5);
            return true;
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
                    angleDiff += BotMath.MAX_ANGLE;
                    break;
                case > BotMath.MAX_ANGLE:
                    angleDiff -= BotMath.MAX_ANGLE;
                    break;
            }

            if (angleDiff > 1.0)
                Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);
        }

        private bool ValidateTarget(ulong guid, out IWowUnit target, out bool needToSwitchTargets)
        {
            if (guid == Bot.Player.Guid)
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