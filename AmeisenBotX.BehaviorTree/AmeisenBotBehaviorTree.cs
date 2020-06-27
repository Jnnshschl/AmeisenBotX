using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;

namespace AmeisenBotX.BehaviorTree
{
    public class AmeisenBotBehaviorTree<T>
    {
        public AmeisenBotBehaviorTree(Node<T> node, T blackboard)
        {
            RootNode = node;
            Blackboard = blackboard;
        }

        public T Blackboard { get; set; }

        public Node<T> RootNode { get; set; }

        public BehaviorTreeStatus Tick()
        {
            return RootNode.Execute(Blackboard);
        }
    }
}