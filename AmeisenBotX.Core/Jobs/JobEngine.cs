﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Jobs.Enums;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Jobs
{
    public class JobEngine
    {
        public JobEngine(WowInterface wowInterface, AmeisenBotConfig config)
        {
            AmeisenLogger.I.Log("JobEngine", $"Initializing", LogLevel.Verbose);

            WowInterface = wowInterface;
            Config = config;

            MiningEvent = new(TimeSpan.FromSeconds(1));
            BlacklistEvent = new(TimeSpan.FromSeconds(1));
            MailSentEvent = new(TimeSpan.FromSeconds(3));

            NodeBlacklist = new();
        }

        public AmeisenBotConfig Config { get; set; }

        public bool GeneratedPathToNode { get; set; }

        public List<ulong> NodeBlacklist { get; set; }

        public IJobProfile Profile { get; set; }

        private TimegatedEvent BlacklistEvent { get; }

        private bool CheckForPathRecovering { get; set; }

        private int CurrentNodeCounter { get; set; }

        private TimegatedEvent MailSentEvent { get; }

        private TimegatedEvent MiningEvent { get; }

        private int NodeTryCounter { get; set; }

        private ulong SelectedGuid { get; set; }

        private Vector3 SelectedPosition { get; set; }

        private int SellActionsNeeded { get; set; }

        private WowInterface WowInterface { get; }

        public void Enter()
        {
            AmeisenLogger.I.Log("JobEngine", $"Entering JobEngine", LogLevel.Verbose);
            CheckForPathRecovering = true;
            GeneratedPathToNode = false;
        }

        public void Execute()
        {
            if (Profile != null)
            {
                switch (Profile.JobType)
                {
                    case JobType.Mining:
                        ExecuteMining((IMiningProfile)Profile);
                        break;
                }
            }
        }

        public void Reset()
        {
            AmeisenLogger.I.Log("JobEngine", $"Resetting JobEngine", LogLevel.Verbose);
        }

        private void ExecuteMining(IMiningProfile miningProfile)
        {
            if (WowInterface.Player.IsCasting)
            {
                return;
            }

            if (CheckForPathRecovering)
            {
                Vector3 closestNode = miningProfile.Path.OrderBy(e => e.GetDistance(WowInterface.Player.Position)).First();
                CurrentNodeCounter = miningProfile.Path.IndexOf(closestNode) + 1;
                CheckForPathRecovering = false;
                NodeTryCounter = 0;
            }

            if (WowInterface.CharacterManager.Inventory.FreeBagSlots < 3 && SellActionsNeeded == 0)
            {
                SellActionsNeeded = (int)Math.Ceiling(WowInterface.CharacterManager.Inventory.Items.Count / 12.0); // 12 items per mail
                CheckForPathRecovering = true;
            }

            if (SellActionsNeeded > 0)
            {
                WowGameobject mailboxNode = WowInterface.Objects.WowObjects
                    .OfType<WowGameobject>()
                    .Where(x => Enum.IsDefined(typeof(MailBox), x.DisplayId)
                            && x.Position.GetDistance(WowInterface.Player.Position) < 15)
                    .OrderBy(x => x.Position.GetDistance(WowInterface.Player.Position))
                    .FirstOrDefault();

                if (mailboxNode != null)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, mailboxNode.Position);

                    if (WowInterface.Player.Position.GetDistance(mailboxNode.Position) <= 4)
                    {
                        WowInterface.MovementEngine.StopMovement();

                        if (MailSentEvent.Run())
                        {
                            WowInterface.NewWowInterface.WowObjectRightClick(mailboxNode.BaseAddress);
                            WowInterface.NewWowInterface.LuaDoString("MailFrameTab2:Click();");

                            int usedItems = 0;
                            foreach (IWowItem item in WowInterface.CharacterManager.Inventory.Items)
                            {
                                if (Config.ItemSellBlacklist.Contains(item.Name) || item.Name.Contains("Mining Pick", StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }

                                WowInterface.NewWowInterface.LuaUseContainerItem(item.BagId, item.BagSlot);
                                ++usedItems;
                            }

                            if (usedItems > 0)
                            {
                                WowInterface.NewWowInterface.LuaDoString($"SendMail('{Config.JobEngineMailReceiver}', '{Config.JobEngineMailHeader}', '{Config.JobEngineMailText}')");
                                --SellActionsNeeded;
                            }
                            else
                            {
                                SellActionsNeeded = 0;
                            }
                        }
                    }
                    else
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, mailboxNode.Position);
                    }
                }
                else
                {
                    Vector3 currentNode = miningProfile.MailboxNodes.OrderBy(x => x.GetDistance(WowInterface.Player.Position)).FirstOrDefault();
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, currentNode);
                }

                return;
            }

            if (SelectedPosition == default)
            {
                // search for nodes
                int miningSkill = WowInterface.CharacterManager.Skills.ContainsKey("Mining") ? WowInterface.CharacterManager.Skills["Mining"].Item1 : 0;

                WowGameobject nearestNode = WowInterface.Objects.WowObjects
                    .OfType<WowGameobject>()
                    .Where(e => !NodeBlacklist.Contains(e.Guid)
                             && Enum.IsDefined(typeof(WowOreId), e.DisplayId)
                             && miningProfile.OreTypes.Contains((WowOreId)e.DisplayId)
                             && (((WowOreId)e.DisplayId) == WowOreId.Copper
                             || (((WowOreId)e.DisplayId) == WowOreId.Tin && miningSkill >= 65)
                             || (((WowOreId)e.DisplayId) == WowOreId.Silver && miningSkill >= 75)
                             || (((WowOreId)e.DisplayId) == WowOreId.Iron && miningSkill >= 125)
                             || (((WowOreId)e.DisplayId) == WowOreId.Gold && miningSkill >= 155)
                             || (((WowOreId)e.DisplayId) == WowOreId.Mithril && miningSkill >= 175)
                             || (((WowOreId)e.DisplayId) == WowOreId.DarkIron && miningSkill >= 230)
                             || (((WowOreId)e.DisplayId) == WowOreId.SmallThorium && miningSkill >= 245)
                             || (((WowOreId)e.DisplayId) == WowOreId.RichThorium && miningSkill >= 275)
                             || (((WowOreId)e.DisplayId) == WowOreId.FelIron && miningSkill >= 300)
                             || (((WowOreId)e.DisplayId) == WowOreId.Adamantite && miningSkill >= 325)
                             || (((WowOreId)e.DisplayId) == WowOreId.Cobalt && miningSkill >= 350)
                             || (((WowOreId)e.DisplayId) == WowOreId.Khorium && miningSkill >= 375)
                             || (((WowOreId)e.DisplayId) == WowOreId.Saronite && miningSkill >= 400)
                             || (((WowOreId)e.DisplayId) == WowOreId.Titanium && miningSkill >= 450)))
                    .OrderBy(x => x.Position.GetDistance(WowInterface.Player.Position))
                    .FirstOrDefault();

                if (nearestNode != null)
                {
                    // select node and try to find it
                    SelectedPosition = nearestNode.Position;
                    SelectedGuid = nearestNode.Guid;
                }
                else
                {
                    // if no node was found, follow the path
                    GeneratedPathToNode = false;

                    Vector3 currentNode = miningProfile.Path[CurrentNodeCounter];
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, currentNode);

                    if (WowInterface.Player.Position.GetDistance(currentNode) < 3.0f)
                    {
                        ++CurrentNodeCounter;

                        if (CurrentNodeCounter >= miningProfile.Path.Count)
                        {
                            if (!miningProfile.IsCirclePath)
                            {
                                miningProfile.Path.Reverse();
                            }

                            CurrentNodeCounter = 0;
                        }
                    }
                }
            }
            else
            {
                // move to the node
                double distanceToNode = WowInterface.Player.Position.GetDistance(SelectedPosition);
                WowGameobject node = WowInterface.Objects.GetWowObjectByGuid<WowGameobject>(SelectedGuid);

                if (distanceToNode < 3)
                {
                    if (WowInterface.Player.IsMounted)
                    {
                        WowInterface.NewWowInterface.LuaDismissCompanion();
                        return;
                    }

                    WowInterface.MovementEngine.StopMovement();

                    if (MiningEvent.Run()) // limit the executions
                    {
                        if (WowInterface.XMemory.Read(WowInterface.OffsetList.LootWindowOpen, out byte lootOpen)
                            && lootOpen > 0)
                        {
                            WowInterface.NewWowInterface.LuaLootEveryThing();
                        }
                        else
                        {
                            WowInterface.NewWowInterface.WowObjectRightClick(node.BaseAddress);
                        }
                    }

                    CheckForPathRecovering = true;
                    NodeTryCounter = 0;
                }
                else if (distanceToNode < 20.0 && node == null)
                {
                    // if we are 20m or less near the node and its still not loaded, we can ignore it
                    SelectedPosition = default;
                    SelectedGuid = 0;
                }
                else
                {
                    if (GeneratedPathToNode && BlacklistEvent.Run())
                    {
                        if (!WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, node.Position))
                        {
                            if (NodeTryCounter > 2)
                            {
                                NodeBlacklist.Add(node.Guid);
                                NodeTryCounter = 0;
                            }

                            ++NodeTryCounter;
                        }
                        else
                        {
                            GeneratedPathToNode = true;
                        }
                    }
                }
            }
        }
    }
}