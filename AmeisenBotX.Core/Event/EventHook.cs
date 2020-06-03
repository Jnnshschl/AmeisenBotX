using AmeisenBotX.Core.Event.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Event
{
    public class EventHook
    {
        public EventHook(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            EventDictionary = new Dictionary<string, List<OnEventFired>>();
            SubscribeQueue = new Queue<(string, OnEventFired)>();
            UnsubscribeQueue = new Queue<(string, OnEventFired)>();
        }

        public delegate void OnEventFired(long timestamp, List<string> args);

        public Dictionary<string, List<OnEventFired>> EventDictionary { get; }

        public bool IsActive { get; private set; }

        public Queue<(string, OnEventFired)> SubscribeQueue { get; }

        public Queue<(string, OnEventFired)> UnsubscribeQueue { get; }

        private WowInterface WowInterface { get; }

        public void Pull()
        {
            if (!IsActive) { return; }

            HandleSubEventQueue();
            HandleUnsubEventQueue();

            try
            {
                // Unminified lua code can be found im my github repo "WowLuaStuff"
                string eventJson = WowInterface.HookManager.ExecuteLuaAndRead("abEventJson='['for a,b in pairs(abEventTable)do abEventJson=abEventJson..'{'for c,d in pairs(b)do if type(d)==\"table\"then abEventJson=abEventJson..'\"args\": ['for e,f in pairs(d)do abEventJson=abEventJson..'\"'..f..'\"'if e<=table.getn(d)then abEventJson=abEventJson..','end end;abEventJson=abEventJson..']}'if a<table.getn(abEventTable)then abEventJson=abEventJson..','end else if type(d)==\"string\"then abEventJson=abEventJson..'\"event\": \"'..d..'\",'else abEventJson=abEventJson..'\"time\": \"'..d..'\",'end end end end;abEventJson=abEventJson..']'abEventTable={}", "abEventJson");

                // sort out the events fired multiple times
                List<WowEvent> finalEvents = JsonConvert.DeserializeObject<List<WowEvent>>(eventJson);
                // buggy atm, prevents multiple item rolls
                // .GroupBy(x => x.Name)
                // .Select(y => y.First())
                // .ToList();

                if (finalEvents != null && finalEvents.Count > 0)
                {
                    foreach (WowEvent rawEvent in finalEvents)
                    {
                        try
                        {
                            if (EventDictionary.ContainsKey(rawEvent.Name))
                            {
                                AmeisenLogger.Instance.Log("WoWEvents", $"[{rawEvent.Timestamp}] {rawEvent.Name} fired: {JsonConvert.SerializeObject(rawEvent.Arguments)}", LogLevel.Verbose);
                                EventDictionary[rawEvent.Name].ForEach(e => e(rawEvent.Timestamp, rawEvent.Arguments));
                            }
                        }
                        catch (Exception e)
                        {
                            AmeisenLogger.Instance.Log("EventHook", $"Failed to invoke {rawEvent.Name}:\n{e}", LogLevel.Error);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("EventHook", $"Failed to read events:\n{e}", LogLevel.Error);
            }
        }

        public void Start()
        {
            if (!IsActive)
            {
                AmeisenLogger.Instance.Log("EventHook", $"Starting EventHookManager...", LogLevel.Verbose);

                IsActive = true;
                SetupEventHook();
            }
        }

        public void Stop()
        {
            if (IsActive)
            {
                AmeisenLogger.Instance.Log("EventHook", $"Stopping EventHookManager...", LogLevel.Verbose);

                IsActive = false;
                WowInterface.HookManager.LuaDoString($"abFrame:UnregisterAllEvents();");
                WowInterface.HookManager.LuaDoString($"abFrame:SetScript(\"OnEvent\", nil);");
            }
        }

        public void Subscribe(string eventName, OnEventFired onEventFired)
        {
            AmeisenLogger.Instance.Log("EventHook", $"Subscribing to event: {eventName}", LogLevel.Verbose);
            SubscribeQueue.Enqueue((eventName, onEventFired));
        }

        public void Unsubscribe(string eventName, OnEventFired onEventFired)
        {
            AmeisenLogger.Instance.Log("EventHook", $"Unsubscribing from event: {eventName}", LogLevel.Verbose);
            UnsubscribeQueue.Enqueue((eventName, onEventFired));
        }

        private void HandleSubEventQueue()
        {
            try
            {
                if (IsActive && SubscribeQueue.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    while (SubscribeQueue.Count > 0)
                    {
                        (string, OnEventFired) queueElement = SubscribeQueue.Dequeue();

                        if (!EventDictionary.ContainsKey(queueElement.Item1))
                        {
                            EventDictionary.Add(queueElement.Item1, new List<OnEventFired>() { queueElement.Item2 });
                            sb.Append($"abFrame:RegisterEvent(\"{queueElement.Item1}\");");
                        }
                        else
                        {
                            EventDictionary[queueElement.Item1].Add(queueElement.Item2);
                        }
                    }

                    WowInterface.HookManager.LuaDoString(sb.ToString());
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("EventHook", $"Failed subscribe to event:\n{e}", LogLevel.Error);
            }
        }

        private void HandleUnsubEventQueue()
        {
            try
            {
                if (IsActive && UnsubscribeQueue.Count > 0)
                {
                    (string, OnEventFired) queueElement = SubscribeQueue.Dequeue();

                    if (EventDictionary.ContainsKey(queueElement.Item1))
                    {
                        EventDictionary[queueElement.Item1].Remove(queueElement.Item2);

                        if (EventDictionary[queueElement.Item1].Count == 0)
                        {
                            WowInterface.HookManager.LuaDoString($"abFrame:UnregisterEvent(\"{queueElement.Item1}\");");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("EventHook", $"Failed unsubscribe from event:\n{e}", LogLevel.Error);
            }
        }

        private void SetupEventHook()
        {
            AmeisenLogger.Instance.Log("EventHook", $"Setting up the EventHookManager...", LogLevel.Verbose);

            StringBuilder luaStuff = new StringBuilder();
            luaStuff.Append("abFrame = CreateFrame(\"FRAME\", \"AbotEventFrame\") ");
            luaStuff.Append("abEventTable = {} ");
            luaStuff.Append("function abEventHandler(self, event, ...) ");
            luaStuff.Append("table.insert(abEventTable, {time(), event, {...}}) end ");
            luaStuff.Append("if abFrame:GetScript(\"OnEvent\") == nil then ");
            luaStuff.Append("abFrame:SetScript(\"OnEvent\", abEventHandler) end");
            WowInterface.HookManager.LuaDoString(luaStuff.ToString());
        }
    }
}