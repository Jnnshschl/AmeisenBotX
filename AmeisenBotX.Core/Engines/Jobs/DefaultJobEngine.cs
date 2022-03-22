using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Jobs.Enums;
using AmeisenBotX.Core.Engines.Jobs.Profiles;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Jobs
{
    public class DefaultJobEngine : IJobEngine
    {
        public DefaultJobEngine(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            AmeisenLogger.I.Log("JobEngine", $"Initializing", LogLevel.Verbose);

            Bot = bot;
            Config = config;

            MiningEvent = new(TimeSpan.FromSeconds(1));
            BlacklistEvent = new(TimeSpan.FromSeconds(1));
            MailSentEvent = new(TimeSpan.FromSeconds(3));

            NodeBlacklist = new();
        }

        public List<ulong> NodeBlacklist { get; set; }

        public IJobProfile Profile { get; set; }

        private TimegatedEvent BlacklistEvent { get; }

        private AmeisenBotInterfaces Bot { get; }

        private bool CheckForPathRecovering { get; set; }

        private AmeisenBotConfig Config { get; }

        private int CurrentNodeCounter { get; set; }

        private TimegatedEvent MailSentEvent { get; }

        private TimegatedEvent MiningEvent { get; }

        private int NodeTryCounter { get; set; }

        private ulong SelectedGuid { get; set; }

        private Vector3 SelectedPosition { get; set; }

        private int SellActionsNeeded { get; set; }

        public void Enter()
        {
            AmeisenLogger.I.Log("JobEngine", $"Entering JobEngine", LogLevel.Verbose);
            CheckForPathRecovering = true;
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
            if (Bot.Player.IsCasting)
            {
                return;
            }

            if (CheckForPathRecovering)
            {
                Vector3 closestNode = miningProfile.Path.OrderBy(e => e.GetDistance(Bot.Player.Position)).First();
                CurrentNodeCounter = miningProfile.Path.IndexOf(closestNode) + 1;
                CheckForPathRecovering = false;
                NodeTryCounter = 0;
            }

            if (Bot.Character.Inventory.FreeBagSlots < 3 && SellActionsNeeded == 0)
            {
                SellActionsNeeded = (int)Math.Ceiling(Bot.Character.Inventory.Items.Count / 12.0); // 12 items per mail
                CheckForPathRecovering = true;
            }

            if (SellActionsNeeded > 0)
            {
                IWowGameobject mailboxNode = Bot.Objects.All.OfType<IWowGameobject>()
                    .Where(x => Enum.IsDefined(typeof(MailBox), x.DisplayId)
                            && x.Position.GetDistance(Bot.Player.Position) < 15)
                    .OrderBy(x => x.Position.GetDistance(Bot.Player.Position))
                    .FirstOrDefault();

                if (mailboxNode != null)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, mailboxNode.Position);

                    if (Bot.Player.Position.GetDistance(mailboxNode.Position) <= 4)
                    {
                        Bot.Movement.StopMovement();

                        if (MailSentEvent.Run())
                        {
                            Bot.Wow.InteractWithObject(mailboxNode);
                            Bot.Wow.LuaDoString("MailFrameTab2:Click();");

                            int usedItems = 0;
                            foreach (IWowInventoryItem item in Bot.Character.Inventory.Items)
                            {
                                if (Config.ItemSellBlacklist.Contains(item.Name)
                                    || item.Name.Contains("Mining Pick", StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }

                                Bot.Wow.UseContainerItem(item.BagId, item.BagSlot);
                                ++usedItems;
                            }

                            if (usedItems > 0)
                            {
                                Bot.Wow.LuaDoString($"SendMail('{Config.JobEngineMailReceiver}', '{Config.JobEngineMailHeader}', '{Config.JobEngineMailText}')");
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
                        Bot.Movement.SetMovementAction(MovementAction.Move, mailboxNode.Position);
                    }
                }
                else
                {
                    Vector3 currentNode = miningProfile.MailboxNodes.OrderBy(x => x.GetDistance(Bot.Player.Position)).FirstOrDefault();
                    Bot.Movement.SetMovementAction(MovementAction.Move, currentNode);
                }

                return;
            }

            if (SelectedPosition == default)
            {
                // search for nodes
                int miningSkill = Bot.Character.Skills.ContainsKey("Mining") ? Bot.Character.Skills["Mining"].Item1 : 0;

                IWowGameobject nearestNode = Bot.Objects.All.OfType<IWowGameobject>()
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
                    .OrderBy(x => x.Position.GetDistance(Bot.Player.Position))
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
                    Vector3 currentNode = miningProfile.Path[CurrentNodeCounter];
                    Bot.Movement.SetMovementAction(MovementAction.Move, currentNode);

                    if (Bot.Player.Position.GetDistance(currentNode) < 3.0f)
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
                double distanceToNode = Bot.Player.Position.GetDistance(SelectedPosition);
                IWowGameobject node = Bot.GetWowObjectByGuid<IWowGameobject>(SelectedGuid);

                if (node == null)
                {
                    // if we are 20m or less near the node and its still not loaded, we can ignore it
                    SelectedPosition = default;
                    SelectedGuid = 0;
                }
                else if (distanceToNode < 3.0f)
                {
                    if (Bot.Player.IsMounted)
                    {
                        Bot.Wow.DismissCompanion("MOUNT");
                        return;
                    }

                    Bot.Movement.StopMovement();

                    if (MiningEvent.Run())
                    {
                        if (Bot.Memory.Read(Bot.Memory.Offsets.LootWindowOpen, out byte lootOpen)
                            && lootOpen > 0)
                        {
                            Bot.Wow.LootEverything();
                        }
                        else
                        {
                            Bot.Wow.InteractWithObject(node);
                        }
                    }

                    CheckForPathRecovering = true;
                    NodeTryCounter = 0;
                }
                else if (!Bot.Movement.SetMovementAction(MovementAction.Move, node.Position))
                {
                    if (NodeTryCounter > 2)
                    {
                        NodeBlacklist.Add(node.Guid);
                        NodeTryCounter = 0;
                    }

                    ++NodeTryCounter;
                }
            }
        }
    }
}