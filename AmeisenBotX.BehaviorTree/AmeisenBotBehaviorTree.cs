using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Interfaces;
using AmeisenBotX.BehaviorTree.Objects;
using System;

namespace AmeisenBotX.BehaviorTree
{
    public class AmeisenBotBehaviorTree<T> where T : IBlackboard
    {
        public AmeisenBotBehaviorTree(Node<T> node, T blackboard, bool resumeOngoingNodes = false)
        {
            RootNode = node;
            Blackboard = blackboard;
            ResumeOngoingNodes = resumeOngoingNodes;

            BlackboardUpdateEnabled = false;
        }

        public AmeisenBotBehaviorTree(Node<T> node, T blackboard, TimeSpan blackboardUpdateTime, bool resumeOngoingNodes = false)
        {
            RootNode = node;
            Blackboard = blackboard;
            BlackboardUpdateTime = blackboardUpdateTime;
            ResumeOngoingNodes = resumeOngoingNodes;

            BlackboardUpdateEnabled = true;
        }

        public AmeisenBotBehaviorTree(string name, Node<T> node, T blackboard, bool resumeOngoingNodes = false)
        {
            Name = name;
            RootNode = node;
            Blackboard = blackboard;
            ResumeOngoingNodes = resumeOngoingNodes;

            BlackboardUpdateEnabled = false;
        }

        public AmeisenBotBehaviorTree(string name, Node<T> node, T blackboard, TimeSpan blackboardUpdateTime, bool resumeOngoingNodes = false)
        {
            Name = name;
            RootNode = node;
            Blackboard = blackboard;
            BlackboardUpdateTime = blackboardUpdateTime;
            ResumeOngoingNodes = resumeOngoingNodes;

            BlackboardUpdateEnabled = true;
        }

        public T Blackboard { get; set; }

        public TimeSpan BlackboardUpdateTime { get; set; }

        public DateTime LastBlackBoardUpdate { get; set; }

        public string LastExecutedName { get; private set; }

        public string Name { get; }

        public Node<T> OngoingNode { get; private set; }

        public bool ResumeOngoingNodes { get; set; }

        public Node<T> RootNode { get; set; }

        private bool BlackboardUpdateEnabled { get; set; }

        public BehaviorTreeStatus Tick()
        {
            if (BlackboardUpdateEnabled && LastBlackBoardUpdate + BlackboardUpdateTime < DateTime.Now)
            {
                Blackboard.Update();
                LastBlackBoardUpdate = DateTime.Now;
            }

            if (ResumeOngoingNodes)
            {
                if (OngoingNode != null)
                {
                    BehaviorTreeStatus status = OngoingNode.Execute(Blackboard);

                    if (status == BehaviorTreeStatus.Failed || status == BehaviorTreeStatus.Success)
                    {
                        OngoingNode = null;
                    }

                    return status;
                }
                else
                {
                    BehaviorTreeStatus status = RootNode.Execute(Blackboard);

                    if (status == BehaviorTreeStatus.Ongoing)
                    {
                        OngoingNode = RootNode.GetNodeToExecute(Blackboard);
                    }

                    return status;
                }
            }
            else
            {
                Node<T> nodeToExecute = RootNode.GetNodeToExecute(Blackboard);
                LastExecutedName = nodeToExecute.Name;
                return nodeToExecute.Execute(Blackboard);
            }
        }
    }
}