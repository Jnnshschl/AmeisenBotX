using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Fsm.Enums;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateDead : BasicState
    {
        public StateDead(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        public bool SetMapAndPosition { get; set; }

        public override void Enter()
        {
            Bot.Movement.StopMovement();
        }

        public override void Execute()
        {
            if (Bot.Player.IsDead)
            {
                if (!SetMapAndPosition) // prevent re-setting the stuff in loading screen
                {
                    SetMapAndPosition = true;
                    StateMachine.LastDiedMap = Bot.Objects.MapId;

                    if (StateMachine.LastDiedMap.IsDungeonMap())
                    {
                        // when we died in a dungeon, we need to return to its portal
                        StateMachine.LastDiedPosition = Bot.Dungeon.Profile.WorldEntry;
                    }
                    else
                    {
                        StateMachine.LastDiedPosition = Bot.Player.Position;
                    }
                }

                if (Config.ReleaseSpirit || Bot.Objects.MapId.IsBattlegroundMap())
                {
                    Bot.Wow.LuaRepopMe();
                }
            }
            else if (Bot.Player.IsGhost)
            {
                StateMachine.SetState(BotState.Ghost);
            }
            else
            {
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Leave()
        {
            SetMapAndPosition = false;
        }
    }
}