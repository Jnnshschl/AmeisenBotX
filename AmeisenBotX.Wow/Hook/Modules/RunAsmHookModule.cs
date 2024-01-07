using System;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Hook.Modules
{
    public abstract class RunAsmHookModule(Action<nint> onUpdate, Action<IHookModule> tick, WowMemoryApi memory, uint allocSize) : IHookModule
    {
        ~RunAsmHookModule()
        {
            if (AsmAddress != nint.Zero) { Memory.FreeMemory(AsmAddress); }
        }

        public nint AsmAddress { get; set; }

        public Action<nint> OnDataUpdate { get; set; } = onUpdate;

        public Action<IHookModule> Tick { get; set; } = tick;

        protected uint AllocSize { get; } = allocSize;

        protected WowMemoryApi Memory { get; } = memory;

        public abstract nint GetDataPointer();

        public virtual bool Inject()
        {
            if (PrepareAsm(out IEnumerable<string> assembly)
                && Memory.AllocateMemory(AllocSize, out nint address))
            {
                AsmAddress = address;
                return Memory.InjectAssembly(assembly, address);
            }

            return false;
        }

        protected abstract bool PrepareAsm(out IEnumerable<string> assembly);
    }
}