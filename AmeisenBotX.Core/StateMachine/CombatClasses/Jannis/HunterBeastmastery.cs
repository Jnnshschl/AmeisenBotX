using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
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
                TimeSpan.FromSeconds(15),
                () => CastSpellIfPossible(mendPetSpell, 0, true),
                () => CastSpellIfPossible(callPetSpell, 0),
                () => CastSpellIfPossible(revivePetSpell, 0));

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { aspectOfTheDragonhawkSpell, () => CastSpellIfPossible(aspectOfTheDragonhawkSpell, 0, true) }
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

        public override WowClass Class => WowClass.Hunter;

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
                { 2, new Talent(1, 2, 5) },
                { 3, new Talent(1, 3, 3) },
                { 4, new Talent(1, 4, 2) },
                { 8, new Talent(1, 8, 1) },
            },
            Tree2 = new Dictionary<int, Talent>(),
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 2) },
                { 2, new Talent(3, 2, 3) },
                { 5, new Talent(3, 5, 3) },
                { 6, new Talent(3, 6, 3) },
                { 7, new Talent(3, 7, 3) },
                { 8, new Talent(3, 8, 1) },
                { 9, new Talent(3, 9, 2) },
                { 10, new Talent(3, 10, 1) },
                { 11, new Talent(3, 11, 3) },
                { 12, new Talent(3, 12, 1) },
                { 13, new Talent(3, 13, 5) },
                { 16, new Talent(3, 16, 5) },
                { 17, new Talent(3, 17, 3) },
                { 18, new Talent(3, 18, 1) },
                { 19, new Talent(3, 19, 3) },
                { 20, new Talent(3, 20, 5) },
                { 21, new Talent(3, 21, 3) },
                { 22, new Talent(3, 22, 3) },
                { 23, new Talent(3, 23, 1) },
                { 24, new Talent(3, 24, 3) },
                { 26, new Talent(3, 26, 5) },
                { 27, new Talent(3, 27, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        private PetManager PetManager { get; set; }

        private bool ReadyToDisengage { get; set; } = false;

        private bool SlowTargetWhenPossible { get; set; } = false;

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run())
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if (PetManager.Tick()) { return; }

            if (WowInterface.ObjectManager.Target != null)
            {
                double distanceToTarget = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                // make some distance
                if (WowInterface.ObjectManager.TargetGuid != 0 && distanceToTarget < 10.0)
                {
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Fleeing, WowInterface.ObjectManager.Target.Position, WowInterface.ObjectManager.Target.Rotation);
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

                    if ((WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 16).Count() > 2 && CastSpellIfPossible(multiShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || CastSpellIfPossible(arcaneShotSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || CastSpellIfPossible(steadyShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return;
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