using AmeisenBotX.Core;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.States;
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
using System.Threading.Tasks;
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
        private readonly Brush dkSecondaryBrush = new SolidColorBrush(Color.FromRgb(0, 209, 255));

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

        public bool RenderState { get; set; }

        private AmeisenBot AmeisenBot { get; set; }

        private bool DrawOverlay { get; set; }

        private InfoWindow InfoWindow { get; set; }

        private Memory.Win32.Rect LastBotWindowPosition { get; set; }

        private DateTime LastStateMachineTickUpdate { get; set; }

        private double M11 { get; set; }

        private double M22 { get; set; }

        private MapWindow MapWindow { get; set; }

        private bool OverlayClear { get; set; }

        private PresentationSource PresentationSource { get; set; }

        private bool SetupWindowOwner { get; set; }

        private void AdjustWowWindow()
        {
            Point screenCoordinates = PointToScreen(new Point(0, 0));
            Point screenCoordinatesWowRect = wowRect.PointToScreen(new Point(0, 0));

            double pixelHeight = Height * M22;
            double pixelWidth = Width * M11;

            double pixelHeightWowRect = wowRect.ActualHeight * M22;
            double pixelWidthWowRect = wowRect.ActualWidth * M11;

            Memory.Win32.Rect botPos = new Memory.Win32.Rect()
            {
                Left = (int)screenCoordinates.X,
                Bottom = (int)screenCoordinates.Y + (int)pixelHeight,
                Top = (int)screenCoordinates.Y,
                Right = (int)screenCoordinates.X + (int)pixelWidth
            };

            // Memory.Win32.Rect wowPos = AmeisenBot.WowInterface.XMemory.GetWindowPositionWow();

            if (botPos != LastBotWindowPosition)
            {
                // int height = (int)Math.Ceiling(pixelHeight * (4.0 / 3.0));
                // int width = (int)Math.Ceiling(pixelHeight * 1.25);

                Memory.Win32.Rect newPos = new Memory.Win32.Rect()
                {
                    Left = (int)screenCoordinatesWowRect.X,
                    Bottom = (int)screenCoordinatesWowRect.Y + (int)pixelHeightWowRect,
                    Top = (int)screenCoordinatesWowRect.Y,
                    Right = (int)screenCoordinatesWowRect.X + (int)pixelWidthWowRect
                };

                Task.Run(() => AmeisenBot.WowInterface.XMemory.SetWindowPositionWow(newPos));
                LastBotWindowPosition = botPos;
            }
        }

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

        private void ButtonDbg_Click(object sender, RoutedEventArgs e)
        {
            // AmeisenBot.WowInterface.XMemory.SetWindowParent(AmeisenBot.WowInterface.WowProcess.MainWindowHandle, Process.GetCurrentProcess().MainWindowHandle);
        }

        private void ButtonDebug_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBot.WowInterface.HookManager.SetRenderState(RenderState);
            RenderState = !RenderState;
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonFaceTarget_Click(object sender, RoutedEventArgs e)
        {
            float angle = BotMath.GetFacingAngle2D(AmeisenBot.WowInterface.ObjectManager.Player.Position, AmeisenBot.WowInterface.ObjectManager.Target.Position);
            AmeisenBot.WowInterface.HookManager.SetFacing(AmeisenBot.WowInterface.ObjectManager.Player, angle);
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(Config).ShowDialog();
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

        private void ComboboxStateOverride_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            AmeisenBot.StateMachine.StateOverride = (BotState)comboboxStateOverride.SelectedItem;
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
                if (AmeisenBot?.WowInterface?.XMemory?.Process != null && AmeisenBot.Config.AutoPositionWow && !SetupWindowOwner)
                {
                    AmeisenBot.WowInterface.XMemory.SetWowWindowOwner(Process.GetCurrentProcess().MainWindowHandle);
                    SetupWindowOwner = true;
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
                            for (int i = 0; i < AmeisenBot.WowInterface.MovementEngine.Path.Count; ++i)
                            {
                                Vector3 start = AmeisenBot.WowInterface.MovementEngine.Path[i];
                                Vector3 end = i == 0 ? AmeisenBot.WowInterface.ObjectManager.Player.Position : AmeisenBot.WowInterface.MovementEngine.Path[i - 1];

                                Color lineColor = Colors.LightCyan;
                                Color startDot = Colors.Red;
                                Color endDot = i == 0 ? Colors.Orange : i == AmeisenBot.WowInterface.MovementEngine.Path.Count ? Colors.Orange : Colors.Cyan;

                                Memory.Win32.Rect windowRect = XMemory.GetWindowPosition(AmeisenBot.WowInterface.XMemory.Process.MainWindowHandle);
                                if (OverlayMath.WorldToScreen(windowRect, AmeisenBot.WowInterface.ObjectManager.Camera, start, out Point startPoint)
                                && OverlayMath.WorldToScreen(windowRect, AmeisenBot.WowInterface.ObjectManager.Camera, end, out Point endPoint))
                                {
                                    Overlay.AddLine((int)startPoint.X, (int)startPoint.Y, (int)endPoint.X, (int)endPoint.Y, lineColor);
                                    Overlay.AddRectangle((int)startPoint.X - 4, (int)startPoint.Y - 4, 7, 7, startDot);
                                    Overlay.AddRectangle((int)endPoint.X - 4, (int)endPoint.Y - 4, 7, 7, endDot);
                                }
                            }
                        }

                        Overlay?.Draw();
                        OverlayClear = true;
                    }
                    else if (OverlayClear)
                    {
                        Overlay.Clear();
                        OverlayClear = false;
                    }
                }
            });
        }

        private void OnStateMachineStateChange()
        {
            Dispatcher.InvokeAsync(() =>
            {
                labelCurrentState.Content = $"{(BotState)AmeisenBot.StateMachine.CurrentState.Key}";
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

            labelCurrentLevel.Content = $"{AmeisenBot.WowInterface.ObjectManager.Player.Level} (iLvl. {Math.Round(AmeisenBot.WowInterface.CharacterManager.Equipment.AverageItemLevel)})";
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
                AmeisenLogger.Instance.Log("MainWindow", "High executionMs, something blocks our thread or CPU is to slow", LogLevel.Warning);
            }

            labelHookCallCount.Content = AmeisenBot.WowInterface.HookManager.PendingCallCount.ToString().PadLeft(2);
            if (AmeisenBot.WowInterface.HookManager.PendingCallCount <= (AmeisenBot.WowInterface.ObjectManager.Player.IsInCombat ? (ulong)Config.MaxFpsCombat : (ulong)Config.MaxFps))
            {
                labelHookCallCount.Foreground = currentTickTimeGoodBrush;
            }
            else
            {
                labelHookCallCount.Foreground = currentTickTimeBadBrush;
                AmeisenLogger.Instance.Log("MainWindow", "High HookCall count, maybe increase your FPS", LogLevel.Warning);
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
            labelPID.Content = $"PID: {Process.GetCurrentProcess().Id}";

            if (Config != null)
            {
                string playername = Path.GetFileName(Path.GetDirectoryName(ConfigPath));
                AmeisenBot = new AmeisenBot(BotDataPath, playername, Config, Process.GetCurrentProcess().MainWindowHandle);

                AmeisenBot.WowInterface.ObjectManager.OnObjectUpdateComplete += OnObjectUpdateComplete;
                AmeisenBot.StateMachine.OnStateMachineStateChanged += OnStateMachineStateChange;

                LastStateMachineTickUpdate = DateTime.Now;
                PresentationSource = PresentationSource.FromVisual(this);

                M11 = PresentationSource.CompositionTarget.TransformToDevice.M11;
                M22 = PresentationSource.CompositionTarget.TransformToDevice.M22;
            }

            comboboxStateOverride.Items.Add(BotState.Idle);
            comboboxStateOverride.SelectedIndex = 0;

            comboboxStateOverride.Items.Add(BotState.Job);
            comboboxStateOverride.Items.Add(BotState.Questing);

            if (AmeisenBot != null)
            {
                AmeisenBot.Start();
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (AmeisenBot?.WowInterface?.XMemory?.Process != null && AmeisenBot.Config.AutoPositionWow)
            {
                AdjustWowWindow();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void WowRect_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AmeisenBot?.WowInterface?.XMemory?.Process != null && AmeisenBot.Config.AutoPositionWow)
            {
                AdjustWowWindow();
            }
        }
    }
}