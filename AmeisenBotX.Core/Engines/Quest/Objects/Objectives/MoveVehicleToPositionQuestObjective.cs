using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class MoveVehicleToPositionQuestObjective : IQuestObjective
    {
        public MoveVehicleToPositionQuestObjective(AmeisenBotInterfaces bot, Vector3 position, double distance, MovementAction movementAction = MovementAction.Move)
        {
            Bot = bot;
            WantedPosition = position;
            Distance = distance;
            MovementAction = movementAction;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Bot.Objects.Vehicle != null && WantedPosition.GetDistance(Bot.Objects.Vehicle.Position) < Distance ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private double Distance { get; }

        private MovementAction MovementAction { get; }

        private Vector3 WantedPosition { get; }

        public void Execute()
        {
            if (Finished)
            {
                Bot.Movement.Reset();
                Bot.Wow.StopClickToMove();
                return;
            }

            if (WantedPosition.GetDistance2D(Bot.Objects.Vehicle.Position) > Distance)
            {
                Bot.Movement.SetMovementAction(MovementAction, WantedPosition, 0);
            }
        }
    }
}