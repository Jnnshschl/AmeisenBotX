using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using System.Globalization;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class TankItemComparator : IWowItemComparator
    {
        public bool IsBetter(IWowItem current, IWowItem item)
        {
            if (item == null)
            {
                return false;
            }
            else if (current == null)
            {
                return true;
            }
            else if (item.Stats == null)
            {
                return false;
            }
            else if (current.Stats == null)
            {
                return true;
            }

            double currentRating = GetRating(current, current.EquipSlot);
            double newItemRating = GetRating(item, current.EquipSlot);
            return currentRating < newItemRating;
        }

        private double GetRating(IWowItem item, EquipmentSlot slot)
        {
            double rating = 0;
            if (slot.Equals(EquipmentSlot.INVSLOT_OFFHAND))
            {
                // shields
                if (item.GetType() == typeof(WowArmor) && ((WowArmor)item).ArmorType.Equals(ArmorType.SHIEDLS))
                {
                    if (item.Stats.TryGetValue("RESISTANCE0_NAME", out string armorString) && double.TryParse(armorString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double armor))
                    {
                        rating += 0.5 * armor;
                    }

                    if (item.Stats.TryGetValue("ITEM_MOD_STAMINA_SHORT", out string staminaString) && double.TryParse(staminaString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double stamina))
                    {
                        rating += 1f * stamina;
                    }

                    if (item.Stats.TryGetValue("ITEM_MOD_DEFENSE_SKILL_RATING_SHORT", out string defenseString) && double.TryParse(defenseString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double defense))
                    {
                        rating += 1f * defense;
                    }

                    if (item.Stats.TryGetValue("ITEM_MOD_BLOCK_VALUE_SHORT", out string blockString) && double.TryParse(blockString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double blockValue))
                    {
                        rating += 5f * blockValue;
                    }

                    if (item.Stats.TryGetValue("ITEM_MOD_BLOCK_RATING_SHORT", out string blockChanceString) && double.TryParse(blockChanceString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double blockChance))
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
                    if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && double.TryParse(strengthString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double strength))
                    {
                        rating += 0.5f * strength;
                    }

                    if (item.Stats.TryGetValue("ITEM_MOD_DAMAGE_PER_SECOND_SHORT", out string dpsString) && double.TryParse(dpsString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double dps))
                    {
                        rating += 1f * dps;
                    }

                    if (item.Stats.TryGetValue("ITEM_MOD_DEFENSE_SKILL_RATING_SHORT", out string defenseString) && double.TryParse(defenseString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double defense))
                    {
                        rating += 5f * defense;
                    }

                    if (item.Stats.TryGetValue("ITEM_MOD_PARRY_RATING_SHORT", out string parryString) && double.TryParse(parryString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double parry))
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
                if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && double.TryParse(strengthString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double strength))
                {
                    rating += 0.5f * strength;
                }

                if (item.Stats.TryGetValue("ITEM_MOD_STAMINA_SHORT", out string staminaString) && double.TryParse(staminaString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double stamina))
                {
                    rating += 1f * stamina;
                }
            }
            else
            {
                // armor stats
                if (item.Stats.TryGetValue("RESISTANCE0_NAME", out string armorString) && double.TryParse(armorString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double armor))
                {
                    rating += 0.5f * armor;
                }

                if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && double.TryParse(strengthString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double strength))
                {
                    rating += 0.5f * strength;
                }

                if (item.Stats.TryGetValue("ITEM_MOD_PARRY_RATING_SHORT", out string parryString) && double.TryParse(parryString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double parry))
                {
                    rating += 0.5f * parry;
                }

                if (item.Stats.TryGetValue("ITEM_MOD_BLOCK_RATING_SHORT", out string blockChanceString) && double.TryParse(blockChanceString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double blockChance))
                {
                    rating += 0.5f * blockChance;
                }

                if (item.Stats.TryGetValue("ITEM_MOD_BLOCK_VALUE_SHORT", out string blockString) && double.TryParse(blockString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double blockValue))
                {
                    rating += 0.5f * blockValue;
                }

                if (item.Stats.TryGetValue("ITEM_MOD_STAMINA_SHORT", out string staminaString) && double.TryParse(staminaString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double stamina))
                {
                    rating += 1f * stamina;
                }

                if (item.Stats.TryGetValue("ITEM_MOD_DEFENSE_SKILL_RATING_SHORT", out string defenseString) && double.TryParse(defenseString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double defense))
                {
                    rating += 1f * defense;
                }
            }

            return rating;
        }
    }
}