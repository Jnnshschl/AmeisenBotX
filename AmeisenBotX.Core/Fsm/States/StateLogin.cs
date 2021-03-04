using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Hook.Modules;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
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

            TracelineJumpHookModule jumpModule = new(null, null, wowInterface);
            jumpModule.Tick = () =>
            {
                // update Traceline Jump Check data
                if (Config.MovementSettings.EnableTracelineJumpCheck && wowInterface.Player != null)
                {
                    Vector3 playerPosition = wowInterface.Player.Position;
                    playerPosition.Z += wowInterface.MovementSettings.ObstacleCheckHeight;

                    Vector3 pos = BotUtils.MoveAhead(playerPosition, wowInterface.Player.Rotation, wowInterface.MovementSettings.ObstacleCheckDistance);

                    wowInterface.XMemory.Write(jumpModule.DataAddress, (1.0f, playerPosition, pos));
                }
            };

            string staticPopupsVarName = BotUtils.FastRandomStringOnlyLetters();
            string battlegroundStatusVarName = BotUtils.FastRandomStringOnlyLetters();
            string handlerName = BotUtils.FastRandomStringOnlyLetters();
            string tableName = BotUtils.FastRandomStringOnlyLetters();
            string eventHookOutput = BotUtils.FastRandomStringOnlyLetters();
            string eventHookFrameName = BotUtils.FastRandomStringOnlyLetters();

            wowInterface.EventHookManager.EventHookFrameName = eventHookFrameName;

            string oldPoupString = string.Empty;
            string oldBattlegroundStatus = string.Empty;

            HookModules = new()
            {
                // Module to process wows events.
                new RunLuaHookModule((x) =>
                {
                    if (wowInterface.XMemory.ReadString(x, Encoding.UTF8, out string s, 8192))
                    {
                        wowInterface.EventHookManager?.OnEventPush(s);
                    }
                }, null, wowInterface, $"{eventHookOutput}='['function {handlerName}(self,a,...)table.insert({tableName},{{time(),a,{{...}}}})end if {eventHookFrameName}==nil then {tableName}={{}}{eventHookFrameName}=CreateFrame(\"FRAME\"){eventHookFrameName}:SetScript(\"OnEvent\",{handlerName})else for b,c in pairs({tableName})do {eventHookOutput}={eventHookOutput}..'{{'for d,e in pairs(c)do if type(e)==\"table\"then {eventHookOutput}={eventHookOutput}..'\"args\": ['for f,g in pairs(e)do {eventHookOutput}={eventHookOutput}..'\"'..g..'\"'if f<=table.getn(e)then {eventHookOutput}={eventHookOutput}..','end end {eventHookOutput}={eventHookOutput}..']}}'if b<table.getn({tableName})then {eventHookOutput}={eventHookOutput}..','end else if type(e)==\"string\"then {eventHookOutput}={eventHookOutput}..'\"event\": \"'..e..'\",'else {eventHookOutput}={eventHookOutput}..'\"time\": \"'..e..'\",'end end end end end {eventHookOutput}={eventHookOutput}..']'{tableName}={{}}", eventHookOutput),

                // Module that does a traceline in front of the character
                // to detect small obstacles that can be jumped over.
                jumpModule,

                // Modules that monitors the STATIC_POPUP windows.
                new RunLuaHookModule((x) =>
                {
                    if (wowInterface.XMemory.ReadString(x, Encoding.UTF8, out string s, 128))
                    {
                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            if (!oldPoupString.Equals(s))
                            {
                                AmeisenLogger.I.Log("StaticPopups", s);
                                oldPoupString = s;
                            }
                        }
                        else
                        {
                            oldPoupString = string.Empty;
                        }
                    }
                }, null, wowInterface, $"{staticPopupsVarName}=\"\"for b=1,STATICPOPUP_NUMDIALOGS do local c=_G[\"StaticPopup\"..b]if c:IsShown()then {staticPopupsVarName}={staticPopupsVarName}..b..\":\"..c.which..\"; \"end end", staticPopupsVarName),

                // Module to monitor the battleground (and queue) status.
                new RunLuaHookModule((x) =>
                {
                    if (wowInterface.XMemory.ReadString(x, Encoding.UTF8, out string s, 128))
                    {
                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            if (!oldBattlegroundStatus.Equals(s))
                            {
                                AmeisenLogger.I.Log("BgStatus", s);
                                oldBattlegroundStatus = s;
                            }
                        }
                        else
                        {
                            oldPoupString = string.Empty;
                        }
                    }
                }, null, wowInterface, $"{battlegroundStatusVarName}=\"\"for b=1,MAX_BATTLEFIELD_QUEUES do local c,d,e,f,g,h=GetBattlefieldStatus(b)local i=GetBattlefieldTimeWaited(b)/1000;{battlegroundStatusVarName}={battlegroundStatusVarName}..b..\":\"..tostring(c or\"unknown\")..\":\"..tostring(d or\"unknown\")..\":\"..tostring(e or\"unknown\")..\":\"..tostring(f or\"unknown\")..\":\"..tostring(g or\"unknown\")..\":\"..tostring(h or\"unknown\")..\":\"..tostring(i or\"unknown\")..\";\"end", battlegroundStatusVarName),
            };
        }

        private List<IHookModule> HookModules { get; }

        private TimegatedEvent LoginAttemptEvent { get; set; }

        public override void Enter()
        {
            if (!WowInterface.HookManager.IsWoWHooked)
            {
                if (!WowInterface.HookManager.Hook(0x7, HookModules))
                {
                    AmeisenLogger.I.Log("StateLogin", "EndsceneHook failed", LogLevel.Error);
                }

                if (Config.AutoSetUlowGfxSettings)
                {
                    SetUlowGfxSettings();
                }
            }

            WowInterface.HookManager.BotOverrideWorldLoadedCheck(true);
        }

        public override void Execute()
        {
            if (!WowInterface.ObjectManager.RefreshIsWorldLoaded())
            {
                if (LoginAttemptEvent.Run())
                {
                    WowInterface.HookManager.LuaDoString($"if CinematicFrame and CinematicFrame:IsShown()then StopCinematic()elseif TOSFrame and TOSFrame:IsShown()then TOSAccept:Enable()TOSAccept:Click()elseif ScriptErrors and ScriptErrors:IsShown()then ScriptErrors:Hide()elseif GlueDialog and GlueDialog:IsShown()then if GlueDialog.which=='OKAY'then GlueDialogButton1:Click()end elseif CharCreateRandomizeButton and CharCreateRandomizeButton:IsVisible()then CharacterCreate_Back()elseif RealmList and RealmList:IsVisible()then for a=1,#GetRealmCategories()do local found=false for b=1,GetNumRealms()do if string.lower(GetRealmInfo(a,b))==string.lower('{Config.Realm}')then ChangeRealm(a,b)RealmList:Hide()found=true break end end if found then break end end elseif CharacterSelectUI and CharacterSelectUI:IsVisible()then if string.find(string.lower(GetServerName()),string.lower('{Config.Realm}'))then CharacterSelect_SelectCharacter({Config.CharacterSlot + 1})CharacterSelect_EnterWorld()elseif RealmList and not RealmList:IsVisible()then CharSelectChangeRealmButton:Click()end elseif AccountLoginUI and AccountLoginUI:IsVisible()then DefaultServerLogin('{Config.Username}','{Config.Password}')end");
                }
            }
            else
            {
                WowInterface.HookManager.BotOverrideWorldLoadedCheck(false);
                StateMachine.SetState(BotState.Idle);
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