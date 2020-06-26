using AmeisenBotX.Core;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AmeisenBotX
{
    public partial class InfoWindow : Window
    {
        public InfoWindow(AmeisenBot ameisenBot)
        {
            AmeisenBot = ameisenBot;
            CurrentDisplayMode = DisplayMode.Equipment;
            InitializeComponent();
        }

        private enum DisplayMode
        {
            Equipment,
            Inventory,
            Spells
        }

        private AmeisenBot AmeisenBot { get; set; }

        private DisplayMode CurrentDisplayMode { get; set; }

        private void ButtonEquipment_Click(object sender, RoutedEventArgs e)
        {
            CurrentDisplayMode = DisplayMode.Equipment;
            DisplayStuff();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void ButtonInventory_Click(object sender, RoutedEventArgs e)
        {
            CurrentDisplayMode = DisplayMode.Inventory;
            DisplayStuff();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            DisplayStuff();
        }

        private void ButtonSpells_Click(object sender, RoutedEventArgs e)
        {
            CurrentDisplayMode = DisplayMode.Spells;
            DisplayStuff();
        }

        private void DisplayStuff()
        {
            equipmentWrapPanel.Children.Clear();
            labelAvgItemLvl.Content = Math.Ceiling(AmeisenBot.WowInterface.CharacterManager.Equipment.AverageItemLevel);

            switch (CurrentDisplayMode)
            {
                case DisplayMode.Equipment:
                    buttonEquipment.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["DarkAccent1"]);
                    buttonInventory.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["DarkBorder"]);
                    buttonSpells.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["DarkBorder"]);

                    IWowItem[] equipmentItems = AmeisenBot.WowInterface.CharacterManager.Equipment.Items.Values.ToArray();

                    for (int i = 0; i < equipmentItems.Length; ++i)
                    {
                        equipmentWrapPanel.Children.Add(new ItemDisplay(equipmentItems[i]));
                    }

                    break;

                case DisplayMode.Inventory:
                    buttonEquipment.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["DarkBorder"]);
                    buttonInventory.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["DarkAccent1"]);
                    buttonSpells.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["DarkBorder"]);

                    IWowItem[] inventoryItems = AmeisenBot.WowInterface.CharacterManager.Inventory.Items.ToArray();

                    for (int i = 0; i < inventoryItems.Length; ++i)
                    {
                        equipmentWrapPanel.Children.Add(new ItemDisplay(inventoryItems[i]));
                    }

                    break;

                case DisplayMode.Spells:
                    buttonEquipment.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["DarkBorder"]);
                    buttonInventory.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["DarkBorder"]);
                    buttonSpells.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["DarkAccent1"]);
                    
                    foreach (Spell spell in AmeisenBot.WowInterface.CharacterManager.SpellBook.Spells.GroupBy(e => e.Name).Select(e => e.First()))
                    {
                        equipmentWrapPanel.Children.Add(new SpellDisplay(spell));
                    }

                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayStuff();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}