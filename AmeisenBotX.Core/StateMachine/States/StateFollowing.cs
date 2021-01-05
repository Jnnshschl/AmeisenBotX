using AmeisenBotX.Core.Character.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    internal class StateFollowing : BasicState
    {
        public StateFollowing(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            LosCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));
            OffsetCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(20000));
            CastMountEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(3000));
        }

        public bool InLos { get; private set; }

        public Vector3 Offset { get; private set; }

        private TimegatedEvent CastMountEvent { get; }

        private TimegatedEvent LosCheckEvent { get; }

        private TimegatedEvent OffsetCheckEvent { get; }

        private WowPlayer PlayerToFollow => WowInterface.ObjectManager.GetWowObjectByGuid<WowPlayer>(PlayerToFollowGuid);

        private ulong PlayerToFollowGuid { get; set; }

        public override void Enter()
        {
            PlayerToFollowGuid = 0;

            // TODO: make this crap less redundant
            // check the specific character
            List<WowPlayer> wowPlayers = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>().ToList();
            if (wowPlayers.Count > 0)
            {
                if (Config.FollowSpecificCharacter)
                {
                    WowPlayer player = SkipIfOutOfRange(wowPlayers.FirstOrDefault(p => p.Name == Config.SpecificCharacterToFollow));

                    if (player != null)
                    {
                        PlayerToFollowGuid = player.Guid;
                    }
                }

                // check the group/raid leader
                if (PlayerToFollow == null && Config.FollowGroupLeader)
                {
                    WowPlayer player = SkipIfOutOfRange(wowPlayers.FirstOrDefault(p => p.Guid == WowInterface.ObjectManager.PartyleaderGuid));

                    if (player != null)
                    {
                        PlayerToFollowGuid = player.Guid;
                    }
                }

                // check the group members
                if (PlayerToFollow == null && Config.FollowGroupMembers)
                {
                    WowPlayer player = SkipIfOutOfRange(wowPlayers.FirstOrDefault(p => WowInterface.ObjectManager.PartymemberGuids.Contains(p.Guid)));

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

                Random rnd = new Random();

                Offset = new Vector3
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
            if (WowInterface.ObjectManager.Player.IsCasting
                || (PlayerToFollow != null
                && (PlayerToFollow.IsDead
                || PlayerToFollow.Health == 1)))
            {
                return;
            }

            if (PlayerToFollow == null)
            {
                // handle nearby portals, if our groupleader enters a portal, we follow
                WowGameobject nearestPortal = WowInterface.ObjectManager.WowObjects
                    .OfType<WowGameobject>()
                    .Where(e => e.DisplayId == (int)GameobjectDisplayId.UtgardeKeepDungeonPortalNormal
                             || e.DisplayId == (int)GameobjectDisplayId.UtgardeKeepDungeonPortalHeroic)
                    .FirstOrDefault(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 12.0);

                if (nearestPortal != null)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.DirectMove, BotUtils.MoveAhead(WowInterface.ObjectManager.Player.Position, nearestPortal.Position, 6f));
                }
                else
                {
                    StateMachine.SetState(BotState.Idle);
                }

                return;
            }

            Vector3 posToGoTo;

            if (Config.FollowPositionDynamic)
            {
                posToGoTo = PlayerToFollow.Position + Offset;
            }
            else
            {
                posToGoTo = PlayerToFollow.Position;
            }

            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(posToGoTo);

            if (distance < Config.MinFollowDistance)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (Config.UseMountsInParty
                && WowInterface.CharacterManager.Mounts?.Count > 0
                && PlayerToFollow != null
                && PlayerToFollow.IsMounted && !WowInterface.ObjectManager.Player.IsMounted)
            {
                if (CastMountEvent.Run())
                {
                    List<WowMount> filteredMounts;

                    if (Config.UseOnlySpecificMounts)
                    {
                        filteredMounts = WowInterface.CharacterManager.Mounts.Where(e => Config.Mounts.Split(",", StringSplitOptions.RemoveEmptyEntries).Contains(e.Name)).ToList();
                    }
                    else
                    {
                        filteredMounts = WowInterface.CharacterManager.Mounts;
                    }

                    if (filteredMounts != null && filteredMounts.Count >= 0)
                    {
                        WowMount mount = filteredMounts[new Random().Next(0, filteredMounts.Count)];
                        WowInterface.MovementEngine.StopMovement();
                        WowInterface.HookManager.LuaCallCompanion(mount.Index);
                    }
                }

                return;
            }

            // run down cliffs
            if (WowInterface.ObjectManager.Player.Position.GetDistance2D(posToGoTo) < 24.0)
            {
                double zDiff = posToGoTo.Z - WowInterface.ObjectManager.Player.Position.Z;

                if (LosCheckEvent.Run())
                {
                    if (WowInterface.HookManager.WowIsInLineOfSight(WowInterface.ObjectManager.Player.Position, posToGoTo, 2f))
                    {
                        InLos = true;
                    }
                    else
                    {
                        InLos = false;
                    }
                }

                if (zDiff < -16.0 && InLos) // target is below us and in line of sight, just run down
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

                double distance = pos.GetDistance(WowInterface.ObjectManager.Player.Position);

                if (playerToFollow.IsDead || UnitIsOutOfRange(distance))
                {
                    playerToFollow = null;
                }
            }

            return playerToFollow;
        }

        private bool UnitIsOutOfRange(double distance)
        {
            return (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance);
        }
    }
}