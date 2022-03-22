using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Movement.Providers.Basic
{
    public class FollowMovementProvider : IMovementProvider
    {
        public FollowMovementProvider(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            Bot = bot;
            Config = config;

            Random = new();
            OffsetCheckEvent = new(TimeSpan.FromMilliseconds(30000));
        }

        private AmeisenBotInterfaces Bot { get; }

        private AmeisenBotConfig Config { get; }

        private Vector3 FollowOffset { get; set; }

        private TimegatedEvent OffsetCheckEvent { get; }

        private Random Random { get; }

        public bool Get(out Vector3 position, out MovementAction type)
        {
            if (!Bot.Player.IsDead
                && !Bot.Player.IsInCombat
                && !Config.Autopilot
                && !Bot.Player.IsGhost)
            {
                if (IsUnitToFollowThere(out IWowUnit player))
                {
                    Vector3 pos = Config.FollowPositionDynamic ? player.Position + FollowOffset : player.Position;
                    float distance = Bot.Player.DistanceTo(pos);

                    if (distance > Config.MinFollowDistance && distance <= Config.MaxFollowDistance)
                    {
                        if (Config.FollowPositionDynamic && OffsetCheckEvent.Run())
                        {
                            float factor = Bot.Player.IsOutdoors ? 2.0f : 1.0f;

                            FollowOffset = new()
                            {
                                X = ((float)Random.NextDouble() * ((float)Config.MinFollowDistance * factor) - ((float)Config.MinFollowDistance * (0.5f * factor))) * 0.7071f,
                                Y = ((float)Random.NextDouble() * ((float)Config.MinFollowDistance * factor) - ((float)Config.MinFollowDistance * (0.5f * factor))) * 0.7071f,
                                Z = 0.0f
                            };
                        }

                        type = MovementAction.Move;
                        position = pos;
                        return true;
                    }
                }
            }

            type = MovementAction.None;
            position = Vector3.Zero;
            return false;
        }

        private bool IsUnitToFollowThere(out IWowUnit playerToFollow, bool ignoreRange = false)
        {
            IEnumerable<IWowPlayer> wowPlayers = Bot.Objects.All.OfType<IWowPlayer>().Where(e => !e.IsDead);

            if (wowPlayers.Any())
            {
                IWowUnit[] playersToTry =
                {
                    Config.FollowSpecificCharacter ? wowPlayers.FirstOrDefault(p => Bot.Db.GetUnitName(p, out string name) && name.Equals(Config.SpecificCharacterToFollow, StringComparison.OrdinalIgnoreCase)) : null,
                    Config.FollowGroupLeader ? Bot.Objects.Partyleader : null,
                    Config.FollowGroupMembers ? Bot.Objects.Partymembers.FirstOrDefault() : null
                };

                foreach (IWowUnit unit in playersToTry)
                {
                    if (unit == null || (!ignoreRange && !ShouldIFollowPlayer(unit)))
                    {
                        continue;
                    }

                    playerToFollow = unit;
                    return true;
                }
            }

            playerToFollow = null;
            return false;
        }

        private bool ShouldIFollowPlayer(IWowUnit playerToFollow)
        {
            if (playerToFollow == null)
            {
                return false;
            }

            Vector3 pos = Config.FollowPositionDynamic ? playerToFollow.Position + FollowOffset : playerToFollow.Position;
            double distance = Bot.Player.DistanceTo(pos);

            return distance > Config.MinFollowDistance && distance < Config.MaxFollowDistance;
        }
    }
}