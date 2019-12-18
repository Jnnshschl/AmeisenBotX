using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System.Collections.Generic;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;
using static AmeisenBotX.Core.StateMachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class DeathknightUnholy : BasicCombatClass
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

        public DeathknightUnholy(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager) : base(objectManager, characterManager, hookManager)
        {
            BuffsToKeepOnMe = new Dictionary<string, CastFunction>()
            {
                { unholyPresenceSpell, () => CastSpellIfPossible(unholyPresenceSpell) },
                { hornOfWinterSpell, () => CastSpellIfPossible(hornOfWinterSpell, true) }
            };

            DebuffsToKeepOnTarget = new Dictionary<string, CastFunction>()
            {
                { frostFeverSpell, () => CastSpellIfPossible(icyTouchSpell, false, false, false, true) },
                { bloodPlagueSpell, () => CastSpellIfPossible(plagueStrikeSpell, false, false, false, true) }
            };

            InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(mindFreezeSpell, true) },
                { 1, () => CastSpellIfPossible(strangulateSpell, false, true) }
            };
        }

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator();

        public override string Displayname => "Deathknight Unholy";

        public override string Version => "1.0";

        public override string Author => "Jannis";

        public override string Description => "FCFS based CombatClass for the Unholy Deathknight spec.";

        public override WowClass Class => WowClass.Deathknight;

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override void Execute()
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

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick()
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

        public override void OutOfCombatExecute()
        {
            MyAuraManager.Tick();
        }

        private bool CastSpellIfPossible(string spellName, bool needsRuneenergy = false, bool needsBloodrune = false, bool needsFrostrune = false, bool needsUnholyrune = false)
        {
            AmeisenLogger.Instance.Log($"[{Displayname}]: Trying to cast \"{spellName}\" on \"{ObjectManager.Target?.Name}\"", LogLevel.Verbose);

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
                AmeisenLogger.Instance.Log($"[{Displayname}]: Casting Spell \"{spellName}\" on \"{ObjectManager.Target?.Name}\"", LogLevel.Verbose);
                return true;
            }

            return false;
        }
    }
}
