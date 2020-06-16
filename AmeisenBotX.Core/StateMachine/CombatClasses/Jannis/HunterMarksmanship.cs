using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
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
    public class HunterMarksmanship : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string aimedShotSpell = "Aimed Shot";
        private const string arcaneShotSpell = "Arcane Shot";
        private const string aspectOfTheDragonhawkSpell = "Aspect of the Dragonhawk";
        private const string callPetSpell = "Call Pet";
        private const string chimeraShotSpell = "Chimera Shot";
        private const string concussiveShotSpell = "Concussive Shot";
        private const string deterrenceSpell = "Deterrence";
        private const string disengageSpell = "Disengage";
        private const string feignDeathSpell = "Feign Death";
        private const string frostTrapSpell = "Frost Trap";
        private const string huntersMarkSpell = "Hunter's Mark";
        private const string killCommandSpell = "Kill Command";
        private const string killShotSpell = "Kill Shot";
        private const string mendPetSpell = "Mend Pet";
        private const string mongooseBiteSpell = "Mongoose Bite";
        private const string multiShotSpell = "Multi-Shot";
        private const string rapidFireSpell = "Rapid Fire";
        private const string raptorStrikeSpell = "Raptor Strike";
        private const string revivePetSpell = "Revive Pet";
        private const string serpentStingSpell = "Serpent Sting";
        private const string silencingShotSpell = "Silencing Shot";
        private const string steadyShotSpell = "Steady Shot";
#pragma warning restore IDE0051

        public HunterMarksmanship(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            PetManager = new PetManager(
                   WowInterface,
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
                { 0, (x) => CastSpellIfPossible(silencingShotSpell, x.Guid, true) }
            };

            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(4000));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Hunter;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Marksmanship Hunter spec.";

        public override string Displayname => "Hunter Marksmanship";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIEDLS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private bool ReadyToDisengage { get; set; } = false;

        private bool SlowTargetWhenPossible { get; set; } = false;

        private PetManager PetManager { get; set; }

        private TimegatedEvent AutoAttackEvent { get; set; }

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run())
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || PetManager.Tick())
            {
                return;
            }

            WowUnit target = (WowUnit)WowInterface.ObjectManager.WowObjects.FirstOrDefault(e => e.Guid == WowInterface.ObjectManager.TargetGuid);

            if (target != null)
            {
                double distanceToTarget = target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

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
                        && CastSpellIfPossible(disengageSpell, 0, true))
                    {
                        SlowTargetWhenPossible = false;
                        return;
                    }

                    if (target.HealthPercentage < 20
                        && CastSpellIfPossible(killShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return;
                    }

                    CastSpellIfPossible(killCommandSpell, WowInterface.ObjectManager.TargetGuid, true);
                    CastSpellIfPossible(rapidFireSpell, WowInterface.ObjectManager.TargetGuid);

                    if ((WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => target.Position.GetDistance(e.Position) < 16).Count() > 2 && CastSpellIfPossible(multiShotSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || CastSpellIfPossible(chimeraShotSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || CastSpellIfPossible(aimedShotSpell, WowInterface.ObjectManager.TargetGuid, true)
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