using AmeisenBotX.BehaviorTree.Enums;
using System;

namespace AmeisenBotX.BehaviorTree.Objects
{
    /// <summary>
    /// Executes a when input is true and b when it is false.
    /// </summary>
    public class Selector(Func<bool> condition, INode nodeA, INode nodeB) : IComposite
    {
        public INode[] Children { get; } = [nodeA, nodeB];

        public Func<bool> Condition { get; } = condition;

        public BtStatus Execute()
        {
            return GetNodeToExecute().Execute();
        }

        public INode GetNodeToExecute()
        {
            return Condition() ? Children[0] : Children[1];
        }
    }

    public class Selector<T>(Func<T, bool> condition, INode<T> nodeA, INode<T> nodeB) : IComposite<T>
    {
        public INode<T>[] Children { get; } = [nodeA, nodeB];

        public Func<T, bool> Condition { get; } = condition;

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