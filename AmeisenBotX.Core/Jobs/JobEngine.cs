using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Jobs.Enums;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Jobs.Profiles.Gathering;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Jobs
{
    public class JobEngine
    {
        public JobEngine(ObjectManager objectManager, IMovementEngine movementEngine, HookManager hookManager)
        {
            ObjectManager = objectManager;
            MovementEngine = movementEngine;
            HookManager = hookManager;

            Reset();
        }

        public IJobProfile JobProfile { get; set; }

        public JobEngineStatus JobEngineStatus { get; private set; }

        private ObjectManager ObjectManager { get; }

        private IMovementEngine MovementEngine { get; }

        private HookManager HookManager { get; }

        private int CurrentNodeAt { get; set; }

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

        public void Reset()
        {
            JobEngineStatus = JobEngineStatus.None;
            CurrentNodeAt = 0;
        }

        public void ExecuteGathering()
        {
            IGatheringProfile gatheringProfile = (IGatheringProfile)JobProfile;
            if (gatheringProfile.Path.Count > 0)
            {
                if (CurrentNodeAt > gatheringProfile.Path.Count - 1)
                {
                    CurrentNodeAt = 0;
                }

                IEnumerable<WowGameobject> nearNodes = ObjectManager.WowObjects.OfType<WowGameobject>().Where(e=>gatheringProfile.DisplayIds.Contains(e.DisplayId));

                if (nearNodes.Count() > 0)
                {
                    JobEngineStatus = JobEngineStatus.Found;
                    WowGameobject selectedNode = nearNodes.First();

                    if(selectedNode.Position.GetDistance(ObjectManager.Player.Position) > 6)
                    {
                        MovementEngine.SetState(MovementEngineState.Moving, gatheringProfile.Path[CurrentNodeAt]);
                        MovementEngine.Execute();
                    }
                    else
                    {
                        JobEngineStatus = JobEngineStatus.Gathering;

                        if (!ObjectManager.Player.IsCasting)
                        {
                            HookManager.RightClickObject(selectedNode);
                        }
                    }
                }
                else
                {
                    JobEngineStatus = JobEngineStatus.Searching;
                    MovementEngine.SetState(MovementEngineState.Moving, gatheringProfile.Path[CurrentNodeAt]);
                    MovementEngine.Execute();
                }
            }
        }
    }
}
