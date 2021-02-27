using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class CollectQuestObjectiveDEPRECATED : IQuestObjective
    {
        public CollectQuestObjectiveDEPRECATED(WowInterface wowInterface, int itemId, int itemAmount, int objectDisplayId, List<AreaNode> area)
        {
            WowInterface = wowInterface;
            ItemId = itemId;
            WantedItemAmount = itemAmount;
            ObjectDisplayId = objectDisplayId;
            Area = area;

            RightClickEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
        }

        public List<AreaNode> Area { get; set; }

        public bool Finished => Progress == 100.0;

        public double Progress => Math.Round((double)CurrentItemAmount / (double)WantedItemAmount * 100.0, 1);

        private int CurrentItemAmount => WowInterface.CharacterManager.Inventory.Items.Count(e => e.Id == ItemId);

        private int ItemId { get; }

        private int ObjectDisplayId { get; }

        private TimegatedEvent RightClickEvent { get; }

        private int WantedItemAmount { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished) { return; }

            WowGameobject lootableObject = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                .Where(e => e.DisplayId == ObjectDisplayId)
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .FirstOrDefault();

            if (lootableObject != null)
            {
                if (lootableObject.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 3.0)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, lootableObject.Position);
                }
                else
                {
                    if (RightClickEvent.Run())
                    {
                        WowInterface.MovementEngine.Reset();
                        WowInterface.HookManager.WowStopClickToMove();
                        WowInterface.HookManager.WowObjectRightClick(lootableObject);
                    }
                }
            }
            else
            {
                AreaNode selectedArea = Area
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                    .FirstOrDefault(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < e.Radius);

                if (selectedArea != null)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, selectedArea.Position);
                }
            }
        }
    }
}