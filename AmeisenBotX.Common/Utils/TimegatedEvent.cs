using System;

namespace AmeisenBotX.Common.Utils
{
    /// <summary>
    /// Utility class to limit the execution of actions. It is able to
    /// run code only when x seconds/minutes/... passed. Useful when
    /// you dont want high cost code to be spammed.
    /// </summary>
    public class TimegatedEvent
    {
        /// <summary>
        /// Create a new timegated event.
        /// </summary>
        /// <param name="timegate">Minimun time to pass until next execution</param>
        public TimegatedEvent(TimeSpan timegate)
        {
            Timegate = timegate;
        }

        /// <summary>
        /// Create a new timegated event.
        /// </summary>
        /// <param name="timegate">Minimun time to pass until next execution</param>
        /// <param name="function">Code to execute</param>
        public TimegatedEvent(TimeSpan timegate, Action function)
        {
            Timegate = timegate;
            Function = function;
        }

        /// <summary>
        /// Last time the code was executed.
        /// </summary>
        public DateTime LastExecution { get; set; }

        /// <summary>
        /// Whether code can be executed or not.
        /// </summary>
        public bool Ready => DateTime.Now - LastExecution > Timegate;

        /// <summary>
        /// Minimum time to pass between executions.
        /// </summary>
        public TimeSpan Timegate { get; set; }

        /// <summary>
        /// Code to execute.
        /// </summary>
        private Action Function { get; set; }

        /// <summary>
        /// Executes the provided function if the last execution is longer than the specified TimeSpan
        /// </summary>
        /// <returns>True when the function is executed, false if not</returns>
        public bool Run()
        {
            return Run(Function);
        }

        /// <summary>
        /// Executes the provided function if the last execution is longer than the specified TimeSpan
        /// </summary>
        /// <param name="function">Function to call</param>
        /// <returns>True when the function is executed, false if not</returns>
        public bool Run(Action function)
        {
            if (DateTime.Now - LastExecution > Timegate)
            {
                LastExecution = DateTime.Now;
                function?.Invoke();
                return true;
            }

            return false;
        }
    }
}