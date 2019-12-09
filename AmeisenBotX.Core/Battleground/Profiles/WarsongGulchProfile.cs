using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.Objectives;
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

namespace AmeisenBotX.Core.Battleground.Profiles
{
    public class WarsongGulchProfile : IBattlegroundProfile
    {
        public WarsongGulchProfile(bool isAlliance, HookManager hookManager, ObjectManager objectManager, IMovementEngine movementEngine, bool forceCombat)
        {
            ObjectManager = objectManager;
            MovementEngine = movementEngine;
            HookManager = hookManager;
            IsAlliance = isAlliance;

            if (isAlliance)
            {
                WsgDataset = new AllianceWsgDataset();
            }
            else
            {
                WsgDataset = new HordeWsgDataset();
            }

            // used to make the players do different things
            Random rnd = new Random();

            Objectives = new List<IBattlegroundObjective>
            {
                new CarryFlagToBaseObjective(100, WsgDataset.OwnFlagPosition, hookManager, objectManager, movementEngine),
                new AttackEnemyPlayers(rnd.Next(50), hookManager, objectManager, movementEngine, ref forceCombat),
                new CaptureFlagObjective(rnd.Next(10), WsgDataset.TargetFlagPosition, hookManager, objectManager, movementEngine),
                new RetrieveOwnFlagObjective(rnd.Next(10), hookManager, objectManager, movementEngine),
            };
        }

        public BattlegroundType BattlegroundType { get; } = BattlegroundType.CaptureTheFlag;

        public List<IBattlegroundObjective> Objectives { get; private set; }

        private bool IsAlliance { get; set; }

        private IWsgDataset WsgDataset { get; set; }

        private ObjectManager ObjectManager { get; set; }

        private HookManager HookManager { get; set; }

        private IMovementEngine MovementEngine { get; set; }

        private WowPlayer AllianceFlagCarrier { get; set; }

        private WowPlayer HordeFlagCarrier { get; set; }

        public void AllianceFlagWasPickedUp(string playername)
        {
            AllianceFlagCarrier = ObjectManager.WowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.Name.ToUpper() == playername.ToUpper());
            Objectives.OfType<CaptureFlagObjective>().First().IsAvailable = !IsAlliance;

            if (IsAlliance)
            {
                Objectives.OfType<RetrieveOwnFlagObjective>().First().IsAvailable = true;
                Objectives.OfType<RetrieveOwnFlagObjective>().First().FlagCarrier = AllianceFlagCarrier;
            }
        }

        public void HordeFlagWasPickedUp(string playername)
        {
            HordeFlagCarrier = ObjectManager.WowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.Name.ToUpper() == playername.ToUpper());
            Objectives.OfType<CaptureFlagObjective>().First().IsAvailable = IsAlliance;

            if (!IsAlliance)
            {
                Objectives.OfType<RetrieveOwnFlagObjective>().First().IsAvailable = true;
                Objectives.OfType<RetrieveOwnFlagObjective>().First().FlagCarrier = HordeFlagCarrier;
            }
        }

        public void AllianceFlagWasDropped(string playername)
        {
            AllianceFlagCarrier = null;
            Objectives.OfType<CaptureFlagObjective>().First().IsAvailable = !IsAlliance;
            Objectives.OfType<RetrieveOwnFlagObjective>().First().IsAvailable = !IsAlliance;
        }

        public void HordeFlagWasDropped(string playername)
        {
            HordeFlagCarrier = null;
            Objectives.OfType<CaptureFlagObjective>().First().IsAvailable = IsAlliance;
            Objectives.OfType<RetrieveOwnFlagObjective>().First().IsAvailable = IsAlliance;
        }

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
