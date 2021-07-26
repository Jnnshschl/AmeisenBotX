using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
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

        public double Progress => IWowGameobject != null && IWowGameobject.Position.GetDistance(Bot.Player.Position) < Distance ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private double Distance { get; }

        private IWowGameobject IWowGameobject { get; set; }

        private List<int> ObjectDisplayIds { get; }

        public void Execute()
        {
            if (Finished)
            {
                Bot.Movement.Reset();
                Bot.Wow.StopClickToMove();
                return;
            }

            IWowGameobject = Bot.GetClosestGameobjectByDisplayId(Bot.Player.Position, ObjectDisplayIds);

            if (IWowGameobject != null)
            {
                if (IWowGameobject.Position.GetDistance2D(Bot.Player.Position) > Distance)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, IWowGameobject.Position);
                }
            }
        }
    }
}