using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool TalkToUnitQuestObjectiveCondition();

    public class TalkToUnitQuestObjective : IQuestObjective
    {
        public TalkToUnitQuestObjective(WowInterface wowInterface, int displayId, List<int> gossipIds, TalkToUnitQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            DisplayIds = new List<int>() { displayId };
            GossipIds = gossipIds;
            Condition = condition;

            TalkEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
        }

        public TalkToUnitQuestObjective(WowInterface wowInterface, List<int> displayIds, List<int> gossipIds, TalkToUnitQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            DisplayIds = displayIds;
            GossipIds = gossipIds;
            Condition = condition;

            TalkEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private TalkToUnitQuestObjectiveCondition Condition { get; }

        private int Counter { get; set; }

        private List<int> DisplayIds { get; }

        private List<int> GossipIds { get; }

        private TimegatedEvent TalkEvent { get; }

        private WowInterface WowInterface { get; }

        private WowUnit WowUnit { get; set; }

        public void Execute()
        {
            if (Finished || WowInterface.ObjectManager.Player.IsCasting) { return; }

            WowUnit = WowInterface.ObjectManager.WowObjects
                .OfType<WowUnit>()
                .Where(e => e.IsGossip && !e.IsDead && DisplayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .FirstOrDefault();

            if (WowUnit != null)
            {
                if (WowUnit.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 3.0)
                {
                    if (TalkEvent.Run())
                    {
                        WowInterface.HookManager.WowStopClickToMove();
                        WowInterface.MovementEngine.Reset();

                        WowInterface.HookManager.WowUnitRightClick(WowUnit);

                        ++Counter;
                        if (Counter > GossipIds.Count)
                        {
                            Counter = 1;
                        }

                        WowInterface.HookManager.LuaSelectGossipOption(GossipIds[Counter - 1]);
                    }
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowUnit.Position);
                }
            }
        }
    }
}