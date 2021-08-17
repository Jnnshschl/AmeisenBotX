using AmeisenBotX.BehaviorTree.Enums;
using System.Linq;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Sequence : Composite
    {
        public Sequence(params Node[] children)
        {
            Children = children.ToList();
        }

        public int Counter { get; set; }

        public override BtStatus Execute()
        {
            if (Counter == Children.Count)
            {
                return BtStatus.Success;
            }

            BtStatus status = Children[Counter].Execute();

            if (status == BtStatus.Success)
            {
                if (Counter < Children.Count)
                {
                    ++Counter;

                    if (Counter == Children.Count)
                    {
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

        internal override Node GetNodeToExecute()
        {
            return this;
        }
    }

    public class Sequence<T> : Composite<T>
    {
        public Sequence(params Node<T>[] children)
        {
            Children = children.ToList();
        }

        public int Counter { get; set; }

        public override BtStatus Execute(T blackboard)
        {
            if (Counter == Children.Count)
            {
                return BtStatus.Success;
            }

            BtStatus status = Children[Counter].Execute(blackboard);

            if (status == BtStatus.Success)
            {
                if (Counter < Children.Count)
                {
                    ++Counter;

                    if (Counter == Children.Count)
                    {
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

        internal override Node<T> GetNodeToExecute(T blackboard)
        {
            return this;
        }
    }
}