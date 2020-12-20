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
using AmeisenBotX.Core.Character.Spells.Objects;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class HunterSurvival : BasicCombatClass
    {
        public HunterSurvival(AmeisenBotStateMachine stateMachine) : base(stateMachine)
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
                if (SpellChain.Run(WowInterface.CharacterManager.SpellBook.IsSpellKnown, out string aspectToUse, aspectOfTheDragonhawkSpell, aspectOfTheHawkSpell))
                {
                    MyAuraManager.BuffsToKeepActive.Add(aspectToUse, () => WowInterface.ObjectManager.Player.ManaPercentage > 50.0 && TryCastSpell(aspectToUse, 0, true));
                }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { huntersMarkSpell, () => TryCastSpell(huntersMarkSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { serpentStingSpell, () => TryCastSpell(serpentStingSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { blackArrowSpell, () => TryCastSpell(blackArrowSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpell(wyvernStingSpell, x.Guid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Survival Hunter spec.";

        public override string Displayname => "Hunter Survival";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIELDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>(),
            Tree2 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(2, 3, 5) },
                { 4, new Talent(2, 4, 3) },
                { 6, new Talent(2, 6, 5) },
                { 7, new Talent(2, 7, 1) },
                { 9, new Talent(2, 9, 1) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 5) },
                { 6, new Talent(3, 6, 3) },
                { 7, new Talent(3, 7, 2) },
                { 8, new Talent(3, 8, 5) },
                { 12, new Talent(3, 12, 3) },
                { 13, new Talent(3, 13, 3) },
                { 14, new Talent(3, 14, 3) },
                { 15, new Talent(3, 15, 3) },
                { 17, new Talent(3, 17, 5) },
                { 18, new Talent(3, 18, 2) },
                { 19, new Talent(3, 19, 3) },
                { 20, new Talent(3, 20, 1) },
                { 21, new Talent(3, 21, 3) },
                { 22, new Talent(3, 22, 4) },
                { 23, new Talent(3, 23, 3) },
                { 25, new Talent(3, 25, 1) },
                { 26, new Talent(3, 26, 3) },
                { 27, new Talent(3, 27, 3) },
                { 28, new Talent(3, 28, 1) },
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

                if (WowInterface.ObjectManager.Target != null)
                {
                    double distanceToTarget = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                    // make some distance
                    if ((WowInterface.ObjectManager.Target.Type == WowObjectType.Player && WowInterface.ObjectManager.TargetGuid != 0 && distanceToTarget < 10.0)
                        || (WowInterface.ObjectManager.Target.Type == WowObjectType.Unit && WowInterface.ObjectManager.TargetGuid != 0 && distanceToTarget < 3.0))
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Fleeing, WowInterface.ObjectManager.Target.Position, WowInterface.ObjectManager.Target.Rotation);
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage < 15.0
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

                        if (WowInterface.ObjectManager.Player.HealthPercentage < 30.0
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
                            && TryCastSpell(concussiveShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            SlowTargetWhenPossible = false;
                            return;
                        }

                        if (WowInterface.ObjectManager.Target.HealthPercentage < 20.0
                            && TryCastSpell(killShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            return;
                        }

                        TryCastSpell(killCommandSpell, WowInterface.ObjectManager.TargetGuid, true);
                        TryCastSpell(rapidFireSpell, 0);

                        if (WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Target.Position, 16.0).Count() > 2
                            && TryCastSpell(multiShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            return;
                        }

                        if ((WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 16.0).Count() > 2 && TryCastSpell(multiShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                            || TryCastSpell(explosiveShotSpell, WowInterface.ObjectManager.TargetGuid, true)
                            || TryCastSpell(aimedShotSpell, WowInterface.ObjectManager.TargetGuid, true)
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