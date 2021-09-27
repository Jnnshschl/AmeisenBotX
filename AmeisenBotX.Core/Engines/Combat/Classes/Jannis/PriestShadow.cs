using AmeisenBotX.Core.Logic.Utils.Auras.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class PriestShadow : BasicCombatClass
    {
        public PriestShadow(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Priest335a.Shadowform, () => TryCastSpell(Priest335a.Shadowform, Bot.Wow.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Priest335a.PowerWordFortitude, () => TryCastSpell(Priest335a.PowerWordFortitude, Bot.Wow.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Priest335a.VampiricEmbrace, () => TryCastSpell(Priest335a.VampiricEmbrace, Bot.Wow.PlayerGuid, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Priest335a.VampiricTouch, () => TryCastSpell(Priest335a.VampiricTouch, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Priest335a.DevouringPlague, () => TryCastSpell(Priest335a.DevouringPlague, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Priest335a.ShadowWordPain, () => TryCastSpell(Priest335a.ShadowWordPain, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Priest335a.MindBlast, () => TryCastSpell(Priest335a.MindBlast, Bot.Wow.TargetGuid, true)));

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((Priest335a.PowerWordFortitude, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Shadow Priest spec.";

        public override string DisplayName => "Priest Shadow";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.Shield }, new() { WowWeaponType.Sword, WowWeaponType.Mace, WowWeaponType.Axe });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 2, new(1, 2, 5) },
                { 4, new(1, 4, 3) },
                { 5, new(1, 5, 2) },
                { 7, new(1, 7, 3) },
            },
            Tree2 = new(),
            Tree3 = new()
            {
                { 1, new(3, 1, 3) },
                { 2, new(3, 2, 2) },
                { 3, new(3, 3, 5) },
                { 5, new(3, 5, 2) },
                { 6, new(3, 6, 3) },
                { 8, new(3, 8, 5) },
                { 9, new(3, 9, 1) },
                { 10, new(3, 10, 2) },
                { 11, new(3, 11, 2) },
                { 12, new(3, 12, 3) },
                { 14, new(3, 14, 1) },
                { 16, new(3, 16, 3) },
                { 17, new(3, 17, 2) },
                { 18, new(3, 18, 3) },
                { 19, new(3, 19, 1) },
                { 20, new(3, 20, 5) },
                { 21, new(3, 21, 2) },
                { 22, new(3, 22, 3) },
                { 24, new(3, 24, 1) },
                { 25, new(3, 25, 3) },
                { 26, new(3, 26, 5) },
                { 27, new(3, 27, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.1";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Priest;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if (Bot.Player.ManaPercentage < 90
                    && TryCastSpell(Priest335a.Shadowfiend, Bot.Wow.TargetGuid))
                {
                    return;
                }

                if (Bot.Player.ManaPercentage < 30
                    && TryCastSpell(Priest335a.HymnOfHope, 0))
                {
                    return;
                }

                if (Bot.Player.HealthPercentage < 70
                    && TryCastSpell(Priest335a.FlashHeal, Bot.Wow.TargetGuid, true))
                {
                    return;
                }

                if (!Bot.Player.IsCasting
                    && TryCastSpell(Priest335a.MindFlay, Bot.Wow.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(Priest335a.Smite, Bot.Wow.TargetGuid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (HandleDeadPartymembers(Priest335a.Resurrection))
            {
                return;
            }
        }
    }
}