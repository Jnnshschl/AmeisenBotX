using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowGameobject : WowObject
    {
        public WowGameobjectType GameobjectType { get; set; }
        public string Name { get; set; }
    }
}
