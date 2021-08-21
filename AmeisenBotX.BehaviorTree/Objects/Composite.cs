namespace AmeisenBotX.BehaviorTree.Objects
{
    public interface IComposite : INode
    {
        INode[] Children { get; }
    }

    public interface IComposite<T> : INode<T>
    {
        INode<T>[] Children { get; }
    }
}