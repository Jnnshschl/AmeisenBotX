using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Logic.CombatClasses.Shino;
using AmeisenBotX.Core.Managers.Character;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Shino
{
    public class PriestShadow : TemplateCombatClass
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

        public override string DisplayName2 => "Priest Shadow";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator
        {
            get =>
                new SimpleItemComparator((DefaultCharacterManager)Bot.Character, new Dictionary<string, double>()
                {
                    { WowStatType.INTELLECT, 2.5 },
                    { WowStatType.SPELL_POWER, 2.5 },
                    { WowStatType.ARMOR, 2.0 },
                    { WowStatType.MP5, 2.0 },
                    { WowStatType.HASTE, 2.0 },
                });
            set { }
        }

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new()
            {
                { 2, new Talent(1, 2, 5) },
                { 4, new Talent(1, 4, 3) },
                { 5, new Talent(1, 5, 2) },
                { 7, new Talent(1, 7, 3) },
            },
            Tree2 = new(),
            Tree3 = new()
            {
                { 1, new Talent(3, 1, 3) },
                { 2, new Talent(3, 2, 2) },
                { 3, new Talent(3, 3, 5) },
                { 5, new Talent(3, 5, 2) },
                { 6, new Talent(3, 6, 3) },
                { 8, new Talent(3, 8, 5) },
                { 9, new Talent(3, 9, 1) },
                { 10, new Talent(3, 10, 2) },
                { 11, new Talent(3, 11, 2) },
                { 12, new Talent(3, 12, 3) },
                { 14, new Talent(3, 14, 1) },
                { 16, new Talent(3, 16, 3) },
                { 17, new Talent(3, 17, 2) },
                { 18, new Talent(3, 18, 3) },
                { 19, new Talent(3, 19, 1) },
                { 20, new Talent(3, 20, 5) },
                { 21, new Talent(3, 21, 2) },
                { 22, new Talent(3, 22, 3) },
                { 24, new Talent(3, 24, 1) },
                { 25, new Talent(3, 25, 3) },
                { 26, new Talent(3, 26, 5) },
                { 27, new Talent(3, 27, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.2";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Priest;

        public override WowVersion WowVersion => WowVersion.WotLK335a;

        public override void Execute()
        {
            base.Execute();

            if (Bot.Target == null)
            {
                return;
            }

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

            if (Bot.Player.ManaPercentage >= 50
                && TryCastSpell(Racials335a.Berserking, Bot.Wow.TargetGuid))
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

        protected override Spell GetOpeningSpell()
        {
            Spell spell = Bot.Character.SpellBook.GetSpellByName(Priest335a.ShadowWordPain);
            if (spell != null)
            {
                return spell;
            }
            return Bot.Character.SpellBook.GetSpellByName(Priest335a.Smite);
        }
    }
}