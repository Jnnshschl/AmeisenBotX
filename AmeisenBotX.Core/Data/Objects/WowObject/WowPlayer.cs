using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowPlayer : WowUnit
    {
        public int Exp { get; set; }
        public int MaxExp { get; set; }

        public WowClass Class { get; set; }
        public WowRace Race { get; set; }
    }
}
