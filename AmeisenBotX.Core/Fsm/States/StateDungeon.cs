﻿using AmeisenBotX.Core.Fsm.Enums;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateDungeon : BasicState
    {
        public StateDungeon(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        public override void Enter()
        {
            Bot.Movement.Reset();
            Bot.Dungeon.Enter();
            StateMachine.OnStateOverride += StateMachine_OnStateOverride;
        }

        public override void Execute()
        {
            if (!Bot.Objects.MapId.IsDungeonMap())
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            Bot.Dungeon.Execute();
            Bot.CombatClass?.OutOfCombatExecute();
        }

        public override void Leave()
        {
            StateMachine.OnStateOverride -= StateMachine_OnStateOverride;
            Bot.Movement.Reset();
            Bot.Dungeon.Exit();
        }

        private void StateMachine_OnStateOverride(BotState botState)
        {
            if (botState == BotState.Dead)
            {
                Bot.Dungeon.OnDeath();
            }
        }
    }
}