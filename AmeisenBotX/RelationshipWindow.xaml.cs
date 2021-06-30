using AmeisenBotX.Core;
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