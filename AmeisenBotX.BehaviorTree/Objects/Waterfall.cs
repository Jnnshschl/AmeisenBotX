using AmeisenBotX.BehaviorTree.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Waterfall<T> : Composite<T>
    {
        public Waterfall(Node<T> fallbackNode, params (Func<T, bool> condition, Node<T> node)[] conditionNodePairs) : base("")
        {
            FallbackNode = fallbackNode;
            ConditionNodePairs = conditionNodePairs.ToList();
        }

        public Waterfall(string name, Node<T> fallbackNode, params (Func<T, bool> condition, Node<T> node)[] conditionNodePairs) : base(name)
        {
            FallbackNode = fallbackNode;
            ConditionNodePairs = conditionNodePairs.ToList();
        }

        public List<(Func<T, bool> condition, Node<T> node)> ConditionNodePairs { get; }

        public Node<T> FallbackNode { get; set; }

        public override BehaviorTreeStatus Execute(T blackboard)
        {
            return GetNodeToExecute(blackboard).Execute(blackboard);
        }

        internal override Node<T> GetNodeToExecute(T blackboard)
        {
            for (int i = 0; i < ConditionNodePairs.Count; ++i)
            {
                if (ConditionNodePairs[i].condition(blackboard))
                {
                    return ConditionNodePairs[i].node;
                }
            }

            return FallbackNode;
        }
    }
}