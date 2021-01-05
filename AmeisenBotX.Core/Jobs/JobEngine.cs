using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Jobs.Enums;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
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

            MiningEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            BlacklistEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            MailSentEvent = new TimegatedEvent(TimeSpan.FromSeconds(3));

            NodeBlacklist = new List<ulong>();
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
            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (CheckForPathRecovering)
            {
                Vector3 closestNode = miningProfile.Path.OrderBy(e => e.GetDistance(WowInterface.ObjectManager.Player.Position)).First();
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
                WowGameobject mailboxNode = WowInterface.ObjectManager.WowObjects
                    .OfType<WowGameobject>()
                    .Where(x => Enum.IsDefined(typeof(MailBox), x.DisplayId)
                            && x.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 15)
                    .OrderBy(x => x.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                    .FirstOrDefault();

                if (mailboxNode != null)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, mailboxNode.Position);

                    if (WowInterface.ObjectManager.Player.Position.GetDistance(mailboxNode.Position) <= 4)
                    {
                        WowInterface.MovementEngine.StopMovement();

                        if (MailSentEvent.Run())
                        {
                            WowInterface.HookManager.WowObjectRightClick(mailboxNode);
                            WowInterface.HookManager.LuaDoString("MailFrameTab2:Click();");

                            int usedItems = 0;
                            foreach (IWowItem item in WowInterface.CharacterManager.Inventory.Items)
                            {
                                if (Config.ItemSellBlacklist.Contains(item.Name) || item.Name.Contains("Mining Pick", StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }

                                WowInterface.HookManager.LuaUseContainerItem(item.BagId, item.BagSlot);
                                ++usedItems;
                            }

                            if (usedItems > 0)
                            {
                                WowInterface.HookManager.LuaDoString($"SendMail('{Config.JobEngineMailReceiver}', '{Config.JobEngineMailHeader}', '{Config.JobEngineMailText}')");
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
                    Vector3 currentNode = miningProfile.MailboxNodes.OrderBy(x => x.GetDistance(WowInterface.ObjectManager.Player.Position)).FirstOrDefault();
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, currentNode);
                }

                return;
            }

            if (SelectedPosition == default)
            {
                // search for nodes
                int miningSkill = WowInterface.CharacterManager.Skills.ContainsKey("Mining") ? WowInterface.CharacterManager.Skills["Mining"].Item1 : 0;

                WowGameobject nearestNode = WowInterface.ObjectManager.WowObjects
                    .OfType<WowGameobject>()
                    .Where(e => !NodeBlacklist.Contains(e.Guid)
                             && Enum.IsDefined(typeof(OreNode), e.DisplayId)
                             && miningProfile.OreTypes.Contains((OreNode)e.DisplayId)
                             && (((OreNode)e.DisplayId) == OreNode.Copper
                             || (((OreNode)e.DisplayId) == OreNode.Tin && miningSkill >= 65)
                             || (((OreNode)e.DisplayId) == OreNode.Silver && miningSkill >= 75)
                             || (((OreNode)e.DisplayId) == OreNode.Iron && miningSkill >= 125)
                             || (((OreNode)e.DisplayId) == OreNode.Gold && miningSkill >= 155)
                             || (((OreNode)e.DisplayId) == OreNode.Mithril && miningSkill >= 175)
                             || (((OreNode)e.DisplayId) == OreNode.DarkIron && miningSkill >= 230)
                             || (((OreNode)e.DisplayId) == OreNode.SmallThorium && miningSkill >= 245)
                             || (((OreNode)e.DisplayId) == OreNode.RichThorium && miningSkill >= 275)
                             || (((OreNode)e.DisplayId) == OreNode.FelIron && miningSkill >= 300)
                             || (((OreNode)e.DisplayId) == OreNode.Adamantite && miningSkill >= 325)
                             || (((OreNode)e.DisplayId) == OreNode.Cobalt && miningSkill >= 350)
                             || (((OreNode)e.DisplayId) == OreNode.Khorium && miningSkill >= 375)
                             || (((OreNode)e.DisplayId) == OreNode.Saronite && miningSkill >= 400)
                             || (((OreNode)e.DisplayId) == OreNode.Titanium && miningSkill >= 450)))
                    .OrderBy(x => x.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
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
                double distanceToNode = WowInterface.ObjectManager.Player.Position.GetDistance(SelectedPosition);
                WowGameobject node = WowInterface.ObjectManager.GetWowObjectByGuid<WowGameobject>(SelectedGuid);

                if (distanceToNode < 3)
                {
                    if (WowInterface.ObjectManager.Player.IsMounted)
                    {
                        WowInterface.HookManager.LuaDismissCompanion();
                        return;
                    }

                    WowInterface.MovementEngine.StopMovement();

                    if (MiningEvent.Run()) // limit the executions
                    {
                        if (WowInterface.XMemory.Read(WowInterface.OffsetList.LootWindowOpen, out byte lootOpen)
                            && lootOpen > 0)
                        {
                            WowInterface.HookManager.LuaLootEveryThing();
                        }
                        else
                        {
                            WowInterface.HookManager.WowObjectRightClick(node);
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