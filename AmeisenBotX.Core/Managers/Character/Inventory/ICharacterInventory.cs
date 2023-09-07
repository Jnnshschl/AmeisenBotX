using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Managers.Character.Inventory
{
    public interface ICharacterInventory
    {
        int FreeBagSlots { get; }
        List<IWowInventoryItem> Items { get; }

        void DestroyItemByName(string name, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase);
        bool HasItemByName(string name, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase);
        void OnStaticPopupDeleteItem(int id);
        void TryDestroyTrash(WowItemQuality maxQuality = default);
        void Update();
    }
}