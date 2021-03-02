using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    internal class GrindingObjective : IQuestObjective
    {
        public GrindingObjective(WowInterface wowInterface, int targetLevel, List<List<Vector3>> grindingAreas)
        {
            WowInterface = wowInterface;
            WantedLevel = targetLevel;
            SearchAreas = new SearchAreaEnsamble(grindingAreas);
        }

        public Vector3 CurrentNode { get; set; }

        public bool Finished => WowInterface.Player.Level >= WantedLevel;

        public double Progress => 100.0 * (WowInterface.Player.Level + WowInterface.Player.XpPercentage / 100.0) / WantedLevel;

        private SearchAreaEnsamble SearchAreas { get; }

        private int WantedLevel { get; }

        private WowInterface WowInterface { get; }

        private WowUnit WowUnit { get; set; }

        public void Execute()
        {
            if (Finished || WowInterface.Player.IsCasting) { return; }

            if (!SearchAreas.IsPlayerNearSearchArea(WowInterface))
            {
                WowInterface.HookManager.WowClearTarget();
                WowUnit = null;
            }

            if (!WowInterface.Player.IsInCombat)
            {
                WowUnit = WowInterface.ObjectManager.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => !e.IsDead && !e.IsNotAttackable
                                && WowInterface.HookManager.WowGetUnitReaction(WowInterface.Player, e) != WowUnitReaction.Friendly)
                    .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                    .FirstOrDefault();

                if (WowUnit != null)
                {
                    WowInterface.HookManager.WowTargetGuid(WowUnit.Guid);
                }
            }

            if (WowUnit != null)
            {
                SearchAreas.NotifyDetour();
                WowInterface.CombatClass.AttackTarget();
            }
            else if (WowInterface.Player.Position.GetDistance(CurrentNode) < 3.5f || SearchAreas.HasAbortedPath())
            {
                CurrentNode = SearchAreas.GetNextPosition(WowInterface);
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, CurrentNode);
            }
        }
    }
}