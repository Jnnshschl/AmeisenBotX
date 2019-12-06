using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Battleground.Objectives
{
    public class RetrieveOwnFlagObjective : IBattlegroundObjective
    {
        public RetrieveOwnFlagObjective(int priority, HookManager hookManager, ObjectManager objectManager, IMovementEngine movementEngine)
        {
            Priority = priority;
            HookManager = hookManager;
            ObjectManager = objectManager;
            MovementEngine = movementEngine;
            IsAvailable = false;
        }

        public int Priority { get; private set; }

        public WowPlayer FlagCarrier { get; set; }

        public bool IsAvailable { get; set; }

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        private IMovementEngine MovementEngine { get; }

        public void Enter()
        {
            MovementEngine.Reset();
        }

        public void Execute()
        {
            if(FlagCarrier == null)
            {
                return;
            }

            ObjectManager.UpdateObject(FlagCarrier);

            if (ObjectManager.Player.Position.GetDistance(FlagCarrier.Position) > 3)
            {
                MovementEngine.SetState(MovementEngineState.Moving, FlagCarrier.Position);
                MovementEngine.Execute();
            }
            else
            {
                HookManager.TargetGuid(FlagCarrier.Guid);
                HookManager.StartAutoAttack();
            }
        }

        public void Exit()
        {
            MovementEngine.Reset();
        }
    }
}
