using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Wow.Events
{
    /// <summary>
    /// This class is an interface to wow's ingame event system.
    /// </summary>
    public class SimpleEventManager : IEventManager
    {
        public SimpleEventManager(Func<string, bool> luaDoString, string frameName)
        {
            LuaDoString = luaDoString;
            FrameName = frameName;

            Setup();
        }

        ///<inheritdoc cref="IEventManager.Events"/>
        public Dictionary<string, List<Action<long, List<string>>>> Events { get; private set; }

        ///<inheritdoc cref="IEventManager.FrameName"/>
        public string FrameName { get; set; }

        ///<inheritdoc cref="IEventManager.IsActive"/>
        public bool IsActive { get; private set; }

        ///<inheritdoc cref="IEventManager.SubscribeQueue"/>
        public Queue<(string, Action<long, List<string>>)> SubscribeQueue { get; private set; }

        ///<inheritdoc cref="IEventManager.UnsubscribeQueue"/>
        public Queue<(string, Action<long, List<string>>)> UnsubscribeQueue { get; private set; }

        private Func<string, bool> LuaDoString { get; }

        private Queue<string> PendingLuaToExecute { get; set; }

        ///<inheritdoc cref="IEventManager.OnEventPush(string)"/>
        public void OnEventPush(string eventJson)
        {
            if (eventJson.Length > 2)
            {
                try
                {
                    List<WowEvent> events = JsonSerializer.Deserialize<List<WowEvent>>(eventJson, new JsonSerializerOptions() { AllowTrailingCommas = true, NumberHandling = JsonNumberHandling.AllowReadingFromString });

                    if (events != null && events.Count > 0)
                    {
                        AmeisenLogger.I.Log("WoWEvents", $"Firing events: {eventJson}", LogLevel.Verbose);

                        foreach (WowEvent x in events)
                        {
                            if (x.Name != null && Events.ContainsKey(x.Name))
                            {
                                List<Action<long, List<string>>> actions = Events[x.Name];

                                foreach (Action<long, List<string>> action in actions)
                                    action(x.Timestamp, x.Arguments);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.I.Log("WoWEvents", $"Failed parsing events: {eventJson}\n{e}", LogLevel.Error);
                }
            }
        }

        ///<inheritdoc cref="IEventManager.Start"/>
        public void Start()
        {
            if (!IsActive)
            {
                AmeisenLogger.I.Log("EventHook", $"Starting EventHookManager", LogLevel.Verbose);
                IsActive = true;
            }
        }

        ///<inheritdoc cref="IEventManager.Stop"/>
        public void Stop()
        {
            if (IsActive)
            {
                AmeisenLogger.I.Log("EventHook", $"Stopping EventHookManager", LogLevel.Verbose);
                Setup();

                IsActive = false;
                LuaDoString($"{FrameName}:UnregisterAllEvents();{FrameName}:SetScript(\"OnEvent\", nil);");
            }
        }

        ///<inheritdoc cref="IEventManager.Subscribe(string, Action{long, List{string}})"/>
        public void Subscribe(string eventName, Action<long, List<string>> onEventFired)
        {
            AmeisenLogger.I.Log("EventHook", $"Subscribing to event: {eventName}", LogLevel.Verbose);
            SubscribeQueue.Enqueue((eventName, onEventFired));
        }

        ///<inheritdoc cref="IEventManager.Tick"/>
        public void Tick()
        {
            HandleSubEventQueue();
            HandleUnsubEventQueue();

            if (PendingLuaToExecute.Count > 0
                && LuaDoString(PendingLuaToExecute.Peek()))
            {
                PendingLuaToExecute.Dequeue();
            }
        }

        ///<inheritdoc cref="IEventManager.Unsubscribe(string, Action{long, List{string}})"/>
        public void Unsubscribe(string eventName, Action<long, List<string>> onEventFired)
        {
            AmeisenLogger.I.Log("EventHook", $"Unsubscribing from event: {eventName}", LogLevel.Verbose);
            UnsubscribeQueue.Enqueue((eventName, onEventFired));
        }

        private void HandleSubEventQueue()
        {
            if (IsActive && SubscribeQueue.Count > 0)
            {
                StringBuilder sb = new();

                while (SubscribeQueue.Count > 0)
                {
                    (string, Action<long, List<string>>) queueElement = SubscribeQueue.Dequeue();

                    if (!Events.ContainsKey(queueElement.Item1))
                    {
                        Events.Add(queueElement.Item1, new List<Action<long, List<string>>>() { queueElement.Item2 });
                        sb.Append($"{FrameName}:RegisterEvent(\"{queueElement.Item1}\");");
                    }
                    else
                    {
                        Events[queueElement.Item1].Add(queueElement.Item2);
                    }
                }

                string lua = sb.ToString();

                if (!string.IsNullOrWhiteSpace(lua))
                {
                    PendingLuaToExecute.Enqueue(lua);
                }
            }
        }

        private void HandleUnsubEventQueue()
        {
            if (IsActive && UnsubscribeQueue.Count > 0)
            {
                StringBuilder sb = new();

                while (SubscribeQueue.Count > 0)
                {
                    (string, Action<long, List<string>>) queueElement = UnsubscribeQueue.Dequeue();

                    if (Events.ContainsKey(queueElement.Item1))
                    {
                        Events[queueElement.Item1].Remove(queueElement.Item2);

                        if (Events[queueElement.Item1].Count == 0)
                        {
                            sb.Append($"{FrameName}:UnregisterEvent(\"{queueElement.Item1}\");");
                        }
                    }
                }

                string lua = sb.ToString();

                if (!string.IsNullOrWhiteSpace(lua))
                {
                    PendingLuaToExecute.Enqueue(lua);
                }
            }
        }

        private void Setup()
        {
            Events = new();
            SubscribeQueue = new();
            UnsubscribeQueue = new();
            PendingLuaToExecute = new();
        }
    }
}