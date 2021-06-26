using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class HunterBeastmastery : BasicCombatClass
    {
        public HunterBeastmastery(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            PetManager = new
            (
                WowInterface,
                TimeSpan.FromSeconds(5),
                () => TryCastSpell(mendPetSpell, 0, true),
                () => TryCastSpell(callPetSpell, 0),
                () => TryCastSpell(revivePetSpell, 0)
            );

            MyAuraManager.Jobs.Add(new KeepBestActiveAuraJob(new List<(string, Func<bool>)>()
            {
                (aspectOfTheViperSpell, () => WowInterface.Player.ManaPercentage < 25.0 && TryCastSpell(aspectOfTheViperSpell, 0, true)),
                (aspectOfTheDragonhawkSpell, () => (!wowInterface.CharacterManager.SpellBook.IsSpellKnown(aspectOfTheViperSpell) || WowInterface.Player.ManaPercentage > 80.0) && TryCastSpell(aspectOfTheDragonhawkSpell, 0, true)),
                (aspectOfTheHawkSpell, () => TryCastSpell(aspectOfTheHawkSpell, 0, true))
            }));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(huntersMarkSpell, () => TryCastSpell(huntersMarkSpell, WowInterface.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(serpentStingSpell, () => TryCastSpell(serpentStingSpell, WowInterface.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(scatterShotSpell, x.Guid, true) },
                { 1, (x) => TryCastSpell(intimidationSpell, x.Guid, true) }
            };

            C.Add("KitingStartDistanceUnit", 3.0f);
            C.Add("KitingEndDistanceUnit", 12.0f);
            C.Add("SteadyShotMinDistanceUnit", 12.0f);
            C.Add("ChaseDistanceUnit", 20.0f);

            C.Add("KitingStartDistancePlayer", 8.0f);
            C.Add("KitingEndDistancePlayer", 22.0f);
            C.Add("SteadyShotMinDistancePlayer", 22.0f);
            C.Add("ChaseDistancePlayer", 24.0f);

            C.Add("FleeActionCooldown", 400);
        }

        public override string Description => "FCFS based CombatClass for the Beastmastery Hunter spec.";

        public override string Displayname => "Hunter Beastmastery";

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

                if (WowInterface.Target != null)
                {
                    float distanceToTarget = WowInterface.Target.Position.GetDistance(WowInterface.Player.Position);

                    if (WowInterface.Player.HealthPercentage < 15.0
                        && TryCastSpell(feignDeathSpell, 0))
                    {
                        return;
                    }

                    if (distanceToTarget < (WowInterface.Target.IsPlayer() ? C["KitingStartDistancePlayer"] : C["KitingStartDistanceUnit"]))
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

                        if (WowInterface.Player.HealthPercentage < 30.0
                            && TryCastSpell(deterrenceSpell, 0, true))
                        {
                            return;
                        }

                        TryCastSpell(raptorStrikeSpell, WowInterface.TargetGuid, true);
                        TryCastSpell(mongooseBiteSpell, WowInterface.TargetGuid, true);
                    }
                    else if (distanceToTarget < (WowInterface.Target.IsPlayer() ? C["KitingEndDistancePlayer"] : C["KitingEndDistanceUnit"]))
                    {
                        if (!WowInterface.Target.HasBuffByName(concussiveShotSpell)
                            && !WowInterface.Target.HasBuffByName("Frost Trap Aura")
                            && TryCastSpell(concussiveShotSpell, WowInterface.TargetGuid, true))
                        {
                            return;
                        }

                        if (WowInterface.Target.HealthPercentage < 20.0
                            && TryCastSpell(killShotSpell, WowInterface.TargetGuid, true))
                        {
                            return;
                        }

                        TryCastSpell(killCommandSpell, WowInterface.TargetGuid, true);
                        TryCastSpell(beastialWrathSpell, WowInterface.TargetGuid, true);
                        TryCastSpell(rapidFireSpell, 0);

                        if (WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.Target.Position, 16.0f).Count() > 2
                            && TryCastSpell(multiShotSpell, WowInterface.TargetGuid, true))
                        {
                            return;
                        }

                        if (TryCastSpell(arcaneShotSpell, WowInterface.TargetGuid, true))
                        {
                            return;
                        }

                        // only cast when we are far away and disengage is ready
                        if (distanceToTarget > (WowInterface.Target.IsPlayer() ? C["SteadyShotMinDistancePlayer"] : C["SteadyShotMinDistanceUnit"])
                            && TryCastSpell(steadyShotSpell, WowInterface.TargetGuid, true))
                        {
                            return;
                        }
                    }
                    else if (distanceToTarget < (WowInterface.Target.IsPlayer() ? C["ChaseDistancePlayer"] : C["ChaseDistanceUnit"]))
                    {
                        if (!WowInterface.Target.HasBuffByName(concussiveShotSpell)
                            && !WowInterface.Target.HasBuffByName("Frost Trap Aura")
                            && TryCastSpell(concussiveShotSpell, WowInterface.TargetGuid, true))
                        {
                            return;
                        }

                        // move to position
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowInterface.Target.Position, WowInterface.Target.Rotation);
                        return;
                    }

                    // nothing to do, run away
                    if (DateTime.UtcNow - TimeSpan.FromMilliseconds(C["FleeActionCooldown"]) > LastSpellCast)
                    {
                        if (RunningAway)
                        {
                            if (distanceToTarget < (WowInterface.Target.IsPlayer() ? C["KitingEndDistancePlayer"] : C["KitingEndDistanceUnit"]))
                            {
                                WowInterface.MovementEngine.SetMovementAction(MovementAction.Flee, WowInterface.Target.Position, WowInterface.Target.Rotation);
                            }
                            else
                            {
                                RunningAway = false;
                            }
                        }
                        else if (distanceToTarget < (WowInterface.Target.IsPlayer() ? C["KitingStartDistancePlayer"] : C["KitingStartDistanceUnit"]))
                        {
                            RunningAway = true;
                        }
                    }
                    else
                    {
                        WowInterface.MovementEngine.Reset();
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