using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AmeisenBotX.Views
{
    public partial class ItemDisplay : UserControl
    {
        public ItemDisplay(IWowInventoryItem wowItem)
        {
            WowItem = wowItem;
            InitializeComponent();
        }

        private IWowInventoryItem WowItem { get; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            labelItemName.Content = WowItem.Name;
            labelItemId.Content = WowItem.Id;

            if (WowItem.GetType() == typeof(WowWeapon)) { labelIcon.Content = "🗡"; }
            else if (WowItem.GetType() == typeof(WowArmor)) { labelIcon.Content = "🛡"; }
            else if (WowItem.GetType() == typeof(WowConsumable)) { labelIcon.Content = "🍏"; }
            else if (WowItem.GetType() == typeof(WowContainer)) { labelIcon.Content = "🎒"; }
            else if (WowItem.GetType() == typeof(WowGem)) { labelIcon.Content = "💎"; }
            else if (WowItem.GetType() == typeof(WowKey)) { labelIcon.Content = "🗝️"; }
            else if (WowItem.GetType() == typeof(WowMoneyItem)) { labelIcon.Content = "💰"; }
            else if (WowItem.GetType() == typeof(WowProjectile) || WowItem.GetType() == typeof(WowQuiver)) { labelIcon.Content = "🏹"; }
            else if (WowItem.GetType() == typeof(WowQuestItem)) { labelIcon.Content = "💡"; }
            else
            {
                labelIcon.Content = WowItem.GetType() == typeof(WowReagent)
                ? "🧪"
                : WowItem.GetType() == typeof(WowRecipe)
                ? "📜"
                : WowItem.GetType() == typeof(WowTradeGoods) ? "📦" : WowItem.GetType() == typeof(WowMiscellaneousItem) ? "📦" : (object)"❓";
            }

            labelItemType.Content = $"{WowItem.Type} - {WowItem.Subtype} - iLvl {WowItem.ItemLevel} - {WowItem.Durability}/{WowItem.MaxDurability}";

            labelItemName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(((WowItemQuality)WowItem.ItemQuality).GetColor()));
        }
    }
}