using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class ArmsAxeItemComparator : IWowItemComparator
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
            else if(item.Stats == null)
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
                // don't use shields or 2nd weapons
                return 0;
            }
            else if (slot.Equals(EquipmentSlot.INVSLOT_MAINHAND))
            {
                // axes
                if (item.GetType() == typeof(WowWeapon) && (((WowWeapon)item).WeaponType.Equals(WeaponType.TWOHANDED_AXES) || ((WowWeapon)item).WeaponType.Equals(WeaponType.ONEHANDED_AXES)))
                {
                    if (item.Stats.TryGetValue("ITEM_MOD_ATTACK_POWER_SHORT", out string attackString) && double.TryParse(attackString, out double attack))
                    {
                        rating += 0.5f * attack;
                    }
                    if (item.Stats.TryGetValue("ITEM_MOD_DAMAGE_PER_SECOND_SHORT", out string dpsString) && double.TryParse(dpsString, out double dps))
                    {
                        rating += 2f * dps;
                    }
                    if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && double.TryParse(strengthString, out double strength))
                    {
                        rating += 1f * strength;
                    }
                }
            }
            else if (slot.Equals(EquipmentSlot.INVSLOT_NECK) || slot.Equals(EquipmentSlot.INVSLOT_RING1)
                || slot.Equals(EquipmentSlot.INVSLOT_RING2) || slot.Equals(EquipmentSlot.INVSLOT_TRINKET1)
                || slot.Equals(EquipmentSlot.INVSLOT_TRINKET2))
            {
                // jewelry stats
                if (item.Stats.TryGetValue("ITEM_MOD_ATTACK_POWER_SHORT", out string attackString) && double.TryParse(attackString, out double attack))
                {
                    rating += 0.5f * attack;
                }
                if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && double.TryParse(strengthString, out double strength))
                {
                    rating += 1f * strength;
                }
            }
            else
            {
                // armor stats
                if (item.Stats.TryGetValue("RESISTANCE0_NAME", out string armorString) && double.TryParse(armorString, out double armor))
                {
                    rating += 0.5f * armor;
                }
                if (item.Stats.TryGetValue("ITEM_MOD_ATTACK_POWER_SHORT", out string attackString) && double.TryParse(attackString, out double attack))
                {
                    rating += 0.5f * attack;
                }
                if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && double.TryParse(strengthString, out double strength))
                {
                    rating += 1f * strength;
                }
            }
            return rating;
        }
    }
}
