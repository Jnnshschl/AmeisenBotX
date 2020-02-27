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
                WowPlayer enemyFlagCarrier = ((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).EnemyFlagCarrierPlayer;

                if (enemyFlagCarrier != null)
                {
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
        }
    }
}