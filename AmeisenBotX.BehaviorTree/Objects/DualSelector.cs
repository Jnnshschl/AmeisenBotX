using AmeisenBotX.BehaviorTree.Enums;
using System;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class DualSelector : Composite
    {
        public DualSelector(Func<bool> conditionA, Func<bool> conditionB, Node nodeNone, Node nodeA, Node nodeB, Node nodeBoth) : base()
        {
            ConditionA = conditionA;
            ConditionB = conditionB;
            Children = new() { nodeNone, nodeA, nodeB, nodeBoth };
        }

        public Func<bool> ConditionA { get; set; }

        public Func<bool> ConditionB { get; set; }

        public override BtStatus Execute()
        {
            return GetNodeToExecute().Execute();
        }

        internal override Node GetNodeToExecute()
        {
            if (ConditionA() && ConditionB())
            {
                return Children[3];
            }
            else if (ConditionA() && !ConditionB())
            {
                return Children[1];
            }
            else if (!ConditionA() && ConditionB())
            {
                return Children[2];
            }
            else
            {
                return Children[0];
            }
        }
    }

    public class DualSelector<T> : Composite<T>
    {
        public DualSelector(Func<T, bool> conditionA, Func<T, bool> conditionB, Node<T> nodeNone, Node<T> nodeA, Node<T> nodeB, Node<T> nodeBoth) : base()
        {
            ConditionA = conditionA;
            ConditionB = conditionB;
            Children = new() { nodeNone, nodeA, nodeB, nodeBoth };
        }

        public Func<T, bool> ConditionA { get; set; }

        public Func<T, bool> ConditionB { get; set; }

        public override BtStatus Execute(T blackboard)
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