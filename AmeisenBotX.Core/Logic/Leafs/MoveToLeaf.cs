using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using System;

namespace AmeisenBotX.Core.Logic.Leafs
{
    public class MoveToLeaf : INode
    {
        public MoveToLeaf(AmeisenBotInterfaces bot, Func<IWowUnit> getUnit, INode child = null, float maxDistance = 3.2f)
        {
            Bot = bot;
            GetUnit = getUnit;
            Child = child;
            MaxDistance = maxDistance;
        }

        protected AmeisenBotInterfaces Bot { get; }

        protected INode Child { get; set; }

        protected Func<IWowUnit> GetUnit { get; }

        protected float MaxDistance { get; }

        protected bool NeedToStopMoving { get; set; }

        public virtual BtStatus Execute()
        {
            IWowUnit unit = GetUnit();

            if (unit == null)
            {
                return BtStatus.Failed;
            }

            if (Bot.Player.DistanceTo(unit) > MaxDistance)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, unit.Position);
                NeedToStopMoving = true;
                return BtStatus.Ongoing;
            }

            if (NeedToStopMoving)
            {
                NeedToStopMoving = false;
                Bot.Movement.StopMovement();
            }

            return Child?.Execute() ?? BtStatus.Success;
        }

        public INode GetNodeToExecute()
        {
            return this;
        }
    }
}