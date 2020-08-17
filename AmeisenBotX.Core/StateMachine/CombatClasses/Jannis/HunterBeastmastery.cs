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
    public class HunterBeastmastery : BasicCombatClass
    {
        public HunterBeastmastery(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            PetManager = new PetManager(WowInterface,
                TimeSpan.FromSeconds(5),
                () => CastSpellIfPossible(mendPetSpell, 0, true),
                () => CastSpellIfPossible(callPetSpell, 0),
                () => CastSpellIfPossible(revivePetSpell, 0));

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { aspectOfTheDragonhawkSpell, () => WowInterface.ObjectManager.Player.ManaPercentage > 50.0 && CastSpellIfPossible(aspectOfTheDragonhawkSpell, 0, true) },
                { aspectOfTheHawkSpell, () => !WowInterface.CharacterManager.SpellBook.IsSpellKnown(aspectOfTheDragonhawkSpell) && WowInterface.ObjectManager.Player.ManaPercentage > 50.0 && CastSpellIfPossible(aspectOfTheHawkSpell, 0, true) },
                { aspectOfTheViperSpell, () => WowInterface.ObjectManager.Player.ManaPercentage < 20.0 && CastSpellIfPossible(aspectOfTheViperSpell, 0, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { huntersMarkSpell, () => CastSpellIfPossible(huntersMarkSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { serpentStingSpell, () => CastSpellIfPossible(serpentStingSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossible(scatterShotSpell, x.Guid, true) },
                { 1, (x) => CastSpellIfPossible(intimidationSpell, x.Guid, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass WowClass => WowClass.Hunter;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Beastmastery Hunter spec.";

        public override string Displayname => "Hunter Beastmastery";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIELDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 5) },
                { 2, new Talent(1, 2, 1) },
                { 3, new Talent(1, 3, 2) },
                { 6, new Talent(1, 6, 2) },
                { 8, new Talent(1, 8, 1) },
                { 9, new Talent(1, 9, 5) },
                { 11, new Talent(1, 11, 5) },
                { 12, new Talent(1, 12, 1) },
                { 13, new Talent(1, 13, 1) },
                { 14, new Talent(1, 14, 2) },
                { 15, new Talent(1, 15, 2) },
                { 16, new Talent(1, 16, 5) },
                { 17, new Talent(1, 17, 3) },
                { 18, new Talent(1, 18, 1) },
                { 21, new Talent(1, 21, 5) },
                { 22, new Talent(1, 22, 3) },
                { 23, new Talent(1, 23, 1) },
                { 24, new Talent(1, 24, 3) },
                { 25, new Talent(1, 25, 5) },
                { 26, new Talent(1, 26, 1) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(1, 2, 1) },
                { 3, new Talent(1, 3, 5) },
                { 4, new Talent(1, 4, 3) },
                { 6, new Talent(1, 6, 5) },
                { 8, new Talent(1, 8, 3) },
            },
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        private PetManager PetManager { get; set; }

        private bool ReadyToDisengage { get; set; } = false;

        private bool SlowTargetWhenPossible { get; set; } = false;

        public override void ExecuteCC()
        {
            if (SelectTarget(DpsTargetManager))
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

                    if (WowInterface.ObjectManager.Player.HealthPercentage < 15
                        && CastSpellIfPossible(feignDeathSpell, 0))
                    {
                        return;
                    }

                    if (distanceToTarget < 5.0)
                    {
                        if (ReadyToDisengage
                            && CastSpellIfPossible(disengageSpell, 0, true))
                        {
                            ReadyToDisengage = false;
                            SlowTargetWhenPossible = true;
                            return;
                        }

                        if (CastSpellIfPossible(frostTrapSpell, 0, true))
                        {
                            ReadyToDisengage = true;
                            SlowTargetWhenPossible = true;
                            return;
                        }

                        if (WowInterface.ObjectManager.Player.HealthPercentage < 30
                            && CastSpellIfPossible(deterrenceSpell, 0, true))
                        {
                            return;
                        }

                        if (CastSpellIfPossible(raptorStrikeSpell, WowInterface.ObjectManager.TargetGuid, true)
                            || CastSpellIfPossible(mongooseBiteSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (SlowTargetWhenPossible
                            && CastSpellIfPossible(concussiveShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            SlowTargetWhenPossible = false;
                            return;
                        }

                        if (WowInterface.ObjectManager.Target.HealthPercentage < 20
                            && CastSpellIfPossible(killShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            return;
                        }

                        CastSpellIfPossible(killCommandSpell, WowInterface.ObjectManager.TargetGuid, true);
                        CastSpellIfPossible(beastialWrathSpell, WowInterface.ObjectManager.TargetGuid, true);
                        CastSpellIfPossible(rapidFireSpell, 0);

                        if (WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Target.Position, 16.0).Count() > 2
                            && CastSpellIfPossible(multiShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            return;
                        }

                        if (CastSpellIfPossible(arcaneShotSpell, WowInterface.ObjectManager.TargetGuid, true)
                            || CastSpellIfPossible(steadyShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            return;
                        }
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || PetManager.Tick())
            {
                return;
            }

            ReadyToDisengage = false;
            SlowTargetWhenPossible = false;
        }
    }
}