using System;

namespace AmeisenBotX.Core.Hook.Modules
{
    public interface IHookModule
    {
        IntPtr AsmAddress { get; }

        Action<IntPtr> OnDataUpdate { get; set; }

        Action Tick { get; set; }

        IntPtr GetDataPointer();

        bool Inject();
    }
}