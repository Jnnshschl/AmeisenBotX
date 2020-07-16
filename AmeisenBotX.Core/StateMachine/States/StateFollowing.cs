using AmeisenBotX.Core.Character.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
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
            OffsetCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(60000));
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
                    PlayerToFollowGuid = wowPlayers.FirstOrDefault(p => p.Name == Config.SpecificCharacterToFollow && !IsUnitOutOfRange(p)).Guid;
                }

                // check the group/raid leader
                if (PlayerToFollow == null && Config.FollowGroupLeader)
                {
                    PlayerToFollowGuid = wowPlayers.FirstOrDefault(p => p.Guid == WowInterface.ObjectManager.PartyleaderGuid && !IsUnitOutOfRange(p)).Guid;
                }

                // check the group members
                if (PlayerToFollow == null && Config.FollowGroupMembers)
                {
                    PlayerToFollowGuid = wowPlayers.FirstOrDefault(p => WowInterface.ObjectManager.PartymemberGuids.Contains(p.Guid) && !IsUnitOutOfRange(p)).Guid;
                }
            }

            if (PlayerToFollow == null)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }
            else if (Config.FollowPositionDynamic && OffsetCheckEvent.Run())
            {
                Random rnd = new Random();
                Offset = new Vector3
                (
                    ((float)rnd.NextDouble() * 4f) + 2f,
                    ((float)rnd.NextDouble() * 4f) + 2f,
                    ((float)rnd.NextDouble() * 4f) + 2f
                );
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
                    .Where(e => e.DisplayId == (int)GameobjectDisplayId.DungeonPortalNormal || e.DisplayId == (int)GameobjectDisplayId.DungeonPortalHeroic)
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

            if (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            double zDiff = posToGoTo.Z - WowInterface.ObjectManager.Player.Position.Z;

            if (WowInterface.CharacterManager.Mounts.Count > 0 && PlayerToFollow != null && PlayerToFollow.IsMounted && !WowInterface.ObjectManager.Player.IsMounted)
            {
                if (CastMountEvent.Run())
                {
                    WowMount mount = WowInterface.CharacterManager.Mounts[new Random().Next(0, WowInterface.CharacterManager.Mounts.Count)];
                    WowInterface.MovementEngine.StopMovement();
                    WowInterface.HookManager.Mount(mount.Index);
                }

                return;
            }

            if (LosCheckEvent.Run())
            {
                if (WowInterface.HookManager.IsInLineOfSight(WowInterface.ObjectManager.Player.Position, posToGoTo, 2f))
                {
                    InLos = true;
                }
                else
                {
                    InLos = false;
                }
            }

            if (!Config.FollowPositionDynamic
                && ((distance < 4.0 && Math.Abs(zDiff) < 1.0) // we are close to the target and on the same z level
                || (distance < 32.0 && zDiff < 0.0 && InLos))) // target is below us and in line of sight, just run down
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.DirectMove, posToGoTo);
            }
            else
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Following, posToGoTo);
            }
        }

        public override void Exit()
        {
            if (!Config.FollowPositionDynamic)
            {
                WowInterface.MovementEngine.StopMovement();
            }
        }

        private bool IsUnitOutOfRange(WowPlayer playerToFollow)
        {
            double distance = playerToFollow.Position.GetDistance(WowInterface.ObjectManager.Player.Position);
            return distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance;
        }
    }
}