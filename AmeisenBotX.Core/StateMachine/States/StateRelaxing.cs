using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using System;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateRelaxing : BasicState
    {
        public StateRelaxing(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            // if we are not in a group and in a relaxing zone
            if (WowInterface.ObjectManager.PartyleaderGuid == 0
                && Enum.IsDefined(typeof(ZoneId), WowInterface.ObjectManager.ZoneId)
                && ((ZoneId)WowInterface.ObjectManager.ZoneId).IsCapitalCityZone(WowInterface.ObjectManager.Player.IsAlliance()))
            {
                WowInterface.RelaxEngine.Execute();
            }
            else
            {
                StateMachine.SetState((int)BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}