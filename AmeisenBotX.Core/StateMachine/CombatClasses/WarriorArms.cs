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

            double distance = ObjectManager.Player.Position.GetDistance(target.Position);

            if (distance > 8 && distance < 25)
            {
                HookManager.CastSpell("charge");
            }
        }

        private void HandleMovement(WowUnit target)
        {
            double distanceTravel = ObjectManager.Player.Position.GetDistance(LastPosition);
            CharacterManager.MoveToPosition(target.Position);
            if(distanceTravel < 0.01)
            {
                CharacterManager.Jump();
            }
            LastPosition = ObjectManager.Player.Position;
        }
    }
}
