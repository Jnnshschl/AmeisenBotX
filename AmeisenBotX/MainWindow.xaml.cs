using AmeisenBotX.Core;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.WowObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace AmeisenBotX
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AmeisenBotConfig Config { get; private set; }

        private readonly string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        private AmeisenBot AmeisenBot { get; }

        private DateTime LastStateMachineTick { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(configPath)) Config = LoadConfig();
            else Config = SaveConfig(); // create a default config

            AmeisenBot = new AmeisenBot(Config);

            AmeisenBot.ObjectManager.OnObjectUpdateComplete += OnObjectUpdateComplete;
            AmeisenBot.StateMachine.OnStateMachineStateChange += OnStateMachineStateChange;

            LastStateMachineTick = DateTime.Now;
        }

        private AmeisenBotConfig LoadConfig()
            => JsonConvert.DeserializeObject<AmeisenBotConfig>(File.ReadAllText(configPath));

        private AmeisenBotConfig SaveConfig()
        {
            if (Config == null) Config = new AmeisenBotConfig();

            File.WriteAllText(configPath, JsonConvert.SerializeObject(Config, Formatting.Indented));
            return Config;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AmeisenBot.Stop();
            SaveConfig();
        }

        private void OnStateMachineStateChange()
        {
            Dispatcher.InvokeAsync(() =>
            {
                labelCurrentState.Content = $"<{AmeisenBot.StateMachine.CurrentState.Key}>";
            });
        }

        private void OnObjectUpdateComplete(List<WowObject> wowObjects)
        {
            Dispatcher.InvokeAsync(() =>
            {
                labelPlayerName.Content = AmeisenBot.ObjectManager.Player.Name;

                labelCurrentLevel.Content = $"lvl. {AmeisenBot.ObjectManager.Player.Level}";
                labelCurrentRace.Content = AmeisenBot.ObjectManager.Player.Race;
                labelCurrentClass.Content = AmeisenBot.ObjectManager.Player.Class;

                progressbarHealth.Maximum = AmeisenBot.ObjectManager.Player.MaxHealth;
                progressbarHealth.Value = AmeisenBot.ObjectManager.Player.Health;

                switch (AmeisenBot.ObjectManager.Player.Class)
                {
                    case WowClass.DeathKnight:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxRuneenergy;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Runeenergy;
                        break;

                    case WowClass.Warrior:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxRage;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Rage;
                        break;

                    case WowClass.Rogue:
                        progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxEnergy;
                        progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Energy;
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
                    if (double.IsNaN(executionMs)) executionMs = 0;
                    labelCurrentTickTime.Content = executionMs;

                    LastStateMachineTick = DateTime.Now;
                }

                labelCurrentObjectCount.Content = AmeisenBot.ObjectManager.WowObjects.Count;

                //labelDebug.Content = JsonConvert.SerializeObject(AmeisenBot.ObjectManager.Player, Formatting.Indented);
            });
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Start();
        }
    }
}