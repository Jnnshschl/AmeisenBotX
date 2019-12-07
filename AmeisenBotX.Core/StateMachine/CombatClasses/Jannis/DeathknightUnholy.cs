using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Spells;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class DeathknightUnholy : ICombatClass
    {
        // author: Jannis Höschele

        private readonly string unholyPresenceSpell = "Unholy Presence";
        private readonly string icyTouchSpell = "Icy Touch";
        private readonly string scourgeStrikeSpell = "Scourge Strike";
        private readonly string bloodStrikeSpell = "Blood Strike";
        private readonly string plagueStrikeSpell = "Plague Strike";
        private readonly string runeStrikeSpell = "Rune Strike";
        private readonly string strangulateSpell = "Strangulate";
        private readonly string mindFreezeSpell = "Mind Freeze";
        private readonly string summonGargoyleSpell = "Summon Gargoyle";
        private readonly string frostFeverSpell = "Frost Fever";
        private readonly string bloodPlagueSpell = "Blood Plague";
        private readonly string deathCoilSpell = "Death Coil";
        private readonly string hornOfWinterSpell = "Horn of Winter";
        private readonly string iceboundFortitudeSpell = "Icebound Fortitude";
        private readonly string armyOfTheDeadSpell = "Army of the Dead";

        private readonly int buffCheckTime = 4;
        private readonly int deBuffCheckTime = 4;
        private readonly int enemyCastingCheckTime = 1;

        public DeathknightUnholy(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            CooldownManager = new CooldownManager(characterManager.SpellBook.Spells);

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

        public bool IsMelee => true;

        public IWowItemComparator ItemComparator { get; } = new BasicStrengthComparator();

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDebuffCheck { get; set; }

        private DateTime LastEnemyCastingCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private CooldownManager CooldownManager { get; }

        private Dictionary<string, Spell> Spells { get; }

        public string Displayname => "Deathknight Unholy";

        public string Version => "1.0";

        public string Author => "Jannis";

        public string Description => "FCFS based CombatClass for the Unholy Deathknight spec.";

        public WowClass Class => WowClass.Deathknight;

        public CombatClassRole Role => CombatClassRole.Dps;

        public Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (!ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
            }

            if ((DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                    && HandleBuffing())
                || (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(deBuffCheckTime)
                    && HandleDebuffing())
                || (DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime)
                    && HandleEnemyCasting())
                || (ObjectManager.Player.HealthPercentage < 60
                    && CastSpellIfPossible(iceboundFortitudeSpell, true))
                || CastSpellIfPossible(bloodStrikeSpell, false, true)
                || CastSpellIfPossible(scourgeStrikeSpell, false, false, true, true)
                || CastSpellIfPossible(deathCoilSpell, true)
                || CastSpellIfPossible(summonGargoyleSpell, true)
                || (ObjectManager.Player.Runeenergy > 60
                    && CastSpellIfPossible(runeStrikeSpell)))
            {
                return;
            }
        }

        private bool HandleEnemyCasting()
        {
            (string, int) castinInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);

            bool isCasting = castinInfo.Item1.Length > 0 && castinInfo.Item2 > 0;

            if (isCasting
                && (CastSpellIfPossible(mindFreezeSpell, true)
                || CastSpellIfPossible(strangulateSpell, false, true)))
            {
                return true;
            }

            LastEnemyCastingCheck = DateTime.Now;
            return false;
        }

        private bool HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if (!targetDebuffs.Any(e => e.Equals(frostFeverSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(icyTouchSpell, false, false, true))
            {
                return true;
            }

            if (!targetDebuffs.Any(e => e.Equals(bloodPlagueSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(plagueStrikeSpell, false, false, false, true))
            {
                return true;
            }

            LastDebuffCheck = DateTime.Now;
            return false;
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

            if ((ObjectManager.Player.IsInCombat
                    && !myBuffs.Any(e => e.Equals(hornOfWinterSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(hornOfWinterSpell))
                || (!myBuffs.Any(e => e.Equals(unholyPresenceSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(unholyPresenceSpell)))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool CastSpellIfPossible(string spellName, bool needsRuneenergy = false, bool needsBloodrune = false, bool needsFrostrune = false, bool needsUnholyrune = false)
        {
            if (!Spells.ContainsKey(spellName))
            {
                Spells.Add(spellName, CharacterManager.SpellBook.GetSpellByName(spellName));
            }

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsRuneenergy || Spells[spellName].Costs < ObjectManager.Player.Runeenergy)
                && (!needsBloodrune || (HookManager.IsRuneReady(0) || HookManager.IsRuneReady(1)))
                && (!needsFrostrune || (HookManager.IsRuneReady(2) || HookManager.IsRuneReady(3)))
                && (!needsUnholyrune || (HookManager.IsRuneReady(4) || HookManager.IsRuneReady(5))))
            {
                HookManager.CastSpell(spellName);
                CooldownManager.SetSpellCooldown(spellName, (int)HookManager.GetSpellCooldown(spellName));
                return true;
            }

            return false;
        }
    }
}
