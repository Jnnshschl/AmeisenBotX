using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    internal class StateFollowing : BasicState
    {
        public StateFollowing(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
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

        private WowPlayer PlayerToFollow => WowInterface.Objects.GetWowObjectByGuid<WowPlayer>(PlayerToFollowGuid);

        private ulong PlayerToFollowGuid { get; set; }

        public override void Enter()
        {
            PlayerToFollowGuid = 0;

            // TODO: make this crap less redundant
            // check the specific character
            IEnumerable<WowPlayer> wowPlayers = WowInterface.Objects.WowObjects.OfType<WowPlayer>();

            if (wowPlayers.Any())
            {
                if (Config.FollowSpecificCharacter)
                {
                    WowPlayer player = SkipIfOutOfRange(wowPlayers.FirstOrDefault(p => WowInterface.Db.GetUnitName(p, out string name) && name == Config.SpecificCharacterToFollow));

                    if (player != null)
                    {
                        PlayerToFollowGuid = player.Guid;
                    }
                }

                // check the group/raid leader
                if (PlayerToFollow == null && Config.FollowGroupLeader)
                {
                    WowPlayer player = SkipIfOutOfRange(wowPlayers.FirstOrDefault(p => p.Guid == WowInterface.Objects.Partyleader.Guid));

                    if (player != null)
                    {
                        PlayerToFollowGuid = player.Guid;
                    }
                }

                // check the group members
                if (PlayerToFollow == null && Config.FollowGroupMembers)
                {
                    WowPlayer player = SkipIfOutOfRange(wowPlayers.FirstOrDefault(p => WowInterface.Objects.PartymemberGuids.Contains(p.Guid)));

                    if (player != null)
                    {
                        PlayerToFollowGuid = player.Guid;
                    }
                }
            }

            if (PlayerToFollow == null)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }
            else if (Config.FollowPositionDynamic && OffsetCheckEvent.Run())
            {
                // Vector3 rndPos = WowInterface.PathfindingHandler.GetRandomPointAround((int)WowInterface.ObjectManager.MapId, PlayerToFollow.Position, Config.MinFollowDistance * 0.2f);
                // Offset = PlayerToFollow.Position - rndPos;

                Random rnd = new();
                Offset = new()
                {
                    X = ((float)rnd.NextDouble() * (Config.MinFollowDistance * 2)) - Config.MinFollowDistance,
                    Y = ((float)rnd.NextDouble() * (Config.MinFollowDistance * 2)) - Config.MinFollowDistance,
                    Z = 0f
                };
            }
        }

        public override void Execute()
        {
            // dont follow when casting, or player is dead/ghost
            if (WowInterface.Player.IsCasting
                || (PlayerToFollow != null
                && (PlayerToFollow.IsDead
                || PlayerToFollow.Health == 1)))
            {
                return;
            }

            if (PlayerToFollow == null)
            {
                // handle nearby portals, if our groupleader enters a portal, we follow
                WowGameobject nearestPortal = WowInterface.Objects.WowObjects
                    .OfType<WowGameobject>()
                    .Where(e => e.DisplayId == (int)WowGameobjectDisplayId.UtgardeKeepDungeonPortalNormal
                             || e.DisplayId == (int)WowGameobjectDisplayId.UtgardeKeepDungeonPortalHeroic)
                    .FirstOrDefault(e => e.Position.GetDistance(WowInterface.Player.Position) < 12.0);

                if (nearestPortal != null)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.DirectMove, BotUtils.MoveAhead(WowInterface.Player.Position, nearestPortal.Position, 6f));
                }
                else
                {
                    StateMachine.SetState(BotState.Idle);
                }

                return;
            }

            Vector3 posToGoTo = Config.FollowPositionDynamic ? PlayerToFollow.Position + Offset : PlayerToFollow.Position;
            double distance = WowInterface.Player.DistanceTo(posToGoTo);

            if (distance < Config.MinFollowDistance)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (Config.UseMountsInParty
                && WowInterface.CharacterManager.Mounts?.Count > 0
                && PlayerToFollow != null
                && PlayerToFollow.IsMounted && !WowInterface.Player.IsMounted)
            {
                if (CastMountEvent.Run())
                {
                    IEnumerable<WowMount> filteredMounts;

                    if (Config.UseOnlySpecificMounts)
                    {
                        filteredMounts = WowInterface.CharacterManager.Mounts.Where(e => Config.Mounts.Split(",", StringSplitOptions.RemoveEmptyEntries).Contains(e.Name));
                    }
                    else
                    {
                        filteredMounts = WowInterface.CharacterManager.Mounts;
                    }

                    if (filteredMounts != null && filteredMounts.Any())
                    {
                        WowMount mount = filteredMounts.ElementAt(new Random().Next(0, filteredMounts.Count()));
                        WowInterface.MovementEngine.StopMovement();
                        WowInterface.NewWowInterface.LuaCallCompanion(mount.Index);
                    }
                }

                return;
            }

            // run down cliffs
            float zDiff = posToGoTo.Z - WowInterface.Player.Position.Z;

            // it goes more down than forward
            if (zDiff < -16.0)
            {
                if (LosCheckEvent.Run())
                {
                    if (WowInterface.NewWowInterface.WowIsInLineOfSight(WowInterface.Player.Position, posToGoTo, 2.0f))
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
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.DirectMove, posToGoTo);
                    return;
                }
            }

            WowInterface.MovementEngine.SetMovementAction(MovementAction.Follow, posToGoTo);
        }

        public override void Leave()
        {
        }

        private WowPlayer SkipIfOutOfRange(WowPlayer playerToFollow)
        {
            if (playerToFollow != null)
            {
                Vector3 pos = playerToFollow.Position;

                if (Config.FollowPositionDynamic)
                {
                    pos += StateMachine.GetState<StateFollowing>().Offset;
                }

                double distance = WowInterface.Player.DistanceTo(pos);

                if (playerToFollow.IsDead || playerToFollow.IsGhost || distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance)
                {
                    playerToFollow = null;
                }
            }

            return playerToFollow;
        }
    }
}