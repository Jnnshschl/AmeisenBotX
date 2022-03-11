using AmeisenBotX.Core.Engines.Combat.Helpers;
using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Bia10
{
    public class ShamanElemental : BasicCombatClassBia10
    {
        public ShamanElemental(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Shaman335a.LightningShield, () =>
                Bot.Player.ManaPercentage > 60.0
                && ValidateSpell(Shaman335a.LightningShield, true)
                && TryCastSpell(Shaman335a.LightningShield, Bot.Player.Guid)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Shaman335a.WaterShield, () =>
                Bot.Player.ManaPercentage < 20.0
                && ValidateSpell(Shaman335a.WaterShield, true)
                && TryCastSpell(Shaman335a.WaterShield, Bot.Player.Guid)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Shaman335a.FlameShock, () =>
                Bot.Target?.HealthPercentage >= 5
                && ValidateSpell(Shaman335a.FlameShock, true)
                && TryCastSpell(Shaman335a.FlameShock, Bot.Wow.TargetGuid)));

            InterruptManager.InterruptSpells = new SortedList<int, InterruptManager.CastInterruptFunction>
            {
                { 0, x => TryCastSpell(Shaman335a.WindShear, x.Guid) },
                { 1, x => TryCastSpell(Shaman335a.Hex, x.Guid) }
            };
        }

        public override string Description => "CombatClass for the Elemental Shaman spec.";

        public override string DisplayName => "Shaman Elemental";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } =
            new ShamanElementalComparator(null, new List<WowWeaponType>
            {
                WowWeaponType.AxeTwoHand,
                WowWeaponType.MaceTwoHand,
                WowWeaponType.SwordTwoHand
            });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new Dictionary<int, Talent>(),
            Tree2 = new Dictionary<int, Talent>(),
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Shaman;

        public override void Execute()
        {
            base.Execute();

            string spellName = SelectSpell(out ulong targetGuid);
            TryCastSpell(spellName, targetGuid);
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (HandleDeadPartyMembers(Shaman335a.AncestralSpirit))
            {
                return;
            }

            string enchSpellName = DecideWeaponEnchantment(out string enchantName);
            CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_MAINHAND, enchantName, enchSpellName);
        }

        private string DecideWeaponEnchantment(out string enchantName)
        {
            if (Bot.Character.SpellBook.IsSpellKnown(Shaman335a.FlametongueWeapon))
            {
                enchantName = Shaman335a.FlametongueBuff;
                return Shaman335a.FlametongueWeapon;
            }
            if (Bot.Character.SpellBook.IsSpellKnown(Shaman335a.RockbiterWeapon))
            {
                enchantName = Shaman335a.RockbiterBuff;
                return Shaman335a.RockbiterWeapon;
            }

            enchantName = string.Empty;
            return string.Empty;
        }

        private string SelectSpell(out ulong targetGuid)
        {
            if (Bot.Player.HealthPercentage < DataConstants.HealSelfPercentage
                && ValidateSpell(Shaman335a.HealingWave, true))
            {
                targetGuid = Bot.Player.Guid;
                return Shaman335a.HealingWave;
            }
            if (Bot.Target.HealthPercentage >= 3
                && IsInSpellRange(Bot.Target, Shaman335a.EarthShock)
                && ValidateSpell(Shaman335a.EarthShock, true))
            {
                targetGuid = Bot.Target.Guid;
                return Shaman335a.EarthShock;
            }
            if (IsInSpellRange(Bot.Target, Shaman335a.LightningBolt)
                && ValidateSpell(Shaman335a.LightningBolt, true))
            {
                targetGuid = Bot.Target.Guid;
                return Shaman335a.LightningBolt;
            }

            targetGuid = 9999999;
            return string.Empty;
        }
    }
}