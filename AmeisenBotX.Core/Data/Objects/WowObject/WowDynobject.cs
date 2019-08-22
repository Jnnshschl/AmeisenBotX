using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowDynobject : WowGameobject
    {
        public ulong CasterGuid { get; set; }
        public int SpellId { get; set; }
        public float Radius { get; set; }
        public WowPosition Position { get; set; }
        public float Facing { get; set; }
    }
}
