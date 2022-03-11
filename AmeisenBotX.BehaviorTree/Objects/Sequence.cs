using AmeisenBotX.BehaviorTree.Enums;

namespace AmeisenBotX.BehaviorTree.Objects
{
    /// <summary>
    /// Executes a sequence of nodes until all nodes returned success. If a node fails or the
    /// sequence finished, it gets resetted.
    /// </summary>
    public class Sequence : IComposite
    {
        public Sequence(params INode[] children)
        {
            Children = children;
        }

        public INode[] Children { get; }

        public int Counter { get; private set; }

        public BtStatus Execute()
        {
            if (Counter == Children.Length)
            {
                Counter = 0;
                return BtStatus.Success;
            }

            BtStatus status = Children[Counter].Execute();

            if (status == BtStatus.Success)
            {
                if (Counter < Children.Length)
                {
                    ++Counter;

                    if (Counter == Children.Length)
                    {
                        Counter = 0;
                        return BtStatus.Success;
                    }
                }
            }
            else if (status == BtStatus.Failed)
            {
                Counter = 0;
                return BtStatus.Failed;
            }

            return BtStatus.Ongoing;
        }

        public INode GetNodeToExecute()
        {
            return this;
        }
    }

    public class Sequence<T> : IComposite<T>
    {
        public Sequence(params INode<T>[] children)
        {
            Children = children;
        }

        public INode<T>[] Children { get; }

        public int Counter { get; private set; }

        public BtStatus Execute(T blackboard)
        {
            if (Counter == Children.Length)
            {
                Counter = 0;
                return BtStatus.Success;
            }

            BtStatus status = Children[Counter].Execute(blackboard);

            if (status == BtStatus.Success)
            {
                if (Counter < Children.Length)
                {
                    ++Counter;

                    if (Counter == Children.Length)
                    {
                        Counter = 0;
                        return BtStatus.Success;
                    }
                }
            }
            else if (status == BtStatus.Failed)
            {
                Counter = 0;
                return BtStatus.Failed;
            }

            return BtStatus.Ongoing;
        }

        public INode<T> GetNodeToExecute(T blackboard)
        {
            return this;
        }
    }
}