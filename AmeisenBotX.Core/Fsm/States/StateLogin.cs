using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Hook.Modules;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateLogin : BasicState
    {
        public StateLogin(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            LoginAttemptEvent = new(TimeSpan.FromMilliseconds(500));            
        }

        private TimegatedEvent LoginAttemptEvent { get; set; }

        public override void Enter()
        {
            if (!WowInterface.NewWowInterface.IsWoWHooked)
            {
                if (!WowInterface.NewWowInterface.Setup())
                {
                    AmeisenLogger.I.Log("StateLogin", "EndsceneHook failed", LogLevel.Error);
                }

                if (Config.AutoSetUlowGfxSettings)
                {
                    SetUlowGfxSettings();
                }
            }

            WowInterface.NewWowInterface.BotOverrideWorldLoadedCheck(true);
        }

        public override void Execute()
        {
            if (!WowInterface.Objects.IsWorldLoaded)
            {
                if (LoginAttemptEvent.Run())
                {
                    WowInterface.NewWowInterface.LuaDoString($"if CinematicFrame and CinematicFrame:IsShown()then StopCinematic()elseif TOSFrame and TOSFrame:IsShown()then TOSAccept:Enable()TOSAccept:Click()elseif ScriptErrors and ScriptErrors:IsShown()then ScriptErrors:Hide()elseif GlueDialog and GlueDialog:IsShown()then if GlueDialog.which=='OKAY'then GlueDialogButton1:Click()end elseif CharCreateRandomizeButton and CharCreateRandomizeButton:IsVisible()then CharacterCreate_Back()elseif RealmList and RealmList:IsVisible()then for a=1,#GetRealmCategories()do local found=false for b=1,GetNumRealms()do if string.lower(GetRealmInfo(a,b))==string.lower('{Config.Realm}')then ChangeRealm(a,b)RealmList:Hide()found=true break end end if found then break end end elseif CharacterSelectUI and CharacterSelectUI:IsVisible()then if string.find(string.lower(GetServerName()),string.lower('{Config.Realm}'))then CharacterSelect_SelectCharacter({Config.CharacterSlot + 1})CharacterSelect_EnterWorld()elseif RealmList and not RealmList:IsVisible()then CharSelectChangeRealmButton:Click()end elseif AccountLoginUI and AccountLoginUI:IsVisible()then DefaultServerLogin('{Config.Username}','{Config.Password}')end");
                }
            }
            else
            {
                WowInterface.NewWowInterface.BotOverrideWorldLoadedCheck(false);
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Leave()
        {
        }

        private void SetUlowGfxSettings()
        {
            WowInterface.NewWowInterface.LuaDoString("SetCVar(\"gxcolorbits\",\"16\");SetCVar(\"gxdepthbits\",\"16\");SetCVar(\"skycloudlod\",\"0\");SetCVar(\"particledensity\",\"0.3\");SetCVar(\"lod\",\"0\");SetCVar(\"mapshadows\",\"0\");SetCVar(\"maxlights\",\"0\");SetCVar(\"specular\",\"0\");SetCVar(\"waterlod\",\"0\");SetCVar(\"basemip\",\"1\");SetCVar(\"shadowlevel\",\"1\")");
        }
    }
}