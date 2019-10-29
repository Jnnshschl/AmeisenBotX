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
            FirstMove = true;
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
        }

        public void UpdateAll()
        {
            Inventory.Update();
            Equipment.Update();
            SpellBook.Update();
            UpdateMoney();
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

        private bool FirstMove { get; set; }

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

        public void AntiAfk() => XMemory.Write(OffsetList.TickCount, Environment.TickCount);

        public void Jump() => BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_SPACE));

        public void MoveToPosition(Vector3 pos)
        {
            if (Config.UseClickToMove)
            {
                XMemory.Write(OffsetList.ClickToMoveX, pos.X);
                XMemory.Write(OffsetList.ClickToMoveY, pos.Y);
                XMemory.Write(OffsetList.ClickToMoveZ, pos.Z);
                XMemory.Write(OffsetList.ClickToMoveDistance, 1.5f);
                XMemory.Write(OffsetList.ClickToMoveAction, (int)ClickToMoveType.Move);
            }
            else
            {
                HandleInputSimulationMovement(pos);
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

        public void StopMovement()
        {
            foreach (KeyValuePair<VirtualKeys, bool> keyValuePair in KeyMap.Where(e => e.Value == true))
            {
                BotUtils.RealeaseKey(XMemory.Process.MainWindowHandle, new IntPtr((int)keyValuePair.Key));
            }
        }

        public bool IsAbleToUseArmor(WowArmor item)
        {
            switch (item.ArmorType)
            {
                case ArmorType.PLATE:
                    return SpellBook.IsSpellKnown("Plate Mail") || SpellBook.IsSpellKnown("Plattenpanzer");

                case ArmorType.MAIL:
                    return SpellBook.IsSpellKnown("Mail") || SpellBook.IsSpellKnown("Panzer");

                case ArmorType.LEATHER:
                    return SpellBook.IsSpellKnown("Leather") || SpellBook.IsSpellKnown("Leder");

                case ArmorType.CLOTH:
                    return SpellBook.IsSpellKnown("Cloth") || SpellBook.IsSpellKnown("Stoff");

                case ArmorType.TOTEMS:
                    return SpellBook.IsSpellKnown("Totem") || SpellBook.IsSpellKnown("Totem");

                case ArmorType.LIBRAMS:
                    return SpellBook.IsSpellKnown("Libram") || SpellBook.IsSpellKnown("Buchband");

                case ArmorType.IDOLS:
                    return SpellBook.IsSpellKnown("Idol") || SpellBook.IsSpellKnown("Götzen");

                case ArmorType.SIGILS:
                    return SpellBook.IsSpellKnown("Sigil") || SpellBook.IsSpellKnown("Siegel");

                case ArmorType.SHIEDLS:
                    return SpellBook.IsSpellKnown("Shield") || SpellBook.IsSpellKnown("Schild");

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
                    return SpellBook.IsSpellKnown("Bows") || SpellBook.IsSpellKnown("Bogen");

                case WeaponType.CROSSBOWS:
                    return SpellBook.IsSpellKnown("Crossbows") || SpellBook.IsSpellKnown("Armbrüste");

                case WeaponType.GUNS:
                    return SpellBook.IsSpellKnown("Guns") || SpellBook.IsSpellKnown("Schusswaffen");

                case WeaponType.WANDS:
                    return SpellBook.IsSpellKnown("Wands") || SpellBook.IsSpellKnown("Zauberstäbe");

                case WeaponType.THROWN:
                    return SpellBook.IsSpellKnown("Thrown") || SpellBook.IsSpellKnown("Wurfwaffe");

                case WeaponType.ONEHANDED_AXES:
                    return SpellBook.IsSpellKnown("One-Handed Axes") || SpellBook.IsSpellKnown("Einhandäxte");

                case WeaponType.TWOHANDED_AXES:
                    return SpellBook.IsSpellKnown("Two-Handed Axes") || SpellBook.IsSpellKnown("Zweihandäxte");

                case WeaponType.ONEHANDED_MACES:
                    return SpellBook.IsSpellKnown("One-Handed Maces") || SpellBook.IsSpellKnown("Einhandstreitkolben");

                case WeaponType.TWOHANDED_MACES:
                    return SpellBook.IsSpellKnown("Two-Handed Maces") || SpellBook.IsSpellKnown("Zweihandstreitkolben");

                case WeaponType.ONEHANDED_SWORDS:
                    return SpellBook.IsSpellKnown("One-Handed Swords") || SpellBook.IsSpellKnown("Einhandschwerter");

                case WeaponType.TWOHANDED_SWORDS:
                    return SpellBook.IsSpellKnown("Two-Handed Swords") || SpellBook.IsSpellKnown("Zweihandschwerter");

                case WeaponType.DAGGERS:
                    return SpellBook.IsSpellKnown("Daggers") || SpellBook.IsSpellKnown("Dolche");

                case WeaponType.FIST_WEAPONS:
                    return SpellBook.IsSpellKnown("Fist Weapons") || SpellBook.IsSpellKnown("Faustwaffen");

                case WeaponType.POLEARMS:
                    return SpellBook.IsSpellKnown("Polearms") || SpellBook.IsSpellKnown("Stangenwaffen");

                case WeaponType.STAVES:
                    return SpellBook.IsSpellKnown("Staves") || SpellBook.IsSpellKnown("Stäbe");

                case WeaponType.FISHING_POLES:
                    return true;

                case WeaponType.MISCELLANEOUS:
                    return true;

                default:
                    return false;
            }
        }

        public bool DoINeedThatItem(IWowItem item)
        {
            if ((item.GetType() == typeof(WowArmor) && IsAbleToUseArmor((WowArmor)item))
                || (item.GetType() == typeof(WowWeapon) && IsAbleToUseWeapon((WowWeapon)item)))
            {
                if (GetMatchingItem(item, out IWowItem matchingItem))
                {
                    // roll need
                    return matchingItem == null || Comparator.IsBetter(matchingItem, item);
                }
            }

            // roll greed
            return false;
        }

        private bool GetMatchingItem(IWowItem item, out IWowItem matchingItem)
        {
            matchingItem = null;

            if (item.GetType() == typeof(WowArmor) || item.GetType() == typeof(WowWeapon))
            {
                if (Equipment.Equipment.TryGetValue(item.EquipLocation, out IWowItem matchedItem))
                {
                    matchingItem = matchedItem;
                }

                return true;
            }

            return false;
        }

        private void HandleInputSimulationMovement(Vector3 positionToMoveTo)
        {
            double angleDiff = BotMath.GetFacingAngle(ObjectManager.Player.Position, positionToMoveTo);
        }
    }
}