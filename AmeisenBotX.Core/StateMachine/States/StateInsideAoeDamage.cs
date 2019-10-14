using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateInsideAoeDamage : State
    {
        public StateInsideAoeDamage(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, CharacterManager characterManager, IPathfindingHandler pathfindingHandler) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            PathfindingHandler = pathfindingHandler;
            CurrentPath = new Queue<Vector3>();
        }

        public Vector3 LastPosition { get; private set; }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private Queue<Vector3> CurrentPath { get; set; }

        private ObjectManager ObjectManager { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private int TryCount { get; set; }

        public override void Enter()
        {
            CurrentPath.Clear();
            TryCount = 0;
        }

        public override void Execute()
        {
            if (ObjectManager != null
                && ObjectManager.Player != null)
            {
                WowDynobject aoeSpellObject = ObjectManager.WowObjects.OfType<WowDynobject>().FirstOrDefault(e => e.Position.GetDistance2D(ObjectManager.Player.Position) < e.Radius + 1);
                if (aoeSpellObject == null)
                {
                    AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
                    return;
                }

                if (CurrentPath.Count > 0)
                {
                    HandleMovement();
                    return;
                }

                Vector3 targetPosition = FindPositionOutsideOfAoeSpell(aoeSpellObject.Position, aoeSpellObject.Radius);

                double distance = ObjectManager.Player.Position.GetDistance(targetPosition);
                if (distance > 3.2)
                {
                    BuildNewPath(targetPosition);
                }
            }
        }

        public override void Exit()
        {
        }

        private Vector3 FindPositionOutsideOfAoeSpell(Vector3 aoePosition, float aoeRadius)
        {
            double angleX = ObjectManager.Player.Position.X - aoePosition.X;
            double angleY = ObjectManager.Player.Position.Y - aoePosition.Y;

            double angle = Math.Atan2(angleX, angleY);

            double distanceToMove = aoeRadius - ObjectManager.Player.Position.GetDistance2D(aoePosition) + 1; // 1m distance additionally

            double x = ObjectManager.Player.Position.X + (Math.Cos(angle) * distanceToMove);
            double y = ObjectManager.Player.Position.Y + (Math.Sin(angle) * distanceToMove);

            Vector3 destination = new Vector3()
            {
                X = Convert.ToSingle(x),
                Y = Convert.ToSingle(y),
                Z = ObjectManager.Player.Position.Z
            };

            return destination;
        }

        private void BuildNewPath(Vector3 targetPosition)
        {
            List<Vector3> path = PathfindingHandler.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, targetPosition);
            if (path.Count > 0)
            {
                foreach (Vector3 pos in path)
                {
                    CurrentPath.Enqueue(pos);
                }
            }
        }

        private void HandleMovement()
        {
            Vector3 pos = CurrentPath.Peek();
            double distance = pos.GetDistance2D(ObjectManager.Player.Position);
            double distTraveled = LastPosition.GetDistance2D(ObjectManager.Player.Position);

            if (distance <= 3
                || TryCount > 5)
            {
                CurrentPath.Dequeue();
                TryCount = 0;
            }
            else
            {
                CharacterManager.MoveToPosition(pos);

                if (distTraveled != 0 && distTraveled < 0.08)
                {
                    TryCount++;
                }

                // if the thing is too far away, drop the whole Path
                if (pos.Z - ObjectManager.Player.Position.Z > 2
                    && distance > 2)
                {
                    CurrentPath.Clear();
                }

                // jump if the node is higher than us
                if (pos.Z - ObjectManager.Player.Position.Z > 1.2
                    && distance < 3)
                {
                    CharacterManager.Jump();
                }
            }

            if (distTraveled != 0
                && distTraveled < 0.08)
            {
                // go forward
                BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr(0x26), 500, 750);
                CharacterManager.Jump();
            }

            LastPosition = ObjectManager.Player.Position;
        }
    }
}
