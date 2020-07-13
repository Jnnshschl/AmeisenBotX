using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Jobs.Enums;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Jobs.Profiles.Gathering;
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
        public JobEngine(WowInterface wowInterface)
        {
            AmeisenLogger.Instance.Log("JobEngine", $"Initializing", LogLevel.Verbose);

            WowInterface = wowInterface;
            MiningEvent = new TimegatedEvent(TimeSpan.FromSeconds(5));
            MailSentEvent = new TimegatedEvent(TimeSpan.FromSeconds(5));
            JobProfile = new CopperElwynnForestProfile();
            //JobProfile = new CopperTinSilverWestfallProfile();
        }

        public IJobProfile JobProfile { get; set; }

        private int CurrentNodeCounter { get; set; }

        private TimegatedEvent MiningEvent { get; }
        private TimegatedEvent MailSentEvent { get; }

        private bool OreIsInRange { get; set; }
        private bool MailBoxIsInRange { get; set; }
        private WowInterface WowInterface { get; }

        public void Execute()
        {
            switch (JobProfile.JobType)
            {
                case JobType.Mining:
                    ExecuteMining((IMiningProfile)JobProfile);
                    break;
            }
        }

        public void Reset()
        {
            AmeisenLogger.Instance.Log("JobEngine", $"Resetting JobEngine", LogLevel.Verbose);
        }
        private void ExecuteMining(IMiningProfile miningProfile)
        {
            if (WowInterface.CharacterManager.Inventory.FreeBagSlots == 0)
            {
                //List<WowGameobject> Objectlist = WowInterface.ObjectManager.WowObjects
                //.OfType<WowGameobject>() // only WowGameobjects
                //.Where(x => x.Position.GetDistance(WowInterface.ObjectManager.Player.Position) <= 15)
                //.ToList();

                List<WowGameobject> MailBoxNode = WowInterface.ObjectManager.WowObjects
                .OfType<WowGameobject>() // only WowGameobjects
                .Where(x => Enum.IsDefined(typeof(MailBox), x.DisplayId) // make sure the displayid is a MailBox
                        && x.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 15) // only nodes that are closer than 15m to me
                .ToList(); // convert to list

                if (MailBoxNode.Count > 0)
                {
                    WowGameobject nearNode = MailBoxNode
                    .OrderBy(x => x.Position.GetDistance(WowInterface.ObjectManager.Player.Position)) // order by distance to me
                    .First(); // get the closest node to me

                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, nearNode.Position);

                    MailBoxIsInRange = WowInterface.ObjectManager.Player.Position.GetDistance(nearNode.Position) <= 4;

                    if (MailBoxIsInRange)
                    {
                        WowInterface.HookManager.StopClickToMoveIfActive();
                        WowInterface.MovementEngine.Reset();

                        if (MailSentEvent.Run())
                        {
                            WowInterface.HookManager.WowObjectOnRightClick(nearNode);
                            WowInterface.HookManager.SendChatMessage("/y /run MailFrameTab2: Click()");
                            WowInterface.HookManager.SendChatMessage("/y /run for b = 0, 4 do for s = 0, 22 do l = GetContainerItemLink(b, s) if l and l:find('Copper Ore')then UseContainerItem(b, s) end end end");
                            WowInterface.HookManager.SendChatMessage("/y /run SendMail('Jamsbaerx', 'Ore', 'New items for you')") ;
                            return;
                        }
                        return;
                    }
                    else
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, nearNode.Position);
                        return;
                    }
                }
                else
                {
                    Vector3 currentNode = miningProfile.MailboxNodes[0];
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, currentNode);
                    return;
                }
            }

            List<WowGameobject> oreNodes = WowInterface.ObjectManager.WowObjects
                .OfType<WowGameobject>() // only WowGameobjects
                .Where(x => Enum.IsDefined(typeof(OreNodes), x.DisplayId) // make sure the displayid is a ore node
                        && miningProfile.OreTypes.Contains((OreNodes)x.DisplayId) // onlynodes in profile
                        && x.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 15) // only nodes that are closer than 15m to me
                .ToList(); // convert to list

            if (oreNodes.Count > 0)
            {
                WowGameobject nearNode = oreNodes
                    .OrderBy(x => x.Position.GetDistance(WowInterface.ObjectManager.Player.Position)) // order by distance to me
                    .First(); // get the closest node to me
                OreIsInRange = WowInterface.ObjectManager.Player.Position.GetDistance(nearNode.Position) <= 4;

                if (OreIsInRange)
                {
                    WowInterface.HookManager.StopClickToMoveIfActive();
                    WowInterface.MovementEngine.Reset();

                    if (MiningEvent.Run()) // limit the executions
                    {
                        WowInterface.HookManager.WowObjectOnRightClick(nearNode);
                        WowInterface.HookManager.LootEveryThing();
                    }
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, nearNode.Position);
                }
            }
            else
            {
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