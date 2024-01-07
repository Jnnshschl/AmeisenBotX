using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class UseItemQuestObjective(AmeisenBotInterfaces bot, int itemId, Func<bool> condition) : IQuestObjective
    {
        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; } = bot;

        private Func<bool> Condition { get; } = condition;

        private int ItemId { get; } = itemId;

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