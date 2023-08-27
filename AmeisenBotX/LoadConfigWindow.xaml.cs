using AmeisenBotX.Common.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Path = System.IO.Path;

namespace AmeisenBotX
{
    public partial class LoadConfigWindow : Window
    {
        public LoadConfigWindow()
        {
            ConfigToLoad = string.Empty;
            InitializeComponent();
        }

        public string ConfigToLoad { get; set; }

        private string DataPath { get; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\AmeisenBotX\\profiles\\";

        public int ProcessToHook { get; set; } = 0;

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
                ConfigEditorWindow configEditor = new(DataPath, null);
                configEditor.ShowDialog();

                if (configEditor.Cancel)
                {
                    comboboxSelectedConfig.SelectedItem = null;
                    return;
                }

                if (configEditor.ConfigName != null && configEditor.Config != null)
                {
                    ConfigToLoad = Path.Combine(DataPath, configEditor.ConfigName, "config.json");
                    IOUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(ConfigToLoad));

                    File.WriteAllText(ConfigToLoad, JsonSerializer.Serialize(configEditor.Config, new JsonSerializerOptions() { AllowTrailingCommas = true, NumberHandling = JsonNumberHandling.AllowReadingFromString }));
                }
            }
            else
            {
                ConfigToLoad = Path.Combine(DataPath, (string)comboboxSelectedConfig.SelectedItem, "config.json");
            }
            Complete();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // check for older data folder that users need to migrate to the new location
            string oldDataPath = $"{Directory.GetCurrentDirectory()}\\data\\";

            if (Directory.Exists(oldDataPath))
            {
                MessageBox.Show($"You need to move the content of your \"\\\\data\\\\\" folder to \"{DataPath}\". Otherwise your profiles may not be displayed.", "New Data Location", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // create our data folder, default will be placed at "%APPDATA%/AmeisenBotX/..."
            IOUtils.CreateDirectoryIfNotExists(DataPath);

            comboboxSelectedConfig.Items.Add("New Config");

            string[] directories = Directory.GetDirectories(DataPath);

            foreach (string str in directories)
            {
                comboboxSelectedConfig.Items.Add(Path.GetFileName(str));
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

        private void comboboxSelectedProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var processes = Process.GetProcesses().Where(t => t.ProcessName.Contains("Wow"));
            var selectedName = comboboxSelectedProcess.SelectedItem as string;
            
            var selected = processes.SingleOrDefault(t=> ProcessToName(t) == selectedName);

            ProcessToHook = default;

            if (selected != default)
            {
                ProcessToHook = selected.Id;
            }

            Complete();
        }

        private void Complete()
        {

            if (comboboxSelectedConfig.SelectedItem != null)
            {
                Hide();

                MainWindow mainWindow = new(DataPath, ConfigToLoad, ProcessToHook);
                mainWindow.Show();

                Close();
            }
        }

        private string ProcessToName(Process process)
        {
            var name = $"[{process.Id}] {process.MainWindowTitle}";
            return name;
        }

        private void comboboxSelectedProcess_Loaded(object sender, RoutedEventArgs e)
        {
            var processes = Process.GetProcesses().Where(t => t.ProcessName.Contains("Wow"));
            
            comboboxSelectedProcess.Items.Clear();

            foreach (var process in processes)
            {
                comboboxSelectedProcess.Items.Add(ProcessToName(process));
            }
        }
    }
}