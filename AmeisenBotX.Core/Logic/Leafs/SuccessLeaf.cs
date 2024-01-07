using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using System;

namespace AmeisenBotX.Core.Logic.Leafs
{
    public class SuccessLeaf(Action action = null) : INode
    {
        private Action Action { get; } = action;

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