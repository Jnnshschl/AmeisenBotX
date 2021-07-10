using AmeisenBotX.Core;
using AmeisenBotX.Core.Engines.Character.Inventory.Objects;
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
            else if (WowItem.GetType() == typeof(WowReagent)) { labelIcon.Content = "🧪"; }
            else if (WowItem.GetType() == typeof(WowRecipe)) { labelIcon.Content = "📜"; }
            else if (WowItem.GetType() == typeof(WowTradegood)) { labelIcon.Content = "📦"; }
            else if (WowItem.GetType() == typeof(WowMiscellaneousItem)) { labelIcon.Content = "📦"; }
            else { labelIcon.Content = "❓"; }

            labelItemType.Content = $"{WowItem.Type} - {WowItem.Subtype} - iLvl {WowItem.ItemLevel} - {WowItem.Durability}/{WowItem.MaxDurability}";

            labelItemName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(WowItem.ItemQuality.GetColor()));
        }
    }
}