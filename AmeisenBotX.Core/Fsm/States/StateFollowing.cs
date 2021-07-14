using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    internal class StateFollowing : BasicState
    {
        public StateFollowing(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
            LosCheckEvent = new(TimeSpan.FromMilliseconds(1000));
            OffsetCheckEvent = new(TimeSpan.FromMilliseconds(20000));
            CastMountEvent = new(TimeSpan.FromMilliseconds(3000));
        }

        public bool InLos { get; private set; }

        public Vector3 Offset { get; private set; }

        private TimegatedEvent CastMountEvent { get; }

        private TimegatedEvent LosCheckEvent { get; }

        private TimegatedEvent OffsetCheckEvent { get; }

        private IWowPlayer PlayerToFollow => Bot.GetWowObjectByGuid<IWowPlayer>(PlayerToFollowGuid);

        private ulong PlayerToFollowGuid { get; set; }

        public override void Enter()
        {
            if (IsUnitToFollowThere(out IWowUnit player))
            {
                PlayerToFollowGuid = player.Guid;
            }
            else
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (Config.FollowPositionDynamic && OffsetCheckEvent.Run())
            {
                // Vector3 rndPos = Bot.PathfindingHandler.GetRandomPointAround((int)Bot.ObjectManager.MapId, PlayerToFollow.Position, Config.MinFollowDistance * 0.2f);
                // Offset = PlayerToFollow.Position - rndPos;

                Random rnd = new();
                Offset = new()
                {
                    X = ((float)rnd.NextDouble() * (Config.MinFollowDistance * 2)) - Config.MinFollowDistance,
                    Y = ((float)rnd.NextDouble() * (Config.MinFollowDistance * 2)) - Config.MinFollowDistance,
                    Z = 0.0f
                };
            }
        }

        public override void Execute()
        {
            // dont follow when we are casting something
            if (Bot.Player.IsCasting)
            {
                return;
            }

            IWowPlayer playerToFollow = PlayerToFollow;

            // our player is gone, try to follow into portals
            if (playerToFollow == null)
            {
                IWowGameobject nearestPortal = Bot.Objects.WowObjects
                    .OfType<IWowGameobject>()
                    .Where(e => e.DisplayId == (int)WowGameobjectDisplayId.UtgardeKeepDungeonPortalNormal
                             || e.DisplayId == (int)WowGameobjectDisplayId.UtgardeKeepDungeonPortalHeroic)
                    .FirstOrDefault(e => e.Position.GetDistance(Bot.Player.Position) < 12.0f);

                if (nearestPortal != null)
                {
                    Bot.Movement.SetMovementAction(MovementAction.DirectMove, BotUtils.MoveAhead(Bot.Player.Position, nearestPortal.Position, 6.0f));
                }
                else
                {
                    StateMachine.SetState(BotState.Idle);
                }

                return;
            }

            Vector3 posToGoTo = Config.FollowPositionDynamic ? playerToFollow.Position + Offset : playerToFollow.Position;
            float distance = Bot.Player.DistanceTo(posToGoTo);

            if (distance < Config.MinFollowDistance)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (Config.UseMountsInParty
                && Bot.Character.Mounts?.Count > 0
                && playerToFollow.IsMounted
                && !Bot.Player.IsMounted)
            {
                if (CastMountEvent.Run())
                {
                    IEnumerable<WowMount> filteredMounts = Bot.Character.Mounts;

                    if (Config.UseOnlySpecificMounts)
                    {
                        filteredMounts = filteredMounts.Where(e => Config.Mounts.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(e.Name));
                    }

                    if (filteredMounts != null && filteredMounts.Any())
                    {
                        WowMount mount = filteredMounts.ElementAt(new Random().Next(0, filteredMounts.Count()));
                        Bot.Movement.StopMovement();
                        Bot.Wow.LuaCallCompanion(mount.Index);
                    }
                }

                return;
            }

            // run down cliffs
            float zDiff = posToGoTo.Z - Bot.Player.Position.Z;

            // it goes more down than forward
            if (zDiff < -16.0f)
            {
                if (LosCheckEvent.Run())
                {
                    if (Bot.Wow.WowIsInLineOfSight(Bot.Player.Position, posToGoTo, 2.0f))
                    {
                        InLos = true;
                    }
                    else
                    {
                        InLos = false;
                    }
                }

                if (InLos) // target is below us and in line of sight, just run down
                {
                    Bot.Movement.SetMovementAction(MovementAction.DirectMove, posToGoTo);
                    return;
                }
            }

            Bot.Movement.SetMovementAction(MovementAction.Follow, posToGoTo);
        }

        public bool IsUnitToFollowThere(out IWowUnit playerToFollow, bool ignoreRange = false)
        {
            IEnumerable<IWowPlayer> wowPlayers = Bot.Objects.WowObjects.OfType<IWowPlayer>().Where(e => !e.IsDead);

            if (wowPlayers.Any())
            {
                IWowUnit[] playersToTry =
                {
                    Config.FollowSpecificCharacter ? wowPlayers.FirstOrDefault(p => Bot.Db.GetUnitName(p, out string name) && name.Equals(Config.SpecificCharacterToFollow, StringComparison.OrdinalIgnoreCase)) : null,
                    Config.FollowGroupLeader ? Bot.Objects.Partyleader : null,
                    Config.FollowGroupMembers ? Bot.Objects.Partymembers.FirstOrDefault() : null
                };

                for (int i = 0; i < playersToTry.Length; ++i)
                {
                    if (playersToTry[i] != null && (ignoreRange || ShouldIFollowPlayer(playersToTry[i])))
                    {
                        playerToFollow = playersToTry[i];
                        return true;
                    }
                }
            }

            playerToFollow = null;
            return false;
        }

        public override void Leave()
        {
            PlayerToFollowGuid = 0L;

            Bot.Movement.StopMovement();

            // Random rnd = new();
            // Bot.Wow.WowFacePosition(Bot.Player.BaseAddress, Bot.Player.Position, Bot.Wow.ObjectProvider.CenterPartyPosition + ((float)rnd.NextDouble() - 0.5f) * (MathF.PI / 4.0f));
        }

        private bool ShouldIFollowPlayer(IWowUnit playerToFollow)
        {
            if (playerToFollow != null)
            {
                Vector3 pos = playerToFollow.Position;

                if (Config.FollowPositionDynamic)
                {
                    pos += StateMachine.GetState<StateFollowing>().Offset;
                }

                double distance = pos.GetDistance(Bot.Player.Position);

                if (distance > Config.MinFollowDistance && distance < Config.MaxFollowDistance)
                {
                    return true;
                }
            }

            return false;
        }
    }
}