using AmeisenBotX.BehaviorTree.Enums;
using System;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Selector : Composite
    {
        public Selector(Func<bool> condition, Node nodeA, Node nodeB) : base()
        {
            Condition = condition;
            Children = new() { nodeA, nodeB };
        }

        public Func<bool> Condition { get; set; }

        public override BtStatus Execute()
        {
            return GetNodeToExecute().Execute();
        }

        internal override Node GetNodeToExecute()
        {
            if (Condition())
            {
                return Children[0];
            }
            else
            {
                return Children[1];
            }
        }
    }

    public class Selector<T> : Composite<T>
    {
        public Selector(Func<T, bool> condition, Node<T> nodeA, Node<T> nodeB) : base()
        {
            Condition = condition;
            Children = new() { nodeA, nodeB };
        }

        public Func<T, bool> Condition { get; set; }

        public override BtStatus Execute(T blackboard)
        {
            return GetNodeToExecute(blackboard).Execute(blackboard);
        }

        internal override Node<T> GetNodeToExecute(T blackboard)
        {
            if (Condition(blackboard))
            {
                return Children[0];
            }
            else
            {
                return Children[1];
            }
        }
    }
}