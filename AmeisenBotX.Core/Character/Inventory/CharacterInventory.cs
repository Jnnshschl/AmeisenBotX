using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Hook;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Inventory
{
    public class CharacterInventory
    {
        public CharacterInventory(HookManager hookManager)
        {
            HookManager = hookManager;
            Items = new List<IWowItem>();
        }

        public List<IWowItem> Items { get; private set; }

        private HookManager HookManager { get; }

        private void Update()
        {
            string command = $"abotInventoryResult='['for b=0,4 do containerSlots=GetContainerNumSlots(b); for a=1,containerSlots do abItemLink=GetContainerItemLink(b,a)if abItemLink then abCurrentDurability,abMaxDurability=GetContainerItemDurability(b,a)abCooldownStart,abCooldownEnd=GetContainerItemCooldown(b,a)abIcon,abItemCount,abLocked,abQuality,abReadable,abLootable,abItemLink,isFiltered=GetContainerItemInfo(b,a)abName,abLink,abRarity,abLevel,abMinLevel,abType,abSubType,abStackCount,abEquipLoc,abIcon,abSellPrice=GetItemInfo(abItemLink)abotInventoryResult=abotInventoryResult..'{{'..'\"id\": \"'..tostring(abId or 0)..'\",'..'\"count\": \"'..tostring(abItemCount or 0)..'\",'..'\"quality\": \"'..tostring(abQuality or 0)..'\",'..'\"curDurability\": \"'..tostring(abCurrentDurability or 0)..'\",'..'\"maxDurability\": \"'..tostring(abMaxDurability or 0)..'\",'..'\"cooldownStart\": \"'..tostring(abCooldownStart or 0)..'\",'..'\"cooldownEnd\": \"'..tostring(abCooldownEnd or 0)..'\",'..'\"name\": \"'..tostring(abName or 0)..'\",'..'\"lootable\": \"'..tostring(abLootable or 0)..'\",'..'\"readable\": \"'..tostring(abReadable or 0)..'\",'..'\"link\": \"'..tostring(abItemLink or 0)..'\",'..'\"level\": \"'..tostring(abLevel or 0)..'\",'..'\"minLevel\": \"'..tostring(abMinLevel or 0)..'\",'..'\"type\": \"'..tostring(abType or 0)..'\",'..'\"subtype\": \"'..tostring(abSubType or 0)..'\",'..'\"maxStack\": \"'..tostring(abStackCount or 0)..'\",'..'\"equiplocation\": \"'..tostring(abEquipLoc or 0)..'\",'..'\"sellprice\": \"'..tostring(abSellPrice or 0)..'\"'..'}}'if b<4 or a<containerSlots then abotInventoryResult=abotInventoryResult..','end end end end;abotInventoryResult=abotInventoryResult..']'";
            HookManager.LuaDoString(command);

            string resultJson = HookManager.GetLocalizedText("abotInventoryResult");

            List<WowBasicItem> basicItems = ItemFactory.ParseItemList(resultJson);
            Items.Clear();

            foreach (WowBasicItem basicItem in basicItems)
            {
                Items.Add(ItemFactory.BuildSpecificItem(basicItem));
            }
        }
    }
}
