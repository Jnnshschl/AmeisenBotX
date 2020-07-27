using AmeisenBotX.Core;
using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Views;
using Microsoft.Win32;
using System;
using System.IO;
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

        public bool ChangedSomething { get; set; }

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

                Config.AntiAfkMs = int.Parse(textboxAntiAfk.Text);
                Config.AutoAcceptQuests = checkboxAutoAcceptQuests.IsChecked.GetValueOrDefault(false);
                Config.AutoChangeRealmlist = checkboxAutoChangeRealmlist.IsChecked.GetValueOrDefault(false);
                Config.AutocloseWow = checkboxAutocloseWow.IsChecked.GetValueOrDefault(false);
                Config.AutoDisableRender = checkboxAutoDisableRendering.IsChecked.GetValueOrDefault(false);
                Config.AutoDodgeAoeSpells = checkboxAvoidAoe.IsChecked.GetValueOrDefault(false);
                Config.AutojoinBg = checkboxAutoJoinBg.IsChecked.GetValueOrDefault(false);
                Config.AutojoinLfg = checkboxAutoJoinLfg.IsChecked.GetValueOrDefault(false);
                Config.AutoLogin = checkboxAutoLogin.IsChecked.GetValueOrDefault(false);
                Config.AutoPositionWow = checkboxAutoPositionWow.IsChecked.GetValueOrDefault(false);
                Config.AutoRepair = checkboxAutoRepair.IsChecked.GetValueOrDefault(false);
                Config.AutostartWow = checkboxAutoStartWow.IsChecked.GetValueOrDefault(false);
                Config.AutoTalkToNearQuestgivers = checkboxAutoTalkToQuestgivers.IsChecked.GetValueOrDefault(false);
                Config.BagSlotsToGoSell = (int)Math.Round(sliderMinFreeBagSlots.Value);
                Config.BattlegroundEngine = comboboxBattlegroundEngine.SelectedItem != null ? comboboxBattlegroundEngine.SelectedItem.ToString() : string.Empty;
                Config.BattlegroundUsePartyMode = checkboxBattlegroundUsePartyMode.IsChecked.GetValueOrDefault(false);
                Config.BuiltInCombatClassName = comboboxBuiltInCombatClass.SelectedItem != null ? comboboxBuiltInCombatClass.SelectedItem.ToString() : string.Empty;
                Config.CharacterSlot = int.Parse(textboxCharacterSlot.Text);
                Config.CustomCombatClassFile = textboxCombatClassFile.Text;
                Config.DrinkUntilPercent = sliderDrinkUntil.Value;
                Config.DungeonUsePartyMode = checkboxDungeonUsePartyMode.IsChecked.GetValueOrDefault(false);
                Config.EatUntilPercent = sliderEatUntil.Value;
                Config.EventPullMs = int.Parse(textboxEventPull.Text);
                Config.FollowGroupLeader = checkboxFollowGroupLeader.IsChecked.GetValueOrDefault(false);
                Config.FollowGroupMembers = checkboxGroupMembers.IsChecked.GetValueOrDefault(false);
                Config.FollowPositionDynamic = checkboxDynamicPosition.IsChecked.GetValueOrDefault(false);
                Config.FollowSpecificCharacter = checkboxFollowSpecificCharacter.IsChecked.GetValueOrDefault(false);
                Config.Friends = textboxFriends.Text;
                Config.IgnoreCombatWhileMounted = checkboxIgnoreCombatMounted.IsChecked.GetValueOrDefault(false);
                Config.ItemRepairThreshold = sliderRepair.Value;
                Config.ItemSellBlacklist = textboxItemSellBlacklist.Text.Split(",", StringSplitOptions.RemoveEmptyEntries);
                Config.JobEngineMailHeader = textboxMailHeader.Text;
                Config.JobEngineMailReceiver = textboxMailReceiver.Text;
                Config.JobEngineMailText = textboxMailText.Text;
                Config.LootOnlyMoneyAndQuestitems = checkboxLootOnlyMoneyAndQuestitems.IsChecked.GetValueOrDefault(false);
                Config.LootUnits = checkboxLooting.IsChecked.GetValueOrDefault(false);
                Config.LootUnitsRadius = Math.Round(sliderLootRadius.Value);
                Config.MaxFollowDistance = (int)Math.Round(sliderMaxFollowDistance.Value);
                Config.MaxFps = (int)Math.Round(sliderMaxFps.Value);
                Config.MaxFpsCombat = (int)Math.Round(sliderMaxFpsCombat.Value);
                Config.MerchantNpcSearchRadius = sliderMerchantSearchRadius.Value;
                Config.MinFollowDistance = (int)Math.Round(sliderMinFollowDistance.Value);
                Config.Mounts = textboxMounts.Text;
                Config.NameshServerPort = int.Parse(textboxNavmeshServerPort.Text);
                Config.NavmeshServerIp = textboxNavmeshServerIp.Text;
                Config.OnlyFriendsMode = checkboxOnlyFriendsMode.IsChecked.GetValueOrDefault(false);
                Config.OnlySupportMaster = checkboxOnlySupportMaster.IsChecked.GetValueOrDefault(false);
                Config.Password = textboxPassword.Password;
                Config.PathToWowExe = textboxWowPath.Text;
                Config.PermanentNameCache = checkboxPermanentNameCache.IsChecked.GetValueOrDefault(false);
                Config.PermanentReactionCache = checkboxPermanentReactionCache.IsChecked.GetValueOrDefault(false);
                Config.RconEnabled = checkboxEnableRcon.IsChecked.GetValueOrDefault(true);
                Config.RconScreenshotInterval = int.Parse(textboxRconScreenshotInterval.Text);
                Config.RconSendScreenshots = checkboxEnableRconScreenshots.IsChecked.GetValueOrDefault(false);
                Config.RconServerAddress = textboxRconAddress.Text;
                Config.RconServerGuid = textboxRconGUID.Text;
                Config.RconServerImage = textboxRconImage.Text;
                Config.RconTickMs = int.Parse(textboxRconInterval.Text);
                Config.Realm = textboxRealm.Text;
                Config.Realmlist = textboxRealmlist.Text;
                Config.ReleaseSpirit = checkboxReleaseSpirit.IsChecked.GetValueOrDefault(false);
                Config.SaveBotWindowPosition = checkboxSaveBotWindowPosition.IsChecked.GetValueOrDefault(false);
                Config.SaveWowWindowPosition = checkboxSaveWowWindowPosition.IsChecked.GetValueOrDefault(false);
                Config.SellBlueItems = checkboxSellBlueItems.IsChecked.GetValueOrDefault(false);
                Config.SellGrayItems = checkboxSellGrayItems.IsChecked.GetValueOrDefault(false);
                Config.SellGreenItems = checkboxSellGreenItems.IsChecked.GetValueOrDefault(false);
                Config.SellPurpleItems = checkboxSellPurpleItems.IsChecked.GetValueOrDefault(false);
                Config.SellWhiteItems = checkboxSellWhiteItems.IsChecked.GetValueOrDefault(false);
                Config.SpecificCharacterToFollow = textboxFollowSpecificCharacterName.Text;
                Config.StateMachineTickMs = int.Parse(textboxStatemachineTick.Text);
                Config.SupportRange = sliderAssistRange.Value;
                Config.UseBuiltInCombatClass = checkboxBuiltinCombatClass.IsChecked.GetValueOrDefault(true);
                Config.UseMounts = checkboxUseMounts.IsChecked.GetValueOrDefault(false);
                Config.UseMountsInParty = checkboxUseMountsInParty.IsChecked.GetValueOrDefault(false);
                Config.UseOnlySpecificMounts = checkboxOnlySpecificMounts.IsChecked.GetValueOrDefault(false);
                Config.Username = textboxUsername.Text;

                SaveConfig = true;
                Close();
            }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            if (ChangedSomething)
            {
                ConfirmWindow confirmWindow = new ConfirmWindow("Unsaved Changes!", "Are you sure that you wan't to cancel?", "Yes", "No");
                confirmWindow.ShowDialog();

                if (!confirmWindow.OkayPressed)
                {
                    return;
                }
            }

            Cancel = true;
            Close();
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
                ChangedSomething = true;
            }
        }

        private void ButtonOpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PNG|*.png|JPEG|*.jpg;"
            };

            if (openFileDialog.ShowDialog().GetValueOrDefault(false))
            {
                string fileExtension = Path.GetExtension(openFileDialog.FileName).ToLower();
                textboxRconImage.Text = $"data:image/{fileExtension};base64,{Convert.ToBase64String(File.ReadAllBytes(openFileDialog.FileName))}";
                ChangedSomething = true;
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
                ChangedSomething = true;
            }
        }

        private void CheckboxAutoStartWow_Checked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                textboxWowPath.IsEnabled = true;
                buttonOpenWowExe.IsEnabled = true;
                ChangedSomething = true;
            }
        }

        private void CheckboxAutoStartWow_Unchecked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                textboxWowPath.IsEnabled = false;
                textboxWowPath.Text = string.Empty;
                buttonOpenWowExe.IsEnabled = false;
                ChangedSomething = true;
            }
        }

        private void CheckboxBuiltinCombatClass_Checked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                buttonOpenCombatClassFile.Visibility = Visibility.Hidden;
                textboxCombatClassFile.Visibility = Visibility.Hidden;
                comboboxBuiltInCombatClass.Visibility = Visibility.Visible;
                ChangedSomething = true;
            }
        }

        private void CheckboxCustomCombatClass_Checked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                buttonOpenCombatClassFile.Visibility = Visibility.Visible;
                textboxCombatClassFile.Visibility = Visibility.Visible;
                comboboxBuiltInCombatClass.Visibility = Visibility.Hidden;
                ChangedSomething = true;
            }
        }

        private void CheckboxFollowSpecificCharacter_Checked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                textboxFollowSpecificCharacterName.IsEnabled = true;
                ChangedSomething = true;
            }
        }

        private void CheckboxFollowSpecificCharacter_Unchecked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                textboxFollowSpecificCharacterName.IsEnabled = false;
                textboxFollowSpecificCharacterName.Text = string.Empty;
                ChangedSomething = true;
            }
        }

        private void CheckboxOnlyFriendsMode_Checked(object sender, RoutedEventArgs e)
        {
            if (textboxFriends != null)
            {
                textboxFriends.IsEnabled = true;
                ChangedSomething = true;
            }
        }

        private void CheckboxOnlyFriendsMode_Unchecked(object sender, RoutedEventArgs e)
        {
            if (textboxFriends != null)
            {
                textboxFriends.IsEnabled = false;
                ChangedSomething = true;
            }
        }

        private void CheckboxOnlySpecificMounts_Checked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                textboxMounts.IsEnabled = true;
            }
        }

        private void CheckboxOnlySpecificMounts_Unchecked(object sender, RoutedEventArgs e)
        {
            if (WindowLoaded)
            {
                textboxMounts.IsEnabled = false;
            }
        }

        private void ComboboxBattlegroundEngine_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (WindowLoaded)
            {
                if (comboboxBattlegroundEngine.SelectedItem == null || comboboxBattlegroundEngine.SelectedItem.ToString() == "None")
                {
                    labelBattlegroundEngineDescription.Content = "...";
                    ChangedSomething = true;
                }
                else
                {
                    IBattlegroundEngine battlegroundEngine = AmeisenBot.BattlegroundEngines.FirstOrDefault(e => e.ToString().Equals(comboboxBattlegroundEngine.SelectedItem.ToString(), StringComparison.OrdinalIgnoreCase));

                    if (battlegroundEngine != null)
                    {
                        labelBattlegroundEngineDescription.Content = battlegroundEngine.Description;
                        ChangedSomething = true;
                    }
                }
            }
        }

        private void LoadConfigToUi()
        {
            checkboxAutoAcceptQuests.IsChecked = Config.AutoAcceptQuests;
            checkboxAutoChangeRealmlist.IsChecked = Config.AutoChangeRealmlist;
            checkboxAutocloseWow.IsChecked = Config.AutocloseWow;
            checkboxAutoDisableRendering.IsChecked = Config.AutoDisableRender;
            checkboxAutoJoinBg.IsChecked = Config.AutojoinBg;
            checkboxAutoJoinLfg.IsChecked = Config.AutojoinLfg;
            checkboxAutoLogin.IsChecked = Config.AutoLogin;
            checkboxAutoPositionWow.IsChecked = Config.AutoPositionWow;
            checkboxAutoRepair.IsChecked = Config.AutoRepair;
            checkboxAutoStartWow.IsChecked = Config.AutostartWow;
            checkboxAutoTalkToQuestgivers.IsChecked = Config.AutoTalkToNearQuestgivers;
            checkboxAvoidAoe.IsChecked = Config.AutoDodgeAoeSpells;
            checkboxBattlegroundUsePartyMode.IsChecked = Config.BattlegroundUsePartyMode;
            checkboxBuiltinCombatClass.IsChecked = Config.UseBuiltInCombatClass;
            checkboxDungeonUsePartyMode.IsChecked = Config.DungeonUsePartyMode;
            checkboxDynamicPosition.IsChecked = Config.FollowPositionDynamic;
            checkboxEnableRcon.IsChecked = Config.RconEnabled;
            checkboxEnableRconScreenshots.IsChecked = Config.RconSendScreenshots;
            checkboxFollowGroupLeader.IsChecked = Config.FollowGroupLeader;
            checkboxFollowSpecificCharacter.IsChecked = Config.FollowSpecificCharacter;
            checkboxGroupMembers.IsChecked = Config.FollowGroupMembers;
            checkboxIgnoreCombatMounted.IsChecked = Config.IgnoreCombatWhileMounted;
            checkboxLooting.IsChecked = Config.LootUnits;
            checkboxLootOnlyMoneyAndQuestitems.IsChecked = Config.LootOnlyMoneyAndQuestitems;
            checkboxOnlyFriendsMode.IsChecked = Config.OnlyFriendsMode;
            checkboxOnlySpecificMounts.IsChecked = Config.UseOnlySpecificMounts;
            checkboxOnlySupportMaster.IsChecked = Config.OnlySupportMaster;
            checkboxPermanentNameCache.IsChecked = Config.PermanentNameCache;
            checkboxPermanentReactionCache.IsChecked = Config.PermanentReactionCache;
            checkboxReleaseSpirit.IsChecked = Config.ReleaseSpirit;
            checkboxSaveBotWindowPosition.IsChecked = Config.SaveBotWindowPosition;
            checkboxSaveWowWindowPosition.IsChecked = Config.SaveWowWindowPosition;
            checkboxSellBlueItems.IsChecked = Config.SellBlueItems;
            checkboxSellGrayItems.IsChecked = Config.SellGrayItems;
            checkboxSellGreenItems.IsChecked = Config.SellGreenItems;
            checkboxSellPurpleItems.IsChecked = Config.SellPurpleItems;
            checkboxSellWhiteItems.IsChecked = Config.SellWhiteItems;
            checkboxUseMounts.IsChecked = Config.UseMounts;
            checkboxUseMountsInParty.IsChecked = Config.UseMountsInParty;
            comboboxBattlegroundEngine.Text = Config.BattlegroundEngine;
            comboboxBuiltInCombatClass.Text = Config.BuiltInCombatClassName;
            sliderAssistRange.Value = Config.SupportRange;
            sliderDrinkUntil.Value = Config.DrinkUntilPercent;
            sliderEatUntil.Value = Config.EatUntilPercent;
            sliderLootRadius.Value = Math.Round(Config.LootUnitsRadius);
            sliderMaxFollowDistance.Value = Config.MaxFollowDistance;
            sliderMaxFps.Value = Config.MaxFps;
            sliderMaxFpsCombat.Value = Config.MaxFpsCombat;
            sliderMerchantSearchRadius.Value = Config.MerchantNpcSearchRadius;
            sliderMinFollowDistance.Value = Config.MinFollowDistance;
            sliderMinFreeBagSlots.Value = Config.BagSlotsToGoSell;
            sliderRepair.Value = Config.ItemRepairThreshold;
            textboxAntiAfk.Text = Config.AntiAfkMs.ToString();
            textboxCharacterSlot.Text = Config.CharacterSlot.ToString();
            textboxCombatClassFile.Text = Config.CustomCombatClassFile;
            textboxEventPull.Text = Config.EventPullMs.ToString();
            textboxFollowSpecificCharacterName.Text = Config.SpecificCharacterToFollow;
            textboxFriends.Text = Config.Friends;
            textboxItemSellBlacklist.Text = string.Join(",", Config.ItemSellBlacklist);
            textboxMailHeader.Text = Config.JobEngineMailHeader;
            textboxMailReceiver.Text = Config.JobEngineMailReceiver;
            textboxMailText.Text = Config.JobEngineMailText;
            textboxMounts.Text = Config.Mounts;
            textboxNavmeshServerIp.Text = Config.NavmeshServerIp;
            textboxNavmeshServerPort.Text = Config.NameshServerPort.ToString();
            textboxPassword.Password = Config.Password;
            textboxRconAddress.Text = Config.RconServerAddress;
            textboxRconGUID.Text = Config.RconServerGuid;
            textboxRconImage.Text = Config.RconServerImage;
            textboxRconInterval.Text = Config.RconTickMs.ToString();
            textboxRconScreenshotInterval.Text = Config.RconScreenshotInterval.ToString();
            textboxRealm.Text = Config.Realm;
            textboxRealmlist.Text = Config.Realmlist;
            textboxStatemachineTick.Text = Config.StateMachineTickMs.ToString();
            textboxUsername.Text = Config.Username;
            textboxWowPath.Text = Config.PathToWowExe;

            ChangedSomething = false;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SliderAssistRange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelGroupAssistRange.Content = $"Assist Range (m): {Math.Round(e.NewValue)}";
                ChangedSomething = true;
            }
        }

        private void SliderDrinkUntil_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelDrinkUntil.Content = $"Drink Until: {Math.Round(e.NewValue)} %";
                ChangedSomething = true;
            }
        }

        private void SliderEatUntil_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelEatUntil.Content = $"Eat Until: {Math.Round(e.NewValue)} %";
                ChangedSomething = true;
            }
        }

        private void SliderLootRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelLootRadius.Content = $"Loot Radius: {Math.Round(e.NewValue)}m";
                ChangedSomething = true;
            }
        }

        private void SliderMaxFollowDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelMaxFollowDistance.Content = $"Max Follow Distance: {Math.Round(e.NewValue)}m";
                ChangedSomething = true;
            }
        }

        private void SliderMaxFps_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelMaxFps.Content = $"Max FPS: {Math.Round(e.NewValue)}";
                ChangedSomething = true;
            }
        }

        private void SliderMaxFpsCombat_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelMaxFpsCombat.Content = $"Max FPS Combat: {Math.Round(e.NewValue)}";
                ChangedSomething = true;
            }
        }

        private void SliderMerchantSearchRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelMerchantSearchRadius.Content = $"Search Radius (m): {Math.Round(e.NewValue)}";
                ChangedSomething = true;
            }
        }

        private void SliderMinFollowDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelMinFollowDistance.Content = $"Min Follow Distance: {Math.Round(e.NewValue)}m";
                ChangedSomething = true;
            }
        }

        private void SliderMinFreeBagSlots_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelMinFreeBagSlots.Content = $"Min Free Bag Slots: {(int)Math.Round(e.NewValue)}";
                ChangedSomething = true;
            }
        }

        private void SliderRepair_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WindowLoaded)
            {
                labelRepairThreshold.Content = $"Repair: {Math.Round(e.NewValue)}%";
                ChangedSomething = true;
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
    }
}