using AmeisenBotX.BehaviorTree.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Waterfall : Composite
    {
        public Waterfall(Node fallbackNode, params (Func<bool> condition, Node node)[] conditionNodePairs) : base()
        {
            FallbackNode = fallbackNode;
            ConditionNodePairs = conditionNodePairs.ToList();
        }

        public List<(Func<bool> condition, Node node)> ConditionNodePairs { get; }

        public Node FallbackNode { get; set; }

        public override BtStatus Execute()
        {
            return GetNodeToExecute().Execute();
        }

        internal override Node GetNodeToExecute()
        {
            for (int i = 0; i < ConditionNodePairs.Count; ++i)
            {
                if (ConditionNodePairs[i].condition())
                {
                    return ConditionNodePairs[i].node;
                }
            }

            return FallbackNode;
        }
    }

    public class Waterfall<T> : Composite<T>
    {
        public Waterfall(Node<T> fallbackNode, params (Func<T, bool> condition, Node<T> node)[] conditionNodePairs) : base()
        {
            FallbackNode = fallbackNode;
            ConditionNodePairs = conditionNodePairs.ToList();
        }

        public List<(Func<T, bool> condition, Node<T> node)> ConditionNodePairs { get; }

        public Node<T> FallbackNode { get; set; }

        public override BtStatus Execute(T blackboard)
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