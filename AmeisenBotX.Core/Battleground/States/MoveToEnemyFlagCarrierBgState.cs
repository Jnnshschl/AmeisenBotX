using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;

namespace AmeisenBotX.Core.Battleground.States
{
    public class MoveToEnemyFlagCarrierBgState : BasicBattlegroundState
    {
        public MoveToEnemyFlagCarrierBgState(BattlegroundEngine battlegroundEngine, WowInterface wowInterface) : base(battlegroundEngine)
        {
            WowInterface = wowInterface;
        }

        private WowInterface WowInterface { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (BattlegroundEngine.BattlegroundProfile.BattlegroundType == BattlegroundType.CaptureTheFlag)
            {
                if (BattlegroundEngine.BattlegroundProfile.HanldeInterruptStates())
                {
                    return;
                }

                if (((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).EnemyFlagCarrierPlayer != null)
                {
                    ulong enemyFlagCarrierGuid = ((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).EnemyFlagCarrierPlayer.Guid;
                    WowPlayer enemyFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowPlayer>(enemyFlagCarrierGuid);

                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, enemyFlagCarrier.Position);

                    if (WowInterface.MovementEngine.IsAtTargetPosition)
                    {
                        if (enemyFlagCarrier != null)
                        {
                            WowInterface.HookManager.TargetGuid(enemyFlagCarrier.Guid);
                            WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
                            WowInterface.HookManager.FacePosition(WowInterface.ObjectManager.Player, enemyFlagCarrier.Position);
                            BattlegroundEngine.ForceCombat = true;
                        }
                    }
                }
                else
                {
                    // there is no enemy flag carrier
                    BattlegroundEngine.SetState(BattlegroundState.DefendMyself);
                    return;
                }
            }
        }

        public override void Exit()
        {
            WowInterface.MovementEngine.Reset();
            BattlegroundEngine.ForceCombat = false;
        }
    }
}