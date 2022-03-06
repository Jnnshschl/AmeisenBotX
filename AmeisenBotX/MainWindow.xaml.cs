using AmeisenBotX.Common.Keyboard;
using AmeisenBotX.Common.Keyboard.Enums;
using AmeisenBotX.Common.Keyboard.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core;
using AmeisenBotX.Core.Logic;
using AmeisenBotX.Core.Logic.Enums;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Overlay;
using AmeisenBotX.Overlay.Utils;
using AmeisenBotX.StateConfig;
using AmeisenBotX.Utils;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace AmeisenBotX
{
    public partial class MainWindow : Window
    {
        public MainWindow(string dataPath, string configPath)
        {
            if (!Directory.Exists(dataPath)) { throw new FileNotFoundException(dataPath); }
            if (!File.Exists(configPath)) { throw new FileNotFoundException(configPath); }

            DataPath = dataPath;
            ConfigPath = configPath;

            InitializeComponent();

            CurrentTickTimeBadBrush = new SolidColorBrush(Color.FromRgb(255, 0, 80));
            CurrentTickTimeGoodBrush = new SolidColorBrush(Color.FromRgb(160, 255, 0));
            DarkForegroundBrush = new SolidColorBrush((Color)FindResource("DarkForeground"));
            DarkBackgroundBrush = new SolidColorBrush((Color)FindResource("DarkBackground"));
            TextAccentBrush = new SolidColorBrush((Color)FindResource("TextAccent"));

            NotificationGmBrush = new(Colors.Cyan);
            NotificationBrush = new(Colors.Pink);
            NotificationWhiteBrush = new(Colors.White);
            NotificationTransparentBrush = new(Colors.Transparent);

            CurrentTickTimeBadBrush.Freeze();
            CurrentTickTimeGoodBrush.Freeze();
            DarkForegroundBrush.Freeze();
            DarkBackgroundBrush.Freeze();
            TextAccentBrush.Freeze();

            NotificationGmBrush.Freeze();
            NotificationBrush.Freeze();
            NotificationWhiteBrush.Freeze();
            NotificationTransparentBrush.Freeze();

            LabelUpdateEvent = new(TimeSpan.FromSeconds(1));

            RenderState = true;

            KeyboardHook = new KeyboardHook();
            KeyboardHook.Enable();
        }

        public bool IsAutoPositionSetup { get; private set; }

        public KeyboardHook KeyboardHook { get; }

        public double M11 { get; private set; }

        public double M22 { get; private set; }

        public AmeisenBotOverlay Overlay { get; private set; }

        public bool RenderState { get; set; }

        private AmeisenBot AmeisenBot { get; set; }

        private string ConfigPath { get; }

        private Brush CurrentTickTimeBadBrush { get; }

        private Brush CurrentTickTimeGoodBrush { get; }

        private Brush DarkBackgroundBrush { get; }

        private Brush DarkForegroundBrush { get; }

        private string DataPath { get; }

        private DevToolsWindow DevToolsWindow { get; set; }

        private bool DrawOverlay { get; set; }

        private InfoWindow InfoWindow { get; set; }

        private TimegatedEvent LabelUpdateEvent { get; }

        private IntPtr MainWindowHandle { get; set; }

        private MapWindow MapWindow { get; set; }

        private bool NeedToClearOverlay { get; set; }

        private SolidColorBrush NoticifactionColor { get; set; }

        private bool NotificationBlinkState { get; set; }

        private SolidColorBrush NotificationBrush { get; }

        private SolidColorBrush NotificationGmBrush { get; }

        private long NotificationLastTimestamp { get; set; }

        private SolidColorBrush NotificationTransparentBrush { get; }

        private SolidColorBrush NotificationWhiteBrush { get; }

        private bool PendingNotification { get; set; }

        private RelationshipWindow RelationshipWindow { get; set; }

        private Dictionary<BotMode, Window> StateConfigWindows { get; set; }

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
                Dispatcher.Invoke(() =>
                {
                    AmeisenBot.Bot.Memory.ResizeParentWindow
                    (
                        (int)((wowRect.Margin.Left + 1) * M11),
                        (int)((wowRect.Margin.Top + 1) * M22),
                        (int)((wowRect.ActualWidth - 1) * M11),
                        (int)((wowRect.ActualHeight - 1) * M22)
                    );
                });
            }

            return size;
        }

        private static bool TryLoadConfig(string configPath, out AmeisenBotConfig config)
        {
            if (!string.IsNullOrWhiteSpace(configPath))
            {
                config = File.Exists(configPath)
                    ? JsonSerializer.Deserialize<AmeisenBotConfig>(File.ReadAllText(configPath), new JsonSerializerOptions() { AllowTrailingCommas = true, NumberHandling = JsonNumberHandling.AllowReadingFromString })
                    : new();

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
                File.WriteAllText(AmeisenBot.Config.Path, JsonSerializer.Serialize(configWindow.Config, new JsonSerializerOptions() { WriteIndented = true }));

                KeyboardHook.Clear();
                LoadHotkeys();
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

            buttonNotification.Foreground = NotificationWhiteBrush;
            buttonNotification.Background = NotificationTransparentBrush;
        }

        private void ButtonStartPause_Click(object sender, RoutedEventArgs e)
        {
            StartPause();
        }

        private void ButtonStateConfig_Click(object sender, RoutedEventArgs e)
        {
            if (StateConfigWindows.ContainsKey((BotMode)comboboxStateOverride.SelectedItem))
            {
                Window selectedWindow = StateConfigWindows[(BotMode)comboboxStateOverride.SelectedItem];
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
            AmeisenBot.Bot.Wow.SetWorldLoadedCheck(true);
            AmeisenBot.Bot.Wow.LuaDoString("CharacterSelect_EnterWorld()");
            AmeisenBot.Bot.Wow.SetWorldLoadedCheck(false);
        }

        private void ComboboxStateOverride_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AmeisenBot != null)
            {
                ((AmeisenBotLogic)AmeisenBot.Logic).ChangeMode((BotMode)comboboxStateOverride.SelectedItem);
                buttonStateConfig.IsEnabled = StateConfigWindows.ContainsKey((BotMode)comboboxStateOverride.SelectedItem);
            }
        }

        private void LoadHotkeys()
        {
            if (AmeisenBot.Config.Hotkeys.TryGetValue("StartStop", out Keybind kv))
            {
                KeyboardHook.AddHotkey((KeyCodes)kv.Key, (KeyCodes)kv.Mod, StartPause);
            }
        }

        private void OnObjectUpdateComplete(IEnumerable<IWowObject> wowObjects)
        {
            Dispatcher.Invoke(() =>
            {
                IWowPlayer player = AmeisenBot.Bot.Player;

                if (player != null)
                {
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

                    if (LabelUpdateEvent.Run())
                    {
                        UpdateBottomLabels();
                    }

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
                }
            });
        }

        private void OnWhisper(long timestamp, List<string> args)
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

                    NoticifactionColor = message.Flags.Contains("GM", StringComparison.OrdinalIgnoreCase) ? NotificationGmBrush : NotificationBrush;
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
                    buttonNotification.Foreground = NotificationWhiteBrush;
                    buttonNotification.Background = NotificationTransparentBrush;
                }

                NotificationBlinkState = !NotificationBlinkState;
            }
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
                && Directory.Exists(Path.GetDirectoryName(AmeisenBot.Config.Path)))
            {
                File.WriteAllText(AmeisenBot.Config.Path, JsonSerializer.Serialize(AmeisenBot.Config, new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true }));
            }
        }

        private void StartPause()
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

        private void UpdateBotInfo(int maxSecondary, int secondary, Brush primaryBrush, Brush secondaryBrush)
        {
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

            progressbarSecondary.Maximum = maxSecondary;
            progressbarSecondary.Value = secondary;
            labelCurrentSecondary.Content = BotUtils.BigValueToString(secondary);

            progressbarHealth.Foreground = primaryBrush;
            progressbarSecondary.Foreground = secondaryBrush;
            labelCurrentClass.Foreground = primaryBrush;
        }

        private void UpdateBottomLabels()
        {
            labelCurrentObjectCount.Content = AmeisenBot.Bot.Objects.ObjectCount.ToString(CultureInfo.InvariantCulture).PadLeft(4);

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

            labelHookCallCount.Content = AmeisenBot.Bot.Wow.HookCallCount.ToString(CultureInfo.InvariantCulture).PadLeft(2);

            if (AmeisenBot.Bot.Wow.HookCallCount <= (AmeisenBot.Bot.Player.IsInCombat ? AmeisenBot.Config.MaxFpsCombat : AmeisenBot.Config.MaxFps))
            {
                labelHookCallCount.Foreground = CurrentTickTimeGoodBrush;
            }
            else
            {
                labelHookCallCount.Foreground = CurrentTickTimeBadBrush;
                AmeisenLogger.I.Log("MainWindow", "High HookCall count, maybe increase your FPS", LogLevel.Warning);
            }

            labelRpmCallCount.Content = AmeisenBot.Bot.Memory.RpmCallCount.ToString(CultureInfo.InvariantCulture).PadLeft(5);
            labelWpmCallCount.Content = AmeisenBot.Bot.Memory.WpmCallCount.ToString(CultureInfo.InvariantCulture).PadLeft(3);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveBotWindowPosition();

            KeyboardHook.Disable();

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

            AmeisenLogger.I.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // obtain a window handle (HWND) to out current WPF window
            MainWindowHandle = new WindowInteropHelper(this).EnsureHandle();

            comboboxStateOverride.Items.Add(BotMode.None);
            comboboxStateOverride.Items.Add(BotMode.Grinding);
            comboboxStateOverride.Items.Add(BotMode.Jobs);
            comboboxStateOverride.Items.Add(BotMode.PvP);
            comboboxStateOverride.Items.Add(BotMode.Questing);
            comboboxStateOverride.Items.Add(BotMode.Testing);

            comboboxStateOverride.SelectedIndex = 0;

            // display the PID, maybe remove this when not debugging
            labelPID.Content = $"PID: {Environment.ProcessId}";

            if (TryLoadConfig(ConfigPath, out AmeisenBotConfig config))
            {
                AmeisenBot = new(config);

                // capture whisper messages and display them in the bots ui as a flashing button
                AmeisenBot.Bot.Wow.Events?.Subscribe("CHAT_MSG_WHISPER", OnWhisper);

                // events used to update our GUI
                AmeisenBot.Bot.Objects.OnObjectUpdateComplete += OnObjectUpdateComplete;
                // AmeisenBot.StateMachine.OnStateMachineStateChanged += () =>
                // {
                //     Dispatcher.InvokeAsync(() => { labelCurrentState.Content = $"{AmeisenBot.StateMachine.CurrentState.Key}"; });
                // };

                // handle the autoposition function where the wow window gets "absorbed" by the bots window
                if (AmeisenBot.Config.AutoPositionWow)
                {
                    // this is used to measure the size of wow's window
                    PresentationSource presentationSource = PresentationSource.FromVisual(this);
                    M11 = presentationSource.CompositionTarget.TransformToDevice.M11;
                    M22 = presentationSource.CompositionTarget.TransformToDevice.M22;

                    AmeisenBot.Logic.OnWoWStarted += () =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            AmeisenBot.Bot.Memory.SetupAutoPosition
                            (
                                MainWindowHandle,
                                (int)((wowRect.Margin.Left + 1) * M11),
                                (int)((wowRect.Margin.Top + 1) * M22),
                                (int)((wowRect.ActualWidth - 1) * M11),
                                (int)((wowRect.ActualHeight - 1) * M22)
                            );
                        });

                        IsAutoPositionSetup = true;
                    };
                }

                AmeisenLogger.I.Log("AmeisenBot", "Loading Hotkeys", LogLevel.Verbose);
                LoadHotkeys();

                AmeisenBot.Start();

                StateConfigWindows = new()
                {
                    { BotMode.Jobs, new StateJobConfigWindow(AmeisenBot, AmeisenBot.Config) },
                    { BotMode.Grinding, new StateGrindingConfigWindow(AmeisenBot, AmeisenBot.Config) },
                    { BotMode.Questing, new StateQuestingConfigWindow(AmeisenBot, AmeisenBot.Config) },
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
                MessageBox.Show($"Check your config, maybe it contains some invalid stuff.\n\n{ConfigPath}", "Failed to load Config", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}