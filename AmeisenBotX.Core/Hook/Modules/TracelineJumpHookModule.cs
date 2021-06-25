using AmeisenBotX.Memory;
using System;
using System.Text;

namespace AmeisenBotX.Core.Hook.Modules
{
    public class TracelineJumpHookModule : RunAsmHookModule
    {
        public TracelineJumpHookModule(Action<IntPtr> onUpdate, Action tick, WowInterface wowInterface) : base(onUpdate, tick, wowInterface.XMemory, 256)
        {
            WowInterface = wowInterface;
        }

        ~TracelineJumpHookModule()
        {
            if(CommandAddress != IntPtr.Zero) { WowInterface.XMemory.FreeMemory(CommandAddress); }
            if (DataAddress != IntPtr.Zero) { WowInterface.XMemory.FreeMemory(DataAddress); }
            if (ExecuteAddress != IntPtr.Zero) { WowInterface.XMemory.FreeMemory(ExecuteAddress); }
        }

        public IntPtr CommandAddress { get; private set; }

        public IntPtr DataAddress { get; private set; }

        public IntPtr ExecuteAddress { get; private set; }

        private WowInterface WowInterface { get; }

        public override IntPtr GetDataPointer()
        {
            return IntPtr.Zero;
        }

        protected override bool PrepareAsm()
        {
            byte[] luaJumpBytes = Encoding.ASCII.GetBytes("JumpOrAscendStart();AscendStop()");

            if (XMemory.AllocateMemory(4, out IntPtr executeAddress)
                && XMemory.AllocateMemory(40, out IntPtr dataAddress)
                && XMemory.AllocateMemory((uint)(luaJumpBytes.Length + 1), out IntPtr commandAddress))
            {
                WowInterface.XMemory.WriteBytes(commandAddress, luaJumpBytes);

                ExecuteAddress = executeAddress;
                CommandAddress = commandAddress;
                DataAddress = dataAddress;

                IntPtr distancePointer = dataAddress;
                IntPtr startPointer = IntPtr.Add(distancePointer, 0x4);
                IntPtr endPointer = IntPtr.Add(startPointer, 0xC);
                IntPtr resultPointer = IntPtr.Add(endPointer, 0xC);

                XMemory.Fasm.Clear();

                XMemory.Fasm.AppendLine("X:");
                XMemory.Fasm.AppendLine($"TEST DWORD [{executeAddress}], 1");
                XMemory.Fasm.AppendLine("JE @out");

                XMemory.Fasm.AppendLine("PUSH 0");
                XMemory.Fasm.AppendLine("PUSH 0x120171");
                XMemory.Fasm.AppendLine($"PUSH {distancePointer}");
                XMemory.Fasm.AppendLine($"PUSH {resultPointer}");
                XMemory.Fasm.AppendLine($"PUSH {endPointer}");
                XMemory.Fasm.AppendLine($"PUSH {startPointer}");
                XMemory.Fasm.AppendLine($"CALL {WowInterface.OffsetList.FunctionTraceline}");
                XMemory.Fasm.AppendLine("ADD ESP, 0x18");

                XMemory.Fasm.AppendLine("TEST AL, 1");
                XMemory.Fasm.AppendLine("JE @out");

                XMemory.Fasm.AppendLine("PUSH 0");
                XMemory.Fasm.AppendLine($"PUSH {commandAddress}");
                XMemory.Fasm.AppendLine($"PUSH {commandAddress}");
                XMemory.Fasm.AppendLine($"CALL {WowInterface.OffsetList.FunctionLuaDoString}");
                XMemory.Fasm.AppendLine("ADD ESP, 0xC");

                XMemory.Fasm.AppendLine($"MOV DWORD [{executeAddress}], 0");
                XMemory.Fasm.AppendLine("@out:");
                XMemory.Fasm.AppendLine("RET");

                return true;
            }

            return false;
        }
    }
}