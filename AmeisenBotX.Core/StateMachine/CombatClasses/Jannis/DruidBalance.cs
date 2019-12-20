using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;
using static AmeisenBotX.Core.StateMachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class DruidBalance : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string moonkinFormSpell = "Moonkin Form";
        private readonly string markOfTheWildSpell = "Mark of the Wild";
        private readonly string innervateSpell = "Innervate";
        private readonly string moonfireSpell = "Moonfire";
        private readonly string insectSwarmSpell = "Insect Swarm";
        private readonly string starfallSpell = "Starfall";
        private readonly string forceOfNatureSpell = "Force of Nature";
        private readonly string wrathSpell = "Wrath";
        private readonly string starfireSpell = "Starfire";
        private readonly string faerieFireSpell = "Faerie Fire";
        private readonly string eclipseSolarSpell = "Eclipse (Solar)";
        private readonly string eclipseLunarSpell = "Eclipse (Lunar)";
        private readonly string barkskinSpell = "Barkskin";

        private readonly int eclipseCheckTime = 1;

        public DruidBalance(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager) : base(objectManager, characterManager, hookManager)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { moonkinFormSpell, () => CastSpellIfPossible(moonkinFormSpell, true) },
                { markOfTheWildSpell, () => 
                    {
                        HookManager.TargetGuid(ObjectManager.PlayerGuid);
                        return CastSpellIfPossible(markOfTheWildSpell, true); 
                    } 
                }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { moonfireSpell, () => LunarEclipse && CastSpellIfPossible(moonfireSpell, true) },
                { insectSwarmSpell, () => SolarEclipse && CastSpellIfPossible(insectSwarmSpell, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(faerieFireSpell, true) },
            };
        }

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator();

        public bool SolarEclipse { get; set; }

        public bool LunarEclipse { get; set; }

        public override string Displayname => "Druid Balance";

        public override string Version => "1.0";

        public override string Author => "Jannis";

        public override string Description => "FCFS based CombatClass for the Balance (Owl) Druid spec.";

        public override WowClass Class => WowClass.Druid;

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public DateTime LastEclipseCheck { get; private set; }

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || (DateTime.Now - LastEclipseCheck > TimeSpan.FromSeconds(eclipseCheckTime)
                    && CheckForEclipseProcs())
                || (ObjectManager.Player.ManaPercentage < 30
                    && CastSpellIfPossible(innervateSpell))
                || (ObjectManager.Player.HealthPercentage < 70
                    && CastSpellIfPossible(barkskinSpell, true))
                || (LunarEclipse
                    && CastSpellIfPossible(starfireSpell, true))
                || (SolarEclipse
                    && CastSpellIfPossible(wrathSpell, true))
                || (ObjectManager.WowObjects.OfType<WowUnit>().Where(e => !e.IsInCombat && ObjectManager.Player.Position.GetDistance(e.Position) < 35).Count() < 4
                    && CastSpellIfPossible(starfallSpell, true)))
            {
                return;
            }

            if (CastSpellIfPossible(forceOfNatureSpell, true))
            {
                HookManager.ClickOnTerrain(ObjectManager.Player.Position);
            }
        }

        public override void OutOfCombatExecute()
        {
            MyAuraManager.Tick();
        }

        private bool CheckForEclipseProcs()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

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
