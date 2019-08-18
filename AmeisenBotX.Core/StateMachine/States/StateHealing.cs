using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateHealing : State
    {
        private AmeisenBotConfig Config { get; }

        private ObjectManager ObjectManager { get; }
        private CharacterManager CharacterManager { get; }

        public StateHealing(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, CharacterManager characterManager) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
        }
    }
}