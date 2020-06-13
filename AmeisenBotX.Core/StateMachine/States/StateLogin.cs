using AmeisenBotX.Core.Common;
using System;
using System.Text;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateLogin : BasicState
    {
        public StateLogin(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            LoginAttemptEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
        }

        private TimegatedEvent LoginAttemptEvent { get; set; }

        private int LoginCounter { get; set; }

        public override void Enter()
        {
            if (!WowInterface.HookManager.IsWoWHooked)
            {
                WowInterface.HookManager.SetupEndsceneHook();
            }
        }

        public override void Execute()
        {
            if (LoginAttemptEvent.Run())
            {
                WowInterface.HookManager.OverrideWorldCheckOn();

                if (WowInterface.XMemory.ReadString(WowInterface.OffsetList.GameState, Encoding.UTF8, out string gameState))
                {
                    switch (gameState.ToUpper())
                    {
                        case "LOGIN":
                            WowInterface.HookManager.LuaDoString($"if(AccountLoginUI and AccountLoginUI:IsVisible()) then DefaultServerLogin('{Config.Username}', '{Config.Password}');elseif (RealmList and RealmList:IsVisible()) then for i = 1, select('#', GetRealmCategories()), 1 do local numRealms = GetNumRealms(i);for j = 1, numRealms, 1 do local name, numCharacters = GetRealmInfo(i, j);if (name ~= nil and name == '{Config.Realm}') then ChangeRealm(i,j); RealmList:Hide();end end end end");
                            ++LoginCounter;
                            break;

                        case "CHARSELECT":
                            WowInterface.HookManager.LuaDoString($"if(CharacterSelectUI and CharacterSelectUI:IsVisible()) then CharacterSelect_SelectCharacter({Config.CharacterSlot});EnterWorld();end");
                            StateMachine.SetState((int)BotState.Idle);
                            break;

                        default:
                            break;
                    }
                }

                //WowInterface.HookManager.LuaDoString($"if(AccountLoginUI and AccountLoginUI:IsVisible()) then DefaultServerLogin('{Config.Username}', '{Config.Password}');elseif (RealmList and RealmList:IsVisible()) then for i = 1, select('#', GetRealmCategories()), 1 do local numRealms = GetNumRealms(i);for j = 1, numRealms, 1 do local name, numCharacters = GetRealmInfo(i, j);if (name ~= nil and name == '{Config.Realm}') then ChangeRealm(i,j); RealmList:Hide();end end end elseif(CharacterSelectUI and CharacterSelectUI:IsVisible()) then CharacterSelect_SelectCharacter({Config.CharacterSlot});EnterWorld();elseif(CharCreateRandomizeButton and CharCreateRandomizeButton:IsVisible()) then CharacterCreate_Back();end;");
                WowInterface.HookManager.OverrideWorldCheckOff();

                if (LoginCounter > 4)
                {
                    // sometimes gettin stuck when worldserver is down, but we cheese this
                    BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr(0x1B));
                    BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr(0x1B));
                    LoginCounter = 0;
                }
            }
        }

        public override void Exit()
        {
        }
    }
}