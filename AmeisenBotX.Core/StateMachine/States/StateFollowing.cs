using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    class StateFollowing : State
    {
        private AmeisenBotConfig Config { get; }

        private ObjectManager ObjectManager { get; }
        private CharacterManager CharacterManager { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private WowPlayer PlayerToFollow { get; set; }

        private Queue<WowPosition> CurrentPath { get; set; }
        private WowPosition LastPosition { get; set; }
        private int TryCount { get; set; }

        public StateFollowing(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, CharacterManager characterManager) : base(stateMachine)
        {
            TryCount = 0;
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            CurrentPath = new Queue<WowPosition>();
            PathfindingHandler = new NavmeshServerClient(Config.NavmeshServerIp, Config.NameshServerPort);
        }

        public override void Enter()
        {
            PlayerToFollow = null;

            // TODO: make this crap less redundant
            // check the specific character
            if (Config.FollowSpecificCharacter)
            {
                PlayerToFollow = ObjectManager.WowObjects.OfType<WowPlayer>().FirstOrDefault(p => p.Name == Config.SpecificCharacterToFollow);

                double distance = PlayerToFollow.Position.GetDistance(ObjectManager.Player.Position);
                if (PlayerToFollow != null // if the Unit is out of range, skip it
                    && UnitIsOutOfRange(distance))
                    PlayerToFollow = null;

            }

            // check the group/raid leader
            if (PlayerToFollow == null && Config.FollowGroupLeader)
            {
                PlayerToFollow = ObjectManager.WowObjects.OfType<WowPlayer>().FirstOrDefault(p => p.Guid == ObjectManager.PartyleaderGuid);

                double distance = PlayerToFollow.Position.GetDistance(ObjectManager.Player.Position);
                if (PlayerToFollow != null // if the Unit is out of range, skip it
                    && UnitIsOutOfRange(distance))
                    PlayerToFollow = null;
            }

            // check the group members
            if (PlayerToFollow == null && Config.FollowGroupMembers)
            {
                PlayerToFollow = ObjectManager.WowObjects.OfType<WowPlayer>().FirstOrDefault(p => ObjectManager.PartymemberGuids.Contains(p.Guid));

                double distance = PlayerToFollow.Position.GetDistance(ObjectManager.Player.Position);
                if (PlayerToFollow != null // if the Unit is out of range, skip it
                    && UnitIsOutOfRange(distance))
                    PlayerToFollow = null;
            }

            if (PlayerToFollow == null)
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
        }

        private bool UnitIsOutOfRange(double distance)
           => (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance);

        public override void Execute()
        {
            double distance = PlayerToFollow.Position.GetDistance(ObjectManager.Player.Position);
            if (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance)
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);

            double distTraveled = LastPosition.GetDistance2D(ObjectManager.Player.Position);

            if (CurrentPath.Count == 0)
            {
                BuildNewPath();
            }
            else
            {
                WowPosition pos = CurrentPath.Peek();
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
                        TryCount++;

                    // if the thing is too far away, drop the whole Path
                    if (pos.Z - ObjectManager.Player.Position.Z > 2
                        && distance > 2)
                        CurrentPath.Clear();

                    // jump if the node is higher than us
                    if(pos.Z - ObjectManager.Player.Position.Z > 1.2 
                        && distance < 3)
                        CharacterManager.Jump();
                }

                if (distTraveled != 0
                    && distTraveled < 0.08)
                {
                    CharacterManager.SendKey(new IntPtr(0x26), 150, 250);
                    CharacterManager.Jump();
                }

                LastPosition = ObjectManager.Player.Position;
            }
        }

        private void BuildNewPath()
        {
            List<WowPosition> path = PathfindingHandler.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, PlayerToFollow.Position);
            if (path.Count > 0)
                foreach (WowPosition pos in path)
                    CurrentPath.Enqueue(pos);
        }

        public override void Exit()
        {

        }
    }
}
