using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Movement.Pathfinding.Objects;
using AmeisenBotX.Wow.Combatlog.Enums;
using AmeisenBotX.Wow.Combatlog.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class KillAndLootQuestObjective : IQuestObjective, IObserverBasicCombatLogEntry
    {
        public KillAndLootQuestObjective(AmeisenBotInterfaces bot, List<int> npcIds, int collectOrKillAmount, int questItemId, List<List<Vector3>> areas)
        {
            Bot = bot;
            NpcIds = npcIds;
            CollectOrKillAmount = collectOrKillAmount;
            QuestItemId = questItemId;
            SearchAreas = new SearchAreaEnsamble(areas);

            if (!CollectQuestItem)
            {
                bot.Db.GetCombatLogSubject().Register(this);
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
                    Character.Inventory.Objects.IWowInventoryItem inventoryItem =
                        Bot.Character.Inventory.Items.Find(item => item.Id == QuestItemId);
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

        private AmeisenBotInterfaces Bot { get; }

        private int CollectOrKillAmount { get; }

        private bool CollectQuestItem => QuestItemId > 0;

        private Vector3 CurrentSpot { get; set; }

        private IWowUnit IWowUnit { get; set; }

        private int Killed { get; set; }

        private DateTime LastUnitCheck { get; set; } = DateTime.UtcNow;

        private List<int> NpcIds { get; }

        private int QuestItemId { get; }

        private SearchAreaEnsamble SearchAreas { get; }

        public void CombatLogChanged(BasicCombatLogEntry entry)
        {
            IWowUnit wowUnit = Bot.GetWowObjectByGuid<IWowUnit>(entry.DestinationGuid);
            if (entry.Subtype == CombatLogEntrySubtype.KILL && NpcIds.Contains(BotUtils.GuidToNpcId(entry.DestinationGuid))
                                                            && wowUnit != null && wowUnit.IsTaggedByMe)
            {
                ++Killed;
            }
        }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            if (!Bot.Player.IsInCombat && DateTime.UtcNow.Subtract(LastUnitCheck).TotalMilliseconds >= 1250.0)
            {
                LastUnitCheck = DateTime.UtcNow;
                IWowUnit = Bot.Objects.WowObjects
                    .OfType<IWowUnit>()
                    .Where(e => !e.IsDead && NpcIds.Contains(BotUtils.GuidToNpcId(e.Guid)) && !e.IsNotAttackable
                                && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Friendly)
                    .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                    .Take(3)
                    .OrderBy(e => Bot.PathfindingHandler.GetPathDistance((int)Bot.Objects.MapId, Bot.Player.Position, e.Position))
                    .FirstOrDefault();

                // Kill enemies in the path
                if (IWowUnit != null && Bot.Db.GetReaction(Bot.Player, IWowUnit) == WowUnitReaction.Hostile)
                {
                    IEnumerable<Vector3> path = Bot.PathfindingHandler.GetPath((int)Bot.Objects.MapId,
                    Bot.Player.Position, IWowUnit.Position);

                    if (path != null)
                    {
                        IEnumerable<IWowUnit> nearEnemies = Bot.GetEnemiesInPath<IWowUnit>(path, 10.0f);

                        if (nearEnemies.Any())
                        {
                            IWowUnit = nearEnemies.FirstOrDefault();
                        }
                    }
                }

                if (IWowUnit != null)
                {
                    Bot.Wow.WowTargetGuid(IWowUnit.Guid);
                }
            }

            if (IWowUnit != null)
            {
                SearchAreas.NotifyDetour();
                Bot.CombatClass.AttackTarget();
            }
            else if (Bot.Player.Position.GetDistance(CurrentSpot) < 3.0f || SearchAreas.HasAbortedPath() || Bot.Movement.Status == MovementAction.None)
            {
                CurrentSpot = SearchAreas.GetNextPosition(Bot);
                Bot.Movement.SetMovementAction(MovementAction.Move, CurrentSpot);
            }
        }
    }
}