using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    internal class StateFollowing : BasicState
    {
        public StateFollowing(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        private WowPlayer PlayerToFollow { get; set; }

        public override void Enter()
        {
            PlayerToFollow = null;

            // TODO: make this crap less redundant
            // check the specific character
            List<WowPlayer> wowPlayers = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>().ToList();
            if (wowPlayers.Count > 0)
            {
                if (Config.FollowSpecificCharacter)
                {
                    PlayerToFollow = wowPlayers.FirstOrDefault(p => p.Name == Config.SpecificCharacterToFollow);
                    PlayerToFollow = SkipIfOutOfRange(PlayerToFollow);
                }

                // check the group/raid leader
                if (PlayerToFollow == null && Config.FollowGroupLeader)
                {
                    PlayerToFollow = wowPlayers.FirstOrDefault(p => p.Guid == WowInterface.ObjectManager.PartyleaderGuid);
                    PlayerToFollow = SkipIfOutOfRange(PlayerToFollow);
                }

                // check the group members
                if (PlayerToFollow == null && Config.FollowGroupMembers)
                {
                    PlayerToFollow = wowPlayers.FirstOrDefault(p => WowInterface.ObjectManager.PartymemberGuids.Contains(p.Guid));
                    PlayerToFollow = SkipIfOutOfRange(PlayerToFollow);
                }
            }

            if (PlayerToFollow == null)
            {
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Execute()
        {
            WowInterface.ObjectManager.UpdateObject(PlayerToFollow);

            Vector3 posToGoTo = default;

            // handle nearby portals, if our groupleader enters a portal, we follow
            WowGameobject nearestPortal = WowInterface.ObjectManager.WowObjects
                .OfType<WowGameobject>()
                .Where(e => e.DisplayId == (int)GameobjectDisplayId.DungeonPortalNormal || e.DisplayId == (int)GameobjectDisplayId.DungeonPortalHeroic)
                .FirstOrDefault(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.GhostPortalScanThreshold);

            if (nearestPortal != null)
            {
                double distanceToPortal = PlayerToFollow.Position.GetDistance(nearestPortal.Position);

                if (distanceToPortal < 4.0)
                {
                    // move into portal, MoveAhead is used to go beyond the portals entry point to make sure enter it
                    posToGoTo = BotUtils.MoveAhead(BotMath.GetFacingAngle2D(WowInterface.ObjectManager.Player.Position, nearestPortal.Position), nearestPortal.Position, 6);
                }
            }

            // if no portal position was found, follow the player
            if (posToGoTo == default)
            {
                posToGoTo = PlayerToFollow.Position;
            }

            double distance = PlayerToFollow.Position.GetDistance(posToGoTo);
            if (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance)
            {
                StateMachine.SetState(BotState.Idle);
            }

            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            WowInterface.MovementEngine.SetState(MovementEngineState.Following, posToGoTo);
            WowInterface.MovementEngine.Execute();
        }

        public override void Exit()
        {
        }

        private WowPlayer SkipIfOutOfRange(WowPlayer playerToFollow)
        {
            if (playerToFollow != null)
            {
                double distance = playerToFollow.Position.GetDistance(WowInterface.ObjectManager.Player.Position);
                if (UnitIsOutOfRange(distance))
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