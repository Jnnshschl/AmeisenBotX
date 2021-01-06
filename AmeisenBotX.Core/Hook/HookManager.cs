using AmeisenBotX.Core.Character.Enums;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Hook.Structs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace AmeisenBotX.Core.Hook
{
    public class HookManager : IHookManager
    {
        private const int MEM_ALLOC_CHECK_SIZE = 128;
        private const int MEM_ALLOC_EXECUTION_SIZE = 4096;
        private const int MEM_ALLOC_GATEWAY_SIZE = 12;
        private readonly object hookLock = new object();

        private ulong hookCalls;

        private byte[] OriginalEndsceneBytes = null;

        public HookManager(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            OriginalFunctionBytes = new Dictionary<IntPtr, byte>();
        }

        public event Action<GameInfo> OnGameInfoPush;

        public IntPtr CodecaveForCheck { get; private set; }

        public IntPtr CodecaveForExecution { get; private set; }

        public IntPtr CodecaveForGateway { get; private set; }

        public IntPtr CodeToExecuteAddress { get; private set; }

        public IntPtr EndsceneAddress { get; private set; }

        public IntPtr EndsceneReturnAddress { get; private set; }

        public IntPtr GameInfoAddress { get; private set; }

        public IntPtr GameInfoExecuteAddress { get; private set; }

        public IntPtr GameInfoExecutedAddress { get; private set; }

        public IntPtr GameInfoExecuteLosCheckAddress { get; private set; }

        public IntPtr GameInfoLosCheckDataAddress { get; private set; }

        public ulong HookCallCount
        {
            get
            {
                unchecked
                {
                    ulong val = hookCalls;
                    hookCalls = 0;
                    return val;
                }
            }
        }

        public bool IsWoWHooked => WowInterface.XMemory.Read(EndsceneAddress, out byte c) && c == 0xE9;

        public DateTime LastGeneralInfoStuffPerformed { get; private set; }

        public int OldRenderFlags { get; private set; }

        public bool OverrideWorldCheck { get; private set; }

        public IntPtr OverrideWorldCheckAddress { get; private set; }

        public bool PerformGeneralInfoStuff { get; private set; }

        public IntPtr ReturnValueAddress { get; private set; }

        private Timer GameInfoTimer { get; set; }

        private Dictionary<IntPtr, byte> OriginalFunctionBytes { get; }

        private WowInterface WowInterface { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BotOverrideWorldLoadedCheck(bool status)
        {
            OverrideWorldCheck = status;
            WowInterface.XMemory.Write(OverrideWorldCheckAddress, status ? 1 : 0);
        }

        public void DisposeHook()
        {
            if (!IsWoWHooked)
            {
                return;
            }

            AmeisenLogger.I.Log("HookManager", "Disposing EnsceneHook", LogLevel.Verbose);

            lock (hookLock)
            {
                WowInterface.XMemory.SuspendMainThread();
                WowInterface.XMemory.WriteBytes(EndsceneAddress, OriginalEndsceneBytes);
                WowInterface.XMemory.ResumeMainThread();

                WowInterface.XMemory.FreeMemory(CodecaveForCheck);
                WowInterface.XMemory.FreeMemory(CodecaveForExecution);
                WowInterface.XMemory.FreeMemory(CodecaveForGateway);
                WowInterface.XMemory.FreeMemory(OverrideWorldCheckAddress);
                WowInterface.XMemory.FreeMemory(CodeToExecuteAddress);
                WowInterface.XMemory.FreeMemory(ReturnValueAddress);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaAbandonQuestsNotIn(IEnumerable<string> questNames)
        {
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetNumQuestLogEntries()"), out string r1)
                && int.TryParse(r1, out int numQuestLogEntries))
            {
                for (int i = 1; i <= numQuestLogEntries; i++)
                {
                    if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetQuestLogTitle({i})"), out string questLogTitle) && !questNames.Contains(questLogTitle))
                    {
                        LuaDoString($"SelectQuestLogEntry({i})");
                        LuaDoString($"SetAbandonQuest()");
                        LuaDoString($"AbandonQuest()");
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaAcceptBattlegroundInvite()
        {
            LuaClickUiElement("StaticPopup1Button1");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaAcceptPartyInvite()
        {
            LuaDoString("AcceptGroup();StaticPopup_Hide(\"PARTY_INVITE\")");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaAcceptQuest()
        {
            LuaDoString($"AcceptQuest()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaAcceptQuests()
        {
            LuaDoString("active=GetNumGossipActiveQuests()if active>0 then for a=1,active do if not not select(a*5-5+4,GetGossipActiveQuests())then SelectGossipActiveQuest(a)end end end;available=GetNumGossipAvailableQuests()if available>0 then for a=1,available do if not not not select(a*6-6+3,GetGossipAvailableQuests())then SelectGossipAvailableQuest(a)end end end;if available==0 and active==0 and GetNumGossipOptions()==1 then _,type=GetGossipOptions()if type=='gossip'then SelectGossipOption(1)return end end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaAcceptResurrect()
        {
            LuaDoString("AcceptResurrect();");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaAcceptSummon()
        {
            LuaDoString("ConfirmSummon();StaticPopup_Hide(\"CONFIRM_SUMMON\")");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaAutoLootEnabled()
        {
            string r = LuaGetCVar("autoLootDefault");
            int.TryParse(r, out int result);
            return result == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaCallCompanion(int index, string type = "MOUNT")
        {
            LuaDoString($"CallCompanion(\"{type}\", {index})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaCancelSummon()
        {
            LuaDoString("CancelSummon()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaCastSpell(string name, bool castOnSelf = false)
        {
            LuaDoString($"CastSpellByName(\"{name}\"{(castOnSelf ? ", \"player\"" : string.Empty)})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaCastSpellById(int spellId)
        {
            LuaDoString($"CastSpellByID({spellId})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaClickUiElement(string elementName)
        {
            LuaDoString($"{elementName}:Click()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaCofirmLootRoll()
        {
            LuaCofirmStaticPopup();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaCofirmReadyCheck(bool isReady)
        {
            LuaDoString($"ConfirmReadyCheck({isReady})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaCofirmStaticPopup()
        {
            LuaDoString($"ConfirmBindOnUse();{(!WowInterface.ObjectManager.Player.IsDead ? "StaticPopup1Button1:Click();" : "")}StaticPopup_Hide(\"AUTOEQUIP_BIND\");StaticPopup_Hide(\"EQUIP_BIND\");StaticPopup_Hide(\"USE_BIND\")");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaCompleteQuest()
        {
            LuaDoString($"CompleteQuest()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaCompleteQuestAndGetReward(int questlogId, int rewardId, int gossipId)
        {
            LuaDoString($"SelectGossipActiveQuest({gossipId});CompleteQuest({questlogId});GetQuestReward({rewardId})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaDeclinePartyInvite()
        {
            LuaDoString("StaticPopup_Hide(\"PARTY_INVITE\")");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaDeclineResurrect()
        {
            LuaDoString("DeclineResurrect()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaDeleteInventoryItemByName(string itemName)
        {
            LuaDoString($"for b=0,4 do for s=1,GetContainerNumSlots(b) do local l=GetContainerItemLink(b,s); if l and string.find(l, \"{itemName}\") then PickupContainerItem(b,s); DeleteCursorItem(); end; end; end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaDismissCompanion(string type = "MOUNT")
        {
            LuaDoString($"DismissCompanion(\"{type}\")");
        }

        public bool LuaDoString(string command)
        {
            AmeisenLogger.I.Log("HookManager", $"LuaDoString: {command}", LogLevel.Verbose);

            if (!string.IsNullOrWhiteSpace(command))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(command);

                if (WowInterface.XMemory.AllocateMemory((uint)bytes.Length + 1, out IntPtr memAlloc))
                {
                    if (memAlloc != IntPtr.Zero)
                    {
                        WowInterface.XMemory.WriteBytes(memAlloc, bytes);
                        WowInterface.XMemory.Write<byte>(memAlloc + (bytes.Length + 1), 0);

                        string[] asm = new string[]
                        {
                            "PUSH 0",
                            $"PUSH {memAlloc}",
                            $"PUSH {memAlloc}",
                            $"CALL {WowInterface.OffsetList.FunctionLuaDoString}",
                            "ADD ESP, 0xC",
                            "RETN",
                        };

                        bool status = InjectAndExecute(asm, false, out _);
                        WowInterface.XMemory.FreeMemory(memAlloc);
                        return status;
                    }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaEquipItem(IWowItem newItem, IWowItem currentItem = null)
        {
            if (newItem == null)
            {
                return;
            }

            if (currentItem == null || currentItem.EquipSlot == EquipmentSlot.NOT_EQUIPABLE)
            {
                LuaDoString($"EquipItemByName(\"{newItem.Name}\")");
            }
            else
            {
                LuaDoString($"EquipItemByName(\"{newItem.Name}\", {(int)currentItem.EquipSlot})");
            }

            LuaCofirmStaticPopup();
        }

        public List<int> LuaGetCompletedQuests()
        {
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=''for a,b in pairs(GetQuestsCompleted())do if b then {{v:0}}={{v:0}}..a..';'end end;"), out string result))
            {
                if (result != null && result.Length > 0)
                {
                    return result.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => int.TryParse(e, out int n) ? n : (int?)null)
                        .Where(e => e.HasValue)
                        .Select(e => e.Value)
                        .ToList();
                }
            }

            return new List<int>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string LuaGetEquipmentItems()
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"[\"for a=0,23 do {v:1}=GetInventoryItemID(\"player\",a)if string.len(tostring({v:1} or\"\"))>0 then {v:2}=GetInventoryItemLink(\"player\",a){v:3}=GetInventoryItemCount(\"player\",a){v:4},{v:5}=GetInventoryItemDurability(a){v:6},{v:7}=GetInventoryItemCooldown(\"player\",a){v:8},{v:9},{v:10},{v:11},{v:12},{v:13},{v:14},{v:15},{v:16},{v:17},{v:18}=GetItemInfo({v:2}){v:19}=GetItemStats({v:2}){v:20}={}for b,c in pairs({v:19})do table.insert({v:20},string.format(\"\\\"%s\\\":\\\"%s\\\"\",b,c))end;{v:0}={v:0}..'{'..'\"id\": \"'..tostring({v:1} or 0)..'\",'..'\"count\": \"'..tostring({v:3} or 0)..'\",'..'\"quality\": \"'..tostring({v:10} or 0)..'\",'..'\"curDurability\": \"'..tostring({v:4} or 0)..'\",'..'\"maxDurability\": \"'..tostring({v:5} or 0)..'\",'..'\"cooldownStart\": \"'..tostring({v:6} or 0)..'\",'..'\"cooldownEnd\": '..tostring({v:7} or 0)..','..'\"name\": \"'..tostring({v:8} or 0)..'\",'..'\"link\": \"'..tostring({v:9} or 0)..'\",'..'\"level\": \"'..tostring({v:11} or 0)..'\",'..'\"minLevel\": \"'..tostring({v:12} or 0)..'\",'..'\"type\": \"'..tostring({v:13} or 0)..'\",'..'\"subtype\": \"'..tostring({v:14} or 0)..'\",'..'\"maxStack\": \"'..tostring({v:15} or 0)..'\",'..'\"equipslot\": \"'..tostring(a or 0)..'\",'..'\"equiplocation\": \"'..tostring({v:16} or 0)..'\",'..'\"stats\": '..\"{\"..table.concat({v:20},\",\")..\"}\"..','..'\"sellprice\": \"'..tostring({v:18} or 0)..'\"'..'}'if a<23 then {v:0}={v:0}..\",\"end end end;{v:0}={v:0}..\"]\""), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LuaGetFreeBagSlotCount()
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=0 for i=1,5 do {v:0}={v:0}+GetContainerNumFreeSlots(i-1)end"), out string sresult)
                && int.TryParse(sresult, out int freeBagSlots)
                 ? freeBagSlots : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaGetGossipActiveQuestTitleById(int gossipId, out string title)
        {
            title = "";
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"local g1,_,_,_,g2,_,_,_,g3,_,_,_,g4,_,_,_,g5,_,_,_,g6 = GetGossipActiveQuests(); local gps={{g1,g2,g3,g4,g5,g6}}; {{v:0}}=gps[{gossipId}]"), out string r1))
            {
                if (r1 == "nil")
                {
                    return false;
                }

                title = r1;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaGetGossipIdByActiveQuestTitle(string title, out int gossipId)
        {
            gossipId = 0;
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"local g1,_,_,_,g2,_,_,_,g3,_,_,_,g4,_,_,_,g5,_,_,_,g6 = GetGossipActiveQuests(); local gps={{g1,g2,g3,g4,g5,g6}}; for k,v in pairs(gps) do if v == \"{title}\" then {{v:0}}=k; break end; end;"), out string r1)
                && int.TryParse(r1, out int foundGossipId))
            {
                gossipId = foundGossipId;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaGetGossipIdByAvailableQuestTitle(string title, out int gossipId)
        {
            gossipId = 0;
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"local g1,_,_,_,_,g2,_,_,_,_,g3,_,_,_,_,g4,_,_,_,_,g5,_,_,_,_,g6 = GetGossipAvailableQuests(); local gps={{g1,g2,g3,g4,g5,g6}}; for k,v in pairs(gps) do if v == \"{title}\" then {{v:0}}=k; break end; end;"), out string r1)
                && int.TryParse(r1, out int foundGossipId))
            {
                gossipId = foundGossipId;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LuaGetGossipOptionCount()
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=GetNumGossipOptions()"), out string sresult)
                && int.TryParse(sresult, out int gossipCount)
                 ? gossipCount : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] LuaGetGossipTypes()
        {
            try
            {
                WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\"function {v:1}(...)for a=1,select(\"#\",...),2 do {v:0}={v:0}..select(a+1,...)..\";\"end end;{v:1}(GetGossipOptions())"), out string result);
                return result.Split(';', StringSplitOptions.RemoveEmptyEntries);
            }
            catch { }

            return Array.Empty<string>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string LuaGetInventoryItems()
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"[\"for a=0,4 do {v:1}=GetContainerNumSlots(a)for b=1,{v:1} do {v:2}=GetContainerItemID(a,b)if string.len(tostring({v:2} or\"\"))>0 then {v:3}=GetContainerItemLink(a,b){v:4},{v:5}=GetContainerItemDurability(a,b){v:6},{v:7}=GetContainerItemCooldown(a,b){v:8},{v:9},{v:10},{v:11},{v:12},{v:13},{v:3},{v:14}=GetContainerItemInfo(a,b){v:15},{v:16},{v:17},{v:18},{v:19},{v:20},{v:21},{v:22},{v:23},{v:8},{v:24}=GetItemInfo({v:3}){v:25}=GetItemStats({v:3}){v:26}={}if {v:25} then for c,d in pairs({v:25})do table.insert({v:26},string.format(\"\\\"%s\\\":\\\"%s\\\"\",c,d))end;end;{v:0}={v:0}..\"{\"..'\"id\": \"'..tostring({v:2} or 0)..'\",'..'\"count\": \"'..tostring({v:9} or 0)..'\",'..'\"quality\": \"'..tostring({v:17} or 0)..'\",'..'\"curDurability\": \"'..tostring({v:4} or 0)..'\",'..'\"maxDurability\": \"'..tostring({v:5} or 0)..'\",'..'\"cooldownStart\": \"'..tostring({v:6} or 0)..'\",'..'\"cooldownEnd\": \"'..tostring({v:7} or 0)..'\",'..'\"name\": \"'..tostring({v:15} or 0)..'\",'..'\"lootable\": \"'..tostring({v:13} or 0)..'\",'..'\"readable\": \"'..tostring({v:12} or 0)..'\",'..'\"link\": \"'..tostring({v:3} or 0)..'\",'..'\"level\": \"'..tostring({v:18} or 0)..'\",'..'\"minLevel\": \"'..tostring({v:19} or 0)..'\",'..'\"type\": \"'..tostring({v:20} or 0)..'\",'..'\"subtype\": \"'..tostring({v:21} or 0)..'\",'..'\"maxStack\": \"'..tostring({v:22} or 0)..'\",'..'\"equiplocation\": \"'..tostring({v:23} or 0)..'\",'..'\"sellprice\": \"'..tostring({v:24} or 0)..'\",'..'\"stats\": '..\"{\"..table.concat({v:26},\",\")..\"}\"..','..'\"bagid\": \"'..tostring(a or 0)..'\",'..'\"bagslot\": \"'..tostring(b or 0)..'\"'..\"}\"{v:0}={v:0}..\",\"end end end;{v:0}={v:0}..\"]\""), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string LuaGetItemBySlot(int itemslot)
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:8}}={itemslot};{{v:0}}='noItem';{{v:1}}=GetInventoryItemID('player',{{v:8}});{{v:2}}=GetInventoryItemCount('player',{{v:8}});{{v:3}}=GetInventoryItemQuality('player',{{v:8}});{{v:4}},{{v:5}}=GetInventoryItemDurability({{v:8}});{{v:6}},{{v:7}}=GetInventoryItemCooldown('player',{{v:8}});{{v:9}},{{v:10}},{{v:11}},{{v:12}},{{v:13}},{{v:14}},{{v:15}},{{v:16}},{{v:17}},{{v:18}},{{v:19}}=GetItemInfo(GetInventoryItemLink('player',{{v:8}}));{{v:0}}='{{'..'\"id\": \"'..tostring({{v:1}} or 0)..'\",'..'\"count\": \"'..tostring({{v:2}} or 0)..'\",'..'\"quality\": \"'..tostring({{v:3}} or 0)..'\",'..'\"curDurability\": \"'..tostring({{v:4}} or 0)..'\",'..'\"maxDurability\": \"'..tostring({{v:5}} or 0)..'\",'..'\"cooldownStart\": \"'..tostring({{v:6}} or 0)..'\",'..'\"cooldownEnd\": '..tostring({{v:7}} or 0)..','..'\"name\": \"'..tostring({{v:9}} or 0)..'\",'..'\"link\": \"'..tostring({{v:10}} or 0)..'\",'..'\"level\": \"'..tostring({{v:12}} or 0)..'\",'..'\"minLevel\": \"'..tostring({{v:13}} or 0)..'\",'..'\"type\": \"'..tostring({{v:14}} or 0)..'\",'..'\"subtype\": \"'..tostring({{v:15}} or 0)..'\",'..'\"maxStack\": \"'..tostring({{v:16}} or 0)..'\",'..'\"equipslot\": \"'..tostring({{v:17}} or 0)..'\",'..'\"sellprice\": \"'..tostring({{v:19}} or 0)..'\"'..'}}';"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string LuaGetItemJsonByNameOrLink(string itemName)
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:1}}=\"{itemName}\";{{v:0}}='noItem';{{v:2}},{{v:3}},{{v:4}},{{v:5}},{{v:6}},{{v:7}},{{v:8}},{{v:9}},{{v:10}},{{v:11}},{{v:12}}=GetItemInfo({{v:1}});{{v:13}}=GetItemStats({{v:3}}){{v:14}}={{}}for c,d in pairs({{v:13}})do table.insert({{v:14}},string.format(\"\\\"%s\\\":\\\"%s\\\"\",c,d))end;{{v:0}}='{{'..'\"id\": \"0\",'..'\"count\": \"1\",'..'\"quality\": \"'..tostring({{v:4}} or 0)..'\",'..'\"curDurability\": \"0\",'..'\"maxDurability\": \"0\",'..'\"cooldownStart\": \"0\",'..'\"cooldownEnd\": \"0\",'..'\"name\": \"'..tostring({{v:2}} or 0)..'\",'..'\"link\": \"'..tostring({{v:3}} or 0)..'\",'..'\"level\": \"'..tostring({{v:5}} or 0)..'\",'..'\"minLevel\": \"'..tostring({{v:6}} or 0)..'\",'..'\"type\": \"'..tostring({{v:7}} or 0)..'\",'..'\"subtype\": \"'..tostring({{v:8}} or 0)..'\",'..'\"maxStack\": \"'..tostring({{v:9}} or 0)..'\",'..'\"equiplocation\": \"'..tostring({{v:10}} or 0)..'\",'..'\"sellprice\": \"'..tostring({{v:12}} or 0)..'\",'..'\"stats\": '..\"{{\"..table.concat({{v:14}},\",\")..\"}}\"..'}}';"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string LuaGetItemStats(string itemLink)
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:1}}=\"{itemLink}\"{{v:0}}=''{{v:2}}={{}}{{v:3}}=GetItemStats({{v:1}},{{v:2}}){{v:0}}='{{'..'\"stamina\": \"'..tostring({{v:2}}[\"ITEM_MOD_STAMINA_SHORT\"]or 0)..'\",'..'\"agility\": \"'..tostring({{v:2}}[\"ITEM_MOD_AGILITY_SHORT\"]or 0)..'\",'..'\"strenght\": \"'..tostring({{v:2}}[\"ITEM_MOD_STRENGHT_SHORT\"]or 0)..'\",'..'\"intellect\": \"'..tostring({{v:2}}[\"ITEM_MOD_INTELLECT_SHORT\"]or 0)..'\",'..'\"spirit\": \"'..tostring({{v:2}}[\"ITEM_MOD_SPIRIT_SHORT\"]or 0)..'\",'..'\"attackpower\": \"'..tostring({{v:2}}[\"ITEM_MOD_ATTACK_POWER_SHORT\"]or 0)..'\",'..'\"spellpower\": \"'..tostring({{v:2}}[\"ITEM_MOD_SPELL_POWER_SHORT\"]or 0)..'\",'..'\"mana\": \"'..tostring({{v:2}}[\"ITEM_MOD_MANA_SHORT\"]or 0)..'\"'..'}}'"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string LuaGetLootRollItemLink(int rollId)
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetLootRollItemLink({rollId});"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string LuaGetMoney()
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=GetMoney();"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string LuaGetMounts()
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=\"[\"{{v:1}}=GetNumCompanions(\"MOUNT\")if {{v:1}}>0 then for b=1,{{v:1}} do {{v:4}},{{v:2}},{{v:3}}=GetCompanionInfo(\"mount\",b){{v:0}}={{v:0}}..\"{{\\\"name\\\":\\\"\"..{{v:2}}..\"\\\",\"..\"\\\"index\\\":\"..b..\",\"..\"\\\"spellId\\\":\"..{{v:3}}..\",\"..\"\\\"mountId\\\":\"..{{v:4}}..\",\"..\"}}\"if b<{{v:1}} then {{v:0}}={{v:0}}..\",\"end end end;{{v:0}}={{v:0}}..\"]\""), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaGetNumQuestLogChoices(out int numChoices)
        {
            numChoices = 0;
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetNumQuestLogChoices();"),
                out string result) && int.TryParse(result, out int num))
            {
                numChoices = num;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaGetQuestLogChoiceItemLink(int index, out string itemLink)
        {
            itemLink = "";
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetQuestLogItemLink(\"choice\", {index});"),
                out string result))
            {
                if (result == "nil")
                {
                    return false;
                }

                itemLink = result;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaGetQuestLogIdByTitle(string title, out int questLogId)
        {
            questLogId = 0;
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"for i=1,GetNumQuestLogEntries() do if GetQuestLogTitle(i) == \"{title}\" then {{v:0}}=i; break end; end;"), out string r1)
                && int.TryParse(r1, out int foundQuestLogId))
            {
                questLogId = foundQuestLogId;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaGetQuestReward(int id)
        {
            LuaDoString($"GetQuestReward({id})");
        }

        public Dictionary<string, (int, int)> LuaGetSkills()
        {
            Dictionary<string, (int, int)> parsedSkills = new Dictionary<string, (int, int)>();

            try
            {
                if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\"{v:1}=GetNumSkillLines()for a=1,{v:1} do local b,c,_,d,_,_,e=GetSkillLineInfo(a)if not c then {v:0}={v:0}..b;if a<{v:1} then {v:0}={v:0}..\":\"..tostring(d or 0)..\"/\"..tostring(e or 0)..\";\"end end end"), out string result))
                {
                    List<string> skills = new List<string>(result.Split(';')).Select(s => s.Trim()).ToList();

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

        public int LuaGetSpellCooldown(string spellName)
        {
            int cooldown = 0;

            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:1}},{{v:2}},{{v:3}}=GetSpellCooldown(\"{spellName}\");{{v:0}}=({{v:1}}+{{v:2}}-GetTime())*1000;if {{v:0}}<0 then {{v:0}}=0 end;"), out string result))
            {
                if (result.Contains('.', StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Split('.')[0];
                }

                if (double.TryParse(result, out double value))
                {
                    cooldown = (int)Math.Round(value);
                }

                AmeisenLogger.I.Log("HookManager", $"{spellName} has a cooldown of {cooldown}ms", LogLevel.Verbose);
            }

            return cooldown;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string LuaGetSpellNameById(int spellId)
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetSpellInfo({spellId});"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string LuaGetSpells()
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}='['{v:1}=GetNumSpellTabs()for a=1,{v:1} do {v:2},{v:3},{v:4},{v:5}=GetSpellTabInfo(a)for b={v:4}+1,{v:4}+{v:5} do {v:6},{v:7}=GetSpellName(b,\"BOOKTYPE_SPELL\")if {v:6} then {v:8},{v:9},_,{v:10},_,_,{v:11},{v:12},{v:13}=GetSpellInfo({v:6},{v:7}){v:0}={v:0}..'{'..'\"spellbookName\": \"'..tostring({v:2} or 0)..'\",'..'\"spellbookId\": \"'..tostring(a or 0)..'\",'..'\"name\": \"'..tostring({v:6} or 0)..'\",'..'\"rank\": \"'..tostring({v:9} or 0)..'\",'..'\"castTime\": \"'..tostring({v:11} or 0)..'\",'..'\"minRange\": \"'..tostring({v:12} or 0)..'\",'..'\"maxRange\": \"'..tostring({v:13} or 0)..'\",'..'\"costs\": \"'..tostring({v:10} or 0)..'\"'..'}'if a<{v:1} or b<{v:4}+{v:5} then {v:0}={v:0}..','end end end end;{v:0}={v:0}..']'"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string LuaGetTalents()
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\"{v:4}=GetNumTalentTabs();for g=1,{v:4} do {v:1}=GetNumTalents(g)for h=1,{v:1} do a,b,c,d,{v:2},{v:3},e,f=GetTalentInfo(g,h){v:0}={v:0}..a..\";\"..g..\";\"..h..\";\"..{v:2}..\";\"..{v:3};if h<{v:1} then {v:0}={v:0}..\"|\"end end;if g<{v:4} then {v:0}={v:0}..\"|\"end end"), out string result) ? result : string.Empty;
        }

        /// <summary>
        /// Check if the WowLuaUnit is casting or channeling a spell
        /// </summary>
        /// <param name="luaunit">player, target, party1...</param>
        /// <returns>(Spellname, duration)</returns>
        public (string, int) LuaGetUnitCastingInfo(WowLuaUnit luaunit)
        {
            string str = WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=\"none,0\";{{v:1}},x,x,x,x,{{v:2}}=UnitCastingInfo(\"{luaunit}\");{{v:3}}=(({{v:2}}/1000)-GetTime())*1000;{{v:0}}={{v:1}}..\",\"..{{v:3}};"), out string result) ? result : string.Empty;

            if (double.TryParse(str.Split(',')[1], out double timeRemaining))
            {
                return (str.Split(',')[0], (int)Math.Round(timeRemaining, 0));
            }

            return (string.Empty, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LuaGetUnspentTalentPoints()
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=GetUnspentTalentPoints()"), out string sresult)
                && int.TryParse(sresult, out int talentPoints)
                 ? talentPoints : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaHasUnitStealableBuffs(WowLuaUnit luaUnit)
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=0;local y=0;for i=1,40 do local n,_,_,_,_,_,_,_,{{v:1}}=UnitAura(\"{luaUnit}\",i);if {{v:1}}==1 then {{v:0}}=1;end end"), out string sresult)
                && int.TryParse(sresult, out int result)
                && result == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaIsBgInviteReady()
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=0;for i=1,2 do local x=GetBattlefieldPortExpiration(i) if x>0 then {v:0}=1 end end"), out string sresult)
                && int.TryParse(sresult, out int result)
                && result == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaIsGhost(WowLuaUnit luaUnit)
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=UnitIsGhost(\"{luaUnit}\");"), out string sresult)
                && int.TryParse(sresult, out int isGhost)
                && isGhost == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaIsInLfgGroup()
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:1},{v:0}=GetLFGInfoServer()"), out string result)
                && bool.TryParse(result, out bool isInLfg)
                && isInLfg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaIsOutdoors()
        {
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=IsOutdoors()"), out string sresult)
                && int.TryParse(sresult, out int result)
                && result == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaKickNpcsOutOfVehicle()
        {
            LuaDoString("for i=1,2 do EjectPassengerFromSeat(i) end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaLearnAllAvaiableSpells()
        {
            LuaDoString("LoadAddOn\"Blizzard_TrainerUI\"f=ClassTrainerTrainButton;f.e=0;if f:GetScript\"OnUpdate\"then f:SetScript(\"OnUpdate\",nil)else f:SetScript(\"OnUpdate\",function(f,a)f.e=f.e+a;if f.e>.01 then f.e=0;f:Click()end end)end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaLeaveBattleground()
        {
            LuaClickUiElement("WorldStateScoreFrameLeaveButton");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaLootEveryThing()
        {
            LuaDoString(BotUtils.ObfuscateLua("{v:0}=GetNumLootItems()for a={v:0},1,-1 do LootSlot(a)ConfirmLootSlot(a)end").Item1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaLootMoneyAndQuestItems()
        {
            LuaDoString("for a=GetNumLootItems(),1,-1 do slotType=GetLootSlotType(a)_,_,_,_,b,c=GetLootSlotInfo(a)if not locked and(c or b==LOOT_SLOT_MONEY or b==LOOT_SLOT_CURRENCY)then LootSlot(a)end end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaQueryQuestsCompleted()
        {
            LuaDoString("QueryQuestsCompleted()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaQueueBattlegroundByName(string bgName)
        {
            LuaDoString(BotUtils.ObfuscateLua($"for i=1,GetNumBattlegroundTypes() do {{v:0}}=GetBattlegroundInfo(i)if {{v:0}}==\"{bgName}\"then JoinBattlefield(i) end end").Item1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaRepairAllItems()
        {
            LuaDoString("RepairAllItems()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaRepopMe()
        {
            LuaDoString("RepopMe()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaRetrieveCorpse()
        {
            LuaDoString("RetrieveCorpse()");
        }

        /// <summary>
        /// Roll something on a dropped item
        /// </summary>
        /// <param name="rollId">The rolls id to roll on</param>
        /// <param name="rollType">Need, Greed or Pass</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaRollOnLoot(int rollId, RollType rollType)
        {
            if (rollType == RollType.Need)
            {
                // first we need to check wether we can do a need on this, otherwise the bot might not roll at all
                LuaDoString($"_,_,_,_,_,canNeed=GetLootRollItemInfo({rollId});if canNeed then RollOnLoot({rollId}, {(int)rollType}) else RollOnLoot({rollId}, {(int)RollType.Greed}) end");
            }
            else
            {
                LuaDoString($"RollOnLoot({rollId}, {(int)rollType})");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaSelectGossipActiveQuest(int gossipId)
        {
            LuaDoString($"SelectGossipActiveQuest({gossipId})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaSelectGossipAvailableQuest(int gossipId)
        {
            LuaDoString($"SelectGossipAvailableQuest({gossipId})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaSelectGossipOption(int gossipId)
        {
            LuaDoString($"SelectGossipOption(max({gossipId},GetNumGossipOptions()))");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaSelectQuestByNameOrGossipId(string questName, int gossipId, bool isAvailableQuest)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaSelectQuestLogEntry(int questLogEntry)
        {
            LuaDoString($"SelectQuestLogEntry({questLogEntry})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaSellAllItems()
        {
            LuaDoString("local a,b,c=0;for d=0,4 do for e=1,GetContainerNumSlots(d)do c=GetContainerItemLink(d,e)if c then b={GetItemInfo(c)}a=a+b[11]UseContainerItem(d,e)end end end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaSellItemsByName(string itemName)
        {
            LuaDoString($"for a=0,4,1 do for b=1,GetContainerNumSlots(a),1 do local c=GetContainerItemLink(a,b)if c and string.find(c,\"{itemName}\")then UseContainerItem(a,b)end end end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaSellItemsByQuality(ItemQuality itemQuality)
        {
            LuaDoString($"local a,b,c=0;for d=0,4 do for e=1,GetContainerNumSlots(d)do c=GetContainerItemLink(d,e)if c and string.find(c,\"{BotUtils.GetColorByQuality(itemQuality)[1..]}\")then b={{GetItemInfo(c)}}a=a+b[11]UseContainerItem(d,e)end end end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaSendChatMessage(string message)
        {
            LuaDoString($"DEFAULT_CHAT_FRAME.editBox:SetText(\"{message}\") ChatEdit_SendText(DEFAULT_CHAT_FRAME.editBox, 0)");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaSendItemMailToCharacter(string itemName, string receiver)
        {
            LuaDoString($"for a=0,4 do for b=0,36 do I=GetContainerItemLink(a,b)if I and I:find(\"{itemName}\")then UseContainerItem(a,b)end end end;SendMailNameEditBox:SetText(\"{receiver}\")");
            LuaClickUiElement("SendMailMailButton");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaSetLfgRole(CombatClassRole combatClassRole)
        {
            int[] roleBools = new int[3]
            {
                combatClassRole == CombatClassRole.Tank ? 1:0,
                combatClassRole == CombatClassRole.Heal ? 1:0,
                combatClassRole == CombatClassRole.Dps ? 1:0
            };

            LuaDoString($"SetLFGRoles(0, {roleBools[0]}, {roleBools[1]}, {roleBools[2]});LFDRoleCheckPopupAcceptButton:Click()");
        }

        public void LuaSpellStopCasting()
        {
            LuaDoString("SpellStopCasting()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaStartAutoAttack()
        {
            // UnitOnRightClick(wowUnit);
            LuaSendChatMessage("/startattack");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaTargetUnit(WowLuaUnit unit)
        {
            LuaDoString($"TargetUnit(\"{unit}\");");
        }

        public bool LuaUiIsVisible(params string[] uiElements)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < uiElements.Length; ++i)
            {
                sb.Append($"{uiElements[i]}:IsVisible()");

                if (i < uiElements.Length - 1)
                {
                    sb.Append($" or ");
                }
            }

            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=0 if {sb} then {{v:0}}=1 end"), out string r)
                && int.TryParse(r, out int result)
                && result == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaUseContainerItem(int bagId, int bagSlot)
        {
            LuaDoString($"UseContainerItem({bagId}, {bagSlot})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaUseInventoryItem(EquipmentSlot equipmentSlot)
        {
            LuaDoString($"UseInventoryItem({(int)equipmentSlot})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaUseItemByName(string itemName)
        {
            LuaSellItemsByName(itemName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WowClearTarget()
        {
            WowTargetGuid(0);
        }

        public void WowClickOnTerrain(Vector3 position)
        {
            if (WowInterface.XMemory.AllocateMemory(20, out IntPtr codeCaveVector3))
            {
                WowInterface.XMemory.Write<ulong>(codeCaveVector3, 0);
                WowInterface.XMemory.Write(IntPtr.Add(codeCaveVector3, 0x8), position);

                string[] asm = new string[]
                {
                    $"PUSH {codeCaveVector3}",
                    $"CALL {WowInterface.OffsetList.FunctionHandleTerrainClick}",
                    "ADD ESP, 0x4",
                    "RETN",
                };

                InjectAndExecute(asm, false, out _);
                WowInterface.XMemory.FreeMemory(codeCaveVector3);
            }
        }

        public void WowClickToMove(WowPlayer player, Vector3 position)
        {
            if (player == null)
            {
                return;
            }

            if (WowInterface.XMemory.AllocateMemory(12, out IntPtr codeCaveVector3))
            {
                WowInterface.XMemory.Write(codeCaveVector3, position);

                WowCallObjectFunction(player.BaseAddress, WowInterface.OffsetList.FunctionPlayerClickToMove, new List<object>() { codeCaveVector3 });
                WowInterface.XMemory.FreeMemory(codeCaveVector3);
            }
        }

        public void WowEnableClickToMove()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.ClickToMovePointer, out IntPtr ctmPointer)
                && WowInterface.XMemory.Read(IntPtr.Add(ctmPointer, (int)WowInterface.OffsetList.ClickToMoveEnabled), out int ctmEnabled)
                && ctmEnabled != 1)
            {
                WowInterface.XMemory.Write(IntPtr.Add(ctmPointer, (int)WowInterface.OffsetList.ClickToMoveEnabled), 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WowExecuteLuaAndRead((string, string) cmdVarTuple, out string result)
        {
            return WowExecuteLuaAndRead(cmdVarTuple.Item1, cmdVarTuple.Item2, out result);
        }

        public bool WowExecuteLuaAndRead(string command, string variable, out string result)
        {
            AmeisenLogger.I.Log("HookManager", $"ExecuteLuaAndRead: command: \"{command}\" variable: \"{variable}\"", LogLevel.Verbose);

            if (!string.IsNullOrWhiteSpace(command)
                && !string.IsNullOrWhiteSpace(variable))
            {
                byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                byte[] variableBytes = Encoding.UTF8.GetBytes(variable);

                if (WowInterface.XMemory.AllocateMemory((uint)commandBytes.Length + (uint)variableBytes.Length + 2, out IntPtr memAllocCmdVar))
                {
                    int varOffset = commandBytes.Length + 1;
                    byte[] bytesToWrite = new byte[commandBytes.Length + (uint)variableBytes.Length + 2];

                    Array.Copy(commandBytes, bytesToWrite, commandBytes.Length);
                    Array.Copy(variableBytes, 0, bytesToWrite, varOffset, variableBytes.Length);

                    WowInterface.XMemory.WriteBytes(memAllocCmdVar, bytesToWrite);

                    string[] asm = new string[]
                    {
                        "PUSH 0",
                        $"PUSH {memAllocCmdVar}",
                        $"PUSH {memAllocCmdVar}",
                        $"CALL {WowInterface.OffsetList.FunctionLuaDoString}",
                        "ADD ESP, 0xC",
                        $"CALL {WowInterface.OffsetList.FunctionGetActivePlayerObject}",
                        "MOV ECX, EAX",
                        "PUSH -1",
                        $"PUSH {memAllocCmdVar + varOffset}",
                        $"CALL {WowInterface.OffsetList.FunctionGetLocalizedText}",
                        "RETN",
                    };

                    if (InjectAndExecute(asm, true, out byte[] bytes))
                    {
                        if (bytes != null)
                        {
                            result = Encoding.UTF8.GetString(bytes);

                            WowInterface.XMemory.FreeMemory(memAllocCmdVar);
                            return true;
                        }
                    }
                }
            }

            result = string.Empty;
            return false;
        }

        public void WowFacePosition(WowPlayer player, Vector3 positionToFace)
        {
            if (player == null)
            {
                return;
            }

            WowSetFacing(player, BotMath.GetFacingAngle(player.Position, positionToFace));
        }

        public bool WowGetLocalizedText(string variable, out string result)
        {
            AmeisenLogger.I.Log("HookManager", $"GetLocalizedText: {variable}", LogLevel.Verbose);

            if (!string.IsNullOrWhiteSpace(variable))
            {
                byte[] variableBytes = Encoding.UTF8.GetBytes(variable);

                if (WowInterface.XMemory.AllocateMemory((uint)variableBytes.Length + 1, out IntPtr memAlloc))
                {
                    WowInterface.XMemory.WriteBytes(memAlloc, variableBytes);
                    WowInterface.XMemory.Write<byte>(memAlloc + (variableBytes.Length + 1), 0);

                    if (memAlloc != IntPtr.Zero)
                    {
                        string[] asm = new string[]
                        {
                            $"CALL {WowInterface.OffsetList.FunctionGetActivePlayerObject}",
                            "MOV ECX, EAX",
                            "PUSH -1",
                            $"PUSH {memAlloc}",
                            $"CALL {WowInterface.OffsetList.FunctionGetLocalizedText}",
                            "RETN",
                        };

                        if (InjectAndExecute(asm, true, out byte[] bytes))
                        {
                            result = Encoding.UTF8.GetString(bytes);
                            WowInterface.XMemory.FreeMemory(memAlloc);
                            return true;
                        }
                    }
                }
            }

            result = string.Empty;
            return false;
        }

        public Dictionary<RuneType, int> WowGetRunesReady()
        {
            Dictionary<RuneType, int> runes = new Dictionary<RuneType, int>()
            {
                { RuneType.Blood, 0 },
                { RuneType.Frost, 0 },
                { RuneType.Unholy, 0 },
                { RuneType.Death, 0 }
            };

            for (int i = 0; i < 6; ++i)
            {
                if (WowInterface.XMemory.Read(WowInterface.OffsetList.RuneType + (4 * i), out RuneType type)
                    && WowInterface.XMemory.Read(WowInterface.OffsetList.Runes, out byte runeStatus)
                    && ((1 << i) & runeStatus) != 0)
                {
                    ++runes[type];
                }
            }

            return runes;
        }

        public void WowGetUnitAuras(IntPtr baseAddress, ref WowAura[] auras, out int auraCount)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(baseAddress, (int)WowInterface.OffsetList.AuraCount1), out int auraCount1))
            {
                if (auraCount1 == -1)
                {
                    if (WowInterface.XMemory.Read(IntPtr.Add(baseAddress, (int)WowInterface.OffsetList.AuraCount2), out int auraCount2)
                        && auraCount2 > 0
                        && WowInterface.XMemory.Read(IntPtr.Add(baseAddress, (int)WowInterface.OffsetList.AuraTable2), out IntPtr auraTable))
                    {
                        auraCount = auraCount2;
                        ReadAuraTable(auraTable, auraCount2, ref auras);
                    }
                    else
                    {
                        auraCount = 0;
                    }
                }
                else
                {
                    auraCount = auraCount1;
                    ReadAuraTable(IntPtr.Add(baseAddress, (int)WowInterface.OffsetList.AuraTable1), auraCount1, ref auras);
                }
            }
            else
            {
                auraCount = 0;
            }
        }

        public WowUnitReaction WowGetUnitReaction(WowUnit wowUnitA, WowUnit wowUnitB)
        {
            WowUnitReaction reaction = WowUnitReaction.Unknown;

            if (wowUnitA == null || wowUnitB == null)
            {
                return reaction;
            }

            if (WowInterface.Db.TryGetReaction(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate, out WowUnitReaction cachedReaction))
            {
                return cachedReaction;
            }

            if (wowUnitA.Health == 0 || wowUnitB.Health == 0 || wowUnitA.Guid == 0 || wowUnitB.Guid == 0)
            {
                return reaction;
            }

            AmeisenLogger.I.Log("HookManager", $"Getting Reaction of {wowUnitA} and {wowUnitB}", LogLevel.Verbose);

            byte[] returnBytes = WowCallObjectFunction(wowUnitA.BaseAddress, WowInterface.OffsetList.FunctionUnitGetReaction, new List<object>() { wowUnitB.BaseAddress }, true);

            if (returnBytes?.Length > 0)
            {
                reaction = (WowUnitReaction)BitConverter.ToInt32(returnBytes, 0);
                WowInterface.Db.CacheReaction(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate, reaction);
            }

            return reaction;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WowIsClickToMoveActive()
        {
            return WowInterface.XMemory.Read(WowInterface.OffsetList.ClickToMoveAction, out int ctmState)
                && (ClickToMoveType)ctmState != ClickToMoveType.None
                && (ClickToMoveType)ctmState != ClickToMoveType.Stop
                && (ClickToMoveType)ctmState != ClickToMoveType.Halted;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WowIsInLineOfSight(Vector3 start, Vector3 end, float heightAdjust = 1.5f)
        {
            start.Z += heightAdjust;
            end.Z += heightAdjust;
            return WowTraceLine(start, end, out _) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WowIsRuneReady(int runeId)
        {
            return WowInterface.XMemory.Read(WowInterface.OffsetList.Runes, out byte runeStatus) && ((1 << runeId) & runeStatus) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WowObjectRightClick(WowObject wowObject)
        {
            if (wowObject == null)
            {
                return;
            }

            WowCallObjectFunction(wowObject.BaseAddress, WowInterface.OffsetList.FunctionGameobjectOnRightClick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WowSetFacing(WowUnit unit, float angle)
        {
            if (unit == null)
            {
                return;
            }

            WowCallObjectFunction(unit.BaseAddress, WowInterface.OffsetList.FunctionUnitSetFacing, new List<object>() { angle.ToString(CultureInfo.InvariantCulture).Replace(',', '.'), Environment.TickCount });
        }

        public void WowSetRenderState(bool renderingEnabled)
        {
            if (renderingEnabled)
            {
                LuaDoString("WorldFrame:Show();UIParent:Show()");
            }

            WowInterface.XMemory.SuspendMainThread();

            if (renderingEnabled)
            {
                EnableFunction(WowInterface.OffsetList.FunctionWorldRender);
                EnableFunction(WowInterface.OffsetList.FunctionWorldRenderWorld);
                EnableFunction(WowInterface.OffsetList.FunctionWorldFrame);

                WowInterface.XMemory.Write(WowInterface.OffsetList.RenderFlags, OldRenderFlags);
            }
            else
            {
                if (WowInterface.XMemory.Read(WowInterface.OffsetList.RenderFlags, out int renderFlags))
                {
                    OldRenderFlags = renderFlags;
                }

                DisableFunction(WowInterface.OffsetList.FunctionWorldRender);
                DisableFunction(WowInterface.OffsetList.FunctionWorldRenderWorld);
                DisableFunction(WowInterface.OffsetList.FunctionWorldFrame);

                WowInterface.XMemory.Write(WowInterface.OffsetList.RenderFlags, 0);
            }

            WowInterface.XMemory.ResumeMainThread();

            if (!renderingEnabled)
            {
                LuaDoString("WorldFrame:Hide();UIParent:Hide()");
            }
        }

        public bool WowSetupEndsceneHook()
        {
            AmeisenLogger.I.Log("HookManager", "Setting up the EndsceneHook", LogLevel.Verbose);

            do
            {
                EndsceneAddress = GetEndScene();
                AmeisenLogger.I.Log("HookManager", $"Endscene is at: 0x{EndsceneAddress.ToInt32():X}", LogLevel.Verbose);

                if (EndsceneAddress == IntPtr.Zero)
                {
                    AmeisenLogger.I.Log("HookManager", $"Wow seems to not be started completely, retry in 500ms", LogLevel.Verbose);
                    Thread.Sleep(500);
                }
            }
            while (EndsceneAddress == IntPtr.Zero);

            // we are going to replace the first 7 bytes of EndScene
            int hookSize = 0x7;
            EndsceneReturnAddress = IntPtr.Add(EndsceneAddress, hookSize);

            if (WowInterface.XMemory.ReadBytes(EndsceneAddress, hookSize, out byte[] bytes))
            {
                OriginalEndsceneBytes = bytes;
                AmeisenLogger.I.Log("HookManager", $"EndsceneHook OriginalEndsceneBytes: {BotUtils.ByteArrayToString(OriginalEndsceneBytes)}", LogLevel.Verbose);
            }

            if (!WowAllocateCodeCaves())
            {
                return false;
            }

            WowInterface.XMemory.Fasm.Clear();

            // save registers
            // WowInterface.XMemory.Fasm.AddLine("PUSHAD");
            // WowInterface.XMemory.Fasm.AddLine("PUSHFD");

            // check for code to be executed
            WowInterface.XMemory.Fasm.AddLine($"TEST DWORD [{CodeToExecuteAddress}], 1");
            WowInterface.XMemory.Fasm.AddLine("JE @out");

            // check if we want to override our is ingame check
            // going to be used while we are in the login screen
            WowInterface.XMemory.Fasm.AddLine($"TEST DWORD [{OverrideWorldCheckAddress}], 1");
            WowInterface.XMemory.Fasm.AddLine("JNE @ovr ");

            // check for world to be loaded
            // we dont want to execute code in
            // the loadingscreen, cause that
            // mostly results in crashes
            WowInterface.XMemory.Fasm.AddLine($"TEST DWORD [{WowInterface.OffsetList.IsWorldLoaded}], 1");
            WowInterface.XMemory.Fasm.AddLine("JE @out ");
            WowInterface.XMemory.Fasm.AddLine("@ovr:");

            // execute our stuff and get return address
            WowInterface.XMemory.Fasm.AddLine($"CALL {CodecaveForExecution}");
            WowInterface.XMemory.Fasm.AddLine($"MOV [{ReturnValueAddress}], EAX");

            // finish up our execution
            WowInterface.XMemory.Fasm.AddLine("@out:");
            WowInterface.XMemory.Fasm.AddLine($"MOV DWORD [{CodeToExecuteAddress}], 0");

            // ----------------
            // # GameInfo stuff
            // ----------------
            // world loaded and should execute check
            WowInterface.XMemory.Fasm.AddLine($"TEST DWORD [{WowInterface.OffsetList.IsWorldLoaded}], 1");
            WowInterface.XMemory.Fasm.AddLine("JE @skpgi ");
            WowInterface.XMemory.Fasm.AddLine($"TEST DWORD [{GameInfoExecuteAddress}], 1");
            WowInterface.XMemory.Fasm.AddLine("JE @skpgi ");

            // isOutdoors
            WowInterface.XMemory.Fasm.AddLine($"CALL {WowInterface.OffsetList.FunctionGetActivePlayerObject}");
            WowInterface.XMemory.Fasm.AddLine($"MOV ECX, EAX");
            WowInterface.XMemory.Fasm.AddLine($"CALL {WowInterface.OffsetList.FunctionIsOutdoors}");
            WowInterface.XMemory.Fasm.AddLine($"MOV DWORD [{GameInfoAddress}], EAX");

            // isTargetInLineOfSight
            WowInterface.XMemory.Fasm.AddLine($"MOV BYTE [{GameInfoAddress + 1}], 0");

            WowInterface.XMemory.Fasm.AddLine($"TEST DWORD [{GameInfoExecuteLosCheckAddress}], 1");
            WowInterface.XMemory.Fasm.AddLine("JE @hnt ");

            IntPtr distancePointer = GameInfoLosCheckDataAddress;
            IntPtr startPointer = IntPtr.Add(distancePointer, 0x4);
            IntPtr endPointer = IntPtr.Add(startPointer, 0xC);
            IntPtr resultPointer = IntPtr.Add(endPointer, 0xC);

            WowInterface.XMemory.Fasm.AddLine("PUSH 0");
            WowInterface.XMemory.Fasm.AddLine($"PUSH {0x120171}");
            WowInterface.XMemory.Fasm.AddLine($"PUSH {distancePointer}");
            WowInterface.XMemory.Fasm.AddLine($"PUSH {resultPointer}");
            WowInterface.XMemory.Fasm.AddLine($"PUSH {endPointer}");
            WowInterface.XMemory.Fasm.AddLine($"PUSH {startPointer}");
            WowInterface.XMemory.Fasm.AddLine($"CALL {WowInterface.OffsetList.FunctionTraceline}");
            WowInterface.XMemory.Fasm.AddLine("ADD ESP, 0x18");

            WowInterface.XMemory.Fasm.AddLine($"XOR AL, 1");
            WowInterface.XMemory.Fasm.AddLine($"MOV BYTE [{GameInfoAddress + 1}], AL");

            WowInterface.XMemory.Fasm.AddLine($"MOV DWORD [{GameInfoExecuteLosCheckAddress}], 0");
            WowInterface.XMemory.Fasm.AddLine("@hnt:");

            WowInterface.XMemory.Fasm.AddLine($"MOV DWORD [{GameInfoExecutedAddress}], 1");
            WowInterface.XMemory.Fasm.AddLine("@skpgi:");
            WowInterface.XMemory.Fasm.AddLine($"MOV DWORD [{GameInfoExecuteAddress}], 0");
            // ----------------

            // call the gateway function
            WowInterface.XMemory.Fasm.AddLine($"JMP {CodecaveForGateway}");

            // restore registers
            // WowInterface.XMemory.Fasm.AddLine("POPFD");
            // WowInterface.XMemory.Fasm.AddLine("POPAD");

            // inject the instructions into our codecave
            WowInterface.XMemory.Fasm.Inject((uint)CodecaveForCheck);

            // ---------------------------------------------------
            // End of the code that checks if there is asm to be
            // executed on our hook
            // ---------------------------------------------------

            // original EndScene in the gateway
            WowInterface.XMemory.Fasm.Clear();
            WowInterface.XMemory.WriteBytes(CodecaveForGateway, OriginalEndsceneBytes);

            // return to original function after we're done with our stuff
            WowInterface.XMemory.Fasm.AddLine($"JMP {EndsceneReturnAddress}"); // 5 bytes

            // note if you increase the hookSize we need to add more NOP's
            WowInterface.XMemory.Fasm.AddLine($"NOP"); // 2 bytes

            WowInterface.XMemory.Fasm.Inject((uint)CodecaveForGateway + (uint)OriginalEndsceneBytes.Length);
            WowInterface.XMemory.Fasm.Clear();

            // ---------------------------------------------------
            // End of doing the original stuff and returning to
            // the original instruction
            // ---------------------------------------------------

            // modify original EndScene instructions to start the hook
            WowInterface.XMemory.Fasm.AddLine($"JMP {CodecaveForCheck}");
            WowInterface.XMemory.Fasm.Inject((uint)EndsceneAddress);

            AmeisenLogger.I.Log("HookManager", "EndsceneHook Successful", LogLevel.Verbose);

            GameInfoTimer = new Timer(GameInfoTimerTick, null, 0, 100);

            // we should've hooked WoW now
            return IsWoWHooked;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WowStopClickToMove()
        {
            if (WowInterface.ObjectManager.Player != null && WowIsClickToMoveActive())
            {
                WowCallObjectFunction(WowInterface.ObjectManager.Player.BaseAddress, WowInterface.OffsetList.FunctionPlayerClickToMoveStop);
            }
        }

        public void WowTargetGuid(ulong guid)
        {
            if (guid < 0)
            {
                return;
            }

            byte[] guidBytes = BitConverter.GetBytes(guid);
            string[] asm = new string[]
            {
                $"PUSH {BitConverter.ToUInt32(guidBytes, 4)}",
                $"PUSH {BitConverter.ToUInt32(guidBytes, 0)}",
                $"CALL {WowInterface.OffsetList.FunctionSetTarget}",
                "ADD ESP, 0x8",
                "RETN"
            };

            InjectAndExecute(asm, false, out _);
            WowInterface.ObjectManager.UpdateWowObjects();
        }

        public byte WowTraceLine(Vector3 start, Vector3 end, out Vector3 result, uint flags = 0x120171)
        {
            result = Vector3.Zero;

            if (WowInterface.XMemory.AllocateMemory(40, out IntPtr tracelineCodecave))
            {
                (float, Vector3, Vector3) tracelineCombo = (1f, start, end);

                IntPtr distancePointer = tracelineCodecave;
                IntPtr startPointer = IntPtr.Add(distancePointer, 0x4);
                IntPtr endPointer = IntPtr.Add(startPointer, 0xC);
                IntPtr resultPointer = IntPtr.Add(endPointer, 0xC);

                if (WowInterface.XMemory.Write(distancePointer, tracelineCombo))
                {
                    string[] asm = new string[]
                    {
                        "PUSH 0",
                        $"PUSH {flags}",
                        $"PUSH {distancePointer}",
                        $"PUSH {resultPointer}",
                        $"PUSH {endPointer}",
                        $"PUSH {startPointer}",
                        $"CALL {WowInterface.OffsetList.FunctionTraceline}",
                        "ADD ESP, 0x18",
                        "RETN",
                    };

                    if (InjectAndExecute(asm, true, out byte[] bytes))
                    {
                        WowInterface.XMemory.FreeMemory(tracelineCodecave);

                        if (bytes != null && bytes.Length > 0)
                        {
                            return bytes[0];
                        }
                    }
                }
            }

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WowUnitRightClick(WowUnit wowUnit)
        {
            if (wowUnit == null)
            {
                return;
            }

            WowCallObjectFunction(wowUnit.BaseAddress, WowInterface.OffsetList.FunctionUnitOnRightClick);
        }

        private void DisableFunction(IntPtr address)
        {
            // check wether we already replaced the function or not
            if (WowInterface.XMemory.Read(address, out byte opcode)
                && opcode != 0xC3)
            {
                SaveOriginalFunctionBytes(address);
                WowInterface.XMemory.PatchMemory<byte>(address, 0xC3);
            }
        }

        private void EnableFunction(IntPtr address)
        {
            // check for RET opcode to be present before restoring original function
            if (OriginalFunctionBytes.ContainsKey(address)
                && WowInterface.XMemory.Read(address, out byte opcode)
                && opcode == 0xC3)
            {
                WowInterface.XMemory.PatchMemory(address, OriginalFunctionBytes[address]);
            }
        }

        private void GameInfoTimerTick(object state)
        {
            if (WowInterface.XMemory.Read(GameInfoExecuteAddress, out int executeStatus)
                && executeStatus == 1)
            {
                // still waiting for execution
                return;
            }

            if (WowInterface.XMemory.Read(GameInfoExecutedAddress, out int executedStatus)
                && executedStatus == 0)
            {
                if (WowInterface.ObjectManager.TargetGuid != 0 && WowInterface.ObjectManager.Target != null)
                {
                    Vector3 playerPosition = WowInterface.Player.Position;
                    playerPosition.Z += 1.5f;

                    Vector3 targetPosition = WowInterface.Target.Position;
                    targetPosition.Z += 1.5f;

                    if (WowInterface.XMemory.Write(GameInfoLosCheckDataAddress, (1.0f, playerPosition, targetPosition)))
                    {
                        // run the los check if we have a target
                        WowInterface.XMemory.Write(GameInfoExecuteLosCheckAddress, 1);
                    }
                }

                // run the gameinfo update
                WowInterface.XMemory.Write(GameInfoExecuteAddress, 1);
            }
            else
            {
                // process the info
                if (WowInterface.XMemory.Read(GameInfoAddress, out GameInfo gameInfo))
                {
                    OnGameInfoPush?.Invoke(gameInfo);
                    AmeisenLogger.I.Log("GameInfo", $"Pushing GameInfo Update: {JsonConvert.SerializeObject(gameInfo)}");
                }

                WowInterface.XMemory.Write(GameInfoExecutedAddress, 0);
            }
        }

        private IntPtr GetEndScene()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.EndSceneStaticDevice, out IntPtr pDevice)
                && WowInterface.XMemory.Read(IntPtr.Add(pDevice, (int)WowInterface.OffsetList.EndSceneOffsetDevice), out IntPtr pEnd)
                && WowInterface.XMemory.Read(pEnd, out IntPtr pScene)
                && WowInterface.XMemory.Read(IntPtr.Add(pScene, (int)WowInterface.OffsetList.EndSceneOffset), out IntPtr pEndscene))
            {
                return pEndscene;
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        private bool InjectAndExecute(string[] asm, bool readReturnBytes, out byte[] bytes)
        {
            WowInterface.ObjectManager.RefreshIsWorldLoaded();

            if (!IsWoWHooked || WowInterface.XMemory.Process.HasExited || (!WowInterface.ObjectManager.IsWorldLoaded && !OverrideWorldCheck))
            {
                bytes = null;
                return false;
            }

            List<byte> returnBytes = readReturnBytes ? new List<byte>() : null;

            lock (hookLock)
            {
                // zero our memory
                if (WowInterface.XMemory.ZeroMemory(CodecaveForExecution, MEM_ALLOC_EXECUTION_SIZE))
                {
                    bool frozenMainThread = false;

                    try
                    {
                        // preparing to inject the given ASM
                        WowInterface.XMemory.Fasm.Clear();

                        // add all lines
                        for (int i = 0; i < asm.Length; ++i)
                        {
                            WowInterface.XMemory.Fasm.AddLine(asm[i]);
                        }

                        // inject it
                        WowInterface.XMemory.SuspendMainThread();
                        frozenMainThread = true;
                        WowInterface.XMemory.Fasm.Inject((uint)CodecaveForExecution);

                        // now there is code to be executed
                        WowInterface.XMemory.Write(CodeToExecuteAddress, 1);
                        WowInterface.XMemory.ResumeMainThread();
                        frozenMainThread = false;

                        // wait for the code to be executed
                        while (WowInterface.XMemory.Read(CodeToExecuteAddress, out int codeToBeExecuted)
                               && codeToBeExecuted > 0)
                        {
                            Thread.Sleep(1);
                        }

                        // if we want to read the return value do it otherwise we're done
                        if (readReturnBytes)
                        {
                            WowInterface.XMemory.Read(ReturnValueAddress, out uint dwAddress);
                            IntPtr addrPointer = new IntPtr(dwAddress);

                            // read all parameter-bytes until we the buffer is 0
                            WowInterface.XMemory.Read(addrPointer, out byte buffer);

                            if (buffer != 0)
                            {
                                do
                                {
                                    returnBytes.Add(buffer);
                                    addrPointer += 1;
                                } while (WowInterface.XMemory.Read(addrPointer, out buffer) && buffer != 0);
                            }
                            else
                            {
                                returnBytes.AddRange(BitConverter.GetBytes(dwAddress));
                            }
                        }
                    }
                    catch
                    {
                        // now there is no more code to be executed
                        WowInterface.XMemory.Write(CodeToExecuteAddress, 0);

                        bytes = null;
                        return false;
                    }
                    finally
                    {
                        if (frozenMainThread)
                        {
                            WowInterface.XMemory.ResumeMainThread();
                        }
                    }
                }
            }

            ++hookCalls;
            bytes = readReturnBytes ? returnBytes.ToArray() : null;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string LuaGetCVar(string CVar)
        {
            WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetCVar(\"{CVar}\");"), out string result);
            return result;
        }

        private void ReadAuraTable(IntPtr buffBase, int auraCount, ref WowAura[] auras)
        {
            if (auraCount > 40)
            {
                return;
            }

            for (int i = 0; i < auraCount; ++i)
            {
                WowInterface.XMemory.Read(buffBase + (0x18 * i), out auras[i]);
            }
        }

        private void SaveOriginalFunctionBytes(IntPtr address)
        {
            if (WowInterface.XMemory.Read(address, out byte opcode))
            {
                if (!OriginalFunctionBytes.ContainsKey(address))
                {
                    OriginalFunctionBytes.Add(address, opcode);
                }
                else
                {
                    OriginalFunctionBytes[address] = opcode;
                }
            }
        }

        private unsafe bool WowAllocateCodeCaves()
        {
            AmeisenLogger.I.Log("HookManager", "Allocating Codecaves for the EndsceneHook", LogLevel.Verbose);

            // integer to check if there is code waiting to be executed
            if (!WowInterface.XMemory.AllocateMemory(4, out IntPtr codeToExecuteAddress)) { return false; }

            CodeToExecuteAddress = codeToExecuteAddress;
            WowInterface.XMemory.Write(CodeToExecuteAddress, 0);
            AmeisenLogger.I.Log("HookManager", $"EndsceneHook CodeToExecuteAddress: 0x{CodeToExecuteAddress.ToInt32():X}", LogLevel.Verbose);

            // integer to save the pointer to the return value
            if (!WowInterface.XMemory.AllocateMemory(4, out IntPtr returnValueAddress)) { return false; }

            ReturnValueAddress = returnValueAddress;
            WowInterface.XMemory.Write(ReturnValueAddress, 0);
            AmeisenLogger.I.Log("HookManager", $"EndsceneHook ReturnValueAddress: 0x{ReturnValueAddress.ToInt32():X}", LogLevel.Verbose);

            // codecave to override the is ingame check, used at the login
            if (!WowInterface.XMemory.AllocateMemory(4, out IntPtr overrideWorldCheckAddress)) { return false; }

            OverrideWorldCheckAddress = overrideWorldCheckAddress;
            WowInterface.XMemory.Write(OverrideWorldCheckAddress, 0);
            AmeisenLogger.I.Log("HookManager", $"EndsceneHook OverrideWorldCheckAddress: 0x{OverrideWorldCheckAddress.ToInt32():X}", LogLevel.Verbose);

            // codecave for the original endscene code
            if (!WowInterface.XMemory.AllocateMemory(MEM_ALLOC_GATEWAY_SIZE, out IntPtr codecaveForGateway)) { return false; }

            CodecaveForGateway = codecaveForGateway;
            AmeisenLogger.I.Log("HookManager", $"EndsceneHook CodecaveForGateway ({MEM_ALLOC_GATEWAY_SIZE} bytes): 0x{CodecaveForGateway.ToInt32():X}", LogLevel.Verbose);

            // codecave to check wether we need to execute something
            if (!WowInterface.XMemory.AllocateMemory(MEM_ALLOC_CHECK_SIZE, out IntPtr codecaveForCheck)) { return false; }

            CodecaveForCheck = codecaveForCheck;
            AmeisenLogger.I.Log("HookManager", $"EndsceneHook CodecaveForCheck ({MEM_ALLOC_CHECK_SIZE} bytes): 0x{CodecaveForCheck.ToInt32():X}", LogLevel.Verbose);

            // codecave for the code we wan't to execute
            if (!WowInterface.XMemory.AllocateMemory(MEM_ALLOC_EXECUTION_SIZE, out IntPtr codecaveForExecution)) { return false; }

            CodecaveForExecution = codecaveForExecution;
            AmeisenLogger.I.Log("HookManager", $"EndsceneHook CodecaveForExecution ({MEM_ALLOC_EXECUTION_SIZE} bytes): 0x{CodecaveForExecution.ToInt32():X}", LogLevel.Verbose);

            // codecave for the gameinfo execution
            if (!WowInterface.XMemory.AllocateMemory(4, out IntPtr gameInfoExecute)) { return false; }

            GameInfoExecuteAddress = gameInfoExecute;
            WowInterface.XMemory.Write(GameInfoExecuteAddress, 0);
            AmeisenLogger.I.Log("HookManager", $"EndsceneHook GameInfoExecuteAddress (4 bytes): 0x{GameInfoExecuteAddress.ToInt32():X}", LogLevel.Verbose);

            // codecave for the gameinfo executed
            if (!WowInterface.XMemory.AllocateMemory(4, out IntPtr gameInfoExecuted)) { return false; }

            GameInfoExecutedAddress = gameInfoExecuted;
            WowInterface.XMemory.Write(GameInfoExecutedAddress, 0);
            AmeisenLogger.I.Log("HookManager", $"EndsceneHook GameInfoExecutedAddress (4 bytes): 0x{GameInfoExecutedAddress.ToInt32():X}", LogLevel.Verbose);

            // codecave for the gameinfo struct
            uint gameinfoSize = (uint)sizeof(GameInfo);

            if (!WowInterface.XMemory.AllocateMemory(gameinfoSize, out IntPtr gameInfo)) { return false; }

            GameInfoAddress = gameInfo;
            AmeisenLogger.I.Log("HookManager", $"EndsceneHook GameInfoAddress ({gameinfoSize} bytes): 0x{GameInfoAddress.ToInt32():X}", LogLevel.Verbose);

            // codecave for the gameinfo line of sight check
            if (!WowInterface.XMemory.AllocateMemory(4, out IntPtr executeLosCheck)) { return false; }

            GameInfoExecuteLosCheckAddress = executeLosCheck;
            WowInterface.XMemory.Write(GameInfoExecuteLosCheckAddress, 0);
            AmeisenLogger.I.Log("HookManager", $"EndsceneHook GameInfoExecuteLosCheckAddress (4 bytes): 0x{GameInfoExecuteLosCheckAddress.ToInt32():X}", LogLevel.Verbose);

            // codecave for the gameinfo line of sight check data
            if (!WowInterface.XMemory.AllocateMemory(40, out IntPtr losCheckData)) { return false; }

            GameInfoLosCheckDataAddress = losCheckData;
            AmeisenLogger.I.Log("HookManager", $"EndsceneHook GameInfoLosCheckDataAddress (40 bytes): 0x{GameInfoLosCheckDataAddress.ToInt32():X}", LogLevel.Verbose);
            return true;
        }

        private byte[] WowCallObjectFunction(IntPtr objectBaseAddress, IntPtr functionAddress, List<object> args = null, bool readReturnBytes = false)
        {
            List<string> asm = new List<string> { $"MOV ECX, {objectBaseAddress}" };

            if (args != null)
            {
                // push all parameters
                for (int i = 0; i < args.Count; ++i)
                {
                    asm.Add($"PUSH {args[i]}");
                }
            }

            asm.Add($"CALL {functionAddress}");
            asm.Add("RETN");

            return InjectAndExecute(asm.ToArray(), readReturnBytes, out byte[] bytes) ? bytes : null;
        }
    }
}