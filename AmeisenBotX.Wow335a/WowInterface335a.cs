using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Hook.Modules;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Events;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow335a.Events;
using AmeisenBotX.Wow335a.Hook;
using AmeisenBotX.Wow335a.Objects;
using AmeisenBotX.Wow335a.Offsets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Wow335a
{
    /// <summary>
    /// WowInterface for the game version 3.3.5a 12340.
    /// </summary>
    public class WowInterface335a : IWowInterface
    {
        public WowInterface335a(IMemoryApi memoryApi)
        {
            Memory = memoryApi;

            OffsetList = new();
            HookModules = new();

            // lua variable names for the event hook
            string handlerName = BotUtils.FastRandomStringOnlyLetters();
            string tableName = BotUtils.FastRandomStringOnlyLetters();
            string eventHookOutput = BotUtils.FastRandomStringOnlyLetters();

            // name of the frame used to capture wows events
            string eventHookFrameName = BotUtils.FastRandomStringOnlyLetters();
            EventManager = new(LuaDoString, eventHookFrameName);

            // module to process wows events.
            HookModules.Add(new RunLuaHookModule
            (
                (x) =>
                {
                    if (x != IntPtr.Zero
                        && memoryApi.ReadString(x, Encoding.UTF8, out string s)
                        && !string.IsNullOrWhiteSpace(s))
                    {
                        EventManager.OnEventPush(s);
                    }
                },
                null,
                memoryApi,
                Offsets,
                $"{eventHookOutput}='['function {handlerName}(self,a,...)table.insert({tableName},{{time(),a,{{...}}}})end if {eventHookFrameName}==nil then {tableName}={{}}{eventHookFrameName}=CreateFrame(\"FRAME\"){eventHookFrameName}:SetScript(\"OnEvent\",{handlerName})else for b,c in pairs({tableName})do {eventHookOutput}={eventHookOutput}..'{{'for d,e in pairs(c)do if type(e)==\"table\"then {eventHookOutput}={eventHookOutput}..'\"args\": ['for f,g in pairs(e)do {eventHookOutput}={eventHookOutput}..'\"'..g..'\"'if f<=table.getn(e)then {eventHookOutput}={eventHookOutput}..','end end {eventHookOutput}={eventHookOutput}..']}}'if b<table.getn({tableName})then {eventHookOutput}={eventHookOutput}..','end else if type(e)==\"string\"then {eventHookOutput}={eventHookOutput}..'\"event\": \"'..e..'\",'else {eventHookOutput}={eventHookOutput}..'\"time\": \"'..e..'\",'end end end end end {eventHookOutput}={eventHookOutput}..']'{tableName}={{}}",
                eventHookOutput
            ));

            string staticPopupsVarName = BotUtils.FastRandomStringOnlyLetters();
            string oldPoupString = string.Empty;

            // module that monitors the STATIC_POPUP windows.
            HookModules.Add(new RunLuaHookModule
            (
                (x) =>
                {
                    if (x != IntPtr.Zero
                        && memoryApi.ReadString(x, Encoding.UTF8, out string s)
                        && !string.IsNullOrWhiteSpace(s))
                    {
                        if (!oldPoupString.Equals(s, StringComparison.Ordinal))
                        {
                            OnStaticPopup?.Invoke(s);
                            oldPoupString = s;
                        }
                    }
                    else
                    {
                        oldPoupString = string.Empty;
                    }
                },
                null,
                memoryApi,
                Offsets,
                $"{staticPopupsVarName}=\"\"for b=1,STATICPOPUP_NUMDIALOGS do local c=_G[\"StaticPopup\"..b]if c:IsShown()then {staticPopupsVarName}={staticPopupsVarName}..b..\":\"..c.which..\"; \"end end",
                staticPopupsVarName
            ));

            string battlegroundStatusVarName = BotUtils.FastRandomStringOnlyLetters();
            string oldBattlegroundStatus = string.Empty;

            // module to monitor the battleground (and queue) status.
            HookModules.Add(new RunLuaHookModule
            (
                (x) =>
                {
                    if (x != IntPtr.Zero
                        && memoryApi.ReadString(x, Encoding.UTF8, out string s)
                        && !string.IsNullOrWhiteSpace(s))
                    {
                        if (!oldBattlegroundStatus.Equals(s, StringComparison.Ordinal))
                        {
                            OnBattlegroundStatus?.Invoke(s);
                            oldBattlegroundStatus = s;
                        }
                    }
                    else
                    {
                        oldBattlegroundStatus = string.Empty;
                    }
                },
                null,
                memoryApi,
                Offsets,
                $"{battlegroundStatusVarName}=\"\"for b=1,MAX_BATTLEFIELD_QUEUES do local c,d,e,f,g,h=GetBattlefieldStatus(b)local i=GetBattlefieldTimeWaited(b)/1000;{battlegroundStatusVarName}={battlegroundStatusVarName}..b..\":\"..tostring(c or\"unknown\")..\":\"..tostring(d or\"unknown\")..\":\"..tostring(e or\"unknown\")..\":\"..tostring(f or\"unknown\")..\":\"..tostring(g or\"unknown\")..\":\"..tostring(h or\"unknown\")..\":\"..tostring(i or\"unknown\")..\";\"end",
                battlegroundStatusVarName
            ));

            // module to detect small obstacles that we can jump over
            HookModules.Add(new TracelineJumpHookModule
            (
                null,
                (x) =>
                {
                    IntPtr dataPtr = x.GetDataPointer();

                    if (dataPtr != IntPtr.Zero && Player != null)
                    {
                        Vector3 playerPosition = Player.Position;
                        playerPosition.Z += 1.3f;

                        Vector3 pos = BotUtils.MoveAhead(playerPosition, Player.Rotation, 0.25f);
                        memoryApi.Write(dataPtr, (1.0f, playerPosition, pos));
                    }
                },
                memoryApi,
                Offsets
            ));

            ObjectManager = new(memoryApi, Offsets);

            Hook = new(memoryApi, Offsets, ObjectManager);
            Hook.OnGameInfoPush += ObjectManager.HookManagerOnGameInfoPush;
        }

        public event Action<string> OnBattlegroundStatus;

        public event Action<string> OnStaticPopup;

        public IEventManager Events => EventManager;

        public int HookCallCount => Hook.HookCallCount;

        public bool IsReady => Hook.IsWoWHooked;

        public IObjectProvider ObjectProvider => ObjectManager;

        public IOffsetList Offsets => OffsetList;

        public IWowPlayer Player => ObjectManager.Player;

        private SimpleEventManager EventManager { get; }

        private EndSceneHook Hook { get; }

        private List<IHookModule> HookModules { get; }

        private IMemoryApi Memory { get; }

        private ObjectManager ObjectManager { get; }

        private OffsetList335a OffsetList { get; }

        public void AbandonQuestsNotIn(IEnumerable<string> quests)
        {
            Hook.LuaAbandonQuestsNotIn(quests);
        }

        public void AcceptBattlegroundInvite()
        {
            ClickUiElement("StaticPopup1Button1");
        }

        public void AcceptPartyInvite()
        {
            LuaDoString("AcceptGroup();StaticPopup_Hide(\"PARTY_INVITE\")");
        }

        public void AcceptQuest()
        {
            LuaDoString($"AcceptQuest()");
        }

        public void AcceptQuests()
        {
            LuaDoString("active=GetNumGossipActiveQuests()if active>0 then for a=1,active do if not not select(a*5-5+4,GetGossipActiveQuests())then SelectGossipActiveQuest(a)end end end;available=GetNumGossipAvailableQuests()if available>0 then for a=1,available do if not not not select(a*6-6+3,GetGossipAvailableQuests())then SelectGossipAvailableQuest(a)end end end;if available==0 and active==0 and GetNumGossipOptions()==1 then _,type=GetGossipOptions()if type=='gossip'then SelectGossipOption(1)return end end");
        }

        public void AcceptResurrect()
        {
            LuaDoString("AcceptResurrect();");
        }

        public void AcceptSummon()
        {
            LuaDoString("ConfirmSummon();StaticPopup_Hide(\"CONFIRM_SUMMON\")");
        }

        public void BuyTrainerService(int serviceIndex)
        {
            LuaDoString($"BuyTrainerService({serviceIndex})");
        }

        public void CallCompanion(int index, string type)
        {
            LuaDoString($"CallCompanion(\"{type}\", {index})");
        }

        public void CastSpell(string name, bool castOnSelf = false)
        {
            LuaDoString($"CastSpellByName(\"{name}\"{(castOnSelf ? ", \"player\"" : string.Empty)})");
        }

        public void CastSpellById(int spellId)
        {
            LuaDoString($"CastSpellByID({spellId})");
        }

        public void ChangeTarget(ulong guid)
        {
            Hook.TargetGuid(guid);
        }

        public void ClearTarget()
        {
            ChangeTarget(0);
        }

        public void ClickOnTerrain(Vector3 position)
        {
            Hook.ClickOnTerrain(position);
        }

        public void ClickOnTrainButton()
        {
            LuaDoString("LoadAddOn\"Blizzard_TrainerUI\"f=ClassTrainerTrainButton;f.e=0;if f:GetScript\"OnUpdate\"then f:SetScript(\"OnUpdate\",nil)else f:SetScript(\"OnUpdate\",function(f,a)f.e=f.e+a;if f.e>.01 then f.e=0;f:Click()end end)end");
        }

        public void ClickUiElement(string elementName)
        {
            LuaDoString($"{elementName}:Click()");
        }

        public void CofirmLootRoll()
        {
            CofirmStaticPopup();
        }

        public void CofirmReadyCheck(bool isReady)
        {
            LuaDoString($"ConfirmReadyCheck({isReady})");
        }

        public void CofirmStaticPopup()
        {
            LuaDoString($"EquipPendingItem(0);ConfirmBindOnUse();StaticPopup_Hide(\"AUTOEQUIP_BIND\");StaticPopup_Hide(\"EQUIP_BIND\");StaticPopup_Hide(\"USE_BIND\")");
        }

        public void CompleteQuest()
        {
            LuaDoString($"CompleteQuest()");
        }

        public void DeleteItemByName(string itemName)
        {
            LuaDoString($"for b=0,4 do for s=1,GetContainerNumSlots(b) do local l=GetContainerItemLink(b,s); if l and string.find(l, \"{itemName}\") then PickupContainerItem(b,s); DeleteCursorItem(); end; end; end");
        }

        public void DismissCompanion(string type)
        {
            LuaDoString($"DismissCompanion(\"{type}\")");
        }

        public void Dispose()
        {
            Hook.Unhook();
        }

        public void EnableClickToMove()
        {
            Hook.EnableClickToMove();
        }

        public void EquipItem(string newItem, int itemSlot = -1)
        {
            if (itemSlot == -1)
            {
                LuaDoString($"EquipItemByName(\"{newItem}\")");
            }
            else
            {
                LuaDoString($"EquipItemByName(\"{newItem}\", {itemSlot})");
            }

            CofirmStaticPopup();
        }

        public bool ExecuteLuaAndRead((string, string) p, out string result)
        {
            return Hook.ExecuteLuaAndRead(p, out result);
        }

        public void FacePosition(IntPtr playerBase, Vector3 playerPosition, Vector3 position)
        {
            Hook.FacePosition(playerBase, playerPosition, position);
        }

        public IEnumerable<int> GetCompletedQuests()
        {
            if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=''for a,b in pairs(GetQuestsCompleted())do if b then {{v:0}}={{v:0}}..a..';'end end;"), out string result))
            {
                if (result != null && result.Length > 0)
                {
                    return result.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => int.TryParse(e, out int n) ? n : (int?)null)
                        .Where(e => e.HasValue)
                        .Select(e => e.Value);
                }
            }

            return Array.Empty<int>();
        }

        public string GetEquipmentItems()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"[\"for a=0,23 do {v:1}=GetInventoryItemID(\"player\",a)if string.len(tostring({v:1} or\"\"))>0 then {v:2}=GetInventoryItemLink(\"player\",a){v:3}=GetInventoryItemCount(\"player\",a){v:4},{v:5}=GetInventoryItemDurability(a){v:6},{v:7}=GetInventoryItemCooldown(\"player\",a){v:8},{v:9},{v:10},{v:11},{v:12},{v:13},{v:14},{v:15},{v:16},{v:17},{v:18}=GetItemInfo({v:2}){v:19}=GetItemStats({v:2}){v:20}={}for b,c in pairs({v:19})do table.insert({v:20},string.format(\"\\\"%s\\\":\\\"%s\\\"\",b,c))end;{v:0}={v:0}..'{'..'\"id\": \"'..tostring({v:1} or 0)..'\",'..'\"count\": \"'..tostring({v:3} or 0)..'\",'..'\"quality\": \"'..tostring({v:10} or 0)..'\",'..'\"curDurability\": \"'..tostring({v:4} or 0)..'\",'..'\"maxDurability\": \"'..tostring({v:5} or 0)..'\",'..'\"cooldownStart\": \"'..tostring({v:6} or 0)..'\",'..'\"cooldownEnd\": '..tostring({v:7} or 0)..','..'\"name\": \"'..tostring({v:8} or 0)..'\",'..'\"link\": \"'..tostring({v:9} or 0)..'\",'..'\"level\": \"'..tostring({v:11} or 0)..'\",'..'\"minLevel\": \"'..tostring({v:12} or 0)..'\",'..'\"type\": \"'..tostring({v:13} or 0)..'\",'..'\"subtype\": \"'..tostring({v:14} or 0)..'\",'..'\"maxStack\": \"'..tostring({v:15} or 0)..'\",'..'\"equipslot\": \"'..tostring(a or 0)..'\",'..'\"equiplocation\": \"'..tostring({v:16} or 0)..'\",'..'\"stats\": '..\"{\"..table.concat({v:20},\",\")..\"}\"..','..'\"sellprice\": \"'..tostring({v:18} or 0)..'\"'..'}'if a<23 then {v:0}={v:0}..\",\"end end end;{v:0}={v:0}..\"]\""), out string result) ? result : string.Empty;
        }

        public int GetFreeBagSlotCount()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=0 for i=1,5 do {v:0}={v:0}+GetContainerNumFreeSlots(i-1)end"), out string sresult)
                && int.TryParse(sresult, out int freeBagSlots)
                 ? freeBagSlots : 0;
        }

        public string[] GetGossipTypes()
        {
            try
            {
                ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\"function {v:1}(...)for a=1,select(\"#\",...),2 do {v:0}={v:0}..select(a+1,...)..\";\"end end;{v:1}(GetGossipOptions())"), out string result);
                return result.Split(';', StringSplitOptions.RemoveEmptyEntries);
            }
            catch
            {
                // ignored
            }

            return Array.Empty<string>();
        }

        public string GetInventoryItems()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"[\"for a=0,4 do {v:1}=GetContainerNumSlots(a)for b=1,{v:1} do {v:2}=GetContainerItemID(a,b)if string.len(tostring({v:2} or\"\"))>0 then {v:3}=GetContainerItemLink(a,b){v:4},{v:5}=GetContainerItemDurability(a,b){v:6},{v:7}=GetContainerItemCooldown(a,b){v:8},{v:9},{v:10},{v:11},{v:12},{v:13},{v:3},{v:14}=GetContainerItemInfo(a,b){v:15},{v:16},{v:17},{v:18},{v:19},{v:20},{v:21},{v:22},{v:23},{v:8},{v:24}=GetItemInfo({v:3}){v:25}=GetItemStats({v:3}){v:26}={}if {v:25} then for c,d in pairs({v:25})do table.insert({v:26},string.format(\"\\\"%s\\\":\\\"%s\\\"\",c,d))end;end;{v:0}={v:0}..\"{\"..'\"id\": \"'..tostring({v:2} or 0)..'\",'..'\"count\": \"'..tostring({v:9} or 0)..'\",'..'\"quality\": \"'..tostring({v:17} or 0)..'\",'..'\"curDurability\": \"'..tostring({v:4} or 0)..'\",'..'\"maxDurability\": \"'..tostring({v:5} or 0)..'\",'..'\"cooldownStart\": \"'..tostring({v:6} or 0)..'\",'..'\"cooldownEnd\": \"'..tostring({v:7} or 0)..'\",'..'\"name\": \"'..tostring({v:15} or 0)..'\",'..'\"lootable\": \"'..tostring({v:13} or 0)..'\",'..'\"readable\": \"'..tostring({v:12} or 0)..'\",'..'\"link\": \"'..tostring({v:3} or 0)..'\",'..'\"level\": \"'..tostring({v:18} or 0)..'\",'..'\"minLevel\": \"'..tostring({v:19} or 0)..'\",'..'\"type\": \"'..tostring({v:20} or 0)..'\",'..'\"subtype\": \"'..tostring({v:21} or 0)..'\",'..'\"maxStack\": \"'..tostring({v:22} or 0)..'\",'..'\"equiplocation\": \"'..tostring({v:23} or 0)..'\",'..'\"sellprice\": \"'..tostring({v:24} or 0)..'\",'..'\"stats\": '..\"{\"..table.concat({v:26},\",\")..\"}\"..','..'\"bagid\": \"'..tostring(a or 0)..'\",'..'\"bagslot\": \"'..tostring(b or 0)..'\"'..\"}\"{v:0}={v:0}..\",\"end end end;{v:0}={v:0}..\"]\""), out string result) ? result : string.Empty;
        }

        public string GetItemByNameOrLink(string itemName)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:1}}=\"{itemName}\";{{v:0}}='noItem';{{v:2}},{{v:3}},{{v:4}},{{v:5}},{{v:6}},{{v:7}},{{v:8}},{{v:9}},{{v:10}},{{v:11}},{{v:12}}=GetItemInfo({{v:1}});{{v:13}}=GetItemStats({{v:3}}){{v:14}}={{}}for c,d in pairs({{v:13}})do table.insert({{v:14}},string.format(\"\\\"%s\\\":\\\"%s\\\"\",c,d))end;{{v:0}}='{{'..'\"id\": \"0\",'..'\"count\": \"1\",'..'\"quality\": \"'..tostring({{v:4}} or 0)..'\",'..'\"curDurability\": \"0\",'..'\"maxDurability\": \"0\",'..'\"cooldownStart\": \"0\",'..'\"cooldownEnd\": \"0\",'..'\"name\": \"'..tostring({{v:2}} or 0)..'\",'..'\"link\": \"'..tostring({{v:3}} or 0)..'\",'..'\"level\": \"'..tostring({{v:5}} or 0)..'\",'..'\"minLevel\": \"'..tostring({{v:6}} or 0)..'\",'..'\"type\": \"'..tostring({{v:7}} or 0)..'\",'..'\"subtype\": \"'..tostring({{v:8}} or 0)..'\",'..'\"maxStack\": \"'..tostring({{v:9}} or 0)..'\",'..'\"equiplocation\": \"'..tostring({{v:10}} or 0)..'\",'..'\"sellprice\": \"'..tostring({{v:12}} or 0)..'\",'..'\"stats\": '..\"{{\"..table.concat({{v:14}},\",\")..\"}}\"..'}}';"), out string result) ? result : string.Empty;
        }

        public string GetLootRollItemLink(int rollId)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetLootRollItemLink({rollId});"), out string result) ? result : string.Empty;
        }

        public int GetMoney()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=GetMoney();"), out string s) ? int.TryParse(s, out int v) ? v : 0 : 0;
        }

        public IEnumerable<WowMount> GetMounts()
        {
            string mountJson = ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=\"[\"{{v:1}}=GetNumCompanions(\"MOUNT\")if {{v:1}}>0 then for b=1,{{v:1}} do {{v:4}},{{v:2}},{{v:3}}=GetCompanionInfo(\"mount\",b){{v:0}}={{v:0}}..\"{{\\\"name\\\":\\\"\"..{{v:2}}..\"\\\",\"..\"\\\"index\\\":\"..b..\",\"..\"\\\"spellId\\\":\"..{{v:3}}..\",\"..\"\\\"mountId\\\":\"..{{v:4}}..\",\"..\"}}\"if b<{{v:1}} then {{v:0}}={{v:0}}..\",\"end end end;{{v:0}}={{v:0}}..\"]\""), out string result) ? result : string.Empty;

            try
            {
                return JsonSerializer.Deserialize<List<WowMount>>(mountJson, new() { AllowTrailingCommas = true, NumberHandling = JsonNumberHandling.AllowReadingFromString });
            }
            catch (Exception e)
            {
                AmeisenLogger.I.Log("CharacterManager", $"Failed to parse Mounts JSON:\n{mountJson}\n{e}", LogLevel.Error);
            }

            return Array.Empty<WowMount>();
        }

        public bool GetNumQuestLogChoices(out int numChoices)
        {
            if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetNumQuestLogChoices();"), out string result)
                && int.TryParse(result, out int num))
            {
                numChoices = num;
                return true;
            }

            numChoices = 0;
            return false;
        }

        public bool GetQuestLogChoiceItemLink(int index, out string itemLink)
        {
            if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetQuestLogItemLink(\"choice\", {index});"),
                out string result)
                && result != "nil")
            {
                itemLink = result;
                return true;
            }

            itemLink = string.Empty;
            return false;
        }

        public bool GetQuestLogIdByTitle(string title, out int questLogId)
        {
            if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"for i=1,GetNumQuestLogEntries() do if GetQuestLogTitle(i) == \"{title}\" then {{v:0}}=i;break;end;end;"), out string r1)
                && int.TryParse(r1, out int foundQuestLogId))
            {
                questLogId = foundQuestLogId;
                return true;
            }

            questLogId = 0;
            return false;
        }

        public WowUnitReaction GetReaction(IntPtr a, IntPtr b)
        {
            return (WowUnitReaction)Hook.GetUnitReaction(a, b);
        }

        public Dictionary<int, int> GetRunesReady()
        {
            Dictionary<int, int> runes = new()
            {
                { 0, 0 },
                { 1, 0 },
                { 2, 0 },
                { 3, 0 }
            };

            for (int i = 0; i < 6; ++i)
            {
                if (Memory.Read(OffsetList.RuneType + (4 * i), out int type)
                    && Memory.Read(OffsetList.Runes, out byte runeStatus)
                    && ((1 << i) & runeStatus) != 0)
                {
                    ++runes[type];
                }
            }

            return runes;
        }

        public Dictionary<string, (int, int)> GetSkills()
        {
            Dictionary<string, (int, int)> parsedSkills = new();

            try
            {
                if (ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\"{v:1}=GetNumSkillLines()for a=1,{v:1} do local b,c,_,d,_,_,e=GetSkillLineInfo(a)if not c then {v:0}={v:0}..b;if a<{v:1} then {v:0}={v:0}..\":\"..tostring(d or 0)..\"/\"..tostring(e or 0)..\";\"end end end"), out string result))
                {
                    IEnumerable<string> skills = new List<string>(result.Split(';')).Select(s => s.Trim());

                    foreach (string x in skills)
                    {
                        string[] splittedParts = x.Split(":");
                        string[] maxSkill = splittedParts[1].Split("/");

                        if (int.TryParse(maxSkill[0], out int currentSkillLevel)
                            && int.TryParse(maxSkill[1], out int maxSkillLevel))
                        {
                            parsedSkills.Add(splittedParts[0], (currentSkillLevel, maxSkillLevel));
                        }
                    }
                }
            }
            catch { }

            return parsedSkills;
        }

        public int GetSpellCooldown(string spellName)
        {
            int cooldown = 0;

            if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:1}},{{v:2}},{{v:3}}=GetSpellCooldown(\"{spellName}\");{{v:0}}=({{v:1}}+{{v:2}}-GetTime())*1000;if {{v:0}}<0 then {{v:0}}=0 end;"), out string result))
            {
                if (result.Contains('.', StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Split('.')[0];
                }

                if (double.TryParse(result, out double value))
                {
                    cooldown = (int)Math.Round(value);
                }
            }

            return cooldown;
        }

        public string GetSpellNameById(int spellId)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetSpellInfo({spellId});"), out string result) ? result : string.Empty;
        }

        public string GetSpells()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}='['{v:1}=GetNumSpellTabs()for a=1,{v:1} do {v:2},{v:3},{v:4},{v:5}=GetSpellTabInfo(a)for b={v:4}+1,{v:4}+{v:5} do {v:6},{v:7}=GetSpellName(b,\"BOOKTYPE_SPELL\")if {v:6} then {v:8},{v:9},_,{v:10},_,_,{v:11},{v:12},{v:13}=GetSpellInfo({v:6},{v:7}){v:0}={v:0}..'{'..'\"spellbookName\": \"'..tostring({v:2} or 0)..'\",'..'\"spellbookId\": \"'..tostring(a or 0)..'\",'..'\"name\": \"'..tostring({v:6} or 0)..'\",'..'\"rank\": \"'..tostring({v:9} or 0)..'\",'..'\"castTime\": \"'..tostring({v:11} or 0)..'\",'..'\"minRange\": \"'..tostring({v:12} or 0)..'\",'..'\"maxRange\": \"'..tostring({v:13} or 0)..'\",'..'\"costs\": \"'..tostring({v:10} or 0)..'\"'..'}'if a<{v:1} or b<{v:4}+{v:5} then {v:0}={v:0}..','end end end end;{v:0}={v:0}..']'"), out string result) ? result : string.Empty;
        }

        public string GetTalents()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\"{v:4}=GetNumTalentTabs();for g=1,{v:4} do {v:1}=GetNumTalents(g)for h=1,{v:1} do a,b,c,d,{v:2},{v:3},e,f=GetTalentInfo(g,h){v:0}={v:0}..a..\";\"..g..\";\"..h..\";\"..{v:2}..\";\"..{v:3};if h<{v:1} then {v:0}={v:0}..\"|\"end end;if g<{v:4} then {v:0}={v:0}..\"|\"end end"), out string result) ? result : string.Empty;
        }

        public void GetTrainerServiceCost(int serviceIndex)
        {
            // todo: returns moneyCost, talentCost, professionCost
            LuaDoString($"GetTrainerServiceCost({serviceIndex})");
        }

        public void GetTrainerServiceInfo(int serviceIndex)
        {
            // todo: returns name, rank, category, expanded
            LuaDoString($"GetTrainerServiceInfo({serviceIndex})");
        }

        public int GetTrainerServicesCount()
        {
            return ExecuteLuaInt(BotUtils.ObfuscateLua("{v:0}=GetNumTrainerServices()"));
        }

        /// <summary>
        /// Check if the string is casting or channeling a spell
        /// </summary>
        /// <param name="luaunit">player, target, party1...</param>
        /// <returns>(Spellname, duration)</returns>
        public (string, int) GetUnitCastingInfo(WowLuaUnit luaunit)
        {
            string str = ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=\"none,0\";{{v:1}},x,x,x,x,{{v:2}}=UnitCastingInfo(\"{luaunit}\");{{v:3}}=(({{v:2}}/1000)-GetTime())*1000;{{v:0}}={{v:1}}..\",\"..{{v:3}};"), out string result) ? result : string.Empty;

            if (double.TryParse(str.Split(',')[1], out double timeRemaining))
            {
                return (str.Split(',')[0], (int)Math.Round(timeRemaining, 0));
            }

            return (string.Empty, 0);
        }

        public int GetUnspentTalentPoints()
        {
            return ExecuteLuaInt(BotUtils.ObfuscateLua("{v:0}=GetUnspentTalentPoints()"));
        }

        public void InteractWithObject(IntPtr objectBase)
        {
            Hook.ObjectRightClick(objectBase);
        }

        public void InteractWithUnit(IntPtr unitBase)
        {
            Hook.InteractWithUnit(unitBase);
        }

        public bool IsAutoLootEnabled()
        {
            return int.TryParse(LuaGetCVar("autoLootDefault"), out int result) && result == 1;
        }

        public bool IsClickToMoveActive()
        {
            return Memory.Read(OffsetList.ClickToMoveAction, out int ctmState)
                && ctmState != 0    // None
                && ctmState != 3    // Stop
                && ctmState != 13;  // Halted
        }

        public bool IsInLfgGroup()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:1},{v:0}=GetLFGInfoServer()"), out string result)
                && bool.TryParse(result, out bool isInLfg)
                && isInLfg;
        }

        public bool IsInLineOfSight(Vector3 start, Vector3 end, float heightAdjust = 1.5f)
        {
            start.Z += heightAdjust;
            end.Z += heightAdjust;
            return Hook.TraceLine(start, end);
        }

        public bool IsRuneReady(int runeId)
        {
            return Memory.Read(OffsetList.Runes, out byte runeStatus) && ((1 << runeId) & runeStatus) != 0;
        }

        public void LeaveBattleground()
        {
            ClickUiElement("WorldStateScoreFrameLeaveButton");
        }

        public void LootEverything()
        {
            LuaDoString(BotUtils.ObfuscateLua("{v:0}=GetNumLootItems()for a={v:0},1,-1 do LootSlot(a)ConfirmLootSlot(a)end").Item1);
        }

        public void LootMoneyAndQuestItems()
        {
            LuaDoString("for a=GetNumLootItems(),1,-1 do slotType=GetLootSlotType(a)_,_,_,_,b,c=GetLootSlotInfo(a)if not locked and(c or b==LOOT_SLOT_MONEY or b==LOOT_SLOT_CURRENCY)then LootSlot(a)end end");
        }

        public void LuaCompleteQuestAndGetReward(int questlogId, int rewardId, int gossipId)
        {
            LuaDoString($"SelectGossipActiveQuest({gossipId});CompleteQuest({questlogId});GetQuestReward({rewardId})");
        }

        public void LuaDeclinePartyInvite()
        {
            LuaDoString("StaticPopup_Hide(\"PARTY_INVITE\")");
        }

        public void LuaDeclineResurrect()
        {
            LuaDoString("DeclineResurrect()");
        }

        public bool LuaDoString(string v)
        {
            return Hook.LuaDoString(v);
        }

        public string LuaGetCVar(string CVar)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetCVar(\"{CVar}\");"), out string s) ? s : string.Empty;
        }

        public bool LuaGetGossipActiveQuestTitleById(int gossipId, out string title)
        {
            if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"local g1,_,_,_,g2,_,_,_,g3,_,_,_,g4,_,_,_,g5,_,_,_,g6 = GetGossipActiveQuests(); local gps={{g1,g2,g3,g4,g5,g6}}; {{v:0}}=gps[{gossipId}]"), out string r1))
            {
                if (r1 == "nil")
                {
                    title = string.Empty;
                    return false;
                }

                title = r1;
                return true;
            }

            title = string.Empty;
            return false;
        }

        public bool LuaGetGossipIdByActiveQuestTitle(string title, out int gossipId)
        {
            gossipId = 0;

            if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"local g1,_,_,_,g2,_,_,_,g3,_,_,_,g4,_,_,_,g5,_,_,_,g6 = GetGossipActiveQuests(); local gps={{g1,g2,g3,g4,g5,g6}}; for k,v in pairs(gps) do if v == \"{title}\" then {{v:0}}=k; break end; end;"), out string r1)
                && int.TryParse(r1, out int foundGossipId))
            {
                gossipId = foundGossipId;
                return true;
            }

            return false;
        }

        public bool LuaGetGossipIdByAvailableQuestTitle(string title, out int gossipId)
        {
            if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"local g1,_,_,_,_,g2,_,_,_,_,g3,_,_,_,_,g4,_,_,_,_,g5,_,_,_,_,g6 = GetGossipAvailableQuests(); local gps={{g1,g2,g3,g4,g5,g6}}; for k,v in pairs(gps) do if v == \"{title}\" then {{v:0}}=k; break end; end;"), out string r1)
                && int.TryParse(r1, out int foundGossipId))
            {
                gossipId = foundGossipId;
                return true;
            }

            gossipId = 0;
            return false;
        }

        public int LuaGetGossipOptionsCount()
        {
            return ExecuteLuaInt(BotUtils.ObfuscateLua("{v:0}=GetNumGossipOptions()"));
        }

        public string LuaGetItemBySlot(int itemslot)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:8}}={itemslot};{{v:0}}='noItem';{{v:1}}=GetInventoryItemID('player',{{v:8}});{{v:2}}=GetInventoryItemCount('player',{{v:8}});{{v:3}}=GetInventoryItemQuality('player',{{v:8}});{{v:4}},{{v:5}}=GetInventoryItemDurability({{v:8}});{{v:6}},{{v:7}}=GetInventoryItemCooldown('player',{{v:8}});{{v:9}},{{v:10}},{{v:11}},{{v:12}},{{v:13}},{{v:14}},{{v:15}},{{v:16}},{{v:17}},{{v:18}},{{v:19}}=GetItemInfo(GetInventoryItemLink('player',{{v:8}}));{{v:0}}='{{'..'\"id\": \"'..tostring({{v:1}} or 0)..'\",'..'\"count\": \"'..tostring({{v:2}} or 0)..'\",'..'\"quality\": \"'..tostring({{v:3}} or 0)..'\",'..'\"curDurability\": \"'..tostring({{v:4}} or 0)..'\",'..'\"maxDurability\": \"'..tostring({{v:5}} or 0)..'\",'..'\"cooldownStart\": \"'..tostring({{v:6}} or 0)..'\",'..'\"cooldownEnd\": '..tostring({{v:7}} or 0)..','..'\"name\": \"'..tostring({{v:9}} or 0)..'\",'..'\"link\": \"'..tostring({{v:10}} or 0)..'\",'..'\"level\": \"'..tostring({{v:12}} or 0)..'\",'..'\"minLevel\": \"'..tostring({{v:13}} or 0)..'\",'..'\"type\": \"'..tostring({{v:14}} or 0)..'\",'..'\"subtype\": \"'..tostring({{v:15}} or 0)..'\",'..'\"maxStack\": \"'..tostring({{v:16}} or 0)..'\",'..'\"equipslot\": \"'..tostring({{v:17}} or 0)..'\",'..'\"sellprice\": \"'..tostring({{v:19}} or 0)..'\"'..'}}';"), out string result) ? result : string.Empty;
        }

        public string LuaGetItemStats(string itemLink)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:1}}=\"{itemLink}\"{{v:0}}=''{{v:2}}={{}}{{v:3}}=GetItemStats({{v:1}},{{v:2}}){{v:0}}='{{'..'\"stamina\": \"'..tostring({{v:2}}[\"ITEM_MOD_STAMINA_SHORT\"]or 0)..'\",'..'\"agility\": \"'..tostring({{v:2}}[\"ITEM_MOD_AGILITY_SHORT\"]or 0)..'\",'..'\"strenght\": \"'..tostring({{v:2}}[\"ITEM_MOD_STRENGHT_SHORT\"]or 0)..'\",'..'\"intellect\": \"'..tostring({{v:2}}[\"ITEM_MOD_INTELLECT_SHORT\"]or 0)..'\",'..'\"spirit\": \"'..tostring({{v:2}}[\"ITEM_MOD_SPIRIT_SHORT\"]or 0)..'\",'..'\"attackpower\": \"'..tostring({{v:2}}[\"ITEM_MOD_ATTACK_POWER_SHORT\"]or 0)..'\",'..'\"spellpower\": \"'..tostring({{v:2}}[\"ITEM_MOD_SPELL_POWER_SHORT\"]or 0)..'\",'..'\"mana\": \"'..tostring({{v:2}}[\"ITEM_MOD_MANA_SHORT\"]or 0)..'\"'..'}}'"), out string result) ? result : string.Empty;
        }

        public bool LuaHasUnitStealableBuffs(string luaUnit)
        {
            return ExecuteLuaIntResult(BotUtils.ObfuscateLua($"{{v:0}}=0;local y=0;for i=1,40 do local n,_,_,_,_,_,_,_,{{v:1}}=UnitAura(\"{luaUnit}\",i);if {{v:1}}==1 then {{v:0}}=1;end end"));
        }

        public bool LuaIsBgInviteReady()
        {
            return ExecuteLuaIntResult(BotUtils.ObfuscateLua("{v:0}=0;for i=1,2 do local x=GetBattlefieldPortExpiration(i) if x>0 then {v:0}=1 end end"));
        }

        public bool LuaIsGhost(string luaUnit)
        {
            return ExecuteLuaIntResult(BotUtils.ObfuscateLua($"{{v:0}}=UnitIsGhost(\"{luaUnit}\");"));
        }

        public void LuaKickNpcsOutOfVehicle()
        {
            LuaDoString("for i=1,2 do EjectPassengerFromSeat(i) end");
        }

        public void LuaQueueBattlegroundByName(string bgName)
        {
            LuaDoString(BotUtils.ObfuscateLua($"for i=1,GetNumBattlegroundTypes() do {{v:0}}=GetBattlegroundInfo(i)if {{v:0}}==\"{bgName}\"then JoinBattlefield(i) end end").Item1);
        }

        public void LuaSellAllItems()
        {
            LuaDoString("local a,b,c=0;for d=0,4 do for e=1,GetContainerNumSlots(d)do c=GetContainerItemLink(d,e)if c then b={GetItemInfo(c)}a=a+b[11]UseContainerItem(d,e)end end end");
        }

        public void LuaSellItemsByName(string itemName)
        {
            LuaDoString($"for a=0,4,1 do for b=1,GetContainerNumSlots(a),1 do local c=GetContainerItemLink(a,b)if c and string.find(c,\"{itemName}\")then UseContainerItem(a,b)end end end");
        }

        public void LuaSendItemMailToCharacter(string itemName, string receiver)
        {
            LuaDoString($"for a=0,4 do for b=0,36 do I=GetContainerItemLink(a,b)if I and I:find(\"{itemName}\")then UseContainerItem(a,b)end end end;SendMailNameEditBox:SetText(\"{receiver}\")");
            ClickUiElement("SendMailMailButton");
        }

        public void LuaTargetUnit(string unit)
        {
            LuaDoString($"TargetUnit(\"{unit}\");");
        }

        public void QueryQuestsCompleted()
        {
            LuaDoString("QueryQuestsCompleted()");
        }

        public void RepairAllItems()
        {
            LuaDoString("RepairAllItems()");
        }

        public void RepopMe()
        {
            LuaDoString("RepopMe()");
        }

        public void RetrieveCorpse()
        {
            LuaDoString("RetrieveCorpse()");
        }

        /// <summary>
        /// Roll something on a dropped item
        /// </summary>
        /// <param name="rollId">The rolls id to roll on</param>
        /// <param name="rollType">Need, Greed or Pass</param>
        public void RollOnLoot(int rollId, WowRollType rollType)
        {
            if (rollType == WowRollType.Need)
            {
                // first we need to check whether we can roll a need on this, otherwise the bot might not roll at all
                LuaDoString($"_,_,_,_,_,canNeed=GetLootRollItemInfo({rollId});if canNeed then RollOnLoot({rollId}, {(int)rollType}) else RollOnLoot({rollId}, 2) end");
            }
            else
            {
                LuaDoString($"RollOnLoot({rollId}, {(int)rollType})");
            }
        }

        public void SelectGossipActiveQuest(int gossipId)
        {
            LuaDoString($"SelectGossipActiveQuest({gossipId})");
        }

        public void SelectGossipAvailableQuest(int gossipId)
        {
            LuaDoString($"SelectGossipAvailableQuest({gossipId})");
        }

        public void SelectGossipOption(int gossipId)
        {
            LuaDoString($"SelectGossipOption(max({gossipId}, GetNumGossipOptions()))");
        }

        public void SelectGossipOptionSimple(int gossipId)
        {
            LuaDoString($"SelectGossipOption({gossipId})");
        }

        public void SelectQuestByNameOrGossipId(string questName, int gossipId, bool isAvailableQuest)
        {
            string identifier = isAvailableQuest ? "AvailableQuestIcon" : "ActiveQuestIcon";
            string selectFunction = isAvailableQuest ? "SelectGossipAvailableQuest" : "SelectGossipActiveQuest";

            LuaDoString($"if QuestFrame ~= nil and QuestFrame:IsShown() then " +
                        $"local foundQuest=false; for i=1,20 do local f=getglobal(\"QuestTitleButton\"..i); if f then local fi=getglobal(\"QuestTitleButton\"..i..\"QuestIcon\"); if fi and fi:GetTexture() ~= nil and string.find(fi:GetTexture(), \"{identifier}\") and f:GetText() ~= nil and string.find(f:GetText(), \"{questName}\") then f:Click(); foundQuest=true; break; end; else break; end; end; " +
                        $"if not foundQuest then for i=1,20 do local f=getglobal(\"QuestTitleButton\"..i); if f then local fi=getglobal(\"QuestTitleButton\"..i..\"QuestIcon\"); if fi and fi:GetTexture() ~= nil and string.find(fi:GetTexture(), \"{identifier}\") and f:GetID() == {gossipId} then f:Click(); break; end; else break; end; end; end; " +
                        $"else " +
                        $"local foundQuest=false; local g1,_,_,_,_,g2,_,_,_,_,g3,_,_,_,_,g4,_,_,_,_,g5,_,_,_,_,g6 = GetGossipAvailableQuests(); local gps={{g1,g2,g3,g4,g5,g6}}; for k,v in pairs(gps) do if v == \"{questName}\" then {selectFunction}(k); foundQuest=true; break end; end; " +
                        $"if not foundQuest then {selectFunction}({gossipId}); end; " +
                        $"end");
        }

        public void SelectQuestLogEntry(int questLogEntry)
        {
            LuaDoString($"SelectQuestLogEntry({questLogEntry})");
        }

        public void SelectQuestReward(int id)
        {
            LuaDoString($"GetQuestReward({id})");
        }

        public void SendChatMessage(string message)
        {
            LuaDoString($"DEFAULT_CHAT_FRAME.editBox:SetText(\"{message}\") ChatEdit_SendText(DEFAULT_CHAT_FRAME.editBox, 0)");
        }

        public void SetFacing(IntPtr playerBase, float angle)
        {
            Hook.SetFacing(playerBase, angle);
        }

        public void SetLfgRole(WowRole combatClassRole)
        {
            int[] roleBools = new int[3]
            {
                combatClassRole == WowRole.Tank ? 1:0,
                combatClassRole == WowRole.Heal ? 1:0,
                combatClassRole == WowRole.Dps ? 1:0
            };

            LuaDoString($"SetLFGRoles(0, {roleBools[0]}, {roleBools[1]}, {roleBools[2]});LFDRoleCheckPopupAcceptButton:Click()");
        }

        public void SetRenderState(bool state)
        {
            Hook.SetRenderState(state);
        }

        public bool Setup()
        {
            return Hook.Hook(7, HookModules);
        }

        public void SetWorldLoadedCheck(bool enabled)
        {
            Hook.BotOverrideWorldLoadedCheck(enabled);
        }

        public void StartAutoAttack()
        {
            // UnitOnRightClick(wowUnit);
            SendChatMessage("/startattack");
        }

        public void StopCasting()
        {
            LuaDoString("SpellStopCasting()");
        }

        public void StopClickToMove()
        {
            if (IsClickToMoveActive())
            {
                Hook.CallObjectFunction(Player.BaseAddress, OffsetList.FunctionPlayerClickToMoveStop, null, false, out _);
            }
        }

        public void Tick()
        {
            if (ObjectManager.RefreshIsWorldLoaded())
            {
                ObjectManager.UpdateWowObjects();
            }

            Hook.GameInfoTick();
        }

        public bool UiIsVisible(params string[] uiElements)
        {
            StringBuilder sb = new();

            for (int i = 0; i < uiElements.Length; ++i)
            {
                sb.Append($"{uiElements[i]}:IsVisible()");

                if (i < uiElements.Length - 1)
                {
                    sb.Append($" or ");
                }
            }

            return ExecuteLuaIntResult(BotUtils.ObfuscateLua($"{{v:0}}=0 if {sb} then {{v:0}}=1 end"));
        }

        public void UseContainerItem(int bagId, int bagSlot)
        {
            LuaDoString($"UseContainerItem({bagId}, {bagSlot})");
        }

        public void UseInventoryItem(WowEquipmentSlot equipmentSlot)
        {
            LuaDoString($"UseInventoryItem({(int)equipmentSlot})");
        }

        public void UseItemByName(string itemName)
        {
            LuaSellItemsByName(itemName);
        }

        private int ExecuteLuaInt((string, string) cmdVar)
        {
            return ExecuteLuaAndRead(cmdVar, out string s)
                && int.TryParse(s, out int i)
                 ? i : 0;
        }

        private bool ExecuteLuaIntResult((string, string) cmdVar)
        {
            return ExecuteLuaAndRead(cmdVar, out string s)
                && int.TryParse(s, out int i)
                && i == 1;
        }
    }
}