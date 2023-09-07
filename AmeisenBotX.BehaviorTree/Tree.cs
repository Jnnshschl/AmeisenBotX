using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.BehaviorTree.Interfaces;
using System;

namespace AmeisenBotX.BehaviorTree
{
    public class BehaviorTree<T> where T : IBlackboard
    {
        public BehaviorTree(INode<T> node, T blackboard, bool resumeOngoingNodes = false)
        {
            RootNode = node;
            Blackboard = blackboard;
            ResumeOngoingNodes = resumeOngoingNodes;

            BlackboardUpdateEnabled = false;
        }

        public BehaviorTree(INode<T> node, T blackboard, TimeSpan blackboardUpdateTime, bool resumeOngoingNodes = false)
        {
            RootNode = node;
            Blackboard = blackboard;
            BlackboardUpdateTime = blackboardUpdateTime;
            ResumeOngoingNodes = resumeOngoingNodes;

            BlackboardUpdateEnabled = true;
        }

        public T Blackboard { get; set; }

        public bool BlackboardUpdateEnabled { get; set; }

        public TimeSpan BlackboardUpdateTime { get; set; }

        public DateTime LastBlackBoardUpdate { get; set; }

        public INode<T> OngoingNode { get; private set; }

        public bool ResumeOngoingNodes { get; set; }

        public INode<T> RootNode { get; set; }

        public BtStatus Tick()
        {
            if (BlackboardUpdateEnabled && LastBlackBoardUpdate + BlackboardUpdateTime < DateTime.Now)
            {
                Blackboard.Update();
                LastBlackBoardUpdate = DateTime.Now;
            }

            if (ResumeOngoingNodes)
            {
                BtStatus status;

                if (OngoingNode != null)
                {
                    status = OngoingNode.Execute(Blackboard);

                    if (status is BtStatus.Failed or BtStatus.Success)
                    {
                        OngoingNode = null;
                    }
                }
                else
                {
                    status = RootNode.Execute(Blackboard);

                    if (status is BtStatus.Ongoing)
                    {
                        OngoingNode = RootNode.GetNodeToExecute(Blackboard);
                    }
                }

                return status;
            }
            else
            {
                return RootNode.GetNodeToExecute(Blackboard).Execute(Blackboard);
            }
        }
    }

    public class Tree
    {
        public Tree(INode node, bool resumeOngoingNodes = false)
        {
            RootNode = node;
            ResumeOngoingNodes = resumeOngoingNodes;
        }

        public INode OngoingNode { get; private set; }

        public bool ResumeOngoingNodes { get; set; }

        public INode RootNode { get; set; }

        public BtStatus Tick()
        {
            if (ResumeOngoingNodes)
            {
                BtStatus status;

                if (OngoingNode != null)
                {
                    status = OngoingNode.Execute();

                    if (status is BtStatus.Failed or BtStatus.Success)
                    {
                        OngoingNode = null;
                    }
                }
                else
                {
                    status = RootNode.Execute();

                    if (status is BtStatus.Ongoing)
                    {
                        OngoingNode = RootNode.GetNodeToExecute();
                    }
                }

                return status;
            }
            else
            {
                return RootNode.GetNodeToExecute().Execute();
            }
        }
    }
}