using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Jobs.Enums;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Jobs.Profiles.Gathering;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Jobs
{
    public class JobEngine
    {
        public JobEngine(WowInterface wowInterface)
        {
            AmeisenLogger.Instance.Log("JobEngine", $"Initializing", LogLevel.Verbose);

            WowInterface = wowInterface;

            MailboxItemQueue = new Queue<string>();

            Reset();
        }

        public JobEngineStatus JobEngineStatus { get; private set; }

        public IJobProfile JobProfile { get; set; }

        private int CurrentNodeAt { get; set; }

        private Queue<string> MailboxItemQueue { get; }

        private bool MailboxMode { get; set; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (JobProfile != null)
            {
                switch (JobProfile.JobType)
                {
                    case JobType.Gathering:
                        ExecuteGathering();
                        break;

                    case JobType.Crafting:
                        break;

                    case JobType.Grinding:
                        break;
                }
            }
        }

        public void ExecuteGathering()
        {
            IGatheringProfile gatheringProfile = (IGatheringProfile)JobProfile;
            if (gatheringProfile.Path.Count > 0)
            {
                // check wether we gather something
                if (WowInterface.ObjectManager.Player.IsCasting)
                {
                    return;
                }

                if ((gatheringProfile.MailboxPosition != Vector3.Zero
                    && gatheringProfile.MailItems != null
                    && gatheringProfile.MailItems.Count > 0
                    && gatheringProfile.MailReceiver.Length > 0
                    && WowInterface.HookManager.GetFreeBagSlotCount() == 0)
                    || MailboxMode)
                {
                    JobEngineStatus = JobEngineStatus.Mailbox;

                    if (gatheringProfile.MailboxPosition.GetDistance(WowInterface.ObjectManager.Player.Position) > 6)
                    {
                        // move towards mailbox
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, gatheringProfile.MailboxPosition);
                        WowInterface.MovementEngine.Execute();
                    }
                    else
                    {
                        // get the mailbox
                        WowGameobject mailbox = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                            .FirstOrDefault(e => e.GameobjectType == WowGameobjectType.Mailbox);

                        if (mailbox != null)
                        {
                            SendItemMails(gatheringProfile.MailItems, gatheringProfile.MailReceiver, mailbox);
                        }
                    }

                    // mailbox has priority
                    return;
                }

                // return to start when end of path reached
                if (CurrentNodeAt > gatheringProfile.Path.Count - 1)
                {
                    CurrentNodeAt = 0;
                    AmeisenLogger.Instance.Log("JobEngine", $"End of path reached, moving to start", LogLevel.Verbose);
                }

                // scan for nearby nodes
                IEnumerable<WowGameobject> nearNodes = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                    .Where(e => gatheringProfile.DisplayIds.Contains(e.DisplayId));

                if (nearNodes.Count() > 0)
                {
                    JobEngineStatus = JobEngineStatus.Found;

                    // select the nearest node
                    WowGameobject selectedNode = nearNodes.OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).First();

                    if (selectedNode.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 6)
                    {
                        // move to it until we are close enough
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, gatheringProfile.Path[CurrentNodeAt]);
                        WowInterface.MovementEngine.Execute();
                    }
                    else
                    {
                        JobEngineStatus = JobEngineStatus.Gathering;

                        // gather it
                        WowInterface.HookManager.WowObjectOnRightClick(selectedNode);
                        AmeisenLogger.Instance.Log("JobEngine", $"Trying to gather gObject with GUID: {selectedNode.Guid}", LogLevel.Verbose);
                    }
                }
                else
                {
                    JobEngineStatus = JobEngineStatus.Searching;

                    if (gatheringProfile.Path[CurrentNodeAt].GetDistance(WowInterface.ObjectManager.Player.Position) > 6)
                    {
                        // move towards next node
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, gatheringProfile.Path[CurrentNodeAt]);
                        WowInterface.MovementEngine.Execute();
                    }
                    else
                    {
                        // next node
                        CurrentNodeAt++;
                        AmeisenLogger.Instance.Log("JobEngine", $"Moving to next Node", LogLevel.Verbose);
                    }
                }
            }
        }

        public void Reset()
        {
            AmeisenLogger.Instance.Log("JobEngine", $"Resetting JobEngine", LogLevel.Verbose);
            JobEngineStatus = JobEngineStatus.None;
            CurrentNodeAt = 0;
            MailboxMode = false;
            MailboxItemQueue.Clear();
        }

        private void SendItemMails(List<string> items, string receiver, WowGameobject mailbox)
        {
            if (!MailboxMode)
            {
                foreach (string item in items)
                {
                    MailboxItemQueue.Enqueue(item);
                }

                MailboxMode = true;
                AmeisenLogger.Instance.Log("JobEngine", $"Entering MailboxMode", LogLevel.Verbose);
            }

            // open mailbox
            WowInterface.HookManager.WowObjectOnRightClick(mailbox);
            Task.Delay(1000).GetAwaiter().GetResult();
            AmeisenLogger.Instance.Log("JobEngine", $"Rightclicked Mailbox", LogLevel.Verbose);

            JobEngineStatus = JobEngineStatus.Sending;

            // send stuff to character
            WowInterface.HookManager.SendItemMailToCharacter(MailboxItemQueue.Peek(), receiver);
            Task.Delay(1000).GetAwaiter().GetResult();
            AmeisenLogger.Instance.Log("JobEngine", $"Sent Mail with \"{MailboxItemQueue.Peek()}\" to \"{receiver}\"", LogLevel.Verbose);

            // remove item from mail list if we have no units left in our bags
            WowInterface.CharacterManager.Inventory.Update();
            if (!WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.ToUpper() == MailboxItemQueue.Peek().ToUpper()))
            {
                AmeisenLogger.Instance.Log("JobEngine", $"Finished sending \"{MailboxItemQueue.Peek()}\"", LogLevel.Verbose);
                MailboxItemQueue.Dequeue();
            }

            // continue if ther are no item left to be checked
            MailboxMode = MailboxItemQueue.Count > 0;

            if (!MailboxMode)
            {
                AmeisenLogger.Instance.Log("JobEngine", $"Leaving MailboxMode", LogLevel.Verbose);
            }
        }
    }
}