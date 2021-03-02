using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using System.Globalization;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class ArmsItemComparator : IItemComparator
    {
        private readonly WowInterface WowInterface;

        public ArmsItemComparator(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

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

        public bool IsBlacklistedItem(IWowItem item)
        {
            return false;
        }

        private double GetRating(IWowItem item, WowEquipmentSlot slot)
        {
            double rating = 0;
            if (slot.Equals(WowEquipmentSlot.INVSLOT_OFFHAND))
            {
                // don't use shields or 2nd weapons
                return 0;
            }
            else if (slot.Equals(WowEquipmentSlot.INVSLOT_MAINHAND))
            {
                // axes
                if (item.GetType() == typeof(WowWeapon) && WowInterface.Player.IsAlliance() ? (((WowWeapon)item).WeaponType.Equals(WowWeaponType.TWOHANDED_AXES) || ((WowWeapon)item).WeaponType.Equals(WowWeaponType.ONEHANDED_AXES)) : (((WowWeapon)item).WeaponType.Equals(WowWeaponType.TWOHANDED_MACES) || ((WowWeapon)item).WeaponType.Equals(WowWeaponType.ONEHANDED_MACES)))
                {
                    if (item.Stats.TryGetValue("ITEM_MOD_ATTACK_POWER_SHORT", out string attackString) && double.TryParse(attackString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double attack))
                    {
                        rating += 0.5f * attack;
                    }

                    if (item.Stats.TryGetValue("ITEM_MOD_DAMAGE_PER_SECOND_SHORT", out string dpsString) && double.TryParse(dpsString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double dps))
                    {
                        rating += 2f * dps;
                    }

                    if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && double.TryParse(strengthString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double strength))
                    {
                        rating += 1f * strength;
                    }
                }
            }
            else if (slot.Equals(WowEquipmentSlot.INVSLOT_NECK) || slot.Equals(WowEquipmentSlot.INVSLOT_RING1)
                || slot.Equals(WowEquipmentSlot.INVSLOT_RING2) || slot.Equals(WowEquipmentSlot.INVSLOT_TRINKET1)
                || slot.Equals(WowEquipmentSlot.INVSLOT_TRINKET2))
            {
                // jewelry stats
                if (item.Stats.TryGetValue("ITEM_MOD_ATTACK_POWER_SHORT", out string attackString) && double.TryParse(attackString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double attack))
                {
                    rating += 0.5f * attack;
                }

                if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && double.TryParse(strengthString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double strength))
                {
                    rating += 1f * strength;
                }
            }
            else
            {
                // armor stats
                if (item.Stats.TryGetValue("RESISTANCE0_NAME", out string armorString) && double.TryParse(armorString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double armor))
                {
                    rating += 0.5f * armor;
                }

                if (item.Stats.TryGetValue("ITEM_MOD_ATTACK_POWER_SHORT", out string attackString) && double.TryParse(attackString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double attack))
                {
                    rating += 0.5f * attack;
                }

                if (item.Stats.TryGetValue("ITEM_MOD_STRENGTH_SHORT", out string strengthString) && double.TryParse(strengthString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double strength))
                {
                    rating += 1f * strength;
                }
            }

            return rating;
        }
    }
}