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
        private readonly IWowUnit trainer;

        public DefaultTestEngine(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            Bot = bot;
            Config = config;

            trainer = Bot.GetClosestTrainerByEntryId(trainerEntryId);

            RootSelector = new Selector
            (
                () => trainer != null,
                new Selector
                (
                    () => Bot.Wow.UiIsVisible("GossipFrame", "MerchantFrame"),
                    new Leaf(SelectTraining),
                    new Leaf(OpenTrainer)
                ),
                new Leaf(Fail)
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

        private BtStatus OpenTrainer()
        {
            if (Bot == null || trainer == null)
                return BtStatus.Failed;

            if (Bot.Wow.TargetGuid != trainer.Guid)
                Bot.Wow.ChangeTarget(trainer.Guid);

            if (!BotMath.IsFacing(Bot.Objects.Player.Position, Bot.Objects.Player.Rotation, trainer.Position, 0.5f))
                Bot.Wow.FacePosition(Bot.Objects.Player.BaseAddress, Bot.Player.Position, trainer.Position);

            if (Bot.Wow.UiIsVisible("GossipFrame", "MerchantFrame"))
                return BtStatus.Success;

            Bot.Wow.InteractWithUnit(trainer.BaseAddress);
            return BtStatus.Success;
        }

        private BtStatus SelectTraining()
        {
            if (!trainer.IsGossip) 
                return BtStatus.Failed;

            // TODO: resolve this mess??
            // Bot.Wow.SelectGossipOptionSimple(1);             does nothing
            // LuaDoString from devTools SelectGossipOption(1); learn skills frame
            // Bot.Wow.SelectGossipOption(1);                   unlearns talents frame

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