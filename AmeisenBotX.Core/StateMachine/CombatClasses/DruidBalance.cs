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
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();
            }

            if (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime))
            {
                HandleDebuffing();
            }

            if (DateTime.Now - LastEclipseCheck > TimeSpan.FromSeconds(eclipseCheckTime))
            {
                CheckForEclipseProcs();
            }

            if (ObjectManager.Player.ManaPercentage < 30
                && IsSpellKnown(innervateSpell)
                && !IsOnCooldown(innervateSpell))
            {
                HookManager.CastSpell(innervateSpell);
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 70
                && IsSpellKnown(barkskinSpell)
                && HasEnoughMana(barkskinSpell)
                && !IsOnCooldown(barkskinSpell))
            {
                HookManager.CastSpell(barkskinSpell);
                return;
            }

            if (IsSpellKnown(forceOfNatureSpell)
                && HasEnoughMana(forceOfNatureSpell)
                && !IsOnCooldown(forceOfNatureSpell))
            {
                HookManager.CastSpell(forceOfNatureSpell);
                HookManager.ClickOnTerrain(ObjectManager.Player.Position);
                return;
            }

            if (LunarEclipse
                && IsSpellKnown(starfireSpell)
                && HasEnoughMana(starfireSpell)
                && !IsOnCooldown(starfireSpell))
            {
                HookManager.CastSpell(starfireSpell);
                return;
            }

            if (SolarEclipse
                && IsSpellKnown(wrathSpell)
                && HasEnoughMana(wrathSpell)
                && !IsOnCooldown(wrathSpell))
            {
                HookManager.CastSpell(wrathSpell);
                return;
            }

            if (IsSpellKnown(starfallSpell)
                && HasEnoughMana(starfallSpell)
                && !IsOnCooldown(starfallSpell))
            {
                HookManager.CastSpell(starfallSpell);
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

        private void HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if (IsSpellKnown(moonkinFormSpell)
                && !myBuffs.Any(e => e.Equals(moonkinFormSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(moonkinFormSpell))
            {
                HookManager.CastSpell(moonkinFormSpell);
                return;
            }

            if (IsSpellKnown(markOfTheWildSpell)
                && !myBuffs.Any(e => e.Equals(markOfTheWildSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(markOfTheWildSpell))
            {
                HookManager.CastSpell(markOfTheWildSpell);
                return;
            }

            LastBuffCheck = DateTime.Now;
        }

        private void HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if (IsSpellKnown(faerieFireSpell)
                && !targetDebuffs.Any(e => e.Equals(faerieFireSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(faerieFireSpell))
            {
                HookManager.CastSpell(faerieFireSpell);
                return;
            }

            if (LunarEclipse
                && IsSpellKnown(moonfireSpell)
                && HasEnoughMana(moonfireSpell)
                && !targetDebuffs.Any(e => e.Equals(moonfireSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(moonfireSpell))
            {
                HookManager.CastSpell(moonfireSpell);
                return;
            }

            if (SolarEclipse
                && IsSpellKnown(insectSwarmSpell)
                && HasEnoughMana(insectSwarmSpell)
                && !targetDebuffs.Any(e => e.Equals(insectSwarmSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(insectSwarmSpell))
            {
                HookManager.CastSpell(insectSwarmSpell);
                return;
            }

            LastDebuffCheck = DateTime.Now;
        }

        private void CheckForEclipseProcs()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (myBuffs.Any(e => e.Equals(eclipseLunarSpell, StringComparison.OrdinalIgnoreCase)))
            {
                SolarEclipse = false;
                LunarEclipse = true;
                return;
            }

            if (myBuffs.Any(e => e.Equals(eclipseSolarSpell, StringComparison.OrdinalIgnoreCase)))
            {
                SolarEclipse = true;
                LunarEclipse = false;
                return;
            }

            LastEclipseCheck = DateTime.Now;
        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells.OrderByDescending(e => e.Rank).FirstOrDefault(e => e.Name.Equals(spellName))?.Costs <= ObjectManager.Player.Mana;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells.Any(e => e.Name.Equals(spellName));
    }
}
