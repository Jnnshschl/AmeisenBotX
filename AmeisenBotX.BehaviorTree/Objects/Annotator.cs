using AmeisenBotX.BehaviorTree.Enums;

namespace AmeisenBotX.BehaviorTree.Objects
{
    /// <summary>
    /// BehaviorTree Node that executes a node before executing annother node. Use this to update stuff before executing a node.
    /// </summary>
    public class Annotator : INode
    {
        public Annotator(INode annotationNode, INode child) : base()
        {
            AnnotationNode = annotationNode;
            Child = child;
        }

        public INode AnnotationNode { get; set; }

        public INode Child { get; set; }

        public BtStatus Execute()
        {
            AnnotationNode.Execute();
            return Child.Execute();
        }

        public INode GetNodeToExecute()
        {
            return Child;
        }
    }

    public class Annotator<T> : INode<T>
    {
        public Annotator(INode<T> annotationNode, INode<T> child) : base()
        {
            AnnotationNode = annotationNode;
            Child = child;
        }

        public INode<T> AnnotationNode { get; set; }

        public INode<T> Child { get; set; }

        public BtStatus Execute(T blackboard)
        {
            AnnotationNode.Execute(blackboard);
            return Child.Execute(blackboard);
        }

        public INode<T> GetNodeToExecute(T blackboard)
        {
            return Child;
        }
    }
}