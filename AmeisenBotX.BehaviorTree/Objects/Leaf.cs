﻿using AmeisenBotX.BehaviorTree.Enums;
using System;

namespace AmeisenBotX.BehaviorTree.Objects
{
    public class Leaf : Node
    {
        public Leaf(Func<BtStatus> behaviorTreeAction) : base()
        {
            BehaviorTreeAction = behaviorTreeAction;
        }

        public Func<BtStatus> BehaviorTreeAction { get; set; }

        public override BtStatus Execute()
        {
            return BehaviorTreeAction();
        }

        internal override Node GetNodeToExecute()
        {
            return this;
        }
    }

    public class Leaf<T> : Node<T>
    {
        public Leaf(Func<T, BtStatus> behaviorTreeAction) : base()
        {
            BehaviorTreeAction = behaviorTreeAction;
        }

        public Func<T, BtStatus> BehaviorTreeAction { get; set; }

        public override BtStatus Execute(T blackboard)
        {
            return BehaviorTreeAction(blackboard);
        }

        internal override Node<T> GetNodeToExecute(T blackboard)
        {
            return this;
        }
    }
}