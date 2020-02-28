using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Jobs.Enums;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Jobs.Profiles.Gathering;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Pathfinding.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Jobs
{
    public class JobEngine
    {
        public JobEngine(ObjectManager objectManager, IMovementEngine movementEngine, HookManager hookManager, CharacterManager characterManager)
        {
            AmeisenLogger.Instance.Log("JobEngine", $"Initializing", LogLevel.Verbose);

            ObjectManager = objectManager;
            MovementEngine = movementEngine;
            HookManager = hookManager;
            CharacterManager = characterManager;

            MailboxItemQueue = new Queue<string>();

            Reset();
        }

        public JobEngineStatus JobEngineStatus { get; private set; }

        public IJobProfile JobProfile { get; set; }

        private int CurrentNodeAt { get; set; }

        private HookManager HookManager { get; }

        private CharacterManager CharacterManager { get; }

        private bool MailboxMode { get; set; }

        private IMovementEngine MovementEngine { get; }

        private ObjectManager ObjectManager { get; }

        private Queue<string> MailboxItemQueue { get; }

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
                if (ObjectManager.Player.IsCasting)
                {
                    return;
                }

                if ((gatheringProfile.MailboxPosition != Vector3.Zero
                    && gatheringProfile.MailItems != null
                    && gatheringProfile.MailItems.Count > 0
                    && gatheringProfile.MailReceiver.Length > 0
                    && HookManager.GetFreeBagSlotCount() == 0)
                    || MailboxMode)
                {
                    JobEngineStatus = JobEngineStatus.Mailbox;

                    if (gatheringProfile.MailboxPosition.GetDistance(ObjectManager.Player.Position) > 6)
                    {
                        // move towards mailbox
                        MovementEngine.SetState(MovementEngineState.Moving, gatheringProfile.MailboxPosition);
                        MovementEngine.Execute();
                    }
                    else
                    {
                        // get the mailbox
                        WowGameobject mailbox = ObjectManager.WowObjects.OfType<WowGameobject>().FirstOrDefault(e => e.GameobjectType == WowGameobjectType.Mailbox);

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
                IEnumerable<WowGameobject> nearNodes = ObjectManager.WowObjects.OfType<WowGameobject>().Where(e => gatheringProfile.DisplayIds.Contains(e.DisplayId));

                if (nearNodes.Count() > 0)
                {
                    JobEngineStatus = JobEngineStatus.Found;

                    // select the nearest node
                    WowGameobject selectedNode = nearNodes.OrderBy(e => e.Position.GetDistance(ObjectManager.Player.Position)).First();

                    if (selectedNode.Position.GetDistance(ObjectManager.Player.Position) > 6)
                    {
                        // move to it until we are close enough
                        MovementEngine.SetState(MovementEngineState.Moving, gatheringProfile.Path[CurrentNodeAt]);
                        MovementEngine.Execute();
                    }
                    else
                    {
                        JobEngineStatus = JobEngineStatus.Gathering;

                        // gather it
                        HookManager.RightClickObject(selectedNode);
                        AmeisenLogger.Instance.Log("JobEngine", $"Trying to gather gObject with GUID: {selectedNode.Guid}", LogLevel.Verbose);
                    }
                }
                else
                {
                    JobEngineStatus = JobEngineStatus.Searching;

                    if (gatheringProfile.Path[CurrentNodeAt].GetDistance(ObjectManager.Player.Position) > 6)
                    {
                        // move towards next node
                        MovementEngine.SetState(MovementEngineState.Moving, gatheringProfile.Path[CurrentNodeAt]);
                        MovementEngine.Execute();
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
            HookManager.RightClickObject(mailbox);
            Task.Delay(1000).GetAwaiter().GetResult();
            AmeisenLogger.Instance.Log("JobEngine", $"Rightclicked Mailbox", LogLevel.Verbose);

            JobEngineStatus = JobEngineStatus.Sending;

            // send stuff to character
            HookManager.SendItemMailToCharacter(MailboxItemQueue.Peek(), receiver);
            Task.Delay(1000).GetAwaiter().GetResult();
            AmeisenLogger.Instance.Log("JobEngine", $"Sent Mail with \"{MailboxItemQueue.Peek()}\" to \"{receiver}\"", LogLevel.Verbose);

            // remove item from mail list if we have no units left in our bags
            CharacterManager.Inventory.Update();
            if (!CharacterManager.Inventory.Items.Any(e=>e.Name.ToUpper() == MailboxItemQueue.Peek().ToUpper()))
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

        public void Reset()
        {
            AmeisenLogger.Instance.Log("JobEngine", $"Resetting JobEngine", LogLevel.Verbose);
            JobEngineStatus = JobEngineStatus.None;
            CurrentNodeAt = 0;
            MailboxMode = false;
            MailboxItemQueue.Clear();
        }
    }
}