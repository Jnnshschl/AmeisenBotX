using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Interfaces;
using AmeisenBotX.BehaviorTree.Objects;
using System;

namespace AmeisenBotX.BehaviorTree
{
    public class AmeisenBotBehaviorTree
    {
        public AmeisenBotBehaviorTree(Node node, bool resumeOngoingNodes = false)
        {
            RootNode = node;
            ResumeOngoingNodes = resumeOngoingNodes;
        }

        public Node OngoingNode { get; private set; }

        public bool ResumeOngoingNodes { get; set; }

        public Node RootNode { get; set; }

        public BtStatus Tick()
        {
            if (ResumeOngoingNodes)
            {
                if (OngoingNode != null)
                {
                    BtStatus status = OngoingNode.Execute();

                    if (status == BtStatus.Failed || status == BtStatus.Success)
                    {
                        OngoingNode = null;
                    }

                    return status;
                }
                else
                {
                    BtStatus status = RootNode.Execute();

                    if (status == BtStatus.Ongoing)
                    {
                        OngoingNode = RootNode.GetNodeToExecute();
                    }

                    return status;
                }
            }
            else
            {
                Node nodeToExecute = RootNode.GetNodeToExecute();
                return nodeToExecute.Execute();
            }
        }
    }

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

        public T Blackboard { get; set; }

        public TimeSpan BlackboardUpdateTime { get; set; }

        public DateTime LastBlackBoardUpdate { get; set; }

        public Node<T> OngoingNode { get; private set; }

        public bool ResumeOngoingNodes { get; set; }

        public Node<T> RootNode { get; set; }

        private bool BlackboardUpdateEnabled { get; set; }

        public BtStatus Tick()
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
                    BtStatus status = OngoingNode.Execute(Blackboard);

                    if (status == BtStatus.Failed || status == BtStatus.Success)
                    {
                        OngoingNode = null;
                    }

                    return status;
                }
                else
                {
                    BtStatus status = RootNode.Execute(Blackboard);

                    if (status == BtStatus.Ongoing)
                    {
                        OngoingNode = RootNode.GetNodeToExecute(Blackboard);
                    }

                    return status;
                }
            }
            else
            {
                Node<T> nodeToExecute = RootNode.GetNodeToExecute(Blackboard);
                return nodeToExecute.Execute(Blackboard);
            }
        }
    }
}