using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Wow.Objects;
using System;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateTalkToQuestgivers : BasicState
    {
        public StateTalkToQuestgivers(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
            QuestgiverCheckEvent = new(TimeSpan.FromMilliseconds(250));
            QuestgiverRightClickEvent = new(TimeSpan.FromMilliseconds(250));
        }

        private TimegatedEvent QuestgiverCheckEvent { get; }

        private TimegatedEvent QuestgiverRightClickEvent { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (Config.AutoTalkToNearQuestgivers)
            {
                if (StateMachine.Get<StateFollowing>().IsUnitToFollowThere(out IWowUnit unitToFollow, true)
                    && unitToFollow.TargetGuid != 0)
                {
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
                else
                {
                    StateMachine.SetState(BotState.Idle);
                    return;
                }
            }
        }

        public override void Leave()
        {
        }

        private void HandleAutoQuestMode(IWowUnit possibleQuestgiver)
        {
            if (Bot.Player.Position.GetDistance(possibleQuestgiver.Position) > 4.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, possibleQuestgiver.Position);
            }
            else if (QuestgiverRightClickEvent.Run())
            {
                if (!BotMath.IsFacing(Bot.Player.Position, Bot.Player.Rotation, possibleQuestgiver.Position))
                {
                    Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, possibleQuestgiver.Position);
                }

                Bot.Wow.InteractWithUnit(possibleQuestgiver.BaseAddress);
                Bot.Movement.StopMovement();
            }
        }
    }
}