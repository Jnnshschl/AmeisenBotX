using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class AssassinationItemComparator : IWowItemComparator
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
            if (item.Stats.TryGetValue("ITEM_MOD_CRIT_MELEE_RATING_SHORT", out string meleeCritString) && double.TryParse(meleeCritString, out double meleeCrit))
            {
                rating += 2f * meleeCrit;
            }
            if (item.Stats.TryGetValue("ITEM_MOD_CRIT_RATING_SHORT", out string critString) && double.TryParse(critString, out double crit))
            {
                rating += 2f * crit;
            }
            if (item.Stats.TryGetValue("ITEM_MOD_AGILITY_SHORT", out string agilityString) && double.TryParse(agilityString, out double agility))
            {
                rating += 1f * agility;
            }
            if (item.Stats.TryGetValue("ITEM_MOD_ATTACK_POWER_SHORT", out string attackString) && double.TryParse(attackString, out double attack))
            {
                rating += 0.25f * attack;
            }
            if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && double.TryParse(strengthString, out double strength))
            {
                rating += 0.5f * strength;
            }
            if (slot.Equals(EquipmentSlot.INVSLOT_OFFHAND) || slot.Equals(EquipmentSlot.INVSLOT_MAINHAND))
            {
                // also 2nd weapons
                if (item.GetType() == typeof(WowWeapon) && ((WowWeapon)item).WeaponType.Equals(WeaponType.ONEHANDED_SWORDS))
                {
                    if (item.Stats.TryGetValue("ITEM_MOD_DAMAGE_PER_SECOND_SHORT", out string dpsString) && double.TryParse(dpsString, out double dps))
                    {
                        rating += 2f * dps;
                    }
                }
            }
            else if (!(slot.Equals(EquipmentSlot.INVSLOT_NECK) || slot.Equals(EquipmentSlot.INVSLOT_RING1)
                || slot.Equals(EquipmentSlot.INVSLOT_RING2) || slot.Equals(EquipmentSlot.INVSLOT_TRINKET1)
                || slot.Equals(EquipmentSlot.INVSLOT_TRINKET2)))
            {
                // armor stats
                if (item.Stats.TryGetValue("RESISTANCE0_NAME", out string armorString) && double.TryParse(armorString, out double armor))
                {
                    rating += 0.05f * armor;
                }
                if (item.Stats.TryGetValue("ITEM_MOD_DODGE_RATING_SHORT", out string dodgeString) && double.TryParse(dodgeString, out double dodge))
                {
                    rating += 0.5f * dodge;
                }
            }
            return rating;
        }
    }
}
