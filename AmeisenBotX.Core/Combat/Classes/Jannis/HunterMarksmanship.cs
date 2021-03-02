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
    public class HunterMarksmanship : BasicCombatClass
    {
        public HunterMarksmanship(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            PetManager = new PetManager
            (
                WowInterface,
                TimeSpan.FromSeconds(15),
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
                { 0, (x) => TryCastSpell(silencingShotSpell, x.Guid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Marksmanship Hunter spec.";

        public override string Displayname => "Hunter Marksmanship";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.SHIELDS });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 5) },
                { 3, new(1, 3, 2) },
            },
            Tree2 = new()
            {
                { 2, new(2, 2, 3) },
                { 3, new(2, 3, 5) },
                { 4, new(2, 4, 3) },
                { 6, new(2, 6, 5) },
                { 7, new(2, 7, 1) },
                { 8, new(2, 8, 3) },
                { 9, new(2, 9, 1) },
                { 11, new(2, 11, 3) },
                { 14, new(2, 14, 1) },
                { 15, new(2, 15, 3) },
                { 16, new(2, 16, 2) },
                { 17, new(2, 17, 3) },
                { 18, new(2, 18, 3) },
                { 19, new(2, 19, 1) },
                { 20, new(2, 20, 3) },
                { 21, new(2, 21, 5) },
                { 23, new(2, 23, 3) },
                { 25, new(2, 25, 3) },
                { 26, new(2, 26, 5) },
                { 27, new(2, 27, 1) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 5) },
                { 7, new(3, 7, 2) },
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

            if (SelectTarget(TargetManagerDps))
            {
                if (PetManager.Tick()) { return; }

                WowUnit target = (WowUnit)WowInterface.ObjectManager.WowObjects.FirstOrDefault(e => e != null && e.Guid == WowInterface.TargetGuid);

                if (target != null)
                {
                    double distanceToTarget = target.Position.GetDistance(WowInterface.Player.Position);

                    // make some distance
                    if ((WowInterface.Target.Type == WowObjectType.Player && WowInterface.TargetGuid != 0 && distanceToTarget < 10.0)
                        || (WowInterface.Target.Type == WowObjectType.Unit && WowInterface.TargetGuid != 0 && distanceToTarget < 3.0))
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Flee, WowInterface.Target.Position, WowInterface.Target.Rotation);
                    }

                    if (WowInterface.Player.HealthPercentage < 15
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

                        if (WowInterface.Player.HealthPercentage < 30
                            && TryCastSpell(deterrenceSpell, 0, true))
                        {
                            return;
                        }

                        if (TryCastSpell(raptorStrikeSpell, WowInterface.TargetGuid, true)
                            || TryCastSpell(mongooseBiteSpell, WowInterface.TargetGuid, true))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (SlowTargetWhenPossible
                            && TryCastSpell(disengageSpell, 0, true))
                        {
                            SlowTargetWhenPossible = false;
                            return;
                        }

                        if (target.HealthPercentage < 20
                            && TryCastSpell(killShotSpell, WowInterface.TargetGuid, true))
                        {
                            return;
                        }

                        TryCastSpell(killCommandSpell, WowInterface.TargetGuid, true);
                        TryCastSpell(rapidFireSpell, WowInterface.TargetGuid);

                        if (WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.Target.Position, 16.0f).Count() > 2
                            && TryCastSpell(multiShotSpell, WowInterface.TargetGuid, true))
                        {
                            return;
                        }

                        if ((WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => target.Position.GetDistance(e.Position) < 16).Count() > 2 && TryCastSpell(multiShotSpell, WowInterface.TargetGuid, true))
                            || TryCastSpell(chimeraShotSpell, WowInterface.TargetGuid, true)
                            || TryCastSpell(aimedShotSpell, WowInterface.TargetGuid, true)
                            || TryCastSpell(arcaneShotSpell, WowInterface.TargetGuid, true)
                            || TryCastSpell(steadyShotSpell, WowInterface.TargetGuid, true))
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