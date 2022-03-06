using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Globalization;

namespace AmeisenBotX.Core.Managers.Character.Comparators
{
    public class ArmsItemComparator : IItemComparator
    {
        private readonly AmeisenBotInterfaces Bot;

        public ArmsItemComparator(AmeisenBotInterfaces bot)
        {
            Bot = bot;
        }

        public bool IsBetter(IWowInventoryItem current, IWowInventoryItem item)
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

            double currentRating = GetRating(current, (WowEquipmentSlot)current.EquipSlot);
            double newItemRating = GetRating(item, (WowEquipmentSlot)current.EquipSlot);
            return currentRating < newItemRating;
        }

        public bool IsBlacklistedItem(IWowInventoryItem item)
        {
            return false;
        }

        private double GetRating(IWowInventoryItem item, WowEquipmentSlot slot)
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
                if (item.GetType() == typeof(WowWeapon) && Bot.Player.IsAlliance() ? (((WowWeapon)item).WeaponType.Equals(WowWeaponType.AxeTwoHand) || ((WowWeapon)item).WeaponType.Equals(WowWeaponType.Axe)) : (((WowWeapon)item).WeaponType.Equals(WowWeaponType.MaceTwoHand) || ((WowWeapon)item).WeaponType.Equals(WowWeaponType.Mace)))
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