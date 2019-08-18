using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateDead : State
    {
        private AmeisenBotConfig Config { get; }
        private ObjectManager ObjectManager { get; }
        private HookManager HookManager { get; }

        public StateDead(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, HookManager hookManager) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            HookManager = hookManager;
        }

        public override void Enter()
        {

        }

        public override void Execute()
        {
            if (ObjectManager.Player.IsDead)
                HookManager.ReleaseSpirit();
            else if (HookManager.IsGhost("player"))
                AmeisenBotStateMachine.SetState(AmeisenBotState.Ghost);
            else
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
        }

        public override void Exit()
        {

        }
    }
}
