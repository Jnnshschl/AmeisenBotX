﻿using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Character.Inventory.Objects
{
    public interface IWowInventoryItem
    {
        int BagId { get; }

        int BagSlot { get; }

        int Count { get; }

        int Durability { get; }

        string EquipLocation { get; }

        WowEquipmentSlot EquipSlot { get; }

        int Id { get; }

        int ItemLevel { get; }

        string ItemLink { get; }

        WowItemQuality ItemQuality { get; }

        int MaxDurability { get; }

        int MaxStack { get; }

        string Name { get; }

        int Price { get; }

        int RequiredLevel { get; }

        Dictionary<string, string> Stats { get; }

        string Subtype { get; }

        string Type { get; }
    }
}