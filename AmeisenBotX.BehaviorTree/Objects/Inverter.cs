using AmeisenBotX.BehaviorTree.Enums;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Inverter : Node
    {
        public Inverter(Node child) : base()
        {
            Child = child;
        }

        public Node Child { get; set; }

        public override BtStatus Execute()
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

        internal override Node GetNodeToExecute()
        {
            return Child;
        }
    }

    public class Inverter<T> : Node<T>
    {
        public Inverter(Node<T> child) : base()
        {
            Child = child;
        }

        public Node<T> Child { get; set; }

        public override BtStatus Execute(T blackboard)
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

        internal override Node<T> GetNodeToExecute(T blackboard)
        {
            return Child;
        }
    }
}