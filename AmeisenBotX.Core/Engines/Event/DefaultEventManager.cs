using AmeisenBotX.Core.Engines.Event.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Engines.Event
{
    /// <summary>
    /// This class is an interface to wow's ingame event system.
    /// </summary>
    public class DefaultEventManager : IEventManager
    {
        public DefaultEventManager(IWowInterface wowInterface, string frameName)
        {
            Wow = wowInterface;
            FrameName = frameName;

            Setup();

            JsonSerializerSettings = new()
            {
                Error = (sender, errorArgs) => errorArgs.ErrorContext.Handled = true
            };
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

        private JsonSerializerSettings JsonSerializerSettings { get; }

        private Queue<string> PendingLuaToExecute { get; set; }

        private IWowInterface Wow { get; }

        ///<inheritdoc cref="IEventManager.OnEventPush(string)"/>
        public void OnEventPush(string eventJson)
        {
            if (eventJson.Length > 2)
            {
                AmeisenLogger.I.Log("WoWEvents", $"Firing events: {eventJson}", LogLevel.Verbose);
                List<WowEvent> events = JsonConvert.DeserializeObject<List<WowEvent>>(eventJson, JsonSerializerSettings);

                if (events != null && events.Count > 0)
                {
                    foreach (WowEvent x in events)
                    {
                        if (x.Name != null && Events.ContainsKey(x.Name))
                        {
                            List<Action<long, List<string>>> actions = Events[x.Name];

                            for (int i = 0; i < actions.Count; ++i)
                            {
                                actions[i](x.Timestamp, x.Arguments);
                            }
                        }
                    }
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

                if (Wow.IsReady)
                {
                    Wow.LuaDoString($"{FrameName}:UnregisterAllEvents();{FrameName}:SetScript(\"OnEvent\", nil);");
                }
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
                && Wow.LuaDoString(PendingLuaToExecute.Peek()))
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

                PendingLuaToExecute.Enqueue(sb.ToString());
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

                PendingLuaToExecute.Enqueue(sb.ToString());
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