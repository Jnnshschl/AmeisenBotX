using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Wow.Objects;
using System;

namespace AmeisenBotX.Core.Logic.Leafs
{
    public class InteractWithUnitLeaf : MoveToLeaf
    {
        public InteractWithUnitLeaf(AmeisenBotInterfaces bot, Func<IWowUnit> getUnit, INode child = null, float maxDistance = 3.2f, int interactInterval = 1500) : base(bot, getUnit, null, maxDistance)
        {
            Child = child;
            InteractionEvent = new(TimeSpan.FromMilliseconds(interactInterval));
        }

        private TimegatedEvent InteractionEvent { get; }

        public override BtStatus Execute()
        {
            BtStatus status = base.Execute();

            if (!InteractionEvent.Run())
            {
                return BtStatus.Ongoing;
            }

            if (status == BtStatus.Success)
            {
                Bot.Wow.InteractWithUnit(GetUnit());
            }

            return Child?.Execute() ?? BtStatus.Success;
        }
    }
}