using AmeisenBotX.Memory;
using System;

namespace AmeisenBotX.Core.Hook.Modules
{
    public abstract class RunAsmHookModule : IHookModule
    {
        public RunAsmHookModule(Action<IntPtr> onUpdate, Action tick, XMemory xMemory, uint allocSize)
        {
            XMemory = xMemory;
            AllocSize = allocSize;
            OnDataUpdate = onUpdate;
            Tick = tick;
        }

        ~RunAsmHookModule()
        {
            if (AsmAddress != IntPtr.Zero) { XMemory.FreeMemory(AsmAddress); }
        }

        public IntPtr AsmAddress { get; set; }

        public Action<IntPtr> OnDataUpdate { get; set; }

        public Action Tick { get; set; }

        protected uint AllocSize { get; }

        protected XMemory XMemory { get; }

        public abstract IntPtr GetDataPointer();

        public virtual bool Inject()
        {
            if (PrepareAsm()
                && XMemory.AllocateMemory(AllocSize, out IntPtr address))
            {
                AsmAddress = address;
                return XMemory.FasmInject(address);
            }
            else
            {
                return false;
            }
        }

        protected abstract bool PrepareAsm();
    }
}