using AmeisenBotX.Core.Logic.Idle.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Logic.Idle
{
    public class IdleActionManager
    {
        public IdleActionManager(IEnumerable<IIdleAction> idleActions)
        {
            IdleActions = idleActions;

            Rnd = new();
            LastActions = new();
        }

        public TimeSpan Cooldown { get; private set; }

        public DateTime ExecuteUntil { get; private set; }

        public IEnumerable<IIdleAction> IdleActions { get; set; }

        public DateTime LastActionExecuted { get; private set; }

        public List<KeyValuePair<DateTime, IIdleAction>> LastActions { get; private set; }

        private IIdleAction CurrentAction { get; set; }

        private int MaxActionCooldown { get; } = 28 * 1000;

        private int MinActionCooldown { get; } = 12 * 1000;

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

            // cleanup old events
            LastActions.RemoveAll(e => e.Key < e.Value.Cooldown);

            if (LastActionExecuted + Cooldown <= DateTime.UtcNow)
            {
                IEnumerable<IIdleAction> filteredActions = IdleActions.Where
                (
                    e => (!e.AutopilotOnly || autopilotEnabled)
                      && DateTime.Now > e.Cooldown
                );

                if (filteredActions.Any())
                {
                    CurrentAction = filteredActions.ElementAtOrDefault(Rnd.Next(0, filteredActions.Count()));

                    if (CurrentAction != null && CurrentAction.Enter())
                    {
                        LastActionExecuted = DateTime.UtcNow;

                        Cooldown = TimeSpan.FromMilliseconds(Rnd.Next(MinActionCooldown, MaxActionCooldown));
                        CurrentAction.Cooldown = DateTime.Now + TimeSpan.FromMilliseconds(Rnd.Next(CurrentAction.MinCooldown, CurrentAction.MaxCooldown));
                        ExecuteUntil = LastActionExecuted + TimeSpan.FromMilliseconds(Rnd.Next(CurrentAction.MinDuration, CurrentAction.MaxDuration));

                        LastActions.Add(new(LastActionExecuted, CurrentAction));

                        CurrentAction.Execute();
                        return true;
                    }
                }
            }

            return false;
        }
    }
}