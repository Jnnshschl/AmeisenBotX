using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
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
        private AmeisenBotConfig Config { get; }

        private DateTime LastObjectUpdate { get; set; }

        private Dictionary<AmeisenBotState, State> States { get; }

        public delegate void StateMachineTick();
        public event StateMachineTick OnStateMachineTick;

        public delegate void StateMachineStateChange();
        public event StateMachineStateChange OnStateMachineStateChange;

        public AmeisenBotStateMachine(Process wowProcess, AmeisenBotConfig config, XMemory xMemory, IOffsetList offsetList, ObjectManager objectManager, CharacterManager characterManager)
        {
            XMemory = xMemory;
            ObjectManager = objectManager;
            Config = config;

            LastObjectUpdate = DateTime.Now;

            States = new Dictionary<AmeisenBotState, State>()
            {
                { AmeisenBotState.None, new StateNone(this, config)},
                { AmeisenBotState.StartWow, new StateStartWow(this, config, wowProcess, xMemory)},
                { AmeisenBotState.Login, new StateLogin(this, config, offsetList)},
                { AmeisenBotState.LoadingScreen, new StateLoadingScreen(this, config, objectManager)},
                { AmeisenBotState.Idle, new StateIdle(this, config, objectManager)},
                { AmeisenBotState.Following, new StateFollowing(this, config, objectManager, characterManager)}
            };

            CurrentState = States.First();
            CurrentState.Value.Enter();
        }

        public void Execute()
        {
            if (!ObjectManager.IsWorldLoaded)
                SetState(AmeisenBotState.LoadingScreen);

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
    }
}
