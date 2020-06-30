using AmeisenBotX.BehaviorTree.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class DualSelector<T> : Composite<T>
    {
        public DualSelector(Func<T, bool> conditionA, Func<T, bool> conditionB, Node<T> nodeNone, Node<T> nodeA, Node<T> nodeB, Node<T> nodeBoth) : base("")
        {
            ConditionA = conditionA;
            ConditionB = conditionB;
            Children = new List<Node<T>>() { nodeNone, nodeA, nodeB, nodeBoth };
        }

        public DualSelector(string name, Func<T, bool> conditionA, Func<T, bool> conditionB, Node<T> nodeNone, Node<T> nodeA, Node<T> nodeB, Node<T> nodeBoth) : base(name)
        {
            ConditionA = conditionA;
            ConditionB = conditionB;
            Children = new List<Node<T>>() { nodeNone, nodeA, nodeB, nodeBoth };
        }

        public Func<T, bool> ConditionA { get; set; }

        public Func<T, bool> ConditionB { get; set; }

        public override BehaviorTreeStatus Execute(T blackboard)
        {
            return GetNodeToExecute(blackboard).Execute(blackboard);
        }

        internal override Node<T> GetNodeToExecute(T blackboard)
        {
            if (ConditionA(blackboard) && ConditionB(blackboard))
            {
                return Children[3];
            }
            else if (ConditionA(blackboard) && !ConditionB(blackboard))
            {
                return Children[1];
            }
            else if (!ConditionA(blackboard) && ConditionB(blackboard))
            {
                return Children[2];
            }
            else
            {
                return Children[0];
            }
        }
    }
}