using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Battleground.States
{
    public class MoveToOwnBaseBgState : BasicBattlegroundState
    {
        public MoveToOwnBaseBgState(BattlegroundEngine battlegroundEngine, WowInterface wowInterface, Vector3 ownBasePosition) : base(battlegroundEngine)
        {
            WowInterface = wowInterface;
            OwnBasePosition = ownBasePosition;
        }

        private Vector3 OwnBasePosition { get; }

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

                if (BattlegroundEngine.AttackNearEnemies())
                {
                    BattlegroundEngine.ForceCombat = true;
                    return;
                }

                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, OwnBasePosition);

                if (WowInterface.MovementEngine.IsAtTargetPosition)
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