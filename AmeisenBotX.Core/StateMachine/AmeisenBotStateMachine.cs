using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine
{
    public class AmeisenBotStateMachine : AbstractStateMachine<BasicState>
    {
        public AmeisenBotStateMachine(string botDataPath, AmeisenBotConfig config, WowInterface wowInterface)
        {
            AmeisenLogger.Instance.Log("StateMachine", "Starting AmeisenBotStateMachine", LogLevel.Verbose);

            BotDataPath = botDataPath;
            Config = config;
            WowInterface = wowInterface;

            LastState = (int)BotState.None;

            States = new Dictionary<int, BasicState>()
            {
                { (int)BotState.None, new StateNone(this, config, WowInterface) },
                { (int)BotState.Attacking, new StateAttacking(this, config, WowInterface) },
                { (int)BotState.Battleground, new StateBattleground(this, config, WowInterface) },
                { (int)BotState.Dead, new StateDead(this, config, WowInterface) },
                { (int)BotState.Dungeon, new StateDungeon(this, config, WowInterface) },
                { (int)BotState.Eating, new StateEating(this, config, WowInterface) },
                { (int)BotState.Following, new StateFollowing(this, config, WowInterface) },
                { (int)BotState.Ghost, new StateGhost(this, config, WowInterface) },
                { (int)BotState.Idle, new StateIdle(this, config, WowInterface) },
                { (int)BotState.InsideAoeDamage, new StateInsideAoeDamage(this, config, WowInterface) },
                { (int)BotState.Job, new StateJob(this, config, WowInterface) },
                { (int)BotState.LoadingScreen, new StateLoadingScreen(this, config, WowInterface) },
                { (int)BotState.Login, new StateLogin(this, config, WowInterface) },
                { (int)BotState.Looting, new StateLooting(this, config, WowInterface) },
                { (int)BotState.Questing, new StateQuesting(this, config, WowInterface) },
                { (int)BotState.Repairing, new StateRepairing(this, config, WowInterface) },
                { (int)BotState.Selling, new StateSelling(this, config, WowInterface) },
                { (int)BotState.StartWow, new StateStartWow(this, config, WowInterface) }
            };

            AntiAfkEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(Config.AntiAfkMs), WowInterface.CharacterManager.AntiAfk);
            EventPullEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(Config.EventPullMs), WowInterface.EventHookManager.Pull);
            GhostCheckEvent = new TimegatedEvent<bool>(TimeSpan.FromMilliseconds(Config.GhostCheckMs), () => WowInterface.ObjectManager.Player.Health == 1 && WowInterface.HookManager.IsGhost(WowLuaUnit.Player));

            RenderSwitchEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            CurrentState = States.First();
            CurrentState.Value.Enter();

            OnStateMachineStateChanged += () => WowInterface.MovementEngine.Reset();
        }

        public override event StateMachineTick OnStateMachineTick;

        public override event StateMachineOverride OnStateOverride;

        public string BotDataPath { get; }

        public MapId LastDiedMap { get; internal set; }

        public Vector3 LastDiedPosition { get; internal set; }

        public string PlayerName { get; internal set; }

        public bool WowCrashed { get; internal set; }

        internal WowInterface WowInterface { get; }

        private TimegatedEvent AntiAfkEvent { get; set; }

        private AmeisenBotConfig Config { get; }

        private TimegatedEvent EventPullEvent { get; set; }

        private TimegatedEvent<bool> GhostCheckEvent { get; set; }

        private TimegatedEvent RenderSwitchEvent { get; set; }

        public BotState StateOverride { get; set; }

        public override void Execute()
        {
            // we cant do anything if wow has crashed
            if ((WowInterface.XMemory.Process == null || WowInterface.XMemory.Process.HasExited)
                && SetState((int)BotState.None))
            {
                AmeisenLogger.Instance.Log("StateMachine", "WoW crashed", LogLevel.Verbose);

                WowCrashed = true;
                ((StateIdle)States[(int)BotState.Idle]).FirstStart = true;

                WowInterface.MovementEngine.Reset();
                WowInterface.ObjectManager.WowObjects.Clear();
                WowInterface.EventHookManager.Stop();

                return;
            }

            bool setStateOverride = true;

            // ingame override states
            if (CurrentState.Key != (int)BotState.None
                && CurrentState.Key != (int)BotState.StartWow
                && CurrentState.Key != (int)BotState.Login
                && WowInterface.ObjectManager != null)
            {
                WowInterface.ObjectManager.RefreshIsWorldLoaded();

                if (!WowInterface.ObjectManager.IsWorldLoaded)
                {
                    setStateOverride = false;

                    if (SetState((int)BotState.LoadingScreen, true))
                    {
                        OnStateOverride?.Invoke(CurrentState.Key);
                        AmeisenLogger.Instance.Log("StateMachine", "World is not loaded", LogLevel.Verbose);
                        return;
                    }
                }
                else
                {
                    WowInterface.ObjectManager.UpdateWowObjects();
                    EventPullEvent.Run();

                    if (WowInterface.ObjectManager.Player != null)
                    {
                        if (!WowInterface.ObjectManager.Player.IsCasting)
                        {
                            WowInterface.MovementEngine.Execute();
                        }

                        if (WowInterface.ObjectManager.Player.IsDead)
                        {
                            // we are dead, state needs to release the spirit
                            setStateOverride = false;

                            if (SetState((int)BotState.Dead, true))
                            {
                                OnStateOverride?.Invoke(CurrentState.Key);
                                return;
                            }
                        }
                        else if (GhostCheckEvent.Run(out bool isGhost)
                            && isGhost)
                        {
                            // we cant be a ghost if we are still dead
                            setStateOverride = false;

                            if (SetState((int)BotState.Ghost, true))
                            {
                                OnStateOverride?.Invoke(CurrentState.Key);
                                return;
                            }
                        }

                        // we cant fight nor do we receive damage when we are dead or a ghost
                        // so ignore these overrides
                        if (CurrentState.Key != (int)BotState.Dead
                            && CurrentState.Key != (int)BotState.Ghost)
                        {
                            // if (Config.AutoDodgeAoeSpells
                            //     && BotUtils.IsPositionInsideAoeSpell(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.GetNearAoeSpells())
                            //     && SetState(BotState.InsideAoeDamage, true))
                            // {
                            //     OnStateOverride(CurrentState.Key);
                            //     return;
                            // }

                            // TODO: handle combat bug, sometimes when combat ends, the player stays in combat for no reason
                            if (!WowInterface.Globals.IgnoreCombat
                                && (WowInterface.ObjectManager.Player.IsInCombat
                                    || WowInterface.Globals.ForceCombat
                                    || IsAnyPartymemberInCombat()))
                            {
                                setStateOverride = false;

                                if (SetState((int)BotState.Attacking, true))
                                {
                                    OnStateOverride?.Invoke(CurrentState.Key);
                                    return;
                                }
                            }
                        }
                    }
                }

                if (setStateOverride)
                {
                    SetState((int)StateOverride);
                }
            }

            AntiAfkEvent.Run();

            // auto disable rendering when not in focus
            if (Config.AutoDisableRender && RenderSwitchEvent.Run())
            {
                IntPtr foregroundWindow = WowInterface.XMemory.GetForegroundWindow();
                WowInterface.HookManager.SetRenderState(foregroundWindow == WowInterface.XMemory.Process.MainWindowHandle);
            }

            // execute the State and Movement
            CurrentState.Value.Execute();
            OnStateMachineTick?.Invoke();
        }

        internal IEnumerable<WowUnit> GetNearLootableUnits()
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .Where(e => e.IsLootable
                                && !((StateLooting)States[12]).UnitsAlreadyLootedList.Contains(e.Guid)
                                && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.LootUnitsRadius);
        }

        internal bool IsAnyPartymemberInCombat()
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>()
                       .Where(e => WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid))
                       .Any(r => r.IsInCombat);
        }

        internal bool IsBattlegroundMap(MapId map)
        {
            return map == MapId.AlteracValley
                       || map == MapId.WarsongGulch
                       || map == MapId.ArathiBasin
                       || map == MapId.EyeOfTheStorm
                       || map == MapId.StrandOfTheAncients;
        }

        internal bool IsCapitalCityZone(ZoneId zone)
        {
            if (WowInterface.ObjectManager.Player.IsAlliance())
            {
                return zone == ZoneId.StormwindCity
                            || zone == ZoneId.Ironforge
                            || zone == ZoneId.Teldrassil
                            || zone == ZoneId.TheExodar;
            }
            else if (WowInterface.ObjectManager.Player.IsHorde())
            {
                return zone == ZoneId.Orgrimmar
                            || zone == ZoneId.Undercity
                            || zone == ZoneId.ThunderBluff
                            || zone == ZoneId.SilvermoonCity;
            }
            else
            {
                return false;
            }
        }

        internal bool IsDungeonMap(MapId map)
        {
            return map == MapId.Deadmines
                       || map == MapId.HellfireRamparts
                       || map == MapId.TheBloodFurnace
                       || map == MapId.TheSlavePens
                       || map == MapId.TheUnderbog
                       || map == MapId.TheSteamvault
                       || map == MapId.UtgardeKeep
                       || map == MapId.AzjolNerub;
        }
    }
}