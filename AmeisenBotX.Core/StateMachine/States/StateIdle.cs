using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateIdle : State
    {
        public StateIdle(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, IOffsetList offsetList, ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, EventHookManager eventHookManager, ICombatClass combatClass) : base(stateMachine)
        {
            Config = config;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            HookManager = hookManager;
            EventHookManager = eventHookManager;
            CharacterManager = characterManager;
            CombatClass = combatClass;
        }

        private AmeisenBotConfig Config { get; }

        private EventHookManager EventHookManager { get; }

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        private CharacterManager CharacterManager { get; }

        private IOffsetList OffsetList { get; }

        private ICombatClass CombatClass { get; }

        public override void Enter()
        {
            AmeisenBotStateMachine.XMemory.ReadString(OffsetList.PlayerName, Encoding.ASCII, out string playerName);
            AmeisenBotStateMachine.PlayerName = playerName;

            // first start
            if (!HookManager.IsWoWHooked)
            {
                HookManager.SetupEndsceneHook();
                HookManager.SetMaxFps((byte)Config.MaxFps);

                EventHookManager.Start();

                CharacterManager.UpdateAll();
            }
        }

        public override void Execute()
        {
            if (IsUnitToFollowThere())
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Following);
            }

            CombatClass?.OutOfCombatExecute();
        }

        public override void Exit()
        {
        }

        public bool IsUnitToFollowThere()
        {
            WowPlayer playerToFollow = null;

            // TODO: make this crap less redundant
            // check the specific character
            List<WowPlayer> wowPlayers = ObjectManager.WowObjects.OfType<WowPlayer>().ToList();
            if (wowPlayers.Count > 0)
            {
                if (Config.FollowSpecificCharacter)
                {
                    playerToFollow = wowPlayers.FirstOrDefault(p => p.Name == Config.SpecificCharacterToFollow);
                    playerToFollow = SkipIfOutOfRange(playerToFollow);
                }

                // check the group/raid leader
                if (playerToFollow == null && Config.FollowGroupLeader)
                {
                    playerToFollow = wowPlayers.FirstOrDefault(p => p.Guid == ObjectManager.PartyleaderGuid);
                    playerToFollow = SkipIfOutOfRange(playerToFollow);
                }

                // check the group members
                if (playerToFollow == null && Config.FollowGroupMembers)
                {
                    playerToFollow = wowPlayers.FirstOrDefault(p => ObjectManager.PartymemberGuids.Contains(p.Guid));
                    playerToFollow = SkipIfOutOfRange(playerToFollow);
                }
            }

            return playerToFollow != null;
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