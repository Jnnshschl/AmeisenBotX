using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;
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
            if (AmeisenBot.WowInterface.NewWowInterface.WowExecuteLuaAndRead(BotUtils.ObfuscateLua(textboxLuaCode.Text), out string result))
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
            AmeisenBot.WowInterface.NewWowInterface.LuaDoString(textboxLuaCode.Text);
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshActiveData();
        }

        private void OnWowEventFired(long timestamp, List<string> args)
        {
            Dispatcher.Invoke(() =>
            {
                textboxEventResult.AppendText($"{timestamp} - {JsonConvert.SerializeObject(args)}\n");
            });
        }

        private void RefreshActiveData()
        {
            if (tabcontrolMain.SelectedIndex == 0)
            {
                listviewCachePoi.Items.Clear();

                foreach (KeyValuePair<WowMapId, Dictionary<PoiType, List<Vector3>>> mapIdPair in AmeisenBot.WowInterface.Db.AllPointsOfInterest().OrderBy(e => e.Key))
                {
                    foreach (KeyValuePair<PoiType, List<Vector3>> typePair in mapIdPair.Value.OrderBy(e => e.Key))
                    {
                        listviewCachePoi.Items.Add($"{mapIdPair.Key} {typePair.Key}: {JsonConvert.SerializeObject(typePair.Value)}");
                    }
                }
            }
            else if (tabcontrolMain.SelectedIndex == 1)
            {
                listviewCacheOre.Items.Clear();

                foreach (KeyValuePair<WowMapId, Dictionary<WowOreId, List<Vector3>>> mapIdPair in AmeisenBot.WowInterface.Db.AllOreNodes().OrderBy(e => e.Key))
                {
                    foreach (KeyValuePair<WowOreId, List<Vector3>> typePair in mapIdPair.Value.OrderBy(e => e.Key))
                    {
                        listviewCacheOre.Items.Add($"{mapIdPair.Key} {typePair.Key}: {JsonConvert.SerializeObject(typePair.Value)}");
                    }
                }
            }
            else if (tabcontrolMain.SelectedIndex == 2)
            {
                listviewCacheHerb.Items.Clear();

                foreach (KeyValuePair<WowMapId, Dictionary<WowHerbId, List<Vector3>>> mapIdPair in AmeisenBot.WowInterface.Db.AllHerbNodes().OrderBy(e => e.Key))
                {
                    foreach (KeyValuePair<WowHerbId, List<Vector3>> typePair in mapIdPair.Value.OrderBy(e => e.Key))
                    {
                        listviewCacheHerb.Items.Add($"{mapIdPair.Key} {typePair.Key}: {JsonConvert.SerializeObject(typePair.Value)}");
                    }
                }
            }
            else if (tabcontrolMain.SelectedIndex == 3)
            {
                listviewCacheNames.Items.Clear();

                foreach (KeyValuePair<ulong, string> x in AmeisenBot.WowInterface.Db.AllNames().OrderBy(e => e.Value))
                {
                    listviewCacheNames.Items.Add(x);
                }
            }
            else if (tabcontrolMain.SelectedIndex == 4)
            {
                listviewCacheReactions.Items.Clear();

                foreach (KeyValuePair<int, Dictionary<int, WowUnitReaction>> mapIdPair in AmeisenBot.WowInterface.Db.AllReactions().OrderBy(e => e.Key))
                {
                    foreach (KeyValuePair<int, WowUnitReaction> typePair in mapIdPair.Value.OrderBy(e => e.Key))
                    {
                        listviewCacheReactions.Items.Add($"{mapIdPair.Key} {typePair.Key}: {typePair.Value}");
                    }
                }
            }
            else if (tabcontrolMain.SelectedIndex == 5)
            {
                listviewCacheSpellnames.Items.Clear();

                foreach (KeyValuePair<int, string> x in AmeisenBot.WowInterface.Db.AllSpellNames().OrderBy(e => e.Value))
                {
                    listviewCacheSpellnames.Items.Add(x);
                }
            }
            else if (tabcontrolMain.SelectedIndex == 6)
            {
                listviewNearWowObjects.Items.Clear();

                List<(WowObject, double)> wowObjects = new();

                foreach (WowObject x in AmeisenBot.WowInterface.Objects.WowObjects)
                {
                    if (x == null)
                    {
                        break;
                    }

                    wowObjects.Add((x, Math.Round(x.Position.GetDistance(AmeisenBot.WowInterface.Player.Position), 2)));
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