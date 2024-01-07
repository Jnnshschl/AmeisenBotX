using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class MoveToPositionQuestObjective(AmeisenBotInterfaces bot, Vector3 position, double distance, MovementAction movementAction = MovementAction.Move) : IQuestObjective
    {
        public bool Finished { get; set; }

        public double Progress => WantedPosition.GetDistance(Bot.Player.Position) < Distance ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; } = bot;

        private double Distance { get; } = distance;

        private MovementAction MovementAction { get; } = movementAction;

        private Vector3 WantedPosition { get; } = position;

        public void Execute()
        {
            if (Finished || Progress == 100.0)
            {
                Finished = true;
                Bot.Movement.Reset();
                Bot.Wow.StopClickToMove();
                return;
            }

            if (WantedPosition.GetDistance2D(Bot.Player.Position) > Distance)
            {
                Bot.Movement.SetMovementAction(MovementAction, WantedPosition);
            }
        }
    }
}