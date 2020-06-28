using AmeisenBotX.BehaviorTree.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class DuoSelector<T> : Composite<T>
    {
        public DuoSelector(Func<T, bool> conditionA, Func<T, bool> conditionB, Node<T> nodeA, Node<T> nodeB, Node<T> nodeBoth, Node<T> nodeNone)
        {
            ConditionA = conditionA;
            ConditionB = conditionB;
            Children = new List<Node<T>>() { nodeA, nodeB, nodeBoth, nodeNone };
        }

        public Func<T, bool> ConditionA { get; set; }

        public Func<T, bool> ConditionB { get; set; }

        public override BehaviorTreeStatus Execute(T blackboard)
        {
            if (ConditionA(blackboard) && ConditionB(blackboard))
            {
                return Children[2].Execute(blackboard);
            }
            else if (ConditionA(blackboard) && !ConditionB(blackboard))
            {
                return Children[0].Execute(blackboard);
            }
            else if (!ConditionA(blackboard) && ConditionB(blackboard))
            {
                return Children[1].Execute(blackboard);
            }
            else
            {
                return Children[3].Execute(blackboard);
            }
        }
    }
}