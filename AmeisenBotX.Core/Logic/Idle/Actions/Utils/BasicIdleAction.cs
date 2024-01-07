using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Logic.Idle.Actions.Utils
{
    public abstract class BasicIdleAction : IIdleAction
    {
        public BasicIdleAction(List<IIdleAction> actions, string name = "")
        {
            Name = name;
            Actions = actions;
            SelectedAction = Actions.ElementAt(new Random().Next(0, Actions.Count));
        }

        public bool AutopilotOnly => Actions.Any(e => e.AutopilotOnly);

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => SelectedAction.MaxCooldown;

        public int MaxDuration => SelectedAction.MaxDuration;

        public int MinCooldown => SelectedAction.MinCooldown;

        public int MinDuration => SelectedAction.MinDuration;

        public string Name { get; }

        protected List<IIdleAction> Actions { get; }

        protected IIdleAction SelectedAction { get; set; }

        public abstract bool Enter();

        public virtual void Execute()
        {
            SelectedAction?.Execute();
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}{Name}";
        }
    }
}