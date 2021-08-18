using AmeisenBotX.BehaviorTree.Enums;
using System;

namespace AmeisenBotX.BehaviorTree.Objects
{
    /// <summary>
    /// Executes a when input is true and b when it is false.
    /// </summary>
    public class Selector : IComposite
    {
        public Selector(Func<bool> condition, INode nodeA, INode nodeB) : base()
        {
            Condition = condition;
            Children = new INode[] { nodeA, nodeB };
        }

        public INode[] Children { get; }

        public Func<bool> Condition { get; }

        public BtStatus Execute()
        {
            return GetNodeToExecute().Execute();
        }

        public INode GetNodeToExecute()
        {
            return Condition() ? Children[0] : Children[1];
        }
    }

    public class Selector<T> : IComposite<T>
    {
        public Selector(Func<T, bool> condition, INode<T> nodeA, INode<T> nodeB) : base()
        {
            Condition = condition;
            Children = new INode<T>[] { nodeA, nodeB };
        }

        public INode<T>[] Children { get; }

        public Func<T, bool> Condition { get; }

        public BtStatus Execute(T blackboard)
        {
            return GetNodeToExecute(blackboard).Execute(blackboard);
        }

        public INode<T> GetNodeToExecute(T blackboard)
        {
            return Condition(blackboard) ? Children[0] : Children[1];
        }
    }
}