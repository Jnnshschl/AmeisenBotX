using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Logic.Idle.Actions
{
    public class LookAtNpcsIdleAction : IIdleAction
    {
        public LookAtNpcsIdleAction(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public AmeisenBotInterfaces Bot { get; }

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => 28 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 9 * 1000;

        public int MinDuration => 0;

        private IEnumerable<IWowUnit> NpcsNearMe { get; set; }

        private Random Rnd { get; }

        public bool Enter()
        {
            NpcsNearMe = Bot.Objects.WowObjects.OfType<IWowUnit>().Where(e => e.Position.GetDistance(Bot.Player.Position) < 12.0f);
            return NpcsNearMe.Any();
        }

        public void Execute()
        {
            IWowUnit randomPartymember = NpcsNearMe.ElementAt(Rnd.Next(0, NpcsNearMe.Count()));

            if (randomPartymember != null)
            {
                Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, BotMath.CalculatePositionAround(randomPartymember.Position, 0.0f, (float)Rnd.NextDouble() * (MathF.PI * 2), (float)Rnd.NextDouble()), true);
            }
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}Look at NPCs";
        }
    }
}