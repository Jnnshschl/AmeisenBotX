using AmeisenBotX.Common.Keyboard.Enums;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Inventory;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Core.Managers.Character.Spells;
using AmeisenBotX.Core.Managers.Character.Talents;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Constants;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Managers.Character
{
    public class DefaultCharacterManager : ICharacterManager
    {
        public DefaultCharacterManager(IWowInterface wowInterface, IMemoryApi memoryApi)
        {
            Wow = wowInterface;
            MemoryApi = memoryApi;

            Inventory = new CharacterInventory(Wow);
            Equipment = new CharacterEquipment(Wow);
            SpellBook = new SpellBook(Wow);
            TalentManager = new TalentManager(Wow);
            LastLevelTrained = 0;
            ItemComparator = new ItemLevelComparator();
            Skills = new Dictionary<string, (int, int)>();

            ItemSlotsToSkip = new List<WowEquipmentSlot>();
        }

        public CharacterEquipment Equipment { get; }

        public CharacterInventory Inventory { get; }

        public IItemComparator ItemComparator { get; set; }

        public List<WowEquipmentSlot> ItemSlotsToSkip { get; set; }

        public int LastLevelTrained { get; set; }

        public int Money { get; private set; }

        public IEnumerable<WowMount> Mounts { get; private set; }

        public Dictionary<string, (int, int)> Skills { get; private set; }

        public SpellBook SpellBook { get; }

        public TalentManager TalentManager { get; }

        private IMemoryApi MemoryApi { get; }

        private IWowInterface Wow { get; }

        public void ClickToMove(Vector3 pos, ulong guid,
            WowClickToMoveType clickToMoveType = WowClickToMoveType.Move,
            float turnSpeed = 20.9f, // where is this magic number from?
            float distance = WowClickToMoveDistance.Move)
        {
            if (float.IsInfinity(pos.X) || float.IsNaN(pos.X) || MathF.Abs(pos.X) > 17066.6656
                || float.IsInfinity(pos.Y) || float.IsNaN(pos.Y) || MathF.Abs(pos.Y) > 17066.6656
                || float.IsInfinity(pos.Z) || float.IsNaN(pos.Z) || MathF.Abs(pos.Z) > 17066.6656)
            {
                return;
            }

            MemoryApi.Write(Wow.Offsets.ClickToMoveTurnSpeed, turnSpeed);
            MemoryApi.Write(Wow.Offsets.ClickToMoveDistance, distance);

            if (guid > 0)
                MemoryApi.Write(Wow.Offsets.ClickToMoveGuid, guid);

            MemoryApi.Write(Wow.Offsets.ClickToMoveAction, clickToMoveType);
            MemoryApi.Write(Wow.Offsets.ClickToMoveX, pos);
        }

        public Dictionary<int, int> GetConsumables()
        {
            return Inventory.Items.OfType<WowConsumable>()
                .GroupBy(e => e.Id)
                .ToDictionary(e => e.Key, e => e.Count());
        }

        public bool HasItemTypeInBag<T>(bool needsToBeUseable = false)
        {
            return Inventory.Items.Any(e => Enum.IsDefined(typeof(T), e.Id));
        }

        public bool IsAbleToUseArmor(WowArmor item)
        {
            return item?.ArmorType switch
            {
                WowArmorType.Plate => Skills.Any(e => 
                    e.Key.Equals("Plate Mail", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Plattenpanzer", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.Mail => Skills.Any(e => 
                    e.Key.Equals("Mail", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Panzer", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.Leather => Skills.Any(e =>
                    e.Key.Equals("Leather", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Leder", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.Cloth => Skills.Any(e => 
                    e.Key.Equals("Cloth", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Stoff", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.Totem => Skills.Any(e => 
                    e.Key.Equals("Totem", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Totem", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.Libram => Skills.Any(e => 
                    e.Key.Equals("Libram", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Buchband", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.Idol => Skills.Any(e =>
                    e.Key.Equals("Idol", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Götzen", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.Sigil => Skills.Any(e => 
                    e.Key.Equals("Sigil", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Siegel", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.Shield => Skills.Any(e =>
                    e.Key.Equals("Shield", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Schild", StringComparison.OrdinalIgnoreCase)),
                WowArmorType.Misc => true,
                _ => false,
            };
        }

        public bool IsAbleToUseItem(IWowInventoryItem item)
        {
            return string.Equals(item.Type, "Armor", StringComparison.OrdinalIgnoreCase) && IsAbleToUseArmor((WowArmor)item) 
                   || string.Equals(item.Type, "Weapon", StringComparison.OrdinalIgnoreCase) && IsAbleToUseWeapon((WowWeapon)item);
        }

        public bool IsAbleToUseWeapon(WowWeapon item)
        {
            return item?.WeaponType switch
            {
                WowWeaponType.Bow => Skills.Any(e => 
                    e.Key.Equals("Bows", StringComparison.OrdinalIgnoreCase) 
                    || e.Key.Equals("Bogen", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.Crossbow => Skills.Any(e =>
                    e.Key.Equals("Crossbows", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Armbrüste", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.Gun => Skills.Any(e =>
                    e.Key.Equals("Guns", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Schusswaffen", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.Wand => Skills.Any(e =>
                    e.Key.Equals("Wands", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Zauberstäbe", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.Thrown => Skills.Any(e =>
                    e.Key.Equals("Thrown", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Wurfwaffe", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.Axe => Skills.Any(e =>
                    e.Key.Equals("Axes", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Einhandäxte", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.AxeTwoHand => Skills.Any(e =>
                    e.Key.Equals("Two-Handed Axes", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Zweihandäxte", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.Mace => Skills.Any(e =>
                    e.Key.Equals("Maces", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Einhandstreitkolben", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.MaceTwoHand => Skills.Any(e =>
                    e.Key.Equals("Two-Handed Maces", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Zweihandstreitkolben", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.Sword => Skills.Any(e =>
                    e.Key.Equals("Swords", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Einhandschwerter", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.SwordTwoHand => Skills.Any(e =>
                    e.Key.Equals("Two-Handed Swords", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Zweihandschwerter", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.Dagger => Skills.Any(e =>
                    e.Key.Equals("Daggers", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Dolche", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.Fist => Skills.Any(e =>
                    e.Key.Equals("Fist Weapons", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Faustwaffen", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.Polearm => Skills.Any(e =>
                    e.Key.Equals("Polearms", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Stangenwaffen", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.Staff => Skills.Any(e =>
                    e.Key.Equals("Staves", StringComparison.OrdinalIgnoreCase)
                    || e.Key.Equals("Stäbe", StringComparison.OrdinalIgnoreCase)),
                WowWeaponType.FishingPole => true,
                WowWeaponType.Misc => true,
                _ => false,
            };
        }

        public bool IsItemAnImprovement(IWowInventoryItem item, out IWowInventoryItem itemToReplace)
        {
            itemToReplace = null;

            if (item == null || ItemComparator.IsBlacklistedItem(item))
                return false;

            if (!IsAbleToUseItem(item)) return false;
            if (!GetItemsByEquipLocation(item.EquipLocation, out List<IWowInventoryItem> matchedItems, out _))
                return false;

            // if we don't have an item in the slot or if we only have 3 of 4 bags
            if (matchedItems.Count == 0)
                return true;

            foreach (var inventoryItem in matchedItems
                .Where(invItem => invItem != null && item.Id != invItem.Id && ItemComparator.IsBetter(invItem, item)))
            {
                itemToReplace = inventoryItem;
                return true;
            }

            return false;
        }

        public void Jump()
        {
            AmeisenLogger.I.Log("Movement", $"Jumping", LogLevel.Verbose);
            Task.Run(() => BotUtils.SendKey(MemoryApi.Process.MainWindowHandle, new IntPtr((int)KeyCode.Space), 500, 1000));
        }

        public void MoveToPosition(Vector3 pos, float turnSpeed = 20.9f, float distance = 0.1f)
        {
            ClickToMove(pos, 0, WowClickToMoveType.Move, turnSpeed, distance);
        }

        public void UpdateAll()
        {
            AmeisenLogger.I.Log("CharacterManager", $"Updating full character", LogLevel.Verbose);

            Inventory.Update();
            Equipment.Update();
            SpellBook.Update();
            TalentManager.Update();

            Mounts = Wow.GetMounts();
            Skills = Wow.GetSkills();
            Money = Wow.GetMoney();
        }

        public void UpdateBags()
        {
            IEnumerable<IWowInventoryItem> container = Inventory.Items.Where(item => 
                    item.Type.Equals("container", StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            if (!container.Any()) return;

            for (int slotIndex = 20; slotIndex <= 23; ++slotIndex)
            {
                if (Equipment.Items.Any(kvp =>
                    kvp.Key == (WowEquipmentSlot)slotIndex)) continue;

                Wow.EquipItem(container.First().Name);
                break;
            }
        }

        public void UpdateGear()
        {
            IList equipmentSlots = Enum.GetValues(typeof(WowEquipmentSlot));

            foreach (var equipmentSlot in equipmentSlots)
            {
                WowEquipmentSlot slot = (WowEquipmentSlot)equipmentSlot;

                if (ItemSlotsToSkip.Contains(slot)) continue;

                if (slot == WowEquipmentSlot.INVSLOT_OFFHAND
                    && Equipment.Items.TryGetValue(WowEquipmentSlot.INVSLOT_MAINHAND, out IWowInventoryItem mainHandItem)
                    && mainHandItem.EquipLocation.Contains("INVTYPE_2HWEAPON", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                IEnumerable<IWowInventoryItem> itemsLikeEquipped = Inventory.Items.Where(e => 
                        !string.IsNullOrWhiteSpace(e.EquipLocation) && SlotToEquipLocation((int)slot)
                    .Contains(e.EquipLocation, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(e => e.ItemLevel)
                    .ToList();

                if (!itemsLikeEquipped.Any()) continue;

                if (Equipment.Items.TryGetValue(slot, out IWowInventoryItem equippedItem))
                {
                    for (int f = 0; f < itemsLikeEquipped.Count(); ++f)
                    {
                        IWowInventoryItem item = itemsLikeEquipped.ElementAt(f);

                        if (!IsItemAnImprovement(item, out IWowInventoryItem itemToReplace)) continue;

                        AmeisenLogger.I.Log("Equipment", $"Replacing \"{itemToReplace}\" with \"{item}\"", LogLevel.Verbose);
                        Wow.EquipItem(item.Name);
                        Equipment.Update();
                        break;
                    }
                }
                else
                {
                    IWowInventoryItem itemToEquip = itemsLikeEquipped.First();

                    if ((!string.Equals(itemToEquip.Type, "Armor", StringComparison.OrdinalIgnoreCase) ||
                         !IsAbleToUseArmor((WowArmor)itemToEquip)) &&
                        (!string.Equals(itemToEquip.Type, "Weapon", StringComparison.OrdinalIgnoreCase) ||
                         !IsAbleToUseWeapon((WowWeapon)itemToEquip))) continue;

                    AmeisenLogger.I.Log("Equipment", $"Equipping \"{itemToEquip}\"", LogLevel.Verbose);
                    Wow.EquipItem(itemToEquip.Name);
                    Equipment.Update();
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

        private bool GetItemsByEquipLocation(string equipLocation, out List<IWowInventoryItem> matchedItems, out int expectedItemCount)
        {
            expectedItemCount = 1;
            matchedItems = new List<IWowInventoryItem>();

            switch (equipLocation)
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
            }

            return true;
        }

        private void TryAddAllBags(List<IWowInventoryItem> matchedItems, ref int expectedItemCount)
        {
            TryAddItem(WowEquipmentSlot.CONTAINER_BAG_1, matchedItems);
            TryAddItem(WowEquipmentSlot.CONTAINER_BAG_2, matchedItems);
            TryAddItem(WowEquipmentSlot.CONTAINER_BAG_3, matchedItems);
            TryAddItem(WowEquipmentSlot.CONTAINER_BAG_4, matchedItems);

            expectedItemCount = 4;
        }

        private void TryAddItem(WowEquipmentSlot slot, List<IWowInventoryItem> matchedItems)
        {
            if (Equipment.Items.TryGetValue(slot, out IWowInventoryItem ammoItem))
                matchedItems.Add(ammoItem);
        }

        private void TryAddRings(List<IWowInventoryItem> matchedItems, ref int expectedItemCount)
        {
            TryAddItem(WowEquipmentSlot.INVSLOT_RING1, matchedItems);
            TryAddItem(WowEquipmentSlot.INVSLOT_RING2, matchedItems);

            expectedItemCount = 2;
        }

        private void TryAddTrinkets(List<IWowInventoryItem> matchedItems, ref int expectedItemCount)
        {
            TryAddItem(WowEquipmentSlot.INVSLOT_TRINKET1, matchedItems);
            TryAddItem(WowEquipmentSlot.INVSLOT_TRINKET2, matchedItems);

            expectedItemCount = 2;
        }

        private void TryAddWeapons(List<IWowInventoryItem> matchedItems, ref int expectedItemCount)
        {
            TryAddItem(WowEquipmentSlot.INVSLOT_MAINHAND, matchedItems);
            TryAddItem(WowEquipmentSlot.INVSLOT_OFFHAND, matchedItems);

            expectedItemCount = 2;
        }
    }
}