using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool UseObjectQuestObjectiveCondition();

    public class UseObjectQuestObjective : IQuestObjective
    {
        public UseObjectQuestObjective(AmeisenBotInterfaces bot, int objectDisplayId, UseObjectQuestObjectiveCondition condition)
        {
            Bot = bot;
            ObjectDisplayIds = new List<int>() { objectDisplayId };
            Condition = condition;

            UseEvent = new(TimeSpan.FromSeconds(1));
        }

        public UseObjectQuestObjective(AmeisenBotInterfaces bot, List<int> objectDisplayIds, UseObjectQuestObjectiveCondition condition)
        {
            Bot = bot;
            ObjectDisplayIds = objectDisplayIds;
            Condition = condition;

            UseEvent = new(TimeSpan.FromSeconds(1));
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private UseObjectQuestObjectiveCondition Condition { get; }

        private List<int> ObjectDisplayIds { get; }

        private TimegatedEvent UseEvent { get; }

        private WowGameobject WowGameobject { get; set; }

        private AmeisenBotInterfaces Bot { get; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            WowGameobject = Bot.Objects.WowObjects
                .OfType<WowGameobject>()
                .Where(e => ObjectDisplayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                .FirstOrDefault();

            if (WowGameobject != null)
            {
                if (WowGameobject.Position.GetDistance(Bot.Player.Position) < 3.0)
                {
                    if (UseEvent.Run())
                    {
                        Bot.Wow.WowStopClickToMove();
                        Bot.Movement.Reset();

                        Bot.Wow.WowObjectRightClick(WowGameobject.BaseAddress);
                    }
                }
                else
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, WowGameobject.Position);
                }
            }
        }
    }
}