using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateDead : BasicState
    {
        public StateDead(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, HookManager hookManager) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            HookManager = hookManager;
        }

        private AmeisenBotConfig Config { get; }

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (ObjectManager.Player.IsDead)
            {
                HookManager.ReleaseSpirit();
            }
            else if (HookManager.IsGhost("player"))
            {
                AmeisenBotStateMachine.SetState(BotState.Ghost);
            }
            else
            {
                AmeisenBotStateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}