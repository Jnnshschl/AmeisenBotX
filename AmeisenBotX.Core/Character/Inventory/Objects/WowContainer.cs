namespace AmeisenBotX.Core.Character.Inventory.Objects
{
    public class WowContainer : WowBasicItem
    {
        public WowContainer(WowBasicItem wowBasicItem) : base()
        {
            Id = wowBasicItem.Id;
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
