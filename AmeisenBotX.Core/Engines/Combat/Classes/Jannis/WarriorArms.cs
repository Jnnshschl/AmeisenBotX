using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Logic.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class WarriorArms : BasicCombatClass
    {
        public WarriorArms(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Warrior335a.BattleShout, () => TryCastSpell(Warrior335a.BattleShout, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Warrior335a.Hamstring, () => Bot.Target?.Type == WowObjectType.Player && TryCastSpell(Warrior335a.Hamstring, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Warrior335a.Rend, () => Bot.Target?.Type == WowObjectType.Player && Bot.Player.Rage > 75 && TryCastSpell(Warrior335a.Rend, Bot.Wow.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpellWarrior(Warrior335a.IntimidatingShout, Warrior335a.BerserkerStance, x.Guid, true) },
                { 1, (x) => TryCastSpellWarrior(Warrior335a.IntimidatingShout, Warrior335a.BattleStance, x.Guid, true) }
            };

            HeroicStrikeEvent = new(TimeSpan.FromSeconds(2));
        }

        public override string Description => "FCFS based CombatClass for the Arms Warrior spec.";

        public override string DisplayName => "Warrior Arms";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 3) },
                { 3, new(1, 3, 2) },
                { 4, new(1, 4, 2) },
                { 6, new(1, 6, 3) },
                { 7, new(1, 7, 2) },
                { 8, new(1, 8, 1) },
                { 9, new(1, 9, 2) },
                { 10, new(1, 10, 3) },
                { 11, new(1, 11, 3) },
                { 12, new(1, 12, 3) },
                { 13, new(1, 13, 5) },
                { 14, new(1, 14, 1) },
                { 17, new(1, 17, 2) },
                { 19, new(1, 19, 2) },
                { 21, new(1, 21, 1) },
                { 22, new(1, 22, 2) },
                { 24, new(1, 24, 1) },
                { 25, new(1, 25, 3) },
                { 26, new(1, 26, 2) },
                { 27, new(1, 27, 3) },
                { 28, new(1, 28, 1) },
                { 29, new(1, 29, 2) },
                { 30, new(1, 30, 5) },
                { 31, new(1, 31, 1) },
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 3) },
                { 2, new(2, 2, 2) },
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 7, new(2, 7, 1) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Warrior;

        private TimegatedEvent HeroicStrikeEvent { get; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if (Bot.Target != null)
                {
                    double distanceToTarget = Bot.Target.Position.GetDistance(Bot.Player.Position);

                    if (distanceToTarget > 3.0)
                    {
                        if (TryCastSpellWarrior(Warrior335a.Charge, Warrior335a.BattleStance, Bot.Wow.TargetGuid, true)
                            || TryCastSpellWarrior(Warrior335a.Intercept, Warrior335a.BerserkerStance, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if ((Bot.Target.HealthPercentage < 20 || Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Sudden Death"))
                           && TryCastSpellWarrior(Warrior335a.Execute, Warrior335a.BattleStance, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        if ((Bot.Objects.WowObjects.OfType<IWowUnit>().Where(e => Bot.Target.Position.GetDistance(e.Position) < 8).Count() > 2 && TryCastSpell(Warrior335a.Bladestorm, 0, true))
                            || TryCastSpellWarrior(Warrior335a.Overpower, Warrior335a.BattleStance, Bot.Wow.TargetGuid, true)
                            || TryCastSpellWarrior(Warrior335a.MortalStrike, Warrior335a.BattleStance, Bot.Wow.TargetGuid, true)
                            || (HeroicStrikeEvent.Run() && TryCastSpellWarrior(Warrior335a.HeroicStrike, Warrior335a.BattleStance, Bot.Wow.TargetGuid, true)))
                        {
                            return;
                        }
                    }
                }
            }
        }
    }
}