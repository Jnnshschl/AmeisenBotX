using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.States;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Pathfinding.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground.Profiles
{
    public class WarsongGulchProfile : ICtfBattlegroundProfile
    {
        public WarsongGulchProfile(WowInterface wowInterface, BattlegroundEngine battlegroundEngine)
        {
            WowInterface = wowInterface;
            IsAlliance = wowInterface.ObjectManager.Player.IsAlliance();
            BattlegroundEngine = battlegroundEngine;
            IsMeFlagCarrier = false;

            if (IsAlliance)
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
                { BattlegroundState.MoveToEnemyBase, new MoveToEnemyBaseBgState(battlegroundEngine,WowInterface, WsgDataset.EnemyFlagPosition) },
                { BattlegroundState.MoveToOwnBase, new MoveToOwnBaseBgState(battlegroundEngine, WowInterface, WsgDataset.OwnFlagPosition) },
                { BattlegroundState.MoveToEnemyFlagCarrier, new MoveToEnemyFlagCarrierBgState(battlegroundEngine, WowInterface) },
                { BattlegroundState.AssistOwnFlagCarrier, new AssistOwnFlagCarrierBgState(battlegroundEngine, WowInterface) },
                { BattlegroundState.DefendMyself, new DefendMyselfBgState(battlegroundEngine, WowInterface) },
                { BattlegroundState.PickupEnemyFlag, new PickupEnemyFlagBgState(battlegroundEngine, WowInterface) },
                { BattlegroundState.PickupOwnFlag, new PickupOwnFlagBgState(battlegroundEngine, WowInterface) },
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

        public WowPlayer EnemyFlagCarrierPlayer { get; private set; }

        public bool IsBattlegroundRunning => IsAlliance;

        public bool IsMeFlagCarrier { get; private set; }

        public Vector3 OwnBasePosition => WsgDataset.OwnFlagPosition;

        public WowPlayer OwnFlagCarrierPlayer { get; private set; }

        public Dictionary<BattlegroundState, BasicBattlegroundState> States { get; private set; }

        private BattlegroundEngine BattlegroundEngine { get; set; }

        private bool IsAlliance { get; set; }

        private WowPlayer LastEnemyFlagCarrier { get; set; }

        private WowPlayer LastOwnFlagCarrier { get; set; }

        private WowInterface WowInterface { get; }

        private IWsgDataset WsgDataset { get; }

        public void AllianceFlagWasDropped(string playername) => UnsetFlagCarrier(!IsAlliance, playername);

        public void AllianceFlagWasPickedUp(string playername) => SetFlagCarrier(!IsAlliance, playername);

        public bool HanldeInterruptStates()
        {
            IEnumerable<WowGameobject> enemyflags = BattlegroundEngine.GetBattlegroundFlags();
            if (enemyflags.Count() > 0
                && !((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).IsMeFlagCarrier)
            {
                BattlegroundEngine.SetState(BattlegroundState.PickupEnemyFlag);
                return true;
            }

            IEnumerable<WowGameobject> ownFlags = BattlegroundEngine.GetBattlegroundFlags(false);
            if (ownFlags.Count() > 0
                && !((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).IsMeFlagCarrier
                && ownFlags.FirstOrDefault()?.Position.GetDistance(WsgDataset.OwnFlagPosition) > 4)
            {
                BattlegroundEngine.SetState(BattlegroundState.PickupOwnFlag);
                return true;
            }

            return false;
        }

        public void HordeFlagWasDropped(string playername) => UnsetFlagCarrier(IsAlliance, playername);

        public void HordeFlagWasPickedUp(string playername) => SetFlagCarrier(IsAlliance, playername);

        private void SetFlagCarrier(bool own, string playername)
        {
            if (own)
            {
                OwnFlagCarrierPlayer = WowInterface.ObjectManager.GetWowPlayerByName(playername);
                IsMeFlagCarrier = playername.ToUpper() == WowInterface.ObjectManager.Player.Name.ToUpper();
            }
            else
            {
                EnemyFlagCarrierPlayer = WowInterface.ObjectManager.GetWowPlayerByName(playername);
            }
        }

        private void UnsetFlagCarrier(bool own, string playername)
        {
            if (own)
            {
                LastOwnFlagCarrier = OwnFlagCarrierPlayer;
                OwnFlagCarrierPlayer = null;
                IsMeFlagCarrier = false;
            }
            else
            {
                LastEnemyFlagCarrier = EnemyFlagCarrierPlayer;
                EnemyFlagCarrierPlayer = null;
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