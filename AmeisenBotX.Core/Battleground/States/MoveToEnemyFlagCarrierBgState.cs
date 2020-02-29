using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;

namespace AmeisenBotX.Core.Battleground.States
{
    public class MoveToEnemyFlagCarrierBgState : BasicBattlegroundState
    {
        public MoveToEnemyFlagCarrierBgState(BattlegroundEngine battlegroundEngine, ObjectManager objectManager, IMovementEngine movementEngine, HookManager hookManager) : base(battlegroundEngine)
        {
            ObjectManager = objectManager;
            MovementEngine = movementEngine;
            HookManager = hookManager;
        }

        private HookManager HookManager { get; }

        private IMovementEngine MovementEngine { get; }

        private ObjectManager ObjectManager { get; }

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
                    WowPlayer enemyFlagCarrier = ObjectManager.GetWowObjectByGuid<WowPlayer>(enemyFlagCarrierGuid);

                    if (ObjectManager.Player.Position.GetDistance(enemyFlagCarrier.Position) > 10)
                    {
                        MovementEngine.SetState(MovementEngineState.Moving, enemyFlagCarrier.Position);
                        MovementEngine.Execute();
                    }
                    else
                    {
                        if (enemyFlagCarrier != null)
                        {
                            HookManager.TargetGuid(enemyFlagCarrier.Guid);
                            HookManager.StartAutoAttack();
                            HookManager.FacePosition(ObjectManager.Player, target.Position);
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
            MovementEngine.Reset();
            BattlegroundEngine.ForceCombat = false;
        }
    }
}