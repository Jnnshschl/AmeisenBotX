using AmeisenBotX.Core.Common;
using System;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateLogin : BasicState
    {
        public StateLogin(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        private DateTime LastLoginAttempt { get; set; }

        private int LoginCounter { get; set; }

        public override void Enter()
        {
            if (!WowInterface.HookManager.IsWoWHooked)
            {
                WowInterface.HookManager.SetupEndsceneHook();
            }

            // wait for one second to start the login
            LastLoginAttempt = DateTime.Now;
        }

        public override void Execute()
        {
            if (DateTime.Now - LastLoginAttempt > TimeSpan.FromSeconds(1))
            {
                LastLoginAttempt = DateTime.Now;
                WowInterface.HookManager.OverrideWorldCheckOn();
                WowInterface.HookManager.LuaDoString($"if(AccountLoginUI and AccountLoginUI:IsVisible()) then DefaultServerLogin('{Config.Username}', '{Config.Password}');elseif (RealmList and RealmList:IsVisible()) then for i = 1, select('#', GetRealmCategories()), 1 do local numRealms = GetNumRealms(i);for j = 1, numRealms, 1 do local name, numCharacters = GetRealmInfo(i, j);if (name ~= nil and name == '{Config.Realm}') then ChangeRealm(i,j); RealmList:Hide();end end end elseif(CharacterSelectUI and CharacterSelectUI:IsVisible()) then CharacterSelect_SelectCharacter({Config.CharacterSlot});EnterWorld();elseif(CharCreateRandomizeButton and CharCreateRandomizeButton:IsVisible()) then CharacterCreate_Back();end;");
                WowInterface.HookManager.OverrideWorldCheckOff();

                if (LoginCounter > 4)
                {
                    // sometimes gettin stuck when worldserver is down, but we cheese this
                    BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr(0x1B));
                    BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr(0x1B));
                    LoginCounter = 0;
                }

                ++LoginCounter;
            }
            else if (WowInterface.XMemory.Read(WowInterface.OffsetList.IsWorldLoaded, out int isIngame)
                     && isIngame == 1)
            {
                StateMachine.SetState(BotState.Idle);
            }

            // old login method

            // if (WowInterface.LoginHandler.Login(WowInterface.XMemory.Process, Config.Username, Config.Password, Config.CharacterSlot))
            // {
            //     if (WowInterface.XMemory.Read(WowInterface.OffsetList.ChatOpened, out byte isChatOpened)
            //         && isChatOpened == 0x1)
            //     {
            //         // send enter to close the chat lmao
            //         BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr(0x0D));
            //     }
            //     else
            //     {
            //         StateMachine.SetState(BotState.Idle);
            //     }
            // }
            // else
            // {
            //     WowInterface.XMemory.Process.Kill();
            //     StateMachine.SetState(BotState.None);
            // }
        }

        public override void Exit()
        {
        }
    }
}