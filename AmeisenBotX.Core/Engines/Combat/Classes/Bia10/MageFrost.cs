using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Bia10
{
    public class MageFrost : BasicCombatClassBia10
    {
        public MageFrost(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.FrostArmor, () =>
                Bot.Player.ManaPercentage > 20.0
                && ValidateSpell(Mage335a.FrostArmor, true)
                && TryCastSpell(Mage335a.FrostArmor, Bot.Player.Guid)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.ArcaneIntellect, () =>
                Bot.Player.ManaPercentage > 30.0
                && ValidateSpell(Mage335a.ArcaneIntellect, true)
                && TryCastSpell(Mage335a.ArcaneIntellect, Bot.Player.Guid)));
        }

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

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new(),
            Tree2 = new(),
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Mage;

        public override void Execute()
        {
            base.Execute();

            string spellName = SelectSpell(out ulong targetGuid);
            TryCastSpell(spellName, targetGuid);
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
        }

        private string SelectSpell(out ulong targetGuid)
        {
            if (IsInSpellRange(Bot.Target, Mage335a.FireBlast)
                && ValidateSpell(Mage335a.FireBlast, true)
                && Bot.Target.HealthPercentage > 10)
            {
                targetGuid = Bot.Target.Guid;
                return Mage335a.FireBlast;
            }
            if (IsInSpellRange(Bot.Target, Mage335a.Fireball)
                && ValidateSpell(Mage335a.Fireball, true)
                && !IsInSpellRange(Bot.Target, Mage335a.FrostBolt))
            {
                targetGuid = Bot.Target.Guid;
                return Mage335a.Fireball;
            }
            if (IsInSpellRange(Bot.Target, Mage335a.FrostBolt)
                && ValidateSpell(Mage335a.FrostBolt, true))
            {
                targetGuid = Bot.Target.Guid;
                return Mage335a.FrostBolt;
            }
            if (Bot.GetEnemiesOrNeutralsInCombatWithMe<IWowUnit>(Bot.Player.Position, 10).Count() >= 2
                || Bot.GetEnemiesOrNeutralsTargetingMe<IWowUnit>(Bot.Player.Position, 10).Count() >= 2
                && ValidateSpell(Mage335a.FrostNova, true))
            {
                targetGuid = 9999999;
                return Mage335a.FrostNova;
            }

            targetGuid = 9999999;
            return string.Empty;
        }
    }
}