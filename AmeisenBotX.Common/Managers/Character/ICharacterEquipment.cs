using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Managers.Character.Inventory
{
    public interface ICharacterEquipment
    {
        float AverageItemLevel { get; }
        Dictionary<WowEquipmentSlot, IWowInventoryItem> Items { get; }

        bool HasEnchantment(WowEquipmentSlot slot, int enchantmentId);
        void Update();
    }
}