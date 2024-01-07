using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class TalkToUnitQuestObjective : IQuestObjective
    {
        public TalkToUnitQuestObjective(AmeisenBotInterfaces bot, int displayId, List<int> gossipIds, Func<bool> condition)
        {
            Bot = bot;
            DisplayIds = [displayId];
            GossipIds = gossipIds;
            Condition = condition;

            TalkEvent = new(TimeSpan.FromMilliseconds(500));
        }

        public TalkToUnitQuestObjective(AmeisenBotInterfaces bot, List<int> displayIds, List<int> gossipIds, Func<bool> condition)
        {
            Bot = bot;
            DisplayIds = displayIds;
            GossipIds = gossipIds;
            Condition = condition;

            TalkEvent = new(TimeSpan.FromMilliseconds(500));
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private Func<bool> Condition { get; }

        private int Counter { get; set; }

        private List<int> DisplayIds { get; }

        private List<int> GossipIds { get; }

        private TimegatedEvent TalkEvent { get; }

        private IWowUnit Unit { get; set; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            Unit = Bot.Objects.All
                .OfType<IWowUnit>()
                .Where(e => e.IsGossip && !e.IsDead && DisplayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                .FirstOrDefault();

            if (Unit != null)
            {
                if (Unit.Position.GetDistance(Bot.Player.Position) < 3.0)
                {
                    if (TalkEvent.Run())
                    {
                        Bot.Wow.StopClickToMove();
                        Bot.Movement.Reset();

                        Bot.Wow.InteractWithUnit(Unit);

                        ++Counter;
                        if (Counter > GossipIds.Count)
                        {
                            Counter = 1;
                        }

                        Bot.Wow.SelectGossipOption(GossipIds[Counter - 1]);
                    }
                }
                else
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, Unit.Position);
                }
            }
        }
    }
}