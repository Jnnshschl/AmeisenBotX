using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Enums;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Character.Objects;
using AmeisenBotX.Core.Character.Spells;
using AmeisenBotX.Core.Character.Talents;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Common.Enums;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            TalentManager = new TalentManager(WowInterface);
            ItemComparator = new ItemLevelComparator();
            Skills = new Dictionary<string, (int, int)>();
        }

        public CharacterEquipment Equipment { get; }

        public CharacterInventory Inventory { get; }

        public IWowItemComparator ItemComparator { get; set; }

        public int Money { get; private set; }

        public List<WowMount> Mounts { get; private set; }

        public Dictionary<string, (int, int)> Skills { get; private set; }

        public SpellBook SpellBook { get; }

        public TalentManager TalentManager { get; }

        private AmeisenBotConfig Config { get; }

        private WowInterface WowInterface { get; }

        public void AntiAfk()
        {
            WowInterface.XMemory.Write(WowInterface.OffsetList.TickCount, Environment.TickCount);
        }

        public Dictionary<int, int> GetConsumeables()
        {
            return Inventory.Items.OfType<WowConsumable>().GroupBy(e => e.Id).ToDictionary(e => e.Key, e => e.Count());
        }

        public bool HasFoodInBag()
        {
            return Inventory.Items.Any(e => Enum.IsDefined(typeof(WowFood), e.Id));
        }

        public bool HasRefreshmentInBag()
        {
            return Inventory.Items.Select(e => e.Id).Any(e => Enum.IsDefined(typeof(WowRefreshment), e));
        }

        public bool HasWaterInBag()
        {
            return Inventory.Items.Select(e => e.Id).Any(e => Enum.IsDefined(typeof(WowWater), e));
        }

        public bool IsAbleToUseArmor(WowArmor item)
        {
            return item != null && item.ArmorType switch
            {
                ArmorType.PLATE => Skills.Any(e => e.Key.Equals("Plate Mail", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Plattenpanzer", StringComparison.OrdinalIgnoreCase)),
                ArmorType.MAIL => Skills.Any(e => e.Key.Equals("Mail", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Panzer", StringComparison.OrdinalIgnoreCase)),
                ArmorType.LEATHER => Skills.Any(e => e.Key.Equals("Leather", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Leder", StringComparison.OrdinalIgnoreCase)),
                ArmorType.CLOTH => Skills.Any(e => e.Key.Equals("Cloth", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Stoff", StringComparison.OrdinalIgnoreCase)),
                ArmorType.TOTEMS => Skills.Any(e => e.Key.Equals("Totem", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Totem", StringComparison.OrdinalIgnoreCase)),
                ArmorType.LIBRAMS => Skills.Any(e => e.Key.Equals("Libram", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Buchband", StringComparison.OrdinalIgnoreCase)),
                ArmorType.IDOLS => Skills.Any(e => e.Key.Equals("Idol", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Götzen", StringComparison.OrdinalIgnoreCase)),
                ArmorType.SIGILS => Skills.Any(e => e.Key.Equals("Sigil", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Siegel", StringComparison.OrdinalIgnoreCase)),
                ArmorType.SHIELDS => Skills.Any(e => e.Key.Equals("Shield", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Schild", StringComparison.OrdinalIgnoreCase)),
                ArmorType.MISCELLANEOUS => true,
                _ => false,
            };
        }

        public bool IsAbleToUseWeapon(WowWeapon item)
        {
            return item != null && item.WeaponType switch
            {
                WeaponType.BOWS => Skills.Any(e => e.Key.Equals("Bows", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Bogen", StringComparison.OrdinalIgnoreCase)),
                WeaponType.CROSSBOWS => Skills.Any(e => e.Key.Equals("Crossbows", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Armbrüste", StringComparison.OrdinalIgnoreCase)),
                WeaponType.GUNS => Skills.Any(e => e.Key.Equals("Guns", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Schusswaffen", StringComparison.OrdinalIgnoreCase)),
                WeaponType.WANDS => Skills.Any(e => e.Key.Equals("Wands", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Zauberstäbe", StringComparison.OrdinalIgnoreCase)),
                WeaponType.THROWN => Skills.Any(e => e.Key.Equals("Thrown", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Wurfwaffe", StringComparison.OrdinalIgnoreCase)),
                WeaponType.ONEHANDED_AXES => Skills.Any(e => e.Key.Equals("One-Handed Axes", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Einhandäxte", StringComparison.OrdinalIgnoreCase)),
                WeaponType.TWOHANDED_AXES => Skills.Any(e => e.Key.Equals("Two-Handed Axes", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Zweihandäxte", StringComparison.OrdinalIgnoreCase)),
                WeaponType.ONEHANDED_MACES => Skills.Any(e => e.Key.Equals("One-Handed Maces", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Einhandstreitkolben", StringComparison.OrdinalIgnoreCase)),
                WeaponType.TWOHANDED_MACES => Skills.Any(e => e.Key.Equals("Two-Handed Maces", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Zweihandstreitkolben", StringComparison.OrdinalIgnoreCase)),
                WeaponType.ONEHANDED_SWORDS => Skills.Any(e => e.Key.Equals("One-Handed Swords", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Einhandschwerter", StringComparison.OrdinalIgnoreCase)),
                WeaponType.TWOHANDED_SWORDS => Skills.Any(e => e.Key.Equals("Two-Handed Swords", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Zweihandschwerter", StringComparison.OrdinalIgnoreCase)),
                WeaponType.DAGGERS => Skills.Any(e => e.Key.Equals("Daggers", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Dolche", StringComparison.OrdinalIgnoreCase)),
                WeaponType.FIST_WEAPONS => Skills.Any(e => e.Key.Equals("Fist Weapons", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Faustwaffen", StringComparison.OrdinalIgnoreCase)),
                WeaponType.POLEARMS => Skills.Any(e => e.Key.Equals("Polearms", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Stangenwaffen", StringComparison.OrdinalIgnoreCase)),
                WeaponType.STAVES => Skills.Any(e => e.Key.Equals("Staves", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Stäbe", StringComparison.OrdinalIgnoreCase)),
                WeaponType.FISHING_POLES => true,
                WeaponType.MISCELLANEOUS => true,
                _ => false,
            };
        }

        public bool IsItemAnImprovement(IWowItem item, out IWowItem itemToReplace)
        {
            itemToReplace = null;

            if (item == null || ItemComparator.IsBlacklistedItem(item))
            {
                return false;
            }

            if ((string.Equals(item.Type, "Armor", StringComparison.OrdinalIgnoreCase) && IsAbleToUseArmor((WowArmor)item))
                || (string.Equals(item.Type, "Weapon", StringComparison.OrdinalIgnoreCase) && IsAbleToUseWeapon((WowWeapon)item)))
            {
                if (GetItemsByEquiplocation(item.EquipLocation, out List<IWowItem> matchedItems, out _))
                {
                    // if we dont have an item in the slot or if we only have 3 of 4 bags
                    if (matchedItems.Count == 0)
                    {
                        return true;
                    }

                    for (int i = 0; i < matchedItems.Count; ++i)
                    {
                        IWowItem matchedItem = matchedItems[i];

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

        public void Jump()
        {
            AmeisenLogger.I.Log("Movement", $"Jumping", LogLevel.Verbose);
            Task.Run(() => BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKey.VKSPACE), 500, 1000));
        }

        public void MoveToPosition(Vector3 pos, float turnSpeed = 20.9f, float distance = 0.5f)
        {
            if (pos == default || (WowInterface.XMemory.Read(WowInterface.OffsetList.ClickToMoveX, out Vector3 currentTargetPos) && currentTargetPos == pos))
            {
                return;
            }

            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveTurnSpeed, turnSpeed);
            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveDistance, distance);
            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveGuid, WowInterface.ObjectManager.PlayerGuid);
            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveAction, ClickToMoveType.Move);
            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveX, pos);
        }

        public void ClickToMove(Vector3 pos, ulong guid, ClickToMoveType clickToMoveType = ClickToMoveType.Move, float turnSpeed = 20.9f, float distance = 0.5f)
        {
            if (pos == default || (WowInterface.XMemory.Read(WowInterface.OffsetList.ClickToMoveX, out Vector3 currentTargetPos) && currentTargetPos == pos))
            {
                return;
            }

            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveTurnSpeed, turnSpeed);
            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveDistance, distance);
            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveGuid, guid);
            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveAction, clickToMoveType);
            WowInterface.XMemory.Write(WowInterface.OffsetList.ClickToMoveX, pos);
        }

        public void UpdateAll()
        {
            AmeisenLogger.I.Log("CharacterManager", $"Updating full character", LogLevel.Verbose);

            Parallel.Invoke
            (
                () => Inventory.Update(),
                () => Equipment.Update(),
                () => SpellBook.Update(),
                () => TalentManager.Update(),
                () => UpdateSkills(),
                () => UpdateMoney(),
                () => UpdateMounts()
            );
        }

        public void UpdateCharacterGear()
        {
            System.Collections.IList list = Enum.GetValues(typeof(EquipmentSlot));

            for (int i = 0; i < list.Count; ++i)
            {
                EquipmentSlot slot = (EquipmentSlot)list[i];

                if (slot == EquipmentSlot.INVSLOT_OFFHAND && Equipment.Items.TryGetValue(EquipmentSlot.INVSLOT_MAINHAND, out IWowItem mainHandItem) && mainHandItem.EquipLocation.Contains("INVTYPE_2HWEAPON", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                IEnumerable<IWowItem> itemsLikeEquipped = Inventory.Items.Where(e => !string.IsNullOrWhiteSpace(e.EquipLocation) && SlotToEquipLocation((int)slot).Contains(e.EquipLocation, StringComparison.OrdinalIgnoreCase)).OrderByDescending(e => e.ItemLevel).ToList();

                if (itemsLikeEquipped.Any())
                {
                    if (Equipment.Items.TryGetValue(slot, out IWowItem equippedItem))
                    {
                        for (int f = 0; f < itemsLikeEquipped.Count(); ++f)
                        {
                            IWowItem item = itemsLikeEquipped.ElementAt(f);
                            if (IsItemAnImprovement(item, out IWowItem itemToReplace))
                            {
                                AmeisenLogger.I.Log("Equipment", $"Replacing \"{itemToReplace}\" with \"{item}\"", LogLevel.Verbose);
                                WowInterface.HookManager.ReplaceItem(null, item);
                                Equipment.Update();
                                break;
                            }
                        }
                    }
                    else
                    {
                        IWowItem itemToEquip = itemsLikeEquipped.First();

                        if ((string.Equals(itemToEquip.Type, "Armor", StringComparison.OrdinalIgnoreCase) && IsAbleToUseArmor((WowArmor)itemToEquip))
                            || (string.Equals(itemToEquip.Type, "Weapon", StringComparison.OrdinalIgnoreCase) && IsAbleToUseWeapon((WowWeapon)itemToEquip)))
                        {
                            AmeisenLogger.I.Log("Equipment", $"Equipping \"{itemToEquip}\"", LogLevel.Verbose);
                            WowInterface.HookManager.ReplaceItem(null, itemToEquip);
                            Equipment.Update();
                        }
                    }
                }
            }
        }

        private static string SlotToEquipLocation(int slot)
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
            if (int.TryParse(WowInterface.HookManager.GetMoney(), out int money))
            {
                Money = money;
            }
        }

        private void UpdateMounts()
        {
            try
            {
                Mounts = JsonConvert.DeserializeObject<List<WowMount>>(WowInterface.HookManager.GetMounts());
            }
            catch { }
        }

        private void UpdateSkills()
        {
            Skills = WowInterface.HookManager.GetSkills();
        }
    }
}