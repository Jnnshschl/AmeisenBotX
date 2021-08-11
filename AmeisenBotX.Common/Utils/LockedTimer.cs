using System;
using System.Threading;

namespace AmeisenBotX.Common.Utils
{
    public class LockedTimer
    {
        private int timerBusy;

        /// <summary>
        /// Utility class to create a ticker which can only run one tick at a time.
        /// </summary>
        /// <param name="tickMs">Milliseconds interval</param>
        /// <param name="actions">Callbacks</param>
        public LockedTimer(int tickMs, params Action[] actions)
        {
            Timer = new(Tick, null, 0, tickMs);

            foreach (Action a in actions)
            {
                OnTick += a;
            }
        }

        ~LockedTimer()
        {
            Timer.Dispose();
        }

        public event Action OnTick;

        private Timer Timer { get; }

        public void SetInterval(int tickMs)
        {
            Timer.Change(0, tickMs);
        }

        private void Tick(object o)
        {
            if (Interlocked.CompareExchange(ref timerBusy, 1, 0) == 0)
            {
                OnTick?.Invoke();
                timerBusy = 0;
            }
        }
    }
}