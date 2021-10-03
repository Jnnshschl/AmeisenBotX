using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;

namespace AmeisenBotX.Core.Engines.PvP
{
    public class DefaultPvpEngine : IPvpEngine
    {
        public DefaultPvpEngine(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            Bot = bot;
            Config = config;

            INode mainNode = new Annotator
            (
                new Leaf(() => { Bot.Memory.Read(Bot.Wow.Offsets.BattlegroundStatus, out int q); QueueStatus = q; return BtStatus.Success; }),
                new Waterfall
                (
                    new Leaf(() => BtStatus.Ongoing),
                    (() => QueueStatus == 0, new Leaf(QueueForBattlegrounds))
                    // (() => QueueStatus == 2, new Leaf(() => { Bot.Wow.AcceptBattlegroundInvite(); return BtStatus.Success; }))
                )
            );

            Bt = new(mainNode);
        }

        private AmeisenBotInterfaces Bot { get; }

        private Tree Bt { get; }

        private AmeisenBotConfig Config { get; }

        private int QueueStatus { get; set; }

        public void Execute()
        {
            Bt.Tick();
        }

        private BtStatus QueueForBattlegrounds()
        {
            // TODO: fix this fucntion
            // Bot.Wow.LuaQueueBattlegroundByName("Warsong Gulch");

            Bot.Wow.ClickUiElement("BattlegroundType2");
            Bot.Wow.ClickUiElement("PVPBattlegroundFrameJoinButton");

            return BtStatus.Success;
        }
    }
}