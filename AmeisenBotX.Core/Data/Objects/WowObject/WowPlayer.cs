using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs.SubStructs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Memory;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowPlayer : WowUnit
    {
        public WowPlayer(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public int ComboPoints { get; set; }

        public bool IsFlying { get; set; }

        public bool IsSwimming { get; set; }

        public bool IsUnderwater { get; set; }

        public int NextLevelXp { get; set; }

        public int Xp { get; set; }

        public double XpPercentage { get; set; }

        public List<VisibleItemEnchantment> GetItemEnchantments { get; set; }

        public List<QuestlogEntry> GetQuestlogEntries { get; set; }

        public bool IsAlliance()
        {
            return Race == WowRace.Draenei
                        || Race == WowRace.Human
                        || Race == WowRace.Dwarf
                        || Race == WowRace.Gnome
                        || Race == WowRace.Nightelf;
        }

        public bool IsHorde()
        {
            return Race == WowRace.Undead
                       || Race == WowRace.Orc
                       || Race == WowRace.Bloodelf
                       || Race == WowRace.Tauren
                       || Race == WowRace.Troll;
        }

        public override string ToString()
        {
            return $"Player: [{Guid}] {Name} lvl. {Level}";
        }

        public WowPlayer UpdateRawWowPlayer()
        {
            UpdateRawWowUnit();

            unsafe
            {
                fixed (RawWowPlayer* objPtr = stackalloc RawWowPlayer[1])
                {
                    if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset + RawWowPlayer.EndOffset, objPtr))
                    {
                        Xp = objPtr[0].Xp;
                        NextLevelXp = objPtr[0].NextLevelXp;

                        XpPercentage = BotMath.Percentage(Xp, NextLevelXp);

                        GetQuestlogEntries = new List<QuestlogEntry>()
                        {
                            objPtr[0].QuestlogEntry1,
                            objPtr[0].QuestlogEntry2,
                            objPtr[0].QuestlogEntry3,
                            objPtr[0].QuestlogEntry4,
                            objPtr[0].QuestlogEntry5,
                            objPtr[0].QuestlogEntry6,
                            objPtr[0].QuestlogEntry7,
                            objPtr[0].QuestlogEntry8,
                            objPtr[0].QuestlogEntry9,
                            objPtr[0].QuestlogEntry10,
                            objPtr[0].QuestlogEntry11,
                            objPtr[0].QuestlogEntry12,
                            objPtr[0].QuestlogEntry13,
                            objPtr[0].QuestlogEntry14,
                            objPtr[0].QuestlogEntry15,
                            objPtr[0].QuestlogEntry16,
                            objPtr[0].QuestlogEntry17,
                            objPtr[0].QuestlogEntry18,
                            objPtr[0].QuestlogEntry19,
                            objPtr[0].QuestlogEntry20,
                            objPtr[0].QuestlogEntry21,
                            objPtr[0].QuestlogEntry22,
                            objPtr[0].QuestlogEntry23,
                            objPtr[0].QuestlogEntry24,
                            objPtr[0].QuestlogEntry25,
                        };

                        GetItemEnchantments = new List<VisibleItemEnchantment>()
                        {
                            objPtr[0].VisibleItemEnchantment1,
                            objPtr[0].VisibleItemEnchantment2,
                            objPtr[0].VisibleItemEnchantment3,
                            objPtr[0].VisibleItemEnchantment4,
                            objPtr[0].VisibleItemEnchantment5,
                            objPtr[0].VisibleItemEnchantment6,
                            objPtr[0].VisibleItemEnchantment7,
                            objPtr[0].VisibleItemEnchantment8,
                            objPtr[0].VisibleItemEnchantment9,
                            objPtr[0].VisibleItemEnchantment10,
                            objPtr[0].VisibleItemEnchantment11,
                            objPtr[0].VisibleItemEnchantment12,
                            objPtr[0].VisibleItemEnchantment13,
                            objPtr[0].VisibleItemEnchantment14,
                            objPtr[0].VisibleItemEnchantment15,
                            objPtr[0].VisibleItemEnchantment16,
                            objPtr[0].VisibleItemEnchantment17,
                            objPtr[0].VisibleItemEnchantment18,
                            objPtr[0].VisibleItemEnchantment19,
                        };
                    }
                }
            }

            if (WowInterface.I.XMemory.Read(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.WowUnitSwimFlags), out uint swimFlags))
            {
                IsSwimming = (swimFlags & 0x200000) != 0;
            }

            if (WowInterface.I.XMemory.Read(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.WowUnitFlyFlagsPointer), out IntPtr flyFlagsPointer)
            && WowInterface.I.XMemory.Read(IntPtr.Add(flyFlagsPointer, (int)WowInterface.I.OffsetList.WowUnitFlyFlags), out uint flyFlags))
            {
                IsFlying = (flyFlags & 0x2000000) != 0;
            }

            if (WowInterface.I.XMemory.Read(WowInterface.I.OffsetList.BreathTimer, out int breathTimer))
            {
                IsUnderwater = breathTimer > 0;
            }

            return this;
        }
    }
}