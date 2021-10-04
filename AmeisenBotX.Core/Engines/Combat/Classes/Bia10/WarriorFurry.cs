using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Bia10
{
    public class WarrirorFurry : BasicCombatClassBia10
    {
        public WarrirorFurry(AmeisenBotInterfaces bot) : base(bot)
        {
        }

        public override string Version => "1.0";
        public override string Description => "CombatClass for the Warrior Furry spec.";
        public override string DisplayName => "Warrior Furry";
        public override bool HandlesMovement => false;
        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } =
            new BasicStrengthComparator(null, new List<WowWeaponType>
            {
                WowWeaponType.AxeTwoHand,
                WowWeaponType.MaceTwoHand,
                WowWeaponType.SwordTwoHand
            });

        public override WowClass WowClass => WowClass.Warrior;
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
            var spellCast = TryCastSpell(spellName, targetGuid);
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
        }

        private string SelectSpell(out ulong targetGuid)
        {
            if (IsInSpellRange(Bot.Target, Warrior335a.HeroicStrike)
                && ValidateSpell(Warrior335a.HeroicStrike, true))
            {
                targetGuid = Bot.Target.Guid;
                return Warrior335a.HeroicStrike;
            }

            targetGuid = 9999999;
            return string.Empty;
        }
    }
}