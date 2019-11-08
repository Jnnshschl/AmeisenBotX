using AmeisenBotX.Core.Event.Objects;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Event
{
    public class EventHookManager
    {
        public EventHookManager(HookManager hookManager)
        {
            EventDictionary = new Dictionary<string, OnEventFired>();
            SubscribeQueue = new Queue<(string, OnEventFired)>();
            UnsubscribeQueue = new Queue<string>();
            HookManager = hookManager;
            IsSetUp = false;
        }

        public delegate void OnEventFired(long timestamp, List<string> args);

        public Dictionary<string, OnEventFired> EventDictionary { get; }

        public bool IsSetUp { get; private set; }

        public Queue<(string, OnEventFired)> SubscribeQueue { get; }

        public Queue<string> UnsubscribeQueue { get; }

        private HookManager HookManager { get; }

        public void ReadEvents()
        {
            HandleSubEventQueue();
            HandleUnsubEventQueue();

            try
            {
                // Unminified lua code can be found im my github repo "WowLuaStuff"
                HookManager.LuaDoString("abEventJson='['for a,b in pairs(abEventTable)do abEventJson=abEventJson..'{'for c,d in pairs(b)do if type(d)==\"table\"then abEventJson=abEventJson..'\"args\": ['for e,f in pairs(d)do abEventJson=abEventJson..'\"'..f..'\"'if e<=table.getn(d)then abEventJson=abEventJson..','end end;abEventJson=abEventJson..']}'if a<table.getn(abEventTable)then abEventJson=abEventJson..','end else if type(d)==\"string\"then abEventJson=abEventJson..'\"event\": \"'..d..'\",'else abEventJson=abEventJson..'\"time\": \"'..d..'\",'end end end end;abEventJson=abEventJson..']'abEventTable={}");
                string eventJson = HookManager.GetLocalizedText("abEventJson");

                List<WowEvent> rawEvents = new List<WowEvent>();
                try
                {
                    List<WowEvent> finalEvents = new List<WowEvent>();
                    rawEvents = JsonConvert.DeserializeObject<List<WowEvent>>(eventJson);

                    foreach (WowEvent rawEvent in rawEvents)
                    {
                        if (!finalEvents.Contains(rawEvent))
                        {
                            finalEvents.Add(rawEvent);
                        }
                    }

                    if (finalEvents.Count > 0)
                    {
                        foreach (WowEvent rawEvent in finalEvents)
                        {
                            try
                            {
                                if (EventDictionary.ContainsKey(rawEvent.EventName))
                                {
                                    EventDictionary[rawEvent.EventName].Invoke(rawEvent.Timestamp, rawEvent.Arguments);
                                }
                            }
                            catch (Exception e)
                            {
                                AmeisenLogger.Instance.Log($"Failed to invoke {rawEvent.EventName}:\n{e.ToString()}", LogLevel.Error);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log($"Failed to parse events:\neventJson: {eventJson}\n{e.ToString()}", LogLevel.Error);
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log($"Failed to read events:\n{e.ToString()}", LogLevel.Error);
            }
        }

        public void Start()
        {
            AmeisenLogger.Instance.Log($"Starting EventHookManager...", LogLevel.Verbose);
            if (!IsSetUp)
            {
                IsSetUp = true;
                SetupEventHook();
            }
        }

        public void Stop()
        {
            AmeisenLogger.Instance.Log($"Stopping EventHookManager...", LogLevel.Verbose);
            HookManager.LuaDoString($"abFrame:UnregisterAllEvents();");
            HookManager.LuaDoString($"abFrame:SetScript(\"OnEvent\", nil);");
        }

        public void Subscribe(string eventName, OnEventFired onEventFired)
        {
            AmeisenLogger.Instance.Log($"Subscribing to event: {eventName}", LogLevel.Verbose);
            SubscribeQueue.Enqueue((eventName, onEventFired));
        }

        public void Unsubscribe(string eventName)
        {
            AmeisenLogger.Instance.Log($"Unsubscribing from event: {eventName}", LogLevel.Verbose);
            UnsubscribeQueue.Enqueue(eventName);
        }

        private void HandleSubEventQueue()
        {
            try
            {
                if (IsSetUp && SubscribeQueue.Count > 0)
                {
                    (string, OnEventFired) queueElement = SubscribeQueue.Dequeue();
                    HookManager.LuaDoString($"abFrame:RegisterEvent(\"{queueElement.Item1}\");");
                    EventDictionary.Add(queueElement.Item1, queueElement.Item2);
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log($"Failed subscribe to event:\n{e.ToString()}", LogLevel.Error);
            }
        }

        private void HandleUnsubEventQueue()
        {
            try
            {
                if (IsSetUp && UnsubscribeQueue.Count > 0)
                {
                    string queueElement = UnsubscribeQueue.Dequeue();
                    HookManager.LuaDoString($"abFrame:UnregisterEvent(\"{queueElement}\");");
                    EventDictionary.Remove(queueElement);
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log($"Failed unsubscribe from event:\n{e.ToString()}", LogLevel.Error);
            }
        }

        private void SetupEventHook()
        {
            AmeisenLogger.Instance.Log($"Setting up the EventHookManager...", LogLevel.Verbose);

            StringBuilder luaStuff = new StringBuilder();
            luaStuff.Append("abFrame = CreateFrame(\"FRAME\", \"AbotEventFrame\") ");
            luaStuff.Append("abEventTable = {} ");
            luaStuff.Append("function abEventHandler(self, event, ...) ");
            luaStuff.Append("table.insert(abEventTable, {time(), event, {...}}) end ");
            luaStuff.Append("if abFrame:GetScript(\"OnEvent\") == nil then ");
            luaStuff.Append("abFrame:SetScript(\"OnEvent\", abEventHandler) end");
            HookManager.LuaDoString(luaStuff.ToString());
        }
    }
}
