using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Units.Unitives
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

        public double Progress => IWowUnit != null && IWowUnit.Position.GetDistance(Bot.Player.Position) < Distance ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private double Distance { get; }

        private IWowUnit IWowUnit { get; set; }

        private List<int> UnitDisplayIds { get; }

        public void Execute()
        {
            if (Finished)
            {
                Bot.Movement.Reset();
                Bot.Wow.StopClickToMove();
                return;
            }

            IWowUnit = Bot.GetClosestQuestgiverByDisplayId(Bot.Player.Position, UnitDisplayIds);

            if (IWowUnit != null)
            {
                if (IWowUnit.Position.GetDistance2D(Bot.Player.Position) > Distance)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, IWowUnit.Position);
                }
            }
        }
    }
}