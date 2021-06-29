﻿using AmeisenBotX.Common.Utils;
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
        public UseObjectQuestObjective(WowInterface wowInterface, int objectDisplayId, UseObjectQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            ObjectDisplayIds = new List<int>() { objectDisplayId };
            Condition = condition;

            UseEvent = new(TimeSpan.FromSeconds(1));
        }

        public UseObjectQuestObjective(WowInterface wowInterface, List<int> objectDisplayIds, UseObjectQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
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

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished || WowInterface.Player.IsCasting) { return; }

            WowGameobject = WowInterface.Objects.WowObjects
                .OfType<WowGameobject>()
                .Where(e => ObjectDisplayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                .FirstOrDefault();

            if (WowGameobject != null)
            {
                if (WowGameobject.Position.GetDistance(WowInterface.Player.Position) < 3.0)
                {
                    if (UseEvent.Run())
                    {
                        WowInterface.NewWowInterface.WowStopClickToMove();
                        WowInterface.MovementEngine.Reset();

                        WowInterface.NewWowInterface.WowObjectRightClick(WowGameobject.BaseAddress);
                    }
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowGameobject.Position);
                }
            }
        }
    }
}