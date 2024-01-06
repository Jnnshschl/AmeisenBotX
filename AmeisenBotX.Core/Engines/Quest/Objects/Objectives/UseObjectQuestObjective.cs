using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public delegate bool UseObjectQuestObjectiveCondition();

    public class UseObjectQuestObjective : IQuestObjective
    {
        public UseObjectQuestObjective(AmeisenBotInterfaces bot, int objectDisplayId, UseObjectQuestObjectiveCondition condition)
        {
            Bot = bot;
            ObjectDisplayIds = [objectDisplayId];
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

        private AmeisenBotInterfaces Bot { get; }

        private UseObjectQuestObjectiveCondition Condition { get; }

        private IWowGameobject IWowGameobject { get; set; }

        private List<int> ObjectDisplayIds { get; }

        private TimegatedEvent UseEvent { get; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            IWowGameobject = Bot.Objects.All
                .OfType<IWowGameobject>()
                .Where(e => ObjectDisplayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                .FirstOrDefault();

            if (IWowGameobject != null)
            {
                if (IWowGameobject.Position.GetDistance(Bot.Player.Position) < 3.0)
                {
                    if (UseEvent.Run())
                    {
                        Bot.Wow.StopClickToMove();
                        Bot.Movement.Reset();

                        Bot.Wow.InteractWithObject(IWowGameobject);
                    }
                }
                else
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, IWowGameobject.Position);
                }
            }
        }
    }
}