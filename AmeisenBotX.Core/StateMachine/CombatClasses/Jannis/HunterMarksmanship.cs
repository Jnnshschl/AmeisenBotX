using AmeisenBotX.Core.Character.Comparators;
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

        private readonly string aimedShotSpell = "Aimed Shot";
        private readonly string arcaneShotSpell = "Arcane Shot";
        private readonly string aspectOfTheDragonhawkSpell = "Aspect of the Dragonhawk";
        private readonly string callPetSpell = "Call Pet";
        private readonly string chimeraShotSpell = "Chimera Shot";
        private readonly string concussiveShotSpell = "Concussive Shot";
        private readonly string deterrenceSpell = "Deterrence";
        private readonly string disengageSpell = "Disengage";
        private readonly string feignDeathSpell = "Feign Death";
        private readonly string frostTrapSpell = "Frost Trap";
        private readonly string huntersMarkSpell = "Hunter's Mark";
        private readonly string killCommandSpell = "Kill Command";
        private readonly string killShotSpell = "Kill Shot";
        private readonly string mendPetSpell = "Mend Pet";
        private readonly string mongooseBiteSpell = "Mongoose Bite";
        private readonly string multiShotSpell = "Multi-Shot";
        private readonly string rapidFireSpell = "Rapid Fire";
        private readonly string raptorStrikeSpell = "Raptor Strike";
        private readonly string revivePetSpell = "Revive Pet";
        private readonly string serpentStingSpell = "Serpent Sting";
        private readonly string silencingShotSpell = "Silencing Shot";
        private readonly string steadyShotSpell = "Steady Shot";

        public HunterMarksmanship(WowInterface wowInterface) : base(wowInterface)
        {
            PetManager = new PetManager(
                   WowInterface.ObjectManager.Pet,
                   TimeSpan.FromSeconds(15),
                   () => CastSpellIfPossible(mendPetSpell, true),
                   () => CastSpellIfPossible(callPetSpell),
                   () => CastSpellIfPossible(revivePetSpell));

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { aspectOfTheDragonhawkSpell, () => CastSpellIfPossible(aspectOfTheDragonhawkSpell, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { huntersMarkSpell, () => CastSpellIfPossible(huntersMarkSpell, true) },
                { serpentStingSpell, () => CastSpellIfPossible(serpentStingSpell, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(silencingShotSpell, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Hunter;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Marksmanship Hunter spec.";

        public override string Displayname => "Hunter Marksmanship";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator();

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private bool DisengagePrepared { get; set; } = false;

        private bool InFrostTrapCombo { get; set; } = false;

        private PetManager PetManager { get; set; }

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (WowInterface.ObjectManager.Player.IsCasting
                || WowInterface.ObjectManager.TargetGuid == WowInterface.ObjectManager.PlayerGuid)
            {
                return;
            }

            if (!WowInterface.ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
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

                if (WowInterface.ObjectManager.Player.HealthPercentage < 15
                    && CastSpellIfPossible(feignDeathSpell))
                {
                    return;
                }

                if (distanceToTarget < 3)
                {
                    if (CastSpellIfPossible(frostTrapSpell, true))
                    {
                        InFrostTrapCombo = true;
                        DisengagePrepared = true;
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage < 30
                        && CastSpellIfPossible(deterrenceSpell, true))
                    {
                        return;
                    }

                    if (CastSpellIfPossible(raptorStrikeSpell, true)
                        || CastSpellIfPossible(mongooseBiteSpell, true))
                    {
                        return;
                    }
                }
                else
                {
                    if (DisengagePrepared
                        && CastSpellIfPossible(concussiveShotSpell, true))
                    {
                        DisengagePrepared = false;
                        return;
                    }

                    if (InFrostTrapCombo
                        && CastSpellIfPossible(disengageSpell, true))
                    {
                        InFrostTrapCombo = false;
                        return;
                    }

                    if (target.HealthPercentage < 20
                        && CastSpellIfPossible(killShotSpell, true))
                    {
                        return;
                    }

                    CastSpellIfPossible(killCommandSpell, true);
                    CastSpellIfPossible(rapidFireSpell);

                    if ((WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => target.Position.GetDistance(e.Position) < 16).Count() > 2 && CastSpellIfPossible(multiShotSpell, true))
                        || CastSpellIfPossible(chimeraShotSpell, true)
                        || CastSpellIfPossible(aimedShotSpell, true)
                        || CastSpellIfPossible(arcaneShotSpell, true)
                        || CastSpellIfPossible(steadyShotSpell, true))
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

            DisengagePrepared = false;
        }
    }
}