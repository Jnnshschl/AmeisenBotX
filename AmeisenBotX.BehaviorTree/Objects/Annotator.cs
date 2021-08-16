using AmeisenBotX.BehaviorTree.Enums;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Annotator : Node
    {
        public Annotator(Node annotationNode, Node child) : base()
        {
            AnnotationNode = annotationNode;
            Child = child;
        }

        public Node AnnotationNode { get; set; }

        public Node Child { get; set; }

        public override BehaviorTreeStatus Execute()
        {
            AnnotationNode.Execute();
            return Child.Execute();
        }

        internal override Node GetNodeToExecute()
        {
            return Child;
        }
    }

    public class Annotator<T> : Node<T>
    {
        public Annotator(Node<T> annotationNode, Node<T> child) : base()
        {
            AnnotationNode = annotationNode;
            Child = child;
        }

        public Node<T> AnnotationNode { get; set; }

        public Node<T> Child { get; set; }

        public override BehaviorTreeStatus Execute(T blackboard)
        {
            AnnotationNode.Execute(blackboard);
            return Child.Execute(blackboard);
        }

        internal override Node<T> GetNodeToExecute(T blackboard)
        {
            return Child;
        }
    }
}