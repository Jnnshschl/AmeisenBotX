using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Bia10
{
    public class MageFrost : BasicCombatClassBia10
    {
        public MageFrost(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.FrostArmor, () =>
                Bot.Player.ManaPercentage > 60.0
                && ValidateSpell(Mage335a.FrostArmor, true)
                && TryCastSpell(Mage335a.FrostArmor, Bot.Player.Guid)));
        }

        public override string Version => "1.0";
        public override string Description => "CombatClass for the Frost Mage spec.";
        public override string DisplayName => "Frost Mage";
        public override bool HandlesMovement => false;
        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } =
            new BasicIntellectComparator(null, new List<WowWeaponType>
            {
                WowWeaponType.AxeTwoHand,
                WowWeaponType.MaceTwoHand,
                WowWeaponType.SwordTwoHand
            });

        public override WowClass WowClass => WowClass.Mage;
        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new Dictionary<int, Talent>(),
            Tree2 = new Dictionary<int, Talent>(),
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override bool UseAutoAttacks => true;
        public override bool WalkBehindEnemy => false;

        public override void Execute()
        {
            base.Execute();

            var spellName = SelectSpell(out var targetGuid);
            TryCastSpell(spellName, targetGuid);
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
            HandleDeadPartyMembers(Priest335a.Resurrection);
        }

        private string SelectSpell(out ulong targetGuid)
        {
            if (IsInSpellRange(Bot.Target, Mage335a.Fireball)
                && ValidateSpell(Mage335a.Fireball, true))
            {
                targetGuid = Bot.Target.Guid;
                return Mage335a.Fireball;
            }

            targetGuid = 9999999;
            return string.Empty;
        }
    }
}