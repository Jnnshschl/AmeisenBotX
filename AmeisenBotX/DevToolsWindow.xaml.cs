using AmeisenBotX.Core;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Cache.Enums;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AmeisenBotX
{
    public partial class DevToolsWindow : Window
    {
        public DevToolsWindow(AmeisenBot ameisenBot)
        {
            AmeisenBot = ameisenBot;

            InitializeComponent();
        }

        public AmeisenBot AmeisenBot { get; private set; }

        private void ButtonEventClear_Click(object sender, RoutedEventArgs e)
        {
            textboxEventResult.Text = string.Empty;
        }

        private void ButtonEventSubscribe_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBot.WowInterface.EventHookManager.Subscribe(textboxEventName.Text, OnWowEventFired);
        }

        private void ButtonEventUnsubscribe_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBot.WowInterface.EventHookManager.Unsubscribe(textboxEventName.Text, OnWowEventFired);
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void ButtonLuaExecute_Click(object sender, RoutedEventArgs e)
        {
            if (AmeisenBot.WowInterface.HookManager.ExecuteLuaAndRead(BotUtils.ObfuscateLua(textboxLuaCode.Text), out string result))
            {
                textboxLuaResult.Text = result;
            }
            else
            {
                textboxLuaResult.Text = "Failed to execute LUA...";
            }
        }

        private void ButtonLuaExecute_Copy_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBot.WowInterface.HookManager.LuaDoString(textboxLuaCode.Text);
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshActiveData();
        }

        private void OnWowEventFired(long timestamp, List<string> args)
        {
            Dispatcher.Invoke(() =>
            {
                textboxEventResult.Text += $"{timestamp} - {JsonConvert.SerializeObject(args)}\n";
            });
        }

        private void RefreshActiveData()
        {
            if (tabcontrolMain.SelectedIndex == 0)
            {
                listviewCachePoi.Items.Clear();

                foreach (KeyValuePair<(MapId, PoiType), List<Vector3>> x in AmeisenBot.WowInterface.BotCache.PointsOfInterest.OrderBy(e => e.Key.Item2).ThenBy(e => e.Key.Item1))
                {
                    listviewCachePoi.Items.Add($"{x.Key.Item1} {x.Key.Item2}: {JsonConvert.SerializeObject(x.Value)}");
                }
            }
            if (tabcontrolMain.SelectedIndex == 1)
            {
                listviewCacheOre.Items.Clear();

                foreach (KeyValuePair<(MapId, OreNodes), List<Vector3>> x in AmeisenBot.WowInterface.BotCache.OreNodes.OrderBy(e => e.Key.Item1).ThenBy(e => e.Key.Item2))
                {
                    listviewCacheOre.Items.Add($"{x.Key.Item1} {x.Key.Item2}: {JsonConvert.SerializeObject(x.Value)}");
                }
            }
            else if (tabcontrolMain.SelectedIndex == 2)
            {
                listviewCacheHerb.Items.Clear();

                foreach (KeyValuePair<(MapId, HerbNodes), List<Vector3>> x in AmeisenBot.WowInterface.BotCache.HerbNodes.OrderBy(e => e.Key.Item1).ThenBy(e => e.Key.Item2))
                {
                    listviewCacheHerb.Items.Add($"{x.Key.Item1} {x.Key.Item2}: {JsonConvert.SerializeObject(x.Value)}");
                }
            }
            else if (tabcontrolMain.SelectedIndex == 3)
            {
                listviewCacheNames.Items.Clear();

                foreach (KeyValuePair<ulong, string> x in AmeisenBot.WowInterface.BotCache.NameCache.OrderBy(e => e.Value))
                {
                    listviewCacheNames.Items.Add(x);
                }
            }
            else if (tabcontrolMain.SelectedIndex == 4)
            {
                listviewCacheReactions.Items.Clear();

                foreach (KeyValuePair<(int, int), WowUnitReaction> x in AmeisenBot.WowInterface.BotCache.ReactionCache.OrderBy(e => e.Key.Item1).ThenBy(e => e.Key.Item2))
                {
                    listviewCacheReactions.Items.Add(x);
                }
            }
            else if (tabcontrolMain.SelectedIndex == 5)
            {
                listviewCacheSpellnames.Items.Clear();

                foreach (KeyValuePair<int, string> x in AmeisenBot.WowInterface.BotCache.SpellNameCache.OrderBy(e => e.Value))
                {
                    listviewCacheSpellnames.Items.Add(x);
                }
            }
            else if (tabcontrolMain.SelectedIndex == 6)
            {
                listviewNearWowObjects.Items.Clear();

                List<(WowObject, double)> wowObjects = new List<(WowObject, double)>();

                foreach (WowObject x in AmeisenBot.WowInterface.ObjectManager.WowObjects)
                {
                    wowObjects.Add((x, Math.Round(x.Position.GetDistance(AmeisenBot.WowInterface.ObjectManager.Player.Position), 2)));
                }

                foreach ((WowObject, double) x in wowObjects.OrderBy(e => e.Item2))
                {
                    listviewNearWowObjects.Items.Add(x);
                }
            }
        }

        private void TabcontrolMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshActiveData();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}