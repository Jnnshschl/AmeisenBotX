using AmeisenBotX.Core.Data.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class RandomEmoteIdleAction : IIdleAction
    {
        public RandomEmoteIdleAction(AmeisenBotInterfaces bot)
        {
            Bot = bot;

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

        public AmeisenBotInterfaces Bot { get; }

        public List<string> Emotes { get; }

        public List<string> EmotesWithInteraction { get; }

        public int MaxCooldown => 220000;

        public int MaxDuration => 0;

        public int MinCooldown => 80000;

        public int MinDuration => 0;

        private Random Rnd { get; }

        public bool Enter()
        {
            return true;
        }

        public void Execute()
        {
            IEnumerable<WowPlayer> friendsAroundMe = Bot.GetNearFriends<WowPlayer>(Bot.Player.Position, 24.0f)
                .Where(e => e.Guid != Bot.Wow.PlayerGuid && Bot.Objects.PartymemberGuids.Contains(e.Guid));

            if (friendsAroundMe.Any() && Rnd.NextDouble() > 0.5)
            {
                WowPlayer player = friendsAroundMe.ElementAt(Rnd.Next(0, friendsAroundMe.Count()));

                if (Bot.Wow.TargetGuid != player.Guid)
                {
                    Bot.Wow.WowTargetGuid(player.Guid);
                    Bot.Wow.WowFacePosition(Bot.Player.BaseAddress, Bot.Player.Position, player.Position);
                }

                Bot.Wow.LuaSendChatMessage($"/{EmotesWithInteraction[Rnd.Next(0, EmotesWithInteraction.Count)]}");
            }
            else
            {
                Bot.Wow.LuaSendChatMessage($"/{Emotes[Rnd.Next(0, Emotes.Count)]}");
            }
        }
    }
}