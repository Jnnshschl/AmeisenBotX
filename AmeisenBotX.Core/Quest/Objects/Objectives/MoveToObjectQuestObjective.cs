using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class MoveToObjectQuestObjective : IQuestObjective
    {
        public MoveToObjectQuestObjective(WowInterface wowInterface, int objectDisplayId, double distance)
        {
            WowInterface = wowInterface;
            ObjectDisplayIds = new List<int>() { objectDisplayId };
            Distance = distance;
        }

        public MoveToObjectQuestObjective(WowInterface wowInterface, List<int> objectDisplayIds, double distance)
        {
            WowInterface = wowInterface;
            ObjectDisplayIds = objectDisplayIds;
            Distance = distance;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => WowGameobject != null && WowGameobject.Position.GetDistance(WowInterface.Player.Position) < Distance ? 100.0 : 0.0;

        private double Distance { get; }

        private List<int> ObjectDisplayIds { get; }

        private WowGameobject WowGameobject { get; set; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished)
            {
                WowInterface.MovementEngine.Reset();
                WowInterface.HookManager.WowStopClickToMove();
                return;
            }

            WowGameobject = WowInterface.ObjectManager.GetClosestWowGameobjectByDisplayId(ObjectDisplayIds);

            if (WowGameobject != null)
            {
                if (WowGameobject.Position.GetDistance2D(WowInterface.Player.Position) > Distance)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowGameobject.Position);
                }
            }
        }
    }
}