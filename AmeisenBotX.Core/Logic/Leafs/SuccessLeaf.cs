using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using System;

namespace AmeisenBotX.Core.Logic.Leafs
{
    public class SuccessLeaf : INode
    {
        public SuccessLeaf(Action action = null)
        {
            Action = action;
        }

        private Action Action { get; }

        public BtStatus Execute()
        {
            Action?.Invoke();
            return BtStatus.Success;
        }

        public INode GetNodeToExecute()
        {
            return this;
        }
    }
}