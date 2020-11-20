using AmeisenBotX.Core.Data.Objects.WowObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.States.Idle.Actions
{
    public class LookAtGroupIdleAction : IIdleAction
    {
        public LookAtGroupIdleAction()
        {
            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public int MaxCooldown => 11 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 5 * 1000;

        public int MinDuration => 0;

        private IEnumerable<WowUnit> NearPartymembers { get; set; }

        private Random Rnd { get; }

        public bool Enter()
        {
            NearPartymembers = WowInterface.I.ObjectManager.Partymembers.Where(e => e.Guid != WowInterface.I.ObjectManager.PlayerGuid && e.Position.GetDistance(WowInterface.I.ObjectManager.Player.Position) < 16.0f);
            return NearPartymembers.Any();
        }

        public void Execute()
        {
            WowUnit randomPartymember = NearPartymembers.ElementAt(Rnd.Next(0, NearPartymembers.Count()));

            if (randomPartymember != null)
            {
                WowInterface.I.HookManager.WowFacePosition(WowInterface.I.ObjectManager.Player, randomPartymember.Position);
            }
        }
    }
}