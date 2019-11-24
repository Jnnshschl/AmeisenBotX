using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateIdle : State
    {
        public StateIdle(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, IOffsetList offsetList, ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, EventHookManager eventHookManager, ICombatClass combatClass, Queue<ulong> unitLootList) : base(stateMachine)
        {
            Config = config;
            OffsetList = offsetList;
            ObjectManager = objectManager;
            HookManager = hookManager;
            EventHookManager = eventHookManager;
            CharacterManager = characterManager;
            CombatClass = combatClass;
            UnitLootList = unitLootList;
        }

        private CharacterManager CharacterManager { get; }

        private ICombatClass CombatClass { get; }

        private AmeisenBotConfig Config { get; }

        private EventHookManager EventHookManager { get; }

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        private IOffsetList OffsetList { get; }

        private DateTime LastRepairCheck { get; set; }

        private DateTime LastBagSlotCheck { get; set; }

        private Queue<ulong> UnitLootList { get; }

        public override void Enter()
        {
            // first start
            if (!HookManager.IsWoWHooked)
            {
                AmeisenBotStateMachine.XMemory.ReadString(OffsetList.PlayerName, Encoding.ASCII, out string playerName);
                AmeisenBotStateMachine.PlayerName = playerName;

                HookManager.SetupEndsceneHook();
                HookManager.SetMaxFps((byte)Config.MaxFps);

                EventHookManager.Start();

                CharacterManager.UpdateAll();
            }
        }

        public override void Execute()
        {
            // do i need to loot units
            if (UnitLootList.Count > 0)
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Looting);
                return;
            }

            // do i need to follow someone
            if (IsUnitToFollowThere())
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Following);
            }

            // do we need to repair our equipment
            if (DateTime.Now - LastRepairCheck > TimeSpan.FromSeconds(12))
            {
                if (IsRepairNpcNear()
                    && CharacterManager.Equipment.Equipment.Any(e => ((double)e.Value.MaxDurability / (double)e.Value.Durability) < 0.2))
                {
                    AmeisenBotStateMachine.SetState(AmeisenBotState.Repairing);
                }

                LastRepairCheck = DateTime.Now;
            }

            // do we need to sell stuff
            if (DateTime.Now - LastBagSlotCheck > TimeSpan.FromSeconds(5)
                && CharacterManager.Inventory.Items.Any(e => e.Price > 0))
            {
                if (IsVendorNpcNear()
                    && HookManager.GetFreeBagSlotCount() < 4)
                {
                    AmeisenBotStateMachine.SetState(AmeisenBotState.Selling);
                }

                LastBagSlotCheck = DateTime.Now;
            }

            // do buffing etc...
            CombatClass?.OutOfCombatExecute();
        }

        internal bool IsVendorNpcNear()
        {
            return ObjectManager.WowObjects.OfType<WowUnit>().Any(e => e.GetType() != typeof(WowPlayer) && e.IsVendor && e.Position.GetDistance(ObjectManager.Player.Position) < 50);
        }

        internal bool IsRepairNpcNear()
        {
            return ObjectManager.WowObjects.OfType<WowUnit>().Any(e => e.GetType() != typeof(WowPlayer) && e.IsRepairVendor && e.Position.GetDistance(ObjectManager.Player.Position) < 50);
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
