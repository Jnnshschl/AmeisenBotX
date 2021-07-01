using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Memory;
using System;
using System.Text;

namespace AmeisenBotX.Core.Hook.Modules
{
    public class TracelineJumpHookModule : RunAsmHookModule
    {
        public TracelineJumpHookModule(Action<IntPtr> onUpdate, Action tick, IMemoryApi memoryApi, IOffsetList offsetList) : base(onUpdate, tick, memoryApi, 256)
        {
            OffsetList = offsetList;
        }

        ~TracelineJumpHookModule()
        {
            if (CommandAddress != IntPtr.Zero) { MemoryApi.FreeMemory(CommandAddress); }
            if (DataAddress != IntPtr.Zero) { MemoryApi.FreeMemory(DataAddress); }
            if (ExecuteAddress != IntPtr.Zero) { MemoryApi.FreeMemory(ExecuteAddress); }
        }

        public IntPtr CommandAddress { get; private set; }

        public IntPtr DataAddress { get; private set; }

        public IntPtr ExecuteAddress { get; private set; }

        private IOffsetList OffsetList { get; }

        public override IntPtr GetDataPointer()
        {
            return IntPtr.Zero;
        }

        protected override bool PrepareAsm()
        {
            byte[] luaJumpBytes = Encoding.ASCII.GetBytes("JumpOrAscendStart();AscendStop()");

            if (base.MemoryApi.AllocateMemory(4, out IntPtr executeAddress)
                && base.MemoryApi.AllocateMemory(40, out IntPtr dataAddress)
                && base.MemoryApi.AllocateMemory((uint)(luaJumpBytes.Length + 1), out IntPtr commandAddress))
            {
                base.MemoryApi.WriteBytes(commandAddress, luaJumpBytes);

                ExecuteAddress = executeAddress;
                CommandAddress = commandAddress;
                DataAddress = dataAddress;

                IntPtr distancePointer = dataAddress;
                IntPtr startPointer = IntPtr.Add(distancePointer, 0x4);
                IntPtr endPointer = IntPtr.Add(startPointer, 0xC);
                IntPtr resultPointer = IntPtr.Add(endPointer, 0xC);

                base.MemoryApi.AssemblyBuffer.Clear();

                base.MemoryApi.AssemblyBuffer.AppendLine("X:");
                base.MemoryApi.AssemblyBuffer.AppendLine($"TEST DWORD [{executeAddress}], 1");
                base.MemoryApi.AssemblyBuffer.AppendLine("JE @out");

                base.MemoryApi.AssemblyBuffer.AppendLine("PUSH 0");
                base.MemoryApi.AssemblyBuffer.AppendLine("PUSH 0x120171");
                base.MemoryApi.AssemblyBuffer.AppendLine($"PUSH {distancePointer}");
                base.MemoryApi.AssemblyBuffer.AppendLine($"PUSH {resultPointer}");
                base.MemoryApi.AssemblyBuffer.AppendLine($"PUSH {endPointer}");
                base.MemoryApi.AssemblyBuffer.AppendLine($"PUSH {startPointer}");
                base.MemoryApi.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionTraceline}");
                base.MemoryApi.AssemblyBuffer.AppendLine("ADD ESP, 0x18");

                base.MemoryApi.AssemblyBuffer.AppendLine("TEST AL, 1");
                base.MemoryApi.AssemblyBuffer.AppendLine("JE @out");

                base.MemoryApi.AssemblyBuffer.AppendLine("PUSH 0");
                base.MemoryApi.AssemblyBuffer.AppendLine($"PUSH {commandAddress}");
                base.MemoryApi.AssemblyBuffer.AppendLine($"PUSH {commandAddress}");
                base.MemoryApi.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionLuaDoString}");
                base.MemoryApi.AssemblyBuffer.AppendLine("ADD ESP, 0xC");

                base.MemoryApi.AssemblyBuffer.AppendLine($"MOV DWORD [{executeAddress}], 0");
                base.MemoryApi.AssemblyBuffer.AppendLine("@out:");
                base.MemoryApi.AssemblyBuffer.AppendLine("RET");

                return true;
            }

            return false;
        }
    }
}