using AmeisenBotX.BehaviorTree.Enums;
using System;

namespace AmeisenBotX.BehaviorTree.Objects
{
    /// <summary>
    /// Special selector that runs the first node where the condition returns true. If none returned
    /// true, the fallbackNode will be executed
    /// </summary>
    public class Waterfall(INode fallbackNode, params (Func<bool> condition, INode node)[] conditionNodePairs) : IComposite
    {
        public INode[] Children { get; }

        public (Func<bool> condition, INode node)[] ConditionNodePairs { get; } = conditionNodePairs;

        public INode FallbackNode { get; } = fallbackNode;

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

    public class Waterfall<T>(INode<T> fallbackNode, params (Func<T, bool> condition, INode<T> node)[] conditionNodePairs) : IComposite<T>
    {
        public INode<T>[] Children { get; }

        public (Func<T, bool> condition, INode<T> node)[] ConditionNodePairs { get; } = conditionNodePairs;

        public INode<T> FallbackNode { get; } = fallbackNode;

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