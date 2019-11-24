namespace AmeisenBotX.Core.Character.Inventory.Objects
{
    public class WowKey : WowBasicItem
    {
        public WowKey(WowBasicItem wowBasicItem) : base()
        {
            Id = wowBasicItem.Id;
            BagId = wowBasicItem.BagId;
            BagSlot = wowBasicItem.BagSlot;
            Type = wowBasicItem.Type;
            Subtype = wowBasicItem.Subtype;
            Name = wowBasicItem.Name;
            ItemLink = wowBasicItem.ItemLink;
            EquipSlot = wowBasicItem.EquipSlot;
            ItemQuality = wowBasicItem.ItemQuality;
            ItemLevel = wowBasicItem.ItemLevel;
            RequiredLevel = wowBasicItem.RequiredLevel;
            Price = wowBasicItem.Price;
            Count = wowBasicItem.Count;
            MaxStack = wowBasicItem.MaxStack;
            Durability = wowBasicItem.Durability;
            MaxDurability = wowBasicItem.MaxDurability;
            EquipLocation = wowBasicItem.EquipLocation;
        }
    }
}
