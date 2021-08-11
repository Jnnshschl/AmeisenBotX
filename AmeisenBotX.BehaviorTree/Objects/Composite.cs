using System.Collections.Generic;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public abstract class Composite : Node
    {
        public Composite() : base()
        {
        }

        public List<Node> Children { get; protected set; }
    }

    public abstract class Composite<T> : Node<T>
    {
        public Composite() : base()
        {
        }

        public List<Node<T>> Children { get; protected set; }
    }
}