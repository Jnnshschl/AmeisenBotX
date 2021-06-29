﻿using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool KillUnitQuestObjectiveCondition();

    public class KillUnitQuestObjective : IQuestObjective
    {
        public KillUnitQuestObjective(WowInterface wowInterface, int objectDisplayId, KillUnitQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            ObjectDisplayIds = new Dictionary<int, int>() { { 0, objectDisplayId } };
            Condition = condition;
        }

        public KillUnitQuestObjective(WowInterface wowInterface, Dictionary<int, int> objectDisplayIds, KillUnitQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            ObjectDisplayIds = objectDisplayIds;
            Condition = condition;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private KillUnitQuestObjectiveCondition Condition { get; }

        private Dictionary<int, int> ObjectDisplayIds { get; }

        private WowInterface WowInterface { get; }

        private WowUnit WowUnit { get; set; }

        public void Execute()
        {
            if (Finished || WowInterface.Player.IsCasting) { return; }

            if (WowInterface.Target != null
                && !WowInterface.Target.IsDead
                && !WowInterface.Target.IsNotAttackable
                && WowInterface.NewWowInterface.GetReaction(WowInterface.Player.BaseAddress, WowInterface.Target.BaseAddress) != WowUnitReaction.Friendly)
            {
                WowUnit = WowInterface.Target;
            }
            else
            {
                WowInterface.NewWowInterface.WowClearTarget();

                WowUnit = WowInterface.Objects.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => !e.IsDead && ObjectDisplayIds.Values.Contains(e.DisplayId))
                    .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                    .OrderBy(e => ObjectDisplayIds.First(x => x.Value == e.DisplayId).Key)
                    .FirstOrDefault();
            }

            if (WowUnit != null)
            {
                if (WowUnit.Position.GetDistance(WowInterface.Player.Position) < 3.0)
                {
                    WowInterface.NewWowInterface.WowStopClickToMove();
                    WowInterface.MovementEngine.Reset();
                    WowInterface.NewWowInterface.WowUnitRightClick(WowUnit.BaseAddress);
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowUnit.Position);
                }
            }
        }
    }
}