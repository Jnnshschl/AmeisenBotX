using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Memory;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace AmeisenBotX.Core.LoginHandler
{
    public class DefaultLoginHandler : ILoginHandler
    {
        public DefaultLoginHandler(XMemory xMemory, IOffsetList offsetList)
        {
            XMemory = xMemory;
            OffsetList = offsetList;
        }

        private IOffsetList OffsetList { get; }

        private XMemory XMemory { get; }

        public bool Login(Process wowProcess, string username, string password, int characterSlot)
        {
            int count = 0;

            if (XMemory.ReadInt(OffsetList.IsWorldLoaded, out int isWorldLoaded))
            {
                while (!XMemory.Process.HasExited && isWorldLoaded == 0)
                {
                    if (count == 7)
                    {
                        return false;
                    }

                    if (XMemory.ReadString(OffsetList.GameState, Encoding.ASCII, out string gameState, 10))
                    {
                        switch (gameState)
                        {
                            case "login":
                                HandleLogin(username, password);
                                count++;
                                break;

                            case "charselect":
                                HandleCharSelect(characterSlot);
                                break;

                            default:
                                count++;
                                break;
                        }
                    }
                    else
                    {
                        count++;
                    }

                    XMemory.ReadInt(OffsetList.IsWorldLoaded, out isWorldLoaded);
                }

                XMemory.Process.WaitForInputIdle();
                return true;
            }

            return false;
        }

        private void HandleCharSelect(int characterSlot)
        {
            if (XMemory.ReadInt(OffsetList.CharacterSlotSelected, out int currentSlot))
            {
                bool failed = false;
                while (!failed && currentSlot != characterSlot)
                {
                    BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr(0x28));
                    Thread.Sleep(200);
                    failed = XMemory.ReadInt(OffsetList.CharacterSlotSelected, out currentSlot);
                }

                BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr(0x0D));
            }
        }

        private void HandleLogin(string username, string password)
        {
            foreach (char c in username)
            {
                BotUtils.SendKeyShift(XMemory.Process.MainWindowHandle, new IntPtr(c), char.IsUpper(c));
                Thread.Sleep(10);
            }

            Thread.Sleep(100);
            BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr(0x09));
            Thread.Sleep(100);

            bool firstTime = true;
            string gameState;
            bool result;

            do
            {
                if (!firstTime)
                {
                    BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr(0x0D));
                }

                foreach (char c in password)
                {
                    BotUtils.SendKeyShift(XMemory.Process.MainWindowHandle, new IntPtr(c), char.IsUpper(c));
                    Thread.Sleep(10);
                }

                Thread.Sleep(500);
                BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr(0x0D));
                Thread.Sleep(5000);

                firstTime = false;

                result = XMemory.ReadString(OffsetList.GameState, Encoding.ASCII, out gameState, 10);
            } while (result && gameState == "login");
        }
    }
}