using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
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

        private AmeisenBotInterfaces Bot { get; }

        private int CurrentItemAmount => Bot.Character.Inventory.Items.Count(e => e.Id == ItemId);

        private int ItemId { get; }

        private int ObjectDisplayId { get; }

        private TimegatedEvent RightClickEvent { get; }

        private int WantedItemAmount { get; }

        public void Execute()
        {
            if (Finished) { return; }

            IWowGameobject lootableObject = Bot.Objects.WowObjects.OfType<IWowGameobject>()
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