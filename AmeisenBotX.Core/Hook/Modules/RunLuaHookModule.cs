using AmeisenBotX.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Hook.Modules
{
    public class RunLuaHookModule : RunAsmHookModule<string>
    {
        private string Lua { get; }

        private string VarName { get; }

        public IntPtr ReturnAddress { get; private set; }

        public IntPtr CommandAddress { get; private set; }

        public IntPtr VarAddress { get; private set; }

        private WowInterface WowInterface { get; }

        public RunLuaHookModule(WowInterface wowInterface, string lua, string varName, uint allocSize = 128) : base(wowInterface.XMemory, allocSize)
        {
            Lua = lua;
            VarName = varName;
            WowInterface = wowInterface;
        }

        public override string Read()
        {
            if (WowInterface.XMemory.Read(ReturnAddress, out IntPtr pString)
                && XMemory.ReadString(pString, Encoding.UTF8, out string s, 8192 * 8))
            {
                return s;
            }

            return string.Empty;
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
