using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Core.StateMachine.States;
using AmeisenBotX.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine
{
    public class AmeisenBotStateMachine
    {
        public KeyValuePair<AmeisenBotState, State> CurrentState { get; private set; }

        internal XMemory XMemory { get; }
        private ObjectManager ObjectManager { get; }
        private CharacterManager CharacterManager { get; }
        private HookManager HookManager { get; }
        private CacheManager CacheManager { get; }
        private AmeisenBotConfig Config { get; }

        private DateTime LastObjectUpdate { get; set; }

        private Dictionary<AmeisenBotState, State> States { get; }

        public delegate void StateMachineTick();
        public event StateMachineTick OnStateMachineTick;

        public delegate void StateMachineStateChange();
        public event StateMachineStateChange OnStateMachineStateChange;

        public AmeisenBotStateMachine(Process wowProcess, AmeisenBotConfig config, XMemory xMemory, IOffsetList offsetList, ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, CacheManager cacheManager)
        {
            Config = config;
            XMemory = xMemory;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            CacheManager = cacheManager;

            LastObjectUpdate = DateTime.Now;

            States = new Dictionary<AmeisenBotState, State>()
            {
                { AmeisenBotState.None, new StateNone(this, config)},
                { AmeisenBotState.StartWow, new StateStartWow(this, config, wowProcess, xMemory)},
                { AmeisenBotState.Login, new StateLogin(this, config, offsetList, characterManager)},
                { AmeisenBotState.LoadingScreen, new StateLoadingScreen(this, config, objectManager)},
                { AmeisenBotState.Idle, new StateIdle(this, config, objectManager, hookManager)},
                { AmeisenBotState.Following, new StateFollowing(this, config, objectManager, characterManager)},
                { AmeisenBotState.Attacking, new StateAttacking(this, config, objectManager, characterManager, hookManager)},
                { AmeisenBotState.Healing, new StateHealing(this, config, objectManager, characterManager)}
            };

            CurrentState = States.First();
            CurrentState.Value.Enter();
        }

        public void Execute()
        {
            if (ObjectManager != null)
            {
                if (!ObjectManager.IsWorldLoaded)
                    SetState(AmeisenBotState.LoadingScreen);

                if (ObjectManager.Player != null)
                {
                    if (ObjectManager.Player.IsInCombat)
                        SetState(HandleCombatSituation());
                }
            }

            if (LastObjectUpdate - TimeSpan.FromMilliseconds(Config.ObjectUpdateMs) < DateTime.Now
                && CurrentState.Key != AmeisenBotState.None
                && CurrentState.Key != AmeisenBotState.StartWow
                && CurrentState.Key != AmeisenBotState.Login
                && CurrentState.Key != AmeisenBotState.LoadingScreen)
            {
                ObjectManager.UpdateWowObjects();
                LastObjectUpdate = DateTime.Now;
            }

            CurrentState.Value.Execute();
            CharacterManager.AntiAfk();
            OnStateMachineTick?.Invoke();
        }

        private AmeisenBotState HandleCombatSituation()
        {
            return AmeisenBotState.Attacking;
        }

        internal void SetState(AmeisenBotState state)
        {
            if (CurrentState.Key == state) return; // we are already in this state

            CurrentState.Value.Exit();
            CurrentState = States.First(s => s.Key == state);
            CurrentState.Value.Enter();

            OnStateMachineStateChange?.Invoke();
        }
    }
}
