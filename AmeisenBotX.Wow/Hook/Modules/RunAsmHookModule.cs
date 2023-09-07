using AmeisenBotX.Common.Memory;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Hook.Modules
{
    public abstract class RunAsmHookModule : IHookModule
    {
        public RunAsmHookModule(Action<IntPtr> onUpdate, Action<IHookModule> tick, IMemoryApi memory, uint allocSize)
        {
            Memory = memory;
            AllocSize = allocSize;
            OnDataUpdate = onUpdate;
            Tick = tick;
        }

        ~RunAsmHookModule()
        {
            if (AsmAddress != IntPtr.Zero) { Memory.FreeMemory(AsmAddress); }
        }

        public IntPtr AsmAddress { get; set; }

        public Action<IntPtr> OnDataUpdate { get; set; }

        public Action<IHookModule> Tick { get; set; }

        protected uint AllocSize { get; }

        protected IMemoryApi Memory { get; }

        public abstract IntPtr GetDataPointer();

        public virtual bool Inject()
        {
            if (PrepareAsm(out IEnumerable<string> assembly)
                && Memory.AllocateMemory(AllocSize, out IntPtr address))
            {
                AsmAddress = address;
                return Memory.InjectAssembly(assembly, address);
            }
            else
            {
                return false;
            }
        }

        protected abstract bool PrepareAsm(out IEnumerable<string> assembly);
    }
}