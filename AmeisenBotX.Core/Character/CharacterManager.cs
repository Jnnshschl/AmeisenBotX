using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Enums;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Character.Spells;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Common.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Character
{
    public class CharacterManager : ICharacterManager
    {
        public CharacterManager(AmeisenBotConfig config, WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            Config = config;

            Inventory = new CharacterInventory(WowInterface);
            Equipment = new CharacterEquipment(WowInterface);
            SpellBook = new SpellBook(WowInterface);
            ItemComparator = new ItemLevelComparator();
            Skills = new List<string>();
        }

        public CharacterEquipment Equipment { get; }

        public CharacterInventory Inventory { get; }

        public IWowItemComparator ItemComparator { get; set; }

        public int Money { get; private set; }

        public List<string> Skills { get; private set; }

        public SpellBook SpellBook { get; }

        private AmeisenBotConfig Config { get; }

        private WowInterface WowInterface { get; }

        public void AntiAfk() => WowInterface.XMemory.Write(WowInterface.OffsetList.TickCount, Environment.TickCount);

        public Dictionary<int, int> GetConsumeables()
            => Inventory.Items.OfType<WowConsumable>().GroupBy(e => e.Id).ToDictionary(e => e.Key, e => e.Count());

        public bool IsAbleToUseArmor(WowArmor item)
        {
            return item.ArmorType switch
            {
                ArmorType.PLATE => Skills.Any(e => e.Contains("Plate Mail") || e.Contains("Plattenpanzer")),
                ArmorType.MAIL => Skills.Any(e => e.Contains("Mail") || e.Contains("Panzer")),
                ArmorType.LEATHER => Skills.Any(e => e.Contains("Leather") || e.Contains("Leder")),
                ArmorType.CLOTH => Skills.Any(e => e.Contains("Cloth") || e.Contains("Stoff")),
                ArmorType.TOTEMS => Skills.Any(e => e.Contains("Totem") || e.Contains("Totem")),
                ArmorType.LIBRAMS => Skills.Any(e => e.Contains("Libram") || e.Contains("Buchband")),
                ArmorType.IDOLS => Skills.Any(e => e.Contains("Idol") || e.Contains("Götzen")),
                ArmorType.SIGILS => Skills.Any(e => e.Contains("Sigil") || e.Contains("Siegel")),
                ArmorType.SHIEDLS => Skills.Any(e => e.Contains("Shield") || e.Contains("Schild")),
                ArmorType.MISCELLANEOUS => true,
                _ => false,
            };
        }

        public bool IsAbleToUseWeapon(WowWeapon item)
        {
            return item.WeaponType switch
            {
                WeaponType.BOWS => Skills.Any(e => e.Contains("Bows") || e.Contains("Bogen")),
                WeaponType.CROSSBOWS => Skills.Any(e => e.Contains("Crossbows") || e.Contains("Armbrüste")),
                WeaponType.GUNS => Skills.Any(e => e.Contains("Guns") || e.Contains("Schusswaffen")),
                WeaponType.WANDS => Skills.Any(e => e.Contains("Wands") || e.Contains("Zauberstäbe")),
                WeaponType.THROWN => Skills.Any(e => e.Contains("Thrown") || e.Contains("Wurfwaffe")),
                WeaponType.ONEHANDED_AXES => Skills.Any(e => e.Contains("One-Handed Axes") || e.Contains("Einhandäxte")),
                WeaponType.TWOHANDED_AXES => Skills.Any(e => e.Contains("Two-Handed Axes") || e.Contains("Zweihandäxte")),
                WeaponType.ONEHANDED_MACES => Skills.Any(e => e.Contains("One-Handed Maces") || e.Contains("Einhandstreitkolben")),
                WeaponType.TWOHANDED_MACES => Skills.Any(e => e.Contains("Two-Handed Maces") || e.Contains("Zweihandstreitkolben")),
                WeaponType.ONEHANDED_SWORDS => Skills.Any(e => e.Contains("One-Handed Swords") || e.Contains("Einhandschwerter")),
                WeaponType.TWOHANDED_SWORDS => Skills.Any(e => e.Contains("Two-Handed Swords") || e.Contains("Zweihandschwerter")),
                WeaponType.DAGGERS => Skills.Any(e => e.Contains("Daggers") || e.Contains("Dolche")),
                WeaponType.FIST_WEAPONS => Skills.Any(e => e.Contains("Fist Weapons") || e.Contains("Faustwaffen")),
                WeaponType.POLEARMS => Skills.Any(e => e.Contains("Polearms") || e.Contains("Stangenwaffen")),
                WeaponType.STAVES => Skills.Any(e => e.Contains("Staves") || e.Contains("Stäbe")),
                WeaponType.FISHING_POLES => true,
                WeaponType.MISCELLANEOUS => true,
                _ => false,
            };
        }

        public bool IsItemAnImprovement(IWowItem item, out IWowItem itemToReplace)
        {
            itemToReplace = null;

            if (((string.Equals(item.Type, "Armor", StringComparison.OrdinalIgnoreCase) && IsAbleToUseArmor((WowArmor)item))
                || (string.Equals(item.Type, "Weapon", StringComparison.OrdinalIgnoreCase) && IsAbleToUseWeapon((WowWeapon)item))))
            {
                if (GetItemsByEquiplocation(item.EquipLocation, out List<IWowItem> matchedItems, out _))
                {
                    // if we dont have an item in the slot or if we only have 3 of 4 bags
                    if (matchedItems.Count == 0)
                    {
                        return true;
                    }

                    foreach (IWowItem matchedItem in matchedItems)
                    {
                        if (matchedItem != null)
                        {
                            if (ItemComparator.IsBetter(matchedItem, item))
                            {
                                itemToReplace = item;
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public void Jump() => BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_SPACE));

        public void MoveToPosition(Vector3 pos, float turnSpeed = 20.9f, float distance = 0.25f)
        {
            if (pos == new Vector3(0, 0, 0))
            {
                return;
            }

            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveX, pos);
            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveTurnSpeed, turnSpeed);
            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveDistance, distance);
            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveGuid, WowInterface.ObjectManager.PlayerGuid);
            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveAction, (int)ClickToMoveType.Move);
        }

        public void UpdateAll()
        {
            AmeisenLogger.Instance.Log("CharacterManager", $"Updating full character...", LogLevel.Verbose);

            Inventory.Update();
            Equipment.Update();
            SpellBook.Update();
            UpdateSkills();
            UpdateMoney();
        }

        public void UpdateCharacterGear()
        {
            Equipment.Update();
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot == EquipmentSlot.INVSLOT_OFFHAND && Equipment.Items.TryGetValue(EquipmentSlot.INVSLOT_MAINHAND, out IWowItem mainHandItem) && mainHandItem.EquipLocation.Contains("INVTYPE_2HWEAPON"))
                {
                    continue;
                }

                List<IWowItem> itemsLikeEquipped = Inventory.Items.Where(e => e.EquipLocation.Length > 0 && SlotToEquipLocation((int)slot).Contains(e.EquipLocation)).OrderByDescending(e => e.ItemLevel).ToList();

                if (itemsLikeEquipped.Count > 0)
                {
                    if (Equipment.Items.TryGetValue(slot, out IWowItem equippedItem))
                    {
                        foreach (IWowItem item in itemsLikeEquipped)
                        {
                            if (IsItemAnImprovement(item, out IWowItem itemToReplace))
                            {
                                WowInterface.HookManager.ReplaceItem(null, item);
                                Equipment.Update();
                                break;
                            }
                        }
                    }
                    else
                    {
                        WowInterface.HookManager.ReplaceItem(null, itemsLikeEquipped.First());
                        Equipment.Update();
                    }
                }
            }
        }

        private bool GetItemsByEquiplocation(string equiplocation, out List<IWowItem> matchedItems, out int expectedItemCount)
        {
            expectedItemCount = 1;
            matchedItems = new List<IWowItem>();

            switch (equiplocation)
            {
                case "INVTYPE_AMMO": TryAddItem(EquipmentSlot.INVSLOT_AMMO, matchedItems); break;
                case "INVTYPE_HEAD": TryAddItem(EquipmentSlot.INVSLOT_HEAD, matchedItems); break;
                case "INVTYPE_NECK": TryAddItem(EquipmentSlot.INVSLOT_NECK, matchedItems); break;
                case "INVTYPE_SHOULDER": TryAddItem(EquipmentSlot.INVSLOT_SHOULDER, matchedItems); break;
                case "INVTYPE_BODY": TryAddItem(EquipmentSlot.INVSLOT_SHIRT, matchedItems); break;
                case "INVTYPE_ROBE": TryAddItem(EquipmentSlot.INVSLOT_CHEST, matchedItems); break;
                case "INVTYPE_CHEST": TryAddItem(EquipmentSlot.INVSLOT_CHEST, matchedItems); break;
                case "INVTYPE_WAIST": TryAddItem(EquipmentSlot.INVSLOT_WAIST, matchedItems); break;
                case "INVTYPE_LEGS": TryAddItem(EquipmentSlot.INVSLOT_LEGS, matchedItems); break;
                case "INVTYPE_FEET": TryAddItem(EquipmentSlot.INVSLOT_FEET, matchedItems); break;
                case "INVTYPE_WRIST": TryAddItem(EquipmentSlot.INVSLOT_WRIST, matchedItems); break;
                case "INVTYPE_HAND": TryAddItem(EquipmentSlot.INVSLOT_HANDS, matchedItems); break;
                case "INVTYPE_FINGER": TryAddRings(matchedItems, ref expectedItemCount); break;
                case "INVTYPE_CLOAK": TryAddItem(EquipmentSlot.INVSLOT_BACK, matchedItems); break;
                case "INVTYPE_TRINKET": TryAddTrinkets(matchedItems, ref expectedItemCount); break;
                case "INVTYPE_WEAPON": TryAddWeapons(matchedItems, ref expectedItemCount); break;
                case "INVTYPE_SHIELD": TryAddItem(EquipmentSlot.INVSLOT_OFFHAND, matchedItems); break;
                case "INVTYPE_2HWEAPON": TryAddItem(EquipmentSlot.INVSLOT_MAINHAND, matchedItems); break;
                case "INVTYPE_WEAPONMAINHAND": TryAddItem(EquipmentSlot.INVSLOT_MAINHAND, matchedItems); break;
                case "INVTYPE_WEAPONOFFHAND": TryAddItem(EquipmentSlot.INVSLOT_OFFHAND, matchedItems); break;
                case "INVTYPE_HOLDABLE": TryAddItem(EquipmentSlot.INVSLOT_OFFHAND, matchedItems); break;
                case "INVTYPE_RANGED": TryAddItem(EquipmentSlot.INVSLOT_RANGED, matchedItems); break;
                case "INVTYPE_THROWN": TryAddItem(EquipmentSlot.INVSLOT_RANGED, matchedItems); break;
                case "INVTYPE_RANGEDRIGHT": TryAddItem(EquipmentSlot.INVSLOT_RANGED, matchedItems); break;
                case "INVTYPE_RELIC": TryAddItem(EquipmentSlot.INVSLOT_RANGED, matchedItems); break;
                case "INVTYPE_TABARD": TryAddItem(EquipmentSlot.INVSLOT_TABARD, matchedItems); break;
                case "INVTYPE_BAG": TryAddAllBags(matchedItems, ref expectedItemCount); break;
                case "INVTYPE_QUIVER": TryAddAllBags(matchedItems, ref expectedItemCount); break;
                default: break;
            }

            return true;
        }

        private string SlotToEquipLocation(int slot)
        {
            return slot switch
            {
                0 => "INVTYPE_AMMO",
                1 => "INVTYPE_HEAD",
                2 => "INVTYPE_NECK",
                3 => "INVTYPE_SHOULDER",
                4 => "INVTYPE_BODY",
                5 => "INVTYPE_CHEST|INVTYPE_ROBE",
                6 => "INVTYPE_WAIST",
                7 => "INVTYPE_LEGS",
                8 => "INVTYPE_FEET",
                9 => "INVTYPE_WRIST",
                10 => "INVTYPE_HAND",
                11 => "INVTYPE_FINGER",
                12 => "INVTYPE_FINGER",
                13 => "INVTYPE_TRINKET",
                14 => "INVTYPE_TRINKET",
                15 => "INVTYPE_CLOAK",
                16 => "INVTYPE_2HWEAPON|INVTYPE_WEAPON|INVTYPE_WEAPONMAINHAND",
                17 => "INVTYPE_SHIELD|INVTYPE_WEAPONOFFHAND|INVTYPE_HOLDABLE",
                18 => "INVTYPE_RANGED|INVTYPE_THROWN|INVTYPE_RANGEDRIGHT|INVTYPE_RELIC",
                19 => "INVTYPE_TABARD",
                20 => "INVTYPE_BAG|INVTYPE_QUIVER",
                21 => "INVTYPE_BAG|INVTYPE_QUIVER",
                22 => "INVTYPE_BAG|INVTYPE_QUIVER",
                23 => "INVTYPE_BAG|INVTYPE_QUIVER",
                _ => "none",
            };
        }

        private void TryAddAllBags(List<IWowItem> matchedItems, ref int expectedItemCount)
        {
            TryAddItem(EquipmentSlot.CONTAINER_BAG_1, matchedItems);
            TryAddItem(EquipmentSlot.CONTAINER_BAG_2, matchedItems);
            TryAddItem(EquipmentSlot.CONTAINER_BAG_3, matchedItems);
            TryAddItem(EquipmentSlot.CONTAINER_BAG_4, matchedItems);

            expectedItemCount = 4;
        }

        private void TryAddItem(EquipmentSlot slot, List<IWowItem> matchedItems)
        {
            if (Equipment.Items.TryGetValue(slot, out IWowItem ammoItem))
            {
                matchedItems.Add(ammoItem);
            }
        }

        private void TryAddRings(List<IWowItem> matchedItems, ref int expectedItemCount)
        {
            TryAddItem(EquipmentSlot.INVSLOT_RING1, matchedItems);
            TryAddItem(EquipmentSlot.INVSLOT_RING2, matchedItems);

            expectedItemCount = 2;
        }

        private void TryAddTrinkets(List<IWowItem> matchedItems, ref int expectedItemCount)
        {
            TryAddItem(EquipmentSlot.INVSLOT_TRINKET1, matchedItems);
            TryAddItem(EquipmentSlot.INVSLOT_TRINKET2, matchedItems);

            expectedItemCount = 2;
        }

        private void TryAddWeapons(List<IWowItem> matchedItems, ref int expectedItemCount)
        {
            TryAddItem(EquipmentSlot.INVSLOT_MAINHAND, matchedItems);
            TryAddItem(EquipmentSlot.INVSLOT_OFFHAND, matchedItems);

            expectedItemCount = 2;
        }

        private void UpdateMoney()
        {
            string rawMoney = WowInterface.HookManager.GetMoney();
            if (int.TryParse(rawMoney, out int money))
            {
                Money = money;
            }
        }

        private void UpdateSkills()
        {
            Skills = WowInterface.HookManager.GetSkills();
        }
    }
}