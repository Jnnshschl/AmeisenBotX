using AmeisenBotX.Core;
using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AmeisenBotX
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly string BotDataPath = $"{AppDomain.CurrentDomain.BaseDirectory}data\\";

        private readonly Brush darkForegroundBrush;
        private readonly Brush textAccentBrush;

        #region ClassBrushes

        private readonly Brush currentTickTimeBadBrush = new SolidColorBrush(Color.FromRgb(255, 0, 80));
        private readonly Brush currentTickTimeGoodBrush = new SolidColorBrush(Color.FromRgb(160, 255, 0));

        private readonly Brush dkPrimaryBrush = new SolidColorBrush(Color.FromRgb(196, 30, 59));
        private readonly Brush dkSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        private readonly Brush druidPrimaryBrush = new SolidColorBrush(Color.FromRgb(255, 125, 10));
        private readonly Brush druidSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        private readonly Brush hunterPrimaryBrush = new SolidColorBrush(Color.FromRgb(171, 212, 115));
        private readonly Brush hunterSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        private readonly Brush magePrimaryBrush = new SolidColorBrush(Color.FromRgb(105, 204, 240));
        private readonly Brush mageSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        private readonly Brush paladinPrimaryBrush = new SolidColorBrush(Color.FromRgb(245, 140, 186));
        private readonly Brush paladinSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        private readonly Brush priestPrimaryBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        private readonly Brush priestSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        private readonly Brush roguePrimaryBrush = new SolidColorBrush(Color.FromRgb(255, 245, 105));
        private readonly Brush rogueSecondaryBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));

        private readonly Brush shamanPrimaryBrush = new SolidColorBrush(Color.FromRgb(0, 112, 222));
        private readonly Brush shamanSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        private readonly Brush warlockPrimaryBrush = new SolidColorBrush(Color.FromRgb(148, 130, 201));
        private readonly Brush warlockSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));

        private readonly Brush warriorPrimaryBrush = new SolidColorBrush(Color.FromRgb(199, 156, 110));
        private readonly Brush warriorSecondaryBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));

        #endregion ClassBrushes

        public MainWindow()
        {
            InitializeComponent();

            Config = LoadConfig();

            darkForegroundBrush = new SolidColorBrush((Color)FindResource("DarkForeground"));
            textAccentBrush = new SolidColorBrush((Color)FindResource("TextAccent"));

            if (Config != null)
            {
                string playername = Path.GetFileName(Path.GetDirectoryName(ConfigPath));
                AmeisenBot = new AmeisenBot(BotDataPath, playername, Config);

                AmeisenBot.WowInterface.ObjectManager.OnObjectUpdateComplete += OnObjectUpdateComplete;
                AmeisenBot.StateMachine.OnStateMachineStateChanged += OnStateMachineStateChange;

                LastStateMachineTickUpdate = DateTime.Now;
            }
        }

        public AmeisenBotConfig Config { get; private set; }

        public string ConfigPath { get; private set; }

        private AmeisenBot AmeisenBot { get; }

        private DateTime LastStateMachineTickUpdate { get; set; }

        private void ButtonClearCache_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBot.WowInterface.BotCache.Clear();
        }

        private void ButtonConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigEditorWindow configWindow = new ConfigEditorWindow(BotDataPath, AmeisenBot, Config, Path.GetFileName(Path.GetDirectoryName(ConfigPath)));
            configWindow.ShowDialog();

            if (configWindow.SaveConfig)
            {
                AmeisenBot.Config = configWindow.Config;
                AmeisenBot.ReloadConfig();
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(configWindow.Config, Formatting.Indented));
            }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e) => Close();

        private void ButtonFaceTarget_Click(object sender, RoutedEventArgs e)
        {
            float angle = BotMath.GetFacingAngle(AmeisenBot.WowInterface.ObjectManager.Player.Position, AmeisenBot.WowInterface.ObjectManager.Target.Position);
            AmeisenBot.WowInterface.HookManager.SetFacing(AmeisenBot.WowInterface.ObjectManager.Player, angle);
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e) => new SettingsWindow(Config).ShowDialog();

        private void ButtonStartAutopilot_Click(object sender, RoutedEventArgs e)
        {
            if (AmeisenBot.Config.Autopilot)
            {
                AmeisenBot.Config.Autopilot = false;
                buttonStartAutopilot.Foreground = darkForegroundBrush;
            }
            else
            {
                AmeisenBot.Config.Autopilot = true;
                buttonStartAutopilot.Foreground = currentTickTimeGoodBrush;
            }
        }

        private void ButtonStartPause_Click(object sender, RoutedEventArgs e)
        {
            if (AmeisenBot.IsRunning)
            {
                AmeisenBot.Pause();
                buttonStartPause.Content = "▶";
                buttonStartPause.Foreground = textAccentBrush;
            }
            else
            {
                AmeisenBot.Resume();
                buttonStartPause.Content = "||";
                buttonStartPause.Foreground = darkForegroundBrush;
            }
        }

        private AmeisenBotConfig LoadConfig()
        {
            LoadConfigWindow loadConfigWindow = new LoadConfigWindow(BotDataPath);
            loadConfigWindow.ShowDialog();

            if (loadConfigWindow.ConfigToLoad.Length > 0)
            {
                AmeisenBotConfig config;
                if (File.Exists(loadConfigWindow.ConfigToLoad))
                {
                    config = JsonConvert.DeserializeObject<AmeisenBotConfig>(File.ReadAllText(loadConfigWindow.ConfigToLoad));
                }
                else
                {
                    config = new AmeisenBotConfig();
                }

                ConfigPath = loadConfigWindow.ConfigToLoad;
                return config;
            }
            else
            {
                Close();
            }

            return null;
        }

        private void OnObjectUpdateComplete(List<WowObject> wowObjects)
        {
            Dispatcher.InvokeAsync(() =>
            {
                // debug label stuff
                if (AmeisenBot.WowInterface.BattlegroundEngine != null)
                {
                    string ownCarrier = string.Empty;
                    string enemyCarrier = string.Empty;
                    bool isMeCarrier = false;

                    if (AmeisenBot.WowInterface.BattlegroundEngine.BattlegroundProfile?.BattlegroundType == Core.Battleground.Enums.BattlegroundType.CaptureTheFlag)
                    {
                        ICtfBattlegroundProfile ctfBattlegroundProfile = (ICtfBattlegroundProfile)AmeisenBot.WowInterface.BattlegroundEngine.BattlegroundProfile;
                        ownCarrier = ctfBattlegroundProfile.OwnFlagCarrierPlayer?.Name;
                        enemyCarrier = ctfBattlegroundProfile.EnemyFlagCarrierPlayer?.Name;
                        isMeCarrier = ctfBattlegroundProfile.IsMeFlagCarrier;
                    }

                    labelBgObjective.Content = $"BGEngine State: {AmeisenBot.WowInterface.BattlegroundEngine.CurrentState.Key}\nBGEngine Last State: {AmeisenBot.WowInterface.BattlegroundEngine.LastState}\n\nBGOwnFlagCarrier: {ownCarrier}\nBGEnemyFlagCarrier: {enemyCarrier}\nisMeCarrier: {isMeCarrier}";
                }

                // update health and secodary power bar and
                // the colors corresponding to the class
                switch (AmeisenBot.WowInterface.ObjectManager.Player.Class)
                {
                    case WowClass.Deathknight:
                        UpdateBotInfo(AmeisenBot.WowInterface.ObjectManager.Player.MaxRuneenergy, AmeisenBot.WowInterface.ObjectManager.Player.Runeenergy, dkPrimaryBrush, dkSecondaryBrush);
                        break;

                    case WowClass.Druid:
                        UpdateBotInfo(AmeisenBot.WowInterface.ObjectManager.Player.MaxMana, AmeisenBot.WowInterface.ObjectManager.Player.Mana, druidPrimaryBrush, druidSecondaryBrush);
                        break;

                    case WowClass.Hunter:
                        UpdateBotInfo(AmeisenBot.WowInterface.ObjectManager.Player.MaxMana, AmeisenBot.WowInterface.ObjectManager.Player.Mana, hunterPrimaryBrush, hunterSecondaryBrush);
                        break;

                    case WowClass.Mage:
                        UpdateBotInfo(AmeisenBot.WowInterface.ObjectManager.Player.MaxMana, AmeisenBot.WowInterface.ObjectManager.Player.Mana, magePrimaryBrush, mageSecondaryBrush);
                        break;

                    case WowClass.Paladin:
                        UpdateBotInfo(AmeisenBot.WowInterface.ObjectManager.Player.MaxMana, AmeisenBot.WowInterface.ObjectManager.Player.Mana, paladinPrimaryBrush, paladinSecondaryBrush);
                        break;

                    case WowClass.Priest:
                        UpdateBotInfo(AmeisenBot.WowInterface.ObjectManager.Player.MaxMana, AmeisenBot.WowInterface.ObjectManager.Player.Mana, priestPrimaryBrush, priestSecondaryBrush);
                        break;

                    case WowClass.Rogue:
                        UpdateBotInfo(AmeisenBot.WowInterface.ObjectManager.Player.MaxEnergy, AmeisenBot.WowInterface.ObjectManager.Player.Energy, roguePrimaryBrush, rogueSecondaryBrush);
                        break;

                    case WowClass.Shaman:
                        UpdateBotInfo(AmeisenBot.WowInterface.ObjectManager.Player.MaxMana, AmeisenBot.WowInterface.ObjectManager.Player.Mana, shamanPrimaryBrush, shamanSecondaryBrush);
                        break;

                    case WowClass.Warlock:
                        UpdateBotInfo(AmeisenBot.WowInterface.ObjectManager.Player.MaxMana, AmeisenBot.WowInterface.ObjectManager.Player.Mana, warlockPrimaryBrush, warlockSecondaryBrush);
                        break;

                    case WowClass.Warrior:
                        UpdateBotInfo(AmeisenBot.WowInterface.ObjectManager.Player.MaxRage, AmeisenBot.WowInterface.ObjectManager.Player.Rage, warriorPrimaryBrush, warriorSecondaryBrush);
                        break;
                }

                // update the ms label every second
                if (LastStateMachineTickUpdate + TimeSpan.FromSeconds(1) < DateTime.Now)
                {
                    UpdateExecutionMsLabel();
                    LastStateMachineTickUpdate = DateTime.Now;
                }

                // update the object count label
                labelCurrentObjectCount.Content = AmeisenBot.WowInterface.ObjectManager.WowObjects.Count;
            });
        }

        private void OnStateMachineStateChange()
        {
            Dispatcher.InvokeAsync(() =>
            {
                labelCurrentState.Content = $"{AmeisenBot.StateMachine.CurrentState.Key}";
            });
        }

        private void SaveConfig()
        {
            if (!string.IsNullOrEmpty(ConfigPath) && Config != null)
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(AmeisenBot.Config, Formatting.Indented));
            }
        }

        private void UpdateBotInfo(int maxSecondary, int secondary, Brush primaryBrush, Brush secondaryBrush)
        {
            // generic stuff
            labelPlayerName.Content = AmeisenBot.WowInterface.ObjectManager.Player.Name;

            labelGamestate.Content = AmeisenBot.WowInterface.ObjectManager.GameState;
            labelMapName.Content = AmeisenBot.WowInterface.ObjectManager.MapId.ToString();
            labelZoneName.Content = AmeisenBot.WowInterface.ObjectManager.ZoneName;
            labelZoneSubName.Content = AmeisenBot.WowInterface.ObjectManager.ZoneSubName;

            labelCurrentLevel.Content = AmeisenBot.WowInterface.ObjectManager.Player.Level;
            labelCurrentRace.Content = $"{AmeisenBot.WowInterface.ObjectManager.Player.Race} {AmeisenBot.WowInterface.ObjectManager.Player.Gender}";
            labelCurrentClass.Content = AmeisenBot.WowInterface.ObjectManager.Player.Class;

            progressbarExp.Maximum = AmeisenBot.WowInterface.ObjectManager.Player.NextLevelXp;
            progressbarExp.Value = AmeisenBot.WowInterface.ObjectManager.Player.Xp;
            labelCurrentExp.Content = $"{Math.Round(AmeisenBot.WowInterface.ObjectManager.Player.XpPercentage)}%";

            progressbarHealth.Maximum = AmeisenBot.WowInterface.ObjectManager.Player.MaxHealth;
            progressbarHealth.Value = AmeisenBot.WowInterface.ObjectManager.Player.Health;
            labelCurrentHealth.Content = BotUtils.BigValueToString(AmeisenBot.WowInterface.ObjectManager.Player.Health);

            labelCurrentCombatclass.Content = AmeisenBot.WowInterface.CombatClass == null ? $"No CombatClass" : AmeisenBot.WowInterface.CombatClass.ToString();

            // class specific stuff
            progressbarSecondary.Maximum = maxSecondary;
            progressbarSecondary.Value = secondary;
            labelCurrentSecondary.Content = BotUtils.BigValueToString(secondary);

            if (progressbarHealth.Foreground != primaryBrush)
            {
                progressbarHealth.Foreground = primaryBrush;
            }

            if (progressbarSecondary.Foreground != secondaryBrush)
            {
                progressbarSecondary.Foreground = secondaryBrush;
            }

            if (labelCurrentClass.Foreground != primaryBrush)
            {
                labelCurrentClass.Foreground = primaryBrush;
            }
        }

        private void UpdateExecutionMsLabel()
        {
            double executionMs = AmeisenBot.CurrentExecutionMs;
            if (double.IsNaN(executionMs) || double.IsInfinity(executionMs))
            {
                executionMs = 0;
            }

            labelCurrentTickTime.Content = executionMs;
            if (executionMs <= Config.StateMachineTickMs)
            {
                labelCurrentTickTime.Foreground = currentTickTimeGoodBrush;
            }
            else
            {
                labelCurrentTickTime.Foreground = currentTickTimeBadBrush;
                AmeisenLogger.Instance.Log("MainWindow", "High executionMs, something blocks our thread or CPU is to slow...", LogLevel.Warning);
            }

            labelHookCallCount.Content = AmeisenBot.WowInterface.HookManager.CallCount;
            if (AmeisenBot.WowInterface.HookManager.CallCount <= (AmeisenBot.WowInterface.ObjectManager.Player.IsInCombat ? (ulong)Config.MaxFpsCombat : (ulong)Config.MaxFps))
            {
                labelHookCallCount.Foreground = currentTickTimeGoodBrush;
            }
            else
            {
                labelHookCallCount.Foreground = currentTickTimeBadBrush;
                AmeisenLogger.Instance.Log("MainWindow", "High HookCall count, maybe increase your FPS...", LogLevel.Warning);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AmeisenBot?.Stop();
            SaveConfig();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Start();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}