using AmeisenBotX.Core;
using AmeisenBotX.Core.Data.Objects.WowObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(configPath)) Config = LoadConfig();
            else Config = SaveConfig(); // create a default config

            AmeisenBot = new AmeisenBot(Config);

            AmeisenBot.ObjectManager.OnObjectUpdateComplete += OnObjectUpdateComplete;
            AmeisenBot.StateMachine.OnStateMachineStateChange += OnStateMachineStateChange;
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
            SaveConfig();
        }

        private void OnStateMachineStateChange()
        {
            Dispatcher.InvokeAsync(() =>
            {
                labelCurrentState.Content = AmeisenBot.StateMachine.CurrentState.Key;
                labelCurrentTickTime.Content = AmeisenBot.CurrentExecutionMs;
            });
        }

        private void OnObjectUpdateComplete(List<WowObject> wowObjects)
        {
            Dispatcher.InvokeAsync(() =>
            {
                labelPlayerName.Content = AmeisenBot.ObjectManager.Player.Name;

                progressbarHealth.Maximum = AmeisenBot.ObjectManager.Player.MaxHealth;
                progressbarHealth.Value = AmeisenBot.ObjectManager.Player.Health;

                progressbarSecondary.Maximum = AmeisenBot.ObjectManager.Player.MaxEnergy;
                progressbarSecondary.Value = AmeisenBot.ObjectManager.Player.Energy;

                progressbarExp.Maximum = AmeisenBot.ObjectManager.Player.MaxExp;
                progressbarExp.Value = AmeisenBot.ObjectManager.Player.Exp;

                labelCurrentTickTime.Content = AmeisenBot.CurrentExecutionMs;
                labelCurrentObjectCount.Content = AmeisenBot.ObjectManager.WowObjects.Count;

                labelDebug.Content = JsonConvert.SerializeObject(AmeisenBot.ObjectManager.Player.Position, Formatting.Indented);
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
