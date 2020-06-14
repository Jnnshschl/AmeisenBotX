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
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Jobs
{
    public class JobEngine
    {
        public JobEngine(WowInterface wowInterface)
        {
            AmeisenLogger.Instance.Log("JobEngine", $"Initializing", LogLevel.Verbose);

            WowInterface = wowInterface;
            MiningEvent = new TimegatedEvent(TimeSpan.FromSeconds(5));
            JobProfile = new CopperElwynnForestProfile();
        }

        private WowInterface WowInterface { get; }

        private TimegatedEvent MiningEvent { get; }

        private int CurrentNodeCounter { get; set; }

        public IJobProfile JobProfile { get; set; }

        public void Execute()
        {
            switch (JobProfile.JobType)
            {
                case JobType.Mining:
                    ExecuteMining((IMiningProfile)JobProfile);
                    break;
            }
        }

        private void ExecuteMining(IMiningProfile miningProfile)
        {
            List<WowGameobject> oreNodes = WowInterface.ObjectManager.WowObjects
                .OfType<WowGameobject>() // only WowGameobjects
                .Where(x => Enum.IsDefined(typeof(OreNodes), x.DisplayId) // make sure the displayid is a ore node
                         && miningProfile.OreTypes.Contains((OreNodes)x.DisplayId) // onlynodes in profile
                         && x.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 100) // only nodes that are closer than 100m to me
                .ToList(); // convert to list

            if (oreNodes.Count > 0)
            {
                WowGameobject nearNode = oreNodes
                    .OrderBy(x => x.Position.GetDistance(WowInterface.ObjectManager.Player.Position)) // order by distance to me
                    .First(); // get the closest node to me

                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, nearNode.Position);

                if (WowInterface.MovementEngine.IsAtTargetPosition
                    && MiningEvent.Run()) // limit the executions
                {
                    WowInterface.HookManager.WowObjectOnRightClick(nearNode);
                    WowInterface.HookManager.LootEveryThing();
                }
            }
            else
            {
                Vector3 currentNode = miningProfile.Path[CurrentNodeCounter];
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, currentNode);

                if (WowInterface.MovementEngine.IsAtTargetPosition)
                {
                    ++CurrentNodeCounter;
                    // [0][1][2]
                    // Count = 3

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

        public void Reset()
        {
            AmeisenLogger.Instance.Log("JobEngine", $"Resetting JobEngine", LogLevel.Verbose);
        }

    }
}