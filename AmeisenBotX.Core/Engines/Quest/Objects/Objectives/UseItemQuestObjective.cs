using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public delegate bool UseItemQuestObjectiveCondition();

    public class UseItemQuestObjective : IQuestObjective
    {
        public UseItemQuestObjective(AmeisenBotInterfaces bot, int itemId, UseItemQuestObjectiveCondition condition)
        {
            Bot = bot;
            ItemId = itemId;
            Condition = condition;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private UseItemQuestObjectiveCondition Condition { get; }

        private int ItemId { get; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            IWowInventoryItem item = Bot.Character.Inventory.Items.FirstOrDefault(e => e.Id == ItemId);

            if (item != null)
            {
                Bot.Movement.Reset();
                Bot.Wow.StopClickToMove();
                Bot.Wow.UseContainerItem(item.BagId, item.BagSlot);
            }
        }
    }
}