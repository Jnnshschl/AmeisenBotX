using AmeisenBotX.Core.Character.Inventory.Objects;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool UseItemQuestObjectiveCondition();

    public class UseItemQuestObjective : IQuestObjective
    {
        public UseItemQuestObjective(WowInterface wowInterface, int itemId, UseItemQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            ItemId = itemId;
            Condition = condition;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private UseItemQuestObjectiveCondition Condition { get; }

        private int ItemId { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished || WowInterface.Player.IsCasting) { return; }

            IWowItem item = WowInterface.CharacterManager.Inventory.Items.FirstOrDefault(e => e.Id == ItemId);

            if (item != null)
            {
                WowInterface.MovementEngine.Reset();
                WowInterface.NewWowInterface.WowStopClickToMove();
                WowInterface.NewWowInterface.LuaUseContainerItem(item.BagId, item.BagSlot);
            }
        }
    }
}