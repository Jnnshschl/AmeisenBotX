using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Units.Unitives
{
    public class MoveToUnitQuestObjective : IQuestObjective
    {
        public MoveToUnitQuestObjective(WowInterface wowInterface, int unitDisplayId, double distance)
        {
            WowInterface = wowInterface;
            UnitDisplayIds = new List<int>() { unitDisplayId };
            Distance = distance;
        }

        public MoveToUnitQuestObjective(WowInterface wowInterface, List<int> unitDisplayIds, double distance)
        {
            WowInterface = wowInterface;
            UnitDisplayIds = unitDisplayIds;
            Distance = distance;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => WowUnit != null && WowUnit.Position.GetDistance(WowInterface.Player.Position) < Distance ? 100.0 : 0.0;

        private double Distance { get; }

        private List<int> UnitDisplayIds { get; }

        private WowInterface WowInterface { get; }

        private WowUnit WowUnit { get; set; }

        public void Execute()
        {
            if (Finished)
            {
                WowInterface.MovementEngine.Reset();
                WowInterface.HookManager.WowStopClickToMove();
                return;
            }

            WowUnit = WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(UnitDisplayIds);

            if (WowUnit != null)
            {
                if (WowUnit.Position.GetDistance2D(WowInterface.Player.Position) > Distance)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowUnit.Position);
                }
            }
        }
    }
}