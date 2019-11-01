using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AmeisenBotX.Core;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.WowObject;
using Newtonsoft.Json;

namespace AmeisenBotX
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly string BotDataPath = $"{AppDomain.CurrentDomain.BaseDirectory}data\\";

        public MainWindow()
        {
            InitializeComponent();

            Config = LoadConfig();

            if (Config != null)
            {
                string playername = Path.GetFileName(Path.GetDirectoryName(ConfigPath));
                AmeisenBot = new AmeisenBot(BotDataPath, playername, Config);

                AmeisenBot.ObjectManager.OnObjectUpdateComplete += OnObjectUpdateComplete;
                AmeisenBot.StateMachine.OnStateMachineStateChange += OnStateMachineStateChange;

                LastStateMachineTick = DateTime.Now;
            }
        }

        public string ConfigPath { get; private set; }

        public AmeisenBotConfig Config { get; private set; }

        private AmeisenBot AmeisenBot { get; }

        private DateTime LastStateMachineTick { get; set; }

        private void ButtonExit_Click(object sender, RoutedEventArgs e) => Close();

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
                labelCurrentRace.Content = $"{AmeisenBot.ObjectManager.Player.Race} {AmeisenBot.ObjectManager.Player.Gender}";
                labelCurrentClass.Content = AmeisenBot.ObjectManager.Player.Class;
                labelCurrentPowerType.Content = AmeisenBot.ObjectManager.Player.PowerType;

                progressbarHealth.Maximum = AmeisenBot.ObjectManager.Player.MaxHealth;
                progressbarHealth.Value = AmeisenBot.ObjectManager.Player.Health;

                switch (AmeisenBot.ObjectManager.Player.Class)
                {
                    case WowClass.DeathKnight:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxRuneenergy;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Runeenergy;

                        progressbarSecondary.Foreground = new SolidColorBrush(Color.FromRgb(0,0,255));
                        progressbarHealth.Foreground = new SolidColorBrush(Color.FromRgb(196,30,59));
                        break;

                    case WowClass.Druid:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;

                        progressbarSecondary.Foreground = new SolidColorBrush(Color.FromRgb(0,0,255));
                        progressbarHealth.Foreground = new SolidColorBrush(Color.FromRgb(255,125,10));
                        break;

                    case WowClass.Hunter:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;

                        progressbarSecondary.Foreground = new SolidColorBrush(Color.FromRgb(0,0,255));
                        progressbarHealth.Foreground = new SolidColorBrush(Color.FromRgb(171,212,115));
                        break;

                    case WowClass.Mage:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;

                        progressbarSecondary.Foreground = new SolidColorBrush(Color.FromRgb(0,0,255));
                        progressbarHealth.Foreground = new SolidColorBrush(Color.FromRgb(105,204,240));
                        break;

                    case WowClass.Paladin:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;

                        progressbarSecondary.Foreground = new SolidColorBrush(Color.FromRgb(0,0,255));
                        progressbarHealth.Foreground = new SolidColorBrush(Color.FromRgb(245,140,186));
                        break;

                    case WowClass.Priest:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;

                        progressbarSecondary.Foreground = new SolidColorBrush(Color.FromRgb(0,0,255));
                        progressbarHealth.Foreground = new SolidColorBrush(Color.FromRgb(255,255,255));
                        break;

                    case WowClass.Rogue:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxEnergy;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Energy;

                        progressbarSecondary.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 0));
                        progressbarHealth.Foreground = new SolidColorBrush(Color.FromRgb(255,245,105));
                        break;

                    case WowClass.Shaman:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;

                        progressbarSecondary.Foreground = new SolidColorBrush(Color.FromRgb(0,0,255));
                        progressbarHealth.Foreground = new SolidColorBrush(Color.FromRgb(0,112,222));
                        break;

                    case WowClass.Warlock:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxMana;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Mana;

                        progressbarSecondary.Foreground = new SolidColorBrush(Color.FromRgb(0,0,255));
                        progressbarHealth.Foreground = new SolidColorBrush(Color.FromRgb(148,130,201));
                        break;

                    case WowClass.Warrior:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxRage;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Rage;

                        progressbarSecondary.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        progressbarHealth.Foreground = new SolidColorBrush(Color.FromRgb(199,156,110));
                        break;

                    default:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxEnergy;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Energy;
                        break;
                }

                progressbarExp.Maximum = AmeisenBot.ObjectManager.Player.MaxExp;
                progressbarExp.Value = AmeisenBot.ObjectManager.Player.Exp;

                if (LastStateMachineTick + TimeSpan.FromSeconds(1) < DateTime.Now)
                {
                    double executionMs = AmeisenBot.CurrentExecutionMs;
                    if (double.IsNaN(executionMs))
                    {
                        executionMs = 0;
                    }

                    labelCurrentTickTime.Content = executionMs;

                    LastStateMachineTick = DateTime.Now;
                }

                labelCurrentObjectCount.Content = AmeisenBot.ObjectManager.WowObjects.Count;

                //// labelDebug.Content = $"{JsonConvert.SerializeObject(AmeisenBot.ObjectManager.WowObjects.OfType<WowDynobject>(), Formatting.Indented)}\n{JsonConvert.SerializeObject(AmeisenBot.ObjectManager.Player.Position,Formatting.Indented)}";
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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(Config);
            settingsWindow.ShowDialog();
        }
    }
}