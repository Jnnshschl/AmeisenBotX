using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Memory;
using System;
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

        protected override bool PrepareAsm()
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

                MemoryApi.AssemblyBuffer.Clear();

                MemoryApi.AssemblyBuffer.AppendLine("X:");
                MemoryApi.AssemblyBuffer.AppendLine($"TEST DWORD [{ExecuteAddress}], 1");
                MemoryApi.AssemblyBuffer.AppendLine("JE @out");

                MemoryApi.AssemblyBuffer.AppendLine("PUSH 0");
                MemoryApi.AssemblyBuffer.AppendLine("PUSH 0x120171");
                MemoryApi.AssemblyBuffer.AppendLine($"PUSH {distancePointer}");
                MemoryApi.AssemblyBuffer.AppendLine($"PUSH {resultPointer}");
                MemoryApi.AssemblyBuffer.AppendLine($"PUSH {endPointer}");
                MemoryApi.AssemblyBuffer.AppendLine($"PUSH {startPointer}");
                MemoryApi.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionTraceline}");
                MemoryApi.AssemblyBuffer.AppendLine("ADD ESP, 0x18");

                MemoryApi.AssemblyBuffer.AppendLine("TEST AL, 1");
                MemoryApi.AssemblyBuffer.AppendLine("JE @out");

                MemoryApi.AssemblyBuffer.AppendLine("PUSH 0");
                MemoryApi.AssemblyBuffer.AppendLine($"PUSH {CommandAddress}");
                MemoryApi.AssemblyBuffer.AppendLine($"PUSH {CommandAddress}");
                MemoryApi.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionLuaDoString}");
                MemoryApi.AssemblyBuffer.AppendLine("ADD ESP, 0xC");

                MemoryApi.AssemblyBuffer.AppendLine($"MOV DWORD [{ExecuteAddress}], 0");
                MemoryApi.AssemblyBuffer.AppendLine("@out:");
                MemoryApi.AssemblyBuffer.AppendLine("RET");

                return true;
            }

            return false;
        }
    }
}