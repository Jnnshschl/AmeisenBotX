using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class CollectQuestObjective(AmeisenBotInterfaces bot, int itemId, int itemAmount, List<int> gameObjectIds, List<Vector3> positions) : IQuestObjective
    {
        public List<AreaNode> Area { get; set; } = positions.Select(pos => new AreaNode(pos, 10.0)).ToList();

        public bool Finished => Math.Abs(Progress - 100.0) < 0.0001;

        public double Progress
        {
            get
            {
                if (WantedItemAmount == 0)
                {
                    return 100.0;
                }

                IWowInventoryItem inventoryItem = Bot.Character.Inventory.Items.Find(item => item.Id == ItemId);
                return inventoryItem != null ? Math.Min(100.0 * ((float)inventoryItem.Count) / ((float)WantedItemAmount), 100.0) : 0.0;
            }
        }

        private AmeisenBotInterfaces Bot { get; } = bot;

        private List<int> GameObjectIds { get; } = gameObjectIds;

        private int ItemId { get; } = itemId;

        private TimegatedEvent RightClickEvent { get; } = new(TimeSpan.FromMilliseconds(1500));

        private int WantedItemAmount { get; } = itemAmount;

        public void Execute()
        {
            if (Finished) { return; }

            IWowGameobject lootableObject = Bot.Objects.All.OfType<IWowGameobject>()
                .Where(e => GameObjectIds.Contains(e.EntryId))
                .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                .FirstOrDefault();

            if (lootableObject != null)
            {
                if (lootableObject.Position.GetDistance(Bot.Player.Position) > 5.0)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, lootableObject.Position);
                }
                else
                {
                    if (RightClickEvent.Run())
                    {
                        Bot.Movement.Reset();
                        Bot.Wow.StopClickToMove();
                        Bot.Wow.InteractWithObject(lootableObject);
                    }
                }
            }
            else
            {
                AreaNode selectedArea = Area
                    .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                    .FirstOrDefault(e => e.Position.GetDistance(Bot.Player.Position) < e.Radius);

                if (selectedArea != null)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, selectedArea.Position);
                }
            }
        }
    }
}