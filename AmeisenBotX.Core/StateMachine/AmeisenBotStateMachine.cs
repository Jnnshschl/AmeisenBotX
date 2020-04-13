using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine
{
    public class AmeisenBotStateMachine
    {
        public AmeisenBotStateMachine(string botDataPath, AmeisenBotConfig config, WowInterface wowInterface)
        {
            AmeisenLogger.Instance.Log("StateMachine", "Starting AmeisenBotStateMachine...", LogLevel.Verbose);

            BotDataPath = botDataPath;
            Config = config;
            WowInterface = wowInterface;

            LastGhostCheck = DateTime.Now;
            LastEventPull = DateTime.Now;

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
        }

        public delegate void StateMachineStateChange();

        public delegate void StateMachineTick();

        public event StateMachineStateChange OnStateMachineStateChanged;

        public event StateMachineTick OnStateMachineTick;

        public string BotDataPath { get; }

        public KeyValuePair<BotState, BasicState> CurrentState { get; private set; }

        public BotState LastState { get; private set; }

        public MapId MapIDiedOn { get; internal set; }

        public string PlayerName { get; internal set; }

        public bool WowCrashed { get; internal set; }

        public Dictionary<BotState, BasicState> States { get; private set; }

        internal WowInterface WowInterface { get; }

        private AmeisenBotConfig Config { get; }

        private DateTime LastEventPull { get; set; }

        private DateTime LastGhostCheck { get; set; }

        public void Execute()
        {
            if (WowInterface.XMemory.Process == null || WowInterface.XMemory.Process.HasExited)
            {
                AmeisenLogger.Instance.Log("StateMachine", "WoW crashed...", LogLevel.Verbose);
                WowCrashed = true;

                WowInterface.MovementEngine.Reset();
                WowInterface.ObjectManager.WowObjects.Clear();
                WowInterface.EventHookManager.Stop();

                SetState(BotState.None);
            }

            if (WowInterface.ObjectManager != null && CurrentState.Key != BotState.LoadingScreen)
            {
                if (!WowInterface.ObjectManager.IsWorldLoaded)
                {
                    AmeisenLogger.Instance.Log("StateMachine", "World is not loaded...", LogLevel.Verbose);
                    SetState(BotState.LoadingScreen, true);
                    WowInterface.MovementEngine.Reset();
                }
                else
                {
                    HandleObjectUpdates();
                    HandleEventPull();

                    if (WowInterface.ObjectManager.Player != null)
                    {
                        HandlePlayerDeadOrGhostState();

                        if (CurrentState.Key != BotState.Dead && CurrentState.Key != BotState.Ghost)
                        {
                            if (Config.AutoDodgeAoeSpells
                                && BotUtils.IsPositionInsideAoeSpell(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.WowObjects.OfType<WowDynobject>().ToList()))
                            {
                                SetState(BotState.InsideAoeDamage, true);
                            }

                            if (WowInterface.ObjectManager.Player.IsInCombat || IsAnyPartymemberInCombat())
                            {
                                SetState(BotState.Attacking, true);
                            }
                        }
                    }
                }
            }

            // execute the State
            CurrentState.Value.Execute();

            // anti AFK
            WowInterface.CharacterManager.AntiAfk();

            // used for ui updates
            OnStateMachineTick?.Invoke();
        }

        internal IEnumerable<WowUnit> GetNearLootableUnits()
            => WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
            .Where(e => e.IsLootable
                && !((StateLooting)States[BotState.Looting]).UnitsAlreadyLootedList.Contains(e.Guid)
                && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.LootUnitsRadius);

        internal bool HasFoodInBag()
            => WowInterface.CharacterManager.Inventory.Items.Select(e => e.Id).Any(e => Enum.IsDefined(typeof(WowFood), e));

        internal bool HasRefreshmentInBag()
            => WowInterface.CharacterManager.Inventory.Items.Select(e => e.Id).Any(e => Enum.IsDefined(typeof(WowRefreshment), e));

        internal bool HasWaterInBag()
            => WowInterface.CharacterManager.Inventory.Items.Select(e => e.Id).Any(e => Enum.IsDefined(typeof(WowWater), e));

        internal bool IsAnyPartymemberInCombat()
                                    => WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>()
            .Where(e => WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid))
            .Any(r => r.IsInCombat);

        internal bool IsBattlegroundMap(MapId map)
            => map == MapId.AlteracValley
            || map == MapId.WarsongGulch
            || map == MapId.ArathiBasin
            || map == MapId.EyeOfTheStorm
            || map == MapId.StrandOfTheAncients;

        internal bool IsDungeonMap(MapId map)
            => map == MapId.Deadmines
            || map == MapId.UtgardeKeep;

        internal bool IsInCapitalCity()
        {
            return false;
        }

        internal void SetState(BotState state, bool ignoreExit = false)
        {
            if (CurrentState.Key == state)
            {
                // we are already in this state
                return;
            }

            LastState = CurrentState.Key;

            // this is used by the combat state because
            // it will override any existing state
            if (!ignoreExit)
            {
                CurrentState.Value.Exit();
            }

            CurrentState = States.First(s => s.Key == state);
            CurrentState.Value.Enter();

            OnStateMachineStateChanged?.Invoke();
        }

        private void HandleEventPull()
        {
            if (WowInterface.EventHookManager.IsSetUp
                && LastEventPull + TimeSpan.FromMilliseconds(Config.EventPullMs) < DateTime.Now)
            {
                WowInterface.EventHookManager.ReadEvents();
                LastEventPull = DateTime.Now;
            }
        }

        private void HandleObjectUpdates()
        {
            if (CurrentState.Key != BotState.None
                && CurrentState.Key != BotState.StartWow
                && CurrentState.Key != BotState.Login
                && CurrentState.Key != BotState.LoadingScreen)
            {
                WowInterface.ObjectManager.UpdateWowObjects();
            }
        }

        private void HandlePlayerDeadOrGhostState()
        {
            if (WowInterface.ObjectManager.Player.IsDead)
            {
                SetState(BotState.Dead);
            }
            else
            {
                if (LastGhostCheck + TimeSpan.FromSeconds(8) < DateTime.Now)
                {
                    bool isGhost = WowInterface.ObjectManager.Player.Health == 1 && WowInterface.HookManager.IsGhost("player");
                    LastGhostCheck = DateTime.Now;

                    if (isGhost)
                    {
                        SetState(BotState.Ghost);
                    }
                }
            }
        }
    }
}