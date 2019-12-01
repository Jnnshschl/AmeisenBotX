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
    public class CarryFlagToBaseObjective : IBattlegroundObjective
    {
        public CarryFlagToBaseObjective(int priority, Vector3 homePosition, HookManager hookManager, ObjectManager objectManager, IMovementEngine movementEngine)
        {
            Priority = priority;
            HomePosition = homePosition;
            HookManager = hookManager;
            ObjectManager = objectManager;
            MovementEngine = movementEngine;
        }

        public int Priority { get; private set; }

        public Vector3 HomePosition { get; private set; }

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        private IMovementEngine MovementEngine { get; }

        public bool IsAvailable => HookManager.GetBuffs(WowLuaUnit.Player).Any(e => e.Contains(" flag"));

        public void Execute()
        {
            if (ObjectManager.Player.Position.GetDistance(HomePosition) > 3)
            {
                MovementEngine.SetState(MovementEngineState.Moving, HomePosition);
                MovementEngine.Execute();
            }
        }
    }
}
