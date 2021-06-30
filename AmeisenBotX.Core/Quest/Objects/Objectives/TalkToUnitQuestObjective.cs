using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool TalkToUnitQuestObjectiveCondition();

    public class TalkToUnitQuestObjective : IQuestObjective
    {
        public TalkToUnitQuestObjective(AmeisenBotInterfaces bot, int displayId, List<int> gossipIds, TalkToUnitQuestObjectiveCondition condition)
        {
            Bot = bot;
            DisplayIds = new List<int>() { displayId };
            GossipIds = gossipIds;
            Condition = condition;

            TalkEvent = new(TimeSpan.FromMilliseconds(500));
        }

        public TalkToUnitQuestObjective(AmeisenBotInterfaces bot, List<int> displayIds, List<int> gossipIds, TalkToUnitQuestObjectiveCondition condition)
        {
            Bot = bot;
            DisplayIds = displayIds;
            GossipIds = gossipIds;
            Condition = condition;

            TalkEvent = new(TimeSpan.FromMilliseconds(500));
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private TalkToUnitQuestObjectiveCondition Condition { get; }

        private int Counter { get; set; }

        private List<int> DisplayIds { get; }

        private List<int> GossipIds { get; }

        private TimegatedEvent TalkEvent { get; }

        private AmeisenBotInterfaces Bot { get; }

        private WowUnit WowUnit { get; set; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            WowUnit = Bot.Objects.WowObjects
                .OfType<WowUnit>()
                .Where(e => e.IsGossip && !e.IsDead && DisplayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                .FirstOrDefault();

            if (WowUnit != null)
            {
                if (WowUnit.Position.GetDistance(Bot.Player.Position) < 3.0)
                {
                    if (TalkEvent.Run())
                    {
                        Bot.Wow.WowStopClickToMove();
                        Bot.Movement.Reset();

                        Bot.Wow.WowUnitRightClick(WowUnit.BaseAddress);

                        ++Counter;
                        if (Counter > GossipIds.Count)
                        {
                            Counter = 1;
                        }

                        Bot.Wow.LuaSelectGossipOption(GossipIds[Counter - 1]);
                    }
                }
                else
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, WowUnit.Position);
                }
            }
        }
    }
}