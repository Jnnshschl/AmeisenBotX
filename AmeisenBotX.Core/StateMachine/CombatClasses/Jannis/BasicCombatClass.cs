using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.Utils;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public abstract class BasicCombatClass : ICombatClass
    {
        protected BasicCombatClass(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            CooldownManager = new CooldownManager(WowInterface.CharacterManager.SpellBook.Spells);

            Spells = new Dictionary<string, Spell>();
            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                Spells.Clear();
                foreach (Spell spell in WowInterface.CharacterManager.SpellBook.Spells)
                {
                    Spells.Add(spell.Name, spell);
                }
            };

            MyAuraManager = new AuraManager(
                null,
                null,
                TimeSpan.FromSeconds(1),
                () => HookManager.GetBuffs(WowLuaUnit.Player),
                () => HookManager.GetDebuffs(WowLuaUnit.Player),
                null,
                DispellDebuffsFunction);

            TargetAuraManager = new AuraManager(
                null,
                null,
                TimeSpan.FromSeconds(1),
                () => HookManager.GetBuffs(WowLuaUnit.Target),
                () => HookManager.GetDebuffs(WowLuaUnit.Target),
                DispellBuffsFunction,
                null);

            TargetInterruptManager = new InterruptManager(WowInterface.ObjectManager.Target, null);
        }

        public abstract string Author { get; }

        public abstract WowClass Class { get; }

        public abstract Dictionary<string, dynamic> Configureables { get; set; }

        public CooldownManager CooldownManager { get; internal set; }

        public abstract string Description { get; }

        public DispellBuffsFunction DispellBuffsFunction { get; internal set; }

        public DispellDebuffsFunction DispellDebuffsFunction { get; internal set; }

        public abstract string Displayname { get; }

        public abstract bool HandlesMovement { get; }

        public abstract bool HandlesTargetSelection { get; }

        public IHookManager HookManager { get; internal set; }

        public abstract bool IsMelee { get; }

        public abstract IWowItemComparator ItemComparator { get; set; }

        public AuraManager MyAuraManager { get; internal set; }

        public abstract CombatClassRole Role { get; }

        public Dictionary<string, Spell> Spells { get; internal set; }

        public AuraManager TargetAuraManager { get; internal set; }

        public InterruptManager TargetInterruptManager { get; internal set; }

        public abstract string Version { get; }

        public WowInterface WowInterface { get; internal set; }

        public abstract void Execute();

        public abstract void OutOfCombatExecute();

        internal bool CastSpellIfPossible(string spellName, bool needsResource = false, int currentResourceAmount = 0)
        {
            if (currentResourceAmount == 0)
            {
                currentResourceAmount = WowInterface.ObjectManager.Player.Class switch
                {
                    WowClass.Deathknight => WowInterface.ObjectManager.Player.Runeenergy,
                    WowClass.Rogue => WowInterface.ObjectManager.Player.Energy,
                    WowClass.Warrior => WowInterface.ObjectManager.Player.Rage,
                    _ => WowInterface.ObjectManager.Player.Mana,
                };
            }

            PrepareCast(spellName);

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsResource || Spells[spellName].Costs < currentResourceAmount)
                && (WowInterface.ObjectManager.Target != null && IsInRange(Spells[spellName], WowInterface.ObjectManager.Target.Position)))
            {
                CastSpell(spellName);
                return true;
            }

            return false;
        }

        internal bool CastSpellIfPossibleDk(string spellName, bool needsRuneenergy = false, bool needsBloodrune = false, bool needsFrostrune = false, bool needsUnholyrune = false)
        {
            PrepareCast(spellName);

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsRuneenergy || Spells[spellName].Costs < WowInterface.ObjectManager.Player.Runeenergy)
                && (!needsBloodrune || (HookManager.IsRuneReady(0) || HookManager.IsRuneReady(1)))
                && (!needsFrostrune || (HookManager.IsRuneReady(2) || HookManager.IsRuneReady(3)))
                && (!needsUnholyrune || (HookManager.IsRuneReady(4) || HookManager.IsRuneReady(5)))
                && IsInRange(Spells[spellName], WowInterface.ObjectManager.Target.Position))
            {
                CastSpell(spellName);
                return true;
            }

            return false;
        }

        internal bool CastSpellIfPossibleRogue(string spellName, bool needsEnergy = false, bool needsCombopoints = false, int requiredCombopoints = 1)
        {
            PrepareCast(spellName);

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsEnergy || Spells[spellName].Costs < WowInterface.ObjectManager.Player.Energy)
                && (!needsCombopoints || WowInterface.ObjectManager.Player.ComboPoints >= requiredCombopoints)
                && IsInRange(Spells[spellName], WowInterface.ObjectManager.Target.Position))
            {
                CastSpell(spellName);
                return true;
            }

            return false;
        }

        private void CastSpell(string spellName)
        {
            HookManager.CastSpell(spellName);
            CooldownManager.SetSpellCooldown(spellName, (int)HookManager.GetSpellCooldown(spellName));
            AmeisenLogger.Instance.Log("CombatClass", $"[{Displayname}]: Casting Spell \"{spellName}\" on \"{WowInterface.ObjectManager.Target?.Name}\"", LogLevel.Verbose);
        }

        private bool IsInRange(Spell spell, Vector3 position)
        {
            if ((spell.MinRange == 0 && spell.MaxRange == 0) || spell.MaxRange == 0)
            {
                return true;
            }

            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(position);
            return distance > spell.MinRange && distance < spell.MaxRange;
        }

        private void PrepareCast(string spellName)
        {
            AmeisenLogger.Instance.Log("CombatClass", $"[{Displayname}]: Trying to cast \"{spellName}\" on \"{WowInterface.ObjectManager.Target?.Name}\"", LogLevel.Verbose);

            if (!Spells.ContainsKey(spellName))
            {
                Spells.Add(spellName, WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName));
            }
        }
    }
}