using AmeisenBotX.Core.Character.Inventory.Enums;

namespace AmeisenBotX.Core.Character.Inventory.Objects
{
    public interface IWowItem
    {
        int Id { get; }

        string Type { get; }

        string Subtype { get; }

        string Name { get; }

        string ItemLink { get; }

        string EquipLocation { get; }

        EquipmentSlot EquipSlot { get; }

        ItemQuality ItemQuality { get; }

        int ItemLevel { get; }

        int RequiredLevel { get; }

        int Price { get; }

        int Count { get; }

        int MaxStack { get; }

        int Durability { get; }

        int MaxDurability { get; }

    }
}
