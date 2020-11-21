using AmeisenBotX.Core;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Overlay;
using AmeisenBotX.Overlay.Utils;
using AmeisenBotX.StateConfig;
using AmeisenBotX.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace AmeisenBotX
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Config = LoadConfig();

            CurrentTickTimeBadBrush = new SolidColorBrush(Color.FromRgb(255, 0, 80));
            CurrentTickTimeGoodBrush = new SolidColorBrush(Color.FromRgb(160, 255, 0));
            DarkForegroundBrush = new SolidColorBrush((Color)FindResource("DarkForeground"));
            DarkBackgroundBrush = new SolidColorBrush((Color)FindResource("DarkBackground"));
            TextAccentBrush = new SolidColorBrush((Color)FindResource("TextAccent"));

            CurrentTickTimeBadBrush.Freeze();
            CurrentTickTimeGoodBrush.Freeze();
            DarkForegroundBrush.Freeze();
            DarkBackgroundBrush.Freeze();
            TextAccentBrush.Freeze();

            LabelUpdateEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            NotificationEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            RenderState = true;
        }

        public AmeisenBotConfig Config { get; private set; }

        public string ConfigPath { get; private set; }

        public WindowInteropHelper InteropHelper { get; private set; }

        public bool IsAutoPositionSetup { get; private set; }

        public double M11 { get; private set; }

        public double M22 { get; private set; }

        public AmeisenBotOverlay Overlay { get; private set; }

        public bool RenderState { get; set; }

        private AmeisenBot AmeisenBot { get; set; }

        private Brush CurrentTickTimeBadBrush { get; }

        private Brush CurrentTickTimeGoodBrush { get; }

        private Brush DarkBackgroundBrush { get; }

        private Brush DarkForegroundBrush { get; }

        private string DataPath { get; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\AmeisenBotX\\profiles\\";

        private DevToolsWindow DevToolsWindow { get; set; }

        private bool DrawOverlay { get; set; }

        private InfoWindow InfoWindow { get; set; }

        private TimegatedEvent LabelUpdateEvent { get; }

        private MapWindow MapWindow { get; set; }

        private bool NeedToClearOverlay { get; set; }

        private SolidColorBrush NoticifactionColor { get; set; }

        private bool NotificationBlinkState { get; set; }

        private TimegatedEvent NotificationEvent { get; }

        private long NotificationLastTimestamp { get; set; }

        private bool PendingNotification { get; set; }

        private RelationshipWindow RelationshipWindow { get; set; }

        private Dictionary<BotState, Window> StateConfigWindows { get; set; }

        private Brush TextAccentBrush { get; }

        /// <summary>
        /// Used to resize the wow window when autoposition is enabled
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = base.MeasureOverride(availableSize);

            if (AmeisenBot != null && IsAutoPositionSetup)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    AmeisenBot.WowInterface.XMemory.ResizeParentWindow
                    (
                        (int)(wowRect.Margin.Left * M11 + 1),
                        (int)(wowRect.Margin.Top * M22 + 1),
                        (int)(wowRect.ActualWidth * M11),
                        (int)(wowRect.ActualHeight * M22)
                    );
                });
            }

            return size;
        }

        private void ButtonClearCache_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBot.WowInterface.Db.Clear();
        }

        private void ButtonConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigEditorWindow configWindow = new ConfigEditorWindow(DataPath, AmeisenBot, Config, Path.GetFileName(Path.GetDirectoryName(ConfigPath)));
            configWindow.ShowDialog();

            if (configWindow.SaveConfig)
            {
                AmeisenBot.Config = configWindow.Config;
                AmeisenBot.ReloadConfig();
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(configWindow.Config, Formatting.Indented));
            }
        }

        private void ButtonDevTools_Click(object sender, RoutedEventArgs e)
        {
            DevToolsWindow ??= new DevToolsWindow(AmeisenBot);
            DevToolsWindow.Show();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonNotification_Click(object sender, RoutedEventArgs e)
        {
            PendingNotification = false;
            NotificationBlinkState = false;

            buttonNotification.Foreground = new SolidColorBrush(Colors.White);
            buttonNotification.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void ButtonStartPause_Click(object sender, RoutedEventArgs e)
        {
            if (AmeisenBot.IsRunning)
            {
                AmeisenBot.Pause();
                buttonStartPause.Content = "▶";
                buttonStartPause.Foreground = TextAccentBrush;
            }
            else
            {
                AmeisenBot.Resume();
                buttonStartPause.Content = "||";
                buttonStartPause.Foreground = DarkForegroundBrush;
            }
        }

        private void ButtonStateConfig_Click(object sender, RoutedEventArgs e)
        {
            if (StateConfigWindows.ContainsKey((BotState)comboboxStateOverride.SelectedItem))
            {
                Window selectedWindow = StateConfigWindows[(BotState)comboboxStateOverride.SelectedItem];
                selectedWindow.ShowDialog();

                if (((IStateConfigWindow)selectedWindow).ShouldSave)
                {
                    AmeisenBot.Config = ((IStateConfigWindow)selectedWindow).Config;
                    AmeisenBot.ReloadConfig();
                    SaveConfig();
                }
            }
        }

        private void ButtonToggleAutopilot_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Config.Autopilot = !AmeisenBot.Config.Autopilot;
            buttonToggleAutopilot.Foreground = AmeisenBot.Config.Autopilot ? CurrentTickTimeGoodBrush : DarkForegroundBrush;
        }

        private void ButtonToggleInfoWindow_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow ??= new InfoWindow(AmeisenBot);
            InfoWindow.Show();
        }

        private void ButtonToggleMapWindow_Click(object sender, RoutedEventArgs e)
        {
            MapWindow ??= new MapWindow(AmeisenBot);
            MapWindow.Show();
        }

        private void ButtonToggleOverlay_Click(object sender, RoutedEventArgs e)
        {
            DrawOverlay = !DrawOverlay;
            buttonToggleOverlay.Foreground = DrawOverlay ? CurrentTickTimeGoodBrush : DarkForegroundBrush;
        }

        private void ButtonToggleRelationshipWindow_Click(object sender, RoutedEventArgs e)
        {
            RelationshipWindow ??= new RelationshipWindow(AmeisenBot);
            RelationshipWindow.Show();
        }

        private void ButtonToggleRendering_Click(object sender, RoutedEventArgs e)
        {
            RenderState = !RenderState;
            AmeisenBot.WowInterface.HookManager.WowSetRenderState(RenderState);
        }

        private void ComboboxStateOverride_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AmeisenBot != null)
            {
                AmeisenBot.StateMachine.StateOverride = (BotState)comboboxStateOverride.SelectedItem;
                buttonStateConfig.IsEnabled = StateConfigWindows.ContainsKey((BotState)comboboxStateOverride.SelectedItem);
            }
        }

        private AmeisenBotConfig LoadConfig()
        {
            if (Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }

            // check for older data folder
            string oldDataPath = $"{Directory.GetCurrentDirectory()}\\data\\";

            if (Directory.Exists(oldDataPath))
            {
                MessageBox.Show("You need to move the content of your \"\\\\data\\\\\" folder to \"%AppData%\\\\Roaming\\\\AmeisenbotX\\\\profiles\\\\\". Otherwise your profiles may not be displayed.", "New Data Location", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            LoadConfigWindow loadConfigWindow = new LoadConfigWindow(DataPath);
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

        private void MainWindow_OnWoWStarted()
        {
            if (Config.AutoPositionWow)
            {
                Dispatcher.Invoke(() =>
                {
                    AmeisenBot.WowInterface.XMemory.SetupAutoPosition
                    (
                        InteropHelper.EnsureHandle(),
                        (int)(wowRect.Margin.Left * M11 + 1),
                        (int)(wowRect.Margin.Top * M22 + 1),
                        (int)(wowRect.ActualWidth * M11),
                        (int)(wowRect.ActualHeight * M22)
                    );
                });

                IsAutoPositionSetup = true;
            }
        }

        private void OnObjectUpdateComplete(IEnumerable<WowObject> wowObjects)
        {
            Dispatcher.InvokeAsync(() =>
            {
                // Notification Symbol
                // ------------------- >
                if (NotificationEvent.Run())
                {
                    if (!PendingNotification)
                    {
                        WowChatMessage message = AmeisenBot.WowInterface.ChatManager.ChatMessages
                            .Where(e => e.Timestamp > NotificationLastTimestamp)
                            .FirstOrDefault(e => e.Type == ChatMessageType.WHISPER);

                        if (message != null)
                        {
                            PendingNotification = true;
                            NotificationLastTimestamp = message.Timestamp;

                            if (message.Flags.Contains("GM", StringComparison.OrdinalIgnoreCase))
                            {
                                NoticifactionColor = new SolidColorBrush(Colors.Cyan);
                            }
                            else
                            {
                                NoticifactionColor = new SolidColorBrush(Colors.Magenta);
                            }
                        }
                    }
                    else
                    {
                        if (NotificationBlinkState)
                        {
                            buttonNotification.Foreground = DarkBackgroundBrush;
                            buttonNotification.Background = NoticifactionColor;
                        }
                        else
                        {
                            buttonNotification.Foreground = new SolidColorBrush(Colors.White);
                            buttonNotification.Background = new SolidColorBrush(Colors.Transparent);
                        }

                        NotificationBlinkState = !NotificationBlinkState;
                    }
                }

                // Update the main view
                // -------------------- >
                WowPlayer player = AmeisenBot.WowInterface.ObjectManager.Player;

                switch (player.Class)
                {
                    case WowClass.Deathknight:
                        UpdateBotInfo(player.MaxRuneenergy, player.Runeenergy, WowColors.dkPrimaryBrush, WowColors.dkSecondaryBrush);
                        break;

                    case WowClass.Druid:
                        UpdateBotInfo(player.MaxMana, player.Mana, WowColors.druidPrimaryBrush, WowColors.druidSecondaryBrush);
                        break;

                    case WowClass.Hunter:
                        UpdateBotInfo(player.MaxMana, player.Mana, WowColors.hunterPrimaryBrush, WowColors.hunterSecondaryBrush);
                        break;

                    case WowClass.Mage:
                        UpdateBotInfo(player.MaxMana, player.Mana, WowColors.magePrimaryBrush, WowColors.mageSecondaryBrush);
                        break;

                    case WowClass.Paladin:
                        UpdateBotInfo(player.MaxMana, player.Mana, WowColors.paladinPrimaryBrush, WowColors.paladinSecondaryBrush);
                        break;

                    case WowClass.Priest:
                        UpdateBotInfo(player.MaxMana, player.Mana, WowColors.priestPrimaryBrush, WowColors.priestSecondaryBrush);
                        break;

                    case WowClass.Rogue:
                        UpdateBotInfo(player.MaxEnergy, player.Energy, WowColors.roguePrimaryBrush, WowColors.rogueSecondaryBrush);
                        break;

                    case WowClass.Shaman:
                        UpdateBotInfo(player.MaxMana, player.Mana, WowColors.shamanPrimaryBrush, WowColors.shamanSecondaryBrush);
                        break;

                    case WowClass.Warlock:
                        UpdateBotInfo(player.MaxMana, player.Mana, WowColors.warlockPrimaryBrush, WowColors.warlockSecondaryBrush);
                        break;

                    case WowClass.Warrior:
                        UpdateBotInfo(player.MaxRage, player.Rage, WowColors.warriorPrimaryBrush, WowColors.warriorSecondaryBrush);
                        break;
                }

                // Bottom labels
                // ------------- >
                if (LabelUpdateEvent.Run())
                {
                    UpdateBottomLabels();
                }

                // Overlay drawing
                // --------------- >
                if (DrawOverlay)
                {
                    Overlay ??= new AmeisenBotOverlay(AmeisenBot.WowInterface.XMemory);
                    OverlayRenderCurrentPath();

                    Overlay?.Draw();
                    NeedToClearOverlay = true;
                }
                else if (NeedToClearOverlay)
                {
                    Overlay.Clear();
                    NeedToClearOverlay = false;
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

        private void OverlayRenderCurrentPath()
        {
            if (AmeisenBot.WowInterface.MovementEngine.Path != null
            && AmeisenBot.WowInterface.MovementEngine.Path.Count > 0)
            {
                List<Vector3> currentNodes = AmeisenBot.WowInterface.MovementEngine.Path.ToList();

                for (int i = 0; i < currentNodes.Count; ++i)
                {
                    Vector3 start = currentNodes[i];
                    Vector3 end = i == 0 ? AmeisenBot.WowInterface.ObjectManager.Player.Position : currentNodes[i - 1];

                    System.Drawing.Color lineColor = System.Drawing.Color.White;
                    System.Drawing.Color startDot = System.Drawing.Color.Cyan;
                    System.Drawing.Color endDot = i == 0 ? System.Drawing.Color.Orange : i == currentNodes.Count ? System.Drawing.Color.Orange : System.Drawing.Color.Cyan;

                    Memory.Win32.Rect windowRect = XMemory.GetWindowPosition(AmeisenBot.WowInterface.XMemory.Process.MainWindowHandle);
                    if (OverlayMath.WorldToScreen(windowRect, AmeisenBot.WowInterface.ObjectManager.Camera, start, out System.Drawing.Point startPoint)
                        && OverlayMath.WorldToScreen(windowRect, AmeisenBot.WowInterface.ObjectManager.Camera, end, out System.Drawing.Point endPoint))
                    {
                        Overlay.AddLine((int)startPoint.X, (int)startPoint.Y, (int)endPoint.X, (int)endPoint.Y, lineColor);
                        Overlay.AddRectangle((int)startPoint.X - 4, (int)startPoint.Y - 4, 7, 7, startDot);
                        Overlay.AddRectangle((int)endPoint.X - 4, (int)endPoint.Y - 4, 7, 7, endDot);
                    }
                }
            }
        }

        private void SaveConfig()
        {
            if (Config != null
                && !string.IsNullOrEmpty(ConfigPath)
                && Directory.Exists(Path.GetDirectoryName(ConfigPath)))
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(AmeisenBot.Config, Formatting.Indented));
            }
        }

        private void UpdateBotInfo(int maxSecondary, int secondary, Brush primaryBrush, Brush secondaryBrush)
        {
            // Generic labels
            // ------------- >
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

            // Class specific labels
            // --------------------- >
            progressbarSecondary.Maximum = maxSecondary;
            progressbarSecondary.Value = secondary;
            labelCurrentSecondary.Content = BotUtils.BigValueToString(secondary);

            // Colors
            // ------ >
            progressbarHealth.Foreground = primaryBrush;
            progressbarSecondary.Foreground = secondaryBrush;
            labelCurrentClass.Foreground = primaryBrush;
        }

        private void UpdateBottomLabels()
        {
            // Object count label
            // ------------------ >
            labelCurrentObjectCount.Content = AmeisenBot.WowInterface.ObjectManager.ObjectCount.ToString(CultureInfo.InvariantCulture).PadLeft(4);

            // Tick time label
            // --------------- >
            double executionMs = AmeisenBot.CurrentExecutionMs;

            if (double.IsNaN(executionMs) || double.IsInfinity(executionMs))
            {
                executionMs = 0;
            }

            labelCurrentTickTime.Content = executionMs.ToString(CultureInfo.InvariantCulture).PadLeft(4);

            if (executionMs <= Config.StateMachineTickMs)
            {
                labelCurrentTickTime.Foreground = CurrentTickTimeGoodBrush;
            }
            else
            {
                labelCurrentTickTime.Foreground = CurrentTickTimeBadBrush;
                AmeisenLogger.I.Log("MainWindow", "High executionMs, something blocks our thread or CPU is to slow", LogLevel.Warning);
            }

            // HookCall label
            // -------------- >
            labelHookCallCount.Content = AmeisenBot.WowInterface.HookManager.HookCallCount.ToString(CultureInfo.InvariantCulture).PadLeft(2);

            if (AmeisenBot.WowInterface.HookManager.HookCallCount <= (AmeisenBot.WowInterface.ObjectManager.Player.IsInCombat ? (ulong)Config.MaxFpsCombat : (ulong)Config.MaxFps))
            {
                labelHookCallCount.Foreground = CurrentTickTimeGoodBrush;
            }
            else
            {
                labelHookCallCount.Foreground = CurrentTickTimeBadBrush;
                AmeisenLogger.I.Log("MainWindow", "High HookCall count, maybe increase your FPS", LogLevel.Warning);
            }

            // RPM/WPM labels
            // -------------- >
            labelRpmCallCount.Content = AmeisenBot.WowInterface.XMemory.RpmCallCount.ToString(CultureInfo.InvariantCulture).PadLeft(5);
            labelWpmCallCount.Content = AmeisenBot.WowInterface.XMemory.WpmCallCount.ToString(CultureInfo.InvariantCulture).PadLeft(3);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AmeisenBot?.Stop();
            Overlay?.Exit();
            InfoWindow?.Close();
            MapWindow?.Close();
            DevToolsWindow?.Close();
            RelationshipWindow?.Close();

            if (StateConfigWindows != null)
            {
                foreach (Window window in StateConfigWindows.Values)
                {
                    window.Close();
                }
            }

            SaveConfig();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Init GUI stuff
            // -------------- >
            InteropHelper = new WindowInteropHelper(this);

            PresentationSource presentationSource = PresentationSource.FromVisual(this);
            M11 = presentationSource.CompositionTarget.TransformToDevice.M11;
            M22 = presentationSource.CompositionTarget.TransformToDevice.M22;

            comboboxStateOverride.Items.Add(BotState.Idle);
            comboboxStateOverride.Items.Add(BotState.Attacking);
            comboboxStateOverride.Items.Add(BotState.Grinding);
            comboboxStateOverride.Items.Add(BotState.Job);
            comboboxStateOverride.Items.Add(BotState.Questing);

            comboboxStateOverride.SelectedIndex = 0;

            labelPID.Content = $"PID: {Environment.ProcessId}";

            if (Config.Autopilot)
            {
                buttonToggleAutopilot.Foreground = CurrentTickTimeGoodBrush;
            }

            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }

            // Init the bot
            // ------------ >
            AmeisenBot = new AmeisenBot(DataPath, Path.GetFileName(Path.GetDirectoryName(ConfigPath)), Config, InteropHelper.EnsureHandle());

            AmeisenBot.WowInterface.ObjectManager.OnObjectUpdateComplete += OnObjectUpdateComplete;
            AmeisenBot.StateMachine.OnStateMachineStateChanged += OnStateMachineStateChange;
            AmeisenBot.StateMachine.GetState<StateStartWow>().OnWoWStarted += MainWindow_OnWoWStarted;

            StateConfigWindows = new Dictionary<BotState, Window>()
            {
                { BotState.Grinding, new StateGrindingConfigWindow(AmeisenBot, AmeisenBot.Config)},
                { BotState.Job, new StateJobConfigWindow(AmeisenBot, AmeisenBot.Config)},
                { BotState.Questing, new StateQuestingConfigWindow(AmeisenBot, AmeisenBot.Config)},
            };

            AmeisenBot.Start();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}