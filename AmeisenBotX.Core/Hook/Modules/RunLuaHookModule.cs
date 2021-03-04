using AmeisenBotX.Memory;
using System;
using System.Text;

namespace AmeisenBotX.Core.Hook.Modules
{
    public class RunLuaHookModule : RunAsmHookModule
    {
        public RunLuaHookModule(Action<IntPtr> onUpdate, Action tick, WowInterface wowInterface, string lua, string varName, uint allocSize = 128) : base(onUpdate, tick, wowInterface.XMemory, allocSize)
        {
            WowInterface = wowInterface;
            Lua = lua;
            VarName = varName;
        }

        public IntPtr CommandAddress { get; private set; }

        public IntPtr ReturnAddress { get; private set; }

        public IntPtr VarAddress { get; private set; }

        private string Lua { get; }

        private Action TickAction { get; }

        private string VarName { get; }

        private WowInterface WowInterface { get; }

        public override IntPtr GetDataPointer()
        {
            if (WowInterface.XMemory.Read(ReturnAddress, out IntPtr pString))
            {
                return pString;
            }

            return IntPtr.Zero;
        }

        protected override bool PrepareAsm()
        {
            byte[] luaBytes = Encoding.ASCII.GetBytes(Lua);
            byte[] luaVarBytes = Encoding.ASCII.GetBytes(VarName);

            if (XMemory.AllocateMemory(4, out IntPtr returnAddress)
                && XMemory.AllocateMemory((uint)(luaBytes.Length + 1), out IntPtr commandAddress)
                && XMemory.AllocateMemory((uint)(luaVarBytes.Length + 1), out IntPtr varAddress))
            {
                XMemory.WriteBytes(commandAddress, luaBytes);
                XMemory.WriteBytes(varAddress, luaVarBytes);

                ReturnAddress = returnAddress;
                CommandAddress = commandAddress;
                VarAddress = varAddress;

                XMemory.Fasm.Clear();

                XMemory.Fasm.AppendLine("X:");

                XMemory.Fasm.AppendLine("PUSH 0");
                XMemory.Fasm.AppendLine($"PUSH {commandAddress}");
                XMemory.Fasm.AppendLine($"PUSH {commandAddress}");
                XMemory.Fasm.AppendLine($"CALL {WowInterface.OffsetList.FunctionLuaDoString}");
                XMemory.Fasm.AppendLine("ADD ESP, 0xC");

                XMemory.Fasm.AppendLine($"CALL {WowInterface.OffsetList.FunctionGetActivePlayerObject}");
                XMemory.Fasm.AppendLine("MOV ECX, EAX");
                XMemory.Fasm.AppendLine("PUSH -1");
                XMemory.Fasm.AppendLine($"PUSH {varAddress}");
                XMemory.Fasm.AppendLine($"CALL {WowInterface.OffsetList.FunctionGetLocalizedText}");
                XMemory.Fasm.AppendLine($"MOV DWORD [{returnAddress}], EAX");

                XMemory.Fasm.AppendLine($"RET");

                return true;
            }

            return false;
        }
    }
}