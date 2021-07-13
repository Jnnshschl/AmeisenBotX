using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Fsm.Enums;
using System;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateTalkToQuestgivers : BasicState
    {
        private TimegatedEvent QuestgiverCheckEvent { get; }

        private TimegatedEvent QuestgiverRightClickEvent { get; }

        public StateTalkToQuestgivers(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
            QuestgiverCheckEvent = new(TimeSpan.FromMilliseconds(250));
            QuestgiverRightClickEvent = new(TimeSpan.FromMilliseconds(250));
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (!Config.AutoTalkToNearQuestgivers
                || !StateMachine.GetState<StateIdle>().IsUnitToFollowThere(out IWowUnit unitToFollow, true)
                || unitToFollow == null
                || unitToFollow.TargetGuid == 0)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            IWowUnit target = Bot.GetWowObjectByGuid<IWowUnit>(unitToFollow.TargetGuid);

            if (target == null || unitToFollow.DistanceTo(target) >= 5.0f || !(target.IsQuestgiver || target.IsGossip))
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (QuestgiverCheckEvent.Run())
            {
                HandleAutoQuestMode(target);
            }
        }

        public override void Leave()
        {
        }

        private void HandleAutoQuestMode(IWowUnit possibleQuestgiver)
        {
            float distance = Bot.Player.Position.GetDistance(possibleQuestgiver.Position);

            if (distance > 4.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, possibleQuestgiver.Position);
            }
            else if (QuestgiverRightClickEvent.Run())
            {
                if (!BotMath.IsFacing(Bot.Player.Position, Bot.Player.Rotation, possibleQuestgiver.Position))
                {
                    Bot.Wow.WowFacePosition(Bot.Player.BaseAddress, Bot.Player.Position, possibleQuestgiver.Position);
                }

                Bot.Wow.WowUnitRightClick(possibleQuestgiver.BaseAddress);
                Bot.Movement.StopMovement();
            }
        }
    }
}