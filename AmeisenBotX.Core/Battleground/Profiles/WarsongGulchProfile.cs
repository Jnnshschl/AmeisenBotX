using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.States;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Battleground.Profiles
{
    public class WarsongGulchProfile : IBattlegroundProfile
    {
        public WarsongGulchProfile(bool isAlliance, HookManager hookManager, ObjectManager objectManager, IMovementEngine movementEngine, BattlegroundEngine battlegroundEngine)
        {
            ObjectManager = objectManager;
            IsAlliance = isAlliance;
            IsMeFlagCarrier = false;

            if (isAlliance)
            {
                WsgDataset = new AllianceWsgDataset();
            }
            else
            {
                WsgDataset = new HordeWsgDataset();
            }

            States = new Dictionary<BattlegroundState, BasicBattlegroundState>()
            {
                { BattlegroundState.WaitingForStart, new WaitingForStartBgState(battlegroundEngine) },
                { BattlegroundState.MoveToEnemyBase, new MoveToEnemyBaseBgState(battlegroundEngine, objectManager, movementEngine, WsgDataset.EnemyFlagPosition) },
                { BattlegroundState.MoveToOwnBase, new MoveToOwnBaseBgState(battlegroundEngine, objectManager, movementEngine, WsgDataset.OwnFlagPosition) },
                { BattlegroundState.MoveToEnemyFlagCarrier, new MoveToEnemyFlagCarrierBgState(battlegroundEngine, objectManager, movementEngine, hookManager) },
                { BattlegroundState.AssistOwnFlagCarrier, new AssistOwnFlagCarrierBgState(battlegroundEngine, objectManager, movementEngine, hookManager) },
                { BattlegroundState.DefendMyself, new DefendMyselfBgState(battlegroundEngine, objectManager) },
                { BattlegroundState.PickupEnemyFlag, new PickupEnemyFlagBgState(battlegroundEngine, objectManager, movementEngine, hookManager) },
                { BattlegroundState.PickupOwnFlag, new PickupOwnFlagBgState(battlegroundEngine, objectManager, movementEngine, hookManager) },
                { BattlegroundState.PickupBuff, new PickupBuffBgState(battlegroundEngine) },
                { BattlegroundState.ExitBattleground, new ExitBattlegroundBgState(battlegroundEngine) }
            };
        }

        private interface IWsgDataset
        {
            Vector3 EnemyFlagPosition { get; }

            Vector3 OwnFlagPosition { get; }
        }

        public BattlegroundType BattlegroundType { get; } = BattlegroundType.CaptureTheFlag;

        public Vector3 EnemyBasePosition => WsgDataset.EnemyFlagPosition;

        public WowPlayer EnemyFlagCarrier { get; private set; }

        public bool IsMeFlagCarrier { get; private set; }

        public Vector3 OwnBasePosition => WsgDataset.OwnFlagPosition;

        public WowPlayer OwnFlagCarrier { get; private set; }

        public Dictionary<BattlegroundState, BasicBattlegroundState> States { get; private set; }

        private bool IsAlliance { get; set; }

        private WowPlayer LastEnemyFlagCarrier { get; set; }

        private WowPlayer LastOwnFlagCarrier { get; set; }

        private ObjectManager ObjectManager { get; set; }

        private IWsgDataset WsgDataset { get; set; }

        public void AllianceFlagWasDropped(string playername) => UnsetFlagCarrier(IsAlliance, playername);

        public void AllianceFlagWasPickedUp(string playername) => SetFlagCarrier(IsAlliance, playername);

        public void HordeFlagWasDropped(string playername) => UnsetFlagCarrier(!IsAlliance, playername);

        public void HordeFlagWasPickedUp(string playername) => SetFlagCarrier(!IsAlliance, playername);

        private void SetFlagCarrier(bool own, string playername)
        {
            if (own)
            {
                OwnFlagCarrier = ObjectManager.GetWowPlayerByName(playername);
            }
            else
            {
                EnemyFlagCarrier = ObjectManager.GetWowPlayerByName(playername);
            }

            IsMeFlagCarrier = playername.ToUpper() == ObjectManager.Player.Name.ToUpper();
        }

        private void UnsetFlagCarrier(bool own, string playername)
        {
            if (own)
            {
                LastOwnFlagCarrier = OwnFlagCarrier;
                OwnFlagCarrier = null;
            }
            else
            {
                LastEnemyFlagCarrier = EnemyFlagCarrier;
                EnemyFlagCarrier = null;
            }
        }

        private class AllianceWsgDataset : IWsgDataset
        {
            public Vector3 EnemyFlagPosition { get; } = new Vector3(916, 1434, 346);

            public Vector3 OwnFlagPosition { get; } = new Vector3(1539, 1481, 352);
        }

        private class HordeWsgDataset : IWsgDataset
        {
            public Vector3 EnemyFlagPosition { get; } = new Vector3(1539, 1481, 352);

            public Vector3 OwnFlagPosition { get; } = new Vector3(916, 1434, 346);
        }
    }
}