using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class HunterMarksmanship : BasicCombatClass
    {
        public HunterMarksmanship(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            PetManager = new PetManager
            (
                WowInterface,
                TimeSpan.FromSeconds(15),
                () => TryCastSpell(mendPetSpell, 0, true),
                () => TryCastSpell(callPetSpell, 0),
                () => TryCastSpell(revivePetSpell, 0)
            );

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { aspectOfTheViperSpell, () => WowInterface.ObjectManager.Player.ManaPercentage < 20.0 && TryCastSpell(aspectOfTheViperSpell, 0, true) }
            };

            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                if (SpellChain.Get(WowInterface.CharacterManager.SpellBook.IsSpellKnown, out string aspectToUse, aspectOfTheDragonhawkSpell, aspectOfTheHawkSpell))
                {
                    MyAuraManager.BuffsToKeepActive.Add(aspectToUse, () => WowInterface.ObjectManager.Player.ManaPercentage > 50.0 && TryCastSpell(aspectToUse, 0, true));
                }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { huntersMarkSpell, () => TryCastSpell(huntersMarkSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { serpentStingSpell, () => TryCastSpell(serpentStingSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpell(silencingShotSpell, x.Guid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Marksmanship Hunter spec.";

        public override string Displayname => "Hunter Marksmanship";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIELDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 5) },
                { 3, new Talent(1, 3, 2) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(2, 2, 3) },
                { 3, new Talent(2, 3, 5) },
                { 4, new Talent(2, 4, 3) },
                { 6, new Talent(2, 6, 5) },
                { 7, new Talent(2, 7, 1) },
                { 8, new Talent(2, 8, 3) },
                { 9, new Talent(2, 9, 1) },
                { 11, new Talent(2, 11, 3) },
                { 14, new Talent(2, 14, 1) },
                { 15, new Talent(2, 15, 3) },
                { 16, new Talent(2, 16, 2) },
                { 17, new Talent(2, 17, 3) },
                { 18, new Talent(2, 18, 3) },
                { 19, new Talent(2, 19, 1) },
                { 20, new Talent(2, 20, 3) },
                { 21, new Talent(2, 21, 5) },
                { 23, new Talent(2, 23, 3) },
                { 25, new Talent(2, 25, 3) },
                { 26, new Talent(2, 26, 5) },
                { 27, new Talent(2, 27, 1) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 5) },
                { 7, new Talent(3, 7, 2) },
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

                WowUnit target = (WowUnit)WowInterface.ObjectManager.WowObjects.FirstOrDefault(e => e.Guid == WowInterface.ObjectManager.TargetGuid);

                if (target != null)
                {
                    double distanceToTarget = target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                    // make some distance
                    if ((WowInterface.ObjectManager.Target.Type == WowObjectType.Player && WowInterface.ObjectManager.TargetGuid != 0 && distanceToTarget < 10.0)
                        || (WowInterface.ObjectManager.Target.Type == WowObjectType.Unit && WowInterface.ObjectManager.TargetGuid != 0 && distanceToTarget < 3.0))
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Fleeing, WowInterface.ObjectManager.Target.Position, WowInterface.ObjectManager.Target.Rotation);
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage < 15
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

                        if (WowInterface.ObjectManager.Player.HealthPercentage < 30
                            && TryCastSpell(deterrenceSpell, 0, true))
                        {
                            return;
                        }

                        if (TryCastSpell(raptorStrikeSpell, WowInterface.ObjectManager.TargetGuid, true)
                            || TryCastSpell(mongooseBiteSpell, WowInterface.ObjectManager.TargetGuid, true))
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
                            && TryCastSpell(killShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            return;
                        }

                        TryCastSpell(killCommandSpell, WowInterface.ObjectManager.TargetGuid, true);
                        TryCastSpell(rapidFireSpell, WowInterface.ObjectManager.TargetGuid);

                        if (WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Target.Position, 16.0).Count() > 2
                            && TryCastSpell(multiShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            return;
                        }

                        if ((WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => target.Position.GetDistance(e.Position) < 16).Count() > 2 && TryCastSpell(multiShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                            || TryCastSpell(chimeraShotSpell, WowInterface.ObjectManager.TargetGuid, true)
                            || TryCastSpell(aimedShotSpell, WowInterface.ObjectManager.TargetGuid, true)
                            || TryCastSpell(arcaneShotSpell, WowInterface.ObjectManager.TargetGuid, true)
                            || TryCastSpell(steadyShotSpell, WowInterface.ObjectManager.TargetGuid, true))
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