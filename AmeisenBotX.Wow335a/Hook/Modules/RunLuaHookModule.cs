using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Memory;
using System;
using System.Text;

namespace AmeisenBotX.Core.Hook.Modules
{
    public class RunLuaHookModule : RunAsmHookModule
    {
        public RunLuaHookModule(Action<IntPtr> onUpdate, Action<IHookModule> tick, IMemoryApi memoryApi, IOffsetList offsetList, string lua, string varName, uint allocSize = 128) : base(onUpdate, tick, memoryApi, allocSize)
        {
            OffsetList = offsetList;
            Lua = lua;
            VarName = varName;
        }

        ~RunLuaHookModule()
        {
            if (ReturnAddress != IntPtr.Zero) { MemoryApi.FreeMemory(ReturnAddress); }
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

            uint memoryNeeded = (uint)(4 + luaBytes.Length + 1 + luaVarBytes.Length + 1);

            if (MemoryApi.AllocateMemory(memoryNeeded, out IntPtr memory))
            {
                ReturnAddress = memory;
                CommandAddress = ReturnAddress + 4;
                VarAddress = CommandAddress + luaBytes.Length + 1;

                MemoryApi.WriteBytes(CommandAddress, luaBytes);
                MemoryApi.WriteBytes(VarAddress, luaVarBytes);

                MemoryApi.AssemblyBuffer.Clear();

                MemoryApi.AssemblyBuffer.AppendLine("X:");

                MemoryApi.AssemblyBuffer.AppendLine("PUSH 0");
                MemoryApi.AssemblyBuffer.AppendLine($"PUSH {CommandAddress}");
                MemoryApi.AssemblyBuffer.AppendLine($"PUSH {CommandAddress}");
                MemoryApi.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionLuaDoString}");
                MemoryApi.AssemblyBuffer.AppendLine("ADD ESP, 0xC");

                MemoryApi.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionGetActivePlayerObject}");
                MemoryApi.AssemblyBuffer.AppendLine("MOV ECX, EAX");
                MemoryApi.AssemblyBuffer.AppendLine("PUSH -1");
                MemoryApi.AssemblyBuffer.AppendLine($"PUSH {VarAddress}");
                MemoryApi.AssemblyBuffer.AppendLine($"CALL {OffsetList.FunctionGetLocalizedText}");
                MemoryApi.AssemblyBuffer.AppendLine($"MOV DWORD [{ReturnAddress}], EAX");

                MemoryApi.AssemblyBuffer.AppendLine($"RET");

                return true;
            }

            return false;
        }
    }
}