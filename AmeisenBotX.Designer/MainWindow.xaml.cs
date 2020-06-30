using System.Windows;
using System.Windows.Input;

namespace AmeisenBotX.Designer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBehaviorTree_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            new BehaviorTreeDesigner().ShowDialog();
            Close();
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