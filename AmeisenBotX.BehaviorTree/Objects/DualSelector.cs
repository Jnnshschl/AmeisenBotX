using AmeisenBotX.BehaviorTree.Enums;
using System;

namespace AmeisenBotX.BehaviorTree.Objects
{
    /// <summary>
    /// Special selector that executes nodeNone when input is 0|0, nodeA when input is 1|0, nodeB
    /// when input is 0|1 and nodeBoth when input is 1|1.
    /// </summary>
    public class DualSelector(Func<bool> conditionA, Func<bool> conditionB, INode nodeNone, INode nodeA, INode nodeB, INode nodeBoth) : IComposite
    {
        public INode[] Children { get; } = [nodeNone, nodeA, nodeB, nodeBoth];

        public Func<bool> ConditionA { get; } = conditionA;

        public Func<bool> ConditionB { get; } = conditionB;

        public BtStatus Execute()
        {
            return GetNodeToExecute().Execute();
        }

        public INode GetNodeToExecute()
        {
            return ConditionA() && ConditionB()
                ? Children[3]
                : ConditionA() && !ConditionB() ? Children[1] : !ConditionA() && ConditionB() ? Children[2] : Children[0];
        }
    }

    public class DualSelector<T>(Func<T, bool> conditionA, Func<T, bool> conditionB, INode<T> nodeNone, INode<T> nodeA, INode<T> nodeB, INode<T> nodeBoth) : IComposite<T>
    {
        public INode<T>[] Children { get; } = [nodeNone, nodeA, nodeB, nodeBoth];

        public Func<T, bool> ConditionA { get; } = conditionA;

        public Func<T, bool> ConditionB { get; } = conditionB;

        public BtStatus Execute(T blackboard)
        {
            return GetNodeToExecute(blackboard).Execute(blackboard);
        }

        public INode<T> GetNodeToExecute(T blackboard)
        {
            return ConditionA(blackboard) && ConditionB(blackboard)
                ? Children[3]
                : ConditionA(blackboard) && !ConditionB(blackboard)
                    ? Children[1]
                    : !ConditionA(blackboard) && ConditionB(blackboard) ? Children[2] : Children[0];
        }
    }
}