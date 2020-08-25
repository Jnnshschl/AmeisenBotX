using AmeisenBotX.Core.Character.Enums;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
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
        private const int MEM_ALLOC_CHECK_SIZE = 64;
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

        public IntPtr CodecaveForCheck { get; private set; }

        public IntPtr CodecaveForExecution { get; private set; }

        public IntPtr CodecaveForGateway { get; private set; }

        public IntPtr CodeToExecuteAddress { get; private set; }

        public IntPtr EndsceneAddress { get; private set; }

        public IntPtr EndsceneReturnAddress { get; private set; }

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

        public int OldRenderFlags { get; private set; }

        public bool OverrideWorldCheck { get; private set; }

        public IntPtr OverrideWorldCheckAddress { get; private set; }

        public IntPtr ReturnValueAddress { get; private set; }

        private Dictionary<IntPtr, byte> OriginalFunctionBytes { get; }

        private WowInterface WowInterface { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AcceptBattlegroundInvite()
        {
            ClickUiElement("StaticPopup1Button1");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AcceptPartyInvite()
        {
            LuaDoString("AcceptGroup();StaticPopup_Hide(\"PARTY_INVITE\")");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AcceptQuest(int gossipId)
        {
            LuaDoString($"SelectGossipAvailableQuest({gossipId});AcceptQuest()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AcceptResurrect()
        {
            LuaDoString("AcceptResurrect();");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AcceptSummon()
        {
            LuaDoString("ConfirmSummon();StaticPopup_Hide(\"CONFIRM_SUMMON\")");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AutoAcceptQuests()
        {
            LuaDoString("active=GetNumGossipActiveQuests()if active>0 then for a=1,active do if not not select(a*5-5+4,GetGossipActiveQuests())then SelectGossipActiveQuest(a)end end end;available=GetNumGossipAvailableQuests()if available>0 then for a=1,available do if not not not select(a*6-6+3,GetGossipAvailableQuests())then SelectGossipAvailableQuest(a)end end end;if available==0 and active==0 and GetNumGossipOptions()==1 then _,type=GetGossipOptions()if type=='gossip'then SelectGossipOption(1)return end end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CancelSummon()
        {
            LuaDoString("CancelSummon()");
        }

        public int CastAndGetSpellCooldown(string spellName, bool castOnSelf = false)
        {
            int cooldown = 0;

            if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"CastSpellByName(\"{spellName}\"{(castOnSelf ? ", \"player\"" : string.Empty)});{{v:1}},{{v:2}},{{v:3}}=GetSpellCooldown(\"{spellName}\");{{v:0}}=({{v:1}}+{{v:2}}-GetTime())*1000;if {{v:0}}<0 then {{v:0}}=0 end;"), out string result))
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
        public void CastSpell(string name, bool castOnSelf = false)
        {
            LuaDoString($"CastSpellByName(\"{name}\"{(castOnSelf ? ", \"player\"" : string.Empty)})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CastSpellById(int spellId)
        {
            LuaDoString($"CastSpellByID({spellId})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearTarget()
        {
            TargetGuid(0);
        }

        public void ClickOnTerrain(Vector3 position)
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

        public void ClickToMove(WowPlayer player, Vector3 position)
        {
            if (player == null) return;

            if (WowInterface.XMemory.AllocateMemory(12, out IntPtr codeCaveVector3))
            {
                WowInterface.XMemory.Write(codeCaveVector3, position);

                CallObjectFunction(player.BaseAddress, WowInterface.OffsetList.FunctionPlayerClickToMove, new List<object>() { codeCaveVector3 });
                WowInterface.XMemory.FreeMemory(codeCaveVector3);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClickUiElement(string elementName)
        {
            LuaDoString($"{elementName}:Click()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CofirmBop()
        {
            LuaDoString($"ConfirmBindOnUse();{(!WowInterface.ObjectManager.Player.IsDead ? "StaticPopup1Button1:Click();" : "")}StaticPopup_Hide(\"AUTOEQUIP_BIND\");StaticPopup_Hide(\"EQUIP_BIND\");StaticPopup_Hide(\"USE_BIND\")");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CofirmLootRoll()
        {
            CofirmBop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CofirmReadyCheck(bool isReady)
        {
            LuaDoString($"ConfirmReadyCheck({isReady})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CompleteQuestAndGetReward(int questlogId, int rewardId, int gossipId)
        {
            LuaDoString($"SelectGossipActiveQuest(max({gossipId},GetNumGossipActiveQuests()));CompleteQuest({questlogId});GetQuestReward({rewardId})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeclinePartyInvite()
        {
            LuaDoString("StaticPopup_Hide(\"PARTY_INVITE\")");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeclineResurrect()
        {
            LuaDoString("DeclineResurrect()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dismount()
        {
            LuaDoString("DismissCompanion(\"MOUNT\")");
        }

        public void DisposeHook()
        {
            if (!IsWoWHooked) return;

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

        public void EnableClickToMove()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.ClickToMovePointer, out IntPtr ctmPointer)
                && WowInterface.XMemory.Read(IntPtr.Add(ctmPointer, (int)WowInterface.OffsetList.ClickToMoveEnabled), out int ctmEnabled)
                && ctmEnabled != 1)
            {
                WowInterface.XMemory.Write(IntPtr.Add(ctmPointer, (int)WowInterface.OffsetList.ClickToMoveEnabled), 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExecuteLuaAndRead((string, string) cmdVarTuple, out string result)
        {
            return ExecuteLuaAndRead(cmdVarTuple.Item1, cmdVarTuple.Item2, out result);
        }

        public bool ExecuteLuaAndRead(string command, string variable, out string result)
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

        public void FacePosition(WowPlayer player, Vector3 positionToFace)
        {
            if (player == null) return;
            SetFacing(player, BotMath.GetFacingAngle2D(player.Position, positionToFace));
        }

        public List<int> GetCompletedQuests()
        {
            if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=''for a,b in pairs(GetQuestsCompleted())do if b then {{v:0}}={{v:0}}..a..';'end end;"), out string result))
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
        public string GetEquipmentItems()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"[\"for a=0,23 do {v:1}=GetInventoryItemID(\"player\",a)if string.len(tostring({v:1} or\"\"))>0 then {v:2}=GetInventoryItemLink(\"player\",a){v:3}=GetInventoryItemCount(\"player\",a){v:4},{v:5}=GetInventoryItemDurability(a){v:6},{v:7}=GetInventoryItemCooldown(\"player\",a){v:8},{v:9},{v:10},{v:11},{v:12},{v:13},{v:14},{v:15},{v:16},{v:17},{v:18}=GetItemInfo({v:2}){v:19}=GetItemStats({v:2}){v:20}={}for b,c in pairs({v:19})do table.insert({v:20},string.format(\"\\\"%s\\\":\\\"%s\\\"\",b,c))end;{v:0}={v:0}..'{'..'\"id\": \"'..tostring({v:1} or 0)..'\",'..'\"count\": \"'..tostring({v:3} or 0)..'\",'..'\"quality\": \"'..tostring({v:10} or 0)..'\",'..'\"curDurability\": \"'..tostring({v:4} or 0)..'\",'..'\"maxDurability\": \"'..tostring({v:5} or 0)..'\",'..'\"cooldownStart\": \"'..tostring({v:6} or 0)..'\",'..'\"cooldownEnd\": '..tostring({v:7} or 0)..','..'\"name\": \"'..tostring({v:8} or 0)..'\",'..'\"link\": \"'..tostring({v:9} or 0)..'\",'..'\"level\": \"'..tostring({v:11} or 0)..'\",'..'\"minLevel\": \"'..tostring({v:12} or 0)..'\",'..'\"type\": \"'..tostring({v:13} or 0)..'\",'..'\"subtype\": \"'..tostring({v:14} or 0)..'\",'..'\"maxStack\": \"'..tostring({v:15} or 0)..'\",'..'\"equipslot\": \"'..tostring(a or 0)..'\",'..'\"equiplocation\": \"'..tostring({v:16} or 0)..'\",'..'\"stats\": '..\"{\"..table.concat({v:20},\",\")..\"}\"..','..'\"sellprice\": \"'..tostring({v:18} or 0)..'\"'..'}'if a<23 then {v:0}={v:0}..\",\"end end end;{v:0}={v:0}..\"]\""), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetFreeBagSlotCount()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=0 for i=1,5 do {v:0}={v:0}+GetContainerNumFreeSlots(i-1)end"), out string sresult)
                && int.TryParse(sresult, out int freeBagSlots)
                 ? freeBagSlots : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetGossipOptionCount()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=GetNumGossipOptions()"), out string sresult)
                && int.TryParse(sresult, out int gossipCount)
                 ? gossipCount : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetInventoryItems()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"[\"for a=0,4 do {v:1}=GetContainerNumSlots(a)for b=1,{v:1} do {v:2}=GetContainerItemID(a,b)if string.len(tostring({v:2} or\"\"))>0 then {v:3}=GetContainerItemLink(a,b){v:4},{v:5}=GetContainerItemDurability(a,b){v:6},{v:7}=GetContainerItemCooldown(a,b){v:8},{v:9},{v:10},{v:11},{v:12},{v:13},{v:3},{v:14}=GetContainerItemInfo(a,b){v:15},{v:16},{v:17},{v:18},{v:19},{v:20},{v:21},{v:22},{v:23},{v:8},{v:24}=GetItemInfo({v:3}){v:25}=GetItemStats({v:3}){v:26}={}if {v:25} then for c,d in pairs({v:25})do table.insert({v:26},string.format(\"\\\"%s\\\":\\\"%s\\\"\",c,d))end;end;{v:0}={v:0}..\"{\"..'\"id\": \"'..tostring({v:2} or 0)..'\",'..'\"count\": \"'..tostring({v:9} or 0)..'\",'..'\"quality\": \"'..tostring({v:17} or 0)..'\",'..'\"curDurability\": \"'..tostring({v:4} or 0)..'\",'..'\"maxDurability\": \"'..tostring({v:5} or 0)..'\",'..'\"cooldownStart\": \"'..tostring({v:6} or 0)..'\",'..'\"cooldownEnd\": \"'..tostring({v:7} or 0)..'\",'..'\"name\": \"'..tostring({v:15} or 0)..'\",'..'\"lootable\": \"'..tostring({v:13} or 0)..'\",'..'\"readable\": \"'..tostring({v:12} or 0)..'\",'..'\"link\": \"'..tostring({v:3} or 0)..'\",'..'\"level\": \"'..tostring({v:18} or 0)..'\",'..'\"minLevel\": \"'..tostring({v:19} or 0)..'\",'..'\"type\": \"'..tostring({v:20} or 0)..'\",'..'\"subtype\": \"'..tostring({v:21} or 0)..'\",'..'\"maxStack\": \"'..tostring({v:22} or 0)..'\",'..'\"equiplocation\": \"'..tostring({v:23} or 0)..'\",'..'\"sellprice\": \"'..tostring({v:24} or 0)..'\",'..'\"stats\": '..\"{\"..table.concat({v:26},\",\")..\"}\"..','..'\"bagid\": \"'..tostring(a or 0)..'\",'..'\"bagslot\": \"'..tostring(b or 0)..'\"'..\"}\"{v:0}={v:0}..\",\"end end end;{v:0}={v:0}..\"]\""), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetItemBySlot(int itemslot)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:8}}={itemslot};{{v:0}}='noItem';{{v:1}}=GetInventoryItemID('player',{{v:8}});{{v:2}}=GetInventoryItemCount('player',{{v:8}});{{v:3}}=GetInventoryItemQuality('player',{{v:8}});{{v:4}},{{v:5}}=GetInventoryItemDurability({{v:8}});{{v:6}},{{v:7}}=GetInventoryItemCooldown('player',{{v:8}});{{v:9}},{{v:10}},{{v:11}},{{v:12}},{{v:13}},{{v:14}},{{v:15}},{{v:16}},{{v:17}},{{v:18}},{{v:19}}=GetItemInfo(GetInventoryItemLink('player',{{v:8}}));{{v:0}}='{{'..'\"id\": \"'..tostring({{v:1}} or 0)..'\",'..'\"count\": \"'..tostring({{v:2}} or 0)..'\",'..'\"quality\": \"'..tostring({{v:3}} or 0)..'\",'..'\"curDurability\": \"'..tostring({{v:4}} or 0)..'\",'..'\"maxDurability\": \"'..tostring({{v:5}} or 0)..'\",'..'\"cooldownStart\": \"'..tostring({{v:6}} or 0)..'\",'..'\"cooldownEnd\": '..tostring({{v:7}} or 0)..','..'\"name\": \"'..tostring({{v:9}} or 0)..'\",'..'\"link\": \"'..tostring({{v:10}} or 0)..'\",'..'\"level\": \"'..tostring({{v:12}} or 0)..'\",'..'\"minLevel\": \"'..tostring({{v:13}} or 0)..'\",'..'\"type\": \"'..tostring({{v:14}} or 0)..'\",'..'\"subtype\": \"'..tostring({{v:15}} or 0)..'\",'..'\"maxStack\": \"'..tostring({{v:16}} or 0)..'\",'..'\"equipslot\": \"'..tostring({{v:17}} or 0)..'\",'..'\"sellprice\": \"'..tostring({{v:19}} or 0)..'\"'..'}}';"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetItemJsonByNameOrLink(string itemName)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:1}}=\"{itemName}\";{{v:0}}='noItem';{{v:2}},{{v:3}},{{v:4}},{{v:5}},{{v:6}},{{v:7}},{{v:8}},{{v:9}},{{v:10}},{{v:11}},{{v:12}}=GetItemInfo({{v:1}});{{v:13}}=GetItemStats({{v:3}}){{v:14}}={{}}for c,d in pairs({{v:13}})do table.insert({{v:14}},string.format(\"\\\"%s\\\":\\\"%s\\\"\",c,d))end;{{v:0}}='{{'..'\"id\": \"0\",'..'\"count\": \"1\",'..'\"quality\": \"'..tostring({{v:4}} or 0)..'\",'..'\"curDurability\": \"0\",'..'\"maxDurability\": \"0\",'..'\"cooldownStart\": \"0\",'..'\"cooldownEnd\": \"0\",'..'\"name\": \"'..tostring({{v:2}} or 0)..'\",'..'\"link\": \"'..tostring({{v:3}} or 0)..'\",'..'\"level\": \"'..tostring({{v:5}} or 0)..'\",'..'\"minLevel\": \"'..tostring({{v:6}} or 0)..'\",'..'\"type\": \"'..tostring({{v:7}} or 0)..'\",'..'\"subtype\": \"'..tostring({{v:8}} or 0)..'\",'..'\"maxStack\": \"'..tostring({{v:9}} or 0)..'\",'..'\"equiplocation\": \"'..tostring({{v:10}} or 0)..'\",'..'\"sellprice\": \"'..tostring({{v:12}} or 0)..'\",'..'\"stats\": '..\"{{\"..table.concat({{v:14}},\",\")..\"}}\"..'}}';"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetItemStats(string itemLink)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:1}}=\"{itemLink}\"{{v:0}}=''{{v:2}}={{}}{{v:3}}=GetItemStats({{v:1}},{{v:2}}){{v:0}}='{{'..'\"stamina\": \"'..tostring({{v:2}}[\"ITEM_MOD_STAMINA_SHORT\"]or 0)..'\",'..'\"agility\": \"'..tostring({{v:2}}[\"ITEM_MOD_AGILITY_SHORT\"]or 0)..'\",'..'\"strenght\": \"'..tostring({{v:2}}[\"ITEM_MOD_STRENGHT_SHORT\"]or 0)..'\",'..'\"intellect\": \"'..tostring({{v:2}}[\"ITEM_MOD_INTELLECT_SHORT\"]or 0)..'\",'..'\"spirit\": \"'..tostring({{v:2}}[\"ITEM_MOD_SPIRIT_SHORT\"]or 0)..'\",'..'\"attackpower\": \"'..tostring({{v:2}}[\"ITEM_MOD_ATTACK_POWER_SHORT\"]or 0)..'\",'..'\"spellpower\": \"'..tostring({{v:2}}[\"ITEM_MOD_SPELL_POWER_SHORT\"]or 0)..'\",'..'\"mana\": \"'..tostring({{v:2}}[\"ITEM_MOD_MANA_SHORT\"]or 0)..'\"'..'}}'"), out string result) ? result : string.Empty;
        }

        public bool GetLocalizedText(string variable, out string result)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetLootRollItemLink(int rollId)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetLootRollItemLink({rollId});"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetMoney()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=GetMoney();"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetMounts()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=\"[\"{{v:1}}=GetNumCompanions(\"MOUNT\")if {{v:1}}>0 then for b=1,{{v:1}} do {{v:4}},{{v:2}},{{v:3}}=GetCompanionInfo(\"mount\",b){{v:0}}={{v:0}}..\"{{\\\"name\\\":\\\"\"..{{v:2}}..\"\\\",\"..\"\\\"index\\\":\"..b..\",\"..\"\\\"spellId\\\":\"..{{v:3}}..\",\"..\"\\\"mountId\\\":\"..{{v:4}}..\",\"..\"}}\"if b<{{v:1}} then {{v:0}}={{v:0}}..\",\"end end end;{{v:0}}={{v:0}}..\"]\""), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetQuestReward(int id)
        {
            LuaDoString($"GetQuestReward({id})");
        }

        public Dictionary<RuneType, int> GetRunesReady()
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

        public Dictionary<string, (int, int)> GetSkills()
        {
            Dictionary<string, (int, int)> parsedSkills = new Dictionary<string, (int, int)>();

            try
            {
                if (ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\"{v:1}=GetNumSkillLines()for a=1,{v:1} do local b,c,_,d,_,_,e=GetSkillLineInfo(a)if not c then {v:0}={v:0}..b;if a<{v:1} then {v:0}={v:0}..\":\"..tostring(d or 0)..\"/\"..tostring(e or 0)..\";\"end end end"), out string result))
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

                AmeisenLogger.I.Log("HookManager", $"{spellName} has a cooldown of {cooldown}ms", LogLevel.Verbose);
            }

            return cooldown;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetSpellNameById(int spellId)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetSpellInfo({spellId});"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetSpells()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}='['{v:1}=GetNumSpellTabs()for a=1,{v:1} do {v:2},{v:3},{v:4},{v:5}=GetSpellTabInfo(a)for b={v:4}+1,{v:4}+{v:5} do {v:6},{v:7}=GetSpellName(b,\"BOOKTYPE_SPELL\")if {v:6} then {v:8},{v:9},_,{v:10},_,_,{v:11},{v:12},{v:13}=GetSpellInfo({v:6},{v:7}){v:0}={v:0}..'{'..'\"spellbookName\": \"'..tostring({v:2} or 0)..'\",'..'\"spellbookId\": \"'..tostring(a or 0)..'\",'..'\"name\": \"'..tostring({v:6} or 0)..'\",'..'\"rank\": \"'..tostring({v:9} or 0)..'\",'..'\"castTime\": \"'..tostring({v:11} or 0)..'\",'..'\"minRange\": \"'..tostring({v:12} or 0)..'\",'..'\"maxRange\": \"'..tostring({v:13} or 0)..'\",'..'\"costs\": \"'..tostring({v:10} or 0)..'\"'..'}'if a<{v:1} or b<{v:4}+{v:5} then {v:0}={v:0}..','end end end end;{v:0}={v:0}..']'"), out string result) ? result : string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetTalents()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\"{v:4}=GetNumTalentTabs();for g=1,{v:4} do {v:1}=GetNumTalents(g)for h=1,{v:1} do a,b,c,d,{v:2},{v:3},e,f=GetTalentInfo(g,h){v:0}={v:0}..a..\";\"..g..\";\"..h..\";\"..{v:2}..\";\"..{v:3};if h<{v:1} then {v:0}={v:0}..\"|\"end end;if g<{v:4} then {v:0}={v:0}..\"|\"end end"), out string result) ? result : string.Empty;
        }

        public void GetUnitAuras(IntPtr baseAddress, ref WowAura[] auras, out int auraCount)
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

        /// <summary>
        /// Check if the WowLuaUnit is casting or channeling a spell
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

        public WowUnitReaction GetUnitReaction(WowUnit wowUnitA, WowUnit wowUnitB)
        {
            WowUnitReaction reaction = WowUnitReaction.Unknown;

            if (wowUnitA == null || wowUnitB == null)
            {
                return reaction;
            }

            if (WowInterface.BotCache.TryGetReaction(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate, out WowUnitReaction cachedReaction))
            {
                return cachedReaction;
            }

            if (wowUnitA.Health == 0 || wowUnitB.Health == 0 || wowUnitA.Guid == 0 || wowUnitB.Guid == 0)
            {
                return reaction;
            }

            AmeisenLogger.I.Log("HookManager", $"Getting Reaction of {wowUnitA} and {wowUnitB}", LogLevel.Verbose);

            byte[] returnBytes = CallObjectFunction(wowUnitA.BaseAddress, WowInterface.OffsetList.FunctionUnitGetReaction, new List<object>() { wowUnitB.BaseAddress }, true);

            if (returnBytes.Length > 0)
            {
                reaction = (WowUnitReaction)BitConverter.ToInt32(returnBytes, 0);
                WowInterface.BotCache.CacheReaction(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate, reaction);
            }

            return reaction;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetUnspentTalentPoints()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=GetUnspentTalentPoints()"), out string sresult)
                && int.TryParse(sresult, out int talentPoints)
                 ? talentPoints : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasUnitStealableBuffs(WowLuaUnit luaUnit)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=0;local y=0;for i=1,40 do local n,_,_,_,_,_,_,_,{{v:1}}=UnitAura(\"{luaUnit}\",i);if {{v:1}}==1 then {{v:0}}=1;end end"), out string sresult)
                && int.TryParse(sresult, out int result)
                && result == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBgInviteReady()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=0;for i=1,2 do local x=GetBattlefieldPortExpiration(i) if x>0 then {v:0}=1 end end"), out string sresult)
                && int.TryParse(sresult, out int result)
                && result == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsClickToMoveActive()
        {
            return WowInterface.XMemory.Read(WowInterface.OffsetList.ClickToMoveAction, out int ctmState)
                && (ClickToMoveType)ctmState != ClickToMoveType.None
                && (ClickToMoveType)ctmState != ClickToMoveType.Stop
                && (ClickToMoveType)ctmState != ClickToMoveType.Halted;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsGhost(WowLuaUnit luaUnit)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=UnitIsGhost(\"{luaUnit}\");"), out string sresult)
                && int.TryParse(sresult, out int isGhost)
                && isGhost == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInLfgGroup()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:1},{v:0}=GetLFGInfoServer()"), out string result)
                && bool.TryParse(result, out bool isInLfg)
                && isInLfg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInLineOfSight(Vector3 start, Vector3 end, float heightAdjust = 1.5f)
        {
            start.Z += heightAdjust;
            end.Z += heightAdjust;
            return TraceLine(start, end, out _) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOutdoors()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=IsOutdoors()"), out string sresult)
                && int.TryParse(sresult, out int result)
                && result == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRuneReady(int runeId)
        {
            return WowInterface.XMemory.Read(WowInterface.OffsetList.Runes, out byte runeStatus) && ((1 << runeId) & runeStatus) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSpellKnown(int spellId, bool isPetSpell = false)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetLFGInfoServer({spellId}, {isPetSpell});"), out string sresult)
                && bool.TryParse(sresult, out bool result)
                && result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void KickNpcsOutOfVehicle()
        {
            LuaDoString("for i=1,2 do EjectPassengerFromSeat(i) end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LearnAllAvaiableSpells()
        {
            LuaDoString("LoadAddOn\"Blizzard_TrainerUI\"f=ClassTrainerTrainButton;f.e=0;if f:GetScript\"OnUpdate\"then f:SetScript(\"OnUpdate\",nil)else f:SetScript(\"OnUpdate\",function(f,a)f.e=f.e+a;if f.e>.01 then f.e=0;f:Click()end end)end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LeaveBattleground()
        {
            ClickUiElement("WorldStateScoreFrameLeaveButton");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LootEveryThing()
        {
            LuaDoString(BotUtils.ObfuscateLua("{v:0}=GetNumLootItems()for a={v:0},1,-1 do LootSlot(a)ConfirmLootSlot(a)end").Item1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LootOnlyMoneyAndQuestItems()
        {
            LuaDoString("for a=GetNumLootItems(),1,-1 do slotType=GetLootSlotType(a)_,_,_,_,b,c=GetLootSlotInfo(a)if not locked and(c or b==LOOT_SLOT_MONEY or b==LOOT_SLOT_CURRENCY)then LootSlot(a)end end");
        }

        public bool LuaDoString(string command)
        {
            AmeisenLogger.I.Log("HookManager", $"LuaDoString: {command}", LogLevel.Verbose);

            if (!string.IsNullOrWhiteSpace(command))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(command);

                if (WowInterface.XMemory.AllocateMemory((uint)bytes.Length + 1, out IntPtr memAlloc))
                {
                    WowInterface.XMemory.WriteBytes(memAlloc, bytes);
                    WowInterface.XMemory.Write<byte>(memAlloc + (bytes.Length + 1), 0);

                    if (memAlloc != IntPtr.Zero)
                    {
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
        public void Mount(int index)
        {
            LuaDoString($"CallCompanion(\"MOUNT\", {index})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OverrideWorldLoadedCheck(bool status)
        {
            OverrideWorldCheck = status;
            WowInterface.XMemory.Write(OverrideWorldCheckAddress, status ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueryQuestsCompleted()
        {
            LuaDoString("QueryQuestsCompleted()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueueBattlegroundByName(string bgName)
        {
            LuaDoString(BotUtils.ObfuscateLua($"for i=1,GetNumBattlegroundTypes() do {{v:0}}=GetBattlegroundInfo(i)if {{v:0}}==\"{bgName}\"then JoinBattlefield(i) end end").Item1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReleaseSpirit()
        {
            LuaDoString("RepopMe()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RepairAllItems()
        {
            LuaDoString("RepairAllItems()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReplaceItem(IWowItem currentItem, IWowItem newItem)
        {
            if (newItem == null) return;

            if (currentItem == null)
            {
                LuaDoString($"EquipItemByName(\"{newItem.Name}\")");
            }
            else
            {
                LuaDoString($"EquipItemByName(\"{newItem.Name}\", {(int)currentItem.EquipSlot})");
            }

            CofirmBop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RetrieveCorpse()
        {
            LuaDoString("RetrieveCorpse()");
        }

        /// <summary>
        /// Roll something on a dropped item
        /// </summary>
        /// <param name="rollId">The rolls id to roll on</param>
        /// <param name="rollType">Need, Greed or Pass</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RollOnItem(int rollId, RollType rollType)
        {
            if (rollType == RollType.Need)
            {
                LuaDoString($"_,_,_,_,_,canNeed=GetLootRollItemInfo({rollId});if canNeed then RollOnLoot({rollId}, {(int)rollType}) else RollOnLoot({rollId}, {(int)RollType.Greed}) end");
            }
            else
            {
                LuaDoString($"RollOnLoot({rollId}, {(int)rollType})");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SelectLfgRole(CombatClassRole combatClassRole)
        {
            int[] roleBools = new int[3]
            {
                combatClassRole == CombatClassRole.Tank ? 1:0,
                combatClassRole == CombatClassRole.Heal ? 1:0,
                combatClassRole == CombatClassRole.Dps ? 1:0
            };

            LuaDoString($"SetLFGRoles(0, {roleBools[0]}, {roleBools[1]}, {roleBools[2]});LFDRoleCheckPopupAcceptButton:Click()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SellAllItems()
        {
            LuaDoString("local a,b,c=0;for d=0,4 do for e=1,GetContainerNumSlots(d)do c=GetContainerItemLink(d,e)if c then b={GetItemInfo(c)}a=a+b[11]UseContainerItem(d,e)end end end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SellItemsByName(string itemName)
        {
            LuaDoString($"for a=0,4,1 do for b=1,GetContainerNumSlots(a),1 do local c=GetContainerItemLink(a,b)if c and string.find(c,\"{itemName}\")then UseContainerItem(a,b)end end end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SellItemsByQuality(ItemQuality itemQuality)
        {
            LuaDoString($"local a,b,c=0;for d=0,4 do for e=1,GetContainerNumSlots(d)do c=GetContainerItemLink(d,e)if c and string.find(c,\"{BotUtils.GetColorByQuality(itemQuality).Substring(1)}\")then b={{GetItemInfo(c)}}a=a+b[11]UseContainerItem(d,e)end end end");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendChatMessage(string message)
        {
            LuaDoString($"DEFAULT_CHAT_FRAME.editBox:SetText(\"{message}\") ChatEdit_SendText(DEFAULT_CHAT_FRAME.editBox, 0)");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendItemMailToCharacter(string itemName, string receiver)
        {
            LuaDoString($"for a=0,4 do for b=0,36 do I=GetContainerItemLink(a,b)if I and I:find(\"{itemName}\")then UseContainerItem(a,b)end end end;SendMailNameEditBox:SetText(\"{receiver}\")");
            ClickUiElement("SendMailMailButton");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFacing(WowUnit unit, float angle)
        {
            if (unit == null) return;
            CallObjectFunction(unit.BaseAddress, WowInterface.OffsetList.FunctionUnitSetFacing, new List<object>() { angle.ToString(CultureInfo.InvariantCulture).Replace(',', '.'), Environment.TickCount });
        }

        public void SetRenderState(bool renderingEnabled)
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

        public bool SetupEndsceneHook()
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

            if (!AllocateCodeCaves())
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
            WowInterface.XMemory.Fasm.AddLine("JNE @ovr");

            // check for world to be loaded
            // we dont want to execute code in
            // the loadingscreen, cause that
            // mostly results in crashes
            WowInterface.XMemory.Fasm.AddLine($"TEST DWORD [{WowInterface.OffsetList.IsWorldLoaded}], 1");
            WowInterface.XMemory.Fasm.AddLine("JE @out");
            WowInterface.XMemory.Fasm.AddLine("@ovr:");

            // execute our stuff and get return address
            WowInterface.XMemory.Fasm.AddLine($"CALL {CodecaveForExecution}");
            WowInterface.XMemory.Fasm.AddLine($"MOV [{ReturnValueAddress}], EAX");

            // finish up our execution
            WowInterface.XMemory.Fasm.AddLine("@out:");
            WowInterface.XMemory.Fasm.AddLine($"MOV DWORD [{CodeToExecuteAddress}], 0");

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
            WowInterface.XMemory.Fasm.AddLine($"NOP");                                   // 2 bytes

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

            // we should've hooked WoW now
            return IsWoWHooked;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartAutoAttack()
        {
            // UnitOnRightClick(wowUnit);
            SendChatMessage("/startattack");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopClickToMoveIfActive()
        {
            if (WowInterface.ObjectManager.Player != null && IsClickToMoveActive())
            {
                CallObjectFunction(WowInterface.ObjectManager.Player.BaseAddress, WowInterface.OffsetList.FunctionPlayerClickToMoveStop);
            }
        }

        public void TargetGuid(ulong guid)
        {
            if (guid < 0) return;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TargetLuaUnit(WowLuaUnit unit)
        {
            LuaDoString($"TargetUnit(\"{unit}\");");
        }

        public byte TraceLine(Vector3 start, Vector3 end, out Vector3 result, uint flags = 0x120171)
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
        public void UnitOnRightClick(WowUnit wowUnit)
        {
            if (wowUnit == null) return;
            CallObjectFunction(wowUnit.BaseAddress, WowInterface.OffsetList.FunctionUnitOnRightClick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnitSelectGossipOption(int gossipId)
        {
            LuaDoString($"SelectGossipOption(max({gossipId},GetNumGossipOptions()))");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UseInventoryItem(EquipmentSlot equipmentSlot)
        {
            LuaDoString($"UseInventoryItem({(int)equipmentSlot})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UseItemByBagAndSlot(int bagId, int bagSlot)
        {
            LuaDoString($"UseContainerItem({bagId}, {bagSlot})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UseItemByName(string itemName)
        {
            SellItemsByName(itemName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WowObjectOnRightClick(WowObject wowObject)
        {
            if (wowObject == null) return;
            CallObjectFunction(wowObject.BaseAddress, WowInterface.OffsetList.FunctionGameobjectOnRightClick);
        }

        private bool AllocateCodeCaves()
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

            return true;
        }

        private byte[] CallObjectFunction(IntPtr objectBaseAddress, IntPtr functionAddress, List<object> args = null, bool readReturnBytes = false)
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

        private void DisableFunction(IntPtr address)
        {
            // check wether we already replaced the function or not
            if (WowInterface.XMemory.Read(address, out byte opcode)
                && opcode != 0xC3)
            {
                SaveOriginalFunctionByte(address);
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

        private void SaveOriginalFunctionByte(IntPtr address)
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
    }
}