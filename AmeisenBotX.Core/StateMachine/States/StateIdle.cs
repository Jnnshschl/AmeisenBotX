﻿using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
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
            FirstStart = true;

            BagSlotCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(5000));
            EatCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(2000));
            LootCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(2000));
            RepairCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(5000));
            QuestgiverCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(2000));
            QuestgiverRightClickEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(3000));
            RefreshCharacterEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));
        }

        public bool FirstStart { get; set; }

        public bool QuestgiverGossipOpen { get; set; }

        public int QuestgiverGossipOptionCount { get; set; }

        private TimegatedEvent BagSlotCheckEvent { get; }

        private TimegatedEvent EatCheckEvent { get; }

        private TimegatedEvent LootCheckEvent { get; }

        private TimegatedEvent QuestgiverCheckEvent { get; }

        private TimegatedEvent QuestgiverRightClickEvent { get; }

        private TimegatedEvent RefreshCharacterEvent { get; }

        private TimegatedEvent RepairCheckEvent { get; }

        public override void Enter()
        {
            if (WowInterface.WowProcess != null && !WowInterface.WowProcess.HasExited && FirstStart)
            {
                FirstStart = false;
                WowInterface.XMemory.ReadString(WowInterface.OffsetList.PlayerName, Encoding.ASCII, out string playerName);
                StateMachine.PlayerName = playerName;

                if (!WowInterface.EventHookManager.IsActive)
                {
                    WowInterface.EventHookManager.Start();
                }

                WowInterface.EventHookManager.Subscribe("GOSSIP_SHOW", OnGossipShow);
                WowInterface.EventHookManager.Subscribe("GOSSIP_CLOSED", OnGossipClosed);

                WowInterface.HookManager.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFps});SetCVar(\"maxfpsbk\", {Config.MaxFps})");
                WowInterface.HookManager.EnableClickToMove();
            }

            if (RefreshCharacterEvent.Run())
            {
                WowInterface.CharacterManager.UpdateAll();
            }
        }

        public override void Execute()
        {
            // do we need to loot stuff
            if (LootCheckEvent.Run()
                && WowInterface.CharacterManager.Inventory.FreeBagSlots > 0
                && StateMachine.GetNearLootableUnits().Count() > 0)
            {
                StateMachine.SetState(BotState.Looting);
                return;
            }

            // do we need to eat something
            if (EatCheckEvent.Run()
                && StateMachine.GetState<StateEating>().NeedToEat())
            {
                StateMachine.SetState(BotState.Eating);
                return;
            }

            // we are on a battleground
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.BattlegroundStatus, out int bgStatus)
                && bgStatus == 3
                && !Config.BattlegroundUsePartyMode)
            {
                StateMachine.SetState(BotState.Battleground);
                return;
            }

            // we are in a dungeon
            if (WowInterface.ObjectManager.MapId.IsDungeonMap()
                && !Config.DungeonUsePartyMode)
            {
                StateMachine.SetState(BotState.Dungeon);
                return;
            }

            // do i need to follow someone
            if ((!Config.Autopilot || WowInterface.ObjectManager.MapId.IsDungeonMap()) && IsUnitToFollowThere(out _))
            {
                StateMachine.SetState(BotState.Following);
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
                    StateMachine.SetState(BotState.Repairing);
                    return;
                }
            }

            // do we need to sell stuff
            if (Config.AutoSell
                && BagSlotCheckEvent.Run()
                && StateMachine.GetState<StateSelling>().NeedToSell())
            {
                StateMachine.SetState(BotState.Selling);
                return;
            }

            // do i need to complete/get quests
            if (Config.AutoTalkToNearQuestgivers
                && QuestgiverCheckEvent.Run()
                && IsUnitToFollowThere(out WowPlayer unitToFollow, true)
                && unitToFollow.TargetGuid != 0)
            {
                HandleAutoQuestMode(unitToFollow);
            }

            // do buffing etc...
            WowInterface.CombatClass?.OutOfCombatExecute();

            if (StateMachine.StateOverride != BotState.Idle
                && StateMachine.StateOverride != BotState.None)
            {
                StateMachine.SetState(StateMachine.StateOverride);
            }
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

        private void CheckForBattlegroundInvites()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.BattlegroundStatus, out int bgStatus)
                && bgStatus == 2)
            {
                WowInterface.HookManager.AcceptBattlegroundInvite();
            }
        }

        private void HandleAutoQuestMode(WowPlayer wowPlayer)
        {
            WowUnit possibleQuestgiver = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(wowPlayer.TargetGuid);

            if (possibleQuestgiver != null && (possibleQuestgiver.IsQuestgiver || possibleQuestgiver.IsGossip))
            {
                double distance = WowInterface.ObjectManager.Player.Position.GetDistance(possibleQuestgiver.Position);

                if (distance > 32.0)
                {
                    return;
                }

                if (distance > 4.0)
                {
                    QuestgiverGossipOpen = false;
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, possibleQuestgiver.Position);
                    return;
                }
                else
                {
                    if (QuestgiverRightClickEvent.Run())
                    {
                        if (!QuestgiverGossipOpen)
                        {
                            WowInterface.HookManager.UnitOnRightClick(possibleQuestgiver);
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
                Vector3 pos = playerToFollow.Position;

                if (Config.FollowPositionDynamic)
                {
                    pos += StateMachine.GetState<StateFollowing>().Offset;
                }

                double distance = pos.GetDistance(WowInterface.ObjectManager.Player.Position);

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