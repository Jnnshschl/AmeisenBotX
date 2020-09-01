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

        public Sequence(string name, params Node[] children) : base(name)
        {
            Children = children.ToList();
        }

        public int Counter { get; set; }

        public override BehaviorTreeStatus Execute()
        {
            if (Counter == Children.Count)
            {
                return BehaviorTreeStatus.Success;
            }

            BehaviorTreeStatus status = Children[Counter].Execute();

            if (status == BehaviorTreeStatus.Success)
            {
                if (Counter < Children.Count)
                {
                    ++Counter;

                    if (Counter == Children.Count)
                    {
                        return BehaviorTreeStatus.Success;
                    }
                }
            }
            else if (status == BehaviorTreeStatus.Failed)
            {
                Counter = 0;
                return BehaviorTreeStatus.Failed;
            }

            return BehaviorTreeStatus.Ongoing;
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

        public Sequence(string name, params Node<T>[] children) : base(name)
        {
            Children = children.ToList();
        }

        public int Counter { get; set; }

        public override BehaviorTreeStatus Execute(T blackboard)
        {
            if (Counter == Children.Count)
            {
                return BehaviorTreeStatus.Success;
            }

            BehaviorTreeStatus status = Children[Counter].Execute(blackboard);

            if (status == BehaviorTreeStatus.Success)
            {
                if (Counter < Children.Count)
                {
                    ++Counter;

                    if (Counter == Children.Count)
                    {
                        return BehaviorTreeStatus.Success;
                    }
                }
            }
            else if (status == BehaviorTreeStatus.Failed)
            {
                Counter = 0;
                return BehaviorTreeStatus.Failed;
            }

            return BehaviorTreeStatus.Ongoing;
        }

        internal override Node<T> GetNodeToExecute(T blackboard)
        {
            return this;
        }
    }
}