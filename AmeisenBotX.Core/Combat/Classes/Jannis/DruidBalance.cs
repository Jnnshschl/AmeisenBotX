using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class DruidBalance : BasicCombatClass
    {
        public DruidBalance(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(moonkinFormSpell, () => TryCastSpell(moonkinFormSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(thornsSpell, () => TryCastSpell(thornsSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(markOfTheWildSpell, () => TryCastSpell(markOfTheWildSpell, WowInterface.Player.Guid, true, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(moonfireSpell, () => LunarEclipse && TryCastSpell(moonfireSpell, WowInterface.Target.Guid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(insectSwarmSpell, () => SolarEclipse && TryCastSpell(insectSwarmSpell, WowInterface.Target.Guid, true)));

            InterruptManager.InterruptSpells = new()
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

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public DateTime LastEclipseCheck { get; private set; }

        public bool LunarEclipse { get; set; }

        public override WowRole Role => WowRole.Dps;

        public bool SolarEclipse { get; set; }

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 5) },
                { 3, new(1, 3, 1) },
                { 4, new(1, 4, 2) },
                { 5, new(1, 5, 2) },
                { 7, new(1, 7, 3) },
                { 8, new(1, 8, 1) },
                { 9, new(1, 9, 2) },
                { 10, new(1, 10, 5) },
                { 11, new(1, 11, 3) },
                { 12, new(1, 12, 3) },
                { 13, new(1, 13, 1) },
                { 16, new(1, 16, 3) },
                { 17, new(1, 17, 2) },
                { 18, new(1, 18, 1) },
                { 19, new(1, 19, 3) },
                { 20, new(1, 20, 3) },
                { 22, new(1, 22, 5) },
                { 23, new(1, 23, 3) },
                { 25, new(1, 25, 1) },
                { 26, new(1, 26, 2) },
                { 27, new(1, 27, 3) },
                { 28, new(1, 28, 1) },
            },
            Tree2 = new(),
            Tree3 = new()
            {
                { 1, new(3, 1, 2) },
                { 3, new(3, 3, 5) },
                { 6, new(3, 6, 3) },
                { 7, new(3, 7, 3) },
                { 8, new(3, 8, 1) },
                { 9, new(3, 9, 2) },
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

            if (SelectTarget(TargetProviderDps))
            {
                if (TryCastSpell(naturesGraspSpell, 0))
                {
                    return;
                }

                double distance = WowInterface.Target.Position.GetDistance(WowInterface.Player.Position);

                if (distance < 12.0
                    && WowInterface.Target.HasBuffByName(entanglingRootsSpell)
                    && TryCastSpellDk(entanglingRootsSpell, WowInterface.Target.Guid, false, false, true))
                {
                    return;
                }

                if (NeedToHealMySelf())
                {
                    return;
                }

                if ((WowInterface.Player.ManaPercentage < 30
                        && TryCastSpell(innervateSpell, 0))
                    || (WowInterface.Player.HealthPercentage < 70
                        && TryCastSpell(barkskinSpell, 0, true))
                    || (LunarEclipse
                        && TryCastSpell(starfireSpell, WowInterface.Target.Guid, true))
                    || (SolarEclipse
                        && TryCastSpell(wrathSpell, WowInterface.Target.Guid, true))
                    || (WowInterface.Objects.WowObjects.OfType<WowUnit>().Where(e => !e.IsInCombat && WowInterface.Player.Position.GetDistance(e.Position) < 35).Count() < 4
                        && TryCastSpell(starfallSpell, WowInterface.Target.Guid, true)))
                {
                    return;
                }

                if (TryCastSpell(forceOfNatureSpell, 0, true))
                {
                    WowInterface.NewWowInterface.WowClickOnTerrain(WowInterface.Player.Position);
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
            if (WowInterface.Player.HasBuffByName(eclipseLunarSpell))
            {
                SolarEclipse = false;
                LunarEclipse = true;
            }
            else if (WowInterface.Player.HasBuffByName(eclipseSolarSpell))
            {
                SolarEclipse = true;
                LunarEclipse = false;
            }

            LastEclipseCheck = DateTime.Now;
            return false;
        }

        private bool NeedToHealMySelf()
        {
            if (WowInterface.Player.HealthPercentage < 60
                && !WowInterface.Player.HasBuffByName(rejuvenationSpell)
                && TryCastSpell(rejuvenationSpell, 0, true))
            {
                return true;
            }

            if (WowInterface.Player.HealthPercentage < 40
                && TryCastSpell(healingTouchSpell, 0, true))
            {
                return true;
            }

            return false;
        }
    }
}