using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class HunterBeastmastery : ICombatClass
    {
        public HunterBeastmastery(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => false;

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        public void Execute()
        {
            
        }

        public void OutOfCombatExecute()
        {

        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells.OrderByDescending(e => e.Rank).FirstOrDefault(e => e.Name.Equals(spellName))?.Costs <= ObjectManager.Player.Mana;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells.Any(e => e.Name.Equals(spellName));

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;
    }
}
