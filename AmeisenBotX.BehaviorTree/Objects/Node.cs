using AmeisenBotX.BehaviorTree.Enums;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public abstract class Node
    {
        public abstract BehaviorTreeStatus Execute();

        internal abstract Node GetNodeToExecute();
    }

    public abstract class Node<T>
    {
        public abstract BehaviorTreeStatus Execute(T blackboard);

        internal abstract Node<T> GetNodeToExecute(T blackboard);
    }
}