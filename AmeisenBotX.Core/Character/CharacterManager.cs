using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Character.Spells;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Common.Enums;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Character
{
    public class CharacterManager
    {
        public CharacterManager(XMemory xMemory, AmeisenBotConfig config, IOffsetList offsetList, ObjectManager objectManager, HookManager hookManager)
        {
            XMemory = xMemory;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            HookManager = hookManager;
            Config = config;

            KeyMap = new Dictionary<VirtualKeys, bool>();

            Inventory = new CharacterInventory(hookManager);
            Equipment = new CharacterEquipment(hookManager);
            SpellBook = new SpellBook(hookManager);
            Comparator = new ItemLevelComparator();
            Skills = new List<string>();
        }

        public void UpdateAll()
        {
            AmeisenLogger.Instance.Log($"Updating full character...", LogLevel.Verbose);

            Inventory.Update();
            Equipment.Update();
            SpellBook.Update();
            UpdateSkills();
            UpdateMoney();
        }

        private void UpdateSkills()
        {
            Skills = HookManager.GetSkills();
        }

        private void UpdateMoney()
        {
            string rawMoney = HookManager.GetMoney();
            if (int.TryParse(rawMoney, out int money))
            {
                Money = money;
            }
        }

        private Dictionary<VirtualKeys, bool> KeyMap { get; set; }

        private AmeisenBotConfig Config { get; }

        private ObjectManager ObjectManager { get; }

        private HookManager HookManager { get; }

        private IOffsetList OffsetList { get; }

        private XMemory XMemory { get; }

        private IWowItemComparator Comparator { get; }

        public CharacterInventory Inventory { get; }

        public CharacterEquipment Equipment { get; }

        public SpellBook SpellBook { get; }

        public int Money { get; private set; }

        public List<string> Skills { get; private set; }

        public void AntiAfk() => XMemory.Write(OffsetList.TickCount, Environment.TickCount);

        public void Jump() => BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_SPACE));

        public void MoveToPosition(Vector3 pos, float turnSpeed = 3.14f)
        {
            if (pos == Vector3.Zero)
            {
                return;
            }

            if (Config.UseClickToMove)
            {
                XMemory.Write(OffsetList.ClickToMoveX, pos.X);
                XMemory.Write(OffsetList.ClickToMoveY, pos.Y);
                XMemory.Write(OffsetList.ClickToMoveZ, pos.Z);
                XMemory.Write(OffsetList.ClickToMoveTurnSpeed, turnSpeed);
                XMemory.Write(OffsetList.ClickToMoveDistance, 3.0f);
                XMemory.Write(OffsetList.ClickToMoveGuid, ObjectManager.PlayerGuid);
                XMemory.Write(OffsetList.ClickToMoveAction, (int)ClickToMoveType.Move);
            }
            else
            {
                HandleInputSimulationMovement(pos);
            }
        }

        public void Face(Vector3 pos, ulong guid, float turnSpeed = 6.28f)
        {
            if (pos == Vector3.Zero)
            {
                return;
            }

            if (Config.UseClickToMove)
            {
                XMemory.Write(OffsetList.ClickToMoveX, pos.X);
                XMemory.Write(OffsetList.ClickToMoveY, pos.Y);
                XMemory.Write(OffsetList.ClickToMoveZ, pos.Z);
                XMemory.Write(OffsetList.ClickToMoveTurnSpeed, turnSpeed);
                XMemory.Write(OffsetList.ClickToMoveDistance, 1.0f);
                XMemory.Write(OffsetList.ClickToMoveGuid, guid);
                XMemory.Write(OffsetList.ClickToMoveAction, (int)ClickToMoveType.FaceTarget);
            }
            else
            {

            }
        }

        public void HoldKey(VirtualKeys key)
        {
            BotUtils.HoldKey(XMemory.Process.MainWindowHandle, new IntPtr((int)key));

            if (KeyMap.ContainsKey(key))
            {
                KeyMap[key] = false;
            }
            else
            {
                KeyMap.Add(key, false);
            }
        }

        public void ReleaseKey(VirtualKeys key)
        {
            BotUtils.RealeaseKey(XMemory.Process.MainWindowHandle, new IntPtr((int)key));

            if (KeyMap.ContainsKey(key))
            {
                KeyMap[key] = true;
            }
            else
            {
                KeyMap.Add(key, true);
            }
        }

        public void StopMovement(Vector3 playerPosition, ulong playerGuid)
        {
            if (Config.UseClickToMove)
            {
                XMemory.Write(OffsetList.ClickToMoveX, playerPosition.X);
                XMemory.Write(OffsetList.ClickToMoveY, playerPosition.Y);
                XMemory.Write(OffsetList.ClickToMoveZ, playerPosition.Z);
                XMemory.Write(OffsetList.ClickToMoveTurnSpeed, 6.28);
                XMemory.Write(OffsetList.ClickToMoveDistance, 1.0f);
                XMemory.Write(OffsetList.ClickToMoveGuid, playerGuid);
                XMemory.Write(OffsetList.ClickToMoveAction, (int)ClickToMoveType.Stop);
                BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_S), 0, 0);
            }
            else
            {
                foreach (KeyValuePair<VirtualKeys, bool> keyValuePair in KeyMap.Where(e => e.Value == true))
                {
                    BotUtils.RealeaseKey(XMemory.Process.MainWindowHandle, new IntPtr((int)keyValuePair.Key));
                }
            }
        }

        public bool GetCurrentClickToMovePoint(out Vector3 currentCtmPosition)
        {
            if (XMemory.Read(OffsetList.ClickToMoveX, out Vector3 currentCtmPos))
            {
                currentCtmPosition = currentCtmPos;
                return true;
            }

            currentCtmPosition = Vector3.Zero;
            return false;
        }

        public void UpdateCharacterGear()
        {
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                List<IWowItem> itemsLikeEquipped = Inventory.Items.Where(e => e.EquipLocation.Length > 0 && SlotToEquipLocation((int)slot).Contains(e.EquipLocation)).OrderByDescending(e => e.ItemLevel).ToList();

                if (itemsLikeEquipped.Count > 0)
                {
                    if (Equipment.Equipment.TryGetValue(slot, out IWowItem equippedItem))
                    {
                        foreach (IWowItem item in itemsLikeEquipped)
                        {
                            if (IsItemAnImprovement(item, out IWowItem itemToReplace))
                            {
                                HookManager.ReplaceItem(equippedItem, item);
                                break;
                            }
                        }
                    }
                    else
                    {
                        HookManager.ReplaceItem(null, itemsLikeEquipped.First());
                    }
                }
            }
        }

        public bool IsAbleToUseArmor(WowArmor item)
        {
            switch (item.ArmorType)
            {
                case ArmorType.PLATE:
                    return Skills.Any(e => e.Contains("Plate Mail") || e.Contains("Plattenpanzer"));

                case ArmorType.MAIL:
                    return Skills.Any(e => e.Contains("Mail") || e.Contains("Panzer"));

                case ArmorType.LEATHER:
                    return Skills.Any(e => e.Contains("Leather") || e.Contains("Leder"));

                case ArmorType.CLOTH:
                    return Skills.Any(e => e.Contains("Cloth") || e.Contains("Stoff"));

                case ArmorType.TOTEMS:
                    return Skills.Any(e => e.Contains("Totem") || e.Contains("Totem"));

                case ArmorType.LIBRAMS:
                    return Skills.Any(e => e.Contains("Libram") || e.Contains("Buchband"));

                case ArmorType.IDOLS:
                    return Skills.Any(e => e.Contains("Idol") || e.Contains("Götzen"));

                case ArmorType.SIGILS:
                    return Skills.Any(e => e.Contains("Sigil") || e.Contains("Siegel"));

                case ArmorType.SHIEDLS:
                    return Skills.Any(e => e.Contains("Shield") || e.Contains("Schild"));

                case ArmorType.MISCELLANEOUS:
                    return true;

                default:
                    return false;
            }
        }

        public bool IsAbleToUseWeapon(WowWeapon item)
        {
            switch (item.WeaponType)
            {
                case WeaponType.BOWS:
                    return Skills.Any(e => e.Contains("Bows") || e.Contains("Bogen"));

                case WeaponType.CROSSBOWS:
                    return Skills.Any(e => e.Contains("Crossbows") || e.Contains("Armbrüste"));

                case WeaponType.GUNS:
                    return Skills.Any(e => e.Contains("Guns") || e.Contains("Schusswaffen"));

                case WeaponType.WANDS:
                    return Skills.Any(e => e.Contains("Wands") || e.Contains("Zauberstäbe"));

                case WeaponType.THROWN:
                    return Skills.Any(e => e.Contains("Thrown") || e.Contains("Wurfwaffe"));

                case WeaponType.ONEHANDED_AXES:
                    return Skills.Any(e => e.Contains("One-Handed Axes") || e.Contains("Einhandäxte"));

                case WeaponType.TWOHANDED_AXES:
                    return Skills.Any(e => e.Contains("Two-Handed Axes") || e.Contains("Zweihandäxte"));

                case WeaponType.ONEHANDED_MACES:
                    return Skills.Any(e => e.Contains("One-Handed Maces") || e.Contains("Einhandstreitkolben"));

                case WeaponType.TWOHANDED_MACES:
                    return Skills.Any(e => e.Contains("Two-Handed Maces") || e.Contains("Zweihandstreitkolben"));

                case WeaponType.ONEHANDED_SWORDS:
                    return Skills.Any(e => e.Contains("One-Handed Swords") || e.Contains("Einhandschwerter"));

                case WeaponType.TWOHANDED_SWORDS:
                    return Skills.Any(e => e.Contains("Two-Handed Swords") || e.Contains("Zweihandschwerter"));

                case WeaponType.DAGGERS:
                    return Skills.Any(e => e.Contains("Daggers") || e.Contains("Dolche"));

                case WeaponType.FIST_WEAPONS:
                    return Skills.Any(e => e.Contains("Fist Weapons") || e.Contains("Faustwaffen"));

                case WeaponType.POLEARMS:
                    return Skills.Any(e => e.Contains("Polearms") || e.Contains("Stangenwaffen"));

                case WeaponType.STAVES:
                    return Skills.Any(e => e.Contains("Staves") || e.Contains("Stäbe"));

                case WeaponType.FISHING_POLES:
                    return true;

                case WeaponType.MISCELLANEOUS:
                    return true;

                default:
                    return false;
            }
        }

        public bool IsItemAnImprovement(IWowItem item, out IWowItem itemToReplace)
        {
            itemToReplace = null;

            if ((string.Equals(item.Type, "Armor", StringComparison.OrdinalIgnoreCase) && IsAbleToUseArmor((WowArmor)item))
                || (string.Equals(item.Type, "Weapon", StringComparison.OrdinalIgnoreCase) && IsAbleToUseWeapon((WowWeapon)item)))
            {
                if (GetItemsByEquiplocation(item.EquipLocation, out List<IWowItem> matchedItems, out int expectedItemCount))
                {
                    if (matchedItems.Count == 0)
                    {
                        return true;
                    }

                    foreach (IWowItem matchedItem in matchedItems)
                    {
                        if (matchedItem != null)
                        {
                            if (Comparator.IsBetter(matchedItem, item))
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

        private bool GetMatchingItem(IWowItem item, out IWowItem matchingItem)
        {
            matchingItem = null;

            if (item.GetType() == typeof(WowArmor) || item.GetType() == typeof(WowWeapon))
            {
                if (Equipment.Equipment.TryGetValue(item.EquipSlot, out IWowItem matchedItem))
                {
                    matchingItem = matchedItem;
                }

                return true;
            }

            return false;
        }

        private bool GetItemsByEquiplocation(string equiplocation, out List<IWowItem> matchedItems, out int expectedItemCount)
        {
            expectedItemCount = 1;
            matchedItems = new List<IWowItem>();

            switch (equiplocation)
            {
                case "INVTYPE_AMMO":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_AMMO, out IWowItem ammoItem))
                    {
                        matchedItems.Add(ammoItem);
                    }
                    break;

                case "INVTYPE_HEAD":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_HEAD, out IWowItem headItem))
                    {
                        matchedItems.Add(headItem);
                    }
                    break;

                case "INVTYPE_NECK":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_NECK, out IWowItem neckItem))
                    {
                        matchedItems.Add(neckItem);
                    }
                    break;

                case "INVTYPE_SHOULDER":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_SHOULDER, out IWowItem shoulderItem))
                    {
                        matchedItems.Add(shoulderItem);
                    }
                    break;

                case "INVTYPE_BODY":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_SHIRT, out IWowItem bodyItem))
                    {
                        matchedItems.Add(bodyItem);
                    }
                    break;

                case "INVTYPE_ROBE":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_CHEST, out IWowItem robeItem))
                    {
                        matchedItems.Add(robeItem);
                    }
                    break;

                case "INVTYPE_CHEST":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_CHEST, out IWowItem chestItem))
                    {
                        matchedItems.Add(chestItem);
                    }
                    break;

                case "INVTYPE_WAIST":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_WAIST, out IWowItem waistItem))
                    {
                        matchedItems.Add(waistItem);
                    }
                    break;

                case "INVTYPE_LEGS":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_LEGS, out IWowItem legItem))
                    {
                        matchedItems.Add(legItem);
                    }
                    break;

                case "INVTYPE_FEET":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_FEET, out IWowItem feetItem))
                    {
                        matchedItems.Add(feetItem);
                    }
                    break;

                case "INVTYPE_WRIST":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_WRIST, out IWowItem wristItem))
                    {
                        matchedItems.Add(wristItem);
                    }
                    break;

                case "INVTYPE_HAND":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_HANDS, out IWowItem handItem))
                    {
                        matchedItems.Add(handItem);
                    }
                    break;

                case "INVTYPE_FINGER":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_RING1, out IWowItem fingerItem1))
                    {
                        matchedItems.Add(fingerItem1);
                    }

                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_RING2, out IWowItem fingerItem2))
                    {
                        matchedItems.Add(fingerItem2);
                    }

                    expectedItemCount = 2;
                    break;

                case "INVTYPE_CLOAK":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_BACK, out IWowItem cloakItem))
                    {
                        matchedItems.Add(cloakItem);
                    }
                    break;

                case "INVTYPE_TRINKET":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_TRINKET1, out IWowItem trinketItem1))
                    {
                        matchedItems.Add(trinketItem1);
                    }

                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_TRINKET2, out IWowItem trinketItem2))
                    {
                        matchedItems.Add(trinketItem2);
                    }

                    expectedItemCount = 2;
                    break;

                case "INVTYPE_WEAPON":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_MAINHAND, out IWowItem mainhandweaponItem))
                    {
                        matchedItems.Add(mainhandweaponItem);
                    }

                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_OFFHAND, out IWowItem offhandweaponItem))
                    {
                        matchedItems.Add(offhandweaponItem);
                    }
                    break;

                case "INVTYPE_SHIELD":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_OFFHAND, out IWowItem shieldItem))
                    {
                        matchedItems.Add(shieldItem);
                    }
                    break;

                case "INVTYPE_2HWEAPON":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_MAINHAND, out IWowItem twohandweaponItem))
                    {
                        matchedItems.Add(twohandweaponItem);
                    }
                    break;

                case "INVTYPE_WEAPONMAINHAND":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_MAINHAND, out IWowItem mainhandItem))
                    {
                        matchedItems.Add(mainhandItem);
                    }
                    break;

                case "INVTYPE_WEAPONOFFHAND":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_OFFHAND, out IWowItem offhandItem))
                    {
                        matchedItems.Add(offhandItem);
                    }
                    break;

                case "INVTYPE_HOLDABLE":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_OFFHAND, out IWowItem holdableItem))
                    {
                        matchedItems.Add(holdableItem);
                    }
                    break;

                case "INVTYPE_RANGED":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_RANGED, out IWowItem rangeItem))
                    {
                        matchedItems.Add(rangeItem);
                    }
                    break;

                case "INVTYPE_THROWN":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_RANGED, out IWowItem thrownItem))
                    {
                        matchedItems.Add(thrownItem);
                    }
                    break;

                case "INVTYPE_RANGEDRIGHT":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_RANGED, out IWowItem rangedrightItem))
                    {
                        matchedItems.Add(rangedrightItem);
                    }
                    break;

                case "INVTYPE_RELIC":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_RANGED, out IWowItem relicItem))
                    {
                        matchedItems.Add(relicItem);
                    }
                    break;

                case "INVTYPE_TABARD":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.INVSLOT_TABARD, out IWowItem tabardItem))
                    {
                        matchedItems.Add(tabardItem);
                    }
                    break;

                case "INVTYPE_BAG":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.CONTAINER_BAG_1, out IWowItem bagitem1))
                    {
                        matchedItems.Add(bagitem1);
                    }

                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.CONTAINER_BAG_2, out IWowItem bagitem2))
                    {
                        matchedItems.Add(bagitem2);
                    }

                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.CONTAINER_BAG_3, out IWowItem bagitem3))
                    {
                        matchedItems.Add(bagitem3);
                    }

                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.CONTAINER_BAG_4, out IWowItem bagitem4))
                    {
                        matchedItems.Add(bagitem4);
                    }

                    expectedItemCount = 4;
                    break;

                case "INVTYPE_QUIVER":
                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.CONTAINER_BAG_1, out IWowItem quiverItem1))
                    {
                        matchedItems.Add(quiverItem1);
                    }

                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.CONTAINER_BAG_2, out IWowItem quiverItem2))
                    {
                        matchedItems.Add(quiverItem2);
                    }

                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.CONTAINER_BAG_3, out IWowItem quiverItem3))
                    {
                        matchedItems.Add(quiverItem3);
                    }

                    if (Equipment.Equipment.TryGetValue(EquipmentSlot.CONTAINER_BAG_4, out IWowItem quiverItem4))
                    {
                        matchedItems.Add(quiverItem4);
                    }

                    expectedItemCount = 4;
                    break;
            }

            return true;
        }

        private string SlotToEquipLocation(int slot)
        {
            switch (slot)
            {
                case 0: return "INVTYPE_AMMO";
                case 1: return "INVTYPE_HEAD";
                case 2: return "INVTYPE_NECK";
                case 3: return "INVTYPE_SHOULDER";
                case 4: return "INVTYPE_BODY";
                case 5: return "INVTYPE_CHEST|INVTYPE_ROBE";
                case 6: return "INVTYPE_WAIST";
                case 7: return "INVTYPE_LEGS";
                case 8: return "INVTYPE_FEET";
                case 9: return "INVTYPE_WRIST";
                case 10: return "INVTYPE_HAND";
                case 11: return "INVTYPE_FINGER";
                case 12: return "INVTYPE_FINGER";
                case 13: return "INVTYPE_TRINKET";
                case 14: return "INVTYPE_TRINKET";
                case 15: return "INVTYPE_CLOAK";
                case 16: return "INVTYPE_2HWEAPON|INVTYPE_WEAPON|INVTYPE_WEAPONMAINHAND";
                case 17: return "INVTYPE_SHIELD|INVTYPE_WEAPONOFFHAND|INVTYPE_HOLDABLE";
                case 18: return "INVTYPE_RANGED|INVTYPE_THROWN|INVTYPE_RANGEDRIGHT|INVTYPE_RELIC";
                case 19: return "INVTYPE_TABARD";
                case 20: return "INVTYPE_BAG|INVTYPE_QUIVER";
                case 21: return "INVTYPE_BAG|INVTYPE_QUIVER";
                case 22: return "INVTYPE_BAG|INVTYPE_QUIVER";
                case 23: return "INVTYPE_BAG|INVTYPE_QUIVER";
                default: return "none";
            }
        }

        private void HandleInputSimulationMovement(Vector3 positionToMoveTo)
        {
            double angleDiff = BotMath.GetFacingAngle(ObjectManager.Player.Position, positionToMoveTo);
        }
    }
}