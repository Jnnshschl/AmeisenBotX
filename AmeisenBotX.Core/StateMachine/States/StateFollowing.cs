using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
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
            WowInterface.ObjectManager.UpdateObject(PlayerToFollow.Type, PlayerToFollow.BaseAddress);
            double distance = PlayerToFollow.Position.GetDistance(WowInterface.ObjectManager.Player.Position);
            if (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance)
            {
                StateMachine.SetState(BotState.Idle);
            }

            if (WowInterface.ObjectManager.Player.CurrentlyCastingSpellId > 0 || WowInterface.ObjectManager.Player.CurrentlyChannelingSpellId > 0)
            {
                return;
            }

            WowInterface.MovementEngine.SetState(MovementEngineState.Following, PlayerToFollow.Position);
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
           => (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance);
    }
}