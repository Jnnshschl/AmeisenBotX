using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Hook.Modules
{
    public interface IHookModule<T>
    {
        bool Inject();

        T Read();
    }
}
