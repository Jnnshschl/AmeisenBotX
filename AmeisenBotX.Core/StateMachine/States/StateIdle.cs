using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateIdle : State
    {
        private AmeisenBotConfig Config { get; }
        private ObjectManager ObjectManager { get; }
        private HookManager HookManager { get; }

        private bool NeedToSetupHook { get; set; }

        public StateIdle(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, HookManager hookManager) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            HookManager = hookManager;
            NeedToSetupHook = true;
        }

        public override void Enter()
        {
            if (NeedToSetupHook)
                if (HookManager.SetupEndsceneHook())
                    NeedToSetupHook = false;
        }

        public override void Execute()
        {
            if (AmeisenBotStateMachine.XMemory.Process.HasExited)
                AmeisenBotStateMachine.SetState(AmeisenBotState.None);

            if (IsUnitToFollowThere())
                AmeisenBotStateMachine.SetState(AmeisenBotState.Following);
        }

        public bool IsUnitToFollowThere()
        {
            WowPlayer PlayerToFollow = null;

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

            return PlayerToFollow != null;
        }

        private WowPlayer SkipIfOutOfRange(WowPlayer PlayerToFollow)
        {
            if (PlayerToFollow != null)
            {
                double distance = PlayerToFollow.Position.GetDistance(ObjectManager.Player.Position);
                if (UnitIsOutOfRange(distance))
                    PlayerToFollow = null;
            }

            return PlayerToFollow;
        }

        private bool UnitIsOutOfRange(double distance)
           => (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance);

        public override void Exit()
        {
        }
    }
}