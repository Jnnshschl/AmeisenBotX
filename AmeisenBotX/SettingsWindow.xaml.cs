using AmeisenBotX.Core;
using AmeisenBotX.Views;
using System.Windows;
using System.Windows.Input;

namespace AmeisenBotX
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(AmeisenBotConfig config)
        {
            Config = config;
            PendingChanges = false;

            InitializeComponent();
        }

        public AmeisenBotConfig Config { get; private set; }

        private bool PendingChanges { get; set; }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            if (PendingChanges)
            {
                ConfirmWindow confirmWindow = new ConfirmWindow("Unsaved Changes!", "Are you sure that you want to exit without saving?", "Exit without saving", "Cancel");
                confirmWindow.ShowDialog();

                if (confirmWindow.OkayPressed)
                {
                    Close();
                }
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => DragMove();
    }
}
