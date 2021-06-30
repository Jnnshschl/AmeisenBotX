using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Memory;
using System;
using System.Text;

namespace AmeisenBotX.Core.Hook.Modules
{
    public class TracelineJumpHookModule : RunAsmHookModule
    {
        public TracelineJumpHookModule(Action<IntPtr> onUpdate, Action tick, XMemory xMemory, IOffsetList offsetList) : base(onUpdate, tick, xMemory, 256)
        {
            OffsetList = offsetList;
        }

        ~TracelineJumpHookModule()
        {
            if (CommandAddress != IntPtr.Zero) { XMemory.FreeMemory(CommandAddress); }
            if (DataAddress != IntPtr.Zero) { XMemory.FreeMemory(DataAddress); }
            if (ExecuteAddress != IntPtr.Zero) { XMemory.FreeMemory(ExecuteAddress); }
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

            if (base.XMemory.AllocateMemory(4, out IntPtr executeAddress)
                && base.XMemory.AllocateMemory(40, out IntPtr dataAddress)
                && base.XMemory.AllocateMemory((uint)(luaJumpBytes.Length + 1), out IntPtr commandAddress))
            {
                base.XMemory.WriteBytes(commandAddress, luaJumpBytes);

                ExecuteAddress = executeAddress;
                CommandAddress = commandAddress;
                DataAddress = dataAddress;

                IntPtr distancePointer = dataAddress;
                IntPtr startPointer = IntPtr.Add(distancePointer, 0x4);
                IntPtr endPointer = IntPtr.Add(startPointer, 0xC);
                IntPtr resultPointer = IntPtr.Add(endPointer, 0xC);

                base.XMemory.Fasm.Clear();

                base.XMemory.Fasm.AppendLine("X:");
                base.XMemory.Fasm.AppendLine($"TEST DWORD [{executeAddress}], 1");
                base.XMemory.Fasm.AppendLine("JE @out");

                base.XMemory.Fasm.AppendLine("PUSH 0");
                base.XMemory.Fasm.AppendLine("PUSH 0x120171");
                base.XMemory.Fasm.AppendLine($"PUSH {distancePointer}");
                base.XMemory.Fasm.AppendLine($"PUSH {resultPointer}");
                base.XMemory.Fasm.AppendLine($"PUSH {endPointer}");
                base.XMemory.Fasm.AppendLine($"PUSH {startPointer}");
                base.XMemory.Fasm.AppendLine($"CALL {OffsetList.FunctionTraceline}");
                base.XMemory.Fasm.AppendLine("ADD ESP, 0x18");

                base.XMemory.Fasm.AppendLine("TEST AL, 1");
                base.XMemory.Fasm.AppendLine("JE @out");

                base.XMemory.Fasm.AppendLine("PUSH 0");
                base.XMemory.Fasm.AppendLine($"PUSH {commandAddress}");
                base.XMemory.Fasm.AppendLine($"PUSH {commandAddress}");
                base.XMemory.Fasm.AppendLine($"CALL {OffsetList.FunctionLuaDoString}");
                base.XMemory.Fasm.AppendLine("ADD ESP, 0xC");

                base.XMemory.Fasm.AppendLine($"MOV DWORD [{executeAddress}], 0");
                base.XMemory.Fasm.AppendLine("@out:");
                base.XMemory.Fasm.AppendLine("RET");

                return true;
            }

            return false;
        }
    }
}