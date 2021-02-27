using AmeisenBotX.Core;
using AmeisenBotX.Core.Personality.Objects;
using AmeisenBotX.Views;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace AmeisenBotX
{
    public partial class RelationshipWindow : Window
    {
        public RelationshipWindow(AmeisenBot ameisenBot)
        {
            AmeisenBot = ameisenBot;
            InitializeComponent();
        }

        private AmeisenBot AmeisenBot { get; set; }

        private void ButtonEquipment_Click(object sender, RoutedEventArgs e)
        {
            DisplayStuff();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            DisplayStuff();
        }

        private void DisplayStuff()
        {
            relationshipWrapPanel.Children.Clear();

            foreach (KeyValuePair<ulong, Relationship> kv in AmeisenBot.WowInterface.Db.AllPlayerRelationships())
            {
                relationshipWrapPanel.Children.Add(new RelationshipDisplay(AmeisenBot.WowInterface, kv.Key, kv.Value));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayStuff();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}