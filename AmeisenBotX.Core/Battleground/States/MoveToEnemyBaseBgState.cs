using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Pathfinding.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground.States
{
    public class MoveToEnemyBaseBgState : BasicBattlegroundState
    {
        public MoveToEnemyBaseBgState(BattlegroundEngine battlegroundEngine, ObjectManager objectManager, HookManager hookManager, IMovementEngine movementEngine, Vector3 enemyBasePosition) : base(battlegroundEngine)
        {
            ObjectManager = objectManager;
            MovementEngine = movementEngine;
            EnemyBasePosition = enemyBasePosition;
            HookManager = hookManager;
        }

        private Vector3 EnemyBasePosition { get; }

        private IMovementEngine MovementEngine { get; }

        private ObjectManager ObjectManager { get; }

        private HookManager HookManager { get; }

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

                if (ObjectManager.Player.Position.GetDistance(EnemyBasePosition) > 5)
                {
                    MovementEngine.SetState(MovementEngineState.Moving, EnemyBasePosition);
                    MovementEngine.Execute();
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
            MovementEngine.Reset();
            BattlegroundEngine.ForceCombat = false;
        }
    }
}