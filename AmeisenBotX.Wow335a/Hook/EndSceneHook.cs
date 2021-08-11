using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Hook.Modules;
using AmeisenBotX.Core.Hook.Structs;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow335a.Objects;
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

        private int hookCalls;

        public EndSceneHook(IMemoryApi memoryApi, IOffsetList offsetList, ObjectManager objectManager)
        {
            Memory = memoryApi;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            OriginalFunctionBytes = new();
        }

        internal event Action<GameInfo> OnGameInfoPush;

        public int HookCallCount
        {
            get
            {
                unchecked
                {
                    int val = hookCalls;
                    hookCalls = 0;
                    return val;
                }
            }
        }

        public bool IsWoWHooked => WowEndSceneAddress != IntPtr.Zero && Memory.Read(WowEndSceneAddress, out byte c) && c == 0xE9;

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

        /// <summary>
        /// Whether the hook should ignore if the world is not loaded or not.
        /// Used in the login screen as the world isnt loaded there.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BotOverrideWorldLoadedCheck(bool status)
        {
            Memory.Write(OverrideWorldCheckAddress, status ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CallObjectFunction(IntPtr objectBaseAddress, IntPtr functionAddress, List<object> args = null)
        {
            return CallObjectFunction(objectBaseAddress, functionAddress, args, false, out _);
        }

        /// <summary>
        /// Use this to call a thiscall function of a wowobject
        /// </summary>
        /// <param name="objectBaseAddress">Object base</param>
        /// <param name="functionAddress">Function to call</param>
        /// <param name="args">Arguments, can be null</param>
        /// <param name="readReturnBytes">Whether to read the retunr address or not</param>
        /// <param name="returnAddress">Return address</param>
        /// <returns>True if everything went right, false if not</returns>
        public bool CallObjectFunction(IntPtr objectBaseAddress, IntPtr functionAddress, List<object> args, bool readReturnBytes, out IntPtr returnAddress)
        {
#if DEBUG
            if (objectBaseAddress == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(objectBaseAddress), "objectBaseAddress is an invalid pointer"); }
            if (functionAddress == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(functionAddress), "functionAddress is an invalid pointer"); }
#endif
            List<string> asm = new() { $"MOV ECX, {objectBaseAddress}" };

            if (args != null)
            {
                for (int i = 0; i < args.Count; ++i)
                {
                    asm.Add($"PUSH {args[i]}");
                }
            }

            asm.Add($"CALL {functionAddress}");
            asm.Add("RET");

            if (readReturnBytes)
            {
                bool status = InjectAndExecute(asm, readReturnBytes, out returnAddress);
                return status;
            }

            returnAddress = IntPtr.Zero;
            return InjectAndExecute(asm, readReturnBytes, out _);
        }

        public bool Hook(int hookSize, List<IHookModule> hookModules)
        {
            if (hookSize < 0x5) { throw new ArgumentOutOfRangeException(nameof(hookSize), "cannot be smaller than 5"); }

            HookModules = hookModules;

            AmeisenLogger.I.Log("HookManager", $"Setting up the EndsceneHook (hookSize: {hookSize})", LogLevel.Verbose);

            do
            {
                try
                {
                    WowEndSceneAddress = GetEndScene();
                    AmeisenLogger.I.Log("HookManager", $"Endscene is at: 0x{WowEndSceneAddress.ToInt32():X}", LogLevel.Verbose);
                }
                catch
                {
                    // ignored, as we expect it to fail here atleast once cause wows startup takes some time
                    //AmeisenLogger.I.Log("HookManager", "Failed to read EndScene Address...", LogLevel.Verbose);
                }

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

            if (!AllocateCodeCaves())
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

            List<string> assemblyBuffer = new();

            // check for code to be executed
            assemblyBuffer.Add($"TEST DWORD [{IntShouldExecute}], 1");
            assemblyBuffer.Add("JE @out");

            // check if we want to override our is ingame check
            // going to be used while we are in the login screen
            assemblyBuffer.Add($"TEST DWORD [{OverrideWorldCheckAddress}], 1");
            assemblyBuffer.Add("JNE @ovr");

            // check for world to be loaded
            // we dont want to execute code in
            // the loadingscreen, cause that
            // mostly results in crashes
            assemblyBuffer.Add($"TEST DWORD [{OffsetList.IsWorldLoaded}], 1");
            assemblyBuffer.Add("JE @out");
            assemblyBuffer.Add("@ovr:");

            // execute our stuff and get return address
            assemblyBuffer.Add($"CALL {CExecution}");
            assemblyBuffer.Add($"MOV [{ReturnValueAddress}], EAX");

            // finish up our execution
            assemblyBuffer.Add("@out:");
            assemblyBuffer.Add($"MOV DWORD [{IntShouldExecute}], 0");

            // ----------------------------
            // # GameInfo & EventHook stuff
            // ----------------------------
            // world loaded and should execute check
            assemblyBuffer.Add($"TEST DWORD [{OffsetList.IsWorldLoaded}], 1");
            assemblyBuffer.Add("JE @skpgi");
            assemblyBuffer.Add($"TEST DWORD [{GameInfoExecuteAddress}], 1");
            assemblyBuffer.Add("JE @skpgi");

            // isOutdoors
            assemblyBuffer.Add($"CALL {OffsetList.FunctionGetActivePlayerObject}");
            assemblyBuffer.Add("MOV ECX, EAX");
            assemblyBuffer.Add($"CALL {OffsetList.FunctionIsOutdoors}");
            assemblyBuffer.Add($"MOV BYTE [{GameInfoAddress}], AL");

            // isTargetInLineOfSight
            assemblyBuffer.Add($"MOV DWORD [{GameInfoAddress.ToInt32() + 1}], 0");

            assemblyBuffer.Add($"TEST DWORD [{GameInfoExecuteLosCheckAddress}], 1");
            assemblyBuffer.Add("JE @loscheck");

            IntPtr distancePointer = GameInfoLosCheckDataAddress;
            IntPtr startPointer = IntPtr.Add(distancePointer, 0x4);
            IntPtr endPointer = IntPtr.Add(startPointer, 0xC);
            IntPtr resultPointer = IntPtr.Add(endPointer, 0xC);

            assemblyBuffer.Add("PUSH 0");
            assemblyBuffer.Add("PUSH 0x120171");
            assemblyBuffer.Add($"PUSH {distancePointer}");
            assemblyBuffer.Add($"PUSH {resultPointer}");
            assemblyBuffer.Add($"PUSH {endPointer}");
            assemblyBuffer.Add($"PUSH {startPointer}");
            assemblyBuffer.Add($"CALL {OffsetList.FunctionTraceline}");
            assemblyBuffer.Add("ADD ESP, 0x18");

            assemblyBuffer.Add($"MOV DWORD [{GameInfoAddress.ToInt32() + 1}], EAX");
            assemblyBuffer.Add($"MOV DWORD [{GameInfoExecuteLosCheckAddress}], 0");

            assemblyBuffer.Add("@loscheck:");

            if (hookModules != null)
            {
                foreach (IHookModule module in hookModules)
                {
                    assemblyBuffer.Add($"CALL {module.AsmAddress}");
                    AmeisenLogger.I.Log("HookManager", $"Loading module: {module.GetType().Name}", LogLevel.Verbose);
                }
            }

            assemblyBuffer.Add($"MOV DWORD [{GameInfoExecutedAddress}], 1");

            assemblyBuffer.Add("@skpgi:");
            assemblyBuffer.Add($"MOV DWORD [{GameInfoExecuteAddress}], 0");
            // ----------------

            assemblyBuffer.Add($"JMP {CGateway}");

            if (!Memory.InjectAssembly(assemblyBuffer, CRoutine))
            {
                Memory.ResumeMainThread();
                AmeisenLogger.I.Log("HookManager", $"Failed to inject hook check", LogLevel.Error);
                return false;
            }

            assemblyBuffer.Clear();

            // ---------------------------------------------------
            // End of the code that checks if there is asm to be
            // executed on our hook
            // ---------------------------------------------------

            // write the original EndScene instructions
            Memory.WriteBytes(CGateway, OriginalEndsceneBytes);
            assemblyBuffer.Add($"JMP {IntPtr.Add(WowEndSceneAddress, hookSize)}");

            // jump back to the original EndScene
            if (!Memory.InjectAssembly(assemblyBuffer, CGateway + OriginalEndsceneBytes.Length))
            {
                Memory.ResumeMainThread();
                AmeisenLogger.I.Log("HookManager", $"Failed to inject hook check", LogLevel.Error);
                return false;
            }

            assemblyBuffer.Clear();

            // ---------------------------------------------------
            // End of doing the original stuff and returning to
            // the original instruction
            // ---------------------------------------------------

            // modify original EndScene instructions to start the hook
            assemblyBuffer.Add($"JMP {CRoutine}");

            // for (int i = 5; i < hookSize; ++i)
            // {
            //     assemblyBuffer.Add("NOP");
            // }

            // suspend wows main thread and inject
            Memory.SuspendMainThread();

            if (!Memory.InjectAssembly(assemblyBuffer, WowEndSceneAddress, true))
            {
                Memory.ResumeMainThread();
                AmeisenLogger.I.Log("HookManager", $"Failed to modify original endscene bytes", LogLevel.Error);
                return false;
            }

            Memory.ResumeMainThread();

            AmeisenLogger.I.Log("HookManager", "EndsceneHook successful", LogLevel.Verbose);
            return IsWoWHooked;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LuaAbandonQuestsNotIn(IEnumerable<string> questNames)
        {
            if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetNumQuestLogEntries()"), out string r1)
                && int.TryParse(r1, out int numQuestLogEntries))
            {
                for (int i = 1; i <= numQuestLogEntries; i++)
                {
                    if (ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetQuestLogTitle({i})"), out string questLogTitle) && !questNames.Contains(questLogTitle))
                    {
                        LuaDoString($"SelectQuestLogEntry({i});SetAbandonQuest();AbandonQuest()");
                        break;
                    }
                }
            }
        }

        public bool LuaDoString(string command)
        {
#if DEBUG
            if (string.IsNullOrWhiteSpace(command)) { throw new ArgumentOutOfRangeException(nameof(command), "command is empty"); }
#endif
            AmeisenLogger.I.Log("335aHook", $"LuaDoString: {command}", LogLevel.Verbose);

            byte[] bytes = Encoding.UTF8.GetBytes(command + "\0");

            if (Memory.AllocateMemory((uint)bytes.Length, out IntPtr memAlloc))
            {
                try
                {
                    if (Memory.WriteBytes(memAlloc, bytes))
                    {
                        return InjectAndExecute(new string[]
                        {
                            "PUSH 0",
                            $"PUSH {memAlloc}",
                            $"PUSH {memAlloc}",
                            $"CALL {OffsetList.FunctionLuaDoString}",
                            "ADD ESP, 0xC",
                            "RET",
                        });
                    }
                }
                finally
                {
                    Memory.FreeMemory(memAlloc);
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ObjectRightClick(IntPtr objectBase)
        {
            CallObjectFunction(objectBase, OffsetList.FunctionGameobjectOnRightClick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFacing(IntPtr unitBase, float angle)
        {
            CallObjectFunction(unitBase, OffsetList.FunctionUnitSetFacing, new()
            {
                angle.ToString(CultureInfo.InvariantCulture).Replace(',', '.'),
                Environment.TickCount
            });
        }

        public void SetRenderState(bool renderingEnabled)
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

        public void TargetGuid(ulong guid)
        {
            byte[] guidBytes = BitConverter.GetBytes(guid);

            InjectAndExecute(new string[]
            {
                $"PUSH {BitConverter.ToUInt32(guidBytes, 4)}",
                $"PUSH {BitConverter.ToUInt32(guidBytes, 0)}",
                $"CALL {OffsetList.FunctionSetTarget}",
                "ADD ESP, 0x8",
                "RET"
            });
        }

        public bool TraceLine(Vector3 start, Vector3 end, uint flags = 0x120171)
        {
            if (Memory.AllocateMemory(40, out IntPtr tracelineCodecave))
            {
                try
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

                        if (InjectAndExecute(asm, true, out IntPtr returnAddress))
                        {
                            return returnAddress != IntPtr.Zero && (returnAddress.ToInt32() & 0xFF) == 0;
                        }
                    }
                }
                finally
                {
                    Memory.FreeMemory(tracelineCodecave);
                }
            }

            return false;
        }

        public void Unhook()
        {
            if (IsWoWHooked)
            {
                lock (hookLock)
                {
                    AmeisenLogger.I.Log("HookManager", "Disposing EndScene hook", LogLevel.Verbose);

                    Memory.SuspendMainThread();
                    Memory.WriteBytes(WowEndSceneAddress, OriginalEndsceneBytes);
                    Memory.ResumeMainThread();
                    Memory.FreeAllMemory();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InteractWithUnit(IntPtr unitBase)
        {
            CallObjectFunction(unitBase, OffsetList.FunctionUnitOnRightClick);
        }

        public void ClickOnTerrain(Vector3 position)
        {
            if (Memory.AllocateMemory(20, out IntPtr codeCaveVector3))
            {
                try
                {
                    if (Memory.Write(IntPtr.Add(codeCaveVector3, 8), position))
                    {
                        InjectAndExecute(new string[]
                        {
                            $"PUSH {codeCaveVector3.ToInt32()}",
                            $"CALL {OffsetList.FunctionHandleTerrainClick}",
                            "ADD ESP, 0x4",
                            "RET",
                        });
                    }
                }
                finally
                {
                    Memory.FreeMemory(codeCaveVector3);
                }
            }
        }

        public void ClickToMove(IntPtr playerBase, Vector3 position)
        {
            if (Memory.AllocateMemory(12, out IntPtr codeCaveVector3))
            {
                try
                {
                    if (Memory.Write(codeCaveVector3, position))
                    {
                        CallObjectFunction(playerBase, OffsetList.FunctionPlayerClickToMove, new() { codeCaveVector3 });
                    }
                }
                finally
                {
                    Memory.FreeMemory(codeCaveVector3);
                }
            }
        }

        public void EnableClickToMove()
        {
            if (Memory.Read(OffsetList.ClickToMovePointer, out IntPtr ctmPointer)
                && Memory.Read(IntPtr.Add(ctmPointer, (int)OffsetList.ClickToMoveEnabled), out int ctmEnabled)
                && ctmEnabled != 1)
            {
                Memory.Write(IntPtr.Add(ctmPointer, (int)OffsetList.ClickToMoveEnabled), 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExecuteLuaAndRead((string, string) cmdVarTuple, out string result)
        {
            return ExecuteLuaAndRead(cmdVarTuple.Item1, cmdVarTuple.Item2, out result);
        }

        public bool ExecuteLuaAndRead(string command, string variable, out string result)
        {
#if DEBUG
            if (string.IsNullOrWhiteSpace(command)) { throw new ArgumentOutOfRangeException(nameof(command), "command is empty"); }
            if (string.IsNullOrWhiteSpace(variable)) { throw new ArgumentOutOfRangeException(nameof(variable), "variable is empty"); }
#endif
            AmeisenLogger.I.Log("335aHook", $"WowExecuteLuaAndRead: {command}", LogLevel.Verbose);

            byte[] commandBytes = Encoding.UTF8.GetBytes(command + "\0");
            byte[] variableBytes = Encoding.UTF8.GetBytes(variable + "\0");

            if (Memory.AllocateMemory((uint)commandBytes.Length + (uint)variableBytes.Length, out IntPtr memAllocCmdVar))
            {
                try
                {
                    byte[] bytesToWrite = new byte[commandBytes.Length + variableBytes.Length];

                    Array.Copy(commandBytes, bytesToWrite, commandBytes.Length);
                    Array.Copy(variableBytes, 0, bytesToWrite, commandBytes.Length, variableBytes.Length);

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
                        $"PUSH {memAllocCmdVar + commandBytes.Length}",
                        $"CALL {OffsetList.FunctionGetLocalizedText}",
                        "RET",
                    };

                    if (InjectAndExecute(asm, true, out IntPtr returnAddress)
                        && Memory.ReadString(returnAddress, Encoding.UTF8, out result))
                    {
                        return !string.IsNullOrWhiteSpace(result);
                    }
                }
                finally
                {
                    Memory.FreeMemory(memAllocCmdVar);
                }
            }

            result = string.Empty;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FacePosition(IntPtr playerBase, Vector3 playerPosition, Vector3 positionToFace)
        {
            SetFacing(playerBase, BotMath.GetFacingAngle(playerPosition, positionToFace));
        }

        public bool GetLocalizedText(string variable, out string result)
        {
#if DEBUG
            if (string.IsNullOrWhiteSpace(variable)) { throw new ArgumentOutOfRangeException(nameof(variable), "variable is empty"); }
#endif
            if (!string.IsNullOrWhiteSpace(variable))
            {
                byte[] variableBytes = Encoding.UTF8.GetBytes(variable + "\0");

                if (Memory.AllocateMemory((uint)variableBytes.Length, out IntPtr memAlloc))
                {
                    try
                    {
                        Memory.WriteBytes(memAlloc, variableBytes);

                        string[] asm = new string[]
                        {
                            $"CALL {OffsetList.FunctionGetActivePlayerObject}",
                            "MOV ECX, EAX",
                            "PUSH -1",
                            $"PUSH {memAlloc}",
                            $"CALL {OffsetList.FunctionGetLocalizedText}",
                            "RET",
                        };

                        if (InjectAndExecute(asm, true, out IntPtr returnAddress)
                            && Memory.ReadString(returnAddress, Encoding.UTF8, out result))
                        {
                            return !string.IsNullOrWhiteSpace(result);
                        }
                    }
                    finally
                    {
                        Memory.FreeMemory(memAlloc);
                    }
                }
            }

            result = string.Empty;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetUnitReaction(IntPtr a, IntPtr b)
        {
#if DEBUG
            if (a == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(a), "a is no valid pointer"); }
            if (b == IntPtr.Zero) { throw new ArgumentOutOfRangeException(nameof(b), "b is no valid pointer"); }
#endif
            return CallObjectFunction(a, OffsetList.FunctionUnitGetReaction, new() { b }, true, out IntPtr ret)
                && ret != IntPtr.Zero ? ret.ToInt32() : 2;
        }

        private unsafe bool AllocateCodeCaves()
        {
            AmeisenLogger.I.Log("HookManager", "Allocating Codecaves", LogLevel.Verbose);

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

            return true;
        }

        private void DisableFunction(IntPtr address)
        {
            // check whether we already replaced the function or not
            if (Memory.Read(address, out byte opcode)
                && opcode != 0xC3)
            {
                SaveOriginalFunctionBytes(address);
                Memory.PatchMemory(address, (byte)0xC3);
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

        public void GameInfoTick()
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
                    // AmeisenLogger.I.Log("GameInfo", $"Pushing GameInfo Update: {JsonSerializer.Serialize(gameInfo, new() { IncludeFields = true })}");
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool InjectAndExecute(IEnumerable<string> asm)
        {
            return InjectAndExecute(asm, false, out _);
        }

        private bool InjectAndExecute(IEnumerable<string> asm, bool returns, out IntPtr returnAddress)
        {
            if (!IsWoWHooked)
            {
                returnAddress = IntPtr.Zero;
                return false;
            }

            lock (hookLock)
            {
                ++hookCalls;

                try
                {
                    // Memory.SuspendMainThread();
                    //
                    // try
                    // {
                    Memory.InjectAssembly(asm, CExecution);
                    Memory.Write(IntShouldExecute, 1);
                    // }
                    // finally
                    // {
                    //     Memory.ResumeMainThread();
                    // }

                    // wait for the code to be executed
                    while (Memory.Read(IntShouldExecute, out int c) && c == 1)
                    {
                        Thread.Sleep(1);
                    }

                    // if we want to read the return value do it otherwise we're done
                    if (!returns)
                    {
                        returnAddress = IntPtr.Zero;
                        return true;
                    }
                    else
                    {
                        return Memory.Read(ReturnValueAddress, out returnAddress) && returnAddress != IntPtr.Zero;
                    }
                }
                catch (Exception ex)
                {
                    AmeisenLogger.I.Log("Hook", $"Failed to InjectAndExecute:\n{ex}");
                    Memory.Write(IntShouldExecute, 0);
                }
            }

            returnAddress = IntPtr.Zero;
            return false;
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
    }
}