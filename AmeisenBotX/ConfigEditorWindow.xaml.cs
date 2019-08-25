using AmeisenBotX.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AmeisenBotX
{
    /// <summary>
    /// Interaktionslogik für ConfigEditorWindow.xaml
    /// </summary>
    public partial class ConfigEditorWindow : Window
    {
        public string ConfigName { get; private set; }
        public AmeisenBotConfig Config { get; private set; }

        public ConfigEditorWindow()
        {
            InitializeComponent();
        }

        private void ButtonOkay_Click(object sender, RoutedEventArgs e)
        {
            ConfigName = textboxConfigName.Text;
            Config = new AmeisenBotConfig()
            {
                Username = textboxUsername.Text,
                Password = textboxPassword.Password,
                CharacterSlot = int.Parse(textboxCharacterSlot.Text),
                CombatClassName = textboxCombatClass.Text,
                AutostartWow = checkboxAutoStartWow.IsChecked.GetValueOrDefault(false),
                AutoLogin = checkboxAutoLogin.IsChecked.GetValueOrDefault(false),
                FollowGroupLeader = checkboxFollowGroupLeader.IsChecked.GetValueOrDefault(false),
                FollowGroupMembers = checkboxGroupMembers.IsChecked.GetValueOrDefault(false),
                FollowSpecificCharacter = checkboxFollowSpecificCharacter.IsChecked.GetValueOrDefault(false),
                PermanentNameCache = checkboxPermanentNameCache.IsChecked.GetValueOrDefault(false),
                PermanentReactionCache = checkboxPermanentReactionCache.IsChecked.GetValueOrDefault(false),
                ReleaseSpirit = checkboxReleaseSpirit.IsChecked.GetValueOrDefault(false),
                SaveWowWindowPosition = checkboxSaveWowWindowPosition.IsChecked.GetValueOrDefault(false),
                SaveBotWindowPosition = checkboxSaveBotWindowPosition.IsChecked.GetValueOrDefault(false),
                PathToWowExe = textboxWowPath.Text,
                SpecificCharacterToFollow = textboxFollowSpecificCharacterName.Text
            };

            Close();
        }

        private void CheckboxAutoStartWow_Checked(object sender, RoutedEventArgs e)
        {
            textboxWowPath.IsEnabled = true;
            buttonOpenWowExe.IsEnabled = true;
        }

        private void CheckboxAutoStartWow_Unchecked(object sender, RoutedEventArgs e)
        {
            textboxWowPath.IsEnabled = false;
            textboxWowPath.Text = "";
            buttonOpenWowExe.IsEnabled = false;
        }

        private void CheckboxFollowSpecificCharacter_Checked(object sender, RoutedEventArgs e)
        {
            textboxFollowSpecificCharacterName.IsEnabled = true;
        }

        private void CheckboxFollowSpecificCharacter_Unchecked(object sender, RoutedEventArgs e)
        {
            textboxFollowSpecificCharacterName.IsEnabled = false;
            textboxFollowSpecificCharacterName.Text = "";
        }

        private void ButtonAbort_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonOpenWowExe_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Wow Executeable|*.exe"
            };

            if (openFileDialog.ShowDialog().GetValueOrDefault(false))
            {
                textboxWowPath.Text = openFileDialog.FileName;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}
