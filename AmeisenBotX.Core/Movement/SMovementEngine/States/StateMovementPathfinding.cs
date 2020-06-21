using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Movement.SMovementEngine.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Movement.SMovementEngine.States
{
    public class StateMovementPathfinding : BasicMovementState
    {
        public StateMovementPathfinding(StateBasedMovementEngine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (StateMachine.FinalTargetPosition == default)
            {
                StateMachine.Reset();
                return;
            }

            if (StateMachine.MovementAction != MovementAction.DirectMove)
            {
                if (StateMachine.Path?.Count == 0)
                {
                    List<Vector3> nodeList;

                    double distanceToTargetPosition = WowInterface.ObjectManager.Player.Position.GetDistance(StateMachine.FinalTargetPosition);

                    if (distanceToTargetPosition > 1024)
                    {
                        return;
                    }

                    if (distanceToTargetPosition > 8.0
                        || !WowInterface.HookManager.IsInLineOfSight(WowInterface.ObjectManager.Player.Position, StateMachine.FinalTargetPosition))
                    {
                        // regular pathfinding
                        nodeList = StateMachine.PathfindingHandler.GetPath((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, StateMachine.FinalTargetPosition);
                    }
                    else
                    {
                        // move along surface
                        // nodeList = new List<Vector3>() { StateMachine.PathfindingHandler.MoveAlongSurface((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, StateMachine.FinalTargetPosition) };
                        nodeList = new List<Vector3>() { StateMachine.FinalTargetPosition };
                    }

                    if (nodeList != null && nodeList.Count > 0)
                    {
                        foreach (Vector3 node in nodeList)
                        {
                            StateMachine.Nodes.Enqueue(node);
                        }
                    }
                }
                else
                {
                    double lastNodeDistanceToTarget = StateMachine.Path.Last().GetDistance(StateMachine.FinalTargetPosition);

                    if (lastNodeDistanceToTarget > 24)
                    {
                        // target position is to far away from the end of the path, path may be incomplete
                        StateMachine.SetState((int)MovementState.MoveToNode);
                    }
                    else
                    {
                        // pathfinding successful
                        StateMachine.SetState((int)MovementState.MoveToNode);
                    }
                }
            }
            else
            {
                StateMachine.SetState((int)MovementState.MoveToNode);
            }
        }

        public override void Exit()
        {
        }
    }
}