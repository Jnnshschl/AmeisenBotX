using AmeisenBotX.Core.Common;
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

            Setup();

            JsonSerializerSettings = new JsonSerializerSettings()
            {
                Error = (sender, errorArgs) => errorArgs.ErrorContext.Handled = true
            };
        }

        public delegate void WowEventAction(long timestamp, List<string> args);

        public Dictionary<string, List<WowEventAction>> EventDictionary { get; private set; }

        public bool IsActive { get; private set; }

        public Queue<(string, WowEventAction)> SubscribeQueue { get; private set; }

        public Queue<(string, WowEventAction)> UnsubscribeQueue { get; private set; }

        private string EventFrameName { get; set; }

        private string EventHandlerName { get; set; }

        private string EventTableName { get; set; }

        private JsonSerializerSettings JsonSerializerSettings { get; }

        private Queue<string> PendingLuaToExecute { get; set; }

        private WowInterface WowInterface { get; }

        public void Pull()
        {
            if (!IsActive || !WowInterface.HookManager.IsWoWHooked)
            {
                return;
            }

            HandleSubEventQueue();
            HandleUnsubEventQueue();

            // execute the pending lua stuff
            if (PendingLuaToExecute.Count > 0
                && WowInterface.HookManager.LuaDoString(PendingLuaToExecute.Peek()))
            {
                PendingLuaToExecute.Dequeue();
            }

            try
            {
                // Unminified lua code can be found im my github repo "WowLuaStuff"
                if (WowInterface.HookManager.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}='['if {EventTableName} then for a,b in pairs({EventTableName})do {{v:0}}={{v:0}}..'{{'for c,d in pairs(b)do if type(d)==\"table\"then {{v:0}}={{v:0}}..'\"args\": ['for e,f in pairs(d)do {{v:0}}={{v:0}}..'\"'..f..'\"'if e<=table.getn(d)then {{v:0}}={{v:0}}..','end end;{{v:0}}={{v:0}}..']}}'if a<table.getn({EventTableName})then {{v:0}}={{v:0}}..','end else if type(d)==\"string\"then {{v:0}}={{v:0}}..'\"event\": \"'..d..'\",'else {{v:0}}={{v:0}}..'\"time\": \"'..d..'\",'end end end end end {{v:0}}={{v:0}}..']'{EventTableName}={{}}"), out string eventJson))
                {
                    List<WowEvent> finalEvents = JsonConvert.DeserializeObject<List<WowEvent>>(eventJson, JsonSerializerSettings);

                    if (finalEvents != null && finalEvents.Count > 0)
                    {
                        for (int i = 0; i < finalEvents.Count; ++i)
                        {
                            WowEvent rawEvent = finalEvents[i];

                            try
                            {
                                if (EventDictionary.ContainsKey(rawEvent.Name))
                                {
                                    AmeisenLogger.Instance.Log("WoWEvents", $"[{rawEvent.Timestamp}] {rawEvent.Name} fired: {JsonConvert.SerializeObject(rawEvent.Arguments)}", LogLevel.Verbose);

                                    for (int e = 0; e < EventDictionary[rawEvent.Name].Count; ++e)
                                    {
                                        EventDictionary[rawEvent.Name][e](rawEvent.Timestamp, rawEvent.Arguments);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                AmeisenLogger.Instance.Log("EventHook", $"Failed to invoke {rawEvent.Name}:\n{e}", LogLevel.Error);
                            }
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
                AmeisenLogger.Instance.Log("EventHook", $"Starting EventHookManager", LogLevel.Verbose);

                AmeisenLogger.Instance.Log("EventHook", $"EventTableName:   {EventTableName}", LogLevel.Verbose);
                AmeisenLogger.Instance.Log("EventHook", $"EventFrameName:   {EventFrameName}", LogLevel.Verbose);
                AmeisenLogger.Instance.Log("EventHook", $"EventHandlerName: {EventHandlerName}", LogLevel.Verbose);

                IsActive = true;
                SetupEventHook();
            }
        }

        public void Stop()
        {
            if (IsActive)
            {
                AmeisenLogger.Instance.Log("EventHook", $"Stopping EventHookManager", LogLevel.Verbose);
                Setup();

                IsActive = false;

                if (WowInterface.HookManager.IsWoWHooked)
                {
                    WowInterface.HookManager.LuaDoString($"{EventFrameName}:UnregisterAllEvents();");
                    WowInterface.HookManager.LuaDoString($"{EventFrameName}:SetScript(\"OnEvent\", nil);");
                }
            }
        }

        public void Subscribe(string eventName, WowEventAction onEventFired)
        {
            AmeisenLogger.Instance.Log("EventHook", $"Subscribing to event: {eventName}", LogLevel.Verbose);
            SubscribeQueue.Enqueue((eventName, onEventFired));
        }

        public void Unsubscribe(string eventName, WowEventAction onEventFired)
        {
            AmeisenLogger.Instance.Log("EventHook", $"Unsubscribing from event: {eventName}", LogLevel.Verbose);
            UnsubscribeQueue.Enqueue((eventName, onEventFired));
        }

        private void HandleSubEventQueue()
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                if (IsActive && SubscribeQueue.Count > 0)
                {
                    while (SubscribeQueue.Count > 0)
                    {
                        (string, WowEventAction) queueElement = SubscribeQueue.Dequeue();

                        if (!EventDictionary.ContainsKey(queueElement.Item1))
                        {
                            EventDictionary.Add(queueElement.Item1, new List<WowEventAction>() { queueElement.Item2 });
                            sb.Append($"{EventFrameName}:RegisterEvent(\"{queueElement.Item1}\");");
                        }
                        else
                        {
                            EventDictionary[queueElement.Item1].Add(queueElement.Item2);
                        }
                    }

                    PendingLuaToExecute.Enqueue(sb.ToString());
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("EventHook", $"Failed subscribe to events:\nEvent Subscribtion LUA:{sb}\n{e}", LogLevel.Error);
            }
        }

        private void HandleUnsubEventQueue()
        {
            try
            {
                if (IsActive && UnsubscribeQueue.Count > 0)
                {
                    (string, WowEventAction) queueElement = SubscribeQueue.Dequeue();

                    if (EventDictionary.ContainsKey(queueElement.Item1))
                    {
                        EventDictionary[queueElement.Item1].Remove(queueElement.Item2);

                        if (EventDictionary[queueElement.Item1].Count == 0)
                        {
                            PendingLuaToExecute.Enqueue($"{EventFrameName}:UnregisterEvent(\"{queueElement.Item1}\");");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("EventHook", $"Failed unsubscribe from event:\n{e}", LogLevel.Error);
            }
        }

        private void Setup()
        {
            EventDictionary = new Dictionary<string, List<WowEventAction>>();
            SubscribeQueue = new Queue<(string, WowEventAction)>();
            UnsubscribeQueue = new Queue<(string, WowEventAction)>();
            PendingLuaToExecute = new Queue<string>();

            EventFrameName = BotUtils.FastRandomStringOnlyLetters();
            EventHandlerName = BotUtils.FastRandomStringOnlyLetters();
            EventTableName = BotUtils.FastRandomStringOnlyLetters();
        }

        private void SetupEventHook()
        {
            AmeisenLogger.Instance.Log("EventHook", $"Setting up the EventHookManager", LogLevel.Verbose);

            StringBuilder luaStuff = new StringBuilder();

            luaStuff.Append($"{EventFrameName}=CreateFrame(\"FRAME\",\"{EventFrameName}\") ");
            luaStuff.Append($"{EventTableName}={{}} ");
            luaStuff.Append($"function {EventHandlerName}(self,event,...) ");
            luaStuff.Append($"table.insert({EventTableName},{{time(),event, {{...}}}})end ");
            luaStuff.Append($"if {EventFrameName}:GetScript(\"OnEvent\")==nil then ");
            luaStuff.Append($"{EventFrameName}:SetScript(\"OnEvent\",{EventHandlerName}) end");

            PendingLuaToExecute.Enqueue(luaStuff.ToString());
        }
    }
}