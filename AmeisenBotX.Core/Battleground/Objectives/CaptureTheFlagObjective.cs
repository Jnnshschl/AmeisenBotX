using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Battleground.Objectives
{
    public class CaptureTheFlagObjective : IBattlegroundObjective
    {
        public CaptureTheFlagObjective(int priority, Vector3 flagPosition, HookManager hookManager, ObjectManager objectManager, IMovementEngine movementEngine)
        {
            Priority = priority;
            FlagPosition = flagPosition;
            HookManager = hookManager;
            ObjectManager = objectManager;
            MovementEngine = movementEngine;
            IsAvailable = true;
        }

        public int Priority { get; private set; }

        public Vector3 FlagPosition { get; private set; }

        public bool IsAvailable { get; set; }

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        private IMovementEngine MovementEngine { get; }

        public void Execute()
        {
            if (ObjectManager.Player.Position.GetDistance(FlagPosition) > 3)
            {
                MovementEngine.SetState(MovementEngineState.Moving, FlagPosition);
                MovementEngine.Execute();
            }
            else
            {
                ObjectManager.UpdateWowObjects();

                // interact with the flag
                WowObject flagObject = ObjectManager.WowObjects.OfType<WowObject>().OrderBy(e => e.Position.GetDistance(FlagPosition)).FirstOrDefault();

                if(flagObject != null && flagObject.GetType() == typeof(WowGameobject))
                {
                    HookManager.RightClickObject(flagObject);
                }
                else
                {
                    // flag is not here
                }
            }
        }
    }
}
