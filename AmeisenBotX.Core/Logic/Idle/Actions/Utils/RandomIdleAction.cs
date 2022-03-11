using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Logic.Idle.Actions.Utils
{
    public class RandomIdleAction : BasicIdleAction
    {
        public RandomIdleAction(List<IIdleAction> actions)
            : base(actions)
        {
        }

        public RandomIdleAction(string name, List<IIdleAction> actions)
            : base(actions, name)
        {
        }

        private Random Rnd { get; } = new();

        public override bool Enter()
        {
            IEnumerable<IIdleAction> possibleActions = Actions.Where(e => e.Enter());
            int actionCount = possibleActions.Count();

            if (actionCount > 1)
            {
                SelectedAction = possibleActions.ElementAt(Rnd.Next(0, actionCount));
                return true;
            }
            else if (actionCount == 1)
            {
                SelectedAction = possibleActions.FirstOrDefault();
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}{Name}";
        }
    }
}