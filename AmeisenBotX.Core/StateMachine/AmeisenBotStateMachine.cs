using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Data.Persistence;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
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
            ICombatClass combatClass)
        {
            AmeisenLogger.Instance.Log("Starting AmeisenBotStateMachine...", LogLevel.Verbose);

            BotDataPath = botDataPath;
            Config = config;
            XMemory = xMemory;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            EventHookManager = eventHookManager;
            BotCache = botCache;

            LastObjectUpdate = DateTime.Now;
            LastGhostCheck = DateTime.Now;
            LastEventPull = DateTime.Now;

            LastState = AmeisenBotState.None;
            UnitLootList = new Queue<ulong>();

            States = new Dictionary<AmeisenBotState, State>()
            {
                { AmeisenBotState.None, new StateNone(this, config) },
                { AmeisenBotState.StartWow, new StateStartWow(this, config, wowProcess, xMemory) },
                { AmeisenBotState.Login, new StateLogin(this, config, offsetList, characterManager) },
                { AmeisenBotState.LoadingScreen, new StateLoadingScreen(this, xMemory, config, objectManager) },
                { AmeisenBotState.Idle, new StateIdle(this, config, offsetList, objectManager, characterManager, hookManager, eventHookManager, combatClass, UnitLootList) },
                { AmeisenBotState.Dead, new StateDead(this, config, objectManager, hookManager) },
                { AmeisenBotState.Ghost, new StateGhost(this, config, offsetList, objectManager, characterManager, hookManager, pathfindingHandler, movementEngine) },
                { AmeisenBotState.Following, new StateFollowing(this, config, objectManager, characterManager, pathfindingHandler, movementEngine) },
                { AmeisenBotState.Attacking, new StateAttacking(this, config, objectManager, characterManager, hookManager, pathfindingHandler, movementEngine, movementSettings, combatClass) },
                { AmeisenBotState.Repairing, new StateRepairing(this, config, objectManager, hookManager, characterManager, pathfindingHandler, movementEngine) },
                { AmeisenBotState.Selling, new StateSelling(this, config, objectManager, hookManager, characterManager, pathfindingHandler, movementEngine) },
                { AmeisenBotState.Healing, new StateEating(this, config, objectManager, characterManager) },
                { AmeisenBotState.InsideAoeDamage, new StateInsideAoeDamage(this, config, objectManager, characterManager, pathfindingHandler, movementEngine) },
                { AmeisenBotState.Looting, new StateLooting(this, config, offsetList, objectManager, characterManager, hookManager, pathfindingHandler, movementEngine, UnitLootList) }
            };

            CurrentState = States.First();
            CurrentState.Value.Enter();
        }

        public delegate void StateMachineStateChange();

        public delegate void StateMachineTick();

        public event StateMachineStateChange OnStateMachineStateChanged;

        public event StateMachineTick OnStateMachineTick;

        public string BotDataPath { get; }

        public KeyValuePair<AmeisenBotState, State> CurrentState { get; private set; }

        public AmeisenBotState LastState { get; private set; }

        public string PlayerName { get; internal set; }

        internal IOffsetList OffsetList { get; }

        internal XMemory XMemory { get; }

        internal Queue<ulong> UnitLootList { get; set; }

        private IAmeisenBotCache BotCache { get; }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private EventHookManager EventHookManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastEventPull { get; set; }

        private DateTime LastGhostCheck { get; set; }

        private DateTime LastObjectUpdate { get; set; }

        private ObjectManager ObjectManager { get; }

        private Dictionary<AmeisenBotState, State> States { get; }

        public void Execute()
        {
            if (XMemory.Process == null || XMemory.Process.HasExited)
            {
                AmeisenLogger.Instance.Log("WoW crashed...", LogLevel.Verbose);
                SetState(AmeisenBotState.None);
            }

            if (ObjectManager != null)
            {
                if (!ObjectManager.IsWorldLoaded)
                {
                    SetState(AmeisenBotState.LoadingScreen);
                }

                HandleEventPull();

                if (ObjectManager.Player != null)
                {
                    HandlePlayerDeadOrGhostState();

                    if (CurrentState.Key != AmeisenBotState.Dead && CurrentState.Key != AmeisenBotState.Ghost)
                    {
                        if (Config.AutoDodgeAoeSpells
                            && BotUtils.IsPositionInsideAoeSpell(ObjectManager.Player.Position, ObjectManager.WowObjects.OfType<WowDynobject>().ToList()))
                        {
                            SetState(AmeisenBotState.InsideAoeDamage);
                        }

                        if (ObjectManager.Player.IsInCombat || IsAnyPartymemberInCombat())
                        {
                            SetState(AmeisenBotState.Attacking);
                        }
                    }
                }
            }

            HandleObjectUpdates();
            CharacterManager.AntiAfk();

            // used for ui updates
            OnStateMachineTick?.Invoke();
            CurrentState.Value.Execute();
        }

        internal bool IsAnyPartymemberInCombat()
            => ObjectManager.WowObjects.OfType<WowPlayer>().Where(e => ObjectManager.PartymemberGuids.Contains(e.Guid)).Any(r => r.Position.GetDistance(ObjectManager.Player.Position) < 60 && r.IsInCombat);

        internal void SetState(AmeisenBotState state)
        {
            if (CurrentState.Key == state)
            {
                // we are already in this state
                return;
            }

            LastState = CurrentState.Key;
            CurrentState.Value.Exit();
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
                && CurrentState.Key != AmeisenBotState.None
                && CurrentState.Key != AmeisenBotState.StartWow
                && CurrentState.Key != AmeisenBotState.Login
                && CurrentState.Key != AmeisenBotState.LoadingScreen)
            {
                ObjectManager.UpdateWowObjects();
                LastObjectUpdate = DateTime.Now;
            }
        }

        private void HandlePlayerDeadOrGhostState()
        {
            if (ObjectManager.Player.IsDead)
            {
                SetState(AmeisenBotState.Dead);
            }
            else
            {
                if (LastGhostCheck + TimeSpan.FromSeconds(8) < DateTime.Now)
                {
                    bool isGhost = HookManager.IsGhost("player");
                    LastGhostCheck = DateTime.Now;

                    if (isGhost)
                    {
                        SetState(AmeisenBotState.Ghost);
                    }
                }
            }
        }
    }
}
