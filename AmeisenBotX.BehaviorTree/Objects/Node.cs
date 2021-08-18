using AmeisenBotX.BehaviorTree.Enums;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public interface INode
    {
        BtStatus Execute();

        INode GetNodeToExecute();
    }

    public interface INode<T>
    {
        BtStatus Execute(T blackboard);

        INode<T> GetNodeToExecute(T blackboard);
    }
}