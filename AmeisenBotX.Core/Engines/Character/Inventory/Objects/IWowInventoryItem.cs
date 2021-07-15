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

        int EquipSlot { get; }

        int Id { get; }

        int ItemLevel { get; }

        string ItemLink { get; }

        int ItemQuality { get; }

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