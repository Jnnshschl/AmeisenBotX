using AmeisenBotX.Core;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private readonly Brush darkForegroundBrush;

        private readonly Brush textAccentBrush;

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

                AmeisenBot.ObjectManager.OnObjectUpdateComplete += OnObjectUpdateComplete;
                AmeisenBot.StateMachine.OnStateMachineStateChanged += OnStateMachineStateChange;

                LastStateMachineTick = DateTime.Now;
            }
        }

        public AmeisenBotConfig Config { get; private set; }

        public string ConfigPath { get; private set; }

        private AmeisenBot AmeisenBot { get; }

        private DateTime LastStateMachineTick { get; set; }

        private void ButtonClearCache_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBot.BotCache.Clear();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e) => Close();

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(Config);
            settingsWindow.ShowDialog();
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
                labelPlayerName.Content = AmeisenBot.ObjectManager.Player.Name;

                labelCurrentLevel.Content = $"lvl. {AmeisenBot.ObjectManager.Player.Level}";
                labelCurrentRace.Content = $"{AmeisenBot.ObjectManager.Player.Gender} {AmeisenBot.ObjectManager.Player.Race}";
                labelCurrentClass.Content = $"{AmeisenBot.ObjectManager.Player.Class} [{AmeisenBot.ObjectManager.Player.PowerType}]";

                if (AmeisenBot.CombatClass == null)
                {
                    labelCurrentCombatclass.Content = $"No CombatClass";
                }
                else
                {
                    labelCurrentCombatclass.Content = $"{AmeisenBot.CombatClass.GetType().Name}";
                }

                progressbarExp.Maximum = AmeisenBot.ObjectManager.Player.MaxExp;
                progressbarExp.Value = AmeisenBot.ObjectManager.Player.Exp;
                labelCurrentHealth.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Exp)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxExp)}";

                progressbarHealth.Maximum = AmeisenBot.ObjectManager.Player.MaxHealth;
                progressbarHealth.Value = AmeisenBot.ObjectManager.Player.Health;
                labelCurrentHealth.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Health)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxHealth)}";

                switch (AmeisenBot.ObjectManager.Player.Class)
                {
                    case WowClass.DeathKnight:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxRuneenergy;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Runeenergy;
                        labelCurrentSecondary.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Runeenergy)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxRuneenergy)}";

                        progressbarHealth.Foreground = dkPrimaryBrush;
                        progressbarSecondary.Foreground = dkSecondaryBrush;
                        break;

                    case WowClass.Druid:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;
                        labelCurrentSecondary.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Mana)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxMana)}";

                        progressbarHealth.Foreground = druidPrimaryBrush;
                        progressbarSecondary.Foreground = druidSecondaryBrush;
                        break;

                    case WowClass.Hunter:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;
                        labelCurrentSecondary.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Mana)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxMana)}";

                        progressbarHealth.Foreground = hunterPrimaryBrush;
                        progressbarSecondary.Foreground = hunterSecondaryBrush;
                        break;

                    case WowClass.Mage:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;
                        labelCurrentSecondary.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Mana)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxMana)}";

                        progressbarHealth.Foreground = magePrimaryBrush;
                        progressbarSecondary.Foreground = mageSecondaryBrush;
                        break;

                    case WowClass.Paladin:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;
                        labelCurrentSecondary.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Mana)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxMana)}";

                        progressbarHealth.Foreground = paladinPrimaryBrush;
                        progressbarSecondary.Foreground = paladinSecondaryBrush;
                        break;

                    case WowClass.Priest:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;
                        labelCurrentSecondary.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Mana)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxMana)}";

                        progressbarHealth.Foreground = priestPrimaryBrush;
                        progressbarSecondary.Foreground = priestSecondaryBrush;
                        break;

                    case WowClass.Rogue:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxEnergy;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Energy;
                        labelCurrentSecondary.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Energy)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxEnergy)}";

                        progressbarHealth.Foreground = roguePrimaryBrush;
                        progressbarSecondary.Foreground = rogueSecondaryBrush;
                        break;

                    case WowClass.Shaman:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;
                        labelCurrentSecondary.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Mana)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxMana)}";

                        progressbarHealth.Foreground = shamanPrimaryBrush;
                        progressbarSecondary.Foreground = shamanSecondaryBrush;
                        break;

                    case WowClass.Warlock:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;
                        labelCurrentSecondary.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Mana)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxMana)}";

                        progressbarHealth.Foreground = warlockPrimaryBrush;
                        progressbarSecondary.Foreground = warlockSecondaryBrush;
                        break;

                    case WowClass.Warrior:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxRage;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Rage;
                        labelCurrentSecondary.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Rage)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxRage)}";

                        progressbarHealth.Foreground = warriorPrimaryBrush;
                        progressbarSecondary.Foreground = warriorSecondaryBrush;
                        break;

                    default:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;
                        labelCurrentSecondary.Content = $"{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.Mana)}/{BotUtils.BigValueToString(AmeisenBot.ObjectManager.Player.MaxMana)}";
                        break;
                }

                if (LastStateMachineTick + TimeSpan.FromSeconds(1) < DateTime.Now)
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
                    }

                    LastStateMachineTick = DateTime.Now;
                }

                labelCurrentObjectCount.Content = AmeisenBot.ObjectManager.WowObjects.Count;
            });
        }

        private void OnStateMachineStateChange()
        {
            Dispatcher.InvokeAsync(() =>
            {
                labelCurrentState.Content = $"<{AmeisenBot.StateMachine.CurrentState.Key}>";
            });
        }

        private void SaveConfig()
        {
            if (!string.IsNullOrEmpty(ConfigPath) && Config != null)
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config, Formatting.Indented));
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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => DragMove();

        private void ButtonConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigEditorWindow configEditor = new ConfigEditorWindow(BotDataPath, Config);
            configEditor.ShowDialog();
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

        private void ButtonFaceTarget_Click(object sender, RoutedEventArgs e)
        {
            float angle = BotMath.GetFacingAngle(AmeisenBot.ObjectManager.Player.Position, AmeisenBot.ObjectManager.Target.Position);
            AmeisenBot.HookManager.SetFacing(AmeisenBot.ObjectManager.Player, angle);
        }
    }
}
