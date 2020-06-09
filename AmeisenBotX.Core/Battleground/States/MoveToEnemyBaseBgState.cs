using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Battleground.States
{
    public class MoveToEnemyBaseBgState : BasicBattlegroundState
    {
        public MoveToEnemyBaseBgState(BattlegroundEngine battlegroundEngine, WowInterface wowInterface, Vector3 enemyBasePosition) : base(battlegroundEngine)
        {
            WowInterface = wowInterface;
            EnemyBasePosition = enemyBasePosition;
        }

        private Vector3 EnemyBasePosition { get; }

        private WowInterface WowInterface { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            // CTF flag priority
            if (BattlegroundEngine.BattlegroundProfile.BattlegroundType == BattlegroundType.CaptureTheFlag)
            {
                if (BattlegroundEngine.BattlegroundProfile.HanldeInterruptStates())
                {
                    return;
                }

                if (BattlegroundEngine.AttackNearEnemies())
                {
                    BattlegroundEngine.ForceCombat = true;
                    return;
                }

                if (WowInterface.ObjectManager.Player.Position.GetDistance(EnemyBasePosition) > 5)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, EnemyBasePosition);
                    WowInterface.MovementEngine.Execute();
                    return;
                }
                else
                {
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