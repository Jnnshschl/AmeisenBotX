using AmeisenBotX.BehaviorTree.Enums;
using System;

namespace AmeisenBotX.BehaviorTree.Objects
{
    /// <summary>
    /// Special selector that executes nodeNone when input is 0|0, nodeA when input is 1|0, nodeB
    /// when input is 0|1 and nodeBoth when input is 1|1.
    /// </summary>
    public class DualSelector : IComposite
    {
        public DualSelector(Func<bool> conditionA, Func<bool> conditionB, INode nodeNone, INode nodeA, INode nodeB, INode nodeBoth) : base()
        {
            ConditionA = conditionA;
            ConditionB = conditionB;
            Children = new INode[] { nodeNone, nodeA, nodeB, nodeBoth };
        }

        public INode[] Children { get; }

        public Func<bool> ConditionA { get; }

        public Func<bool> ConditionB { get; }

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

    public class DualSelector<T> : IComposite<T>
    {
        public DualSelector(Func<T, bool> conditionA, Func<T, bool> conditionB, INode<T> nodeNone, INode<T> nodeA, INode<T> nodeB, INode<T> nodeBoth) : base()
        {
            ConditionA = conditionA;
            ConditionB = conditionB;
            Children = new INode<T>[] { nodeNone, nodeA, nodeB, nodeBoth };
        }

        public INode<T>[] Children { get; }

        public Func<T, bool> ConditionA { get; }

        public Func<T, bool> ConditionB { get; }

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