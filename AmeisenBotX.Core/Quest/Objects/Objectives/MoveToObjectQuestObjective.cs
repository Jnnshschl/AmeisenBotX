using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class MoveToObjectQuestObjective : IQuestObjective
    {
        public MoveToObjectQuestObjective(AmeisenBotInterfaces bot, int objectDisplayId, double distance)
        {
            Bot = bot;
            ObjectDisplayIds = new List<int>() { objectDisplayId };
            Distance = distance;
        }

        public MoveToObjectQuestObjective(AmeisenBotInterfaces bot, List<int> objectDisplayIds, double distance)
        {
            Bot = bot;
            ObjectDisplayIds = objectDisplayIds;
            Distance = distance;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => WowGameobject != null && WowGameobject.Position.GetDistance(Bot.Player.Position) < Distance ? 100.0 : 0.0;

        private double Distance { get; }

        private List<int> ObjectDisplayIds { get; }

        private WowGameobject WowGameobject { get; set; }

        private AmeisenBotInterfaces Bot { get; }

        public void Execute()
        {
            if (Finished)
            {
                Bot.Movement.Reset();
                Bot.Wow.WowStopClickToMove();
                return;
            }

            WowGameobject = Bot.Objects.GetClosestWowGameobjectByDisplayId(Bot.Player.Position, ObjectDisplayIds);

            if (WowGameobject != null)
            {
                if (WowGameobject.Position.GetDistance2D(Bot.Player.Position) > Distance)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, WowGameobject.Position);
                }
            }
        }
    }
}