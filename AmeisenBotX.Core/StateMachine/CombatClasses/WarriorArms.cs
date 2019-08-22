using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.States;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class WarriorArms : ICombatClass
    {
        private ObjectManager ObjectManager { get; }
        private CharacterManager CharacterManager { get; }
        private HookManager HookManager { get; }

        private WowPosition LastPosition { get; set; }

        private DateTime HeroicStrikeLastUsed { get; set; }

        public WarriorArms(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
        }

        public void Execute()
        {

            ulong targetGuid = ObjectManager.TargetGuid;
            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(t => t.Guid == targetGuid);
            if (target != null)
            {
                HandleMovement(target);
                HandleAttacking(target);
            }
        }

        private void HandleAttacking(WowUnit target)
        {
            if (!ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
            }

            double rage = ObjectManager.Player.Rage;
            double distance = ObjectManager.Player.Position.GetDistance(target.Position);
            double healthpercent = ((double)target.Health / (double)target.MaxHealth) * 100;
            double healthpercentme = ((double)ObjectManager.Player.Health / (double)ObjectManager.Player.MaxHealth) * 100.0;

            if (HookManager.GetSpellCooldown("Enraged Regeneration") <= 0 
                && rage >= 15 && healthpercentme <= 40)
            {
                HookManager.CastSpell("Enraged Regeneration");
            }

            if (HookManager.GetSpellCooldown("Intimidating Shout") <= 0 
                && rage >= 25 && healthpercentme <= 40)
            {
                HookManager.CastSpell("Intimidating Shout");
            }

            if (HookManager.GetSpellCooldown("charge") <= 0 
                && distance > 8 && distance < 25)
            {
                HookManager.CastSpell("charge");
            }

            if (HookManager.GetSpellCooldown("Bladestorm") <= 0
            && rage >= 25)
            {
                HookManager.CastSpell("Bladestorm");
            }

            if (HookManager.GetSpellCooldown("Mortal Strike") <= 0 
                && rage >= 30 && distance < 3)
            {
                HookManager.CastSpell("Mortal Strike");
            }

            if (HookManager.GetSpellCooldown("Pummel") <= 0
            && rage >= 10 && distance < 3)
            {
                HookManager.CastSpell("Pummel");
            }

            if (HeroicStrikeLastUsed + TimeSpan.FromSeconds(6) < DateTime.Now
                && rage >= 12 && distance < 3)
            {
                HookManager.CastSpell("Heroic Strike");
                HeroicStrikeLastUsed = DateTime.Now;
            }

            if (target.IsFleeing && distance < 3 && rage >= 12)
            {
                HookManager.CastSpell("Hamstring");
            }

            if (rage >= 15 && distance < 3 && healthpercent <= 20)
            {
                HookManager.CastSpell("Execute");
            }

            if (HookManager.GetSpellCooldown("Berserker Rage") <= 0
            && ObjectManager.Player.IsConfused || ObjectManager.Player.IsDazed || ObjectManager.Player.IsSilenced
            || ObjectManager.Player.IsFleeing)
            {
                HookManager.CastSpell("Berserker Rage");
            }

            if (HookManager.GetSpellCooldown("Heroic Throw") <= 0
            && distance <= 30 && distance >= 10)
            {
                HookManager.CastSpell("Heroic Throw");
            }

        }

        private void HandleMovement(WowUnit target)
        {
            double distanceTravel = ObjectManager.Player.Position.GetDistance(LastPosition);
            CharacterManager.MoveToPosition(target.Position);
            if(distanceTravel < 0.001 && distanceTravel > 0)
            {
                CharacterManager.Jump();
            }
            LastPosition = ObjectManager.Player.Position;
        }
    }
}
