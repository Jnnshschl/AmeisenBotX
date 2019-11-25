using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Common.Enums;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Pathfinding;
using AmeisenBotX.Pathfinding.Objects;
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

        private IMovementEngine MovementEngine { get; set; }

        private ObjectManager ObjectManager { get; }

        private IOffsetList OffsetList { get; }

        private IPathfindingHandler PathfindingHandler { get; }

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
                MovementEngine.SetState(MovementEngineState.Moving, corpsePosition);
                MovementEngine.Execute();
            }
            else
            {
                HookManager.RetrieveCorpse();
            }
        }

        public override void Exit()
        {

        }
    }
}
