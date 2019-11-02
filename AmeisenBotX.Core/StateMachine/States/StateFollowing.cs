using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Common.Enums;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Pathfinding;
using AmeisenBotX.Pathfinding.Objects;
using System;
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

        public Vector3 CurrentMovementTarget { get; private set; }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private ObjectManager ObjectManager { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private WowPlayer PlayerToFollow { get; set; }

        private IMovementEngine MovementEngine { get; set; }

        private int TryCount { get; set; }

        public override void Enter()
        {
            MovementEngine.CurrentPath.Clear();
            MovementEngine.Reset();
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

            TryCount = 0;
        }

        public override void Execute()
        {
            double distance = PlayerToFollow.Position.GetDistance(ObjectManager.Player.Position);
            if (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance)
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
            }

            if (MovementEngine.CurrentPath?.Count == 0 || CurrentMovementTarget.GetDistance2D(PlayerToFollow.Position) > Config.MinFollowDistance || TryCount == 5)
            {
                CurrentMovementTarget = PlayerToFollow.Position;
                BuildNewPath();
                TryCount = 0;
            }
            else
            {
                if (MovementEngine.GetNextStep(ObjectManager.Player.Position, ObjectManager.Player.Rotation, out Vector3 positionToGoTo, out bool needToJump))
                {
                    CharacterManager.MoveToPosition(positionToGoTo);

                    if (needToJump)
                    {
                        CharacterManager.Jump();

                        Random rnd = new Random();
                        BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_S), 300, 1000);

                        if (rnd.Next(10) >= 5)
                        {
                            BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_Q), 300, 600);
                        }
                        else
                        {
                            BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_E), 300, 600);
                        }

                        TryCount++;
                    }
                }
            }
        }

        public override void Exit()
        {
            MovementEngine.CurrentPath.Clear();
            MovementEngine.Reset();
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