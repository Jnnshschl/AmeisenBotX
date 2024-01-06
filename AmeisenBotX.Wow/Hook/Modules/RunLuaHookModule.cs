using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Wow.Hook.Modules
{
    public class RunLuaHookModule : RunAsmHookModule
    {
        public RunLuaHookModule(Action<nint> onUpdate, Action<IHookModule> tick, WowMemoryApi memory, string lua, string varName, uint allocSize = 128) : base(onUpdate, tick, memory, allocSize)
        {
            Lua = lua;
            VarName = varName;
        }

        ~RunLuaHookModule()
        {
            if (ReturnAddress != nint.Zero) { Memory.FreeMemory(ReturnAddress); }
        }

        public nint CommandAddress { get; private set; }

        public nint ReturnAddress { get; private set; }

        public nint VarAddress { get; private set; }

        private string Lua { get; }

        private string VarName { get; }

        public override nint GetDataPointer()
        {
            return Memory.Read(ReturnAddress, out nint pString) ? pString : nint.Zero;
        }

        protected override bool PrepareAsm(out IEnumerable<string> assembly)
        {
            byte[] luaBytes = Encoding.ASCII.GetBytes(Lua);
            byte[] luaVarBytes = Encoding.ASCII.GetBytes(VarName);

            uint memoryNeeded = (uint)(4 + luaBytes.Length + 1 + luaVarBytes.Length + 1);

            if (Memory.AllocateMemory(memoryNeeded, out nint memory))
            {
                ReturnAddress = memory;
                CommandAddress = ReturnAddress + 4;
                VarAddress = CommandAddress + luaBytes.Length + 1;

                Memory.WriteBytes(CommandAddress, luaBytes);
                Memory.WriteBytes(VarAddress, luaVarBytes);

                assembly = new List<string>()
                {
                    "X:",
                    "PUSH 0",
                    $"PUSH {CommandAddress}",
                    $"PUSH {CommandAddress}",
                    $"CALL {Memory.Offsets.FunctionLuaDoString}",
                    "ADD ESP, 0xC",
                    $"CALL {Memory.Offsets.FunctionGetActivePlayerObject}",
                    "MOV ECX, EAX",
                    "PUSH -1",
                    $"PUSH {VarAddress}",
                    $"CALL {Memory.Offsets.FunctionGetLocalizedText}",
                    $"MOV DWORD [{ReturnAddress}], EAX",
                    $"RET"
                };

                return true;
            }

            assembly = Array.Empty<string>();
            return false;
        }
    }
}