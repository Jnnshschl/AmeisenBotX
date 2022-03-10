using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Logic.Idle.Actions.Utils
{
    public class FcfsIdleAction : IIdleAction
    {
        public bool AutopilotOnly => Actions.Any(e => e.AutopilotOnly);

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => SelectedAction.MaxCooldown;

        public int MaxDuration => SelectedAction.MaxDuration;

        public int MinCooldown => SelectedAction.MinCooldown;

        public int MinDuration => SelectedAction.MinDuration;

        private IIdleAction SelectedAction { get; set; }

        private Random Rnd { get; }

        private List<IIdleAction> Actions { get; }

        public FcfsIdleAction(List<IIdleAction> actions)
        {
            Rnd = new();
            Actions = actions;
            SelectedAction = Actions.ElementAt(Rnd.Next(0, Actions.Count));
        }

        public bool Enter()
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

        public void Execute()
        {
            if (SelectedAction != null)
            {
                SelectedAction.Execute();
            }
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}Look at Group/Around";
        }
    }
}
