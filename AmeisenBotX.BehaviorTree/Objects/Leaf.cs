using AmeisenBotX.BehaviorTree.Enums;
using System;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Leaf : Node
    {
        public Leaf(Func<BehaviorTreeStatus> behaviorTreeAction) : base()
        {
            BehaviorTreeAction = behaviorTreeAction;
        }

        public Func<BehaviorTreeStatus> BehaviorTreeAction { get; set; }

        public override BehaviorTreeStatus Execute()
        {
            return BehaviorTreeAction();
        }

        internal override Node GetNodeToExecute()
        {
            return this;
        }
    }

    public class Leaf<T> : Node<T>
    {
        public Leaf(Func<T, BehaviorTreeStatus> behaviorTreeAction) : base()
        {
            BehaviorTreeAction = behaviorTreeAction;
        }

        public Func<T, BehaviorTreeStatus> BehaviorTreeAction { get; set; }

        public override BehaviorTreeStatus Execute(T blackboard)
        {
            return BehaviorTreeAction(blackboard);
        }

        internal override Node<T> GetNodeToExecute(T blackboard)
        {
            return this;
        }
    }
}