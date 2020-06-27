using AmeisenBotX.Core.Character.Enums;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private ulong endsceneCalls;

        public HookManager(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            OriginalFunctionBytes = new Dictionary<IntPtr, byte>();
        }

        public ulong CallCount
        {
            get
            {
                unchecked
                {
                    ulong val = endsceneCalls;
                    endsceneCalls = 0;
                    return val;
                }
            }
        }

        public IntPtr CodecaveForCheck { get; private set; }

        public IntPtr CodecaveForExecution { get; private set; }

        public IntPtr CodecaveForGateway { get; private set; }

        public IntPtr CodeToExecuteAddress { get; private set; }

        public IntPtr EndsceneAddress { get; private set; }

        public IntPtr EndsceneReturnAddress { get; private set; }

        public bool IsWoWHooked => WowInterface.XMemory.Read(EndsceneAddress, out byte c) ? c == 0xE9 : false;

        public byte[] OriginalEndsceneBytes { get; private set; }

        public bool OverrideWorldCheck { get; private set; }

        public IntPtr OverrideWorldCheckAddress { get; private set; }

        public IntPtr ReturnValueAddress { get; private set; }

        private Dictionary<IntPtr, byte> OriginalFunctionBytes { get; }

        private WowInterface WowInterface { get; }

        public void AcceptBattlegroundInvite()
        {
            ClickUiElement("StaticPopup1Button1");
        }

        public void AcceptPartyInvite()
        {
            ClickUiElement("StaticPopup1Button1");
            // LuaDoString("AcceptGroup();");
        }

        public void AcceptQuest(int gossipId)
        {
            LuaDoString($"SelectGossipAvailableQuest({gossipId});AcceptQuest();");
        }

        public void AcceptResurrect()
        {
            ClickUiElement("StaticPopup1Button1");
            // LuaDoString("AcceptResurrect();");
        }

        public void AcceptSummon()
        {
            ClickUiElement("StaticPopup1Button1");
            // LuaDoString("ConfirmSummon();");
        }

        public void CastSpell(string name, bool castOnSelf = false)
        {
            AmeisenLogger.Instance.Log("HookManager", $"Casting spell with name: {name}", LogLevel.Verbose);
            LuaDoString($"CastSpellByName(\"{name}\"{(castOnSelf ? ", \"player\"" : string.Empty)});");
        }

        public void CastSpellById(int spellId)
        {
            AmeisenLogger.Instance.Log("HookManager", $"Casting spell with id: {spellId}", LogLevel.Verbose);

            if (spellId > 0)
            {
                LuaDoString($"CastSpellByID({spellId});");
            }
        }

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
                    $"PUSH {codeCaveVector3.ToInt32()}",
                    $"CALL {WowInterface.OffsetList.FunctionHandleTerrainClick}",
                    "ADD ESP, 0x4",
                    "RETN",
                };

                InjectAndExecute(asm, false);
                WowInterface.XMemory.FreeMemory(codeCaveVector3);
            }
        }

        public void ClickToMove(WowPlayer player, Vector3 position)
        {
            if (WowInterface.XMemory.AllocateMemory(12, out IntPtr codeCaveVector3))
            {
                WowInterface.XMemory.Write(codeCaveVector3, position);

                CallObjectFunction(player.BaseAddress, WowInterface.OffsetList.FunctionPlayerClickToMove, new List<object>() { codeCaveVector3.ToInt32() });
                WowInterface.XMemory.FreeMemory(codeCaveVector3);
            }
        }

        public void ClickUiElement(string elementName)
        {
            LuaDoString($"{elementName}:Click()");
        }

        public void CofirmBop()
        {
            LuaDoString("ConfirmBindOnUse();");
            ClickUiElement("StaticPopup1Button1");
        }

        public void CofirmReadyCheck(bool isReady)
        {
            LuaDoString($"ConfirmReadyCheck({isReady});");
        }

        public void CompleteQuestAndGetReward(int questlogId, int rewardId, int gossipId)
        {
            LuaDoString($"SelectGossipActiveQuest(max({gossipId}, GetNumGossipActiveQuests()));CompleteQuest({questlogId});GetQuestReward({rewardId});");
        }

        public void DisposeHook()
        {
            if (IsWoWHooked)
            {
                AmeisenLogger.Instance.Log("HookManager", "Disposing EnsceneHook", LogLevel.Verbose);

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
                && WowInterface.XMemory.Read(IntPtr.Add(ctmPointer, WowInterface.OffsetList.ClickToMoveEnabled.ToInt32()), out int ctmEnabled)
                && ctmEnabled != 1)
            {
                WowInterface.XMemory.Write(IntPtr.Add(ctmPointer, WowInterface.OffsetList.ClickToMoveEnabled.ToInt32()), 1);
            }
        }

        public string ExecuteLuaAndRead((string, string) cmdVarTuple)
        {
            return ExecuteLuaAndRead(cmdVarTuple.Item1, cmdVarTuple.Item2);
        }

        public string ExecuteLuaAndRead(string command, string variable)
        {
            AmeisenLogger.Instance.Log("HookManager", $"ExecuteLuaAndRead: command: \"{command}\" variable: \"{variable}\"", LogLevel.Verbose);

            if (command.Length > 0
                && variable.Length > 0)
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
                        $"PUSH {memAllocCmdVar.ToInt32()}",
                        $"PUSH {memAllocCmdVar.ToInt32()}",
                        $"CALL {WowInterface.OffsetList.FunctionLuaDoString.ToInt32()}",
                        "ADD ESP, 0xC",
                        $"CALL {WowInterface.OffsetList.FunctionGetActivePlayerObject.ToInt32()}",
                        "MOV ECX, EAX",
                        "PUSH -1",
                        $"PUSH {memAllocCmdVar.ToInt32() + varOffset}",
                        $"CALL {WowInterface.OffsetList.FunctionGetLocalizedText.ToInt32()}",
                        "RETN",
                    };

                    string result = Encoding.UTF8.GetString(InjectAndExecute(asm, true));
                    WowInterface.XMemory.FreeMemory(memAllocCmdVar);

                    return result;
                }
            }

            return string.Empty;
        }

        public void FacePosition(WowPlayer player, Vector3 positionToFace)
        {
            SetFacing(player, BotMath.GetFacingAngle2D(player.Position, positionToFace));
        }

        [Obsolete]
        public List<string> GetAuras(WowLuaUnit luaunit)
        {
            return ReadAuras(luaunit, "UnitAura");
        }

        [Obsolete]
        public List<string> GetBuffs(WowLuaUnit luaunit)
        {
            return ReadAuras(luaunit, "UnitBuff");
        }

        public List<int> GetCompletedQuests()
        {
            string result = ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=''for a,b in pairs(GetQuestsCompleted())do if b then {{v:0}}={{v:0}}..a..';'end end;"));

            if (result != null && result.Length > 0)
            {
                return result.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => int.TryParse(e, out int n) ? n : (int?)null)
                    .Where(e => e.HasValue)
                    .Select(e => e.Value)
                    .ToList();
            }

            return new List<int>();
        }

        [Obsolete]
        public List<string> GetDebuffs(WowLuaUnit luaunit)
        {
            return ReadAuras(luaunit, "UnitDebuff");
        }

        public string GetEquipmentItems()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"[\"for a=0,23 do {v:1}=GetInventoryItemID(\"player\",a)if string.len(tostring({v:1} or\"\"))>0 then {v:2}=GetInventoryItemLink(\"player\",a){v:3}=GetInventoryItemCount(\"player\",a){v:4},{v:5}=GetInventoryItemDurability(a){v:6},{v:7}=GetInventoryItemCooldown(\"player\",a){v:8},{v:9},{v:10},{v:11},{v:12},{v:13},{v:14},{v:15},{v:16},{v:17},{v:18}=GetItemInfo({v:2}){v:19}=GetItemStats({v:2}){v:20}={}for b,c in pairs({v:19})do table.insert({v:20},string.format(\"\\\"%s\\\":\\\"%s\\\"\",b,c))end;{v:0}={v:0}..'{'..'\"id\": \"'..tostring({v:1} or 0)..'\",'..'\"count\": \"'..tostring({v:3} or 0)..'\",'..'\"quality\": \"'..tostring({v:10} or 0)..'\",'..'\"curDurability\": \"'..tostring({v:4} or 0)..'\",'..'\"maxDurability\": \"'..tostring({v:5} or 0)..'\",'..'\"cooldownStart\": \"'..tostring({v:6} or 0)..'\",'..'\"cooldownEnd\": '..tostring({v:7} or 0)..','..'\"name\": \"'..tostring({v:8} or 0)..'\",'..'\"link\": \"'..tostring({v:9} or 0)..'\",'..'\"level\": \"'..tostring({v:11} or 0)..'\",'..'\"minLevel\": \"'..tostring({v:12} or 0)..'\",'..'\"type\": \"'..tostring({v:13} or 0)..'\",'..'\"subtype\": \"'..tostring({v:14} or 0)..'\",'..'\"maxStack\": \"'..tostring({v:15} or 0)..'\",'..'\"equipslot\": \"'..tostring(a or 0)..'\",'..'\"equiplocation\": \"'..tostring({v:16} or 0)..'\",'..'\"stats\": '..\"{\"..table.concat({v:20},\",\")..\"}\"..','..'\"sellprice\": \"'..tostring({v:18} or 0)..'\"'..'}'if a<23 then {v:0}={v:0}..\",\"end end end;{v:0}={v:0}..\"]\""));
        }

        public int GetFreeBagSlotCount()
        {
            return int.TryParse(ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=0 for i=1,5 do {v:0}={v:0}+GetContainerNumFreeSlots(i-1)end")), out int bagSlots) ? bagSlots : 0;
        }

        public string GetInventoryItems()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"[\"for a=0,4 do {v:1}=GetContainerNumSlots(a)for b=1,{v:1} do {v:2}=GetContainerItemID(a,b)if string.len(tostring({v:2} or\"\"))>0 then {v:3}=GetContainerItemLink(a,b){v:4},{v:5}=GetContainerItemDurability(a,b){v:6},{v:7}=GetContainerItemCooldown(a,b){v:8},{v:9},{v:10},{v:11},{v:12},{v:13},{v:3},{v:14}=GetContainerItemInfo(a,b){v:15},{v:16},{v:17},{v:18},{v:19},{v:20},{v:21},{v:22},{v:23},{v:8},{v:24}=GetItemInfo({v:3}){v:25}=GetItemStats({v:3}){v:26}={}for c,d in pairs({v:25})do table.insert({v:26},string.format(\"\\\"%s\\\":\\\"%s\\\"\",c,d))end;{v:0}={v:0}..\"{\"..'\"id\": \"'..tostring({v:2} or 0)..'\",'..'\"count\": \"'..tostring({v:9} or 0)..'\",'..'\"quality\": \"'..tostring({v:17} or 0)..'\",'..'\"curDurability\": \"'..tostring({v:4} or 0)..'\",'..'\"maxDurability\": \"'..tostring({v:5} or 0)..'\",'..'\"cooldownStart\": \"'..tostring({v:6} or 0)..'\",'..'\"cooldownEnd\": \"'..tostring({v:7} or 0)..'\",'..'\"name\": \"'..tostring({v:15} or 0)..'\",'..'\"lootable\": \"'..tostring({v:13} or 0)..'\",'..'\"readable\": \"'..tostring({v:12} or 0)..'\",'..'\"link\": \"'..tostring({v:3} or 0)..'\",'..'\"level\": \"'..tostring({v:18} or 0)..'\",'..'\"minLevel\": \"'..tostring({v:19} or 0)..'\",'..'\"type\": \"'..tostring({v:20} or 0)..'\",'..'\"subtype\": \"'..tostring({v:21} or 0)..'\",'..'\"maxStack\": \"'..tostring({v:22} or 0)..'\",'..'\"equiplocation\": \"'..tostring({v:23} or 0)..'\",'..'\"sellprice\": \"'..tostring({v:24} or 0)..'\",'..'\"stats\": '..\"{\"..table.concat({v:26},\",\")..\"}\"..','..'\"bagid\": \"'..tostring(a or 0)..'\",'..'\"bagslot\": \"'..tostring(b or 0)..'\"'..\"}\"{v:0}={v:0}..\",\"end end end;{v:0}={v:0}..\"]\""));
        }

        public string GetItemBySlot(int itemslot)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:8}}={itemslot};{{v:0}}='noItem';{{v:1}}=GetInventoryItemID('player',{{v:8}});{{v:2}}=GetInventoryItemCount('player',{{v:8}});{{v:3}}=GetInventoryItemQuality('player',{{v:8}});{{v:4}},{{v:5}}=GetInventoryItemDurability({{v:8}});{{v:6}},{{v:7}}=GetInventoryItemCooldown('player',{{v:8}});{{v:9}},{{v:10}},{{v:11}},{{v:12}},{{v:13}},{{v:14}},{{v:15}},{{v:16}},{{v:17}},{{v:18}},{{v:19}}=GetItemInfo(GetInventoryItemLink('player',{{v:8}}));{{v:0}}='{{'..'\"id\": \"'..tostring({{v:1}} or 0)..'\",'..'\"count\": \"'..tostring({{v:2}} or 0)..'\",'..'\"quality\": \"'..tostring({{v:3}} or 0)..'\",'..'\"curDurability\": \"'..tostring({{v:4}} or 0)..'\",'..'\"maxDurability\": \"'..tostring({{v:5}} or 0)..'\",'..'\"cooldownStart\": \"'..tostring({{v:6}} or 0)..'\",'..'\"cooldownEnd\": '..tostring({{v:7}} or 0)..','..'\"name\": \"'..tostring({{v:9}} or 0)..'\",'..'\"link\": \"'..tostring({{v:10}} or 0)..'\",'..'\"level\": \"'..tostring({{v:12}} or 0)..'\",'..'\"minLevel\": \"'..tostring({{v:13}} or 0)..'\",'..'\"type\": \"'..tostring({{v:14}} or 0)..'\",'..'\"subtype\": \"'..tostring({{v:15}} or 0)..'\",'..'\"maxStack\": \"'..tostring({{v:16}} or 0)..'\",'..'\"equipslot\": \"'..tostring({{v:17}} or 0)..'\",'..'\"sellprice\": \"'..tostring({{v:19}} or 0)..'\"'..'}}';"));
        }

        public string GetItemJsonByNameOrLink(string itemName)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:1}}=\"{itemName}\";{{v:0}}='noItem';{{v:2}},{{v:3}},{{v:4}},{{v:5}},{{v:6}},{{v:7}},{{v:8}},{{v:9}},{{v:10}},{{v:11}},{{v:12}}=GetItemInfo({{v:1}});{{v:13}}=GetItemStats({{v:3}}){{v:14}}={{}}for c,d in pairs({{v:13}})do table.insert({{v:14}},string.format(\"\\\"%s\\\":\\\"%s\\\"\",c,d))end;{{v:0}}='{{'..'\"id\": \"0\",'..'\"count\": \"1\",'..'\"quality\": \"'..tostring({{v:4}} or 0)..'\",'..'\"curDurability\": \"0\",'..'\"maxDurability\": \"0\",'..'\"cooldownStart\": \"0\",'..'\"cooldownEnd\": \"0\",'..'\"name\": \"'..tostring({{v:2}} or 0)..'\",'..'\"link\": \"'..tostring({{v:3}} or 0)..'\",'..'\"level\": \"'..tostring({{v:5}} or 0)..'\",'..'\"minLevel\": \"'..tostring({{v:6}} or 0)..'\",'..'\"type\": \"'..tostring({{v:7}} or 0)..'\",'..'\"subtype\": \"'..tostring({{v:8}} or 0)..'\",'..'\"maxStack\": \"'..tostring({{v:9}} or 0)..'\",'..'\"equiplocation\": \"'..tostring({{v:10}} or 0)..'\",'..'\"sellprice\": \"'..tostring({{v:12}} or 0)..'\",'..'\"stats\": '..\"{{\"..table.concat({{v:14}},\",\")..\"}}\"..'}}';"));
        }

        public string GetItemStats(string itemLink)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:1}}=\"{itemLink}\"{{v:0}}=''{{v:2}}={{}}{{v:3}}=GetItemStats({{v:1}},{{v:2}}){{v:0}}='{{'..'\"stamina\": \"'..tostring({{v:2}}[\"ITEM_MOD_STAMINA_SHORT\"]or 0)..'\",'..'\"agility\": \"'..tostring({{v:2}}[\"ITEM_MOD_AGILITY_SHORT\"]or 0)..'\",'..'\"strenght\": \"'..tostring({{v:2}}[\"ITEM_MOD_STRENGHT_SHORT\"]or 0)..'\",'..'\"intellect\": \"'..tostring({{v:2}}[\"ITEM_MOD_INTELLECT_SHORT\"]or 0)..'\",'..'\"spirit\": \"'..tostring({{v:2}}[\"ITEM_MOD_SPIRIT_SHORT\"]or 0)..'\",'..'\"attackpower\": \"'..tostring({{v:2}}[\"ITEM_MOD_ATTACK_POWER_SHORT\"]or 0)..'\",'..'\"spellpower\": \"'..tostring({{v:2}}[\"ITEM_MOD_SPELL_POWER_SHORT\"]or 0)..'\",'..'\"mana\": \"'..tostring({{v:2}}[\"ITEM_MOD_MANA_SHORT\"]or 0)..'\"'..'}}'"));
        }

        public string GetLocalizedText(string variable)
        {
            AmeisenLogger.Instance.Log("HookManager", $"GetLocalizedText: {variable}", LogLevel.Verbose);

            if (variable.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(variable);
                if (WowInterface.XMemory.AllocateMemory((uint)bytes.Length + 1, out IntPtr memAlloc))
                {
                    WowInterface.XMemory.WriteBytes(memAlloc, bytes);
                    WowInterface.XMemory.Write<byte>(memAlloc + (bytes.Length + 1), 0);

                    if (memAlloc == IntPtr.Zero)
                    {
                        return string.Empty;
                    }

                    string[] asm = new string[]
                    {
                        $"CALL {WowInterface.OffsetList.FunctionGetActivePlayerObject.ToInt32()}",
                        "MOV ECX, EAX",
                        "PUSH -1",
                        $"PUSH {memAlloc.ToInt32()}",
                        $"CALL {WowInterface.OffsetList.FunctionGetLocalizedText.ToInt32()}",
                        "RETN",
                    };

                    string result = Encoding.UTF8.GetString(InjectAndExecute(asm, true));
                    WowInterface.XMemory.FreeMemory(memAlloc);
                    return result;
                }
            }

            return string.Empty;
        }

        public string GetLootRollItemLink(int rollId)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetLootRollItemLink({rollId});"));
        }

        public string GetMoney()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=GetMoney();"));
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

        public List<string> GetSkills()
        {
            try
            {
                return new List<string>(ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\"{v:1}=GetNumSkillLines()for a=1,{v:1} do local b,c=GetSkillLineInfo(a)if not c then {v:0}={v:0}..b;if a<{v:1} then {v:0}={v:0}..\"; \"end end end")).Split(';')).Select(s => s.Trim()).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public double GetSpellCooldown(string spellName)
        {
            string result = ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:1}},{{v:2}},{{v:3}} = GetSpellCooldown(\"{spellName}\");{{v:0}}=({{v:1}}+{{v:2}}-GetTime())*1000;if {{v:0}} < 0 then {{v:0}} = 0 end;"));
            double cooldown = 0;

            if (result.Contains('.'))
            {
                result = result.Split('.')[0];
            }

            if (double.TryParse(result, out double value))
            {
                cooldown = Math.Round(value);
            }

            AmeisenLogger.Instance.Log("HookManager", $"{spellName} has a cooldown of {cooldown}ms", LogLevel.Verbose);
            return cooldown;
        }

        public string GetSpellNameById(int spellId)
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetSpellInfo({spellId});"));
        }

        public string GetSpells()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}='['{v:1}=GetNumSpellTabs()for a=1,{v:1} do {v:2},{v:3},{v:4},{v:5}=GetSpellTabInfo(a)for b={v:4}+1,{v:4}+{v:5} do {v:6},{v:7}=GetSpellName(b,\"BOOKTYPE_SPELL\")if {v:6} then {v:8},{v:9},_,{v:10},_,_,{v:11},{v:12},{v:13}=GetSpellInfo({v:6},{v:7}){v:0}={v:0}..'{'..'\"spellbookName\": \"'..tostring({v:2} or 0)..'\",'..'\"spellbookId\": \"'..tostring(a or 0)..'\",'..'\"name\": \"'..tostring({v:6} or 0)..'\",'..'\"rank\": \"'..tostring({v:9} or 0)..'\",'..'\"castTime\": \"'..tostring({v:11} or 0)..'\",'..'\"minRange\": \"'..tostring({v:12} or 0)..'\",'..'\"maxRange\": \"'..tostring({v:13} or 0)..'\",'..'\"costs\": \"'..tostring({v:10} or 0)..'\"'..'}'if a<{v:1} or b<{v:4}+{v:5} then {v:0}={v:0}..','end end end end;{v:0}={v:0}..']'"));
        }

        public string GetTalents()
        {
            return ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\"{v:4}=GetNumTalentTabs();for g=1,{v:4} do {v:1}=GetNumTalents(g)for h=1,{v:1} do a,b,c,d,{v:2},{v:3},e,f=GetTalentInfo(g,h){v:0}={v:0}..a..\";\"..g..\";\"..h..\";\"..{v:2}..\";\"..{v:3};if h<{v:1} then {v:0}={v:0}..\"|\"end end;if g<{v:4} then {v:0}={v:0}..\"|\"end end"));
        }

        public List<WowAura> GetUnitAuras(WowUnit wowUnit)
        {
            List<WowAura> buffs = new List<WowAura>();

            if (WowInterface.XMemory.Read(IntPtr.Add(wowUnit.BaseAddress, WowInterface.OffsetList.AuraCount1.ToInt32()), out int auraCount1))
            {
                if (auraCount1 == -1)
                {
                    if (WowInterface.XMemory.Read(IntPtr.Add(wowUnit.BaseAddress, WowInterface.OffsetList.AuraCount2.ToInt32()), out int auraCount2))
                    {
                        if (auraCount2 > 0 && WowInterface.XMemory.Read(IntPtr.Add(wowUnit.BaseAddress, WowInterface.OffsetList.AuraTable2.ToInt32()), out IntPtr auraTable))
                        {
                            buffs.AddRange(ReadAuraTable<RawWowAuraTable40>(auraTable, auraCount2));
                        }
                    }
                }
                else
                {
                    buffs.AddRange(ReadAuraTable<RawWowAuraTable16>(IntPtr.Add(wowUnit.BaseAddress, WowInterface.OffsetList.AuraTable1.ToInt32()), auraCount1));
                }
            }

            return buffs;
        }

        /// <summary>
        /// Check if the WowLuaUnit is casting or channeling a spell
        /// </summary>
        /// <param name="luaunit">player, target, party1...</param>
        /// <returns>(Spellname, duration)</returns>
        public (string, int) GetUnitCastingInfo(WowLuaUnit luaunit)
        {
            string str = ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=\"none,0\";{{v:1}},x,x,x,x,{{v:2}}=UnitCastingInfo(\"{luaunit}\");{{v:3}}=(({{v:2}}/1000)-GetTime())*1000;{{v:0}}={{v:1}}..\",\"..{{v:3}};"));

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

            WowInterface.ObjectManager.UpdateObject(wowUnitA);
            WowInterface.ObjectManager.UpdateObject(wowUnitB);

            if (wowUnitA.Health == 0 || wowUnitB.Health == 0 || wowUnitA.Guid == 0 || wowUnitB.Guid == 0)
            {
                return reaction;
            }

            AmeisenLogger.Instance.Log("HookManager", $"Getting Reaction of {wowUnitA} and {wowUnitB}", LogLevel.Verbose);

            byte[] returnBytes = CallObjectFunction(wowUnitA.BaseAddress, WowInterface.OffsetList.FunctionUnitGetReaction, new List<object>() { wowUnitB.BaseAddress }, true);

            if (returnBytes.Length > 0)
            {
                reaction = (WowUnitReaction)BitConverter.ToInt32(returnBytes, 0);
                WowInterface.BotCache.CacheReaction(wowUnitA.FactionTemplate, wowUnitB.FactionTemplate, reaction);
            }

            return reaction;
        }

        public int GetUnspentTalentPoints()
        {
            return int.TryParse(ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=GetUnspentTalentPoints()")), out int talentPoints) ? talentPoints : 0;
        }

        public bool HasUnitStealableBuffs(WowLuaUnit luaUnit)
        {
            return int.TryParse(ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=0;local y=0;for i=1,40 do local n,_,_,_,_,_,_,_,{{v:1}}=UnitAura(\"{luaUnit}\",i);if {{v:1}}==1 then {{v:0}}=1;end end")), out int result) ? result == 1 : false;
        }

        public bool IsBgInviteReady()
        {
            return int.TryParse(ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=0;for i=1,2 do local x=GetBattlefieldPortExpiration(i) if x>0 then {v:0}=1 end end")), out int result) ? result == 1 : false;
        }

        public bool IsClickToMoveActive(WowPlayer player)
        {
            return WowInterface.XMemory.Read(WowInterface.OffsetList.ClickToMoveAction, out int ctmState)
                       && (ClickToMoveType)ctmState != ClickToMoveType.None
                       && (ClickToMoveType)ctmState != ClickToMoveType.Stop;
        }

        public bool IsGhost(WowLuaUnit luaUnit)
        {
            return int.TryParse(ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=UnitIsGhost(\"{luaUnit}\");")), out int isGhost) && isGhost == 1;
        }

        public bool IsInLfgGroup()
        {
            string lfgMode = ExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:1},{v:0}=GetLFGInfoServer()"));
            return bool.TryParse(lfgMode, out bool isInLfg) && isInLfg;
        }

        public bool IsInLineOfSight(Vector3 start, Vector3 end, float heightAdjust = 1.5f)
        {
            start.Z += heightAdjust;
            end.Z += heightAdjust;
            return TraceLine(start, end, out _) == 0;
        }

        public bool IsRuneReady(int runeId)
        {
            return WowInterface.XMemory.Read(WowInterface.OffsetList.Runes, out byte runeStatus) && ((1 << runeId) & runeStatus) != 0;
        }

        public bool IsSpellKnown(int spellId, bool isPetSpell = false)
        {
            return bool.TryParse(ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetLFGInfoServer({spellId}, {isPetSpell});")), out bool result) ? result : false;
        }

        public void KickNpcsOutOfVehicle()
        {
            LuaDoString("for i=1,2 do EjectPassengerFromSeat(i) end");
        }

        public void LearnAllAvaiableSpells()
        {
            LuaDoString("LoadAddOn\"Blizzard_TrainerUI\" f=ClassTrainerTrainButton f.e=0 if f:GetScript\"OnUpdate\" then f:SetScript(\"OnUpdate\", nil)else f:SetScript(\"OnUpdate\", function(f,e) f.e=f.e+e if f.e>.01 then f.e=0 f:Click() end end)end");
        }

        public void LeaveBattleground()
        {
            ClickUiElement("WorldStateScoreFrameLeaveButton");
        }

        public void LootEveryThing()
        {
            LuaDoString(BotUtils.ObfuscateLua("{v:0}=GetNumLootItems();for i={v:0},1,-1 do LootSlot(i); ConfirmLootSlot(i); end").Item1);
        }

        public void LuaDoString(string command)
        {
            AmeisenLogger.Instance.Log("HookManager", $"LuaDoString: {command}", LogLevel.Verbose);

            if (command.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(command);
                if (WowInterface.XMemory.AllocateMemory((uint)bytes.Length + 1, out IntPtr memAlloc))
                {
                    WowInterface.XMemory.WriteBytes(memAlloc, bytes);
                    WowInterface.XMemory.Write<byte>(memAlloc + (bytes.Length + 1), 0);

                    if (memAlloc == IntPtr.Zero)
                    {
                        return;
                    }

                    string[] asm = new string[]
                    {
                        "PUSH 0",
                        $"PUSH {memAlloc.ToInt32()}",
                        $"PUSH {memAlloc.ToInt32()}",
                        $"CALL {WowInterface.OffsetList.FunctionLuaDoString.ToInt32()}",
                        "ADD ESP, 0xC",
                        "RETN",
                    };

                    InjectAndExecute(asm, false);
                    WowInterface.XMemory.FreeMemory(memAlloc);
                }
            }
        }

        public void OverrideWorldCheckOff()
        {
            OverrideWorldCheck = false;
            WowInterface.XMemory.Write(OverrideWorldCheckAddress, 0);
        }

        public void OverrideWorldCheckOn()
        {
            OverrideWorldCheck = true;
            WowInterface.XMemory.Write(OverrideWorldCheckAddress, 1);
        }

        public void QueryQuestsCompleted()
        {
            LuaDoString("QueryQuestsCompleted();");
        }

        public void QueueBattlegroundByName(string bgName)
        {
            LuaDoString(BotUtils.ObfuscateLua($"for i=1,GetNumBattlegroundTypes() do local {{v:0}}=GetBattlegroundInfo(i)if {{v:0}}==\"{bgName}\"then JoinBattlefield(i) end end").Item1);
        }

        public void ReleaseSpirit()
        {
            LuaDoString("RepopMe();");
        }

        public void RepairAllItems()
        {
            LuaDoString("RepairAllItems();");
        }

        public void ReplaceItem(IWowItem currentItem, IWowItem newItem)
        {
            if (currentItem == null)
            {
                LuaDoString($"EquipItemByName(\"{newItem.Name}\");");
            }
            else
            {
                LuaDoString($"EquipItemByName(\"{newItem.Name}\", {(int)currentItem.EquipSlot});");
            }

            CofirmBop();
        }

        public void RetrieveCorpse()
        {
            LuaDoString("RetrieveCorpse();");
        }

        /// <summary>
        /// Roll something on a dropped item
        /// </summary>
        /// <param name="rollId">The rolls id to roll on</param>
        /// <param name="rollType">Need, Greed or Pass</param>
        public void RollOnItem(int rollId, RollType rollType)
        {
            LuaDoString($"RollOnLoot({rollId}, {(int)rollType});");
            ClickUiElement("StaticPopup1Button1");
        }

        public void SelectLfgRole(CombatClassRole combatClassRole)
        {
            string selectRoleUiElement = combatClassRole switch
            {
                CombatClassRole.Tank => "LFDRoleCheckPopupRoleButtonTank",
                CombatClassRole.Heal => "LFDRoleCheckPopupRoleButtonHealer",
                CombatClassRole.Dps => "LFDRoleCheckPopupRoleButtonDPS",
                _ => "LFDRoleCheckPopupRoleButtonDPS", // should never happen but in case, queue as DPS
            };

            // do this twice to ensure that we join the queue
            WowInterface.HookManager.ClickUiElement(selectRoleUiElement);
            WowInterface.HookManager.ClickUiElement("LFDRoleCheckPopupAcceptButton");

            WowInterface.HookManager.ClickUiElement(selectRoleUiElement);
            WowInterface.HookManager.ClickUiElement("LFDRoleCheckPopupAcceptButton");
        }

        public void SellAllGrayItems()
        {
            LuaDoString("local p,N,n=0 for b=0,4 do for s=1,GetContainerNumSlots(b) do n=GetContainerItemLink(b,s) if n and string.find(n,\"9d9d9d\") then N={GetItemInfo(n)} p=p+N[11] UseContainerItem(b,s) end end end");
        }

        public void SellAllItems()
        {
            LuaDoString("local p,N,n=0 for b=0,4 do for s=1,GetContainerNumSlots(b) do n=GetContainerItemLink(b,s) if n then N={GetItemInfo(n)} p=p+N[11] UseContainerItem(b,s) end end end");
        }

        public void SellItemsByName(string itemName)
        {
            LuaDoString($"for bag = 0,4,1 do for slot = 1, GetContainerNumSlots(bag), 1 do local name = GetContainerItemLink(bag,slot); if name and string.find(name,\"{itemName}\") then UseContainerItem(bag,slot) end end end");
        }

        public void SendChatMessage(string message)
        {
            LuaDoString($"DEFAULT_CHAT_FRAME.editBox:SetText(\"{message}\") ChatEdit_SendText(DEFAULT_CHAT_FRAME.editBox, 0)");
        }

        public void SendItemMailToCharacter(string itemName, string receiver)
        {
            LuaDoString($"for b=0,4 do for s=0,36 do I=GetContainerItemLink(b,s) if I and I:find(\"{itemName}\")then UseContainerItem(b,s) end end end SendMailNameEditBox:SetText(\"{receiver}\"))");
            ClickUiElement("SendMailMailButton");
        }

        public void SetFacing(WowUnit unit, float angle)
        {
            if (unit == null || angle < 0 || angle > Math.PI * 2) return;

            CallObjectFunction(unit.BaseAddress, WowInterface.OffsetList.FunctionUnitSetFacing, new List<object>() { angle.ToString().Replace(',', '.'), Environment.TickCount });
        }

        public void SetMaxFps(byte maxFps)
        {
            WowInterface.XMemory.Write(WowInterface.OffsetList.CvarMaxFps, maxFps);
        }

        public void SetRenderState(bool renderingEnabled)
        {
            WowInterface.XMemory.SuspendMainThread();

            if (renderingEnabled)
            {
                EnableFunction(WowInterface.OffsetList.FunctionWorldRender);
                EnableFunction(WowInterface.OffsetList.FunctionWorldRenderWorld);
                EnableFunction(WowInterface.OffsetList.FunctionWorldFrame);
            }
            else
            {
                DisableFunction(WowInterface.OffsetList.FunctionWorldRender);
                DisableFunction(WowInterface.OffsetList.FunctionWorldRenderWorld);
                DisableFunction(WowInterface.OffsetList.FunctionWorldFrame);
            }

            WowInterface.XMemory.ResumeMainThread();
        }

        public bool SetupEndsceneHook()
        {
            AmeisenLogger.Instance.Log("HookManager", "Setting up the EndsceneHook", LogLevel.Verbose);
            EndsceneAddress = GetEndScene();

            if (EndsceneAddress == IntPtr.Zero)
            {
                AmeisenLogger.Instance.Log("HookManager", "Unable to find Endscene function, exiting", LogLevel.Verbose);
                return false;
            }

            // we are going to replace the first 7 bytes of EndScene
            int hookSize = 0x7;
            EndsceneReturnAddress = IntPtr.Add(EndsceneAddress, hookSize);

            AmeisenLogger.Instance.Log("HookManager", $"Endscene is at: 0x{EndsceneAddress.ToInt32():X}", LogLevel.Verbose);

            // if WoW is already hooked, unhook it, wont do anything if wow is not hooked
            DisposeHook();

            if (WowInterface.XMemory.ReadBytes(EndsceneAddress, hookSize, out byte[] bytes))
            {
                OriginalEndsceneBytes = bytes;
                AmeisenLogger.Instance.Log("HookManager", $"EndsceneHook OriginalEndsceneBytes: {BotUtils.ByteArrayToString(OriginalEndsceneBytes)}", LogLevel.Verbose);
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
            WowInterface.XMemory.Fasm.AddLine($"TEST DWORD [{CodeToExecuteAddress.ToInt32()}], 1");
            WowInterface.XMemory.Fasm.AddLine("JE @out");

            // check if we want to override our is ingame check
            WowInterface.XMemory.Fasm.AddLine($"TEST DWORD [{OverrideWorldCheckAddress.ToInt32()}], 1");
            WowInterface.XMemory.Fasm.AddLine("JNE @ovr");

            // check for world to be loaded
            // we dont want to execute code in
            // the loadingscreen, cause that
            // mostly results in crashes
            WowInterface.XMemory.Fasm.AddLine($"TEST DWORD [{WowInterface.OffsetList.IsWorldLoaded.ToInt32()}], 1");
            WowInterface.XMemory.Fasm.AddLine("JE @out");
            WowInterface.XMemory.Fasm.AddLine("@ovr:");

            // execute our stuff and get return address
            WowInterface.XMemory.Fasm.AddLine($"CALL {CodecaveForExecution.ToInt32()}");
            WowInterface.XMemory.Fasm.AddLine($"MOV [{ReturnValueAddress.ToInt32()}], EAX");

            // finish up our execution
            WowInterface.XMemory.Fasm.AddLine("@out:");
            WowInterface.XMemory.Fasm.AddLine($"MOV DWORD [{CodeToExecuteAddress.ToInt32()}], 0");

            // call the gateway function
            WowInterface.XMemory.Fasm.AddLine($"JMP {CodecaveForGateway.ToInt32()}");

            // restore registers
            // WowInterface.XMemory.Fasm.AddLine("POPFD");
            // WowInterface.XMemory.Fasm.AddLine("POPAD");

            // inject the instructions into our codecave
            WowInterface.XMemory.Fasm.Inject((uint)CodecaveForCheck.ToInt32());

            // ---------------------------------------------------
            // End of the code that checks if there is asm to be
            // executed on our hook
            // ---------------------------------------------------

            // original EndScene in the gateway
            WowInterface.XMemory.Fasm.Clear();
            WowInterface.XMemory.WriteBytes(CodecaveForGateway, OriginalEndsceneBytes);

            // return to original function after we're done with our stuff
            WowInterface.XMemory.Fasm.AddLine($"JMP {EndsceneReturnAddress.ToInt32()}"); // 5 bytes

            // note if you increase the hookSize we need to add more NOP's
            WowInterface.XMemory.Fasm.AddLine($"NOP");                                   // 2 bytes

            WowInterface.XMemory.Fasm.Inject((uint)CodecaveForGateway.ToInt32() + (uint)OriginalEndsceneBytes.Length);
            WowInterface.XMemory.Fasm.Clear();

            // ---------------------------------------------------
            // End of doing the original stuff and returning to
            // the original instruction
            // ---------------------------------------------------

            // modify original EndScene instructions to start the hook
            WowInterface.XMemory.Fasm.AddLine($"JMP {CodecaveForCheck.ToInt32()}");
            WowInterface.XMemory.Fasm.Inject((uint)EndsceneAddress.ToInt32());

            AmeisenLogger.Instance.Log("HookManager", "EndsceneHook Successful", LogLevel.Verbose);

            // we should've hooked WoW now
            return IsWoWHooked;
        }

        public void StartAutoAttack(WowUnit wowUnit)
        {
            UnitOnRightClick(wowUnit);
        }

        public void StopClickToMoveIfActive()
        {
            if (IsClickToMoveActive(WowInterface.ObjectManager.Player))
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
                $"CALL {WowInterface.OffsetList.FunctionSetTarget.ToInt32()}",
                "ADD ESP, 0x8",
                "RETN"
            };

            InjectAndExecute(asm, false);
        }

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

                    byte returnedByte = InjectAndExecute(asm, true)[0];
                    // WowInterface.XMemory.Read(resultPointer, out result);

                    WowInterface.XMemory.FreeMemory(tracelineCodecave);
                    return returnedByte;
                }
            }

            return 0;
        }

        public void UnitOnRightClick(WowUnit wowUnit)
        {
            if (wowUnit == null || wowUnit.Guid == 0) return;

            CallObjectFunction(wowUnit.BaseAddress, WowInterface.OffsetList.FunctionUnitOnRightClick);
        }

        public void UnitSelectGossipOption(int gossipId)
        {
            LuaDoString($"SelectGossipOption(max({gossipId}, GetNumGossipOptions()))");
        }

        public void UseInventoryItem(EquipmentSlot equipmentSlot)
        {
            LuaDoString($"UseInventoryItem({(int)equipmentSlot})");
        }

        public void UseItemByBagAndSlot(int bagId, int bagSlot)
        {
            LuaDoString($"UseContainerItem({bagId}, {bagSlot});");
        }

        public void UseItemByName(string itemName)
        {
            SellItemsByName(itemName);
        }

        public void WowObjectOnRightClick(WowObject wowObject)
        {
            if (wowObject.GetType() == typeof(WowObject)
                || wowObject.GetType() == typeof(WowGameobject))
            {
                CallObjectFunction(wowObject.BaseAddress, WowInterface.OffsetList.FunctionGameobjectOnRightClick);
            }
        }

        private bool AllocateCodeCaves()
        {
            AmeisenLogger.Instance.Log("HookManager", "Allocating Codecaves for the EndsceneHook", LogLevel.Verbose);

            // integer to check if there is code waiting to be executed
            if (!WowInterface.XMemory.AllocateMemory(4, out IntPtr codeToExecuteAddress)) { return false; }

            CodeToExecuteAddress = codeToExecuteAddress;
            WowInterface.XMemory.Write(CodeToExecuteAddress, 0);
            AmeisenLogger.Instance.Log("HookManager", $"EndsceneHook CodeToExecuteAddress: 0x{CodeToExecuteAddress.ToInt32():X}", LogLevel.Verbose);

            // integer to save the pointer to the return value
            if (!WowInterface.XMemory.AllocateMemory(4, out IntPtr returnValueAddress)) { return false; }

            ReturnValueAddress = returnValueAddress;
            WowInterface.XMemory.Write(ReturnValueAddress, 0);
            AmeisenLogger.Instance.Log("HookManager", $"EndsceneHook ReturnValueAddress: 0x{ReturnValueAddress.ToInt32():X}", LogLevel.Verbose);

            // codecave to override the is ingame check, used at the login
            if (!WowInterface.XMemory.AllocateMemory(4, out IntPtr overrideWorldCheckAddress)) { return false; }

            OverrideWorldCheckAddress = overrideWorldCheckAddress;
            WowInterface.XMemory.Write(OverrideWorldCheckAddress, 0);
            AmeisenLogger.Instance.Log("HookManager", $"EndsceneHook OverrideWorldCheckAddress: 0x{OverrideWorldCheckAddress.ToInt32():X}", LogLevel.Verbose);

            // codecave for the original endscene code
            if (!WowInterface.XMemory.AllocateMemory(MEM_ALLOC_GATEWAY_SIZE, out IntPtr codecaveForGateway)) { return false; }

            CodecaveForGateway = codecaveForGateway;
            AmeisenLogger.Instance.Log("HookManager", $"EndsceneHook CodecaveForGateway ({MEM_ALLOC_GATEWAY_SIZE} bytes): 0x{CodecaveForGateway.ToInt32():X}", LogLevel.Verbose);

            // codecave to check wether we need to execute something
            if (!WowInterface.XMemory.AllocateMemory(MEM_ALLOC_CHECK_SIZE, out IntPtr codecaveForCheck)) { return false; }

            CodecaveForCheck = codecaveForCheck;
            AmeisenLogger.Instance.Log("HookManager", $"EndsceneHook CodecaveForCheck ({MEM_ALLOC_CHECK_SIZE} bytes): 0x{CodecaveForCheck.ToInt32():X}", LogLevel.Verbose);

            // codecave for the code we wan't to execute
            if (!WowInterface.XMemory.AllocateMemory(MEM_ALLOC_EXECUTION_SIZE, out IntPtr codecaveForExecution)) { return false; }

            CodecaveForExecution = codecaveForExecution;
            AmeisenLogger.Instance.Log("HookManager", $"EndsceneHook CodecaveForExecution ({MEM_ALLOC_EXECUTION_SIZE} bytes): 0x{CodecaveForExecution.ToInt32():X}", LogLevel.Verbose);

            return true;
        }

        private byte[] CallObjectFunction(IntPtr objectBaseAddress, IntPtr functionAddress, List<object> args = null, bool readReturnBytes = false)
        {
            if (objectBaseAddress == IntPtr.Zero || functionAddress == IntPtr.Zero || !WowInterface.ObjectManager.WowObjects.Any(e => e.BaseAddress == objectBaseAddress)) { return null; }

            AmeisenLogger.Instance.Log("HookManager", $"CallObjectFunction objectBaseAddress: 0x{objectBaseAddress.ToInt32():X} functionAddress: 0x{functionAddress.ToInt32():X} readReturnBytes: {readReturnBytes} args: {JsonConvert.SerializeObject(args)}", LogLevel.Verbose);
            AmeisenLogger.Instance.Log("HookManager", $"CallObjectFunction object: {WowInterface.ObjectManager.WowObjects.FirstOrDefault(e => e.BaseAddress == objectBaseAddress)}", LogLevel.Verbose);

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

            return InjectAndExecute(asm.ToArray(), readReturnBytes);
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
                && WowInterface.XMemory.Read(IntPtr.Add(pDevice, WowInterface.OffsetList.EndSceneOffsetDevice.ToInt32()), out IntPtr pEnd)
                && WowInterface.XMemory.Read(pEnd, out IntPtr pScene)
                && WowInterface.XMemory.Read(IntPtr.Add(pScene, WowInterface.OffsetList.EndSceneOffset.ToInt32()), out IntPtr pEndscene))
            {
                return pEndscene;
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        private byte[] InjectAndExecute(string[] asm, bool readReturnBytes, [CallerFilePath] string callingClass = "", [CallerMemberName]string callingFunction = "", [CallerLineNumber] int callingCodeline = 0)
        {
            WowInterface.ObjectManager.RefreshIsWorldLoaded();
            if (WowInterface.XMemory.Process.HasExited || (!WowInterface.ObjectManager.IsWorldLoaded && !OverrideWorldCheck))
            {
                return null;
            }

            Stopwatch fullStopwatch;
            List<byte> returnBytes = new List<byte>();

            AmeisenLogger.Instance.Log("HookManager", $"InjectAndExecute called by {callingClass}.{callingFunction}:{callingCodeline} ", LogLevel.Verbose);
            AmeisenLogger.Instance.Log("HookManager", $"Injecting: {JsonConvert.SerializeObject(asm)}", LogLevel.Verbose);

            lock (hookLock)
            {
                fullStopwatch = Stopwatch.StartNew();

                // zero our memory
                if (WowInterface.XMemory.ZeroMemory(CodecaveForExecution, MEM_ALLOC_EXECUTION_SIZE))
                {
                    bool frozenMainThread = false;

                    try
                    {
                        Stopwatch injectionStopwatch = Stopwatch.StartNew();

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
                        WowInterface.XMemory.Fasm.Inject((uint)CodecaveForExecution.ToInt32());

                        // now there is code to be executed
                        WowInterface.XMemory.Write(CodeToExecuteAddress, 1);
                        WowInterface.XMemory.ResumeMainThread();
                        frozenMainThread = false;

                        injectionStopwatch.Stop();
                        AmeisenLogger.Instance.Log("HookManager", $"Injection took {injectionStopwatch.ElapsedMilliseconds}ms", LogLevel.Verbose);

                        int delayCount = 0;
                        Stopwatch executionStopwatch = Stopwatch.StartNew();

                        AmeisenLogger.Instance.Log("HookManager", $"Injection completed", LogLevel.Verbose);

                        // wait for the code to be executed
                        while (WowInterface.XMemory.Read(CodeToExecuteAddress, out int codeToBeExecuted)
                               && codeToBeExecuted > 0)
                        {
                            ++delayCount;
                            Thread.Sleep(1);
                        }

                        executionStopwatch.Stop();
                        AmeisenLogger.Instance.Log("HookManager", $"Execution completed in {executionStopwatch.ElapsedMilliseconds}ms (delayCount: {delayCount})", LogLevel.Verbose);

                        // if we want to read the return value do it otherwise we're done
                        if (readReturnBytes)
                        {
                            AmeisenLogger.Instance.Log("HookManager", $"Reading return bytes", LogLevel.Verbose);

                            Stopwatch returnbytesStopwatch = Stopwatch.StartNew();

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

                            returnbytesStopwatch.Stop();
                            AmeisenLogger.Instance.Log("HookManager", $"Reading ReturnBytes (size: {returnBytes.Count}) took {returnbytesStopwatch.ElapsedMilliseconds}ms", LogLevel.Verbose);
                        }
                    }
                    catch (Exception e)
                    {
                        AmeisenLogger.Instance.Log("HookManager", $"Failed to inject:\n{e}", LogLevel.Error);

                        // now there is no more code to be executed
                        WowInterface.XMemory.Write(CodeToExecuteAddress, 0);
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

            fullStopwatch.Stop();
            AmeisenLogger.Instance.Log("HookManager", $"InjectAndExecute took {fullStopwatch.ElapsedMilliseconds}ms", LogLevel.Verbose);

            ++endsceneCalls;
            return returnBytes.ToArray();
        }

        private List<string> ReadAuras(WowLuaUnit luaunit, string functionName)
        {
            string[] debuffs = ExecuteLuaAndRead($"local a,b={{}},1;local c={functionName}(\"{luaunit}\",b)while c do a[#a+1]=c;b=b+1;c={functionName}(\"{luaunit}\",b)end;if#a<1 then a=\"\"else activeAuras=table.concat(a,\",\")end", "activeAuras").Split(',');

            List<string> resultLowered = new List<string>();

            for (int i = 0; i < debuffs.Length; ++i)
            {
                string s = debuffs[i];
                resultLowered.Add(s.Trim().ToLowerInvariant());
            }

            return resultLowered;
        }

        private List<WowAura> ReadAuraTable<T>(IntPtr buffBase, int auraCount) where T : unmanaged, IRawWowAuraTable
        {
            List<WowAura> buffs = new List<WowAura>();

            if (WowInterface.XMemory.Read(buffBase, out T auraTable))
            {
                List<RawWowAura> list = auraTable.AsList().GetRange(0, auraCount);
                for (int i = 0; i < list.Count; ++i)
                {
                    RawWowAura aura = list[i];
                    if (aura.SpellId > 0)
                    {
                        if (!WowInterface.BotCache.TryGetSpellName(aura.SpellId, out string name))
                        {
                            name = GetSpellNameById(aura.SpellId);
                            WowInterface.BotCache.CacheSpellName(aura.SpellId, name);
                        }

                        buffs.Add(new WowAura(aura, name.Length > 0 ? name : "unk"));
                    }
                }
            }

            return buffs;
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