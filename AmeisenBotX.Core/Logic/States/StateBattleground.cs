using AmeisenBotX.Core.Logic.Enums;

namespace AmeisenBotX.Core.Logic.States
{
    public class StateBattleground : BasicState
    {
        public StateBattleground(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        public override void Enter()
        {
            Bot.Battleground?.Enter();
        }

        public override void Execute()
        {
            if (Bot.Battleground == null)
            {
                return;
            }

            if (Bot.Memory.Read(Bot.Wow.Offsets.BattlegroundStatus, out int bgStatus)
                && bgStatus == 0)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (Bot.Memory.Read(Bot.Wow.Offsets.BattlegroundFinished, out int bgFinished)
                && bgFinished == 1)
            {
                Bot.Wow.LeaveBattleground();
                return;
            }

            Bot.Battleground.Execute();
        }

        public override void Leave()
        {
            Bot.Battleground?.Leave();
            Bot.Movement.Reset();
        }
    }
}