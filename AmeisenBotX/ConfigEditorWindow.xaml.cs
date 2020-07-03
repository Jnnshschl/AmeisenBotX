using AmeisenBotX.Core;
using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Views;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AmeisenBotX
{
    public partial class ConfigEditorWindow : Window
    {
        public SolidColorBrush errorBorderBrush;
        public SolidColorBrush normalBorderBrush;

        public ConfigEditorWindow(string dataDir, AmeisenBot ameisenBot, AmeisenBotConfig initialConfig = null, string initialConfigName = "")
        {
            InitializeComponent();

            DataDir = dataDir;
            NewConfig = initialConfig == null;
            Config = initialConfig ?? new AmeisenBotConfig();
            AmeisenBot = ameisenBot;
            ConfigName = initialConfigName;

            SaveConfig = NewConfig;

            normalBorderBrush = new SolidColorBrush((Color)FindResource("DarkBorder"));
            errorBorderBrush = new SolidColorBrush((Color)FindResource("DarkError"));
        }

        public AmeisenBot AmeisenBot { get; private set; }

        public bool Cancel { get; set; }

        public AmeisenBotConfig Config { get; private set; }

        public string ConfigName { get; private set; }

        public string DataDir { get; }

        public bool NewConfig { get; }

        public bool SaveConfig { get; private set; }

        public bool WindowLoaded { get; set; }

        private void AddBattlegroundEngines()
        {
            comboboxBattlegroundEngine.Items.Add("None");

            for (int i = 0; i < AmeisenBot.BattlegroundEngines.Count; ++i)
            {
                comboboxBattlegroundEngine.Items.Add(AmeisenBot.BattlegroundEngines[i].ToString());
            }

            comboboxBattlegroundEngine.SelectedIndex = 0;
        }

        private void AddCombatClasses()
        {
            comboboxBuiltInCombatClass.Items.Add("None");

            for (int i = 0; i < AmeisenBot.CombatClasses.Count; ++i)
            {
                comboboxBuiltInCombatClass.Items.Add(AmeisenBot.CombatClasses[i].ToString());
            }

            comboboxBuiltInCombatClass.SelectedIndex = 0;
        }

        private void ButtonDone_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateFields())
            {
                ConfigName = textboxConfigName.Text.Trim();
                Config = new AmeisenBotConfig()
                {
                    AutocloseWow = checkboxAutocloseWow.IsChecked.GetValueOrDefault(false),
                    AutoDodgeAoeSpells = checkboxAvoidAoe.IsChecked.GetValueOrDefault(false),
                    AutoLogin = checkboxAutoLogin.IsChecked.GetValueOrDefault(false),
                    AutoPositionWow = checkboxAutoPositionWow.IsChecked.GetValueOrDefault(false),
                    AutostartWow = checkboxAutoStartWow.IsChecked.GetValueOrDefault(false),
                    BattlegroundEngine = comboboxBattlegroundEngine.SelectedItem != null ? comboboxBattlegroundEngine.SelectedItem.ToString() : string.Empty,
                    BuiltInCombatClassName = comboboxBuiltInCombatClass.SelectedItem != null ? comboboxBuiltInCombatClass.SelectedItem.ToString() : string.Empty,
                    CharacterSlot = int.Parse(textboxCharacterSlot.Text),
                    CustomCombatClassFile = textboxCombatClassFile.Text,
                    FollowGroupLeader = checkboxFollowGroupLeader.IsChecked.GetValueOrDefault(false),
                    FollowGroupMembers = checkboxGroupMembers.IsChecked.GetValueOrDefault(false),
                    FollowSpecificCharacter = checkboxFollowSpecificCharacter.IsChecked.GetValueOrDefault(false),
                    LootUnits = checkboxLooting.IsChecked.GetValueOrDefault(false),
                    LootUnitsRadius = Math.Round(sliderLootRadius.Value),
                    MaxFollowDistance = (int)Math.Round(sliderMaxFollowDistance.Value),
                    MaxFps = (int)Math.Round(sliderMaxFps.Value),
                    MaxFpsCombat = (int)Math.Round(sliderMaxFpsCombat.Value),
                    MinFollowDistance = (int)Math.Round(sliderMinFollowDistance.Value),
                    NameshServerPort = int.Parse(textboxNavmeshServerPort.Text),
                    NavmeshServerIp = textboxNavmeshServerIp.Text,
                    Password = textboxPassword.Password,
                    PathToWowExe = textboxWowPath.Text,
                    PermanentNameCache = checkboxPermanentNameCache.IsChecked.GetValueOrDefault(false),
                    PermanentReactionCache = checkboxPermanentReactionCache.IsChecked.GetValueOrDefault(false),
                    ReleaseSpirit = checkboxReleaseSpirit.IsChecked.GetValueOrDefault(false),
                    SaveBotWindowPosition = checkboxSaveBotWindowPosition.IsChecked.GetValueOrDefault(false),
                    SaveWowWindowPosition = checkboxSaveWowWindowPosition.IsChecked.GetValueOrDefault(false),
                    SpecificCharacterToFollow = textboxFollowSpecificCharacterName.Text,
                    UseBuiltInCombatClass = checkboxBuiltinCombatClass.IsChecked.GetValueOrDefault(true),
                    Username = textboxUsername.Text
                };

                SaveConfig = true;
                Close();
            }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            ConfirmWindow confirmWindow = new ConfirmWindow("Unsaved Changes!", "Are you sure that you wan't to cancel?", "Yes", "No");
            confirmWindow.ShowDialog();

            if (confirmWindow.OkayPressed)
            {
                Cancel = true;
                Close();
            }
        }

        private void ButtonOpenCombatClassFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "C# Files|*.cs"
            };

            if (openFileDialog.ShowDialog().GetValueOrDefault(false))
            {
                textboxCombatClassFile.Text = openFileDialog.FileName;
            }
        }

        private void ButtonOpenWowExe_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "WoW Executeable|*.exe"
            };

            if (openFileDialog.ShowDialog().GetValueOrDefault(false))
            {
                textboxWowPath.Text = openFileDialog.FileName;
            }
        }

        private void CheckboxAutoStartWow_Checked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                textboxWowPath.IsEnabled = true;
                buttonOpenWowExe.IsEnabled = true;
            }
        }

        private void CheckboxAutoStartWow_Unchecked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                textboxWowPath.IsEnabled = false;
                textboxWowPath.Text = string.Empty;
                buttonOpenWowExe.IsEnabled = false;
            }
        }

        private void CheckboxBuiltinCombatClass_Checked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                buttonOpenCombatClassFile.Visibility = Visibility.Hidden;
                textboxCombatClassFile.Visibility = Visibility.Hidden;
                comboboxBuiltInCombatClass.Visibility = Visibility.Visible;
            }
        }

        private void CheckboxCustomCombatClass_Checked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                buttonOpenCombatClassFile.Visibility = Visibility.Visible;
                textboxCombatClassFile.Visibility = Visibility.Visible;
                comboboxBuiltInCombatClass.Visibility = Visibility.Hidden;
            }
        }

        private void CheckboxFollowSpecificCharacter_Checked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                textboxFollowSpecificCharacterName.IsEnabled = true;
            }
        }

        private void CheckboxFollowSpecificCharacter_Unchecked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                textboxFollowSpecificCharacterName.IsEnabled = false;
                textboxFollowSpecificCharacterName.Text = string.Empty;
            }
        }

        private void LoadConfigToUi()
        {
            checkboxAutocloseWow.IsChecked = Config.AutocloseWow;
            checkboxAutoLogin.IsChecked = Config.AutoLogin;
            checkboxAutoPositionWow.IsChecked = Config.AutoPositionWow;
            checkboxAutoStartWow.IsChecked = Config.AutostartWow;
            checkboxAvoidAoe.IsChecked = Config.AutoDodgeAoeSpells;
            checkboxBuiltinCombatClass.IsChecked = Config.UseBuiltInCombatClass;
            checkboxFollowGroupLeader.IsChecked = Config.FollowGroupLeader;
            checkboxFollowSpecificCharacter.IsChecked = Config.FollowSpecificCharacter;
            checkboxGroupMembers.IsChecked = Config.FollowGroupMembers;
            checkboxLooting.IsChecked = Config.LootUnits;
            checkboxPermanentNameCache.IsChecked = Config.PermanentNameCache;
            checkboxPermanentReactionCache.IsChecked = Config.PermanentReactionCache;
            checkboxReleaseSpirit.IsChecked = Config.ReleaseSpirit;
            checkboxSaveBotWindowPosition.IsChecked = Config.SaveBotWindowPosition;
            checkboxSaveWowWindowPosition.IsChecked = Config.SaveWowWindowPosition;
            comboboxBattlegroundEngine.Text = Config.BattlegroundEngine;
            comboboxBuiltInCombatClass.Text = Config.BuiltInCombatClassName;
            sliderLootRadius.Value = Math.Round(Config.LootUnitsRadius);
            sliderMaxFollowDistance.Value = Config.MaxFollowDistance;
            sliderMaxFps.Value = Config.MaxFps;
            sliderMaxFpsCombat.Value = Config.MaxFpsCombat;
            sliderMinFollowDistance.Value = Config.MinFollowDistance;
            textboxCharacterSlot.Text = Config.CharacterSlot.ToString();
            textboxCombatClassFile.Text = Config.CustomCombatClassFile;
            textboxFollowSpecificCharacterName.Text = Config.SpecificCharacterToFollow;
            textboxNavmeshServerIp.Text = Config.NavmeshServerIp;
            textboxNavmeshServerPort.Text = Config.NameshServerPort.ToString();
            textboxPassword.Password = Config.Password;
            textboxUsername.Text = Config.Username;
            textboxWowPath.Text = Config.PathToWowExe;
        }

        private void SliderLootRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelLootRadius.Content = $"Loot Radius: {Math.Round(e.NewValue)}m";
            }
        }

        private void SliderMaxFollowDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelMaxFollowDistance.Content = $"Max Follow Distance: {Math.Round(e.NewValue)}m";
            }
        }

        private void SliderMaxFps_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelMaxFps.Content = $"Max FPS: {Math.Round(e.NewValue)}";
            }
        }

        private void SliderMaxFpsCombat_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelMaxFpsCombat.Content = $"Max FPS Combat: {Math.Round(e.NewValue)}";
            }
        }

        private void SliderMinFollowDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelMinFollowDistance.Content = $"Min Follow Distance: {Math.Round(e.NewValue)}m";
            }
        }

        private bool ValidateAutoLogin(bool failed)
        {
            if (checkboxAutoLogin.IsChecked.GetValueOrDefault(false))
            {
                if (textboxUsername.Text.Length == 0)
                {
                    textboxUsername.BorderBrush = errorBorderBrush;
                    failed = true;
                }
                else
                {
                    textboxUsername.BorderBrush = normalBorderBrush;
                }

                if (textboxPassword.Password.Length == 0)
                {
                    textboxPassword.BorderBrush = errorBorderBrush;
                    failed = true;
                }
                else
                {
                    textboxPassword.BorderBrush = normalBorderBrush;
                }

                if (textboxCharacterSlot.Text.Length == 0
                    || !int.TryParse(textboxCharacterSlot.Text, out int charslot)
                    || charslot < 0
                    || charslot > 9)
                {
                    textboxCharacterSlot.BorderBrush = errorBorderBrush;
                    failed = true;
                }
                else
                {
                    textboxCharacterSlot.BorderBrush = normalBorderBrush;
                }
            }

            return failed;
        }

        private bool ValidateAutostartWow(bool failed)
        {
            if (checkboxAutoStartWow.IsChecked.GetValueOrDefault(false))
            {
                if (textboxWowPath.Text.Length == 0)
                {
                    textboxConfigName.BorderBrush = errorBorderBrush;
                    failed = true;
                }
                else
                {
                    textboxConfigName.BorderBrush = normalBorderBrush;
                }
            }

            return failed;
        }

        private bool ValidateConfigName(bool failed)
        {
            Regex regex = new Regex("^([a-zA-Z]:)?(\\\\[^<>:\"/\\\\|?*]+)+\\\\?$");

            if (textboxConfigName.Text.Length == 0
                || regex.IsMatch(textboxConfigName.Text))
            {
                textboxConfigName.BorderBrush = errorBorderBrush;
                failed = true;
            }
            else
            {
                textboxConfigName.BorderBrush = normalBorderBrush;
            }

            return failed;
        }

        private bool ValidateFields()
        {
            bool failed = false;
            failed = ValidateConfigName(failed);
            failed = ValidateAutoLogin(failed);
            failed = ValidateAutostartWow(failed);
            failed = ValidateSpecificFollow(failed);
            failed = ValidateNavmeshServer(failed);
            return !failed;
        }

        private bool ValidateNavmeshServer(bool failed)
        {
            if (textboxNavmeshServerIp.Text.Length == 0
                || !IPAddress.TryParse(textboxNavmeshServerIp.Text, out IPAddress _))
            {
                textboxNavmeshServerIp.BorderBrush = errorBorderBrush;
                failed = true;
            }
            else
            {
                textboxNavmeshServerIp.BorderBrush = normalBorderBrush;
            }

            if (textboxNavmeshServerPort.Text.Length == 0
                || !int.TryParse(textboxNavmeshServerPort.Text, out int port)
                || port < 0
                || port > ushort.MaxValue)
            {
                textboxNavmeshServerPort.BorderBrush = errorBorderBrush;
                failed = true;
            }
            else
            {
                textboxNavmeshServerPort.BorderBrush = normalBorderBrush;
            }

            return failed;
        }

        private bool ValidateSpecificFollow(bool failed)
        {
            if (checkboxFollowSpecificCharacter.IsChecked.GetValueOrDefault(false))
            {
                if (textboxFollowSpecificCharacterName.Text.Length == 0)
                {
                    textboxFollowSpecificCharacterName.BorderBrush = errorBorderBrush;
                    failed = true;
                }
                else
                {
                    textboxFollowSpecificCharacterName.BorderBrush = normalBorderBrush;
                }
            }

            return failed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowLoaded = true;

            if (!NewConfig)
            {
                textboxConfigName.IsEnabled = false;
                textboxConfigName.Text = ConfigName;
                labelHeader.Content = $"AmeisenBotX - {ConfigName}";

                AddCombatClasses();
                AddBattlegroundEngines();
            }
            else
            {
                buttonOpenCombatClassFile.Visibility = Visibility.Hidden;
                comboboxBuiltInCombatClass.Visibility = Visibility.Hidden;
                checkboxCustomCombatClass.Visibility = Visibility.Hidden;
                checkboxBuiltinCombatClass.Visibility = Visibility.Hidden;

                tabitemCombat.Visibility = Visibility.Collapsed;
                tabitemBattleground.Visibility = Visibility.Collapsed;
            }

            textboxCombatClassFile.Visibility = Visibility.Hidden;

            LoadConfigToUi();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ComboboxBattlegroundEngine_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (WindowLoaded)
            {
                if (comboboxBattlegroundEngine.SelectedItem == null || comboboxBattlegroundEngine.SelectedItem.ToString() == "None")
                {
                    labelBattlegroundEngineDescription.Content = "...";
                }
                else
                {
                    IBattlegroundEngine battlegroundEngine = AmeisenBot.BattlegroundEngines.FirstOrDefault(e => e.ToString().Equals(comboboxBattlegroundEngine.SelectedItem.ToString(), StringComparison.OrdinalIgnoreCase));

                    if (battlegroundEngine != null)
                    {
                        labelBattlegroundEngineDescription.Content = battlegroundEngine.Description;
                    }
                }
            }
        }
    }
}