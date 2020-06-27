using AmeisenBotX.BehaviorTree.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Selector<T> : Composite<T>
    {
        public Selector(Func<T, bool> condition, Node<T> nodeA, Node<T> nodeB)
        {
            Condition = condition;
            Children = new List<Node<T>>() { nodeA, nodeB };
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