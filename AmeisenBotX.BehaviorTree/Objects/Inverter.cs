using AmeisenBotX.BehaviorTree.Enums;

namespace AmeisenBotX.BehaviorTree.Objects
{
    /// <summary>
    /// Inverts the status of a node. Ongoing state will remain Onging.
    /// </summary>
    public class Inverter : INode
    {
        public Inverter(INode child) : base()
        {
            Child = child;
        }

        public INode Child { get; }

        public BtStatus Execute()
        {
            BtStatus status = Child.Execute();

            if (status == BtStatus.Success)
            {
                status = BtStatus.Failed;
            }
            else if (status == BtStatus.Failed)
            {
                status = BtStatus.Success;
            }

            return status;
        }

        public INode GetNodeToExecute()
        {
            return Child;
        }
    }

    public class Inverter<T> : INode<T>
    {
        public Inverter(INode<T> child) : base()
        {
            Child = child;
        }

        public INode<T> Child { get; }

        public BtStatus Execute(T blackboard)
        {
            BtStatus status = Child.Execute(blackboard);

            if (status == BtStatus.Success)
            {
                status = BtStatus.Failed;
            }
            else if (status == BtStatus.Failed)
            {
                status = BtStatus.Success;
            }

            return status;
        }

        public INode<T> GetNodeToExecute(T blackboard)
        {
            return Child;
        }
    }
}