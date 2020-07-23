using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
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
            AmeisenLogger.Instance.Log("JobEngine", $"Initializing", LogLevel.Verbose);

            WowInterface = wowInterface;
            Config = config;

            MiningEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            BlacklistEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            MailSentEvent = new TimegatedEvent(TimeSpan.FromSeconds(3));

            NodeBlacklist = new List<ulong>();
        }

        public AmeisenBotConfig Config { get; set; }

        public IJobProfile Profile { get; set; }

        private int CurrentNodeCounter { get; set; }

        private int SellActionsNeeded { get; set; }

        private int NodeTryCounter { get; set; }

        private TimegatedEvent MailSentEvent { get; }

        private TimegatedEvent MiningEvent { get; }

        private TimegatedEvent BlacklistEvent { get; }

        private WowInterface WowInterface { get; }

        private bool CheckForPathRecovering { get; set; }

        public List<ulong> NodeBlacklist { get; set; }

        public bool GeneratedPathToNode { get; set; }

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

        public void Enter()
        {
            AmeisenLogger.Instance.Log("JobEngine", $"Entering JobEngine", LogLevel.Verbose);
            CheckForPathRecovering = true;
            GeneratedPathToNode = false;
        }

        public void Reset()
        {
            AmeisenLogger.Instance.Log("JobEngine", $"Resetting JobEngine", LogLevel.Verbose);
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
                SellActionsNeeded = (int)Math.Ceiling((double)WowInterface.CharacterManager.Inventory.Items.Count / 12.0); // 12 items per mail
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
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, mailboxNode.Position);

                    if (WowInterface.ObjectManager.Player.Position.GetDistance(mailboxNode.Position) <= 4)
                    {
                        WowInterface.MovementEngine.StopMovement();

                        if (MailSentEvent.Run())
                        {
                            WowInterface.HookManager.WowObjectOnRightClick(mailboxNode);
                            WowInterface.HookManager.LuaDoString("MailFrameTab2:Click();");

                            int usedItems = 0;
                            foreach (IWowItem item in WowInterface.CharacterManager.Inventory.Items)
                            {
                                if (Config.ItemSellBlacklist.Contains(item.Name) || item.Name.Contains("Pickaxe", StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }

                                WowInterface.HookManager.UseItemByBagAndSlot(item.BagId, item.BagSlot);
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
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, mailboxNode.Position);
                    }
                }
                else
                {
                    Vector3 currentNode = miningProfile.MailboxNodes.OrderBy(x => x.GetDistance(WowInterface.ObjectManager.Player.Position)).FirstOrDefault();
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, currentNode);
                }

                return;
            }

<<<<<<< HEAD
            List<WowGameobject> oreNodes = WowInterface.ObjectManager.WowObjects
                .OfType<WowGameobject>() // only WowGameobjects
                .Where(x => Enum.IsDefined(typeof(OreNodes), x.DisplayId) // make sure the displayid is a ore node
                        && miningProfile.OreTypes.Contains((OreNodes)x.DisplayId) // onlynodes in profile
                        && x.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 40) // only nodes that are closer than 40m to me
                .ToList(); // convert to list
=======
            int miningSkill = WowInterface.CharacterManager.Skills.ContainsKey("Mining") ? WowInterface.CharacterManager.Skills["Mining"].Item1 : 0;
>>>>>>> d558893406e80f0fe35c472fffb6bf388939cc27

            WowGameobject nearestNode = WowInterface.ObjectManager.WowObjects
                .OfType<WowGameobject>()
                .Where(e => !NodeBlacklist.Contains(e.Guid)
                         && Enum.IsDefined(typeof(OreNodes), e.DisplayId)
                         && miningProfile.OreTypes.Contains((OreNodes)e.DisplayId)
                         && (((OreNodes)e.DisplayId) == OreNodes.Copper
                         || (((OreNodes)e.DisplayId) == OreNodes.Tin && miningSkill >= 65)
                         || (((OreNodes)e.DisplayId) == OreNodes.Silver && miningSkill >= 75)
                         || (((OreNodes)e.DisplayId) == OreNodes.Iron && miningSkill >= 125)
                         || (((OreNodes)e.DisplayId) == OreNodes.Gold && miningSkill >= 155)
                         || (((OreNodes)e.DisplayId) == OreNodes.Mithril && miningSkill >= 175)
                         || (((OreNodes)e.DisplayId) == OreNodes.Truesilver && miningSkill >= 230)
                         || (((OreNodes)e.DisplayId) == OreNodes.DarkIron && miningSkill >= 230)
                         || (((OreNodes)e.DisplayId) == OreNodes.SmallThorium && miningSkill >= 245)
                         || (((OreNodes)e.DisplayId) == OreNodes.RichThorium && miningSkill >= 275)
                         || (((OreNodes)e.DisplayId) == OreNodes.ObsidianChunk && miningSkill >= 305)
                         || (((OreNodes)e.DisplayId) == OreNodes.FelIron && miningSkill >= 300)
                         || (((OreNodes)e.DisplayId) == OreNodes.Adamantite && miningSkill >= 325)
                         || (((OreNodes)e.DisplayId) == OreNodes.Cobalt && miningSkill >= 350)
                         || (((OreNodes)e.DisplayId) == OreNodes.Khorium && miningSkill >= 375)
                         || (((OreNodes)e.DisplayId) == OreNodes.Saronite && miningSkill >= 400)
                         || (((OreNodes)e.DisplayId) == OreNodes.Titanium && miningSkill >= 450))
                         && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 50.0)
                .OrderBy(x => x.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .FirstOrDefault();

            if (nearestNode != null)
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(nearestNode.Position) < 3)
                {
                    if (WowInterface.ObjectManager.Player.IsMounted)
                    {
                        WowInterface.HookManager.Dismount();
                        return;
                    }

                    WowInterface.MovementEngine.StopMovement();

                    if (MiningEvent.Run()) // limit the executions
                    {
                        if (WowInterface.XMemory.Read(WowInterface.OffsetList.LootWindowOpen, out byte lootOpen)
                            && lootOpen > 0)
                        {
                            WowInterface.HookManager.LootEveryThing();
                        }
                        else
                        {
                            WowInterface.HookManager.WowObjectOnRightClick(nearestNode);
                        }
                    }

                    CheckForPathRecovering = true;
                    NodeTryCounter = 0;
                }
                else
                {
                    if (GeneratedPathToNode && BlacklistEvent.Run() && !WowInterface.MovementEngine.HasCompletePathToPosition(nearestNode.Position, 4.0))
                    {
                        if (NodeTryCounter > 2)
                        {
                            NodeBlacklist.Add(nearestNode.Guid);
                            NodeTryCounter = 0;
                        }

                        ++NodeTryCounter;
                    }

                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, nearestNode.Position);
                    GeneratedPathToNode = true;
                }
            }
            else
            {
                GeneratedPathToNode = false;

                Vector3 currentNode = miningProfile.Path[CurrentNodeCounter];
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, currentNode);

                if (WowInterface.MovementEngine.IsAtTargetPosition)
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
    }
}