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
            PendingChanges = true;

            InitializeComponent();
        }

        public AmeisenBotConfig Config { get; private set; }

        private bool PendingChanges { get; set; }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => DragMove();

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            if (PendingChanges)
            {
                ConfirmWindow confirmWindow = new ConfirmWindow("Unsaved Changes!", "Are you sure that you wan't to exit without saving?", "Exit Wihtout Saving", "Cancel");
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
    }
}