using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;

namespace AmeisenBotX.Core.Engines.Test
{
    public class DefaultTestEngine : ITestEngine
    {
        private const int trainerEntryId = 3173;
        private IWowUnit trainer;

        public DefaultTestEngine(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            Bot = bot;
            Config = config;

            RootSelector = new Selector
            (
                () => trainer != null,
                new Selector
                (
                    () => Bot.Wow.UiIsVisible("GossipFrame"),
                    new Leaf(SelectTraining),
                    new Leaf(OpenTrainer)
                ),
                new Leaf(GetTrainer)
            );

            TestTree = new Tree
            (
                RootSelector
            );
        }
        public AmeisenBotInterfaces Bot { get; }

        public AmeisenBotConfig Config { get; }

        private Tree TestTree { get; }

        private Selector RootSelector { get; }

        public void Execute()
        {
            TestTree.Tick();
        }

        private BtStatus GetTrainer()
        {
            if (Bot.GetClosestTrainerByEntryId(trainerEntryId) == null)
                return BtStatus.Failed;

            trainer = Bot.GetClosestTrainerByEntryId(trainerEntryId);
            return BtStatus.Success;
        }

        private BtStatus OpenTrainer()
        {
            if (Bot == null || trainer == null)
                return BtStatus.Failed;

            if (Bot.Wow.TargetGuid != trainer.Guid)
                Bot.Wow.ChangeTarget(trainer.Guid);

            if (!BotMath.IsFacing(Bot.Objects.Player.Position, Bot.Objects.Player.Rotation, trainer.Position, 0.5f))
                Bot.Wow.FacePosition(Bot.Objects.Player.BaseAddress, Bot.Player.Position, trainer.Position);

            if (Bot.Wow.UiIsVisible("GossipFrame"))
                return BtStatus.Success;

            Bot.Wow.InteractWithUnit(trainer.BaseAddress);
            return BtStatus.Success;
        }

        private BtStatus SelectTraining()
        {
            if (!trainer.IsGossip) 
                return BtStatus.Failed;

            // gossip 1 train skills
            // gossip 2 unlearn talents

             Bot.Wow.SelectGossipOptionSimple(1);

             return BtStatus.Success;
        }
        
        private static BtStatus Fail()
        {
            return BtStatus.Failed;
        }

        private static BtStatus Success()
        {
            return BtStatus.Success;
        }
    }
}