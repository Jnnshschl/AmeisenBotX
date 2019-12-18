using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using System;
using System.Collections.Generic;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;
using static AmeisenBotX.Core.StateMachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public abstract class BasicCombatClass : ICombatClass
    {
        protected BasicCombatClass(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
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

            MyAuraManager = new AuraManager(
                BuffsToKeepOnMe, 
                null, 
                TimeSpan.FromSeconds(1), 
                () => HookManager.GetBuffs(WowLuaUnit.Player), 
                () => HookManager.GetDebuffs(WowLuaUnit.Player), 
                null, 
                DispellDebuffsFunction);

            TargetAuraManager = new AuraManager(
                null,
                DebuffsToKeepOnTarget,
                TimeSpan.FromSeconds(1),
                () => HookManager.GetBuffs(WowLuaUnit.Target),
                () => HookManager.GetDebuffs(WowLuaUnit.Target),
                DispellBuffsFunction,
                null);

            TargetInterruptManager = new InterruptManager(ObjectManager.Target, InterruptSpells);
        }

        public DispellBuffsFunction DispellBuffsFunction { get; internal set; }

        public DispellDebuffsFunction DispellDebuffsFunction { get; internal set; }

        public CharacterManager CharacterManager { get; internal set; }

        public HookManager HookManager { get; internal set; }

        public ObjectManager ObjectManager { get; internal set; }

        public CooldownManager CooldownManager { get; internal set; }

        public Dictionary<string, Spell> Spells { get; internal set; }

        public AuraManager MyAuraManager { get; internal set; }

        public AuraManager TargetAuraManager { get; internal set; }

        public InterruptManager TargetInterruptManager { get; internal set; }

        public Dictionary<string, CastFunction> BuffsToKeepOnMe { get; internal set; }

        public Dictionary<string, CastFunction> DebuffsToKeepOnTarget { get; internal set; }

        public SortedList<int, CastInterruptFunction> InterruptSpells { get; internal set; }

        public abstract string Displayname { get; }

        public abstract string Version { get; }

        public abstract string Author { get; }

        public abstract string Description { get; }

        public abstract CombatClassRole Role { get; }

        public abstract WowClass Class { get; }

        public abstract bool HandlesMovement { get; }

        public abstract bool HandlesTargetSelection { get; }

        public abstract bool IsMelee { get; }

        public abstract IWowItemComparator ItemComparator { get; set; }

        public abstract Dictionary<string, dynamic> Configureables { get; set; }

        public abstract void Execute();

        public abstract void OutOfCombatExecute();
    }
}
