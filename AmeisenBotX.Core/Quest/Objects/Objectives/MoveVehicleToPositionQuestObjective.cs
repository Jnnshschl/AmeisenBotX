using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class MoveVehicleToPositionQuestObjective : IQuestObjective
    {
        public MoveVehicleToPositionQuestObjective(WowInterface wowInterface, Vector3 position, double distance, MovementAction movementAction = MovementAction.Move, bool forceDirectMove = false)
        {
            WowInterface = wowInterface;
            WantedPosition = position;
            Distance = distance;
            MovementAction = movementAction;
            ForceDirectMove = forceDirectMove;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => WowInterface.ObjectManager.Vehicle != null && WantedPosition.GetDistance(WowInterface.ObjectManager.Vehicle.Position) < Distance ? 100.0 : 0.0;

        private double Distance { get; }

        private bool ForceDirectMove { get; }

        private MovementAction MovementAction { get; }

        private Vector3 WantedPosition { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished)
            {
                WowInterface.MovementEngine.Reset();
                WowInterface.HookManager.WowStopClickToMove();
                return;
            }

            if (WantedPosition.GetDistance2D(WowInterface.ObjectManager.Vehicle.Position) > Distance)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction, WantedPosition, 0);
            }
        }
    }
}