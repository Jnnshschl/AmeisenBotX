using System.Collections.Generic;

namespace AmeisenBotX.Core.Logic.Idle.Actions.Utils
{
    public class FcfsIdleAction : BasicIdleAction
    {
        public FcfsIdleAction(List<IIdleAction> actions)
            : base(actions)
        {
        }

        public FcfsIdleAction(string name, List<IIdleAction> actions)
            : base(actions, name)
        {
        }

        public override bool Enter()
        {
            foreach (IIdleAction action in Actions)
            {
                if (action.Enter())
                {
                    SelectedAction = action;
                    return true;
                }
            }

            return false;
        }
    }
}