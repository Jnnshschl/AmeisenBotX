using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Pathfinding.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground.States
{
    public class MoveToEnemyBaseBgState : BasicBattlegroundState
    {
        public MoveToEnemyBaseBgState(BattlegroundEngine battlegroundEngine, ObjectManager objectManager, IMovementEngine movementEngine, Vector3 enemyBasePosition) : base(battlegroundEngine)
        {
            ObjectManager = objectManager;
            MovementEngine = movementEngine;
            EnemyBasePosition = enemyBasePosition;
        }

        private Vector3 EnemyBasePosition { get; }

        private IMovementEngine MovementEngine { get; }

        private ObjectManager ObjectManager { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            // CTF flag priority
            if (BattlegroundEngine.BattlegroundProfile.BattlegroundType == BattlegroundType.CaptureTheFlag)
            {
                IEnumerable<WowGameobject> flags = BattlegroundEngine.GetBattlegroundFlags();
                if (flags.Count() > 0
                    && !((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).IsMeFlagCarrier)
                {
                    BattlegroundEngine.SetState(BattlegroundState.PickupEnemyFlag);
                    return;
                }
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

        public override void Exit()
        {
        }
    }
}