using AmeisenBotX.BehaviorTree.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Sequence<T> : Composite<T>
    {
        public Sequence(Func<T, bool> condition, List<Node<T>> children)
        {
            Condition = condition;
            Children = children;
        }

        public Func<T, bool> Condition { get; set; }

        public int Counter { get; set; }

        public override BehaviorTreeStatus Execute(T blackboard)
        {
            if (Counter == Children.Count)
            {
                return BehaviorTreeStatus.Success;
            }

            BehaviorTreeStatus status = Children[Counter].Execute(blackboard);

            if (status == BehaviorTreeStatus.Success)
            {
                if (Counter < Children.Count)
                {
                    ++Counter;
                }
            }
            else if (status == BehaviorTreeStatus.Failed)
            {
                Counter = 0;
                return BehaviorTreeStatus.Failed;
            }

            return BehaviorTreeStatus.Ongoing;
        }
    }
}