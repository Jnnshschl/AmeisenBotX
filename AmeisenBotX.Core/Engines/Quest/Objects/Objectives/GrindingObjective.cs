using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Movement.Pathfinding.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    internal class GrindingObjective : IQuestObjective
    {
        public GrindingObjective(AmeisenBotInterfaces bot, int targetLevel,
            List<List<Vector3>> grindingAreas, List<Vector3> vendorsLocation = null)
        {
            Bot = bot;
            WantedLevel = targetLevel;
            SearchAreas = new SearchAreaEnsamble(grindingAreas);
            VendorsLocation = vendorsLocation;
        }

        public bool Finished => Bot.Player.Level >= WantedLevel;

        public double Progress => 100.0 * (Bot.Player.Level + Bot.Player.XpPercentage / 100.0) / WantedLevel;

        public List<Vector3> VendorsLocation { get; }

        private AmeisenBotInterfaces Bot { get; }

        private Vector3 CurrentNode { get; set; }

        private IWowUnit IWowUnit { get; set; }

        private SearchAreaEnsamble SearchAreas { get; }

        private int WantedLevel { get; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            if (!SearchAreas.IsPlayerNearSearchArea(Bot) && Bot.Target == null) // if i have target, go nearby don't clear it
            {
                Bot.Wow.ClearTarget();
                IWowUnit = null;
            }

            if (!Bot.Player.IsInCombat
                && Bot.Target == null) // if pulling with ranged we have target and yet not in combat
            {
                IWowUnit = Bot.Objects.All
                    .OfType<IWowUnit>()
                    .Where(e => !e.IsDead && !e.IsNotAttackable
                                    && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Friendly
                                    && e.Health > 10 // workaround to filter some critters, would be nice e.CreatureType != WoWCreatureType.Critter
                                    && BotMath.SlopeGradientAngle(Bot.Player.Position, e.Position) <= 39.0f) // check if not too steep
                    .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                    .FirstOrDefault();

                if (IWowUnit != null)
                {
                    Bot.Wow.ChangeTarget(IWowUnit.Guid);
                }
            }

            if (IWowUnit != null)
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