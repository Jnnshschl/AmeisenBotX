using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class TankItemComparator : IWowItemComparator
    {
        public bool IsBetter(IWowItem current, IWowItem item)
        {
            float currentRating = GetRating(current, current.EquipSlot);
            float newItemRating = GetRating(item, current.EquipSlot);
            return currentRating < newItemRating;
        }

        private float GetRating(IWowItem item, EquipmentSlot slot)
        {
            float rating = 0f;
            if (slot.Equals(EquipmentSlot.INVSLOT_OFFHAND))
            {
                // shields
                if (item.GetType() == typeof(WowArmor) && ((WowArmor) item).ArmorType.Equals(ArmorType.SHIEDLS))
                {
                    if (item.Stats.TryGetValue("RESISTANCE0_NAME", out string armorString) && float.TryParse(armorString, out float armor))
                    {
                        rating += 0.5f * armor;
                    }
                    if (item.Stats.TryGetValue("ITEM_MOD_STAMINA_SHORT", out string staminaString) && float.TryParse(staminaString, out float stamina))
                    {
                        rating += 1f * stamina;
                    }
                    if (item.Stats.TryGetValue("ITEM_MOD_DEFENSE_SKILL_RATING_SHORT", out string defenseString) && float.TryParse(defenseString, out float defense))
                    {
                        rating += 1f * defense;
                    }
                    if (item.Stats.TryGetValue("ITEM_MOD_BLOCK_VALUE_SHORT", out string blockString) && float.TryParse(blockString, out float blockValue))
                    {
                        rating += 5f * blockValue;
                    }
                    if (item.Stats.TryGetValue("ITEM_MOD_BLOCK_RATING_SHORT", out string blockChanceString) && float.TryParse(blockChanceString, out float blockChance))
                    {
                        rating += 10f * blockChance;
                    }
                }
            }
            else if (slot.Equals(EquipmentSlot.INVSLOT_MAINHAND))
            {
                // swords
                if (item.GetType() == typeof(WowWeapon) && ((WowWeapon)item).WeaponType.Equals(WeaponType.ONEHANDED_SWORDS))
                {
                    if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && float.TryParse(strengthString, out float strength))
                    {
                        rating += 0.5f * strength;
                    }
                    if (item.Stats.TryGetValue("ITEM_MOD_DAMAGE_PER_SECOND_SHORT", out string dpsString) && float.TryParse(dpsString, out float dps))
                    {
                        rating += 1f * dps;
                    }
                    if (item.Stats.TryGetValue("ITEM_MOD_DEFENSE_SKILL_RATING_SHORT", out string defenseString) && float.TryParse(defenseString, out float defense))
                    {
                        rating += 5f * defense;
                    }
                    if (item.Stats.TryGetValue("ITEM_MOD_PARRY_RATING_SHORT", out string parryString) && float.TryParse(parryString, out float parry))
                    {
                        rating += 10f * parry;
                    }
                }
            }
            else if (slot.Equals(EquipmentSlot.INVSLOT_NECK) || slot.Equals(EquipmentSlot.INVSLOT_RING1)
                || slot.Equals(EquipmentSlot.INVSLOT_RING2) || slot.Equals(EquipmentSlot.INVSLOT_TRINKET1)
                || slot.Equals(EquipmentSlot.INVSLOT_TRINKET2))
            {
                // jewelry stats
                if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && float.TryParse(strengthString, out float strength))
                {
                    rating += 0.5f * strength;
                }
                if (item.Stats.TryGetValue("ITEM_MOD_STAMINA_SHORT", out string staminaString) && float.TryParse(staminaString, out float stamina))
                {
                    rating += 1f * stamina;
                }
            }
            else
            {
                // armor stats
                if (item.Stats.TryGetValue("RESISTANCE0_NAME", out string armorString) && float.TryParse(armorString, out float armor))
                {
                    rating += 0.5f * armor;
                }
                if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && float.TryParse(strengthString, out float strength))
                {
                    rating += 0.5f * strength;
                }
                if (item.Stats.TryGetValue("ITEM_MOD_PARRY_RATING_SHORT", out string parryString) && float.TryParse(parryString, out float parry))
                {
                    rating += 0.5f * parry;
                }
                if (item.Stats.TryGetValue("ITEM_MOD_BLOCK_RATING_SHORT", out string blockChanceString) && float.TryParse(blockChanceString, out float blockChance))
                {
                    rating += 0.5f * blockChance;
                }
                if (item.Stats.TryGetValue("ITEM_MOD_BLOCK_VALUE_SHORT", out string blockString) && float.TryParse(blockString, out float blockValue))
                {
                    rating += 0.5f * blockValue;
                }
                if (item.Stats.TryGetValue("ITEM_MOD_STAMINA_SHORT", out string staminaString) && float.TryParse(staminaString, out float stamina))
                {
                    rating += 1f * stamina;
                }
                if (item.Stats.TryGetValue("ITEM_MOD_DEFENSE_SKILL_RATING_SHORT", out string defenseString) && float.TryParse(defenseString, out float defense))
                {
                    rating += 1f * defense;
                }
            }
            return rating;
        }
    }
}
