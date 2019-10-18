using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Pathfinding;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.States
{
    internal class StateFollowing : State
    {
        public StateFollowing(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, CharacterManager characterManager, IPathfindingHandler pathfindingHandler, IMovementEngine movementEngine) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            PathfindingHandler = pathfindingHandler;
            MovementEngine = movementEngine;
        }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private ObjectManager ObjectManager { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private WowPlayer PlayerToFollow { get; set; }

        private IMovementEngine MovementEngine { get; set; }

        public override void Enter()
        {
            PlayerToFollow = null;

            // TODO: make this crap less redundant
            // check the specific character
            List<WowPlayer> wowPlayers = ObjectManager.WowObjects.OfType<WowPlayer>().ToList();
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
                    PlayerToFollow = wowPlayers.FirstOrDefault(p => p.Guid == ObjectManager.PartyleaderGuid);
                    PlayerToFollow = SkipIfOutOfRange(PlayerToFollow);
                }

                // check the group members
                if (PlayerToFollow == null && Config.FollowGroupMembers)
                {
                    PlayerToFollow = wowPlayers.FirstOrDefault(p => ObjectManager.PartymemberGuids.Contains(p.Guid));
                    PlayerToFollow = SkipIfOutOfRange(PlayerToFollow);
                }
            }

            if (PlayerToFollow == null)
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
            }
        }

        public override void Execute()
        {
            double distance = PlayerToFollow.Position.GetDistance(ObjectManager.Player.Position);
            if (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance)
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
            }

            if (MovementEngine.CurrentPath == null)
            {
                BuildNewPath();
            }
            else
            {
                if (MovementEngine.GetNextStep(ObjectManager.Player.Position, out Vector3 positionToGoTo))
                {
                    CharacterManager.MoveToPosition(positionToGoTo);
                }
            }
        }

        public override void Exit()
        {
        }

        private void BuildNewPath()
        {
            List<Vector3> path = PathfindingHandler.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, PlayerToFollow.Position);
            MovementEngine.LoadPath(path);
            MovementEngine.PostProcessPath();
        }

        private WowPlayer SkipIfOutOfRange(WowPlayer playerToFollow)
        {
            if (playerToFollow != null)
            {
                double distance = playerToFollow.Position.GetDistance(ObjectManager.Player.Position);
                if (UnitIsOutOfRange(distance))
                {
                    playerToFollow = null;
                }
            }

            return playerToFollow;
        }

        private bool UnitIsOutOfRange(double distance)
           => (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance);
    }
}