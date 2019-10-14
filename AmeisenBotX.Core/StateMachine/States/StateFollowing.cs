using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.States
{
    internal class StateFollowing : State
    {
        public StateFollowing(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, CharacterManager characterManager, IPathfindingHandler pathfindingHandler) : base(stateMachine)
        {
            TryCount = 0;
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            PathfindingHandler = pathfindingHandler;
            CurrentPath = new Queue<Vector3>();
        }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private Queue<Vector3> CurrentPath { get; set; }

        private Vector3 LastPosition { get; set; }

        private ObjectManager ObjectManager { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private WowPlayer PlayerToFollow { get; set; }

        private int TryCount { get; set; }

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

            double distTraveled = LastPosition.GetDistance2D(ObjectManager.Player.Position);

            if (CurrentPath.Count == 0)
            {
                BuildNewPath();
            }
            else
            {
                Vector3 pos = CurrentPath.Peek();
                distance = pos.GetDistance2D(ObjectManager.Player.Position);
                if (distance <= 2
                    || distance > Config.MaxFollowDistance
                    || TryCount > 5)
                {
                    CurrentPath.Dequeue();
                    TryCount = 0;
                }
                else
                {
                    CharacterManager.MoveToPosition(pos);

                    if (distTraveled != 0 && distTraveled < 0.08)
                    {
                        TryCount++;
                    }

                    // if the thing is too far away, drop the whole Path
                    if (pos.Z - ObjectManager.Player.Position.Z > 2
                        && distance > 2)
                    {
                        CurrentPath.Clear();
                    }

                    // jump if the node is higher than us
                    if (pos.Z - ObjectManager.Player.Position.Z > 1.2
                        && distance < 3)
                    {
                        CharacterManager.Jump();
                    }
                }

                if (distTraveled != 0
                    && distTraveled < 0.08)
                {
                    // go forward
                    BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr(0x26), 500, 750);
                    CharacterManager.Jump();
                }

                LastPosition = ObjectManager.Player.Position;
            }
        }

        public override void Exit()
        {
        }

        private void BuildNewPath()
        {
            List<Vector3> path = PathfindingHandler.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, PlayerToFollow.Position);
            if (path.Count > 0)
            {
                foreach (Vector3 pos in path)
                {
                    CurrentPath.Enqueue(pos);
                }
            }
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