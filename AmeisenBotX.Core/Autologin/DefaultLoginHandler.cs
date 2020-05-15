using AmeisenBotX.Core.Common;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace AmeisenBotX.Core.Autologin
{
    public class DefaultLoginHandler : ILoginHandler
    {
        public DefaultLoginHandler(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

        private WowInterface WowInterface { get; }

        public bool Login(Process wowProcess, string username, string password, int characterSlot)
        {
            int count = 0;

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.IsWorldLoaded, out int isWorldLoaded))
            {
                while (!WowInterface.XMemory.Process.HasExited && isWorldLoaded == 0)
                {
                    if (count == 7)
                    {
                        return false;
                    }

                    if (WowInterface.XMemory.ReadString(WowInterface.OffsetList.GameState, Encoding.ASCII, out string gameState, 10))
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

                    WowInterface.XMemory.Read(WowInterface.OffsetList.IsWorldLoaded, out isWorldLoaded);
                }

                if (WowInterface.XMemory.Process != null && !WowInterface.XMemory.Process.HasExited)
                {
                    WowInterface.XMemory.Process?.WaitForInputIdle();
                }

                return true;
            }

            return false;
        }

        private void HandleCharSelect(int characterSlot)
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.CharacterSlotSelected, out int currentSlot))
            {
                bool failed = false;
                while (!failed && currentSlot != characterSlot)
                {
                    BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr(0x28));
                    Thread.Sleep(1000);
                    failed = WowInterface.XMemory.Read(WowInterface.OffsetList.CharacterSlotSelected, out currentSlot);
                }

                BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr(0x0D));
            }
        }

        private void HandleLogin(string username, string password)
        {
            foreach (char c in username)
            {
                BotUtils.SendKeyShift(WowInterface.WowProcess.MainWindowHandle, new IntPtr(c), char.IsUpper(c));
                Thread.Sleep(50);
            }

            Thread.Sleep(100);
            BotUtils.SendKey(WowInterface.WowProcess.MainWindowHandle, new IntPtr(0x09));
            Thread.Sleep(100);

            bool firstTime = true;
            string gameState;
            bool result;

            do
            {
                if (!firstTime)
                {
                    BotUtils.SendKey(WowInterface.WowProcess.MainWindowHandle, new IntPtr(0x0D));
                }

                foreach (char c in password)
                {
                    BotUtils.SendKeyShift(WowInterface.WowProcess.MainWindowHandle, new IntPtr(c), char.IsUpper(c));
                    Thread.Sleep(10);
                }

                Thread.Sleep(500);
                BotUtils.SendKey(WowInterface.WowProcess.MainWindowHandle, new IntPtr(0x0D));
                Thread.Sleep(5000);

                firstTime = false;

                result = WowInterface.XMemory.ReadString(WowInterface.OffsetList.GameState, Encoding.ASCII, out gameState, 10);
            } while (result && gameState == "login");
        }
    }
}