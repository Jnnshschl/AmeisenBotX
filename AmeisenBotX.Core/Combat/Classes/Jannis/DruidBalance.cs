using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Utils.InterruptManager;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class DruidBalance : BasicCombatClass
    {
        public DruidBalance(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(moonkinFormSpell, () => TryCastSpell(moonkinFormSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(thornsSpell, () => TryCastSpell(thornsSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(markOfTheWildSpell, () => TryCastSpell(markOfTheWildSpell, WowInterface.ObjectManager.PlayerGuid, true, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(moonfireSpell, () => LunarEclipse && TryCastSpell(moonfireSpell, WowInterface.ObjectManager.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(insectSwarmSpell, () => SolarEclipse && TryCastSpell(insectSwarmSpell, WowInterface.ObjectManager.TargetGuid, true)));

            InterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpell(faerieFireSpell, x.Guid, true) },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((markOfTheWildSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));

            SolarEclipse = false;
            LunarEclipse = true;
        }

        public override string Description => "FCFS based CombatClass for the Balance (Owl) Druid spec.";

        public override string Displayname => "Druid Balance";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<WowArmorType>() { WowArmorType.SHIELDS }, new List<WowWeaponType>() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public DateTime LastEclipseCheck { get; private set; }

        public bool LunarEclipse { get; set; }

        public override WowRole Role => WowRole.Dps;

        public bool SolarEclipse { get; set; }

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 5) },
                { 3, new Talent(1, 3, 1) },
                { 4, new Talent(1, 4, 2) },
                { 5, new Talent(1, 5, 2) },
                { 7, new Talent(1, 7, 3) },
                { 8, new Talent(1, 8, 1) },
                { 9, new Talent(1, 9, 2) },
                { 10, new Talent(1, 10, 5) },
                { 11, new Talent(1, 11, 3) },
                { 12, new Talent(1, 12, 3) },
                { 13, new Talent(1, 13, 1) },
                { 16, new Talent(1, 16, 3) },
                { 17, new Talent(1, 17, 2) },
                { 18, new Talent(1, 18, 1) },
                { 19, new Talent(1, 19, 3) },
                { 20, new Talent(1, 20, 3) },
                { 22, new Talent(1, 22, 5) },
                { 23, new Talent(1, 23, 3) },
                { 25, new Talent(1, 25, 1) },
                { 26, new Talent(1, 26, 2) },
                { 27, new Talent(1, 27, 3) },
                { 28, new Talent(1, 28, 1) },
            },
            Tree2 = new Dictionary<int, Talent>(),
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 2) },
                { 3, new Talent(3, 3, 5) },
                { 6, new Talent(3, 6, 3) },
                { 7, new Talent(3, 7, 3) },
                { 8, new Talent(3, 8, 1) },
                { 9, new Talent(3, 9, 2) },
            },
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Druid;

        public override void Execute()
        {
            base.Execute();

            CheckForEclipseProcs();

            if (SelectTarget(TargetManagerDps))
            {
                if (TryCastSpell(naturesGraspSpell, 0))
                {
                    return;
                }

                double distance = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                if (distance < 12.0
                    && WowInterface.ObjectManager.Target.HasBuffByName(entanglingRootsSpell)
                    && TryCastSpellDk(entanglingRootsSpell, WowInterface.ObjectManager.TargetGuid, false, false, true))
                {
                    return;
                }

                if (NeedToHealMySelf())
                {
                    return;
                }

                if ((WowInterface.ObjectManager.Player.ManaPercentage < 30
                        && TryCastSpell(innervateSpell, 0))
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 70
                        && TryCastSpell(barkskinSpell, 0, true))
                    || (LunarEclipse
                        && TryCastSpell(starfireSpell, WowInterface.ObjectManager.TargetGuid, true))
                    || (SolarEclipse
                        && TryCastSpell(wrathSpell, WowInterface.ObjectManager.TargetGuid, true))
                    || (WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => !e.IsInCombat && WowInterface.ObjectManager.Player.Position.GetDistance(e.Position) < 35).Count() < 4
                        && TryCastSpell(starfallSpell, WowInterface.ObjectManager.TargetGuid, true)))
                {
                    return;
                }

                if (TryCastSpell(forceOfNatureSpell, 0, true))
                {
                    WowInterface.HookManager.WowClickOnTerrain(WowInterface.ObjectManager.Player.Position);
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealMySelf())
            {
                return;
            }
        }

        private bool CheckForEclipseProcs()
        {
            if (WowInterface.ObjectManager.Player.HasBuffByName(eclipseLunarSpell))
            {
                SolarEclipse = false;
                LunarEclipse = true;
            }
            else if (WowInterface.ObjectManager.Player.HasBuffByName(eclipseSolarSpell))
            {
                SolarEclipse = true;
                LunarEclipse = false;
            }

            LastEclipseCheck = DateTime.Now;
            return false;
        }

        private bool NeedToHealMySelf()
        {
            if (WowInterface.ObjectManager.Player.HealthPercentage < 60
                && !WowInterface.ObjectManager.Player.HasBuffByName(rejuvenationSpell)
                && TryCastSpell(rejuvenationSpell, 0, true))
            {
                return true;
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 40
                && TryCastSpell(healingTouchSpell, 0, true))
            {
                return true;
            }

            return false;
        }
    }
}