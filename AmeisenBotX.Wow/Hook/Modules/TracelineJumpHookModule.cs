using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Wow.Hook.Modules
{
    public class TracelineJumpHookModule : RunAsmHookModule
    {
        public TracelineJumpHookModule(Action<nint> onUpdate, Action<IHookModule> tick, WowMemoryApi memory) : base(onUpdate, tick, memory, 256)
        {
        }

        ~TracelineJumpHookModule()
        {
            if (ExecuteAddress != nint.Zero) { Memory.FreeMemory(ExecuteAddress); }
        }

        public nint CommandAddress { get; private set; }

        public nint DataAddress { get; private set; }

        public nint ExecuteAddress { get; private set; }

        public override nint GetDataPointer()
        {
            return DataAddress;
        }

        protected override bool PrepareAsm(out IEnumerable<string> assembly)
        {
            byte[] luaJumpBytes = Encoding.ASCII.GetBytes("JumpOrAscendStart();AscendStop()");

            uint memoryNeeded = (uint)(4 + 40 + luaJumpBytes.Length + 1);

            if (Memory.AllocateMemory(memoryNeeded, out nint memory))
            {
                ExecuteAddress = memory;
                CommandAddress = ExecuteAddress + 4;
                DataAddress = CommandAddress + 40;

                Memory.WriteBytes(CommandAddress, luaJumpBytes);

                nint distancePointer = DataAddress;
                nint startPointer = nint.Add(distancePointer, 0x4);
                nint endPointer = nint.Add(startPointer, 0xC);
                nint resultPointer = nint.Add(endPointer, 0xC);

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
                    $"CALL {Memory.Offsets.FunctionTraceline}",
                    "ADD ESP, 0x18",
                    "TEST AL, 1",
                    "JE @out",
                    "PUSH 0",
                    $"PUSH {CommandAddress}",
                    $"PUSH {CommandAddress}",
                    $"CALL {Memory.Offsets.FunctionLuaDoString}",
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