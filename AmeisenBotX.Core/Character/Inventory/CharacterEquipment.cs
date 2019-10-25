using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Hook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Inventory
{
    public class CharacterEquipment
    {
        public CharacterEquipment(HookManager hookManager)
        {
            HookManager = hookManager;
            Equipment = new Dictionary<EquipmentSlot, IWowItem>();
        }

        public Dictionary<EquipmentSlot, IWowItem> Equipment { get; private set; }

        private HookManager HookManager { get; }

        public void Update()
        {
            Equipment.Clear();
            foreach (EquipmentSlot equipmentSlot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (equipmentSlot == EquipmentSlot.NOT_EQUIPABLE)
                {
                    continue;
                }

                try
                {
                    string resultJson = ReadEquipmentSlot(equipmentSlot);

                    if (resultJson == "noItem")
                    {
                        continue;
                    }

                    WowBasicItem rawItem = ItemFactory.ParseItem(resultJson);
                    Equipment.Add(equipmentSlot, ItemFactory.BuildSpecificItem(rawItem));
                }
                catch
                {
                    // we will ignore that, this only happens when there is no item equipped
                }
            }
        }

        private string ReadEquipmentSlot(EquipmentSlot equipmentSlot)
        {
            string command = $"abotItemSlot={(int)equipmentSlot};abotItemInfoResult='noItem';abId=GetInventoryItemID('player',abotItemSlot);abCount=GetInventoryItemCount('player',abotItemSlot);abQuality=GetInventoryItemQuality('player',abotItemSlot);abCurrentDurability,abMaxDurability=GetInventoryItemDurability(abotItemSlot);abCooldownStart,abCooldownEnd=GetInventoryItemCooldown('player',abotItemSlot);abName,abLink,abRarity,abLevel,abMinLevel,abType,abSubType,abStackCount,abEquipLoc,abIcon,abSellPrice=GetItemInfo(GetInventoryItemLink('player',abotItemSlot));abotItemInfoResult='{{'..'\"id\": \"'..tostring(abId or 0)..'\",'..'\"count\": \"'..tostring(abCount or 0)..'\",'..'\"quality\": \"'..tostring(abQuality or 0)..'\",'..'\"curDurability\": \"'..tostring(abCurrentDurability or 0)..'\",'..'\"maxDurability\": \"'..tostring(abMaxDurability or 0)..'\",'..'\"cooldownStart\": \"'..tostring(abCooldownStart or 0)..'\",'..'\"cooldownEnd\": '..tostring(abCooldownEnd or 0)..','..'\"name\": \"'..tostring(abName or 0)..'\",'..'\"link\": \"'..tostring(abLink or 0)..'\",'..'\"level\": \"'..tostring(abLevel or 0)..'\",'..'\"minLevel\": \"'..tostring(abMinLevel or 0)..'\",'..'\"type\": \"'..tostring(abType or 0)..'\",'..'\"subtype\": \"'..tostring(abSubType or 0)..'\",'..'\"maxStack\": \"'..tostring(abStackCount or 0)..'\",'..'\"equiplocation\": \"{(int)equipmentSlot}\",'..'\"sellprice\": \"'..tostring(abSellPrice or 0)..'\"'..'}}';";
            HookManager.LuaDoString(command);
            return HookManager.GetLocalizedText("abotItemInfoResult");
        }
    }
}
