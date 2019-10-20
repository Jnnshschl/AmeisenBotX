using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateGhost : State
    {
        public StateGhost(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, IOffsetList offsetList, ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IPathfindingHandler pathfindingHandler, IMovementEngine movementEngine) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            OffsetList = offsetList;
            PathfindingHandler = pathfindingHandler;
            MovementEngine = movementEngine;
        }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        private IOffsetList OffsetList { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private IMovementEngine MovementEngine { get; set; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (ObjectManager.Player.Health > 1)
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
            }

            if (AmeisenBotStateMachine.XMemory.ReadStruct(OffsetList.CorpsePosition, out Vector3 corpsePosition)
                && ObjectManager.Player.Position.GetDistance(corpsePosition) > 16)
            {
                if (MovementEngine.CurrentPath == null)
                {
                    BuildNewPath(corpsePosition);
                }
                else
                {
                    if (MovementEngine.GetNextStep(ObjectManager.Player.Position, ObjectManager.Player.Rotation, out Vector3 positionToGoTo, out bool needToJump))
                    {
                        CharacterManager.MoveToPosition(positionToGoTo);

                        if (needToJump)
                        {
                            CharacterManager.Jump();
                        }
                    }
                }
            }
            else
            {
                HookManager.RetrieveCorpse();
            }
        }

        public override void Exit()
        {
        }

        private void BuildNewPath(Vector3 corpsePosition)
        {
            List<Vector3> path = PathfindingHandler.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, corpsePosition);
            MovementEngine.LoadPath(path);
            MovementEngine.PostProcessPath();
        }
    }
}