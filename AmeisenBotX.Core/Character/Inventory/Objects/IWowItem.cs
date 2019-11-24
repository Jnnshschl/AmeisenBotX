using AmeisenBotX.Core.Character.Inventory.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Inventory.Objects
{
    public interface IWowItem
    {
        int Count { get; }

        int Durability { get; }

        string EquipLocation { get; }

        EquipmentSlot EquipSlot { get; }

        int Id { get; }

        int ItemLevel { get; }

        string ItemLink { get; }

        ItemQuality ItemQuality { get; }

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
