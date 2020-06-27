using AmeisenBotX.BehaviorTree.Enums;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public abstract class Node<T>
    {
        public abstract BehaviorTreeStatus Execute(T blackboard);
    }
}