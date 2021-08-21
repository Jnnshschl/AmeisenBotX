using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Fsm.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateGoingToVendor : BasicState
    {
        public StateGoingToVendor(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute() // TODO: this will need some better implementation
        {
            var vendorsLocations = new List<Vector3>();

            var questsList = Bot.Quest.Profile.Quests.ToList();
            var firstQuestInQue = questsList.First();
            var currentQuest = firstQuestInQue.First();
            var questObjectiveChain = (QuestObjectiveChain) currentQuest.Objectives[0];

            foreach (var questObjective in questObjectiveChain.QuestObjectives)
            {
                vendorsLocations = questObjective switch
                {
                    GrindingObjective grindingObjective => grindingObjective.VendorsLocation,
                    KillAndLootQuestObjective killAndLootObjective => killAndLootObjective.VendorsLocation,

                    _ => throw new ArgumentOutOfRangeException(questObjective.GetType().ToString(), "Unrecognized quest objective")
                };
            }

            if (vendorsLocations.Any() && Bot.Player.DistanceTo(vendorsLocations.FirstOrDefault()) > 5.0f)
                Bot.Movement.SetMovementAction(MovementAction.Move, vendorsLocations.FirstOrDefault());
            else if (vendorsLocations.Any() && Bot.Player.DistanceTo(vendorsLocations.FirstOrDefault()) < 5.0f)
                StateMachine.SetState(BotState.Selling);
        }

        public override void Leave()
        {
        }
    }
}