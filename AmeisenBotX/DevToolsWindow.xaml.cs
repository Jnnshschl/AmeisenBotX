using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
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
    public partial class DevToolsWindow
    {
        public DevToolsWindow(AmeisenBot ameisenBot)
        {
            AmeisenBot = ameisenBot;
            InitializeComponent();
        }

        private enum MainTab
        {
            CachePoi = 0,
            CacheOre,
            CacheHerb,
            CacheNames,
            CacheReactions,
            CacheSpellNames,
            NearWowObjects,
            Lua,
            Events,
            Logs,
            ClientPatches
        }

        private enum NearWowObjectsTab
        {
            Unselected = -1,
            Items,
            Containers,
            Units,
            Players,
            GameObjects,
            DynamicObjects,
            Corpses,
            AiGroups,
            AreaTriggers
        }

        private AmeisenBot AmeisenBot { get; }

        private static void CopyDataOfNearestObject(ItemsControl listView)
        {
            ItemCollection listItems = listView.Items;
            if (listItems.Count == 0)
            {
                return;
            }

            object firstItem = listItems[0];
            if (firstItem == null)
            {
                return;
            }

            string dataString = firstItem.ToString();
            if (string.IsNullOrEmpty(dataString) || string.IsNullOrWhiteSpace(dataString))
            {
                return;
            }

            string[] splitByGuid = dataString.Split(" Guid:", 2);
            string entryId = splitByGuid[0].Replace("EntryId: ", string.Empty);

            string[] splitByPos = dataString.Split("Pos: [", 2);
            string[] splitByBrace = splitByPos[1].Split("]", 2);

            string[] posComponents = splitByBrace[0].Split(", ");
            string[] cleanComponents = { "", "", "" };

            for (int i = 0; i < posComponents.Length; i++)
            {
                cleanComponents[i] = posComponents[i].Split(".")[0];
            }

            string finalPosStr = "new Vector3(" + cleanComponents[0] + ", " + cleanComponents[1] + ", " + cleanComponents[2] + ")";
            Clipboard.SetDataObject(entryId + ", " + finalPosStr);
        }

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
            AmeisenLogger.I.OnLog -= OnLog;
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

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshActiveData();
        }

        private void ClimbSteepSlopesChecked(object sender, RoutedEventArgs e)
        {
            // Todo: find a better way, multi-level pointer redirection very messy
            AmeisenBot.Bot.Memory.Read<nint>(AmeisenBot.Bot.Memory.Offsets.PlayerBase, out nint PlayerBase1);
            AmeisenBot.Bot.Memory.Read<nint>(nint.Add(PlayerBase1, 0x34), out nint PlayerBase2);
            AmeisenBot.Bot.Memory.Read<nint>(nint.Add(PlayerBase2, 0x24), out nint PlayerBase);
            AmeisenBot.Bot.Memory.Write<float>(nint.Add(PlayerBase, (int)AmeisenBot.Bot.Memory.Offsets.ClimbAngle), 255);
        }

        private void ClimbSteepSlopesUnchecked(object sender, RoutedEventArgs e)
        {
            AmeisenBot.Bot.Memory.Read<nint>(AmeisenBot.Bot.Memory.Offsets.PlayerBase, out nint PlayerBase1);
            AmeisenBot.Bot.Memory.Read<nint>(nint.Add(PlayerBase1, 0x34), out nint PlayerBase2);
            AmeisenBot.Bot.Memory.Read<nint>(nint.Add(PlayerBase2, 0x24), out nint PlayerBase);
            AmeisenBot.Bot.Memory.Write<float>(nint.Add(PlayerBase, (int)AmeisenBot.Bot.Memory.Offsets.ClimbAngle), 1);
        }

        private void DisableM2CollisionsChecked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DisableM2CollisionsUnchecked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DisableWMOCollisionsChecked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DisableWMOCollisionsUnchecked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ListViewNearWowObjects_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.C)
            {
                return;
            }

            switch ((NearWowObjectsTab)tabControlNearWowObjects.SelectedIndex)
            {
                case NearWowObjectsTab.Unselected:
                    break;

                case NearWowObjectsTab.Items:
                    CopyDataOfNearestObject(listViewItems);
                    break;

                case NearWowObjectsTab.Containers:
                    CopyDataOfNearestObject(listViewContainers);
                    break;

                case NearWowObjectsTab.Units:
                    CopyDataOfNearestObject(listViewUnits);
                    break;

                case NearWowObjectsTab.Players:
                    CopyDataOfNearestObject(listViewPlayers);
                    break;

                case NearWowObjectsTab.GameObjects:
                    CopyDataOfNearestObject(listViewGameObjects);
                    break;

                case NearWowObjectsTab.DynamicObjects:
                    CopyDataOfNearestObject(listViewDynamicObjects);
                    break;

                case NearWowObjectsTab.Corpses:
                    CopyDataOfNearestObject(listViewCorpses);
                    break;

                case NearWowObjectsTab.AiGroups:
                    break;

                case NearWowObjectsTab.AreaTriggers:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnLog(LogLevel loglevel, string log)
        {
            Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    if (loglevel > (LogLevel)comboboxLoglevels.SelectedItem)
                    {
                        return;
                    }

                    textboxLogs.AppendText($"{log}\n");
                    textboxLogs.ScrollToEnd();
                }
                catch { }
            });
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
            switch ((MainTab)tabcontrolMain.SelectedIndex)
            {
                case MainTab.CachePoi:
                    {
                        listviewCachePoi.Items.Clear();

                        foreach ((WowMapId mapId, Dictionary<PoiType, List<Vector3>> dictionary) in AmeisenBot.Bot.Db.AllPointsOfInterest()
                            .OrderBy(e => e.Key))
                        {
                            foreach ((PoiType poiType, List<Vector3> list) in dictionary.OrderBy(e => e.Key))
                            {
                                listviewCachePoi.Items.Add($"{mapId} {poiType}: {JsonSerializer.Serialize(list)}");
                            }
                        }
                        break;
                    }
                case MainTab.CacheOre:
                    {
                        listviewCacheOre.Items.Clear();

                        foreach ((WowMapId mapId, Dictionary<WowOreId, List<Vector3>> dictionary) in AmeisenBot.Bot.Db.AllOreNodes()
                            .OrderBy(e => e.Key))
                        {
                            foreach ((WowOreId oreId, List<Vector3> list) in dictionary.OrderBy(e => e.Key))
                            {
                                listviewCacheOre.Items.Add($"{mapId} {oreId}: {JsonSerializer.Serialize(list)}");
                            }
                        }
                        break;
                    }
                case MainTab.CacheHerb:
                    {
                        listviewCacheHerb.Items.Clear();

                        foreach ((WowMapId mapId, Dictionary<WowHerbId, List<Vector3>> dictionary) in AmeisenBot.Bot.Db.AllHerbNodes()
                            .OrderBy(e => e.Key))
                        {
                            foreach ((WowHerbId herbId, List<Vector3> list) in dictionary.OrderBy(e => e.Key))
                            {
                                listviewCacheHerb.Items.Add($"{mapId} {herbId}: {JsonSerializer.Serialize(list)}");
                            }
                        }
                        break;
                    }
                case MainTab.CacheNames:
                    {
                        listviewCacheNames.Items.Clear();

                        foreach (KeyValuePair<ulong, string> x in AmeisenBot.Bot.Db.AllNames()
                            .OrderBy(e => e.Value))
                        {
                            listviewCacheNames.Items.Add(x);
                        }
                        break;
                    }
                case MainTab.CacheReactions:
                    {
                        listviewCacheReactions.Items.Clear();

                        // todo: resolve ... name = mapIdPair implies enum WowMapId not int
                        foreach (KeyValuePair<int, Dictionary<int, WowUnitReaction>> mapIdPair in AmeisenBot.Bot.Db.AllReactions()
                            .OrderBy(e => e.Key))
                        {
                            // todo: same ... typePair as enum? or intPair as int?
                            foreach (KeyValuePair<int, WowUnitReaction> typePair in mapIdPair.Value
                                .OrderBy(e => e.Key))
                            {
                                listviewCacheReactions.Items.Add($"{mapIdPair.Key} {typePair.Key}: {typePair.Value}");
                            }
                        }
                        break;
                    }
                case MainTab.CacheSpellNames:
                    {
                        listviewCacheSpellnames.Items.Clear();

                        foreach (KeyValuePair<int, string> keyValuePair in AmeisenBot.Bot.Db.AllSpellNames()
                            .OrderBy(kvp => kvp.Value))
                        {
                            listviewCacheSpellnames.Items.Add(keyValuePair);
                        }
                        break;
                    }
                case MainTab.NearWowObjects:
                    switch ((NearWowObjectsTab)tabControlNearWowObjects.SelectedIndex)
                    {
                        case NearWowObjectsTab.Unselected:
                            break;

                        // case NearWowObjectsTab.Items: { listViewItems.Items.Clear();
                        //
                        // List<(IWowObject, double)> wowObjects = AmeisenBot.Bot.Objects.WowObjects
                        // .TakeWhile(wowObject => wowObject != null) .Select(wowObject =>
                        // (wowObject,
                        // Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position),
                        // 2))) .ToList();
                        //
                        // foreach ((IWowObject wowObject, double distanceTo) in wowObjects .Where(e
                        // => e.Item1.Type == WowObjectType.Item) .OrderBy(e => e.Item2)) {
                        // listViewItems.Items.Add($"EntryId: {wowObject.EntryId} Guid:
                        // {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale}
                        // Distance: {distanceTo}"); } break; } case NearWowObjectsTab.Containers: { listViewContainers.Items.Clear();
                        //
                        // List<(IWowObject, double)> wowObjects = AmeisenBot.Bot.Objects.WowObjects
                        // .TakeWhile(wowObject => wowObject != null) .Select(wowObject =>
                        // (wowObject,
                        // Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position),
                        // 2))) .ToList();
                        //
                        // foreach ((IWowObject wowObject, double distanceTo) in wowObjects .Where(e
                        // => e.Item1.Type == WowObjectType.Container) .OrderBy(e => e.Item2)) {
                        // listViewContainers.Items.Add($"EntryId: {wowObject.EntryId} Guid:
                        // {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale}
                        // Distance: {distanceTo}"); } break; }
                        case NearWowObjectsTab.Units:
                            {
                                listViewUnits.Items.Clear();

                                List<(IWowUnit, double)> wowObjects = AmeisenBot.Bot.Objects.All
                                    .OfType<IWowUnit>()
                                    .TakeWhile(wowObject => wowObject != null)
                                    .Select(wowObject => (wowObject, Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position), 2)))
                                    .ToList();

                                foreach ((IWowUnit wowObject, double distanceTo) in wowObjects
                                    .OrderBy(e => e.Item2))
                                {
                                    listViewUnits.Items.Add($"EntryId: {wowObject.EntryId} Guid: {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale} Distance: {distanceTo}");
                                }
                                break;
                            }
                        case NearWowObjectsTab.Players:
                            {
                                listViewPlayers.Items.Clear();

                                List<(IWowPlayer, double)> wowObjects = AmeisenBot.Bot.Objects.All
                                    .OfType<IWowPlayer>()
                                    .TakeWhile(wowObject => wowObject != null)
                                    .Select(wowObject => (wowObject, Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position), 2)))
                                    .ToList();

                                foreach ((IWowPlayer wowObject, double distanceTo) in wowObjects
                                    .OrderBy(e => e.Item2))
                                {
                                    listViewPlayers.Items.Add($"EntryId: {wowObject.EntryId} Guid: {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale} Distance: {distanceTo}");
                                }
                                break;
                            }
                        case NearWowObjectsTab.GameObjects:
                            {
                                listViewGameObjects.Items.Clear();

                                List<(IWowGameobject, double)> wowObjects = AmeisenBot.Bot.Objects.All
                                    .OfType<IWowGameobject>()
                                    .TakeWhile(wowObject => wowObject != null)
                                    .Select(wowObject => (wowObject, Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position), 2)))
                                    .ToList();

                                foreach ((IWowGameobject wowObject, double distanceTo) in wowObjects
                                    .OrderBy(e => e.Item2))
                                {
                                    listViewGameObjects.Items.Add($"EntryId: {wowObject.EntryId} Guid: {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale} Distance: {distanceTo}");
                                }
                                break;
                            }
                        case NearWowObjectsTab.DynamicObjects:
                            {
                                listViewDynamicObjects.Items.Clear();

                                List<(IWowDynobject, double)> wowObjects = AmeisenBot.Bot.Objects.All
                                    .OfType<IWowDynobject>()
                                    .TakeWhile(wowObject => wowObject != null)
                                    .Select(wowObject => (wowObject, Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position), 2)))
                                    .ToList();

                                foreach ((IWowDynobject wowObject, double distanceTo) in wowObjects
                                    .OrderBy(e => e.Item2))
                                {
                                    listViewDynamicObjects.Items.Add($"EntryId: {wowObject.EntryId} Guid: {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale} Distance: {distanceTo}");
                                }
                                break;
                            }
                        // case NearWowObjectsTab.Corpses: { listViewCorpses.Items.Clear();
                        //
                        // List<(IWowObject, double)> wowObjects = AmeisenBot.Bot.Objects.WowObjects
                        // .TakeWhile(wowObject => wowObject != null) .Select(wowObject =>
                        // (wowObject,
                        // Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position),
                        // 2))) .ToList();
                        //
                        // foreach ((IWowObject wowObject, double distanceTo) in wowObjects .Where(e
                        // => e.Item1.Type == WowObjectType.Corpse) .OrderBy(e => e.Item2)) {
                        // listViewCorpses.Items.Add($"EntryId: {wowObject.EntryId} Guid:
                        // {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale}
                        // Distance: {distanceTo}"); } break; } case NearWowObjectsTab.AiGroups: { listViewAiGroups.Items.Clear();
                        //
                        // List<(IWowObject, double)> wowObjects = AmeisenBot.Bot.Objects.WowObjects
                        // .TakeWhile(wowObject => wowObject != null) .Select(wowObject =>
                        // (wowObject,
                        // Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position),
                        // 2))) .ToList();
                        //
                        // foreach ((IWowObject wowObject, double distanceTo) in wowObjects .Where(e
                        // => e.Item1.Type == WowObjectType.AiGroup) .OrderBy(e => e.Item2)) {
                        // listViewAiGroups.Items.Add($"EntryId: {wowObject.EntryId} Guid:
                        // {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale}
                        // Distance: {distanceTo}"); } break; } case NearWowObjectsTab.AreaTriggers:
                        // { listViewAreaTriggers.Items.Clear();
                        //
                        // List<(IWowObject, double)> wowObjects = AmeisenBot.Bot.Objects.WowObjects
                        // .TakeWhile(wowObject => wowObject != null) .Select(wowObject =>
                        // (wowObject,
                        // Math.Round(wowObject.Position.GetDistance(AmeisenBot.Bot.Player.Position),
                        // 2))) .ToList();
                        //
                        // foreach ((IWowObject wowObject, double distanceTo) in wowObjects .Where(e
                        // => e.Item1.Type == WowObjectType.AiGroup) .OrderBy(e => e.Item2)) {
                        // listViewAreaTriggers.Items.Add($"EntryId: {wowObject.EntryId} Guid:
                        // {wowObject.Guid} Pos: [{wowObject.Position}] Scale: {wowObject.Scale}
                        // Distance: {distanceTo}"); } break; }

                        default:
                            break;
                    }
                    break;

                case MainTab.Lua: break;
                case MainTab.Events: break;
                case MainTab.Logs: break;
                case MainTab.ClientPatches: break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void TabControlMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshActiveData();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (LogLevel l in Enum.GetValues(typeof(LogLevel)))
            {
                comboboxLoglevels.Items.Add(l);
            }

            comboboxLoglevels.SelectedItem = AmeisenLogger.I.ActiveLogLevel;
            AmeisenLogger.I.OnLog += OnLog;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}