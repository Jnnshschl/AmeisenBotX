using AmeisenBotX.Core.Event.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Event
{
    public class EventHook
    {
        public EventHook(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            Setup();

            JsonSerializerSettings = new()
            {
                Error = (sender, errorArgs) => errorArgs.ErrorContext.Handled = true
            };
        }

        public delegate void WowEventAction(long timestamp, List<string> args);

        public Dictionary<string, List<WowEventAction>> EventDictionary { get; private set; }

        public bool IsActive { get; private set; }

        public Queue<(string, WowEventAction)> SubscribeQueue { get; private set; }

        public Queue<(string, WowEventAction)> UnsubscribeQueue { get; private set; }

        private JsonSerializerSettings JsonSerializerSettings { get; }

        private Queue<string> PendingLuaToExecute { get; set; }

        private WowInterface WowInterface { get; }

        public void ExecutePendingLua()
        {
            HandleSubEventQueue();
            HandleUnsubEventQueue();

            // execute the pending lua stuff
            if (PendingLuaToExecute.Count > 0
                && WowInterface.HookManager.LuaDoString(PendingLuaToExecute.Peek()))
            {
                PendingLuaToExecute.Dequeue();
            }
        }

        public void HookManagerOnEventPush(string eventJson)
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
                            List<WowEventAction> actions = EventDictionary[x.Name];

                            for (int i = 0; i < actions.Count; ++i)
                            {
                                actions[i](x.Timestamp, x.Arguments);
                            }
                        }
                    }
                }
            }
        }

        public void Start()
        {
            if (!IsActive)
            {
                AmeisenLogger.I.Log("EventHook", $"Starting EventHookManager", LogLevel.Verbose);

                WowInterface.HookManager.OnEventPush += HookManagerOnEventPush;
                IsActive = true;
            }
        }

        public void Stop()
        {
            if (IsActive)
            {
                AmeisenLogger.I.Log("EventHook", $"Stopping EventHookManager", LogLevel.Verbose);
                Setup();

                IsActive = false;

                if (WowInterface.HookManager.IsWoWHooked)
                {
                    WowInterface.HookManager.LuaDoString($"{WowInterface.HookManager.EventFrameName}:UnregisterAllEvents();");
                    WowInterface.HookManager.LuaDoString($"{WowInterface.HookManager.EventFrameName}:SetScript(\"OnEvent\", nil);");
                }
            }
        }

        public void Subscribe(string eventName, WowEventAction onEventFired)
        {
            AmeisenLogger.I.Log("EventHook", $"Subscribing to event: {eventName}", LogLevel.Verbose);
            SubscribeQueue.Enqueue((eventName, onEventFired));
        }

        public void Unsubscribe(string eventName, WowEventAction onEventFired)
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
                    (string, WowEventAction) queueElement = SubscribeQueue.Dequeue();

                    if (!EventDictionary.ContainsKey(queueElement.Item1))
                    {
                        EventDictionary.Add(queueElement.Item1, new List<WowEventAction>() { queueElement.Item2 });
                        sb.Append($"{WowInterface.HookManager.EventFrameName}:RegisterEvent(\"{queueElement.Item1}\");");
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
                    (string, WowEventAction) queueElement = UnsubscribeQueue.Dequeue();

                    if (EventDictionary.ContainsKey(queueElement.Item1))
                    {
                        EventDictionary[queueElement.Item1].Remove(queueElement.Item2);

                        if (EventDictionary[queueElement.Item1].Count == 0)
                        {
                            sb.Append($"{WowInterface.HookManager.EventFrameName}:UnregisterEvent(\"{queueElement.Item1}\");");
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