using System.Collections.Generic;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public abstract class Composite<T> : Node<T>
    {
        public List<Node<T>> Children { get; set; }
    }
}