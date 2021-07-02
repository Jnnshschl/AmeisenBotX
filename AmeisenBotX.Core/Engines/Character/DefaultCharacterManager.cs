using AmeisenBotX.Common.Enums;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Inventory;
using AmeisenBotX.Core.Engines.Character.Inventory.Objects;
using AmeisenBotX.Core.Engines.Character.Spells;
using AmeisenBotX.Core.Engines.Character.Talents;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Engines.Character
{
    public class DefaultCharacterManager : ICharacterManager
    {
        public DefaultCharacterManager(IWowInterface wowInterface, IMemoryApi memoryApi, IOffsetList offsetList)
        {
            Wow = wowInterface;
            MemoryApi = memoryApi;
            Offsets = offsetList;

            Inventory = new(Wow);
            Equipment = new(Wow);
            SpellBook = new(Wow);
            TalentManager = new(Wow);
            ItemComparator = new ItemLevelComparator();
            Skills = new();

            ItemSlotsToSkip = new();
        }

        public CharacterEquipment Equipment { get; }

        public CharacterInventory Inventory { get; }

        public IItemComparator ItemComparator { get; set; }

        public List<WowEquipmentSlot> ItemSlotsToSkip { get; set; }

        public int Money { get; private set; }

        public List<WowMount> Mounts { get; private set; }

        public Dictionary<string, (int, int)> Skills { get; private set; }

        public SpellBook SpellBook { get; }

        public TalentManager TalentManager { get; }

        private IMemoryApi MemoryApi { get; }

        private IOffsetList Offsets { get; }

        private IWowInterface Wow { get; }

        public void AntiAfk()
        {
            MemoryApi.Write(Offsets.TickCount, Environment.TickCount);
        }

        public void ClickToMove(Vector3 pos, ulong guid, WowClickToMoveType clickToMoveType = WowClickToMoveType.Move, float turnSpeed = 20.9f, float distance = 0.5f)
        {
            if (float.IsInfinity(pos.X) || float.IsNaN(pos.X) || MathF.Abs(pos.X) > 17066.6656
                || float.IsInfinity(pos.Y) || float.IsNaN(pos.Y) || MathF.Abs(pos.Y) > 17066.6656
                || float.IsInfinity(pos.Z) || float.IsNaN(pos.Z) || MathF.Abs(pos.Z) > 17066.6656)
            {
                return;
            }

            MemoryApi.Write(Offsets.ClickToMoveTurnSpeed, turnSpeed);
            MemoryApi.Write(Offsets.ClickToMoveDistance, distance);

            if (guid > 0)
            {
                MemoryApi.Write(Offsets.ClickToMoveGuid, guid);
            }

            MemoryApi.Write(Offsets.ClickToMoveAction, clickToMoveType);
            MemoryApi.Write(Offsets.ClickToMoveX, pos);
        }

        public Dictionary<int, int> GetConsumeables()
        {
            return Inventory.Items.OfType<WowConsumable>().GroupBy(e => e.Id).ToDictionary(e => e.Key, e => e.Count());
        }

        public bool HasItemTypeInBag<T>(bool needsToBeUseable = false)
        {
            return Inventory.Items.Any(e => Enum.IsDefined(typeof(T), e.Id));
        }

        public bool IsAbleToUseArmor(WowArmor item)
        {
            return item != null && item.ArmorType switch
            {
                WowArmorType.PLATE => Skills.Any(e => e.Key.Equals("Plate Mail", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Plattenpanzer", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.MAIL => Skills.Any(e => e.Key.Equals("Mail", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Panzer", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.LEATHER => Skills.Any(e => e.Key.Equals("Leather", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Leder", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.CLOTH => Skills.Any(e => e.Key.Equals("Cloth", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Stoff", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.TOTEMS => Skills.Any(e => e.Key.Equals("Totem", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Totem", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.LIBRAMS => Skills.Any(e => e.Key.Equals("Libram", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Buchband", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.IDOLS => Skills.Any(e => e.Key.Equals("Idol", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Götzen", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.SIGILS => Skills.Any(e => e.Key.Equals("Sigil", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Siegel", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.SHIELDS => Skills.Any(e => e.Key.Equals("Shield", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Schild", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.MISCELLANEOUS => true,
                _ => false,
            };
        }

        public bool IsAbleToUseItem(IWowItem item)
        {
            return (string.Equals(item.Type, "Armor", StringComparison.OrdinalIgnoreCase) &&
                    IsAbleToUseArmor((WowArmor)item)) || (string.Equals(item.Type, "Weapon", StringComparison.OrdinalIgnoreCase) &&
                       IsAbleToUseWeapon((WowWeapon)item));
        }

        public bool IsAbleToUseWeapon(WowWeapon item)
        {
            return item != null && item.WeaponType switch
            {
                WowWeaponType.BOWS => Skills.Any(e => e.Key.Equals("Bows", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Bogen", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.CROSSBOWS => Skills.Any(e => e.Key.Equals("Crossbows", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Armbrüste", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.GUNS => Skills.Any(e => e.Key.Equals("Guns", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Schusswaffen", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.WANDS => Skills.Any(e => e.Key.Equals("Wands", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Zauberstäbe", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.THROWN => Skills.Any(e => e.Key.Equals("Thrown", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Wurfwaffe", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.ONEHANDED_AXES => Skills.Any(e => e.Key.Equals("Axes", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Einhandäxte", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.TWOHANDED_AXES => Skills.Any(e => e.Key.Equals("Two-Handed Axes", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Zweihandäxte", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.ONEHANDED_MACES => Skills.Any(e => e.Key.Equals("Maces", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Einhandstreitkolben", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.TWOHANDED_MACES => Skills.Any(e => e.Key.Equals("Two-Handed Maces", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Zweihandstreitkolben", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.ONEHANDED_SWORDS => Skills.Any(e => e.Key.Equals("Swords", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Einhandschwerter", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.TWOHANDED_SWORDS => Skills.Any(e => e.Key.Equals("Two-Handed Swords", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Zweihandschwerter", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.DAGGERS => Skills.Any(e => e.Key.Equals("Daggers", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Dolche", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.FIST_WEAPONS => Skills.Any(e => e.Key.Equals("Fist Weapons", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Faustwaffen", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.POLEARMS => Skills.Any(e => e.Key.Equals("Polearms", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Stangenwaffen", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.STAVES => Skills.Any(e => e.Key.Equals("Staves", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("Stäbe", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.FISHING_POLES => true,
                WowWeaponType.MISCELLANEOUS => true,
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

            if (IsAbleToUseItem(item))
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

                        if (matchedItem != null
                            && item.Id != matchedItem.Id
                            && ItemComparator.IsBetter(matchedItem, item))
                        {
                            itemToReplace = matchedItems[i];
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Jump()
        {
            AmeisenLogger.I.Log("Movement", $"Jumping", LogLevel.Verbose);
            Task.Run(() => BotUtils.SendKey(MemoryApi.Process.MainWindowHandle, new((int)VirtualKey.VKSPACE), 500, 1000));
        }

        public void MoveToPosition(Vector3 pos, float turnSpeed = 20.9f, float distance = 0.1f)
        {
            ClickToMove(pos, 0, WowClickToMoveType.Move, turnSpeed, distance);
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

        public void UpdateCharacterBags()
        {
            IEnumerable<IWowItem> container = Inventory.Items.Where(item => item.Type.Equals("container", StringComparison.CurrentCultureIgnoreCase));

            if (container.Any())
            {
                for (int i = 20; i <= 23; ++i)
                {
                    if (Equipment.Items.All(keyPair => keyPair.Key != (WowEquipmentSlot)i))
                    {
                        Wow.LuaEquipItem(container.First().Name);
                        break;
                    }
                }
            }
        }

        public void UpdateCharacterGear()
        {
            System.Collections.IList list = Enum.GetValues(typeof(WowEquipmentSlot));

            for (int i = 0; i < list.Count; ++i)
            {
                WowEquipmentSlot slot = (WowEquipmentSlot)list[i];

                if (ItemSlotsToSkip.Contains(slot))
                {
                    continue;
                }

                if (slot == WowEquipmentSlot.INVSLOT_OFFHAND
                    && Equipment.Items.TryGetValue(WowEquipmentSlot.INVSLOT_MAINHAND, out IWowItem mainHandItem)
                    && mainHandItem.EquipLocation.Contains("INVTYPE_2HWEAPON", StringComparison.OrdinalIgnoreCase))
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
                                Wow.LuaEquipItem(item.Name/*, itemToReplace.Name*/);
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
                            Wow.LuaEquipItem(itemToEquip.Name);
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
            matchedItems = new();

            switch (equiplocation)
            {
                case "INVTYPE_AMMO": TryAddItem(WowEquipmentSlot.INVSLOT_AMMO, matchedItems); break;
                case "INVTYPE_HEAD": TryAddItem(WowEquipmentSlot.INVSLOT_HEAD, matchedItems); break;
                case "INVTYPE_NECK": TryAddItem(WowEquipmentSlot.INVSLOT_NECK, matchedItems); break;
                case "INVTYPE_SHOULDER": TryAddItem(WowEquipmentSlot.INVSLOT_SHOULDER, matchedItems); break;
                case "INVTYPE_BODY": TryAddItem(WowEquipmentSlot.INVSLOT_SHIRT, matchedItems); break;
                case "INVTYPE_ROBE": TryAddItem(WowEquipmentSlot.INVSLOT_CHEST, matchedItems); break;
                case "INVTYPE_CHEST": TryAddItem(WowEquipmentSlot.INVSLOT_CHEST, matchedItems); break;
                case "INVTYPE_WAIST": TryAddItem(WowEquipmentSlot.INVSLOT_WAIST, matchedItems); break;
                case "INVTYPE_LEGS": TryAddItem(WowEquipmentSlot.INVSLOT_LEGS, matchedItems); break;
                case "INVTYPE_FEET": TryAddItem(WowEquipmentSlot.INVSLOT_FEET, matchedItems); break;
                case "INVTYPE_WRIST": TryAddItem(WowEquipmentSlot.INVSLOT_WRIST, matchedItems); break;
                case "INVTYPE_HAND": TryAddItem(WowEquipmentSlot.INVSLOT_HANDS, matchedItems); break;
                case "INVTYPE_FINGER": TryAddRings(matchedItems, ref expectedItemCount); break;
                case "INVTYPE_CLOAK": TryAddItem(WowEquipmentSlot.INVSLOT_BACK, matchedItems); break;
                case "INVTYPE_TRINKET": TryAddTrinkets(matchedItems, ref expectedItemCount); break;
                case "INVTYPE_WEAPON": TryAddWeapons(matchedItems, ref expectedItemCount); break;
                case "INVTYPE_SHIELD": TryAddItem(WowEquipmentSlot.INVSLOT_OFFHAND, matchedItems); break;
                case "INVTYPE_2HWEAPON": TryAddItem(WowEquipmentSlot.INVSLOT_MAINHAND, matchedItems); break;
                case "INVTYPE_WEAPONMAINHAND": TryAddItem(WowEquipmentSlot.INVSLOT_MAINHAND, matchedItems); break;
                case "INVTYPE_WEAPONOFFHAND": TryAddItem(WowEquipmentSlot.INVSLOT_OFFHAND, matchedItems); break;
                case "INVTYPE_HOLDABLE": TryAddItem(WowEquipmentSlot.INVSLOT_OFFHAND, matchedItems); break;
                case "INVTYPE_RANGED": TryAddItem(WowEquipmentSlot.INVSLOT_RANGED, matchedItems); break;
                case "INVTYPE_THROWN": TryAddItem(WowEquipmentSlot.INVSLOT_RANGED, matchedItems); break;
                case "INVTYPE_RANGEDRIGHT": TryAddItem(WowEquipmentSlot.INVSLOT_RANGED, matchedItems); break;
                case "INVTYPE_RELIC": TryAddItem(WowEquipmentSlot.INVSLOT_RANGED, matchedItems); break;
                case "INVTYPE_TABARD": TryAddItem(WowEquipmentSlot.INVSLOT_TABARD, matchedItems); break;
                case "INVTYPE_BAG": TryAddAllBags(matchedItems, ref expectedItemCount); break;
                case "INVTYPE_QUIVER": TryAddAllBags(matchedItems, ref expectedItemCount); break;
                default: break;
            }

            return true;
        }

        private void TryAddAllBags(List<IWowItem> matchedItems, ref int expectedItemCount)
        {
            TryAddItem(WowEquipmentSlot.CONTAINER_BAG_1, matchedItems);
            TryAddItem(WowEquipmentSlot.CONTAINER_BAG_2, matchedItems);
            TryAddItem(WowEquipmentSlot.CONTAINER_BAG_3, matchedItems);
            TryAddItem(WowEquipmentSlot.CONTAINER_BAG_4, matchedItems);

            expectedItemCount = 4;
        }

        private void TryAddItem(WowEquipmentSlot slot, List<IWowItem> matchedItems)
        {
            if (Equipment.Items.TryGetValue(slot, out IWowItem ammoItem))
            {
                matchedItems.Add(ammoItem);
            }
        }

        private void TryAddRings(List<IWowItem> matchedItems, ref int expectedItemCount)
        {
            TryAddItem(WowEquipmentSlot.INVSLOT_RING1, matchedItems);
            TryAddItem(WowEquipmentSlot.INVSLOT_RING2, matchedItems);

            expectedItemCount = 2;
        }

        private void TryAddTrinkets(List<IWowItem> matchedItems, ref int expectedItemCount)
        {
            TryAddItem(WowEquipmentSlot.INVSLOT_TRINKET1, matchedItems);
            TryAddItem(WowEquipmentSlot.INVSLOT_TRINKET2, matchedItems);

            expectedItemCount = 2;
        }

        private void TryAddWeapons(List<IWowItem> matchedItems, ref int expectedItemCount)
        {
            TryAddItem(WowEquipmentSlot.INVSLOT_MAINHAND, matchedItems);
            TryAddItem(WowEquipmentSlot.INVSLOT_OFFHAND, matchedItems);

            expectedItemCount = 2;
        }

        private void UpdateMoney()
        {
            if (int.TryParse(Wow.LuaGetMoney(), out int money))
            {
                Money = money;
            }
        }

        private void UpdateMounts()
        {
            try
            {
                Mounts = JsonConvert.DeserializeObject<List<WowMount>>(Wow.LuaGetMounts());
            }
            catch { }
        }

        private void UpdateSkills()
        {
            Skills = Wow.LuaGetSkills();
        }
    }
}