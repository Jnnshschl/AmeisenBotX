using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.Objectives;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Battleground.Profiles
{
    public class WarsongGulchProfile : IBattlegroundProfile
    {
        public WarsongGulchProfile(bool isAlliance, HookManager hookManager, ObjectManager objectManager, IMovementEngine movementEngine)
        {
            ObjectManager = objectManager;
            MovementEngine = movementEngine;
            HookManager = hookManager;

            if (isAlliance)
            {
                WsgDataset = new AllianceWsgDataset();
            }
            else
            {
                WsgDataset = new HordeWsgDataset();
            }

            Objectives = new List<IBattlegroundObjective>
            {
                new CarryFlagToBaseObjective(2, WsgDataset.OwnFlagPosition, hookManager, objectManager, movementEngine),
                new CaptureTheFlagObjective(1, WsgDataset.TargetFlagPosition, hookManager, objectManager, movementEngine)
            };
        }

        public BattlegroundType BattlegroundType { get; } = BattlegroundType.CaptureTheFlag;

        public List<IBattlegroundObjective> Objectives { get; private set; }

        private IWsgDataset WsgDataset { get; set; }

        private ObjectManager ObjectManager { get; set; }

        private HookManager HookManager { get; set; }

        private IMovementEngine MovementEngine { get; set; }

        private interface IWsgDataset
        {
            Vector3 TargetFlagPosition { get; }

            Vector3 OwnFlagPosition { get; }
        }

        private class AllianceWsgDataset : IWsgDataset
        {
            public Vector3 OwnFlagPosition { get; } = new Vector3(1539, 1481, 352);

            public Vector3 TargetFlagPosition { get; } = new Vector3(916, 1434, 346);
        }

        private class HordeWsgDataset : IWsgDataset
        {
            public Vector3 OwnFlagPosition { get; } = new Vector3(916, 1434, 346);

            public Vector3 TargetFlagPosition { get; } = new Vector3(1539, 1481, 352);
        }
    }
}
