using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class UseUnitQuestObjective : IQuestObjective
    {
        public UseUnitQuestObjective(AmeisenBotInterfaces bot, int objectDisplayId, bool questgiversOnly, Func<bool> condition)
        {
            Bot = bot;
            ObjectDisplayIds = [objectDisplayId];
            Condition = condition;
            QuestgiversOnly = questgiversOnly;
        }

        public UseUnitQuestObjective(AmeisenBotInterfaces bot, List<int> objectDisplayIds, bool questgiversOnly, Func<bool> condition)
        {
            Bot = bot;
            ObjectDisplayIds = objectDisplayIds;
            Condition = condition;
            QuestgiversOnly = questgiversOnly;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private Func<bool> Condition { get; }

        private List<int> ObjectDisplayIds { get; }

        private bool QuestgiversOnly { get; }

        private IWowUnit Unit { get; set; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            Unit = Bot.GetClosestQuestGiverByDisplayId(Bot.Player.Position, ObjectDisplayIds, QuestgiversOnly);

            if (Unit != null)
            {
                if (Unit.Position.GetDistance(Bot.Player.Position) < 3.0)
                {
                    Bot.Wow.StopClickToMove();
                    Bot.Movement.Reset();
                }

                Bot.Wow.InteractWithUnit(Unit);
            }
        }
    }
}