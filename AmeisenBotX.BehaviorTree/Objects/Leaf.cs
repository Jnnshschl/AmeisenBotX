using AmeisenBotX.BehaviorTree.Enums;
using System;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Leaf : INode
    {
        public Leaf(Func<BtStatus> behaviorTreeAction) : base()
        {
            BehaviorTreeAction = behaviorTreeAction;
        }

        public Func<BtStatus> BehaviorTreeAction { get; set; }

        public BtStatus Execute()
        {
            return BehaviorTreeAction();
        }

        public INode GetNodeToExecute()
        {
            return this;
        }
    }

    public class Leaf<T> : INode<T>
    {
        public Leaf(Func<T, BtStatus> behaviorTreeAction) : base()
        {
            BehaviorTreeAction = behaviorTreeAction;
        }

        public Func<T, BtStatus> BehaviorTreeAction { get; set; }

        public BtStatus Execute(T blackboard)
        {
            return BehaviorTreeAction(blackboard);
        }

        public INode<T> GetNodeToExecute(T blackboard)
        {
            return this;
        }
    }
}