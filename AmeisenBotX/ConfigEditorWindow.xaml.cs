using AmeisenBotX.Core;
using AmeisenBotX.Views;
using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AmeisenBotX
{
    /// <summary>
    /// Interaktionslogik für ConfigEditorWindow.xaml
    /// </summary>
    public partial class ConfigEditorWindow : Window
    {
        public SolidColorBrush normalBorderBrush;
        public SolidColorBrush errorBorderBrush;

        public ConfigEditorWindow(string dataDir, AmeisenBotConfig initialConfig = null)
        {
            InitializeComponent();

            DataDir = dataDir;
            NewConfig = initialConfig == null;
            Config = initialConfig ?? new AmeisenBotConfig();

            normalBorderBrush = new SolidColorBrush((Color)FindResource("DarkBorder"));
            errorBorderBrush = new SolidColorBrush((Color)FindResource("DarkError"));
        }

        public AmeisenBotConfig Config { get; private set; }

        public string DataDir { get; }

        public string ConfigName { get; private set; }

        public bool NewConfig { get; }

        private void ButtonAbort_Click(object sender, RoutedEventArgs e)
        {
            ConfirmWindow confirmWindow = new ConfirmWindow("Unsaved Changes!", "Are you sure that you wan't to cancel?", "Yes", "No");
            confirmWindow.ShowDialog();

            if (confirmWindow.OkayPressed)
            {
                Close();
            }
        }

        private void ButtonOkay_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateFields())
            {
                ConfigName = textboxConfigName.Text.Trim();
                Config = new AmeisenBotConfig()
                {
                    Username = textboxUsername.Text,
                    Password = textboxPassword.Password,
                    CharacterSlot = int.Parse(textboxCharacterSlot.Text),
                    BuiltInCombatClassName = comboboxBuiltInCombatClass.Text,
                    UseBuiltInCombatClass = checkboxBuiltinCombatClass.IsChecked.GetValueOrDefault(true),
                    CustomCombatClassFile = textboxCombatClassFile.Text,
                    AutoDodgeAoeSpells = checkboxAvoidAoe.IsChecked.GetValueOrDefault(false),
                    AutostartWow = checkboxAutoStartWow.IsChecked.GetValueOrDefault(false),
                    AutocloseWow = checkboxAutocloseWow.IsChecked.GetValueOrDefault(false),
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
                    SpecificCharacterToFollow = textboxFollowSpecificCharacterName.Text,
                    LootUnits = checkboxLooting.IsChecked.GetValueOrDefault(false),
                    MaxFps = (int)Math.Round(sliderMaxFps.Value),
                    MaxFpsCombat = (int)Math.Round(sliderMaxFpsCombat.Value),
                    MaxFollowDistance = (int)Math.Round(sliderMaxFollowDistance.Value),
                    MinFollowDistance = (int)Math.Round(sliderMinFollowDistance.Value),
                    LootUnitsRadius = Math.Round(sliderLootRadius.Value),
                    NavmeshServerIp = textboxNavmeshServerIp.Text,
                    NameshServerPort = int.Parse(textboxNavmeshServerPort.Text)
                };

                Close();
            }
        }

        private bool ValidateFields()
        {
            bool failed = false;
            failed = ValidateConfigName(failed);
            failed = ValidateAutoLogin(failed);
            failed = ValidateAutostartWow(failed);
            failed = ValidateSpecificFollow(failed);
            failed = ValidateCombatClass(failed);
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

        private bool ValidateConfigName(bool failed)
        {
            Regex regex = new Regex("^([a-zA-Z]:)?(\\\\[^<>:\"/\\\\|?*]+)+\\\\?$");

            if (textboxConfigName.Text.Length == 0
                || regex.IsMatch(textboxConfigName.Text)
                || Directory.Exists(Path.Combine(DataDir, textboxConfigName.Text)))
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

        private bool ValidateCombatClass(bool failed)
        {
            if (checkboxBuiltinCombatClass.IsChecked.GetValueOrDefault(false))
            {
                if (comboboxBuiltInCombatClass.Text.Length == 0)
                {
                    comboboxBuiltInCombatClass.BorderBrush = errorBorderBrush;
                    failed = true;
                }
                else
                {
                    comboboxBuiltInCombatClass.BorderBrush = normalBorderBrush;
                }
            }

            if (checkboxCustomCombatClass.IsChecked.GetValueOrDefault(false))
            {
                if (textboxCombatClassFile.Text.Length == 0)
                {
                    textboxCombatClassFile.BorderBrush = errorBorderBrush;
                    failed = true;
                }
                else
                {
                    textboxCombatClassFile.BorderBrush = normalBorderBrush;
                }
            }

            return failed;
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
            textboxWowPath.IsEnabled = true;
            buttonOpenWowExe.IsEnabled = true;
        }

        private void CheckboxAutoStartWow_Unchecked(object sender, RoutedEventArgs e)
        {
            textboxWowPath.IsEnabled = false;
            textboxWowPath.Text = string.Empty;
            buttonOpenWowExe.IsEnabled = false;
        }

        private void CheckboxFollowSpecificCharacter_Checked(object sender, RoutedEventArgs e)
        {
            textboxFollowSpecificCharacterName.IsEnabled = true;
        }

        private void CheckboxFollowSpecificCharacter_Unchecked(object sender, RoutedEventArgs e)
        {
            textboxFollowSpecificCharacterName.IsEnabled = false;
            textboxFollowSpecificCharacterName.Text = string.Empty;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void CheckboxBuiltinCombatClass_Checked(object sender, RoutedEventArgs e)
        {
            labelCombatClassHeader.Content = "Select CombatClass:";
            buttonOpenCombatClassFile.Visibility = Visibility.Hidden;
            textboxCombatClassFile.Visibility = Visibility.Hidden;
            comboboxBuiltInCombatClass.Visibility = Visibility.Visible;
        }

        private void CheckboxCustomCombatClass_Checked(object sender, RoutedEventArgs e)
        {
            labelCombatClassHeader.Content = "CombatClass File:";
            buttonOpenCombatClassFile.Visibility = Visibility.Visible;
            textboxCombatClassFile.Visibility = Visibility.Visible;
            comboboxBuiltInCombatClass.Visibility = Visibility.Hidden;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!NewConfig)
            {
                textboxConfigName.IsEnabled = false;
            }

            AddDefaultCombatClasses();
            LoadConfigToUi();
        }

        private void LoadConfigToUi()
        {
            textboxUsername.Text = Config.Username;
            textboxPassword.Password = Config.Password;
            textboxCharacterSlot.Text = Config.CharacterSlot.ToString();
            comboboxBuiltInCombatClass.Text = Config.BuiltInCombatClassName;
            checkboxBuiltinCombatClass.IsChecked = Config.UseBuiltInCombatClass;
            textboxCombatClassFile.Text = Config.CustomCombatClassFile;
            checkboxAvoidAoe.IsChecked = Config.AutoDodgeAoeSpells;
            checkboxAutoStartWow.IsChecked = Config.AutostartWow;
            checkboxAutocloseWow.IsChecked = Config.AutocloseWow;
            checkboxAutoLogin.IsChecked = Config.AutoLogin;
            checkboxFollowGroupLeader.IsChecked = Config.FollowGroupLeader;
            checkboxGroupMembers.IsChecked = Config.FollowGroupMembers;
            checkboxFollowSpecificCharacter.IsChecked = Config.FollowSpecificCharacter;
            checkboxPermanentNameCache.IsChecked = Config.PermanentNameCache;
            checkboxPermanentReactionCache.IsChecked = Config.PermanentReactionCache;
            checkboxReleaseSpirit.IsChecked = Config.ReleaseSpirit;
            checkboxSaveWowWindowPosition.IsChecked = Config.SaveWowWindowPosition;
            checkboxSaveBotWindowPosition.IsChecked = Config.SaveBotWindowPosition;
            textboxWowPath.Text = Config.PathToWowExe;
            textboxFollowSpecificCharacterName.Text = Config.SpecificCharacterToFollow;
            checkboxLooting.IsChecked = Config.LootUnits;
            sliderMinFollowDistance.Value = Config.MinFollowDistance;
            sliderMaxFollowDistance.Value = Config.MaxFollowDistance;
            sliderMaxFps.Value = Config.MaxFps;
            sliderMaxFpsCombat.Value = Config.MaxFpsCombat;
            sliderLootRadius.Value = Math.Round(Config.LootUnitsRadius);
            textboxNavmeshServerIp.Text = Config.NavmeshServerIp;
            textboxNavmeshServerPort.Text = Config.NameshServerPort.ToString();
        }

        private void AddDefaultCombatClasses()
        {
            comboboxBuiltInCombatClass.Items.Add("None");
            comboboxBuiltInCombatClass.Items.Add("WarriorArms");
            comboboxBuiltInCombatClass.Items.Add("DeathknightBlood");
            comboboxBuiltInCombatClass.Items.Add("DeathknightUnholy");
            comboboxBuiltInCombatClass.Items.Add("DeathknightFrost");
            comboboxBuiltInCombatClass.Items.Add("WarriorFury");
            comboboxBuiltInCombatClass.Items.Add("PaladinHoly");
            comboboxBuiltInCombatClass.Items.Add("PaladinRetribution");
            comboboxBuiltInCombatClass.Items.Add("PaladinProtection");
            comboboxBuiltInCombatClass.Items.Add("MageArcane");
            comboboxBuiltInCombatClass.Items.Add("MageFire");
            comboboxBuiltInCombatClass.Items.Add("HunterBeastmastery");
            comboboxBuiltInCombatClass.Items.Add("PriestHoly");
            comboboxBuiltInCombatClass.Items.Add("PriestShadow");
            comboboxBuiltInCombatClass.Items.Add("WarlockAffliction");
            comboboxBuiltInCombatClass.Items.Add("WarlockDemonology");
            comboboxBuiltInCombatClass.Items.Add("WarlockDestruction");
            comboboxBuiltInCombatClass.Items.Add("DruidRestoration");
            comboboxBuiltInCombatClass.Items.Add("DruidBalance");
            comboboxBuiltInCombatClass.Items.Add("RogueAssasination");
            comboboxBuiltInCombatClass.Items.Add("ShamanElemental");
            comboboxBuiltInCombatClass.SelectedIndex = 0;
        }

        private void SliderMinFollowDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (labelMinFollowDistance != null)
            {
                labelMinFollowDistance.Content = $"Min Follow Distance: {Math.Round(e.NewValue)}m";
            }
        }

        private void SliderMaxFollowDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (labelMaxFollowDistance != null)
            {
                labelMaxFollowDistance.Content = $"Max Follow Distance: {Math.Round(e.NewValue)}m";
            }
        }

        private void SliderLootRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (labelLootRadius != null)
            {
                labelLootRadius.Content = $"Loot Radius: {Math.Round(e.NewValue)}m";
            }
        }

        private void SliderMaxFps_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (labelMaxFps != null)
            {
                labelMaxFps.Content = $"Max FPS: {Math.Round(e.NewValue)}";
            }
        }

        private void SliderMaxFpsCombat_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (labelMaxFpsCombat != null)
            {
                labelMaxFpsCombat.Content = $"Max FPS Combat: {Math.Round(e.NewValue)}";
            }
        }
    }
}
