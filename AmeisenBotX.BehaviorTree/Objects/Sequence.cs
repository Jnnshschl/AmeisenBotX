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

        public override BehaviorTreeStatus Execute(T blackboard)
        {
            if (Condition(blackboard))
            {
                return Children[0].Execute(blackboard);
            }
            else
            {
                return Children[1].Execute(blackboard);
            }
        }
    }
}