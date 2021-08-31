using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public delegate bool UseUnitQuestObjectiveCondition();

    public class UseUnitQuestObjective : IQuestObjective
    {
        public UseUnitQuestObjective(AmeisenBotInterfaces bot, int objectDisplayId, bool questgiversOnly, UseUnitQuestObjectiveCondition condition)
        {
            Bot = bot;
            ObjectDisplayIds = new List<int>() { objectDisplayId };
            Condition = condition;
            QuestgiversOnly = questgiversOnly;
        }

        public UseUnitQuestObjective(AmeisenBotInterfaces bot, List<int> objectDisplayIds, bool questgiversOnly, UseUnitQuestObjectiveCondition condition)
        {
            Bot = bot;
            ObjectDisplayIds = objectDisplayIds;
            Condition = condition;
            QuestgiversOnly = questgiversOnly;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private UseUnitQuestObjectiveCondition Condition { get; }

        private IWowUnit IWowUnit { get; set; }

        private List<int> ObjectDisplayIds { get; }

        private bool QuestgiversOnly { get; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            IWowUnit = Bot.GetClosestQuestGiverByDisplayId(Bot.Player.Position, ObjectDisplayIds, QuestgiversOnly);

            if (IWowUnit != null)
            {
                if (IWowUnit.Position.GetDistance(Bot.Player.Position) < 3.0)
                {
                    Bot.Wow.StopClickToMove();
                    Bot.Movement.Reset();
                }

                Bot.Wow.InteractWithUnit(IWowUnit.BaseAddress);
            }
        }
    }
}