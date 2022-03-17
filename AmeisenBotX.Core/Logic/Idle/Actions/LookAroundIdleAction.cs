using System;

namespace AmeisenBotX.Core.Logic.Idle.Actions
{
    public class LookAroundIdleAction : IIdleAction
    {
        public LookAroundIdleAction(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public AmeisenBotInterfaces Bot { get; }

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => 49 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 2 * 1000;

        public int MinDuration => 0;

        private Random Rnd { get; }

        public bool Enter()
        {
            return true;
        }

        public void Execute()
        {
            float modificationFactor = ((float)Rnd.NextDouble() - 0.5f) / ((float)Rnd.NextDouble() * 1.2f);
            Bot.Wow.SetFacing(Bot.Player.BaseAddress, Bot.Player.Rotation + modificationFactor, true);
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}Look Around";
        }
    }
}