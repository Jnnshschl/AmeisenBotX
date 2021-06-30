using AmeisenBotX.Core.Event.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Event
{
    /// <summary>
    /// This class is an interface to wow's ingame event system.
    /// </summary>
    public class EventHook
    {
        public EventHook(IWowInterface wowInterface)
        {
            Wow = wowInterface;

            Setup();

            JsonSerializerSettings = new()
            {
                Error = (sender, errorArgs) => errorArgs.ErrorContext.Handled = true
            };
        }

        /// <summary>
        /// Contains all the subscribed events and functions.
        /// </summary>
        public Dictionary<string, List<Action<long, List<string>>>> EventDictionary { get; private set; }

        /// <summary>
        /// The current wow event frame name.
        /// </summary>
        public string EventHookFrameName { get; set; }

        public bool IsActive { get; private set; }

        /// <summary>
        /// Pending subscribe actions.
        /// </summary>
        public Queue<(string, Action<long, List<string>>)> SubscribeQueue { get; private set; }

        /// <summary>
        /// Pending unsubscribe actions.
        /// </summary>
        public Queue<(string, Action<long, List<string>>)> UnsubscribeQueue { get; private set; }

        private JsonSerializerSettings JsonSerializerSettings { get; }

        private Queue<string> PendingLuaToExecute { get; set; }

        private IWowInterface Wow { get; }

        /// <summary>
        /// Call this periodically to handle the subscription and unsubscription queues.
        /// </summary>
        public void ExecutePendingLua()
        {
            HandleSubEventQueue();
            HandleUnsubEventQueue();

            // execute the pending lua stuff
            if (PendingLuaToExecute.Count > 0
                && Wow.LuaDoString(PendingLuaToExecute.Peek()))
            {
                PendingLuaToExecute.Dequeue();
            }
        }

        /// <summary>
        /// Call this with the new event string received from the hook module.
        /// </summary>
        /// <param name="eventJson">JSON events string</param>
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
                        if (x.Name != null && EventDictionary.ContainsKey(x.Name))
                        {
                            List<Action<long, List<string>>> actions = EventDictionary[x.Name];

                            for (int i = 0; i < actions.Count; ++i)
                            {
                                actions[i](x.Timestamp, x.Arguments);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the event hookand sets it to active.
        /// </summary>
        public void Start()
        {
            if (!IsActive)
            {
                AmeisenLogger.I.Log("EventHook", $"Starting EventHookManager", LogLevel.Verbose);
                IsActive = true;
            }
        }

        /// <summary>
        /// Unloads the event hook and unregisters all events ingame.
        /// </summary>
        public void Stop()
        {
            if (IsActive)
            {
                AmeisenLogger.I.Log("EventHook", $"Stopping EventHookManager", LogLevel.Verbose);
                Setup();

                IsActive = false;

                if (Wow.IsReady)
                {
                    Wow.LuaDoString($"{EventHookFrameName}:UnregisterAllEvents();{EventHookFrameName}:SetScript(\"OnEvent\", nil);");
                }
            }
        }

        /// <summary>
        /// Subscribe to a wow event.
        /// </summary>
        /// <param name="eventName">Wow event name</param>
        /// <param name="onEventFired">Callback</param>
        public void Subscribe(string eventName, Action<long, List<string>> onEventFired)
        {
            AmeisenLogger.I.Log("EventHook", $"Subscribing to event: {eventName}", LogLevel.Verbose);
            SubscribeQueue.Enqueue((eventName, onEventFired));
        }

        /// <summary>
        /// Unsubscribe from a wow event.
        /// </summary>
        /// <param name="eventName">Wow event name</param>
        /// <param name="onEventFired">Callback to remove</param>
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

                    if (!EventDictionary.ContainsKey(queueElement.Item1))
                    {
                        EventDictionary.Add(queueElement.Item1, new List<Action<long, List<string>>>() { queueElement.Item2 });
                        sb.Append($"{EventHookFrameName}:RegisterEvent(\"{queueElement.Item1}\");");
                    }
                    else
                    {
                        EventDictionary[queueElement.Item1].Add(queueElement.Item2);
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

                    if (EventDictionary.ContainsKey(queueElement.Item1))
                    {
                        EventDictionary[queueElement.Item1].Remove(queueElement.Item2);

                        if (EventDictionary[queueElement.Item1].Count == 0)
                        {
                            sb.Append($"{EventHookFrameName}:UnregisterEvent(\"{queueElement.Item1}\");");
                        }
                    }
                }

                PendingLuaToExecute.Enqueue(sb.ToString());
            }
        }

        private void Setup()
        {
            EventDictionary = new();
            SubscribeQueue = new();
            UnsubscribeQueue = new();
            PendingLuaToExecute = new();
        }
    }
}