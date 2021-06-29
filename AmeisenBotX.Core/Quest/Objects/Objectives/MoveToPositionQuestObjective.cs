﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Movement.Enums;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class MoveToPositionQuestObjective : IQuestObjective
    {
        public MoveToPositionQuestObjective(WowInterface wowInterface, Vector3 position, double distance, MovementAction movementAction = MovementAction.Move)
        {
            WowInterface = wowInterface;
            WantedPosition = position;
            Distance = distance;
            MovementAction = movementAction;
        }

        public bool Finished { get; set; }

        public double Progress => WantedPosition.GetDistance(WowInterface.Player.Position) < Distance ? 100.0 : 0.0;

        private double Distance { get; }

        private MovementAction MovementAction { get; }

        private Vector3 WantedPosition { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished || Progress == 100.0)
            {
                Finished = true;
                WowInterface.MovementEngine.Reset();
                WowInterface.NewWowInterface.WowStopClickToMove();
                return;
            }

            if (WantedPosition.GetDistance2D(WowInterface.Player.Position) > Distance)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction, WantedPosition);
            }
        }
    }
}