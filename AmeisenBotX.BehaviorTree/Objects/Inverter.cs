using AmeisenBotX.BehaviorTree.Enums;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Inverter<T> : Node<T>
    {
        public Inverter(Node<T> child) : base("")
        {
            Child = child;
        }

        public Inverter(string name, Node<T> child) : base(name)
        {
            Child = child;
        }

        public Node<T> Child { get; set; }

        public override BehaviorTreeStatus Execute(T blackboard)
        {
            BehaviorTreeStatus status = Child.Execute(blackboard);

            if (status == BehaviorTreeStatus.Success)
            {
                status = BehaviorTreeStatus.Failed;
            }
            else if (status == BehaviorTreeStatus.Failed)
            {
                status = BehaviorTreeStatus.Success;
            }

            return status;
        }

        internal override Node<T> GetNodeToExecute(T blackboard)
        {
            return Child;
        }
    }
}