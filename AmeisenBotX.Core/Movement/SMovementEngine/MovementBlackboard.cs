using AmeisenBotX.BehaviorTree.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
