using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    internal class GrindingObjective : IQuestObjective
    {
        public GrindingObjective(AmeisenBotInterfaces bot, int targetLevel, List<List<Vector3>> grindingAreas)
        {
            Bot = bot;
            WantedLevel = targetLevel;
            SearchAreas = new SearchAreaEnsamble(grindingAreas);
        }

        public Vector3 CurrentNode { get; set; }

        public bool Finished => Bot.Player.Level >= WantedLevel;

        public double Progress => 100.0 * (Bot.Player.Level + Bot.Player.XpPercentage / 100.0) / WantedLevel;

        private SearchAreaEnsamble SearchAreas { get; }

        private int WantedLevel { get; }

        private AmeisenBotInterfaces Bot { get; }

        private WowUnit WowUnit { get; set; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            if (!SearchAreas.IsPlayerNearSearchArea(Bot))
            {
                Bot.Wow.WowClearTarget();
                WowUnit = null;
            }

            if (!Bot.Player.IsInCombat)
            {
                WowUnit = Bot.Objects.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => !e.IsDead && !e.IsNotAttackable && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Friendly)
                    .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                    .FirstOrDefault();

                if (WowUnit != null)
                {
                    Bot.Wow.WowTargetGuid(WowUnit.Guid);
                }
            }

            if (WowUnit != null)
            {
                SearchAreas.NotifyDetour();
                Bot.CombatClass.AttackTarget();
            }
            else if (Bot.Player.Position.GetDistance(CurrentNode) < 3.5f || SearchAreas.HasAbortedPath())
            {
                CurrentNode = SearchAreas.GetNextPosition(Bot);
                Bot.Movement.SetMovementAction(MovementAction.Move, CurrentNode);
            }
        }
    }
}