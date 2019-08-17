using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    [Flags]
    public enum WowUnitFlags : int
    {
        None = 0,
        Sitting = 0x1,
        NotAttackable = 0x2,
        Totem = 0x10,
        Looting = 0x400,
        PetInCombat = 0x800,
        PvpFlagged = 0x1000,
        Silenced = 0x4000,
        Combat = 0x80000,
        FlightmasterFlight = 0x100000,
        Disarmed = 0x200000,
        Confused = 0x400000,
        Fleeing = 0x800000,
        Mounted = 0x4000000,
        Skinnable = 0x8000000,
        Dazed = 0x20000000
    }
}
