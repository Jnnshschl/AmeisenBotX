using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Core.StateMachine.States;
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
            Process wowProcess,
            AmeisenBotConfig config,
            XMemory xMemory,
            IOffsetList offsetList,
            ObjectManager objectManager,
            CharacterManager characterManager,
            HookManager hookManager,
            EventHookManager eventHookManager,
            CacheManager cacheManager,
            IPathfindingHandler pathfindingHandler,
            ICombatClass combatClass)
        {
            Config = config;
            XMemory = xMemory;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            EventHookManager = eventHookManager;
            CacheManager = cacheManager;

            LastObjectUpdate = DateTime.Now;
            LastGhostCheck = DateTime.Now;
            LastEventPull = DateTime.Now;

            States = new Dictionary<AmeisenBotState, State>()
            {
                { AmeisenBotState.None, new StateNone(this, config)},
                { AmeisenBotState.StartWow, new StateStartWow(this, config, wowProcess, xMemory)},
                { AmeisenBotState.Login, new StateLogin(this, config, offsetList, characterManager)},
                { AmeisenBotState.LoadingScreen, new StateLoadingScreen(this, config, objectManager)},
                { AmeisenBotState.Idle, new StateIdle(this, config, offsetList, objectManager, hookManager, eventHookManager)},
                { AmeisenBotState.Dead, new StateDead(this, config, objectManager, hookManager)},
                { AmeisenBotState.Ghost, new StateGhost(this, config, offsetList, objectManager, characterManager, hookManager, pathfindingHandler)},
                { AmeisenBotState.Following, new StateFollowing(this, config, objectManager, characterManager, pathfindingHandler)},
                { AmeisenBotState.Attacking, new StateAttacking(this, config, objectManager, characterManager, hookManager, pathfindingHandler, combatClass)},
                { AmeisenBotState.Healing, new StateHealing(this, config, objectManager, characterManager)}
            };

            CurrentState = States.First();
            CurrentState.Value.Enter();
        }

        public delegate void StateMachineStateChange();

        public delegate void StateMachineTick();

        public event StateMachineStateChange OnStateMachineStateChange;

        public event StateMachineTick OnStateMachineTick;

        public KeyValuePair<AmeisenBotState, State> CurrentState { get; private set; }

        public string PlayerName { get; internal set; }
        internal XMemory XMemory { get; }
        private CacheManager CacheManager { get; }
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
            if (XMemory.Process != null && XMemory.Process.HasExited)
                SetState(AmeisenBotState.None);

            HandleEventPull();

            if (ObjectManager != null)
            {
                if (!ObjectManager.IsWorldLoaded)
                    SetState(AmeisenBotState.LoadingScreen);

                if (ObjectManager.Player != null)
                {
                    HandlePlayerDeadOrGhostState();

                    if (ObjectManager.Player.IsInCombat)
                        SetState(HandleCombatSituation());
                }
            }

            HandleObjectUpdates();

            CurrentState.Value.Execute();
            CharacterManager.AntiAfk();

            // used for ui updates
            OnStateMachineTick?.Invoke();
        }

        internal void SetState(AmeisenBotState state)
        {
            if (CurrentState.Key == state) return; // we are already in this state

            CurrentState.Value.Exit();
            CurrentState = States.First(s => s.Key == state);
            CurrentState.Value.Enter();

            OnStateMachineStateChange?.Invoke();
        }

        private AmeisenBotState HandleCombatSituation()
        {
            return AmeisenBotState.Attacking;
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
                SetState(AmeisenBotState.Dead);
            else
            {
                if (LastGhostCheck + TimeSpan.FromSeconds(3) < DateTime.Now)
                {
                    bool isGhost = HookManager.IsGhost("player");
                    LastGhostCheck = DateTime.Now;

                    if (isGhost) SetState(AmeisenBotState.Ghost);
                }
            }
        }
    }
}