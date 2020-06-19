using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
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

        private WowUnit WowUnit { get; set; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished || WowInterface.ObjectManager.Player.IsCasting) { return; }

            if (WowInterface.ObjectManager.Target != null
                && !WowInterface.ObjectManager.Target.IsDead
                && !WowInterface.ObjectManager.Target.IsNotAttackable
                && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target) != WowUnitReaction.Friendly)
            {
                WowUnit = WowInterface.ObjectManager.Target;
            }
            else
            {
                WowInterface.HookManager.ClearTarget();

                WowUnit = WowInterface.ObjectManager.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => !e.IsDead && ObjectDisplayIds.Values.Contains(e.DisplayId))
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                    .OrderBy(e => ObjectDisplayIds.First(x => x.Value == e.DisplayId).Key)
                    .FirstOrDefault();
            }

            if (WowUnit != null)
            {
                if (WowUnit.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 3.0)
                {
                    WowInterface.HookManager.StopClickToMoveIfActive();
                    WowInterface.MovementEngine.Reset();
                    WowInterface.HookManager.UnitOnRightClick(WowUnit);
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, WowUnit.Position);
                }
            }
        }
    }
}