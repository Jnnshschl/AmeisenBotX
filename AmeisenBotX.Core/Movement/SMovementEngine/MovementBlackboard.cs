using AmeisenBotX.BehaviorTree.Interfaces;
using System;

namespace AmeisenBotX.Core.Movement.SMovementEngine
{
    public class MovementBlackboard : IBlackboard
    {
        public MovementBlackboard(Action updateAction)
        {
            UpdateAction = updateAction;
        }

        private Action UpdateAction { get; }

        public void Update()
        {
            UpdateAction();
        }
    }
}