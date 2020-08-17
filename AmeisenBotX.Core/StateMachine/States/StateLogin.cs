using AmeisenBotX.Core.Common;
using AmeisenBotX.Logging;
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

                if (Config.AutoSetUlowGfxSettings)
                {
                    SetUlowGfxSettings();
                }
            }

            if (WowInterface.ObjectManager.RefreshIsWorldLoaded())
            {
                StateMachine.SetState(BotState.Idle);
                WowInterface.HookManager.OverrideWorldCheckOff();
                return;
            }
        }

        public override void Execute()
        {
            WowInterface.HookManager.OverrideWorldCheckOn();

            if (LoginAttemptEvent.Run())
            {
                if (WowInterface.XMemory.ReadString(WowInterface.OffsetList.GameState, Encoding.UTF8, out string gameState))
                {
                    AmeisenLogger.I.Log("Login", $"Gamestate is: {gameState}");

                    switch (gameState.ToUpper())
                    {
                        case "LOGIN":
                            WowInterface.HookManager.LuaDoString($"if AccountLoginUI and AccountLoginUI:IsVisible()then DefaultServerLogin('{Config.Username}','{Config.Password}')elseif RealmList and RealmList:IsVisible()then for a=1,select('#',GetRealmCategories()),1 do local b=GetNumRealms(a)for c=1,b,1 do local d,e=GetRealmInfo(a,c)if d~=nil and d=='{Config.Realm}'then ChangeRealm(a,c)RealmList:Hide()end end end end");
                            ++LoginCounter;
                            break;

                        case "CHARSELECT":
                            AmeisenLogger.I.Log("Login", $"Selecting character slot: {Config.CharacterSlot}");
                            WowInterface.HookManager.LuaDoString($"if CharacterSelectUI and CharacterSelectUI:IsVisible()then CharacterSelect_SelectCharacter({Config.CharacterSlot})EnterWorld();elseif CharCreateRandomizeButton and CharCreateRandomizeButton:IsVisible()then CharacterCreate_Back()end");
                            break;

                        default:
                            break;
                    }
                }

                if (WowInterface.ObjectManager.RefreshIsWorldLoaded())
                {
                    StateMachine.SetState(BotState.Idle);
                    WowInterface.HookManager.OverrideWorldCheckOff();
                    return;
                }

                // WowInterface.HookManager.LuaDoString($"if(AccountLoginUI and AccountLoginUI:IsVisible()) then DefaultServerLogin('{Config.Username}', '{Config.Password}');elseif (RealmList and RealmList:IsVisible()) then for i = 1, select('#', GetRealmCategories()), 1 do local numRealms = GetNumRealms(i);for j = 1, numRealms, 1 do local name, numCharacters = GetRealmInfo(i, j);if (name ~= nil and name == '{Config.Realm}') then ChangeRealm(i,j); RealmList:Hide();end end end elseif(CharacterSelectUI and CharacterSelectUI:IsVisible()) then CharacterSelect_SelectCharacter({Config.CharacterSlot});EnterWorld();elseif(CharCreateRandomizeButton and CharCreateRandomizeButton:IsVisible()) then CharacterCreate_Back();end;");

                if (LoginCounter > 4)
                {
                    // sometimes gettin stuck when worldserver is down, but we cheese this
                    BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr(0x1B));
                    BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr(0x1B));
                    LoginCounter = 0;
                }
            }
        }

        public override void Leave()
        {
        }

        private void SetUlowGfxSettings()
        {
            WowInterface.HookManager.LuaDoString("SetCVar(\"gxcolorbits\",\"16\");SetCVar(\"gxdepthbits\",\"16\");SetCVar(\"skycloudlod\",\"0\");SetCVar(\"particledensity\",\"0.3\");SetCVar(\"lod\",\"0\");SetCVar(\"mapshadows\",\"0\");SetCVar(\"maxlights\",\"0\");SetCVar(\"specular\",\"0\");SetCVar(\"waterlod\",\"0\");SetCVar(\"basemip\",\"1\");SetCVar(\"shadowlevel\",\"1\")");
        }
    }
}