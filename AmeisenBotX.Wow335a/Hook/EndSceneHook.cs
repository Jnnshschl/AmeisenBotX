using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Hook.Modules;
using AmeisenBotX.Core.Hook.Structs;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow335a.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmeisenBotX.Wow335a.Hook
{
    public class EndSceneHook
    {
        private const int MEM_ALLOC_EXECUTION_SIZE = 4096;
        private const int MEM_ALLOC_GATEWAY_SIZE = 12;
        private const int MEM_ALLOC_ROUTINE_SIZE = 128;
        private readonly object hookLock = new();

        private ulong hookCalls;

        public EndSceneHook(IMemoryApi memoryApi, IOffsetList offsetList, ObjectManager objectManager)
        {
            Memory = memoryApi;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            OriginalFunctionBytes = new();
        }

        public event Action<GameInfo> OnGameInfoPush;

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

        public bool IsWoWHooked => Memory.Read(WowEndSceneAddress, out byte c) && c == 0xE9;

        /// <summary>
        /// Codecave that hold the code, the bot want's to execute.
        /// </summary>
        private IntPtr CExecution { get; set; }

        /// <summary>
        /// Codecave that hold the original EndScene instructions
        /// and jumps back to the original function.
        /// </summary>
        private IntPtr CGateway { get; set; }

        /// <summary>
        /// Codecave used to check whether the bot want's to execute
        /// code and run the IHookModule's.
        /// </summary>
        private IntPtr CRoutine { get; set; }

        /// <summary>
        /// Pointer to the GameInfo struct that contains various
        /// static information about wow that are needed on a
        /// regular basis.
        /// </summary>
        private IntPtr GameInfoAddress { get; set; }

        /// <summary>
        /// Integer that instructs wow to refresh the GameInfo.
        /// </summary>
        private IntPtr GameInfoExecuteAddress { get; set; }

        /// <summary>
        /// Integer tha will be set to 1 when wow finished
        /// refreshing the GameInfo data.
        /// </summary>
        private IntPtr GameInfoExecutedAddress { get; set; }

        /// <summary>
        /// Integer tha will be set to 1 when we want to
        /// execute the LOS check.
        /// </summary>
        private IntPtr GameInfoExecuteLosCheckAddress { get; set; }

        /// <summary>
        /// Integer tha will be set to 1 when we're able
        /// to perform the LOS check.
        /// </summary>
        private IntPtr GameInfoLosCheckDataAddress { get; set; }

        /// <summary>
        /// Timer used to poll the gamedata returned by wow.
        /// </summary>
        private Timer GameInfoTimer { get; set; }

        /// <summary>
        /// The currently loaded hookmodules
        /// </summary>
        private List<IHookModule> HookModules { get; set; }

        /// <summary>
        /// Integer that will be set to 1 if the bot wait's for
        /// code to be executed. Will be set to 0 when done.
        /// </summary>
        private IntPtr IntShouldExecute { get; set; }

        private IMemoryApi Memory { get; }

        private ObjectManager ObjectManager { get; }

        private IOffsetList OffsetList { get; }

        /// <summary>
        /// Used to save the old render flags of wow.
        /// </summary>
        private int OldRenderFlags { get; set; }

        /// <summary>
        /// Save the original EndScene instructions that will be
        /// restored when the hook gets disposed.
        /// </summary>
        private byte[] OriginalEndsceneBytes { get; set; }

        /// <summary>
        /// Used to save the original instruction when a function get disabled.
        /// </summary>
        private Dictionary<IntPtr, byte> OriginalFunctionBytes { get; }

        /// <summary>
        /// Wether the hook should ignore if the world is not loaded or not.
        /// Used in the login screen as the world isnt loaded there.
        /// </summary>
        private bool OverrideWorldCheck { get; set; }

        /// <summary>
        /// Integer that is used to skip the world loaded check;
        /// </summary>
        private IntPtr OverrideWorldCheckAddress { get; set; }

        /// <summary>
        /// Pointer to the return value of the code executed on the
        /// EndScene hook.
        /// </summary>
        private IntPtr ReturnValueAddress { get; set; }

        /// <summary>
        /// The address of the EndScene function of wow.
        /// </summary>
        private IntPtr WowEndSceneAddress { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BotOverrideWorldLoadedCheck(bool status)
        {
            OverrideWorldCheck = status;
            Memory.Write(OverrideWorldCheckAddress, status ? 1 : 0);
        }

        public bool Hook(int hookSize, List<IHookModule> hookModules)
        {
            AmeisenLogger.I.Log("HookManager", $"Setting up the EndsceneHook (hookSize: {hookSize})", LogLevel.Verbose);

            if (hookSize < 0x5) { throw new ArgumentOutOfRangeException(nameof(hookSize), "cannot be smaller than 5"); }

            HookModules = hookModules;

            do
            {
                WowEndSceneAddress = GetEndScene();
                AmeisenLogger.I.Log("HookManager", $"Endscene is at: 0x{WowEndSceneAddress.ToInt32():X}", LogLevel.Verbose);

                if (WowEndSceneAddress == IntPtr.Zero)
                {
                    AmeisenLogger.I.Log("HookManager", $"Wow seems to not be started completely, retry in 500ms", LogLevel.Verbose);
                    Task.Delay(500).Wait();
                }
            }
            while (WowEndSceneAddress == IntPtr.Zero);

            if (!Memory.ReadBytes(WowEndSceneAddress, hookSize, out byte[] bytes))
            {
                AmeisenLogger.I.Log("HookManager", $"Failed reading the original EndScene bytes at: 0x{WowEndSceneAddress:X}", LogLevel.Error);
                return false;
            }

            OriginalEndsceneBytes = bytes;
            AmeisenLogger.I.Log("HookManager", $"EndsceneHook OriginalEndsceneBytes: {BotUtils.ByteArrayToString(OriginalEndsceneBytes)}", LogLevel.Verbose);

            if (!WowAllocateCodeCaves())
            {
                AmeisenLogger.I.Log("HookManager", $"Failed allocating codecaves", LogLevel.Error);
                return false;
            }

            foreach (IHookModule module in hookModules)
            {
                if (!module.Inject())
                {
                    AmeisenLogger.I.Log("HookManager", $"Failed to inject {nameof(module)} module", LogLevel.Error);
                    return false;
                }
            }

            // check for code to be executed
            Memory.AssemblyBuffer.AppendLine($"TEST DWORD [{IntShouldExecute}], 1");
            Memory.AssemblyBuffer.AppendLine("JE @out");

            // check if we want to override our is ingame check
            // going to be used while we are in the login screen
            Memory.AssemblyBuffer.AppendLine($"TEST DWORD [{OverrideWorldCheckAddress}], 1");
            Memory.AssemblyBuffer.AppendLine("JNE @ovr");

            // check for world to be loaded
            // we dont want to execute code in
            // the loadingscreen, cause that
            // mostly results in crashes
            Memory.AssemblyBuffer.AppendLine($"TEST DWORD [{OffsetList.IsWorldLoaded}], 1");
            Memory.AssemblyBuffer.AppendLine("JE @out");
            Memory.AssemblyBuffer.AppendLine("@ovr:");

            // execute our stuff and get return address
            Memory.AssemblyBuffer.AppendLine($"CALL {CExecution}");
            Memory.AssemblyBuffer.AppendLine($"MOV [{ReturnValueAddress}], EAX");

            // finish up our execution
            Memory.AssemblyBuffer.AppendLine("@out:");
            Memory.AssemblyBuffer.AppendLine($"MOV DWORD [{IntShouldExecute}], 0");

            // ----------------------------
            // # GameInfo & EventHook stuff
            // ----------------------------
            // world loaded and should execute check
            Memory.AssemblyBuffer.AppendLine($"TEST DWORD [{OffsetList.IsWorldLoaded}], 1");
            Memory.AssemblyBuffer.AppendLine("JE @skpgi");
            Memory.AssemblyBuffer.AppendLine($"TEST DWORD [{GameInfoExecuteAddress}], 1");
            Memory.AssemblyBuffer.AppendLine("JE @skpgi");

            // isOutdoors
            Memory.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionGetActivePlayerObject}");
            Memory.AssemblyBuffer.AppendLine("MOV ECX, EAX");
            Memory.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionIsOutdoors}");
            Memory.AssemblyBuffer.AppendLine($"MOV DWORD [{GameInfoAddress}], EAX");

            // isTargetInLineOfSight
            Memory.AssemblyBuffer.AppendLine($"MOV BYTE [{GameInfoAddress + 1}], 0");

            Memory.AssemblyBuffer.AppendLine($"TEST DWORD [{GameInfoExecuteLosCheckAddress}], 1");
            Memory.AssemblyBuffer.AppendLine("JE @loscheck");

            IntPtr distancePointer = GameInfoLosCheckDataAddress;
            IntPtr startPointer = IntPtr.Add(distancePointer, 0x4);
            IntPtr endPointer = IntPtr.Add(startPointer, 0xC);
            IntPtr resultPointer = IntPtr.Add(endPointer, 0xC);

            Memory.AssemblyBuffer.AppendLine("PUSH 0");
            Memory.AssemblyBuffer.AppendLine("PUSH 0x120171");
            Memory.AssemblyBuffer.AppendLine($"PUSH {distancePointer}");
            Memory.AssemblyBuffer.AppendLine($"PUSH {resultPointer}");
            Memory.AssemblyBuffer.AppendLine($"PUSH {endPointer}");
            Memory.AssemblyBuffer.AppendLine($"PUSH {startPointer}");
            Memory.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionTraceline}");
            Memory.AssemblyBuffer.AppendLine("ADD ESP, 0x18");

            Memory.AssemblyBuffer.AppendLine("XOR AL, 1");
            Memory.AssemblyBuffer.AppendLine($"MOV BYTE [{GameInfoAddress + 1}], AL");

            Memory.AssemblyBuffer.AppendLine($"MOV DWORD [{GameInfoExecuteLosCheckAddress}], 0");
            Memory.AssemblyBuffer.AppendLine("@loscheck:");

            foreach (IHookModule module in hookModules)
            {
                Memory.AssemblyBuffer.AppendLine($"CALL {module.AsmAddress}");
            }

            Memory.AssemblyBuffer.AppendLine($"MOV DWORD [{GameInfoExecutedAddress}], 1");
            Memory.AssemblyBuffer.AppendLine("@skpgi:");
            Memory.AssemblyBuffer.AppendLine($"MOV DWORD [{GameInfoExecuteAddress}], 0");
            // ----------------

            Memory.AssemblyBuffer.AppendLine($"JMP {CGateway}");

            if (!Memory.InjectAssembly(CRoutine))
            {
                Memory.ResumeMainThread();
                AmeisenLogger.I.Log("HookManager", $"Failed to inject hook check", LogLevel.Error);
                return false;
            }

            // ---------------------------------------------------
            // End of the code that checks if there is asm to be
            // executed on our hook
            // ---------------------------------------------------

            // write the original EndScene instructions
            Memory.WriteBytes(CGateway, OriginalEndsceneBytes);
            Memory.AssemblyBuffer.AppendLine($"JMP {IntPtr.Add(WowEndSceneAddress, hookSize)}");

            // jump back to the original EndScene
            if (!Memory.InjectAssembly(CGateway + OriginalEndsceneBytes.Length))
            {
                Memory.ResumeMainThread();
                AmeisenLogger.I.Log("HookManager", $"Failed to inject hook check", LogLevel.Error);
                return false;
            }

            // ---------------------------------------------------
            // End of doing the original stuff and returning to
            // the original instruction
            // ---------------------------------------------------

            // modify original EndScene instructions to start the hook
            Memory.AssemblyBuffer.AppendLine($"JMP {CRoutine}");

            for (int i = 5; i < hookSize; ++i)
            {
                Memory.AssemblyBuffer.AppendLine("NOP");
            }

            // suspend wows main thread and inject
            Memory.SuspendMainThread();

            if (!Memory.InjectAssembly(WowEndSceneAddress, true))
            {
                Memory.ResumeMainThread();
                AmeisenLogger.I.Log("HookManager", $"Failed to modify original endscene bytes", LogLevel.Error);
                return false;
            }

            Memory.ResumeMainThread();

            // try to update the GameInfo struct every 100ms
            GameInfoTimer = new((_) => GameInfoTimerTick(), null, 0, 100);

            AmeisenLogger.I.Log("HookManager", "EndsceneHook successful", LogLevel.Verbose);
            return IsWoWHooked;
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
                        LuaDoString($"SelectQuestLogEntry({i});SetAbandonQuest();AbandonQuest()");
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
            return int.TryParse(LuaGetCVar("autoLootDefault"), out int result) && result == 1;
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
            // {(!WowInterface.Player.IsDead ? "StaticPopup1Button1:Click();" : "")}
            LuaDoString($"ConfirmBindOnUse();StaticPopup_Hide(\"AUTOEQUIP_BIND\");StaticPopup_Hide(\"EQUIP_BIND\");StaticPopup_Hide(\"USE_BIND\")");
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

                if (Memory.AllocateMemory((uint)bytes.Length + 1, out IntPtr memAlloc))
                {
                    if (memAlloc != IntPtr.Zero)
                    {
                        Memory.WriteBytes(memAlloc, bytes);
                        Memory.Write<byte>(memAlloc + (bytes.Length + 1), 0);

                        string[] asm = new string[]
                        {
                            "PUSH 0",
                            $"PUSH {memAlloc}",
                            $"PUSH {memAlloc}",
                            $"CALL {OffsetList.FunctionLuaDoString}",
                            "ADD ESP, 0xC",
                            "RET",
                        };

                        bool status = InjectAndExecute(asm, false, out _);
                        Memory.FreeMemory(memAlloc);
                        return status;
                    }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaEquipItem(string newItem, int itemSlot = -1)
        {
            if (itemSlot == -1)
            {
                LuaDoString($"EquipItemByName(\"{newItem}\")");
            }
            else
            {
                LuaDoString($"EquipItemByName(\"{newItem}\", {itemSlot})");
            }

            LuaCofirmStaticPopup();
        }

        public IEnumerable<int> LuaGetCompletedQuests()
        {
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=''for a,b in pairs(GetQuestsCompleted())do if b then {{v:0}}={{v:0}}..a..';'end end;"), out string result))
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
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"local g1,_,_,_,g2,_,_,_,g3,_,_,_,g4,_,_,_,g5,_,_,_,g6 = GetGossipActiveQuests(); local gps={{g1,g2,g3,g4,g5,g6}}; {{v:0}}=gps[{gossipId}]"), out string r1))
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
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"local g1,_,_,_,_,g2,_,_,_,_,g3,_,_,_,_,g4,_,_,_,_,g5,_,_,_,_,g6 = GetGossipAvailableQuests(); local gps={{g1,g2,g3,g4,g5,g6}}; for k,v in pairs(gps) do if v == \"{title}\" then {{v:0}}=k; break end; end;"), out string r1)
                && int.TryParse(r1, out int foundGossipId))
            {
                gossipId = foundGossipId;
                return true;
            }

            gossipId = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LuaGetGossipOptionCount()
        {
            return ExecuteLuaInt(BotUtils.ObfuscateLua("{v:0}=GetNumGossipOptions()"));
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
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetNumQuestLogChoices();"), out string result)
                && int.TryParse(result, out int num))
            {
                numChoices = num;
                return true;
            }

            numChoices = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaGetQuestLogChoiceItemLink(int index, out string itemLink)
        {
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetQuestLogItemLink(\"choice\", {index});"),
                out string result))
            {
                if (result == "nil")
                {
                    itemLink = string.Empty;
                    return false;
                }

                itemLink = result;
                return true;
            }

            itemLink = string.Empty;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaGetQuestLogIdByTitle(string title, out int questLogId)
        {
            if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"for i=1,GetNumQuestLogEntries() do if GetQuestLogTitle(i) == \"{title}\" then {{v:0}}=i; break end; end;"), out string r1)
                && int.TryParse(r1, out int foundQuestLogId))
            {
                questLogId = foundQuestLogId;
                return true;
            }

            questLogId = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaGetQuestReward(int id)
        {
            LuaDoString($"GetQuestReward({id})");
        }

        public Dictionary<string, (int, int)> LuaGetSkills()
        {
            Dictionary<string, (int, int)> parsedSkills = new();

            try
            {
                if (WowExecuteLuaAndRead(BotUtils.ObfuscateLua("{v:0}=\"\"{v:1}=GetNumSkillLines()for a=1,{v:1} do local b,c,_,d,_,_,e=GetSkillLineInfo(a)if not c then {v:0}={v:0}..b;if a<{v:1} then {v:0}={v:0}..\":\"..tostring(d or 0)..\"/\"..tostring(e or 0)..\";\"end end end"), out string result))
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
        /// Check if the string is casting or channeling a spell
        /// </summary>
        /// <param name="luaunit">player, target, party1...</param>
        /// <returns>(Spellname, duration)</returns>
        public (string, int) LuaGetUnitCastingInfo(string luaunit)
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
            return ExecuteLuaInt(BotUtils.ObfuscateLua("{v:0}=GetUnspentTalentPoints()"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaHasUnitStealableBuffs(string luaUnit)
        {
            return ExecuteLuaIntResult(BotUtils.ObfuscateLua($"{{v:0}}=0;local y=0;for i=1,40 do local n,_,_,_,_,_,_,_,{{v:1}}=UnitAura(\"{luaUnit}\",i);if {{v:1}}==1 then {{v:0}}=1;end end"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaIsBgInviteReady()
        {
            return ExecuteLuaIntResult(BotUtils.ObfuscateLua("{v:0}=0;for i=1,2 do local x=GetBattlefieldPortExpiration(i) if x>0 then {v:0}=1 end end"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LuaIsGhost(string luaUnit)
        {
            return ExecuteLuaIntResult(BotUtils.ObfuscateLua($"{{v:0}}=UnitIsGhost(\"{luaUnit}\");"));
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
            return ExecuteLuaIntResult(BotUtils.ObfuscateLua("{v:0}=IsOutdoors()"));
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
        public void LuaRollOnLoot(int rollId, int rollType)
        {
            if (rollType == 1)
            {
                // first we need to check whether we can roll a need on this, otherwise the bot might not roll at all
                LuaDoString($"_,_,_,_,_,canNeed=GetLootRollItemInfo({rollId});if canNeed then RollOnLoot({rollId}, {rollType}) else RollOnLoot({rollId}, 2) end");
            }
            else
            {
                LuaDoString($"RollOnLoot({rollId}, {rollType})");
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
        public void LuaSetLfgRole(int combatClassRole)
        {
            int[] roleBools = new int[3]
            {
                combatClassRole == 1 ? 1:0,
                combatClassRole == 2 ? 1:0,
                combatClassRole == 3 ? 1:0
            };

            LuaDoString($"SetLFGRoles(0, {roleBools[0]}, {roleBools[1]}, {roleBools[2]});LFDRoleCheckPopupAcceptButton:Click()");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public void LuaTargetUnit(string unit)
        {
            LuaDoString($"TargetUnit(\"{unit}\");");
        }

        public bool LuaUiIsVisible(params string[] uiElements)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaUseContainerItem(int bagId, int bagSlot)
        {
            LuaDoString($"UseContainerItem({bagId}, {bagSlot})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaUseInventoryItem(int equipmentSlot)
        {
            LuaDoString($"UseInventoryItem({equipmentSlot})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaUseItemByName(string itemName)
        {
            LuaSellItemsByName(itemName);
        }

        public void Unhook()
        {
            if (!IsWoWHooked)
            {
                return;
            }

            AmeisenLogger.I.Log("HookManager", "Disposing EndScene hook", LogLevel.Verbose);
            GameInfoTimer.Dispose();

            lock (hookLock)
            {
                Memory.SuspendMainThread();
                Memory.WriteBytes(WowEndSceneAddress, OriginalEndsceneBytes);
                Memory.ResumeMainThread();

                Memory.FreeAllMemory();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WowClearTarget()
        {
            WowTargetGuid(0);
        }

        public void WowClickOnTerrain(Vector3 position)
        {
            if (Memory.AllocateMemory(20, out IntPtr codeCaveVector3))
            {
                Memory.Write<ulong>(codeCaveVector3, 0);
                Memory.Write(IntPtr.Add(codeCaveVector3, 0x8), position);

                string[] asm = new string[]
                {
                    $"PUSH {codeCaveVector3}",
                    $"CALL {OffsetList.FunctionHandleTerrainClick}",
                    "ADD ESP, 0x4",
                    "RET",
                };

                InjectAndExecute(asm, false, out _);
                Memory.FreeMemory(codeCaveVector3);
            }
        }

        public void WowClickToMove(IntPtr playerBase, Vector3 position)
        {
            if (Memory.AllocateMemory(12, out IntPtr codeCaveVector3))
            {
                Memory.Write(codeCaveVector3, position);

                WowCallObjectFunction(playerBase, OffsetList.FunctionPlayerClickToMove, new List<object>() { codeCaveVector3 });
                Memory.FreeMemory(codeCaveVector3);
            }
        }

        public void WowEnableClickToMove()
        {
            if (Memory.Read(OffsetList.ClickToMovePointer, out IntPtr ctmPointer)
                && Memory.Read(IntPtr.Add(ctmPointer, (int)OffsetList.ClickToMoveEnabled), out int ctmEnabled)
                && ctmEnabled != 1)
            {
                Memory.Write(IntPtr.Add(ctmPointer, (int)OffsetList.ClickToMoveEnabled), 1);
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

                if (Memory.AllocateMemory((uint)commandBytes.Length + (uint)variableBytes.Length + 2, out IntPtr memAllocCmdVar))
                {
                    int varOffset = commandBytes.Length + 1;
                    byte[] bytesToWrite = new byte[commandBytes.Length + (uint)variableBytes.Length + 2];

                    Array.Copy(commandBytes, bytesToWrite, commandBytes.Length);
                    Array.Copy(variableBytes, 0, bytesToWrite, varOffset, variableBytes.Length);

                    Memory.WriteBytes(memAllocCmdVar, bytesToWrite);

                    string[] asm = new string[]
                    {
                        "PUSH 0",
                        $"PUSH {memAllocCmdVar}",
                        $"PUSH {memAllocCmdVar}",
                        $"CALL {OffsetList.FunctionLuaDoString}",
                        "ADD ESP, 0xC",
                        $"CALL {OffsetList.FunctionGetActivePlayerObject}",
                        "MOV ECX, EAX",
                        "PUSH -1",
                        $"PUSH {memAllocCmdVar + varOffset}",
                        $"CALL {OffsetList.FunctionGetLocalizedText}",
                        "RET",
                    };

                    if (InjectAndExecute(asm, true, out byte[] bytes))
                    {
                        if (bytes != null)
                        {
                            result = Encoding.UTF8.GetString(bytes);

                            Memory.FreeMemory(memAllocCmdVar);
                            return true;
                        }
                    }

                    Memory.FreeMemory(memAllocCmdVar);
                }
            }

            result = string.Empty;
            return false;
        }

        public void WowFacePosition(IntPtr playerBase, Vector3 playerPosition, Vector3 positionToFace)
        {
            WowSetFacing(playerBase, BotMath.GetFacingAngle(playerPosition, positionToFace));
        }

        public bool WowGetLocalizedText(string variable, out string result)
        {
            AmeisenLogger.I.Log("HookManager", $"GetLocalizedText: {variable}", LogLevel.Verbose);

            if (!string.IsNullOrWhiteSpace(variable))
            {
                byte[] variableBytes = Encoding.UTF8.GetBytes(variable);

                if (Memory.AllocateMemory((uint)variableBytes.Length + 1, out IntPtr memAlloc))
                {
                    Memory.WriteBytes(memAlloc, variableBytes);
                    Memory.Write<byte>(memAlloc + (variableBytes.Length + 1), 0);

                    if (memAlloc != IntPtr.Zero)
                    {
                        string[] asm = new string[]
                        {
                            $"CALL {OffsetList.FunctionGetActivePlayerObject}",
                            "MOV ECX, EAX",
                            "PUSH -1",
                            $"PUSH {memAlloc}",
                            $"CALL {OffsetList.FunctionGetLocalizedText}",
                            "RET",
                        };

                        if (InjectAndExecute(asm, true, out byte[] bytes))
                        {
                            result = Encoding.UTF8.GetString(bytes);
                            Memory.FreeMemory(memAlloc);
                            return true;
                        }
                    }
                }
            }

            result = string.Empty;
            return false;
        }

        public Dictionary<int, int> WowGetRunesReady()
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

        public int WowGetUnitReaction(IntPtr a, IntPtr b)
        {
            AmeisenLogger.I.Log("HookManager", $"Getting Reaction of {a} and {b}", LogLevel.Verbose);

            byte[] returnBytes = WowCallObjectFunction(a, OffsetList.FunctionUnitGetReaction, new() { b }, true);
            return returnBytes?.Length > 0 ? BitConverter.ToInt32(returnBytes, 0) : 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WowIsClickToMoveActive()
        {
            return Memory.Read(OffsetList.ClickToMoveAction, out int ctmState)
                && ctmState != 0    // None
                && ctmState != 3    // Stop
                && ctmState != 13;  // Halted
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
            return Memory.Read(OffsetList.Runes, out byte runeStatus) && ((1 << runeId) & runeStatus) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WowObjectRightClick(IntPtr objectBase)
        {
            WowCallObjectFunction(objectBase, OffsetList.FunctionGameobjectOnRightClick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WowSetFacing(IntPtr unitBase, float angle)
        {
            WowCallObjectFunction(unitBase, OffsetList.FunctionUnitSetFacing, new List<object>() { angle.ToString(CultureInfo.InvariantCulture).Replace(',', '.'), Environment.TickCount });
        }

        public void WowSetRenderState(bool renderingEnabled)
        {
            if (renderingEnabled)
            {
                LuaDoString("WorldFrame:Show();UIParent:Show()");
            }

            Memory.SuspendMainThread();

            if (renderingEnabled)
            {
                EnableFunction(OffsetList.FunctionWorldRender);
                EnableFunction(OffsetList.FunctionWorldRenderWorld);
                EnableFunction(OffsetList.FunctionWorldFrame);

                Memory.Write(OffsetList.RenderFlags, OldRenderFlags);
            }
            else
            {
                if (Memory.Read(OffsetList.RenderFlags, out int renderFlags))
                {
                    OldRenderFlags = renderFlags;
                }

                DisableFunction(OffsetList.FunctionWorldRender);
                DisableFunction(OffsetList.FunctionWorldRenderWorld);
                DisableFunction(OffsetList.FunctionWorldFrame);

                Memory.Write(OffsetList.RenderFlags, 0);
            }

            Memory.ResumeMainThread();

            if (!renderingEnabled)
            {
                LuaDoString("WorldFrame:Hide();UIParent:Hide()");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WowStopClickToMove(IntPtr playerBase)
        {
            if (WowIsClickToMoveActive())
            {
                WowCallObjectFunction(playerBase, OffsetList.FunctionPlayerClickToMoveStop);
            }
        }

        public void WowTargetGuid(ulong guid)
        {
            byte[] guidBytes = BitConverter.GetBytes(guid);

            string[] asm = new string[]
            {
                $"PUSH {BitConverter.ToUInt32(guidBytes, 4)}",
                $"PUSH {BitConverter.ToUInt32(guidBytes, 0)}",
                $"CALL {OffsetList.FunctionSetTarget}",
                "ADD ESP, 0x8",
                "RET"
            };

            InjectAndExecute(asm, false, out _);
        }

        public byte WowTraceLine(Vector3 start, Vector3 end, out Vector3 result, uint flags = 0x120171)
        {
            if (Memory.AllocateMemory(40, out IntPtr tracelineCodecave))
            {
                (float, Vector3, Vector3) tracelineCombo = (1.0f, start, end);

                IntPtr distancePointer = tracelineCodecave;
                IntPtr startPointer = IntPtr.Add(distancePointer, 0x4);
                IntPtr endPointer = IntPtr.Add(startPointer, 0xC);
                IntPtr resultPointer = IntPtr.Add(endPointer, 0xC);

                if (Memory.Write(distancePointer, tracelineCombo))
                {
                    string[] asm = new string[]
                    {
                        "PUSH 0",
                        $"PUSH {flags}",
                        $"PUSH {distancePointer}",
                        $"PUSH {resultPointer}",
                        $"PUSH {endPointer}",
                        $"PUSH {startPointer}",
                        $"CALL {OffsetList.FunctionTraceline}",
                        "ADD ESP, 0x18",
                        "RET",
                    };

                    if (InjectAndExecute(asm, true, out byte[] bytes)
                        && bytes != null && bytes.Length > 0
                        && Memory.Read(resultPointer, out result))
                    {
                        Memory.FreeMemory(tracelineCodecave);
                        return bytes[0];
                    }

                    Memory.FreeMemory(tracelineCodecave);
                }
            }

            result = Vector3.Zero;
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WowUnitRightClick(IntPtr unitBase)
        {
            WowCallObjectFunction(unitBase, OffsetList.FunctionUnitOnRightClick);
        }

        private void DisableFunction(IntPtr address)
        {
            // check whether we already replaced the function or not
            if (Memory.Read(address, out byte opcode)
                && opcode != 0xC3)
            {
                SaveOriginalFunctionBytes(address);
                Memory.PatchMemory<byte>(address, 0xC3);
            }
        }

        private void EnableFunction(IntPtr address)
        {
            // check for RET opcode to be present before restoring original function
            if (OriginalFunctionBytes.ContainsKey(address)
                && Memory.Read(address, out byte opcode)
                && opcode == 0xC3)
            {
                Memory.PatchMemory(address, OriginalFunctionBytes[address]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ExecuteLuaInt((string, string) cmdVar)
        {
            return WowExecuteLuaAndRead(cmdVar, out string s)
                && int.TryParse(s, out int i)
                 ? i : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ExecuteLuaIntResult((string, string) cmdVar)
        {
            return WowExecuteLuaAndRead(cmdVar, out string s)
                && int.TryParse(s, out int i)
                && i == 1;
        }

        private void GameInfoTimerTick()
        {
            if (Memory.Read(GameInfoExecuteAddress, out int executeStatus)
                && executeStatus == 1)
            {
                // still waiting for execution
                return;
            }

            if (Memory.Read(GameInfoExecutedAddress, out int executedStatus)
                && executedStatus == 0)
            {
                if (ObjectManager.TargetGuid != 0 && ObjectManager.Target != null)
                {
                    Vector3 playerPosition = ObjectManager.Player.Position;
                    playerPosition.Z += 1.5f;

                    Vector3 targetPosition = ObjectManager.Target.Position;
                    targetPosition.Z += 1.5f;

                    if (Memory.Write(GameInfoLosCheckDataAddress, (1.0f, playerPosition, targetPosition)))
                    {
                        // run the los check if we have a target
                        Memory.Write(GameInfoExecuteLosCheckAddress, 1);
                    }
                }

                // run the gameinfo update
                Memory.Write(GameInfoExecuteAddress, 1);
            }
            else
            {
                // process the info
                if (Memory.Read(GameInfoAddress, out GameInfo gameInfo))
                {
                    OnGameInfoPush?.Invoke(gameInfo);
                    AmeisenLogger.I.Log("GameInfo", $"Pushing GameInfo Update: {JsonConvert.SerializeObject(gameInfo)}");
                }

                Memory.Write(GameInfoExecutedAddress, 0);

                foreach (IHookModule module in HookModules)
                {
                    module.OnDataUpdate?.Invoke(module.GetDataPointer());
                }
            }
        }

        private IntPtr GetEndScene()
        {
            if (Memory.Read(OffsetList.EndSceneStaticDevice, out IntPtr pDevice)
                && Memory.Read(IntPtr.Add(pDevice, (int)OffsetList.EndSceneOffsetDevice), out IntPtr pEnd)
                && Memory.Read(pEnd, out IntPtr pScene)
                && Memory.Read(IntPtr.Add(pScene, (int)OffsetList.EndSceneOffset), out IntPtr pEndscene))
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
            if (!IsWoWHooked || Memory.Process.HasExited)
            {
                bytes = null;
                return false;
            }

            List<byte> returnBytes = readReturnBytes ? new() : null;

            lock (hookLock)
            {
                // zero our memory
                if (Memory.ZeroMemory(CExecution, MEM_ALLOC_EXECUTION_SIZE))
                {
                    bool frozenMainThread = false;

                    try
                    {
                        // add all lines
                        for (int i = 0; i < asm.Length; ++i)
                        {
                            Memory.AssemblyBuffer.AppendLine(asm[i]);
                        }

                        // inject it
                        Memory.SuspendMainThread();
                        frozenMainThread = true;
                        Memory.InjectAssembly(CExecution);

                        // now there is code to be executed
                        Memory.Write(IntShouldExecute, 1);
                        Memory.ResumeMainThread();
                        frozenMainThread = false;

                        // wait for the code to be executed
                        while (Memory.Read(IntShouldExecute, out int codeToBeExecuted)
                               && codeToBeExecuted > 0)
                        {
                            Thread.Sleep(1);
                        }

                        // if we want to read the return value do it otherwise we're done
                        if (readReturnBytes)
                        {
                            Memory.Read(ReturnValueAddress, out uint dwAddress);
                            IntPtr addrPointer = new(dwAddress);

                            // read all parameter-bytes until we the buffer is 0
                            Memory.Read(addrPointer, out byte buffer);

                            if (buffer != 0)
                            {
                                do
                                {
                                    returnBytes.Add(buffer);
                                    addrPointer += 1;
                                } while (Memory.Read(addrPointer, out buffer) && buffer != 0);
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
                        Memory.Write(IntShouldExecute, 0);

                        bytes = null;
                        return false;
                    }
                    finally
                    {
                        if (frozenMainThread)
                        {
                            Memory.ResumeMainThread();
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
            return WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetCVar(\"{CVar}\");"), out string s) ? s : string.Empty;
        }

        private void SaveOriginalFunctionBytes(IntPtr address)
        {
            if (Memory.Read(address, out byte opcode))
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
            AmeisenLogger.I.Log("HookManager", "Allocating Codecaves", LogLevel.Verbose);

            #region EndScene Hook

            // integer to check if there is code waiting to be executed
            if (!Memory.AllocateMemory(4, out IntPtr codeToExecuteAddress)) { return false; }

            IntShouldExecute = codeToExecuteAddress;

            // integer to save the pointer to the return value
            if (!Memory.AllocateMemory(4, out IntPtr returnValueAddress)) { return false; }

            ReturnValueAddress = returnValueAddress;

            // codecave to override the is ingame check, used at the login
            if (!Memory.AllocateMemory(4, out IntPtr overrideWorldCheckAddress)) { return false; }

            OverrideWorldCheckAddress = overrideWorldCheckAddress;

            // codecave for the original endscene code
            if (!Memory.AllocateMemory(MEM_ALLOC_GATEWAY_SIZE, out IntPtr codecaveForGateway)) { return false; }

            CGateway = codecaveForGateway;

            // codecave to check whether we need to execute something
            if (!Memory.AllocateMemory(MEM_ALLOC_ROUTINE_SIZE, out IntPtr codecaveForCheck)) { return false; }

            CRoutine = codecaveForCheck;

            // codecave for the code we wan't to execute
            if (!Memory.AllocateMemory(MEM_ALLOC_EXECUTION_SIZE, out IntPtr codecaveForExecution)) { return false; }

            CExecution = codecaveForExecution;

            #endregion EndScene Hook

            #region GameInfo

            // codecave for the gameinfo execution
            if (!Memory.AllocateMemory(4, out IntPtr gameInfoExecute)) { return false; }

            GameInfoExecuteAddress = gameInfoExecute;
            Memory.Write(GameInfoExecuteAddress, 0);

            // codecave for the gameinfo executed
            if (!Memory.AllocateMemory(4, out IntPtr gameInfoExecuted)) { return false; }

            GameInfoExecutedAddress = gameInfoExecuted;
            Memory.Write(GameInfoExecutedAddress, 0);

            // codecave for the gameinfo struct
            uint gameinfoSize = (uint)sizeof(GameInfo);

            if (!Memory.AllocateMemory(gameinfoSize, out IntPtr gameInfo)) { return false; }

            GameInfoAddress = gameInfo;

            // codecave for the gameinfo line of sight check
            if (!Memory.AllocateMemory(4, out IntPtr executeLosCheck)) { return false; }

            GameInfoExecuteLosCheckAddress = executeLosCheck;
            Memory.Write(GameInfoExecuteLosCheckAddress, 0);

            // codecave for the gameinfo line of sight check data
            if (!Memory.AllocateMemory(40, out IntPtr losCheckData)) { return false; }

            GameInfoLosCheckDataAddress = losCheckData;

            #endregion GameInfo

            #region Allocations Logging

            AmeisenLogger.I.Log("HookManager", $"{"CodeToExecuteAddress",-36} ({4,-4} bytes): 0x{IntShouldExecute.ToInt32():X}", LogLevel.Verbose);
            AmeisenLogger.I.Log("HookManager", $"{"ReturnValueAddress",-36} ({4,-4} bytes): 0x{ReturnValueAddress.ToInt32():X}", LogLevel.Verbose);
            AmeisenLogger.I.Log("HookManager", $"{"OverrideWorldCheckAddress",-36} ({4,-4} bytes): 0x{OverrideWorldCheckAddress.ToInt32():X}", LogLevel.Verbose);
            AmeisenLogger.I.Log("HookManager", $"{"CodecaveForGateway",-36} ({MEM_ALLOC_GATEWAY_SIZE,-4} bytes): 0x{CGateway.ToInt32():X}", LogLevel.Verbose);
            AmeisenLogger.I.Log("HookManager", $"{"CodecaveForCheck",-36} ({MEM_ALLOC_ROUTINE_SIZE,-4} bytes): 0x{CRoutine.ToInt32():X}", LogLevel.Verbose);
            AmeisenLogger.I.Log("HookManager", $"{"CodecaveForExecution",-36} ({MEM_ALLOC_EXECUTION_SIZE,-4} bytes): 0x{CExecution.ToInt32():X}", LogLevel.Verbose);
            AmeisenLogger.I.Log("HookManager", $"{"GameInfoExecuteAddress",-36} ({4,-4} bytes): 0x{GameInfoExecuteAddress.ToInt32():X}", LogLevel.Verbose);
            AmeisenLogger.I.Log("HookManager", $"{"GameInfoExecutedAddress",-36} ({4,-4} bytes): 0x{GameInfoExecutedAddress.ToInt32():X}", LogLevel.Verbose);
            AmeisenLogger.I.Log("HookManager", $"{"GameInfoAddress",-36} ({gameinfoSize,-4} bytes): 0x{GameInfoAddress.ToInt32():X}", LogLevel.Verbose);
            AmeisenLogger.I.Log("HookManager", $"{"GameInfoExecuteLosCheckAddress",-36} ({4,-4} bytes): 0x{GameInfoExecuteLosCheckAddress.ToInt32():X}", LogLevel.Verbose);
            AmeisenLogger.I.Log("HookManager", $"{"GameInfoLosCheckDataAddress",-36} ({40,-4} bytes): 0x{GameInfoLosCheckDataAddress.ToInt32():X}", LogLevel.Verbose);

            #endregion Allocations Logging

            return true;
        }

        private byte[] WowCallObjectFunction(IntPtr objectBaseAddress, IntPtr functionAddress, List<object> args = null, bool readReturnBytes = false)
        {
            List<string> asm = new() { $"MOV ECX, {objectBaseAddress}" };

            if (args != null)
            {
                // push all parameters
                for (int i = 0; i < args.Count; ++i)
                {
                    asm.Add($"PUSH {args[i]}");
                }
            }

            asm.Add($"CALL {functionAddress}");
            asm.Add("RET");

            return InjectAndExecute(asm.ToArray(), readReturnBytes, out byte[] bytes) ? bytes : null;
        }
    }
}