using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Engines.Combat.Helpers;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class HunterBeastmastery : BasicCombatClass
    {
        public HunterBeastmastery(AmeisenBotInterfaces bot, AmeisenBotFsm stateMachine) : base(bot, stateMachine)
        {
            PetManager = new
            (
                Bot,
                TimeSpan.FromSeconds(5),
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

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(scatterShotSpell, x.Guid, true) },
                { 1, (x) => TryCastSpell(intimidationSpell, x.Guid, true) }
            };

            ConfigurableThresholds.TryAdd("KitingEndDistanceUnit", 12.0f);
            ConfigurableThresholds.TryAdd("SteadyShotMinDistanceUnit", 12.0f);
            ConfigurableThresholds.TryAdd("ChaseDistanceUnit", 20.0f);

            ConfigurableThresholds.TryAdd("KitingStartDistancePlayer", 8.0f);
            ConfigurableThresholds.TryAdd("KitingEndDistancePlayer", 22.0f);
            ConfigurableThresholds.TryAdd("SteadyShotMinDistancePlayer", 22.0f);
            ConfigurableThresholds.TryAdd("ChaseDistancePlayer", 24.0f);

            ConfigurableThresholds.TryAdd("FleeActionCooldown", 400);
        }

        public override string Description => "FCFS based CombatClass for the Beastmastery Hunter spec.";

        public override string DisplayName => "Hunter Beastmastery";

        public override bool HandlesMovement => true;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.SHIELDS });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 5) },
                { 2, new(1, 2, 1) },
                { 3, new(1, 3, 2) },
                { 6, new(1, 6, 2) },
                { 8, new(1, 8, 1) },
                { 9, new(1, 9, 5) },
                { 11, new(1, 11, 5) },
                { 12, new(1, 12, 1) },
                { 13, new(1, 13, 1) },
                { 14, new(1, 14, 2) },
                { 15, new(1, 15, 2) },
                { 16, new(1, 16, 5) },
                { 17, new(1, 17, 3) },
                { 18, new(1, 18, 1) },
                { 21, new(1, 21, 5) },
                { 22, new(1, 22, 3) },
                { 23, new(1, 23, 1) },
                { 24, new(1, 24, 3) },
                { 25, new(1, 25, 5) },
                { 26, new(1, 26, 1) },
            },
            Tree2 = new()
            {
                { 2, new(1, 2, 1) },
                { 3, new(1, 3, 5) },
                { 4, new(1, 4, 3) },
                { 6, new(1, 6, 5) },
                { 8, new(1, 8, 3) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Hunter;

        private DateTime LastAction { get; set; }

        private PetManager PetManager { get; set; }

        private bool ReadyToDisengage { get; set; } = false;

        private bool RunningAway { get; set; } = false;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if (PetManager.Tick()) { return; }

                if (Bot.Target != null)
                {
                    float distanceToTarget = Bot.Target.Position.GetDistance(Bot.Player.Position);

                    if (Bot.Player.HealthPercentage < 15.0
                        && TryCastSpell(feignDeathSpell, 0))
                    {
                        return;
                    }

                    if (distanceToTarget < (Bot.Target.IsPlayer() ? ConfigurableThresholds["KitingStartDistancePlayer"] : ConfigurableThresholds["KitingStartDistanceUnit"]))
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
                            return;
                        }

                        if (Bot.Player.HealthPercentage < 30.0
                            && TryCastSpell(deterrenceSpell, 0, true))
                        {
                            return;
                        }

                        TryCastSpell(raptorStrikeSpell, Bot.Wow.TargetGuid, true);
                        TryCastSpell(mongooseBiteSpell, Bot.Wow.TargetGuid, true);
                    }
                    else if (distanceToTarget < (Bot.Target.IsPlayer() ? ConfigurableThresholds["KitingEndDistancePlayer"] : ConfigurableThresholds["KitingEndDistanceUnit"]))
                    {
                        if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == concussiveShotSpell)
                            && !Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Frost Trap Aura")
                            && TryCastSpell(concussiveShotSpell, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        if (Bot.Target.HealthPercentage < 20.0
                            && TryCastSpell(killShotSpell, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        TryCastSpell(killCommandSpell, Bot.Wow.TargetGuid, true);
                        TryCastSpell(beastialWrathSpell, Bot.Wow.TargetGuid, true);
                        TryCastSpell(rapidFireSpell, 0);

                        if (Bot.GetNearEnemies<IWowUnit>(Bot.Target.Position, 16.0f).Count() > 2
                            && TryCastSpell(multiShotSpell, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        if (TryCastSpell(arcaneShotSpell, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        // only cast when we are far away and disengage is ready
                        if (distanceToTarget > (Bot.Target.IsPlayer() ? ConfigurableThresholds["SteadyShotMinDistancePlayer"] : ConfigurableThresholds["SteadyShotMinDistanceUnit"])
                            && TryCastSpell(steadyShotSpell, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }
                    }
                    else if (distanceToTarget < (Bot.Target.IsPlayer() ? ConfigurableThresholds["ChaseDistancePlayer"] : ConfigurableThresholds["ChaseDistanceUnit"]))
                    {
                        if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == concussiveShotSpell)
                            && !Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Frost Trap Aura")
                            && TryCastSpell(concussiveShotSpell, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        // move to position
                        Bot.Movement.SetMovementAction(MovementAction.Move, Bot.Target.Position, Bot.Target.Rotation);
                        return;
                    }

                    // nothing to do, run away
                    if (DateTime.UtcNow - TimeSpan.FromMilliseconds(ConfigurableThresholds["FleeActionCooldown"]) > LastSpellCast)
                    {
                        if (RunningAway)
                        {
                            if (distanceToTarget < (Bot.Target.IsPlayer() ? ConfigurableThresholds["KitingEndDistancePlayer"] : ConfigurableThresholds["KitingEndDistanceUnit"]))
                            {
                                Bot.Movement.SetMovementAction(MovementAction.Flee, Bot.Target.Position, Bot.Target.Rotation);
                            }
                            else
                            {
                                RunningAway = false;
                            }
                        }
                        else if (distanceToTarget < (Bot.Target.IsPlayer() ? ConfigurableThresholds["KitingStartDistancePlayer"] : ConfigurableThresholds["KitingStartDistanceUnit"]))
                        {
                            RunningAway = true;
                        }
                    }
                    else
                    {
                        Bot.Movement.Reset();
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            ReadyToDisengage = false;

            base.OutOfCombatExecute();

            if (PetManager.Tick())
            {
                return;
            }
        }
    }
}