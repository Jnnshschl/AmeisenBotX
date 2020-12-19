using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    class GrindingObjective : IQuestObjective
    {
        public GrindingObjective(WowInterface wowInterface, int targetLevel, List<List<Vector3>> grindingAreas)
        {
            WowInterface = wowInterface;
            WantedLevel = targetLevel;
            SearchAreas = new SearchAreaEnsamble(grindingAreas);
        }
        
        private WowInterface WowInterface { get; }
        
        private int WantedLevel { get;  }

        private SearchAreaEnsamble SearchAreas { get; }

        private WowUnit WowUnit { get; set; }

        public bool Finished => WowInterface.ObjectManager.Player.Level >= WantedLevel;

        public double Progress =>
            100.0 * (WowInterface.ObjectManager.Player.Level +
                     WowInterface.ObjectManager.Player.XpPercentage / 100.0) / WantedLevel;
        public void Execute()
        {
            // TODO: This is a lot of shared code with KillAndLoot!
            if (Finished || WowInterface.ObjectManager.Player.IsCasting) { return; }

            if (WowInterface.ObjectManager.Target != null
                && !WowInterface.ObjectManager.Target.IsDead
                && !WowInterface.ObjectManager.Target.IsNotAttackable
                && WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target) != WowUnitReaction.Friendly)
            {
                WowUnit = WowInterface.ObjectManager.Target;
            }
            else if (SearchAreas.IsPlayerNearSearchArea(WowInterface))
            {
                WowInterface.HookManager.WowClearTarget();

                WowUnit = WowInterface.ObjectManager.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => !e.IsDead && WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Friendly)
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                    .FirstOrDefault();
            }

            if (WowUnit != null)
            {
                SearchAreas.NotifyDetour();
                // TODO: Distance depending on CombatClass
                if (WowUnit.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 3.0)
                {
                    WowInterface.HookManager.WowStopClickToMove();
                    WowInterface.MovementEngine.Reset();
                    WowInterface.HookManager.WowUnitRightClick(WowUnit);
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, WowUnit.Position);
                }
            }
            else if (WowInterface.MovementEngine.IsAtTargetPosition || SearchAreas.HasAbortedPath())
            {
                Debug.WriteLine("GOT HERE!");
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving,
                    SearchAreas.GetNextPosition(WowInterface));
            }
        }
    }
}
