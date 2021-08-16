using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Engines.Combat.Helpers;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Logic;
using AmeisenBotX.Core.Logic.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class HunterSurvival : BasicCombatClass
    {
        public HunterSurvival(AmeisenBotInterfaces bot) : base(bot)
        {
            PetManager = new PetManager
            (
                Bot,
                TimeSpan.FromSeconds(15),
                () => TryCastSpell(mendPetSpell, 0, true),
                () => TryCastSpell(callPetSpell, 0),
                () => TryCastSpell(revivePetSpell, 0)
            );

            MyAuraManager.Jobs.Add(new KeepBestActiveAuraJob(bot.Db, new List<(string, Func<bool>)>()
            {
                (aspectOfTheViperSpell, () => Bot.Player.ManaPercentage < 25.0 && TryCastSpell(aspectOfTheViperSpell, 0, true)),
                (aspectOfTheDragonhawkSpell, () => (!bot.Character.SpellBook.IsSpellKnown(aspectOfTheViperSpell) || Bot.Player.ManaPercentage > 80.0) && TryCastSpell(aspectOfTheDragonhawkSpell, 0, true)),
                (aspectOfTheHawkSpell, () => TryCastSpell(aspectOfTheHawkSpell, 0, true))
            }));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, huntersMarkSpell, () => TryCastSpell(huntersMarkSpell, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, serpentStingSpell, () => TryCastSpell(serpentStingSpell, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, blackArrowSpell, () => TryCastSpell(blackArrowSpell, Bot.Wow.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(wyvernStingSpell, x.Guid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Survival Hunter spec.";

        public override string Displayname => "Hunter Survival";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.SHIELDS });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new(),
            Tree2 = new()
            {
                { 3, new(2, 3, 5) },
                { 4, new(2, 4, 3) },
                { 6, new(2, 6, 5) },
                { 7, new(2, 7, 1) },
                { 9, new(2, 9, 1) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 5) },
                { 6, new(3, 6, 3) },
                { 7, new(3, 7, 2) },
                { 8, new(3, 8, 5) },
                { 12, new(3, 12, 3) },
                { 13, new(3, 13, 3) },
                { 14, new(3, 14, 3) },
                { 15, new(3, 15, 3) },
                { 17, new(3, 17, 5) },
                { 18, new(3, 18, 2) },
                { 19, new(3, 19, 3) },
                { 20, new(3, 20, 1) },
                { 21, new(3, 21, 3) },
                { 22, new(3, 22, 4) },
                { 23, new(3, 23, 3) },
                { 25, new(3, 25, 1) },
                { 26, new(3, 26, 3) },
                { 27, new(3, 27, 3) },
                { 28, new(3, 28, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Hunter;

        private PetManager PetManager { get; set; }

        private bool ReadyToDisengage { get; set; } = false;

        private bool SlowTargetWhenPossible { get; set; } = false;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if (PetManager.Tick()) { return; }

                if (Bot.Target != null)
                {
                    double distanceToTarget = Bot.Target.Position.GetDistance(Bot.Player.Position);

                    // make some distance
                    if ((Bot.Target.Type == WowObjectType.Player && Bot.Wow.TargetGuid != 0 && distanceToTarget < 10.0)
                        || (Bot.Target.Type == WowObjectType.Unit && Bot.Wow.TargetGuid != 0 && distanceToTarget < 3.0))
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Flee, Bot.Target.Position, Bot.Target.Rotation);
                    }

                    if (Bot.Player.HealthPercentage < 15.0
                        && TryCastSpell(feignDeathSpell, 0))
                    {
                        return;
                    }

                    if (distanceToTarget < 5.0)
                    {
                        if (ReadyToDisengage
                            && TryCastSpell(disengageSpell, 0, true))
                        {
                            ReadyToDisengage = false;
                            return;
                        }

                        if (TryCastSpell(frostTrapSpell, 0, true))
                        {
                            ReadyToDisengage = true;
                            SlowTargetWhenPossible = true;
                            return;
                        }

                        if (Bot.Player.HealthPercentage < 30.0
                            && TryCastSpell(deterrenceSpell, 0, true))
                        {
                            return;
                        }

                        if (TryCastSpell(raptorStrikeSpell, Bot.Wow.TargetGuid, true)
                            || TryCastSpell(mongooseBiteSpell, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (SlowTargetWhenPossible
                            && TryCastSpell(concussiveShotSpell, Bot.Wow.TargetGuid, true))
                        {
                            SlowTargetWhenPossible = false;
                            return;
                        }

                        if (Bot.Target.HealthPercentage < 20.0
                            && TryCastSpell(killShotSpell, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        TryCastSpell(killCommandSpell, Bot.Wow.TargetGuid, true);
                        TryCastSpell(rapidFireSpell, 0);

                        if (Bot.GetNearEnemies<IWowUnit>(Bot.Target.Position, 16.0f).Count() > 2
                            && TryCastSpell(multiShotSpell, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        if ((Bot.Objects.WowObjects.OfType<IWowUnit>().Where(e => Bot.Target.Position.GetDistance(e.Position) < 16.0).Count() > 2 && TryCastSpell(multiShotSpell, Bot.Wow.TargetGuid, true))
                            || TryCastSpell(explosiveShotSpell, Bot.Wow.TargetGuid, true)
                            || TryCastSpell(aimedShotSpell, Bot.Wow.TargetGuid, true)
                            || TryCastSpell(steadyShotSpell, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            ReadyToDisengage = false;
            SlowTargetWhenPossible = false;

            base.OutOfCombatExecute();

            if (PetManager.Tick())
            {
                return;
            }
        }
    }
}