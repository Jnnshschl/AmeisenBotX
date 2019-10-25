using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Memory;
using AmeisenBotX.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Character
{
    public class CharacterManager
    {
        public CharacterManager(XMemory xMemory, IOffsetList offsetList, ObjectManager objectManager, HookManager hookManager)
        {
            FirstMove = true;
            XMemory = xMemory;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            HookManager = hookManager;
            Inventory = new CharacterInventory(hookManager);
            Equipment = new CharacterEquipment(hookManager);
            Comparator = new ItemLevelComparator();
        }

        private bool FirstMove { get; set; }

        private ObjectManager ObjectManager { get; }

        private HookManager HookManager { get; }

        private IOffsetList OffsetList { get; }

        private XMemory XMemory { get; }

        private IWowItemComparator Comparator { get; }

        public CharacterInventory Inventory { get; }

        public CharacterEquipment Equipment { get; }

        public void AntiAfk() => XMemory.Write(OffsetList.TickCount, Environment.TickCount);

        public void Jump() => BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr(0x20));

        public void MoveToPosition(Vector3 pos)
        {
            // if we dont do this, ClickToMove wont work
            // TODO: find better way to fix initial CTM bug
            if (FirstMove)
            {
                BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr(0x2));
            }

            XMemory.Write(OffsetList.ClickToMoveX, pos.X);
            XMemory.Write(OffsetList.ClickToMoveY, pos.Y);
            XMemory.Write(OffsetList.ClickToMoveZ, pos.Z);
            XMemory.Write(OffsetList.ClickToMoveDistance, 1.5f);
            XMemory.Write(OffsetList.ClickToMoveAction, (int)ClickToMoveType.Move);
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