using AmeisenBotX.BehaviorTree.Enums;
using System;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Leaf<T> : Node<T>
    {
        public Leaf(Func<T, BehaviorTreeStatus> behaviorTreeAction)
        {
            BehaviorTreeAction = behaviorTreeAction;
        }

        public Func<T, BehaviorTreeStatus> BehaviorTreeAction { get; set; }

        public override BehaviorTreeStatus Execute(T blackboard)
        {
            return BehaviorTreeAction(blackboard);
        }
    }
}