using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Units.Unitives
{
    public class MoveToUnitQuestObjective : IQuestObjective
    {
        public MoveToUnitQuestObjective(AmeisenBotInterfaces bot, int unitDisplayId, double distance)
        {
            Bot = bot;
            UnitDisplayIds = new List<int>() { unitDisplayId };
            Distance = distance;
        }

        public MoveToUnitQuestObjective(AmeisenBotInterfaces bot, List<int> unitDisplayIds, double distance)
        {
            Bot = bot;
            UnitDisplayIds = unitDisplayIds;
            Distance = distance;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => WowUnit != null && WowUnit.Position.GetDistance(Bot.Player.Position) < Distance ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private double Distance { get; }

        private List<int> UnitDisplayIds { get; }

        private WowUnit WowUnit { get; set; }

        public void Execute()
        {
            if (Finished)
            {
                Bot.Movement.Reset();
                Bot.Wow.WowStopClickToMove();
                return;
            }

            WowUnit = Bot.Objects.GetClosestWowUnitByDisplayId(Bot.Player.Position, UnitDisplayIds);

            if (WowUnit != null)
            {
                if (WowUnit.Position.GetDistance2D(Bot.Player.Position) > Distance)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, WowUnit.Position);
                }
            }
        }
    }
}