using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Path = System.IO.Path;

namespace AmeisenBotX
{
    public partial class LoadConfigWindow : Window
    {
        public LoadConfigWindow(string botDataPath)
        {
            BotDataPath = botDataPath;
            ConfigToLoad = string.Empty;
            InitializeComponent();
        }

        public string ConfigToLoad { get; set; }

        private string BotDataPath { get; set; }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ComboboxSelectedConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboboxSelectedConfig.SelectedItem == null)
            {
                return;
            }

            if ((string)comboboxSelectedConfig.SelectedItem == "New Config")
            {
                ConfigEditorWindow configEditor = new(BotDataPath, null);
                configEditor.ShowDialog();

                if (configEditor.Cancel)
                {
                    comboboxSelectedConfig.SelectedItem = null;
                    return;
                }

                if (configEditor.ConfigName != null && configEditor.Config != null)
                {
                    ConfigToLoad = Path.Combine(BotDataPath, configEditor.ConfigName, "config.json");

                    if (!Directory.Exists(Path.GetDirectoryName(ConfigToLoad)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(ConfigToLoad));
                    }

                    File.WriteAllText(ConfigToLoad, JsonConvert.SerializeObject(configEditor.Config, Formatting.Indented));
                }
            }
            else
            {
                ConfigToLoad = Path.Combine(BotDataPath, (string)comboboxSelectedConfig.SelectedItem, "config.json");
            }

            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comboboxSelectedConfig.Items.Add("New Config");

            if (Directory.Exists(BotDataPath))
            {
                string[] directories = Directory.GetDirectories(BotDataPath);

                for (int i = 0; i < directories.Length; ++i)
                {
                    comboboxSelectedConfig.Items.Add(Path.GetFileName(directories[i]));
                }
            }

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                string botnameParam = args[1];

                if (comboboxSelectedConfig.Items.Contains(botnameParam))
                {
                    comboboxSelectedConfig.SelectedItem = botnameParam;
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}