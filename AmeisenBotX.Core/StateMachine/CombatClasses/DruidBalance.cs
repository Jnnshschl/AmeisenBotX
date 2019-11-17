using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.CombatClasses.Utils;
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
            CooldownManager = new CooldownManager(characterManager.SpellBook.Spells);

            LunarEclipse = true;
            SolarEclipse = false;

            Spells = new Dictionary<string, Spell>();
            CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                Spells.Clear();
                foreach (Spell spell in CharacterManager.SpellBook.Spells)
                {
                    Spells.Add(spell.Name, spell);
                }
            };
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

        private CooldownManager CooldownManager { get; }

        private Dictionary<string, Spell> Spells { get; }

        public void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.CurrentlyCastingSpellId > 0
                || ObjectManager.Player.CurrentlyChannelingSpellId > 0)
            {
                return;
            }

            if (CastSpellIfPossible(forceOfNatureSpell, true))
            {
                HookManager.ClickOnTerrain(ObjectManager.Player.Position);
            }

            if ((DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                    && HandleBuffing())
                || (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime)
                    && HandleDebuffing())
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
                || CastSpellIfPossible(starfallSpell, true))
            {
                return;
            }
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                && HandleBuffing())
            {
                return;
            }
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if ((!myBuffs.Any(e => e.Equals(moonkinFormSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(moonkinFormSpell))
                || (!myBuffs.Any(e => e.Equals(markOfTheWildSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(markOfTheWildSpell, true)))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if ((!targetDebuffs.Any(e => e.Equals(faerieFireSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(faerieFireSpell, true))
                || (LunarEclipse
                    && !targetDebuffs.Any(e => e.Equals(moonfireSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(moonfireSpell, true))
                || (SolarEclipse
                    && !targetDebuffs.Any(e => e.Equals(insectSwarmSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(insectSwarmSpell, true)))
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

        private bool CastSpellIfPossible(string spellName, bool needsMana = false)
        {
            if (!Spells.ContainsKey(spellName))
            {
                Spells.Add(spellName, CharacterManager.SpellBook.GetSpellByName(spellName));
            }

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsMana || Spells[spellName].Costs < ObjectManager.Player.Mana))
            {
                HookManager.CastSpell(spellName);
                CooldownManager.SetSpellCooldown(spellName, (int)HookManager.GetSpellCooldown(spellName));
                return true;
            }

            return false;
        }
    }
}
