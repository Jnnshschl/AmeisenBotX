using AmeisenBotX.Core;
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
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace AmeisenBotX
{
    /// <summary>
    /// Interaktionslogik für LoadConfigWindow.xaml
    /// </summary>
    public partial class LoadConfigWindow : Window
    {
        public string ConfigToLoad { get; set; }
        private string BotDataPath { get; set; }

        public LoadConfigWindow(string botDataPath)
        {
            BotDataPath = botDataPath;
            ConfigToLoad = "";
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(BotDataPath))
            {
                foreach (string directory in Directory.GetDirectories(BotDataPath))
                    comboboxSelectedConfig.Items.Add(Path.GetFileName(directory));
            }
            comboboxSelectedConfig.Items.Add("New Config");
        }

        private void ComboboxSelectedConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((string)comboboxSelectedConfig.SelectedItem == "New Config")
            {
                ConfigEditorWindow configEditor = new ConfigEditorWindow();
                configEditor.ShowDialog();

                if (configEditor.ConfigName != null && configEditor.Config != null)
                {
                    ConfigToLoad = Path.Combine(BotDataPath, configEditor.ConfigName, "config.json");

                    if (!Directory.Exists(Path.GetDirectoryName(ConfigToLoad)))
                        Directory.CreateDirectory(Path.GetDirectoryName(ConfigToLoad));

                    File.WriteAllText(ConfigToLoad, JsonConvert.SerializeObject(configEditor.Config, Formatting.Indented));
                }
                else
                    Close();
            }
            else
            {
                ConfigToLoad = Path.Combine(BotDataPath, (string)comboboxSelectedConfig.SelectedItem, "config.json");
            }
            Close();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
