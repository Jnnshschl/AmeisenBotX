using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateIdle : State
    {
        private AmeisenBotConfig Config { get; }

        private ObjectManager ObjectManager { get; }

        public StateIdle(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
        }

        public override void Enter()
        {

        }

        public override void Execute()
        {
            if (AmeisenBotStateMachine.XMemory.Process.HasExited)
                AmeisenBotStateMachine.SetState(AmeisenBotState.None);

            if(IsUnitToFollowThere())
                AmeisenBotStateMachine.SetState(AmeisenBotState.Following);
        }

        public bool IsUnitToFollowThere()
        {
            WowPlayer PlayerToFollow = null;

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

            return PlayerToFollow != null;
        }

        private bool UnitIsOutOfRange(double distance)
           => (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance);

        public override void Exit()
        {

        }
    }
}
