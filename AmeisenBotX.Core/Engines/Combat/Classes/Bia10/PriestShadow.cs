using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Bia10
{
    public class PriestShadow : BasicCombatClassBia10
    {
        public PriestShadow(AmeisenBotInterfaces bot) : base(bot)
        {
        }

        public override string Version => "1.0";
        public override string Description => "CombatClass for the Shadow Priest spec.";
        public override string DisplayName => "Shadow Priest";
        public override bool HandlesMovement => false;
        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } =
            new BasicIntellectComparator(null, new List<WowWeaponType>
            {
                WowWeaponType.AxeTwoHand,
                WowWeaponType.MaceTwoHand,
                WowWeaponType.SwordTwoHand
            });

        public override WowClass WowClass => WowClass.Priest;
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

            if (HandleDeadPartyMembers(Shaman335a.AncestralSpirit))
                return;
        }

        private string SelectSpell(out ulong targetGuid)
        {
            /*if (Bot.Player.HealthPercentage < DataConstants.HealSelfPercentage
                && ValidateSpell(Shaman335a.HealingWave, true))
            {
                targetGuid = Bot.Player.Guid;
                return Shaman335a.HealingWave;
            }
            if (Bot.Target?.HealthPercentage >= 3
                && IsInSpellRange(Bot.Target, Shaman335a.EarthShock)
                && ValidateSpell(Shaman335a.EarthShock, true))
            {
                targetGuid = Bot.Target.Guid;
                return Shaman335a.EarthShock;
            }*/


            if (IsInSpellRange(Bot.Target, Priest335a.Smite)
                && ValidateSpell(Priest335a.Smite, true))
            {
                targetGuid = Bot.Target.Guid;
                return Priest335a.Smite;
            }

            targetGuid = 9999999;
            return string.Empty;
        }
    }
}