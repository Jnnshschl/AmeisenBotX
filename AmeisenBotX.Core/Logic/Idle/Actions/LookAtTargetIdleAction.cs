using AmeisenBotX.Common.Math;
using System;

namespace AmeisenBotX.Core.Logic.Idle.Actions
{
    public class LookAtTargetIdleAction : IIdleAction
    {
        public LookAtTargetIdleAction(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public AmeisenBotInterfaces Bot { get; }

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => 38 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 12 * 1000;

        public int MinDuration => 0;

        private Random Rnd { get; }

        public bool Enter()
        {
            return Bot.Target != null;
        }

        public void Execute()
        {
            if (Bot.Target != null)
            {
                Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, BotMath.CalculatePositionAround(Bot.Target.Position, 0.0f, (float)Rnd.NextDouble() * (MathF.PI * 2), (float)Rnd.NextDouble()), true);
            }
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}Look at Target";
        }
    }
}