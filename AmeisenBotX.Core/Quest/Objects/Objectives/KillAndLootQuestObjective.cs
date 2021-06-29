using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Wow.Combatlog.Enums;
using AmeisenBotX.Wow.Combatlog.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class KillAndLootQuestObjective : IQuestObjective
    {
        public KillAndLootQuestObjective(WowInterface wowInterface, List<int> npcIds, int collectOrKillAmount, int questItemId, List<List<Vector3>> areas)
        {
            WowInterface = wowInterface;
            NpcIds = npcIds;
            CollectOrKillAmount = collectOrKillAmount;
            QuestItemId = questItemId;
            SearchAreas = new SearchAreaEnsamble(areas);

            if (!CollectQuestItem)
            {
                wowInterface.Db.GetCombatLogSubject().Register(this);
            }
        }

        public bool Finished => Math.Abs(Progress - 100.0f) < 0.00001;

        public double Progress
        {
            get
            {
                if (CollectOrKillAmount == 0)
                {
                    return 100.0;
                }

                int amount = Killed;
                if (CollectQuestItem)
                {
                    Character.Inventory.Objects.IWowItem inventoryItem =
                        WowInterface.CharacterManager.Inventory.Items.Find(item => item.Id == QuestItemId);
                    if (inventoryItem != null)
                    {
                        amount = inventoryItem.Count;
                    }
                    else
                    {
                        return 0.0;
                    }
                }

                return Math.Min(100.0 * ((float)amount) / ((float)CollectOrKillAmount), 100.0);
            }
        }

        private int CollectOrKillAmount { get; }

        private bool CollectQuestItem => QuestItemId > 0;

        private Vector3 CurrentSpot { get; set; }

        private int Killed { get; set; }

        private DateTime LastUnitCheck { get; set; } = DateTime.UtcNow;

        private List<int> NpcIds { get; }

        private int QuestItemId { get; }

        private SearchAreaEnsamble SearchAreas { get; }

        private WowInterface WowInterface { get; }

        private WowUnit WowUnit { get; set; }

        public void CombatLogChanged(BasicCombatLogEntry entry)
        {
            WowUnit wowUnit = WowInterface.Objects.GetWowObjectByGuid<WowUnit>(entry.DestinationGuid);
            if (entry.Subtype == CombatLogEntrySubtype.KILL && NpcIds.Contains(WowGuid.ToNpcId(entry.DestinationGuid))
                                                            && wowUnit != null && wowUnit.IsTaggedByMe)
            {
                ++Killed;
            }
        }

        public void Execute()
        {
            if (Finished || WowInterface.Player.IsCasting) { return; }

            if (!WowInterface.Player.IsInCombat && DateTime.UtcNow.Subtract(LastUnitCheck).TotalMilliseconds >= 1250.0)
            {
                LastUnitCheck = DateTime.UtcNow;
                WowUnit = WowInterface.Objects.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => !e.IsDead && NpcIds.Contains(WowGuid.ToNpcId(e.Guid)) && !e.IsNotAttackable
                                && WowInterface.Db.GetReaction(WowInterface.Player, e) != WowUnitReaction.Friendly)
                    .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                    .Take(3)
                    .OrderBy(e => WowInterface.PathfindingHandler.GetPathDistance((int)WowInterface.Objects.MapId, WowInterface.Player.Position, e.Position))
                    .FirstOrDefault();

                // Kill enemies in the path
                if (WowUnit != null && WowInterface.Db.GetReaction(WowInterface.Player, WowUnit) == WowUnitReaction.Hostile)
                {
                    IEnumerable<Vector3> path = WowInterface.PathfindingHandler.GetPath((int)WowInterface.Objects.MapId,
                    WowInterface.Player.Position, WowUnit.Position);

                    if (path != null)
                    {
                        IEnumerable<WowUnit> nearEnemies = WowInterface.Objects.GetEnemiesInPath<WowUnit>(WowInterface.Db.GetReaction, path, 10.0f);

                        if (nearEnemies.Any())
                        {
                            WowUnit = nearEnemies.FirstOrDefault();
                        }
                    }
                }

                if (WowUnit != null)
                {
                    WowInterface.NewWowInterface.WowTargetGuid(WowUnit.Guid);
                }
            }

            if (WowUnit != null)
            {
                SearchAreas.NotifyDetour();
                WowInterface.CombatClass.AttackTarget();
            }
            else if (WowInterface.Player.Position.GetDistance(CurrentSpot) < 3.0f || SearchAreas.HasAbortedPath() || WowInterface.MovementEngine.Status == MovementAction.None)
            {
                CurrentSpot = SearchAreas.GetNextPosition(WowInterface);
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, CurrentSpot);
            }
        }
    }
}