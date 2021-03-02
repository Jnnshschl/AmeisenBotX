using AmeisenBotX.Core.Data.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class LookAtGroupIdleAction : IIdleAction
    {
        public LookAtGroupIdleAction(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public int MaxCooldown => 11 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 5 * 1000;

        public int MinDuration => 0;

        public WowInterface WowInterface { get; }

        private IEnumerable<WowUnit> NearPartymembers { get; set; }

        private Random Rnd { get; }

        public bool Enter()
        {
            NearPartymembers = WowInterface.ObjectManager.Partymembers.Where(e => e.Guid != WowInterface.PlayerGuid && e.Position.GetDistance(WowInterface.Player.Position) < 16.0f);
            return NearPartymembers.Any();
        }

        public void Execute()
        {
            WowUnit randomPartymember = NearPartymembers.ElementAt(Rnd.Next(0, NearPartymembers.Count()));

            if (randomPartymember != null)
            {
                WowInterface.HookManager.WowFacePosition(WowInterface.Player, randomPartymember.Position * ((float)Rnd.NextDouble() / 10.0f));
            }
        }
    }
}