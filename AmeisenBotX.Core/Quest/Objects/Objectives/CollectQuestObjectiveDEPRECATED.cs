﻿using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class CollectQuestObjectiveDEPRECATED : IQuestObjective
    {
        public CollectQuestObjectiveDEPRECATED(AmeisenBotInterfaces bot, int itemId, int itemAmount, int objectDisplayId, List<AreaNode> area)
        {
            Bot = bot;
            ItemId = itemId;
            WantedItemAmount = itemAmount;
            ObjectDisplayId = objectDisplayId;
            Area = area;

            RightClickEvent = new(TimeSpan.FromSeconds(1));
        }

        public List<AreaNode> Area { get; set; }

        public bool Finished => Progress == 100.0;

        public double Progress => Math.Round(CurrentItemAmount / (double)WantedItemAmount * 100.0, 1);

        private int CurrentItemAmount => Bot.Character.Inventory.Items.Count(e => e.Id == ItemId);

        private int ItemId { get; }

        private int ObjectDisplayId { get; }

        private TimegatedEvent RightClickEvent { get; }

        private int WantedItemAmount { get; }

        private AmeisenBotInterfaces Bot { get; }

        public void Execute()
        {
            if (Finished) { return; }

            WowGameobject lootableObject = Bot.Objects.WowObjects.OfType<WowGameobject>()
                .Where(e => e.DisplayId == ObjectDisplayId)
                .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                .FirstOrDefault();

            if (lootableObject != null)
            {
                if (lootableObject.Position.GetDistance(Bot.Player.Position) > 3.0)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, lootableObject.Position);
                }
                else
                {
                    if (RightClickEvent.Run())
                    {
                        Bot.Movement.Reset();
                        Bot.Wow.WowStopClickToMove();
                        Bot.Wow.WowObjectRightClick(lootableObject.BaseAddress);
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