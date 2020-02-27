using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.OffsetLists;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateBattleground : BasicState
    {
        public StateBattleground(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, IOffsetList offsetList, ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IMovementEngine movementEngine, BattlegroundEngine battlegroundEngine) : base(stateMachine)
        {
            Config = config;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            HookManager = hookManager;
            CharacterManager = characterManager;
            MovementEngine = movementEngine;
            BattlegroundEngine = battlegroundEngine;
        }

        private BattlegroundEngine BattlegroundEngine { get; }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private HookManager HookManager { get; }

        private IMovementEngine MovementEngine { get; }

        private ObjectManager ObjectManager { get; }

        private IOffsetList OffsetList { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (AmeisenBotStateMachine.XMemory.Read(OffsetList.BattlegroundStatus, out int bgStatus)
                && bgStatus == 0)
            {
                AmeisenBotStateMachine.SetState(BotState.Idle);
                return;
            }

            if (AmeisenBotStateMachine.XMemory.Read(OffsetList.BattlegroundFinished, out int bgFinished)
                && bgFinished == 1)
            {
                HookManager.LeaveBattleground();
                return;
            }

            BattlegroundEngine.Execute();
        }

        public override void Exit()
        {
            MovementEngine.Reset();
        }
    }
}