using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class MoveToObjectQuestObjective : IQuestObjective
    {
        public MoveToObjectQuestObjective(WowInterface wowInterface, int objectDisplayId, double distance)
        {
            WowInterface = wowInterface;
            ObjectDisplayId = objectDisplayId;
            Distance = distance;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => WowGameobject != null && WowGameobject.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Distance ? 100.0 : 0.0;

        private double Distance { get; }

        private int ObjectDisplayId { get; }

        private WowGameobject WowGameobject { get; set; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished)
            {
                WowInterface.MovementEngine.Reset();
                WowInterface.HookManager.StopClickToMoveIfActive();
                return;
            }

            WowGameobject = WowInterface.ObjectManager.GetClosestWowGameobjectByDisplayId(ObjectDisplayId);

            if (WowGameobject != null)
            {
                if (WowGameobject.Position.GetDistanceIgnoreZ(WowInterface.ObjectManager.Player.Position) > Distance)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, WowGameobject.Position);
                }
            }
        }
    }
}