using AmeisenBotX.Memory;
using System;

namespace AmeisenBotX.Core.Hook.Modules
{
    public abstract class RunAsmHookModule<T> : IHookModule<T>
    {
        public RunAsmHookModule(XMemory xMemory, uint allocSize)
        {
            XMemory = xMemory;
            AllocSize = allocSize;
        }

        protected uint AllocSize { get; }

        public IntPtr AsmAddress { get; set; }

        protected XMemory XMemory { get; }

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

        public abstract T Read();

        protected abstract bool PrepareAsm();
    }
}