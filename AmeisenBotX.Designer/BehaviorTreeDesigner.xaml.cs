using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AmeisenBotX.Designer
{
    public partial class BehaviorTreeDesigner : Window
    {
        public BehaviorTreeDesigner()
        {
            InitializeComponent();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}