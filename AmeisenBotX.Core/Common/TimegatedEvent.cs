using System;

namespace AmeisenBotX.Core.Common
{
    public class TimegatedEvent
    {
        public TimegatedEvent(TimeSpan timegate)
        {
            Timegate = timegate;
        }

        public TimegatedEvent(TimeSpan timegate, Action function)
        {
            Timegate = timegate;
            Function = function;
        }

        public DateTime LastExecution { get; set; }

        public TimeSpan Timegate { get; set; }

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

    public class TimegatedEvent<T>
    {
        public TimegatedEvent(TimeSpan timegate)
        {
            Timegate = timegate;
        }

        public TimegatedEvent(TimeSpan timegate, Func<T> function)
        {
            Timegate = timegate;
            Function = function;
        }

        public DateTime LastExecution { get; set; }

        public TimeSpan Timegate { get; set; }

        private Func<T> Function { get; set; }

        /// <summary>
        /// Executes the provided function if the last execution is longer than the specified TimeSpan
        /// </summary>
        /// <param name="value">The return value of your supplied function</param>
        /// <returns>True when the function is executed, false if not</returns>
        public bool Run(out T value, Func<T> function)
        {
            return CallFunction(out value, function);
        }

        /// <summary>
        /// Executes the provided function if the last execution is longer than the specified TimeSpan
        /// </summary>
        /// <param name="value">The return value of your supplied function</param>
        /// <returns>True when the function is executed, false if not</returns>
        public bool Run(out T value)
        {
            return CallFunction(out value, Function);
        }

        private bool CallFunction(out T value, Func<T> function)
        {
            if (DateTime.Now - LastExecution > Timegate)
            {
                LastExecution = DateTime.Now;
                value = function != null ? function() : default;
                return true;
            }

            value = default;
            return false;
        }
    }
}