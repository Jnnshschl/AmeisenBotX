using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Memory;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Character
{
    public class CharacterManager
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private XMemory XMemory { get; }
        private IOffsetList OffsetList { get; }
        private ObjectManager ObjectManager { get; }

        private bool FirstMove { get; set; }

        public CharacterManager(XMemory xMemory, IOffsetList offsetList, ObjectManager objectManager)
        {
            FirstMove = true;
            XMemory = xMemory;
            OffsetList = offsetList;
            ObjectManager = objectManager;
        }

        public void MoveToPosition(WowPosition pos)
        {
            if(FirstMove)
                SendKey(new IntPtr(0x2));

            XMemory.Write(OffsetList.ClickToMoveX, pos.X);
            XMemory.Write(OffsetList.ClickToMoveY, pos.Y);
            XMemory.Write(OffsetList.ClickToMoveZ, pos.Z);
            XMemory.Write(OffsetList.ClickToMoveDistance, 1.5f);
            XMemory.Write(OffsetList.ClickToMoveAction, (int)ClickToMoveType.Move);
        }

        public void Jump() => SendKey(new IntPtr(0x20)); // 0x20 = Spacebar (VK_SPACE)

        public void SendKey(IntPtr vKey, int minDelay = 20, int maxDelay = 40)
        {
            IntPtr windowHandle = XMemory.Process.MainWindowHandle;

            SendMessage(windowHandle, 0x100, vKey, new IntPtr(0));
            Thread.Sleep(new Random().Next(minDelay, maxDelay)); // make it look more human-like :^)
            SendMessage(windowHandle, 0x101, vKey, new IntPtr(0));
        }
    }
}
