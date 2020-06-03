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
    public class AmeisenBotStateMachine : AbstractStateMachine
    {
        public AmeisenBotStateMachine(string botDataPath, AmeisenBotConfig config, WowInterface wowInterface)
        {
            AmeisenLogger.Instance.Log("StateMachine", "Starting AmeisenBotStateMachine...", LogLevel.Verbose);

            BotDataPath = botDataPath;
            Config = config;
            WowInterface = wowInterface;

            LastState = BotState.None;

            States = new Dictionary<BotState, BasicState>()
            {
                { BotState.None, new StateNone(this, config, WowInterface) },
                { BotState.Attacking, new StateAttacking(this, config, WowInterface) },
                { BotState.Battleground, new StateBattleground(this, config, WowInterface) },
                { BotState.Dead, new StateDead(this, config, WowInterface) },
                { BotState.Dungeon, new StateDungeon(this, config, WowInterface) },
                { BotState.Eating, new StateEating(this, config, WowInterface) },
                { BotState.Following, new StateFollowing(this, config, WowInterface) },
                { BotState.Ghost, new StateGhost(this, config, WowInterface) },
                { BotState.Idle, new StateIdle(this, config, WowInterface) },
                { BotState.InsideAoeDamage, new StateInsideAoeDamage(this, config, WowInterface) },
                { BotState.Job, new StateJob(this, config, WowInterface) },
                { BotState.LoadingScreen, new StateLoadingScreen(this, config, WowInterface) },
                { BotState.Login, new StateLogin(this, config, WowInterface) },
                { BotState.Looting, new StateLooting(this, config, WowInterface) },
                { BotState.Repairing, new StateRepairing(this, config, WowInterface) },
                { BotState.Selling, new StateSelling(this, config, WowInterface) },
                { BotState.StartWow, new StateStartWow(this, config, WowInterface) }
            };

            CurrentState = States.First();
            CurrentState.Value.Enter();

            AntiAfkEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(Config.AntiAfkMs), WowInterface.CharacterManager.AntiAfk);
            EventPullEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(Config.EventPullMs), WowInterface.EventHookManager.Pull);
            GhostCheckEvent = new TimegatedEvent<bool>(TimeSpan.FromMilliseconds(Config.GhostCheckMs), () => WowInterface.ObjectManager.Player.Health == 1 && WowInterface.HookManager.IsGhost(WowLuaUnit.Player));
            ObjectUpdateEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(Config.ObjectUpdateMs), WowInterface.ObjectManager.UpdateWowObjects);
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

        private TimegatedEvent ObjectUpdateEvent { get; set; }

        public override void Execute()
        {
            // we cant do anything if wow has crashed
            if ((WowInterface.XMemory.Process == null || WowInterface.XMemory.Process.HasExited)
                && SetState(BotState.None))
            {
                AmeisenLogger.Instance.Log("StateMachine", "WoW crashed...", LogLevel.Verbose);

                WowCrashed = true;
                ((StateIdle)States[BotState.Idle]).FirstStart = true;

                WowInterface.MovementEngine.Reset();
                WowInterface.ObjectManager.WowObjects.Clear();
                WowInterface.EventHookManager.Stop();

                return;
            }

            // ingame override states
            if (CurrentState.Key != BotState.None
                && CurrentState.Key != BotState.StartWow
                && CurrentState.Key != BotState.Login
                && WowInterface.ObjectManager != null)
            {
                WowInterface.ObjectManager.RefreshIsWorldLoaded();

                if (!WowInterface.ObjectManager.IsWorldLoaded)
                {
                    if (SetState(BotState.LoadingScreen, true))
                    {
                        OnStateOverride?.Invoke(CurrentState.Key);
                        AmeisenLogger.Instance.Log("StateMachine", "World is not loaded...", LogLevel.Verbose);
                        return;
                    }
                }
                else
                {
                    ObjectUpdateEvent.Run();
                    EventPullEvent.Run();

                    if (WowInterface.ObjectManager.Player != null)
                    {
                        if (WowInterface.ObjectManager.Player.IsDead
                            && SetState(BotState.Dead, true))
                        {
                            // we are dead, state needs to release the spirit
                            OnStateOverride?.Invoke(CurrentState.Key);
                            return;
                        }
                        else if (GhostCheckEvent.Run(out bool isGhost)
                            && isGhost
                            && SetState(BotState.Ghost, true))
                        {
                            // we cant be a ghost if we are still dead
                            OnStateOverride?.Invoke(CurrentState.Key);
                            return;
                        }

                        // we cant fight nor do we receive damage when we are dead or a ghost
                        // so ignore these overrides
                        if (CurrentState.Key != BotState.Dead
                            && CurrentState.Key != BotState.Ghost)
                        {
                            // if (Config.AutoDodgeAoeSpells
                            //     && BotUtils.IsPositionInsideAoeSpell(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.GetNearAoeSpells())
                            //     && SetState(BotState.InsideAoeDamage, true))
                            // {
                            //     OnStateOverride(CurrentState.Key);
                            //     return;
                            // }

                            // TODO: handle combat bug, sometimes when combat ends, the player stays in combot for no reason
                            if ((WowInterface.ObjectManager.Player.IsInCombat || IsAnyPartymemberInCombat()) && SetState(BotState.Attacking, true))
                            {
                                OnStateOverride?.Invoke(CurrentState.Key);
                                return;
                            }
                        }
                    }
                }
            }

            // execute the State
            CurrentState.Value.Execute();
            OnStateMachineTick?.Invoke();
            AntiAfkEvent.Run();
        }

        internal IEnumerable<WowUnit> GetNearLootableUnits()
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .Where(e => e.IsLootable
                                && !((StateLooting)States[BotState.Looting]).UnitsAlreadyLootedList.Contains(e.Guid)
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
                       || map == MapId.UtgardeKeep
                       || map == MapId.AzjolNerub;
        }
    }
}