using AmeisenBotX.BehaviorTree.Enums;

namespace AmeisenBotX.BehaviorTree.Objects
{
    /// <summary>
    /// Inverts the status of a node. Ongoing state will remain Onging.
    /// </summary>
    public class Inverter(INode child) : INode
    {
        public INode Child { get; } = child;

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

    public class Inverter<T>(INode<T> child) : INode<T>
    {
        public INode<T> Child { get; } = child;

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