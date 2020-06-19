using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class MovePetToPositionQuestObjective : IQuestObjective
    {
        public MovePetToPositionQuestObjective(WowInterface wowInterface, Vector3 position, double distance, MovementAction movementAction = MovementAction.Moving)
        {
            WowInterface = wowInterface;
            WantedPosition = position;
            Distance = distance;
            MovementAction = movementAction;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => WowInterface.ObjectManager.Pet != null && WantedPosition.GetDistance(WowInterface.ObjectManager.Pet.Position) < Distance ? 100.0 : 0.0;

        private double Distance { get; }

        private MovementAction MovementAction { get; }

        private Vector3 WantedPosition { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished)
            {
                WowInterface.MovementEngine.Reset();
                WowInterface.HookManager.StopClickToMoveIfActive();
                return;
            }

            if (WantedPosition.GetDistanceIgnoreZ(WowInterface.ObjectManager.Pet.Position) > Distance)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction, WantedPosition);
            }
        }
    }
}