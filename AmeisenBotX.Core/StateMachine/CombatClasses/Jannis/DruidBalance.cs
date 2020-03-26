using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class DruidBalance : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string barkskinSpell = "Barkskin";
        private readonly int eclipseCheckTime = 1;
        private readonly string eclipseLunarSpell = "Eclipse (Lunar)";
        private readonly string eclipseSolarSpell = "Eclipse (Solar)";
        private readonly string faerieFireSpell = "Faerie Fire";
        private readonly string forceOfNatureSpell = "Force of Nature";
        private readonly string innervateSpell = "Innervate";
        private readonly string insectSwarmSpell = "Insect Swarm";
        private readonly string markOfTheWildSpell = "Mark of the Wild";
        private readonly string moonfireSpell = "Moonfire";
        private readonly string moonkinFormSpell = "Moonkin Form";
        private readonly string starfallSpell = "Starfall";
        private readonly string starfireSpell = "Starfire";
        private readonly string wrathSpell = "Wrath";

        public DruidBalance(WowInterface wowInterface) : base(wowInterface)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { moonkinFormSpell, () => CastSpellIfPossible(moonkinFormSpell,0, true) },
                { markOfTheWildSpell, () => CastSpellIfPossible(markOfTheWildSpell, WowInterface.ObjectManager.PlayerGuid, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { moonfireSpell, () => LunarEclipse && CastSpellIfPossible(moonfireSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { insectSwarmSpell, () => SolarEclipse && CastSpellIfPossible(insectSwarmSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(faerieFireSpell, WowInterface.ObjectManager.TargetGuid, true) },
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Druid;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Balance (Owl) Druid spec.";

        public override string Displayname => "Druid Balance";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIEDLS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public DateTime LastEclipseCheck { get; private set; }

        public bool LunarEclipse { get; set; }

        public override CombatClassRole Role => CombatClassRole.Dps;

        public bool SolarEclipse { get; set; }

        public override string Version => "1.0";

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || (DateTime.Now - LastEclipseCheck > TimeSpan.FromSeconds(eclipseCheckTime)
                    && CheckForEclipseProcs())
                || (WowInterface.ObjectManager.Player.ManaPercentage < 30
                    && CastSpellIfPossible(innervateSpell, 0))
                || (WowInterface.ObjectManager.Player.HealthPercentage < 70
                    && CastSpellIfPossible(barkskinSpell, 0, true))
                || (LunarEclipse
                    && CastSpellIfPossible(starfireSpell, WowInterface.ObjectManager.TargetGuid, true))
                || (SolarEclipse
                    && CastSpellIfPossible(wrathSpell, WowInterface.ObjectManager.TargetGuid, true))
                || (WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => !e.IsInCombat && WowInterface.ObjectManager.Player.Position.GetDistance(e.Position) < 35).Count() < 4
                    && CastSpellIfPossible(starfallSpell, WowInterface.ObjectManager.TargetGuid, true)))
            {
                return;
            }

            if (CastSpellIfPossible(forceOfNatureSpell, 0, true))
            {
                WowInterface.HookManager.ClickOnTerrain(WowInterface.ObjectManager.Player.Position);
            }
        }

        public override void OutOfCombatExecute()
        {
            MyAuraManager.Tick();
        }

        private bool CheckForEclipseProcs()
        {
            List<string> myBuffs = WowInterface.ObjectManager.Player.Auras.Select(e => e.Name).ToList();

            if (myBuffs.Any(e => e.Equals(eclipseLunarSpell, StringComparison.OrdinalIgnoreCase)))
            {
                SolarEclipse = false;
                LunarEclipse = true;
            }
            else if (myBuffs.Any(e => e.Equals(eclipseSolarSpell, StringComparison.OrdinalIgnoreCase)))
            {
                SolarEclipse = true;
                LunarEclipse = false;
            }

            LastEclipseCheck = DateTime.Now;
            return false;
        }
    }
}