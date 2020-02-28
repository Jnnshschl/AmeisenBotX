using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Data.Persistence;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Jobs;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Core.StateMachine.States;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine
{
    public class AmeisenBotStateMachine
    {
        public AmeisenBotStateMachine(
            string botDataPath,
            Process wowProcess,
            AmeisenBotConfig config,
            XMemory xMemory,
            IOffsetList offsetList,
            ObjectManager objectManager,
            CharacterManager characterManager,
            HookManager hookManager,
            EventHookManager eventHookManager,
            IAmeisenBotCache botCache,
            IPathfindingHandler pathfindingHandler,
            IMovementEngine movementEngine,
            MovementSettings movementSettings,
            ICombatClass combatClass,
            BattlegroundEngine battlegroundEngine,
            JobEngine jobEngine)
        {
            AmeisenLogger.Instance.Log("StateMachine", "Starting AmeisenBotStateMachine...", LogLevel.Verbose);

            BotDataPath = botDataPath;
            Config = config;
            XMemory = xMemory;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            EventHookManager = eventHookManager;
            BotCache = botCache;
            MovementEngine = movementEngine;

            LastObjectUpdate = DateTime.Now;
            LastGhostCheck = DateTime.Now;
            LastEventPull = DateTime.Now;

            LastState = BotState.None;
            UnitLootList = new Queue<ulong>();

            States = new Dictionary<BotState, BasicState>()
            {
                { BotState.None, new StateNone(this, config) },
                { BotState.StartWow, new StateStartWow(this, config, wowProcess, xMemory) },
                { BotState.Login, new StateLogin(this, config, offsetList, characterManager) },
                { BotState.LoadingScreen, new StateLoadingScreen(this, xMemory, config, objectManager) },
                { BotState.Idle, new StateIdle(this, config, offsetList, objectManager, characterManager, hookManager, eventHookManager, combatClass, UnitLootList) },
                { BotState.Dead, new StateDead(this, config, objectManager, hookManager) },
                { BotState.Ghost, new StateGhost(this, config, offsetList, objectManager, characterManager, hookManager, pathfindingHandler, movementEngine) },
                { BotState.Following, new StateFollowing(this, config, objectManager, characterManager, pathfindingHandler, movementEngine) },
                { BotState.Attacking, new StateAttacking(this, config, objectManager, characterManager, hookManager, pathfindingHandler, movementEngine, movementSettings, combatClass) },
                { BotState.Repairing, new StateRepairing(this, config, objectManager, hookManager, characterManager, movementEngine) },
                { BotState.Selling, new StateSelling(this, config, objectManager, hookManager, characterManager, movementEngine) },
                { BotState.Healing, new StateEating(this, config, objectManager, characterManager) },
                { BotState.InsideAoeDamage, new StateInsideAoeDamage(this, config, objectManager, characterManager, pathfindingHandler, movementEngine) },
                { BotState.Looting, new StateLooting(this, config, offsetList, objectManager, characterManager, hookManager, pathfindingHandler, movementEngine, UnitLootList) },
                { BotState.Battleground, new StateBattleground(this, config, offsetList, objectManager, characterManager, hookManager, movementEngine, battlegroundEngine) },
                { BotState.Job, new StateJob(this, jobEngine) }
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

        public string PlayerName { get; internal set; }

        internal BattlegroundEngine BattlegroundEngine { get; }

        internal IOffsetList OffsetList { get; }

        internal Queue<ulong> UnitLootList { get; set; }

        internal XMemory XMemory { get; }

        private IAmeisenBotCache BotCache { get; }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private EventHookManager EventHookManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastEventPull { get; set; }

        private DateTime LastGhostCheck { get; set; }

        private DateTime LastObjectUpdate { get; set; }

        private IMovementEngine MovementEngine { get; set; }

        private ObjectManager ObjectManager { get; }

        private Dictionary<BotState, BasicState> States { get; }

        public void Execute()
        {
            if (XMemory.Process == null || XMemory.Process.HasExited)
            {
                AmeisenLogger.Instance.Log("StateMachine", "WoW crashed...", LogLevel.Verbose);
                SetState(BotState.None);
            }

            if (ObjectManager != null)
            {
                if (!ObjectManager.IsWorldLoaded)
                {
                    SetState(BotState.LoadingScreen);
                    MovementEngine.Reset();
                    return;
                }
                else
                {
                    HandleObjectUpdates();
                    HandleEventPull();

                    if (ObjectManager.Player != null)
                    {
                        HandlePlayerDeadOrGhostState();

                        if (CurrentState.Key != BotState.Dead && CurrentState.Key != BotState.Ghost)
                        {
                            if (Config.AutoDodgeAoeSpells
                                && BotUtils.IsPositionInsideAoeSpell(ObjectManager.Player.Position, ObjectManager.WowObjects.OfType<WowDynobject>().ToList()))
                            {
                                SetState(BotState.InsideAoeDamage);
                            }

                            if (ObjectManager.Player.IsInCombat || IsAnyPartymemberInCombat())
                            {
                                SetState(BotState.Attacking, true);
                            }
                        }
                    }
                }
            }

            CharacterManager.AntiAfk();

            // used for ui updates
            OnStateMachineTick?.Invoke();
            CurrentState.Value.Execute();
        }

        internal bool IsAnyPartymemberInCombat()
            => ObjectManager.WowObjects.OfType<WowPlayer>()
            .Where(e => ObjectManager.PartymemberGuids.Contains(e.Guid))
            .Any(r => r.Position.GetDistance(ObjectManager.Player.Position) < 60 && r.IsInCombat);

        internal bool IsOnBattleground()
            => ObjectManager.MapId == 30
            || ObjectManager.MapId == 489
            || ObjectManager.MapId == 529
            || ObjectManager.MapId == 566
            || ObjectManager.MapId == 607;

        internal void SetState(BotState state, bool ignoreExit = false)
        {
            if (CurrentState.Key == state)
            {
                // we are already in this state
                return;
            }

            LastState = CurrentState.Key;

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
            if (EventHookManager.IsSetUp
                && LastEventPull + TimeSpan.FromSeconds(1) < DateTime.Now)
            {
                EventHookManager.ReadEvents();
                LastEventPull = DateTime.Now;
            }
        }

        private void HandleObjectUpdates()
        {
            if (LastObjectUpdate - TimeSpan.FromMilliseconds(Config.ObjectUpdateMs) < DateTime.Now
                && CurrentState.Key != BotState.None
                && CurrentState.Key != BotState.StartWow
                && CurrentState.Key != BotState.Login
                && CurrentState.Key != BotState.LoadingScreen)
            {
                ObjectManager.UpdateWowObjects();
                LastObjectUpdate = DateTime.Now;
            }
        }

        private void HandlePlayerDeadOrGhostState()
        {
            if (ObjectManager.Player.IsDead)
            {
                SetState(BotState.Dead);
            }
            else
            {
                if (LastGhostCheck + TimeSpan.FromSeconds(8) < DateTime.Now)
                {
                    bool isGhost = HookManager.IsGhost("player");
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