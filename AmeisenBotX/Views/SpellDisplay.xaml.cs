using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using System.Windows;
using System.Windows.Controls;

namespace AmeisenBotX.Views
{
    public partial class SpellDisplay : UserControl
    {
        public SpellDisplay(Spell spell)
        {
            Spell = spell;
            InitializeComponent();
        }

        private Spell Spell { get; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            labelSpellName.Content = Spell.Name;
            labelSpellRank.Content = Spell.Rank;

            // labelIcon.Content = "❓";

            labelItemType.Content = $"{Spell.SpellbookName} - {Spell.Costs} - {Spell.CastTime}s - {Spell.MinRange}m => {Spell.MaxRange}m";

            // labelSpellName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BotUtils.GetColorByQuality(WowItem.ItemQuality)));
        }
    }
}