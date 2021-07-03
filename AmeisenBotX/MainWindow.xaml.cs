using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Fsm.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Overlay;
using AmeisenBotX.Overlay.Utils;
using AmeisenBotX.StateConfig;
using AmeisenBotX.Utils;
using AmeisenBotX.Wow.Objects.Enums;
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

            LabelUpdateEvent = new(TimeSpan.FromSeconds(1));
            NotificationEvent = new(TimeSpan.FromSeconds(1));

            RenderState = true;
        }

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

        private IntPtr MainWindowHandle { get; set; }

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
                    AmeisenBot.Bot.Memory.ResizeParentWindow
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

        private static bool TryLoadConfig(string configPath, out AmeisenBotConfig config)
        {
            if (!string.IsNullOrWhiteSpace(configPath))
            {
                if (File.Exists(configPath))
                {
                    config = JsonConvert.DeserializeObject<AmeisenBotConfig>(File.ReadAllText(configPath));
                }
                else
                {
                    config = new();
                }

                config.Path = configPath;
                return true;
            }

            config = null;
            return false;
        }

        private void ButtonClearCache_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Bot.Db.Clear();
        }

        private void ButtonConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigEditorWindow configWindow = new(DataPath, AmeisenBot, AmeisenBot.Config, AmeisenBot.AccountName);
            configWindow.ShowDialog();

            if (configWindow.SaveConfig)
            {
                AmeisenBot.ReloadConfig(configWindow.Config);
                File.WriteAllText(AmeisenBot.Config.Path, JsonConvert.SerializeObject(configWindow.Config, Formatting.Indented));
            }
        }

        private void ButtonDevTools_Click(object sender, RoutedEventArgs e)
        {
            DevToolsWindow ??= new(AmeisenBot);
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
                    AmeisenBot.ReloadConfig(((IStateConfigWindow)selectedWindow).Config);
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
            InfoWindow ??= new(AmeisenBot);
            InfoWindow.Show();
        }

        private void ButtonToggleMapWindow_Click(object sender, RoutedEventArgs e)
        {
            MapWindow ??= new(AmeisenBot);
            MapWindow.Show();
        }

        private void ButtonToggleOverlay_Click(object sender, RoutedEventArgs e)
        {
            DrawOverlay = !DrawOverlay;
            buttonToggleOverlay.Foreground = DrawOverlay ? CurrentTickTimeGoodBrush : DarkForegroundBrush;
        }

        private void ButtonToggleRelationshipWindow_Click(object sender, RoutedEventArgs e)
        {
            RelationshipWindow ??= new(AmeisenBot);
            RelationshipWindow.Show();
        }

        private void ButtonToggleRendering_Click(object sender, RoutedEventArgs e)
        {
            RenderState = !RenderState;
            AmeisenBot.Bot.Wow.WowSetRenderState(RenderState);
        }

        private void ComboboxStateOverride_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AmeisenBot != null)
            {
                AmeisenBot.StateMachine.StateOverride = (BotState)comboboxStateOverride.SelectedItem;
                buttonStateConfig.IsEnabled = StateConfigWindows.ContainsKey((BotState)comboboxStateOverride.SelectedItem);
            }
        }

        private void OnObjectUpdateComplete(IEnumerable<WowObject> wowObjects)
        {
            Dispatcher.InvokeAsync(() =>
            {
                // Notification Symbol
                if (NotificationEvent.Run())
                {
                    if (!PendingNotification)
                    {
                        WowChatMessage message = AmeisenBot.Bot.Chat.ChatMessages
                            .Where(e => e.Timestamp > NotificationLastTimestamp)
                            .FirstOrDefault(e => e.Type == WowChat.WHISPER);

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
                WowPlayer player = AmeisenBot.Bot.Player;

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
                if (LabelUpdateEvent.Run())
                {
                    UpdateBottomLabels();
                }

                // Overlay drawing
                if (DrawOverlay)
                {
                    Overlay ??= new(AmeisenBot.Bot.Memory.Process.MainWindowHandle);
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

        private void OverlayRenderCurrentPath()
        {
            if (AmeisenBot.Bot.Movement.Path != null
                && AmeisenBot.Bot.Movement.Path.Any())
            {
                // explicitly copy the path as it might change during rendering
                List<Vector3> currentNodes = AmeisenBot.Bot.Movement.Path.ToList();

                for (int i = 0; i < currentNodes.Count; ++i)
                {
                    Vector3 start = currentNodes[i];
                    Vector3 end = i == 0 ? AmeisenBot.Bot.Player.Position : currentNodes[i - 1];

                    System.Drawing.Color lineColor = System.Drawing.Color.White;
                    System.Drawing.Color startDot = System.Drawing.Color.Cyan;
                    System.Drawing.Color endDot = i == 0 ? System.Drawing.Color.Orange : i == currentNodes.Count ? System.Drawing.Color.Orange : System.Drawing.Color.Cyan;

                    Memory.Win32.Rect windowRect = AmeisenBot.Bot.Memory.GetClientSize();

                    if (OverlayMath.WorldToScreen(windowRect, AmeisenBot.Bot.Objects.Camera, start, out System.Drawing.Point startPoint)
                        && OverlayMath.WorldToScreen(windowRect, AmeisenBot.Bot.Objects.Camera, end, out System.Drawing.Point endPoint))
                    {
                        Overlay.AddLine(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, lineColor);
                        Overlay.AddRectangle(startPoint.X - 4, startPoint.Y - 4, 7, 7, startDot);
                        Overlay.AddRectangle(endPoint.X - 4, endPoint.Y - 4, 7, 7, endDot);
                    }
                }
            }
        }

        private void SaveBotWindowPosition()
        {
            if (AmeisenBot != null && AmeisenBot.Config != null && AmeisenBot.Config.SaveBotWindowPosition)
            {
                try
                {
                    Memory.Win32.Rect rc = new();
                    Memory.Win32.Win32Imports.GetWindowRect(MainWindowHandle, ref rc);
                    AmeisenBot.Config.BotWindowRect = rc;
                }
                catch (Exception e)
                {
                    AmeisenLogger.I.Log("AmeisenBot", $"Failed to save bot window position:\n{e}", LogLevel.Error);
                }
            }
        }

        private void SaveConfig()
        {
            if (AmeisenBot != null
                && AmeisenBot.Config != null
                && !string.IsNullOrWhiteSpace(AmeisenBot.Config.Path)
                && Directory.Exists(AmeisenBot.DataFolder))
            {
                File.WriteAllText(AmeisenBot.Config.Path, JsonConvert.SerializeObject(AmeisenBot.Config, Formatting.Indented));
            }
        }

        private void UpdateBotInfo(int maxSecondary, int secondary, Brush primaryBrush, Brush secondaryBrush)
        {
            // Generic labels
            labelPlayerName.Content = AmeisenBot.Bot.Db.GetUnitName(AmeisenBot.Bot.Player, out string name) ? name : "unknown";

            labelMapName.Content = AmeisenBot.Bot.Objects.MapId.ToString();
            labelZoneName.Content = AmeisenBot.Bot.Objects.ZoneName;
            labelZoneSubName.Content = AmeisenBot.Bot.Objects.ZoneSubName;

            labelCurrentLevel.Content = $"{AmeisenBot.Bot.Player.Level} (iLvl. {Math.Round(AmeisenBot.Bot.Character.Equipment.AverageItemLevel)})";
            labelCurrentRace.Content = $"{AmeisenBot.Bot.Player.Race} {AmeisenBot.Bot.Player.Gender}";
            labelCurrentClass.Content = AmeisenBot.Bot.Player.Class;

            progressbarExp.Maximum = AmeisenBot.Bot.Player.NextLevelXp;
            progressbarExp.Value = AmeisenBot.Bot.Player.Xp;
            labelCurrentExp.Content = $"{Math.Round(AmeisenBot.Bot.Player.XpPercentage)}%";

            progressbarHealth.Maximum = AmeisenBot.Bot.Player.MaxHealth;
            progressbarHealth.Value = AmeisenBot.Bot.Player.Health;
            labelCurrentHealth.Content = BotUtils.BigValueToString(AmeisenBot.Bot.Player.Health);

            labelCurrentCombatclass.Content = AmeisenBot.Bot.CombatClass == null ? $"No CombatClass" : AmeisenBot.Bot.CombatClass.ToString();

            // Class specific labels
            progressbarSecondary.Maximum = maxSecondary;
            progressbarSecondary.Value = secondary;
            labelCurrentSecondary.Content = BotUtils.BigValueToString(secondary);

            // Colors
            progressbarHealth.Foreground = primaryBrush;
            progressbarSecondary.Foreground = secondaryBrush;
            labelCurrentClass.Foreground = primaryBrush;
        }

        private void UpdateBottomLabels()
        {
            // Object count label
            labelCurrentObjectCount.Content = AmeisenBot.Bot.Objects.ObjectCount.ToString(CultureInfo.InvariantCulture).PadLeft(4);

            // Tick time label
            float executionMs = AmeisenBot.CurrentExecutionMs;

            if (float.IsNaN(executionMs) || float.IsInfinity(executionMs))
            {
                executionMs = 0;
            }

            labelCurrentTickTime.Content = executionMs.ToString(CultureInfo.InvariantCulture).PadLeft(4);

            if (executionMs <= AmeisenBot.Config.StateMachineTickMs)
            {
                labelCurrentTickTime.Foreground = CurrentTickTimeGoodBrush;
            }
            else
            {
                labelCurrentTickTime.Foreground = CurrentTickTimeBadBrush;
                AmeisenLogger.I.Log("MainWindow", $"High executionMs ({executionMs}), something blocks our thread or CPU is to slow", LogLevel.Warning);
            }

            // HookCall label
            labelHookCallCount.Content = AmeisenBot.Bot.Wow.HookCallCount.ToString(CultureInfo.InvariantCulture).PadLeft(2);

            if (AmeisenBot.Bot.Wow.HookCallCount <= (AmeisenBot.Bot.Player.IsInCombat ? (ulong)AmeisenBot.Config.MaxFpsCombat : (ulong)AmeisenBot.Config.MaxFps))
            {
                labelHookCallCount.Foreground = CurrentTickTimeGoodBrush;
            }
            else
            {
                labelHookCallCount.Foreground = CurrentTickTimeBadBrush;
                AmeisenLogger.I.Log("MainWindow", "High HookCall count, maybe increase your FPS", LogLevel.Warning);
            }

            // RPM/WPM labels
            labelRpmCallCount.Content = AmeisenBot.Bot.Memory.RpmCallCount.ToString(CultureInfo.InvariantCulture).PadLeft(5);
            labelWpmCallCount.Content = AmeisenBot.Bot.Memory.WpmCallCount.ToString(CultureInfo.InvariantCulture).PadLeft(3);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveBotWindowPosition();

            Overlay?.Exit();
            AmeisenBot?.Dispose();

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
            // check for older data folder that users need to migrate to the new location
            string oldDataPath = $"{Directory.GetCurrentDirectory()}\\data\\";

            if (Directory.Exists(oldDataPath))
            {
                MessageBox.Show("You need to move the content of your \"\\\\data\\\\\" folder to \"%AppData%\\\\Roaming\\\\AmeisenbotX\\\\profiles\\\\\". Otherwise your profiles may not be displayed.", "New Data Location", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // create our data folder, default will be placed at "%APPDATA%/AmeisenBotX/..."
            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }

            // show the load config window first
            LoadConfigWindow loadConfigWindow = new(DataPath);
            loadConfigWindow.ShowDialog();

            // exit if the user selected no config
            if (string.IsNullOrWhiteSpace(loadConfigWindow.ConfigToLoad))
            {
                // TODO: close the window without crashing here
                // Close();
            }
            else
            {
                // init GUI related stuff
                MainWindowHandle = new WindowInteropHelper(this).EnsureHandle(); // obtain a window handle (HWND) to out current WPF window

                // load the state overrides
                comboboxStateOverride.Items.Add(BotState.Idle);
                comboboxStateOverride.Items.Add(BotState.Attacking);
                comboboxStateOverride.Items.Add(BotState.Grinding);
                comboboxStateOverride.Items.Add(BotState.Job);
                comboboxStateOverride.Items.Add(BotState.Questing);

                comboboxStateOverride.SelectedIndex = 0;

                // display the PID, maybe remove this when not debugging
                labelPID.Content = $"PID: {Environment.ProcessId}";

                // init the bots engine
                if (File.Exists(loadConfigWindow.ConfigToLoad) && TryLoadConfig(loadConfigWindow.ConfigToLoad, out AmeisenBotConfig config))
                {
                    AmeisenBot = new(config);

                    // events used to update our GUI
                    AmeisenBot.Bot.Objects.OnObjectUpdateComplete += OnObjectUpdateComplete;
                    AmeisenBot.StateMachine.OnStateMachineStateChanged += () =>
                    {
                        Dispatcher.InvokeAsync(() => { labelCurrentState.Content = $"{AmeisenBot.StateMachine.CurrentState.Key}"; });
                    };

                    // handle the autoposition function where the wow window gets "absorbed" by the bots window
                    if (AmeisenBot.Config.AutoPositionWow)
                    {
                        // this is used to measure the size of wow's window
                        PresentationSource presentationSource = PresentationSource.FromVisual(this);
                        M11 = presentationSource.CompositionTarget.TransformToDevice.M11;
                        M22 = presentationSource.CompositionTarget.TransformToDevice.M22;

                        AmeisenBot.StateMachine.GetState<StateStartWow>().OnWoWStarted += () =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                AmeisenBot.Bot.Memory.SetupAutoPosition
                                (
                                    MainWindowHandle,
                                    (int)(wowRect.Margin.Left * M11 + 1),
                                    (int)(wowRect.Margin.Top * M22 + 1),
                                    (int)(wowRect.ActualWidth * M11),
                                    (int)(wowRect.ActualHeight * M22)
                                );
                            });

                            IsAutoPositionSetup = true;
                        };
                    }

                    AmeisenBot.Start();

                    // init misc GUI related stuff
                    StateConfigWindows = new()
                    {
                        { BotState.Grinding, new StateGrindingConfigWindow(AmeisenBot, AmeisenBot.Config) },
                        { BotState.Job, new StateJobConfigWindow(AmeisenBot, AmeisenBot.Config) },
                        { BotState.Questing, new StateQuestingConfigWindow(AmeisenBot, AmeisenBot.Config) },
                    };

                    if (AmeisenBot.Config.Autopilot)
                    {
                        buttonToggleAutopilot.Foreground = CurrentTickTimeGoodBrush;
                    }

                    // load our old window position
                    if (AmeisenBot.Config.SaveBotWindowPosition)
                    {
                        if (MainWindowHandle != IntPtr.Zero && AmeisenBot.Config.BotWindowRect != new Memory.Win32.Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 })
                        {
                            AmeisenBot.Bot.Memory.SetWindowPosition(MainWindowHandle, AmeisenBot.Config.BotWindowRect);
                            AmeisenLogger.I.Log("AmeisenBot", $"Loaded window position: {AmeisenBot.Config.BotWindowRect}", LogLevel.Verbose);
                        }
                        else
                        {
                            AmeisenLogger.I.Log("AmeisenBot", $"Unable to load window position of {MainWindowHandle} to {AmeisenBot.Config.BotWindowRect}", LogLevel.Warning);
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Check your config, maybe it contains some invalid stuff.\n\n{loadConfigWindow.ConfigToLoad}", "Failed to load Config", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}