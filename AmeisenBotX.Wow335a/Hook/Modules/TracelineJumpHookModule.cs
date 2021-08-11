﻿using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Offsets;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Hook.Modules
{
    public class TracelineJumpHookModule : RunAsmHookModule
    {
        public TracelineJumpHookModule(Action<IntPtr> onUpdate, Action<IHookModule> tick, IMemoryApi memoryApi, IOffsetList offsetList) : base(onUpdate, tick, memoryApi, 256)
        {
            OffsetList = offsetList;
        }

        ~TracelineJumpHookModule()
        {
            if (ExecuteAddress != IntPtr.Zero) { MemoryApi.FreeMemory(ExecuteAddress); }
        }

        public IntPtr CommandAddress { get; private set; }

        public IntPtr DataAddress { get; private set; }

        public IntPtr ExecuteAddress { get; private set; }

        private IOffsetList OffsetList { get; }

        public override IntPtr GetDataPointer()
        {
            return DataAddress;
        }

        protected override bool PrepareAsm(out IEnumerable<string> assembly)
        {
            byte[] luaJumpBytes = Encoding.ASCII.GetBytes("JumpOrAscendStart();AscendStop()");

            uint memoryNeeded = (uint)(4 + 40 + luaJumpBytes.Length + 1);

            if (MemoryApi.AllocateMemory(memoryNeeded, out IntPtr memory))
            {
                ExecuteAddress = memory;
                CommandAddress = ExecuteAddress + 4;
                DataAddress = CommandAddress + 40;

                MemoryApi.WriteBytes(CommandAddress, luaJumpBytes);

                IntPtr distancePointer = DataAddress;
                IntPtr startPointer = IntPtr.Add(distancePointer, 0x4);
                IntPtr endPointer = IntPtr.Add(startPointer, 0xC);
                IntPtr resultPointer = IntPtr.Add(endPointer, 0xC);

                assembly = new List<string>()
                {
                    "X:",
                    $"TEST DWORD [{ExecuteAddress}], 1",
                    "JE @out",
                    "PUSH 0",
                    "PUSH 0x120171",
                    $"PUSH {distancePointer}",
                    $"PUSH {resultPointer}",
                    $"PUSH {endPointer}",
                    $"PUSH {startPointer}",
                    $"CALL {OffsetList.FunctionTraceline}",
                    "ADD ESP, 0x18",
                    "TEST AL, 1",
                    "JE @out",
                    "PUSH 0",
                    $"PUSH {CommandAddress}",
                    $"PUSH {CommandAddress}",
                    $"CALL {OffsetList.FunctionLuaDoString}",
                    "ADD ESP, 0xC",
                    $"MOV DWORD [{ExecuteAddress}], 0",
                    "@out:",
                    "RET"
                };

                return true;
            }

            assembly = Array.Empty<string>();
            return false;
        }
    }
}