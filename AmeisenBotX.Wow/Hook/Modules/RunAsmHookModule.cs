using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Hook.Modules
{
    public abstract class RunAsmHookModule : IHookModule
    {
        public RunAsmHookModule(Action<nint> onUpdate, Action<IHookModule> tick, WowMemoryApi memory, uint allocSize)
        {
            Memory = memory;
            AllocSize = allocSize;
            OnDataUpdate = onUpdate;
            Tick = tick;
        }

        ~RunAsmHookModule()
        {
            if (AsmAddress != nint.Zero) { Memory.FreeMemory(AsmAddress); }
        }

        public nint AsmAddress { get; set; }

        public Action<nint> OnDataUpdate { get; set; }

        public Action<IHookModule> Tick { get; set; }

        protected uint AllocSize { get; }

        protected WowMemoryApi Memory { get; }

        public abstract nint GetDataPointer();

        public virtual bool Inject()
        {
            if (PrepareAsm(out IEnumerable<string> assembly)
                && Memory.AllocateMemory(AllocSize, out nint address))
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