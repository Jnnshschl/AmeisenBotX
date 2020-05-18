using AmeisenBotX.Core;
using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Overlay;
using AmeisenBotX.Overlay.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AmeisenBotX
{
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

            DrawOverlay = false;
        }

        public AmeisenBotConfig Config { get; private set; }

        public string ConfigPath { get; private set; }

        public AmeisenBotOverlay Overlay { get; private set; }

        private AmeisenBot AmeisenBot { get; set; }

        private bool DrawOverlay { get; set; }

        private InfoWindow InfoWindow { get; set; }

        private DateTime LastStateMachineTickUpdate { get; set; }

        private MapWindow MapWindow { get; set; }

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

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonFaceTarget_Click(object sender, RoutedEventArgs e)
        {
            float angle = BotMath.GetFacingAngle(AmeisenBot.WowInterface.ObjectManager.Player.Position, AmeisenBot.WowInterface.ObjectManager.Target.Position);
            AmeisenBot.WowInterface.HookManager.SetFacing(AmeisenBot.WowInterface.ObjectManager.Player, angle);
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e) => new SettingsWindow(Config).ShowDialog();

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

        private void ButtonToggleAutopilot_Click(object sender, RoutedEventArgs e)
        {
            if (AmeisenBot.Config.Autopilot)
            {
                AmeisenBot.Config.Autopilot = false;
                buttonToggleAutopilot.Foreground = darkForegroundBrush;
            }
            else
            {
                AmeisenBot.Config.Autopilot = true;
                buttonToggleAutopilot.Foreground = currentTickTimeGoodBrush;
            }
        }

        private void ButtonToggleInfoWindow_Click(object sender, RoutedEventArgs e)
        {
            if (InfoWindow == null)
            {
                InfoWindow = new InfoWindow(AmeisenBot);
            }

            InfoWindow.Show();
        }

        private void ButtonToggleMapWindow_Click(object sender, RoutedEventArgs e)
        {
            if (MapWindow == null)
            {
                MapWindow = new MapWindow(AmeisenBot);
            }

            MapWindow.Show();
        }

        private void ButtonToggleOverlay_Click(object sender, RoutedEventArgs e)
        {
            if (DrawOverlay)
            {
                DrawOverlay = false;
                buttonToggleOverlay.Foreground = darkForegroundBrush;
            }
            else
            {
                DrawOverlay = true;
                buttonToggleOverlay.Foreground = currentTickTimeGoodBrush;
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
                }

                if (AmeisenBot.WowInterface.ObjectManager?.Player != null)
                {
                    labelDebug.Content = AmeisenBot.WowInterface.ObjectManager.Player.Position;
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
                    UpdateBottomLabels();
                    LastStateMachineTickUpdate = DateTime.Now;
                }

                if (Overlay == null)
                {
                    Overlay = new AmeisenBotOverlay(AmeisenBot.WowInterface.XMemory);
                }
                else
                {
                    if (DrawOverlay)
                    {
                        if (AmeisenBot.WowInterface.MovementEngine.Path != null
                        && AmeisenBot.WowInterface.MovementEngine.Path.Count > 0)
                        {
                            for (int i = 0; i < AmeisenBot.WowInterface.MovementEngine.Path.Count && i < 10; ++i)
                            {
                                Vector3 start = AmeisenBot.WowInterface.MovementEngine.Path[i];
                                Vector3 end = i == 0 ? AmeisenBot.WowInterface.ObjectManager.Player.Position : AmeisenBot.WowInterface.MovementEngine.Path[i - 1];

                                Color lineColor = Colors.LightCyan;
                                Color startDot = Colors.Cyan;
                                Color endDot = i == 0 ? Colors.Navy : Colors.Cyan;

                                Memory.Win32.Rect windowRect = XMemory.GetWindowPosition(AmeisenBot.WowInterface.XMemory.Process.MainWindowHandle);
                                if (OverlayMath.WorldToScreen(windowRect, AmeisenBot.WowInterface.ObjectManager.Camera, start, out Point startPoint)
                                && OverlayMath.WorldToScreen(windowRect, AmeisenBot.WowInterface.ObjectManager.Camera, end, out Point endPoint))
                                {
                                    Overlay.AddLine((int)startPoint.X, (int)startPoint.Y, (int)endPoint.X, (int)endPoint.Y, lineColor);
                                    Overlay.AddRectangle((int)startPoint.X - 3, (int)startPoint.Y - 3, 5, 5, startDot);
                                    Overlay.AddRectangle((int)endPoint.X - 3, (int)endPoint.Y - 3, 5, 5, endDot);
                                }
                            }
                        }

                        if (AmeisenBot.WowInterface.DungeonEngine.Nodes != null
                        && AmeisenBot.WowInterface.DungeonEngine.Nodes.Count > 0)
                        {
                            for (int i = 0; i < AmeisenBot.WowInterface.DungeonEngine.Nodes.Count && i < 32; ++i)
                            {
                                Vector3 start = AmeisenBot.WowInterface.DungeonEngine.Nodes[i].Position;
                                Vector3 end = i == 0 ? AmeisenBot.WowInterface.ObjectManager.Player.Position : AmeisenBot.WowInterface.DungeonEngine.Nodes[i - 1].Position;

                                Color lineColor = Colors.White;
                                Color startDot = Colors.Gray;
                                Color endDot = i == 0 ? Colors.Orange : Colors.Gray;

                                Memory.Win32.Rect windowRect = XMemory.GetWindowPosition(AmeisenBot.WowInterface.XMemory.Process.MainWindowHandle);
                                if (OverlayMath.WorldToScreen(windowRect, AmeisenBot.WowInterface.ObjectManager.Camera, start, out Point startPoint)
                                && OverlayMath.WorldToScreen(windowRect, AmeisenBot.WowInterface.ObjectManager.Camera, end, out Point endPoint))
                                {
                                    Overlay.AddLine((int)startPoint.X, (int)startPoint.Y, (int)endPoint.X, (int)endPoint.Y, lineColor);
                                    Overlay.AddRectangle((int)startPoint.X - 3, (int)startPoint.Y - 3, 5, 5, startDot);
                                    Overlay.AddRectangle((int)endPoint.X - 3, (int)endPoint.Y - 3, 5, 5, endDot);
                                }
                            }
                        }

                        Overlay?.Draw();
                    }
                    else
                    {
                        Overlay.Clear();
                    }
                }
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

        private void UpdateBottomLabels()
        {
            double executionMs = AmeisenBot.CurrentExecutionMs;
            if (double.IsNaN(executionMs) || double.IsInfinity(executionMs))
            {
                executionMs = 0;
            }

            // update the object count label
            labelCurrentObjectCount.Content = AmeisenBot.WowInterface.ObjectManager.WowObjects.Count.ToString().PadLeft(4);

            labelCurrentTickTime.Content = executionMs.ToString().PadLeft(4);
            if (executionMs <= Config.StateMachineTickMs)
            {
                labelCurrentTickTime.Foreground = currentTickTimeGoodBrush;
            }
            else
            {
                labelCurrentTickTime.Foreground = currentTickTimeBadBrush;
                AmeisenLogger.Instance.Log("MainWindow", "High executionMs, something blocks our thread or CPU is to slow...", LogLevel.Warning);
            }

            labelHookCallCount.Content = AmeisenBot.WowInterface.HookManager.CallCount.ToString().PadLeft(2);
            if (AmeisenBot.WowInterface.HookManager.CallCount <= (AmeisenBot.WowInterface.ObjectManager.Player.IsInCombat ? (ulong)Config.MaxFpsCombat : (ulong)Config.MaxFps))
            {
                labelHookCallCount.Foreground = currentTickTimeGoodBrush;
            }
            else
            {
                labelHookCallCount.Foreground = currentTickTimeBadBrush;
                AmeisenLogger.Instance.Log("MainWindow", "High HookCall count, maybe increase your FPS...", LogLevel.Warning);
            }

            labelRpmCallCount.Content = AmeisenBot.WowInterface.XMemory.RpmCallCount.ToString().PadLeft(5);
            labelWpmCallCount.Content = AmeisenBot.WowInterface.XMemory.WpmCallCount.ToString().PadLeft(3);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AmeisenBot?.Stop();
            Overlay?.Exit();
            InfoWindow?.Close();
            MapWindow?.Close();
            SaveConfig();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Config != null)
            {
                string playername = Path.GetFileName(Path.GetDirectoryName(ConfigPath));
                AmeisenBot = new AmeisenBot(BotDataPath, playername, Config, Process.GetCurrentProcess().MainWindowHandle);

                AmeisenBot.WowInterface.ObjectManager.OnObjectUpdateComplete += OnObjectUpdateComplete;
                AmeisenBot.StateMachine.OnStateMachineStateChanged += OnStateMachineStateChange;

                LastStateMachineTickUpdate = DateTime.Now;
            }

            if (AmeisenBot != null)
            {
                AmeisenBot.Start();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}