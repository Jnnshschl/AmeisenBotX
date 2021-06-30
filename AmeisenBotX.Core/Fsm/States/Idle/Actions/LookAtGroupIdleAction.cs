﻿using AmeisenBotX.Core.Data.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class LookAtGroupIdleAction : IIdleAction
    {
        public LookAtGroupIdleAction(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public int MaxCooldown => 11 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 5 * 1000;

        public int MinDuration => 0;

        public AmeisenBotInterfaces Bot { get; }

        private IEnumerable<WowUnit> NearPartymembers { get; set; }

        private Random Rnd { get; }

        public bool Enter()
        {
            NearPartymembers = Bot.Objects.Partymembers.Where(e => e.Guid != Bot.Wow.PlayerGuid && e.Position.GetDistance(Bot.Player.Position) < 16.0f);
            return NearPartymembers.Any();
        }

        public void Execute()
        {
            WowUnit randomPartymember = NearPartymembers.ElementAt(Rnd.Next(0, NearPartymembers.Count()));

            if (randomPartymember != null)
            {
                Bot.Wow.WowFacePosition(Bot.Player.BaseAddress, Bot.Player.Position, randomPartymember.Position * ((float)Rnd.NextDouble() / 10.0f));
            }
        }
    }
}