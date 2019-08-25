using AmeisenBotX.Core.Event.Objects;
using AmeisenBotX.Core.Hook;
using Newtonsoft.Json;
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

                List<RawEvent> rawEvents = new List<RawEvent>();
                try
                {
                    List<RawEvent> finalEvents = new List<RawEvent>();
                    rawEvents = JsonConvert.DeserializeObject<List<RawEvent>>(eventJson);

                    foreach (RawEvent rawEvent in rawEvents)
                    {
                        if (!finalEvents.Contains(rawEvent))
                        {
                            finalEvents.Add(rawEvent);
                        }
                    }

                    if (finalEvents.Count > 0)
                    {
                        foreach (RawEvent rawEvent in finalEvents)
                        {
                            if (EventDictionary.ContainsKey(rawEvent.@event))
                            {
                                EventDictionary[rawEvent.@event].Invoke(rawEvent.time, rawEvent.args);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            catch
            {
            }
        }

        public void Start()
        {
            if (!IsSetUp)
            {
                IsSetUp = true;
                SetupEventHook();
            }
        }

        public void Stop()
        {
            HookManager.LuaDoString($"abFrame:UnregisterAllEvents();");
            HookManager.LuaDoString($"abFrame:SetScript(\"OnEvent\", nil);");
        }

        public void Subscribe(string eventName, OnEventFired onEventFired)
        {
            SubscribeQueue.Enqueue((eventName, onEventFired));
        }

        public void Unsubscribe(string eventName)
        {
            UnsubscribeQueue.Enqueue((eventName));
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
            catch { }
        }

        private void HandleUnsubEventQueue()
        {
            try
            {
                if (IsSetUp && SubscribeQueue.Count > 0)
                {
                    string queueElement = UnsubscribeQueue.Dequeue();
                    HookManager.LuaDoString($"abFrame:UnregisterEvent(\"{queueElement}\");");
                    EventDictionary.Remove(queueElement);
                }
            }
            catch { }
        }

        private void SetupEventHook()
        {
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