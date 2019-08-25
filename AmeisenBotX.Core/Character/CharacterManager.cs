using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Memory;
using AmeisenBotX.Pathfinding;
using System;

namespace AmeisenBotX.Core.Character
{
    public class CharacterManager
    {
        public CharacterManager(XMemory xMemory, IOffsetList offsetList, ObjectManager objectManager)
        {
            FirstMove = true;
            XMemory = xMemory;
            OffsetList = offsetList;
            ObjectManager = objectManager;
        }

        private bool FirstMove { get; set; }
        private ObjectManager ObjectManager { get; }
        private IOffsetList OffsetList { get; }
        private XMemory XMemory { get; }

        public void AntiAfk() => XMemory.Write(OffsetList.TickCount, Environment.TickCount);

        public void Jump() => BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr(0x20));

        public void MoveToPosition(WowPosition pos)
        {
            if (FirstMove)
                BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr(0x2));

            XMemory.Write(OffsetList.ClickToMoveX, pos.X);
            XMemory.Write(OffsetList.ClickToMoveY, pos.Y);
            XMemory.Write(OffsetList.ClickToMoveZ, pos.Z);
            XMemory.Write(OffsetList.ClickToMoveDistance, 1.5f);
            XMemory.Write(OffsetList.ClickToMoveAction, (int)ClickToMoveType.Move);
        }

        // 0x20 = Spacebar (VK_SPACE)
    }
}