using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Data.Objects.WowObjects
{
    public class WowGUID
    {
        public static int NpcId(ulong guid)
        {
            return (int) ((guid >> 24) & 0x0000000000FFFFFF);
        }
    }
}
