using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Character.Spells;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Memory;
using AmeisenBotX.Pathfinding.Objects;
using System;

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

            Inventory = new CharacterInventory(hookManager);
            Equipment = new CharacterEquipment(hookManager);
            SpellBook = new SpellBook(hookManager);
            Comparator = new ItemLevelComparator();
            UpdateMoney();
        }

        private void UpdateMoney()
        {
            string rawMoney = HookManager.GetMoney();
            if(int.TryParse(rawMoney, out int money))
            {
                Money = money;
            }
        }

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

        public void Jump() => BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr(0x20));

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

        private void HandleInputSimulationMovement(Vector3 pos)
        {
            // TODO
        }

        public bool IsAbleToUseArmor(WowArmor item)
        {
            return true;
        }

        public bool IsAbleToUseWeapon(WowWeapon item)
        {
            return true;
        }

        public bool DoINeedThatItem(IWowItem item)
        {
            return false;

            // WIP
            //// if ((item.GetType() == typeof(WowArmor) && IsAbleToUseArmor((WowArmor)item))
            ////     || (item.GetType() == typeof(WowWeapon) && IsAbleToUseWeapon((WowWeapon)item)))
            //// {
            ////     return Comparator.IsBetter(null, item);
            //// }
            //// else
            //// {
            ////     return false;
            //// }
        }
    }
}