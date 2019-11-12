using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class DruidBalance : ICombatClass
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

        private readonly int buffCheckTime = 8;
        private readonly int debuffCheckTime = 1;
        private readonly int eclipseCheckTime = 1;

        public DruidBalance(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;

            LunarEclipse = true;
            SolarEclipse = false;
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => false;

        public bool SolarEclipse { get; set; }

        public bool LunarEclipse { get; set; }

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDebuffCheck { get; set; }

        private DateTime LastEclipseCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        public void Execute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                && HandleBuffing())
            {
                return;
            }

            if (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime)
                && HandleDebuffing())
            {
                ;
            }

            if (DateTime.Now - LastEclipseCheck > TimeSpan.FromSeconds(eclipseCheckTime)
                && CheckForEclipseProcs())
            {
                return;
            }

            if (ObjectManager.Player.ManaPercentage < 30
                && CastSpellIfPossible(innervateSpell))
            {
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 70
                && CastSpellIfPossible(barkskinSpell,true))
            {
                return;
            }

            if (CastSpellIfPossible(forceOfNatureSpell,true))
            {
                HookManager.ClickOnTerrain(ObjectManager.Player.Position);
                return;
            }

            if (LunarEclipse
                && CastSpellIfPossible(starfireSpell,true))
            {
                return;
            }

            if (SolarEclipse
                && CastSpellIfPossible(wrathSpell,true))
            {
                return;
            }

            if (CastSpellIfPossible(starfallSpell,true))
            {
                return;
            }
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();
            }
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if (!myBuffs.Any(e => e.Equals(moonkinFormSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(moonkinFormSpell))
            {
                return true;
            }

            if (!myBuffs.Any(e => e.Equals(markOfTheWildSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(markOfTheWildSpell, true))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if (!targetDebuffs.Any(e => e.Equals(faerieFireSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(faerieFireSpell, true))
            {
                return true;
            }

            if (LunarEclipse
                && !targetDebuffs.Any(e => e.Equals(moonfireSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(moonfireSpell, true))
            {
                return true;
            }

            if (SolarEclipse
                && !targetDebuffs.Any(e => e.Equals(insectSwarmSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(insectSwarmSpell, true))
            {
                return true;
            }

            LastDebuffCheck = DateTime.Now;
            return false;
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

        private bool CastSpellIfPossible(string spellname, bool needsMana = false)
        {
            if (IsSpellKnown(spellname)
                && (needsMana && HasEnoughMana(spellname))
                && !IsOnCooldown(spellname))
            {
                HookManager.CastSpell(spellname);
                return true;
            }

            return false;
        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells
            .OrderByDescending(e => e.Rank)
            .FirstOrDefault(e => e.Name.Equals(spellName))
            ?.Costs <= ObjectManager.Player.Mana;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells
            .Any(e => e.Name.Equals(spellName));
    }
}
