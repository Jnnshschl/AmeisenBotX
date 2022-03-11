using AmeisenBotX.BehaviorTree.Enums;
using System;

namespace AmeisenBotX.BehaviorTree.Objects
{
    /// <summary>
    /// Special selector that runs the first node where the condition returns true. If none returned
    /// true, the fallbackNode will be executed
    /// </summary>
    public class Waterfall : IComposite
    {
        public Waterfall(INode fallbackNode, params (Func<bool> condition, INode node)[] conditionNodePairs) : base()
        {
            FallbackNode = fallbackNode;
            ConditionNodePairs = conditionNodePairs;
        }

        public INode[] Children { get; }

        public (Func<bool> condition, INode node)[] ConditionNodePairs { get; }

        public INode FallbackNode { get; }

        public BtStatus Execute()
        {
            return GetNodeToExecute().Execute();
        }

        public INode GetNodeToExecute()
        {
            for (int i = 0; i < ConditionNodePairs.Length; ++i)
            {
                if (ConditionNodePairs[i].condition())
                {
                    return ConditionNodePairs[i].node;
                }
            }

            return FallbackNode;
        }
    }

    public class Waterfall<T> : IComposite<T>
    {
        public Waterfall(INode<T> fallbackNode, params (Func<T, bool> condition, INode<T> node)[] conditionNodePairs) : base()
        {
            FallbackNode = fallbackNode;
            ConditionNodePairs = conditionNodePairs;
        }

        public INode<T>[] Children { get; }

        public (Func<T, bool> condition, INode<T> node)[] ConditionNodePairs { get; }

        public INode<T> FallbackNode { get; }

        public BtStatus Execute(T blackboard)
        {
            return GetNodeToExecute(blackboard).Execute(blackboard);
        }

        public INode<T> GetNodeToExecute(T blackboard)
        {
            for (int i = 0; i < ConditionNodePairs.Length; ++i)
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