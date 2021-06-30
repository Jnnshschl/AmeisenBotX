﻿using AmeisenBotX.Core.Fsm.Enums;

namespace AmeisenBotX.Core.Fsm.States
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

            if (Bot.Memory.Read(Bot.Offsets.BattlegroundStatus, out int bgStatus)
                && bgStatus == 0)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (Bot.Memory.Read(Bot.Offsets.BattlegroundFinished, out int bgFinished)
                && bgFinished == 1)
            {
                Bot.Wow.LuaLeaveBattleground();
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