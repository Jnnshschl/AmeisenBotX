using AmeisenBotX.Core.Data.Objects.WowObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.States.Idle.Actions
{
    public class RandomEmoteIdleAction : IIdleAction
    {
        public RandomEmoteIdleAction()
        {
            Emotes = new List<string>()
            {
                "flex",
                "train",
                "joke",
                "laugh",
                "dance",
                "sit",
            };

            EmotesWithInteraction = new List<string>()
            {
                "hi",
                "wink",
                "salute",
                "fart",
                "flex",
                "laugh",
                "rude",
                "roar",
                "applaud",
            };

            Rnd = new Random();
        }

        public bool AutopilotOnly => false;

        public List<string> Emotes { get; }

        public List<string> EmotesWithInteraction { get; }

        public int MaxCooldown => 6 * 60 * 1000;

        public int MaxDuration => 0;

        public int MinCooldown => 3 * 60 * 1000;

        public int MinDuration => 0;

        private Random Rnd { get; }

        public bool Enter()
        {
            return true;
        }

        public void Execute()
        {
            IEnumerable<WowPlayer> friendsAroundMe = WowInterface.I.ObjectManager.GetNearFriends<WowPlayer>(WowInterface.I.ObjectManager.Player.Position, 24.0)
                .Where(e => e.Guid != WowInterface.I.ObjectManager.PlayerGuid && WowInterface.I.ObjectManager.PartymemberGuids.Contains(e.Guid));

            if (friendsAroundMe.Any() && Rnd.NextDouble() > 0.5)
            {
                WowPlayer player = friendsAroundMe.ElementAt(Rnd.Next(0, friendsAroundMe.Count()));

                if (WowInterface.I.ObjectManager.TargetGuid != player.Guid)
                {
                    WowInterface.I.HookManager.WowTargetGuid(player.Guid);
                    WowInterface.I.HookManager.WowFacePosition(WowInterface.I.ObjectManager.Player, player.Position);
                }

                WowInterface.I.HookManager.LuaSendChatMessage($"/{EmotesWithInteraction[Rnd.Next(0, EmotesWithInteraction.Count)]}");
            }
            else
            {
                WowInterface.I.HookManager.LuaSendChatMessage($"/{Emotes[Rnd.Next(0, Emotes.Count)]}");
            }
        }
    }
}