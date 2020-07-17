using AmeisenBotX.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AmeisenBotX.StateConfig
{
    public partial class StateJobConfigWindow : Window, IStateConfigWindow
    {
        public StateJobConfigWindow(AmeisenBot ameisenBot, AmeisenBotConfig config)
        {
            AmeisenBot = ameisenBot;
            Config = config;
            InitializeComponent();
        }

        public bool ChangedSomething { get; set; }

        public AmeisenBotConfig Config { get; }

        public bool ShouldSave { get; set; }

        public bool WindowLoaded { get; private set; }

        private AmeisenBot AmeisenBot { get; }

        private void AddProfiles()
        {
            comboboxProfile.Items.Add("None");

            for (int i = 0; i < AmeisenBot.JobProfiles.Count; ++i)
            {
                comboboxProfile.Items.Add(AmeisenBot.JobProfiles[i].ToString());
            }

            comboboxProfile.SelectedIndex = 0;
        }

        private void ButtonDone_Click(object sender, RoutedEventArgs e)
        {
            Config.JobProfile = comboboxProfile.Text;

            ShouldSave = true;
            Hide();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            // if (ChangedSomething)
            // {
            //     ConfirmWindow confirmWindow = new ConfirmWindow("Unsaved Changes!", "Are you sure that you wan't to cancel?", "Yes", "No");
            //     confirmWindow.ShowDialog();
            //
            //     if (!confirmWindow.OkayPressed)
            //     {
            //         return;
            //     }
            // }

            Hide();
        }

        private void ComboboxProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WindowLoaded)
            {
                ChangedSomething = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowLoaded = true;

            AddProfiles();

            if (!string.IsNullOrEmpty(Config.JobProfile))
            {
                comboboxProfile.Text = Config.JobProfile;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}