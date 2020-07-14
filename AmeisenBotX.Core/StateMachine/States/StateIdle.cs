using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateIdle : BasicState
    {
        public StateIdle(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            FirstStart = true;

            BagSlotCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(5000));
            EatCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(2000));
            LootCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(2000));
            RepairCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(5000));
            QuestgiverCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(2000));
            QuestgiverRightClickEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(3000));
        }

        public bool FirstStart { get; set; }

        public bool QuestgiverGossipOpen { get; set; }

        public int QuestgiverGossipOptionCount { get; set; }

        private TimegatedEvent BagSlotCheckEvent { get; }

        private TimegatedEvent EatCheckEvent { get; }

        private TimegatedEvent LootCheckEvent { get; }

        private TimegatedEvent QuestgiverCheckEvent { get; }

        private TimegatedEvent QuestgiverRightClickEvent { get; }

        private TimegatedEvent RepairCheckEvent { get; }

        public override void Enter()
        {
            if (WowInterface.WowProcess != null && !WowInterface.WowProcess.HasExited && FirstStart)
            {
                while (!WowInterface.ObjectManager.IsWorldLoaded)
                {
                    WowInterface.ObjectManager.RefreshIsWorldLoaded();
                    Task.Delay(100).Wait();
                }

                FirstStart = false;
                WowInterface.XMemory.ReadString(WowInterface.OffsetList.PlayerName, Encoding.ASCII, out string playerName);
                StateMachine.PlayerName = playerName;

                if (!WowInterface.EventHookManager.IsActive)
                {
                    WowInterface.EventHookManager.Start();
                }

                WowInterface.EventHookManager.Subscribe("GOSSIP_SHOW", OnGossipShow);
                WowInterface.EventHookManager.Subscribe("GOSSIP_CLOSED", OnGossipClosed);
            }

            WowInterface.CharacterManager.UpdateAll();
            WowInterface.HookManager.SetMaxFps((byte)Config.MaxFps);
            WowInterface.HookManager.EnableClickToMove();
        }

        public override void Execute()
        {
            // do we need to loot stuff
            if (LootCheckEvent.Run()
                && StateMachine.GetNearLootableUnits().Count() > 0)
            {
                StateMachine.SetState((int)BotState.Looting);
                return;
            }

            // do we need to eat something
            if (EatCheckEvent.Run()
                // Refreshment
                && ((WowInterface.ObjectManager.Player.HealthPercentage < Config.EatUntilPercent
                         && WowInterface.ObjectManager.Player.ManaPercentage < Config.DrinkUntilPercent
                         && WowInterface.CharacterManager.HasRefreshmentInBag())
                     // Food
                     || (WowInterface.ObjectManager.Player.HealthPercentage < Config.EatUntilPercent
                         && WowInterface.CharacterManager.HasFoodInBag())
                     // Water
                     || (WowInterface.ObjectManager.Player.MaxMana > 0
                         && WowInterface.ObjectManager.Player.ManaPercentage < Config.DrinkUntilPercent
                         && WowInterface.CharacterManager.HasWaterInBag())))
            {
                StateMachine.SetState((int)BotState.Eating);
                return;
            }

            // we are on a battleground
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.BattlegroundStatus, out int bgStatus)
                && bgStatus == 3
                && !Config.BattlegroundUsePartyMode)
            {
                StateMachine.SetState((int)BotState.Battleground);
                return;
            }

            // we are in a dungeon
            if (WowInterface.ObjectManager.MapId.IsDungeonMap()
                && !Config.DungeonUsePartyMode)
            {
                StateMachine.SetState((int)BotState.Dungeon);
                return;
            }

            // do i need to follow someone
            if (!Config.Autopilot && IsUnitToFollowThere(out _))
            {
                StateMachine.SetState((int)BotState.Following);
                return;
            }

            // do we need to repair our equipment
            if (Config.AutoRepair
                && RepairCheckEvent.Run()
                && IsRepairNpcNear())
            {
                WowInterface.CharacterManager.Equipment.Update();
                if (WowInterface.CharacterManager.Equipment.Items.Any(e => e.Value.MaxDurability > 0 && ((double)e.Value.Durability * (double)e.Value.MaxDurability) * 100.0 <= Config.ItemRepairThreshold))
                {
                    StateMachine.SetState((int)BotState.Repairing);
                    return;
                }
            }

            // do we need to sell stuff
            if (Config.AutoSell
                && BagSlotCheckEvent.Run()
                && IsVendorNpcNear()
                && WowInterface.CharacterManager.Inventory.FreeBagSlots < Config.BagSlotsToGoSell
                && WowInterface.CharacterManager.Inventory.Items.Where(e => !Config.ItemSellBlacklist.Contains(e.Name)
                       && ((Config.SellGrayItems && e.ItemQuality == ItemQuality.Poor)
                           || (Config.SellWhiteItems && e.ItemQuality == ItemQuality.Common)
                           || (Config.SellGreenItems && e.ItemQuality == ItemQuality.Uncommon)
                           || (Config.SellBlueItems && e.ItemQuality == ItemQuality.Rare)
                           || (Config.SellPurpleItems && e.ItemQuality == ItemQuality.Epic)))
                   .Any(e => e.Price > 0))
            {
                StateMachine.SetState((int)BotState.Selling);
                return;
            }

            // do i need to complete/get quests
            if (Config.AutoTalkToNearQuestgivers
                && QuestgiverCheckEvent.Run())
            {
                if (IsUnitToFollowThere(out WowPlayer wowPlayer, true)
                    && wowPlayer.TargetGuid != 0)
                {
                    WowUnit possibleQuestgiver = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(wowPlayer.TargetGuid);

                    if (possibleQuestgiver != null && (possibleQuestgiver.IsQuestgiver || possibleQuestgiver.IsGossip))
                    {
                        if (WowInterface.ObjectManager.Player.Position.GetDistance(possibleQuestgiver.Position) > 4.0)
                        {
                            WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, possibleQuestgiver.Position);
                            return;
                        }
                        else
                        {
                            if (!QuestgiverGossipOpen)
                            {
                                if (QuestgiverRightClickEvent.Run())
                                {
                                    WowInterface.HookManager.UnitOnRightClick(possibleQuestgiver);
                                }
                            }
                            else
                            {
                                if (possibleQuestgiver.IsQuestgiver)
                                {
                                    // complete/accept quests
                                    WowInterface.HookManager.AutoAcceptQuests();
                                }
                            }
                        }
                    }
                }
            }

            // do buffing etc...
            WowInterface.CombatClass?.OutOfCombatExecute();
        }

        public override void Exit()
        {
        }

        public bool IsUnitToFollowThere(out WowPlayer playerToFollow, bool ignoreRange = false)
        {
            playerToFollow = null;

            // TODO: make this crap less redundant
            // check the specific character
            List<WowPlayer> wowPlayers = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>().ToList();
            if (wowPlayers.Count > 0)
            {
                if (Config.FollowSpecificCharacter)
                {
                    playerToFollow = wowPlayers.FirstOrDefault(p => p.Name.Equals(Config.SpecificCharacterToFollow, StringComparison.OrdinalIgnoreCase));

                    if (!ignoreRange)
                    {
                        playerToFollow = SkipIfOutOfRange(playerToFollow);
                    }
                }

                // check the group/raid leader
                if (playerToFollow == null && Config.FollowGroupLeader)
                {
                    playerToFollow = wowPlayers.FirstOrDefault(p => p.Guid == WowInterface.ObjectManager.PartyleaderGuid);

                    if (!ignoreRange)
                    {
                        playerToFollow = SkipIfOutOfRange(playerToFollow);
                    }
                }

                // check the group members
                if (playerToFollow == null && Config.FollowGroupMembers)
                {
                    playerToFollow = wowPlayers.FirstOrDefault(p => WowInterface.ObjectManager.PartymemberGuids.Contains(p.Guid));

                    if (!ignoreRange)
                    {
                        playerToFollow = SkipIfOutOfRange(playerToFollow);
                    }
                }
            }

            return playerToFollow != null;
        }

        internal bool IsRepairNpcNear()
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .Any(e => e.GetType() != typeof(WowPlayer)
                       && e.IsRepairVendor
                       && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.RepairNpcSearchRadius);
        }

        internal bool IsVendorNpcNear()
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .Any(e => e.GetType() != typeof(WowPlayer)
                       && e.IsVendor
                       && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.MerchantNpcSearchRadius);
        }

        private void CheckForBattlegroundInvites()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.BattlegroundStatus, out int bgStatus)
                && bgStatus == 2)
            {
                WowInterface.HookManager.AcceptBattlegroundInvite();
            }
        }

        private void OnGossipClosed(long timestamp, List<string> args)
        {
            QuestgiverGossipOpen = false;
            QuestgiverGossipOptionCount = 0;
        }

        private void OnGossipShow(long timestamp, List<string> args)
        {
            QuestgiverGossipOpen = true;
            QuestgiverGossipOptionCount = WowInterface.HookManager.GetGossipOptionCount();
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
        {
            return (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance);
        }
    }
}