using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Memory;
using System;
using System.Text;

namespace AmeisenBotX.Core.Hook.Modules
{
    public class RunLuaHookModule : RunAsmHookModule
    {
        public RunLuaHookModule(Action<IntPtr> onUpdate, Action tick, IMemoryApi memoryApi, IOffsetList offsetList, string lua, string varName, uint allocSize = 128) : base(onUpdate, tick, memoryApi, allocSize)
        {
            OffsetList = offsetList;
            Lua = lua;
            VarName = varName;
        }

        ~RunLuaHookModule()
        {
            if (CommandAddress != IntPtr.Zero) { MemoryApi.FreeMemory(CommandAddress); }
            if (ReturnAddress != IntPtr.Zero) { MemoryApi.FreeMemory(ReturnAddress); }
            if (VarAddress != IntPtr.Zero) { MemoryApi.FreeMemory(VarAddress); }
        }

        public IntPtr CommandAddress { get; private set; }

        public IntPtr ReturnAddress { get; private set; }

        public IntPtr VarAddress { get; private set; }

        private string Lua { get; }

        private IOffsetList OffsetList { get; }

        private string VarName { get; }

        public override IntPtr GetDataPointer()
        {
            if (MemoryApi.Read(ReturnAddress, out IntPtr pString))
            {
                return pString;
            }

            return IntPtr.Zero;
        }

        protected override bool PrepareAsm()
        {
            byte[] luaBytes = Encoding.ASCII.GetBytes(Lua);
            byte[] luaVarBytes = Encoding.ASCII.GetBytes(VarName);

            if (MemoryApi.AllocateMemory(4, out IntPtr returnAddress)
                && MemoryApi.AllocateMemory((uint)(luaBytes.Length + 1), out IntPtr commandAddress)
                && MemoryApi.AllocateMemory((uint)(luaVarBytes.Length + 1), out IntPtr varAddress))
            {
                MemoryApi.WriteBytes(commandAddress, luaBytes);
                MemoryApi.WriteBytes(varAddress, luaVarBytes);

                ReturnAddress = returnAddress;
                CommandAddress = commandAddress;
                VarAddress = varAddress;

                MemoryApi.AssemblyBuffer.Clear();

                MemoryApi.AssemblyBuffer.AppendLine("X:");

                MemoryApi.AssemblyBuffer.AppendLine("PUSH 0");
                MemoryApi.AssemblyBuffer.AppendLine($"PUSH {commandAddress}");
                MemoryApi.AssemblyBuffer.AppendLine($"PUSH {commandAddress}");
                MemoryApi.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionLuaDoString}");
                MemoryApi.AssemblyBuffer.AppendLine("ADD ESP, 0xC");

                MemoryApi.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionGetActivePlayerObject}");
                MemoryApi.AssemblyBuffer.AppendLine("MOV ECX, EAX");
                MemoryApi.AssemblyBuffer.AppendLine("PUSH -1");
                MemoryApi.AssemblyBuffer.AppendLine($"PUSH {varAddress}");
                MemoryApi.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionGetLocalizedText}");
                MemoryApi.AssemblyBuffer.AppendLine($"MOV DWORD [{returnAddress}], EAX");

                MemoryApi.AssemblyBuffer.AppendLine($"RET");

                return true;
            }

            return false;
        }
    }
}