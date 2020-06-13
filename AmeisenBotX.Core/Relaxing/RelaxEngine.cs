using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;

namespace AmeisenBotX.Core.Relaxing
{
    public class RelaxEngine
    {
        public RelaxEngine(WowInterface wowInterface)
        {
            AmeisenLogger.Instance.Log("RelaxEngine", $"Initializing", LogLevel.Verbose);

            WowInterface = wowInterface;
        }

        public WowInterface WowInterface { get; }

        public void Execute()
        {
        }
    }
}