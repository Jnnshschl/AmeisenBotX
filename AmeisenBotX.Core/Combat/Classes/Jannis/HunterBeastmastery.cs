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
            PetManager = new PetManager
            (
                WowInterface,
                TimeSpan.FromSeconds(5),
                () => TryCastSpell(mendPetSpell, 0, true),
                () => TryCastSpell(callPetSpell, 0),
                () => TryCastSpell(revivePetSpell, 0)
            );

            MyAuraManager.Jobs.Add(new KeepBestActiveAuraJob(new List<(string, Func<bool>)>()
            {
                (aspectOfTheViperSpell, () => WowInterface.ObjectManager.Player.ManaPercentage < 25.0 && TryCastSpell(aspectOfTheViperSpell, 0, true)),
                (aspectOfTheDragonhawkSpell, () => (!wowInterface.CharacterManager.SpellBook.IsSpellKnown(aspectOfTheViperSpell) || WowInterface.ObjectManager.Player.ManaPercentage > 80.0) && TryCastSpell(aspectOfTheDragonhawkSpell, 0, true)),
                (aspectOfTheHawkSpell, () => TryCastSpell(aspectOfTheHawkSpell, 0, true))
            }));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(huntersMarkSpell, () => TryCastSpell(huntersMarkSpell, WowInterface.ObjectManager.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(serpentStingSpell, () => TryCastSpell(serpentStingSpell, WowInterface.ObjectManager.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(scatterShotSpell, x.Guid, true) },
                { 1, (x) => TryCastSpell(intimidationSpell, x.Guid, true) }
            };
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

            if (SelectTarget(TargetManagerDps))
            {
                if (PetManager.Tick()) { return; }

                if (WowInterface.ObjectManager.Target != null)
                {
                    float distanceToTarget = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                    if (WowInterface.ObjectManager.Player.HealthPercentage < 15.0
                        && TryCastSpell(feignDeathSpell, 0))
                    {
                        LastAction = DateTime.UtcNow;
                        return;
                    }

                    if (distanceToTarget < 6.0)
                    {
                        if (ReadyToDisengage
                            && TryCastSpell(disengageSpell, 0, true))
                        {
                            ReadyToDisengage = false;
                            LastAction = DateTime.UtcNow;
                            return;
                        }

                        if (TryCastSpell(frostTrapSpell, 0, true))
                        {
                            ReadyToDisengage = true;
                            LastAction = DateTime.UtcNow;
                            return;
                        }

                        if (WowInterface.ObjectManager.Player.HealthPercentage < 30
                            && TryCastSpell(deterrenceSpell, 0, true))
                        {
                            LastAction = DateTime.UtcNow;
                            return;
                        }

                        if (TryCastSpell(raptorStrikeSpell, WowInterface.ObjectManager.TargetGuid, true)
                            || TryCastSpell(mongooseBiteSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                        }
                    }
                    else if (distanceToTarget < 24.0f)
                    {
                        if (distanceToTarget < 16.0f
                            || distanceToTarget > 22.0f
                            && !WowInterface.Target.HasBuffByName(concussiveShotSpell)
                            && !WowInterface.Target.HasBuffByName("Frost Trap Aura")
                            && TryCastSpell(concussiveShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            LastAction = DateTime.UtcNow;
                            return;
                        }

                        if (WowInterface.ObjectManager.Target.HealthPercentage < 20.0
                            && TryCastSpell(killShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            LastAction = DateTime.UtcNow;
                            return;
                        }

                        TryCastSpell(killCommandSpell, WowInterface.ObjectManager.TargetGuid, true);
                        TryCastSpell(beastialWrathSpell, WowInterface.ObjectManager.TargetGuid, true);
                        TryCastSpell(rapidFireSpell, 0);

                        if (WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Target.Position, 16.0).Count() > 2
                            && TryCastSpell(multiShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            LastAction = DateTime.UtcNow;
                            return;
                        }

                        if (TryCastSpell(arcaneShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            LastAction = DateTime.UtcNow;
                            return;
                        }

                        // only cast when we are far away and disengage is ready
                        if ((WowInterface.ObjectManager.Target.Type == WowObjectType.Player && distanceToTarget > 21.0f && !CooldownManager.IsSpellOnCooldown(disengageSpell))
                            || (WowInterface.ObjectManager.Target.Type == WowObjectType.Unit && distanceToTarget > 5.0f))
                        {
                            if (TryCastSpell(steadyShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                            {
                                LastAction = DateTime.UtcNow;
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (!WowInterface.Target.HasBuffByName(concussiveShotSpell)
                            && !WowInterface.Target.HasBuffByName("Frost Trap Aura")
                            && TryCastSpell(concussiveShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            LastAction = DateTime.UtcNow;
                            return;
                        }

                        // move to position
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowInterface.ObjectManager.Target.Position, WowInterface.ObjectManager.Target.Rotation);
                        return;
                    }

                    // nothing to do, run away
                    if (DateTime.UtcNow - TimeSpan.FromMilliseconds(400) > LastAction)
                    {
                        if (RunningAway)
                        {
                            if ((WowInterface.ObjectManager.Target.Type == WowObjectType.Player && distanceToTarget < 22.0f)
                                || (WowInterface.ObjectManager.Target.Type == WowObjectType.Unit && distanceToTarget < 7.0f))
                            {
                                WowInterface.MovementEngine.SetMovementAction(MovementAction.Flee, WowInterface.ObjectManager.Target.Position, WowInterface.ObjectManager.Target.Rotation);
                            }
                            else
                            {
                                RunningAway = false;
                            }
                        }
                        else
                        {
                            if ((WowInterface.ObjectManager.Target.Type == WowObjectType.Player && distanceToTarget < 18.0f)
                                || (WowInterface.ObjectManager.Target.Type == WowObjectType.Unit && distanceToTarget < 5.0f))
                            {
                                RunningAway = true;
                            }
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