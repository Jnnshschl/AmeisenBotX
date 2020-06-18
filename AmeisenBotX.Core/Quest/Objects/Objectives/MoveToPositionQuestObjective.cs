using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class MoveToPositionQuestObjective : IQuestObjective
    {
        public MoveToPositionQuestObjective(WowInterface wowInterface, Vector3 position, double distance)
        {
            WowInterface = wowInterface;
            WantedPosition = position;
            Distance = distance;

            InitialDistance = WantedPosition.GetDistance(WowInterface.ObjectManager.Player.Position);
        }

        public bool Finished => Progress == 100.0;

        public double Progress => WantedPosition.GetDistance(WowInterface.ObjectManager.Player.Position) / InitialDistance * 100.0;

        private double Distance { get; }

        private double InitialDistance { get; }

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

            if (WantedPosition.GetDistanceIgnoreZ(WowInterface.ObjectManager.Player.Position) > Distance)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, WantedPosition);
            }
        }
    }
}