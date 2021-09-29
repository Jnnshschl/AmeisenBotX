using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core;
using AmeisenBotX.Wow.Cache.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
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
            AmeisenBot.Bot.Wow.Events.Subscribe(textboxEventName.Text, OnWowEventFired);
        }

        private void ButtonEventUnsubscribe_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Bot.Wow.Events.Unsubscribe(textboxEventName.Text, OnWowEventFired);
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void ButtonLuaExecute_Click(object sender, RoutedEventArgs e)
        {
            textboxLuaResult.Text = AmeisenBot.Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua(textboxLuaCode.Text), out string result)
                ? result : "Failed to execute LUA...";
        }

        private void ButtonLuaExecute_Copy_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Bot.Wow.LuaDoString(textboxLuaCode.Text);
        }

        private void ListViewNearWowObjects_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C)
            { 
                CopyLocalPlayerPosition(listViewPlayers);
            }
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshActiveData();
        }

        private void TabControlMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshActiveData();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OnWowEventFired(long timestamp, List<string> args)
        {
            Dispatcher.Invoke(() =>
            {
                textboxEventResult.AppendText($"{timestamp} - {JsonSerializer.Serialize(args)}\n");
            });
        }

        private void RefreshActiveData()
        {
            if (tabcontrolMain.SelectedIndex == 0)
            {
                listviewCachePoi.Items.Clear();

                foreach (KeyValuePair<WowMapId, Dictionary<PoiType, List<Vector3>>> mapIdPair in AmeisenBot.Bot.Db.AllPointsOfInterest().OrderBy(e => e.Key))
                {
                    foreach (KeyValuePair<PoiType, List<Vector3>> typePair in mapIdPair.Value.OrderBy(e => e.Key))
                    {
                        listviewCachePoi.Items.Add($"{mapIdPair.Key} {typePair.Key}: {JsonSerializer.Serialize(typePair.Value)}");
                    }
                }
            }
            else if (tabcontrolMain.SelectedIndex == 1)
            {
                listviewCacheOre.Items.Clear();

                foreach (KeyValuePair<WowMapId, Dictionary<WowOreId, List<Vector3>>> mapIdPair in AmeisenBot.Bot.Db.AllOreNodes().OrderBy(e => e.Key))
                {
                    foreach (KeyValuePair<WowOreId, List<Vector3>> typePair in mapIdPair.Value.OrderBy(e => e.Key))
                    {
                        listviewCacheOre.Items.Add($"{mapIdPair.Key} {typePair.Key}: {JsonSerializer.Serialize(typePair.Value)}");
                    }
                }
            }
            else if (tabcontrolMain.SelectedIndex == 2)
            {
                listviewCacheHerb.Items.Clear();

                foreach (KeyValuePair<WowMapId, Dictionary<WowHerbId, List<Vector3>>> mapIdPair in AmeisenBot.Bot.Db.AllHerbNodes().OrderBy(e => e.Key))
                {
                    foreach (KeyValuePair<WowHerbId, List<Vector3>> typePair in mapIdPair.Value.OrderBy(e => e.Key))
                    {
                        listviewCacheHerb.Items.Add($"{mapIdPair.Key} {typePair.Key}: {JsonSerializer.Serialize(typePair.Value)}");
                    }
                }
            }
            else if (tabcontrolMain.SelectedIndex == 3)
            {
                listviewCacheNames.Items.Clear();

                foreach (KeyValuePair<ulong, string> x in AmeisenBot.Bot.Db.AllNames().OrderBy(e => e.Value))
                {
                    listviewCacheNames.Items.Add(x);
                }
            }
            else if (tabcontrolMain.SelectedIndex == 4)
            {
                listviewCacheReactions.Items.Clear();

                foreach (KeyValuePair<int, Dictionary<int, WowUnitReaction>> mapIdPair in AmeisenBot.Bot.Db.AllReactions().OrderBy(e => e.Key))
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

                foreach (KeyValuePair<int, string> x in AmeisenBot.Bot.Db.AllSpellNames().OrderBy(e => e.Value))
                {
                    listviewCacheSpellnames.Items.Add(x);
                }
            }
            else if (tabcontrolMain.SelectedIndex == 6)
            {
                switch (tabControlNearWowObjects.SelectedIndex)
                {
                    case 0: // Items
                    {
                        listViewItems.Items.Clear();

                        List<(IWowObject, double)> wowObjects = AmeisenBot.Bot.Objects.WowObjects
                            .TakeWhile(wowObject => wowObject != null)
                            .Select(wowObject => (wowObject, Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position), 2)))
                            .ToList();

                        foreach ((IWowObject wowObject, double distanceTo) in wowObjects
                            .Where(e => e.Item1.Type == WowObjectType.Item)
                            .OrderBy(e => e.Item2))
                        {
                            listViewItems.Items.Add($"EntryId: {wowObject.EntryId} Guid: {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale} Distance: {distanceTo}");
                        }
                        break;
                    }
                    case 1: // Containers
                    {
                        listViewContainers.Items.Clear();

                        List<(IWowObject, double)> wowObjects = AmeisenBot.Bot.Objects.WowObjects
                            .TakeWhile(wowObject => wowObject != null)
                            .Select(wowObject => (wowObject, Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position), 2)))
                            .ToList();

                        foreach ((IWowObject wowObject, double distanceTo) in wowObjects
                            .Where(e => e.Item1.Type == WowObjectType.Container)
                            .OrderBy(e => e.Item2))
                        {
                            listViewContainers.Items.Add($"EntryId: {wowObject.EntryId} Guid: {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale} Distance: {distanceTo}");
                        }
                        break;
                    }
                    case 2: // Units
                    {
                        listViewUnits.Items.Clear();

                        List<(IWowObject, double)> wowObjects = AmeisenBot.Bot.Objects.WowObjects
                            .TakeWhile(wowObject => wowObject != null)
                            .Select(wowObject => (wowObject, Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position), 2)))
                            .ToList();

                        foreach ((IWowObject wowObject, double distanceTo) in wowObjects
                            .Where(e => e.Item1.Type == WowObjectType.Unit)
                            .OrderBy(e => e.Item2))
                        {
                            listViewUnits.Items.Add($"EntryId: {wowObject.EntryId} Guid: {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale} Distance: {distanceTo}");
                        }
                        break;
                    }
                    case 3: // Players
                    {
                        listViewPlayers.Items.Clear();

                        List<(IWowObject, double)> wowObjects = AmeisenBot.Bot.Objects.WowObjects
                            .TakeWhile(wowObject => wowObject != null)
                            .Select(wowObject => (wowObject, Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position), 2)))
                            .ToList();

                        foreach ((IWowObject wowObject, double distanceTo) in wowObjects
                            .Where(e => e.Item1.Type == WowObjectType.Player)
                            .OrderBy(e => e.Item2))
                        {
                            listViewPlayers.Items.Add($"EntryId: {wowObject.EntryId} Guid: {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale} Distance: {distanceTo}");
                        }
                        break;
                    }
                    case 4: // GameObjects
                    {
                        listViewGameObjects.Items.Clear();

                        List<(IWowObject, double)> wowObjects = AmeisenBot.Bot.Objects.WowObjects
                            .TakeWhile(wowObject => wowObject != null)
                            .Select(wowObject => (wowObject, Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position), 2)))
                            .ToList();

                        foreach ((IWowObject wowObject, double distanceTo) in wowObjects
                            .Where(e => e.Item1.Type == WowObjectType.GameObject)
                            .OrderBy(e => e.Item2))
                        {
                            listViewGameObjects.Items.Add($"EntryId: {wowObject.EntryId} Guid: {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale} Distance: {distanceTo}");
                        }
                        break;
                    }
                    case 5: // DynamicObjects
                    {
                        listViewDynamicObjects.Items.Clear();

                        List<(IWowObject, double)> wowObjects = AmeisenBot.Bot.Objects.WowObjects
                            .TakeWhile(wowObject => wowObject != null)
                            .Select(wowObject => (wowObject, Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position), 2)))
                            .ToList();

                        foreach ((IWowObject wowObject, double distanceTo) in wowObjects
                            .Where(e => e.Item1.Type == WowObjectType.DynamicObject)
                            .OrderBy(e => e.Item2))
                        {
                            listViewDynamicObjects.Items.Add($"EntryId: {wowObject.EntryId} Guid: {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale} Distance: {distanceTo}");
                        }
                        break;
                    }
                    case 6: // Corpses
                    {
                        listViewCorpses.Items.Clear();

                        List<(IWowObject, double)> wowObjects = AmeisenBot.Bot.Objects.WowObjects
                            .TakeWhile(wowObject => wowObject != null)
                            .Select(wowObject => (wowObject, Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position), 2)))
                            .ToList();

                        foreach ((IWowObject wowObject, double distanceTo) in wowObjects
                            .Where(e => e.Item1.Type == WowObjectType.Corpse)
                            .OrderBy(e => e.Item2))
                        {
                            listViewCorpses.Items.Add($"EntryId: {wowObject.EntryId} Guid: {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale} Distance: {distanceTo}");
                        }
                        break;
                    }
                        // todo: case7 AiGroup, case8 AreaTrigger
                }
            }
        }

        private void CopyLocalPlayerPosition(ListView list)
        {
            ItemCollection listItems = list.Items;
            if (listItems.Count <= 0) { return; }
            object playerData = listItems[0];
            if (playerData == null) { return; }

            string[] split = playerData.ToString().Split("[", 2);
            string pos = split[1].Replace("] DistanceTo: 0", string.Empty);
            string[] posComponents = pos.Split(", ");
            string[] cleanComponents = { "", "", "" };

            for (int i = 0; i < posComponents.Length; i++)
            {
                cleanComponents[i] = posComponents[i].Split(".")[0];
            }

            string finalPosStr = cleanComponents[0] + ", " + cleanComponents[1] + ", " + cleanComponents[2];
            Clipboard.SetDataObject(finalPosStr);
        }
    }
}