using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Logic.Idle.Actions
{
    public class LookAtGroupmemberIdleAction : IIdleAction
    {
        public LookAtGroupmemberIdleAction(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public AmeisenBotInterfaces Bot { get; }

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => 18 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 5 * 1000;

        public int MinDuration => 0;

        private IEnumerable<IWowUnit> NearPartymembersFacingMe { get; set; }

        private Random Rnd { get; }

        public bool Enter()
        {
            NearPartymembersFacingMe = Bot.Objects.Partymembers.Where(e => e.Guid != Bot.Wow.PlayerGuid && e.Position.GetDistance(Bot.Player.Position) < 12.0f && BotMath.IsFacing(e.Position, e.Rotation, Bot.Player.Position));
            return NearPartymembersFacingMe.Any();
        }

        public void Execute()
        {
            IWowUnit randomPartymember = NearPartymembersFacingMe.ElementAt(Rnd.Next(0, NearPartymembersFacingMe.Count()));

            if (randomPartymember != null)
            {
                Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, BotMath.CalculatePositionAround(randomPartymember.Position, 0.0f, (float)Rnd.NextDouble() * (MathF.PI * 2), (float)Rnd.NextDouble()), true);
            }
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}Look at Group";
        }
    }
}