using AmeisenBotX.Core.Fsm.States.Idle.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle
{
    public class IdleActionManager
    {
        public IdleActionManager(IEnumerable<IIdleAction> idleActions)
        {
            IdleActions = idleActions;

            Rnd = new Random();
            LastActions = new List<KeyValuePair<DateTime, Type>>();
        }

        public TimeSpan Cooldown { get; private set; }

        public DateTime ExecuteUntil { get; private set; }

        public IEnumerable<IIdleAction> IdleActions { get; set; }

        public DateTime LastActionExecuted { get; private set; }

        public List<KeyValuePair<DateTime, Type>> LastActions { get; private set; }

        private IIdleAction CurrentAction { get; set; }

        private Random Rnd { get; }

        public void Reset()
        {
            ExecuteUntil = default;
            LastActionExecuted = DateTime.UtcNow;
        }

        public bool Tick(bool autopilotEnabled)
        {
            if (ExecuteUntil > DateTime.UtcNow && CurrentAction != null)
            {
                CurrentAction.Execute();
                return true;
            }

            if (LastActionExecuted + Cooldown <= DateTime.UtcNow)
            {
                IEnumerable<IIdleAction> filteredActions = IdleActions.Where
                (
                    e => (!e.AutopilotOnly || autopilotEnabled)
                    && !LastActions.Any(e => e.Value == e.GetType())
                    || LastActions.Where(x => x.Value == e.GetType() && (DateTime.UtcNow - x.Key).TotalMilliseconds > Rnd.Next(CurrentAction.MinCooldown, CurrentAction.MaxCooldown)).Any()
                );

                if (filteredActions.Any())
                {
                    CurrentAction = filteredActions.ElementAtOrDefault(Rnd.Next(0, filteredActions.Count()));

                    if (CurrentAction != null && CurrentAction.Enter())
                    {
                        LastActionExecuted = DateTime.UtcNow;
                        Cooldown = TimeSpan.FromMilliseconds(Rnd.Next(CurrentAction.MinCooldown, CurrentAction.MaxCooldown));
                        ExecuteUntil = LastActionExecuted + TimeSpan.FromMilliseconds(Rnd.Next(CurrentAction.MinDuration, CurrentAction.MaxDuration));

                        LastActions.Add(new KeyValuePair<DateTime, Type>(LastActionExecuted, CurrentAction.GetType()));

                        CurrentAction.Execute();
                        return true;
                    }
                }
            }

            return false;
        }
    }
}