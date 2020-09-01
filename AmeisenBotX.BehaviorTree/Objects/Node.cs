using AmeisenBotX.BehaviorTree.Enums;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public abstract class Node
    {
        public Node(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public abstract BehaviorTreeStatus Execute();

        internal abstract Node GetNodeToExecute();
    }

    public abstract class Node<T>
    {
        public Node(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public abstract BehaviorTreeStatus Execute(T blackboard);

        internal abstract Node<T> GetNodeToExecute(T blackboard);
    }
}