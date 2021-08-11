using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class CollectQuestObjective : IQuestObjective
    {
        public CollectQuestObjective(AmeisenBotInterfaces bot, int itemId, int itemAmount, List<int> gameObjectIds, List<Vector3> positions)
        {
            Bot = bot;
            ItemId = itemId;
            WantedItemAmount = itemAmount;
            GameObjectIds = gameObjectIds;
            Area = positions.Select(pos => new AreaNode(pos, 10.0)).ToList();
            RightClickEvent = new(TimeSpan.FromMilliseconds(1500));
        }

        public List<AreaNode> Area { get; set; }

        public bool Finished => Math.Abs(Progress - 100.0) < 0.0001;

        public double Progress
        {
            get
            {
                if (WantedItemAmount == 0)
                {
                    return 100.0;
                }

                Character.Inventory.Objects.IWowInventoryItem inventoryItem = Bot.Character.Inventory.Items.Find(item => item.Id == ItemId);
                return inventoryItem != null ? Math.Min(100.0 * ((float)inventoryItem.Count) / ((float)WantedItemAmount), 100.0) : 0.0;
            }
        }

        private AmeisenBotInterfaces Bot { get; }

        private List<int> GameObjectIds { get; }

        private int ItemId { get; }

        private TimegatedEvent RightClickEvent { get; }

        private int WantedItemAmount { get; }

        public void Execute()
        {
            if (Finished) { return; }

            IWowGameobject lootableObject = Bot.Objects.WowObjects.OfType<IWowGameobject>()
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
                        Bot.Wow.InteractWithObject(lootableObject.BaseAddress);
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