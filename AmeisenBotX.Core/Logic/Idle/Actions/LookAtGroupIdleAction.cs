using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Logic.Idle.Actions
{
    public class LookAtGroupIdleAction : IIdleAction
    {
        public LookAtGroupIdleAction(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public AmeisenBotInterfaces Bot { get; }

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => 8 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 2 * 1000;

        public int MinDuration => 0;

        private IEnumerable<IWowUnit> NearPartymembers { get; set; }

        private Random Rnd { get; }

        public bool Enter()
        {
            return Bot.Objects.CenterPartyPosition != Vector3.Zero
                && Bot.Objects.Partymembers.Any(e => e.Guid != Bot.Wow.PlayerGuid && e.Position.GetDistance(Bot.Player.Position) < 12.0f);
        }

        public void Execute()
        {
            Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, BotMath.CalculatePositionAround(Bot.Objects.CenterPartyPosition, 0.0f, (float)Rnd.NextDouble() * (MathF.PI * 2), (float)Rnd.NextDouble()), true);
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}Look at Group";
        }
    }
}