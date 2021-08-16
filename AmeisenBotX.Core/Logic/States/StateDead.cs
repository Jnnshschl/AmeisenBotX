using AmeisenBotX.Core.Logic.Enums;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Logic.States
{
    public class StateDead : BasicState
    {
        public StateDead(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        /// <summary>
        /// The map we died on.
        /// </summary>
        public WowMapId LastDiedMap { get; set; }

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
                    LastDiedMap = Bot.Objects.MapId;
                }

                if (Config.ReleaseSpirit || Bot.Objects.MapId.IsBattlegroundMap())
                {
                    Bot.Wow.RepopMe();
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