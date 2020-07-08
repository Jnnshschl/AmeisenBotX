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
        }

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
                StateMachine.SetState((int)BotState.Idle);
                return;
            }
        }

        public override void Execute()
        {
            Vector3 posToGoTo = default;

            // handle nearby portals, if our groupleader enters a portal, we follow
            WowGameobject nearestPortal = WowInterface.ObjectManager.WowObjects
                .OfType<WowGameobject>()
                .Where(e => e.DisplayId == (int)GameobjectDisplayId.DungeonPortalNormal || e.DisplayId == (int)GameobjectDisplayId.DungeonPortalHeroic)
                .FirstOrDefault(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.GhostPortalScanThreshold);

            bool moveIntoPortal = false;

            if (nearestPortal != null)
            {
                double distanceToPortal = PlayerToFollow.Position.GetDistance(nearestPortal.Position);

                if (distanceToPortal < 4.0)
                {
                    // move into portal, MoveAhead is used to go beyond the portals entry point to make sure enter it
                    posToGoTo = BotUtils.MoveAhead(nearestPortal.Position, BotMath.GetFacingAngle2D(WowInterface.ObjectManager.Player.Position, nearestPortal.Position), 6);
                    moveIntoPortal = true;
                }
            }

            // if no portal position was found, follow the player
            if (!moveIntoPortal)
            {
                if (PlayerToFollow == null)
                {
                    return;
                }

                posToGoTo = PlayerToFollow.Position;
            }

            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(posToGoTo);

            if (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance)
            {
                StateMachine.SetState((int)BotState.Idle);
                return;
            }

            // dont follow when casting, or player is dead/ghost
            if (WowInterface.ObjectManager.Player.IsCasting
                || PlayerToFollow.IsDead
                || PlayerToFollow.Health == 1)
            {
                return;
            }

            double zDiff = posToGoTo.Z - WowInterface.ObjectManager.Player.Position.Z;

            Vector3 playerPosZMod = WowInterface.ObjectManager.Player.Position;
            playerPosZMod.Z += 1f;

            Vector3 posToGoToZMod = posToGoTo;
            posToGoToZMod.Z += 1f;

            if ((distance < 4.0 && Math.Abs(zDiff) < 1.0) // we are close to the target and on the same z level
                || (distance < 32.0 && zDiff < 0.0 && WowInterface.HookManager.IsInLineOfSight(playerPosZMod, posToGoToZMod))) // target is below us and in line of sight, just run down
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
            WowInterface.MovementEngine.Reset();
            WowInterface.HookManager.StopClickToMoveIfActive();
        }

        private bool IsUnitOutOfRange(WowPlayer playerToFollow)
        {
            double distance = playerToFollow.Position.GetDistance(WowInterface.ObjectManager.Player.Position);
            return distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance;
        }
    }
}