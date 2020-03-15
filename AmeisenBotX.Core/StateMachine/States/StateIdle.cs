using AmeisenBotX.Core.Data.Objects.WowObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateIdle : BasicState
    {
        public StateIdle(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        private DateTime LastBagSlotCheck { get; set; }

        private DateTime LastLoot { get; set; }

        private DateTime LastEatCheck { get; set; }

        private DateTime LastRepairCheck { get; set; }

        public override void Enter()
        {
            // first start
            if (!WowInterface.HookManager.IsWoWHooked)
            {
                WowInterface.XMemory.ReadString(WowInterface.OffsetList.PlayerName, Encoding.ASCII, out string playerName);
                StateMachine.PlayerName = playerName;

                WowInterface.HookManager.SetupEndsceneHook();
                WowInterface.HookManager.SetMaxFps((byte)Config.MaxFps);
                WowInterface.HookManager.EnableClickToMove();

                WowInterface.EventHookManager.Start();

                WowInterface.CharacterManager.UpdateAll();
            }
        }

        public override void Execute()
        {
            if (Config.AutojoinBg)
            {
                CheckForBattlegroundInvites();
            }

            // do we need to repair our equipment
            if (DateTime.Now - LastLoot > TimeSpan.FromSeconds(1))
            {
                LastLoot = DateTime.Now;

                if (StateMachine.GetNearLootableUnits().Count() > 0)
                {
                    StateMachine.SetState(BotState.Looting);
                    return;
                }
            }

            // do we need to repair our equipment
            if (DateTime.Now - LastEatCheck > TimeSpan.FromSeconds(2))
            {
                LastEatCheck = DateTime.Now;

                if ((WowInterface.ObjectManager.Player.HealthPercentage < 75 && WowInterface.ObjectManager.Player.ManaPercentage < 75 && StateMachine.HasRefreshmentInBag())
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 75 && StateMachine.HasFoodInBag())
                    || (WowInterface.ObjectManager.Player.ManaPercentage < 75 && StateMachine.HasWaterInBag()))
                {
                    StateMachine.SetState(BotState.Eating);
                    return;
                }
            }

            // we are on a battleground
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.BattlegroundStatus, out int bgStatus)
                && bgStatus == 3)
            {
                StateMachine.SetState(BotState.Battleground);
                return;
            }

            // we are in a dungeon
            if (StateMachine.IsDungeonMap(WowInterface.ObjectManager.MapId))
            {
                StateMachine.SetState(BotState.Dungeon);
                return;
            }

            // do i need to follow someone
            if (IsUnitToFollowThere())
            {
                StateMachine.SetState(BotState.Following);
                return;
            }

            // do we need to repair our equipment
            if (DateTime.Now - LastRepairCheck > TimeSpan.FromSeconds(12))
            {
                LastRepairCheck = DateTime.Now;

                if (IsRepairNpcNear()
                    && WowInterface.CharacterManager.Equipment.Items.Any(e => ((double)e.Value.MaxDurability / (double)e.Value.Durability) < 0.2))
                {
                    StateMachine.SetState(BotState.Repairing);
                    return;
                }
            }

            // do we need to sell stuff
            if (DateTime.Now - LastBagSlotCheck > TimeSpan.FromSeconds(5)
                && WowInterface.CharacterManager.Inventory.Items.Any(e => e.Price > 0))
            {
                LastBagSlotCheck = DateTime.Now;

                if (IsVendorNpcNear()
                    && WowInterface.HookManager.GetFreeBagSlotCount() < 4)
                {
                    StateMachine.SetState(BotState.Selling);
                    return;
                }
            }

            // do buffing etc...
            WowInterface.CombatClass?.OutOfCombatExecute();
        }

        public override void Exit()
        {
        }

        public bool IsUnitToFollowThere()
        {
            WowPlayer playerToFollow = null;

            // TODO: make this crap less redundant
            // check the specific character
            List<WowPlayer> wowPlayers = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>().ToList();
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
                    playerToFollow = wowPlayers.FirstOrDefault(p => p.Guid == WowInterface.ObjectManager.PartyleaderGuid);
                    playerToFollow = SkipIfOutOfRange(playerToFollow);
                }

                // check the group members
                if (playerToFollow == null && Config.FollowGroupMembers)
                {
                    playerToFollow = wowPlayers.FirstOrDefault(p => WowInterface.ObjectManager.PartymemberGuids.Contains(p.Guid));
                    playerToFollow = SkipIfOutOfRange(playerToFollow);
                }
            }

            return playerToFollow != null;
        }

        internal bool IsRepairNpcNear()
            => WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
            .Any(e => e.GetType() != typeof(WowPlayer) && e.IsRepairVendor && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 50);

        internal bool IsVendorNpcNear()
            => WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
            .Any(e => e.GetType() != typeof(WowPlayer) && e.IsVendor && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 50);

        private void CheckForBattlegroundInvites()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.BattlegroundStatus, out int bgStatus)
                && bgStatus == 2)
            {
                WowInterface.HookManager.AcceptBattlegroundInvite();
            }
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