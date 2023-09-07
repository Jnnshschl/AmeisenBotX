﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Memory;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow.Hook.Modules;
using AmeisenBotX.Wow.Hook.Structs;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AmeisenBotX.Wow.Hook
{
    public class GenericEndSceneHook
    {
        private const int MEM_ALLOC_EXECUTION_SIZE = 4096;
        private const int MEM_ALLOC_GATEWAY_SIZE = 24;
        private const int MEM_ALLOC_ROUTINE_SIZE = 256;
        private readonly object hookLock = new();

        private int hookCalls;

        public GenericEndSceneHook(IMemoryApi memory)
        {
            Memory = memory;
        }

        public event Action<GameInfo> OnGameInfoPush;

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

        protected IMemoryApi Memory { get; }

        /// <summary>
        /// Codecave that hold the code, the bot want's to execute.
        /// </summary>
        private IntPtr CExecution { get; set; }

        /// <summary>
        /// Codecave that hold the original EndScene instructions and jumps back to the original function.
        /// </summary>
        private IntPtr CGateway { get; set; }

        /// <summary>
        /// Codecave used to check whether the bot want's to execute code and run the IHookModule's.
        /// </summary>
        private IntPtr CRoutine { get; set; }

        /// <summary>
        /// Pointer to the GameInfo struct that contains various static information about wow that
        /// are needed on a regular basis.
        /// </summary>
        private IntPtr GameInfoAddress { get; set; }

        /// <summary>
        /// Integer that instructs wow to refresh the GameInfo.
        /// </summary>
        private IntPtr GameInfoExecuteAddress { get; set; }

        /// <summary>
        /// Integer tha will be set to 1 when wow finished refreshing the GameInfo data.
        /// </summary>
        private IntPtr GameInfoExecutedAddress { get; set; }

        /// <summary>
        /// Integer tha will be set to 1 when we want to execute the LOS check.
        /// </summary>
        private IntPtr GameInfoExecuteLosCheckAddress { get; set; }

        /// <summary>
        /// Integer tha will be set to 1 when we're able to perform the LOS check.
        /// </summary>
        private IntPtr GameInfoLosCheckDataAddress { get; set; }

        /// <summary>
        /// The currently loaded hookmodules
        /// </summary>
        private List<IHookModule> HookModules { get; set; }

        /// <summary>
        /// Integer that will be set to 1 if the bot wait's for code to be executed. Will be set to
        /// 0 when done.
        /// </summary>
        private IntPtr IntShouldExecute { get; set; }

        /// <summary>
        /// Save the original EndScene instructions that will be restored when the hook gets disposed.
        /// </summary>
        private byte[] OriginalEndsceneBytes { get; set; }

        /// <summary>
        /// Integer that is used to skip the world loaded check;
        /// </summary>
        private IntPtr OverrideWorldCheckAddress { get; set; }

        /// <summary>
        /// Pointer to the return value of the code executed on the EndScene hook.
        /// </summary>
        private IntPtr ReturnValueAddress { get; set; }

        /// <summary>
        /// The address of the EndScene function of wow.
        /// </summary>
        private IntPtr WowEndSceneAddress { get; set; }

        /// <summary>
        /// Whether the hook should ignore if the world is not loaded or not. Used in the login
        /// screen as the world isnt loaded there.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BotOverrideWorldLoadedCheck(bool status)
        {
            Memory.Write(OverrideWorldCheckAddress, status ? 1 : 0);
        }

        public void GameInfoTick(IWowUnit player, IWowUnit target)
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
                if (player != null && target != null)
                {
                    Vector3 playerPosition = player.Position;
                    playerPosition.Z += 1.5f;

                    Vector3 targetPosition = target.Position;
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
                if (Memory.Read(GameInfoAddress, out GameInfo gameInfo))
                {
                    OnGameInfoPush?.Invoke(gameInfo);
                    // AmeisenLogger.I.Log("GameInfo", $"Pushing GameInfo Update:
                    // {JsonSerializer.Serialize(gameInfo, new JsonSerializerOptions() {
                    // IncludeFields = true })}");
                }

                Memory.Write(GameInfoExecutedAddress, 0);

                foreach (IHookModule module in HookModules)
                {
                    module.OnDataUpdate?.Invoke(module.GetDataPointer());
                }
            }
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
                    // ignored, as we expect it to fail here atleast once, because wows start takes
                    // some time
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

            // check if we want to override our is ingame check going to be used while we are in the
            // login screen
            assemblyBuffer.Add($"TEST DWORD [{OverrideWorldCheckAddress}], 1");
            assemblyBuffer.Add("JNE @ovr");

            // check for world to be loaded we dont want to execute code in the loadingscreen, cause
            // that mostly results in crashes
            assemblyBuffer.Add($"TEST DWORD [{Memory.Offsets.IsWorldLoaded}], 1");
            assemblyBuffer.Add("JE @out");
            assemblyBuffer.Add("@ovr:");

            // execute our stuff and get return address
            assemblyBuffer.Add($"CALL {CExecution}");
            assemblyBuffer.Add($"MOV [{ReturnValueAddress}], EAX");

            // finish up our execution
            assemblyBuffer.Add("@out:");
            assemblyBuffer.Add($"MOV DWORD [{IntShouldExecute}], 0");

            // ---------------------------- # GameInfo & EventHook stuff
            // ---------------------------- world loaded and should execute check
            assemblyBuffer.Add($"TEST DWORD [{Memory.Offsets.IsWorldLoaded}], 1");
            assemblyBuffer.Add("JE @skpgi");
            assemblyBuffer.Add($"TEST DWORD [{GameInfoExecuteAddress}], 1");
            assemblyBuffer.Add("JE @skpgi");

            // isOutdoors
            assemblyBuffer.Add($"CALL {Memory.Offsets.FunctionGetActivePlayerObject}");
            assemblyBuffer.Add("MOV ECX, EAX");
            assemblyBuffer.Add($"CALL {Memory.Offsets.FunctionIsOutdoors}");
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
            assemblyBuffer.Add($"CALL {Memory.Offsets.FunctionTraceline}");
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

            // --------------------------------------------------- End of the code that checks if
            // there is asm to be executed on our hook ---------------------------------------------------

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

            // --------------------------------------------------- End of doing the original stuff
            // and returning to the original instruction ---------------------------------------------------

            // modify original EndScene instructions to start the hook
            assemblyBuffer.Add($"JMP {CRoutine}");

            // for (int i = 5; i < hookSize; ++i) { assemblyBuffer.Add("NOP"); }

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
        public bool InjectAndExecute(IEnumerable<string> asm)
        {
            return InjectAndExecute(asm, false, out _);
        }

        public bool InjectAndExecute(IEnumerable<string> asm, bool returns, out IntPtr returnAddress)
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
                    Memory.SuspendMainThread();

                    try
                    {
                        Memory.InjectAssembly(asm, CExecution);
                        Memory.Write(IntShouldExecute, 1);
                    }
                    finally
                    {
                        Memory.ResumeMainThread();
                    }

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

        private IntPtr GetEndScene()
        {
            if (Memory.Read(Memory.Offsets.EndSceneStaticDevice, out IntPtr pDevice)
                && Memory.Read(IntPtr.Add(pDevice, (int)Memory.Offsets.EndSceneOffsetDevice), out IntPtr pEnd)
                && Memory.Read(pEnd, out IntPtr pScene)
                && Memory.Read(IntPtr.Add(pScene, (int)Memory.Offsets.EndSceneOffset), out IntPtr pEndscene))
            {
                return pEndscene;
            }
            else
            {
                return IntPtr.Zero;
            }
        }
    }
}