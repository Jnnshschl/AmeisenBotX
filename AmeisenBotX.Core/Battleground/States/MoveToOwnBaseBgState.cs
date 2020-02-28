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
    public class MoveToOwnBaseBgState : BasicBattlegroundState
    {
        public MoveToOwnBaseBgState(BattlegroundEngine battlegroundEngine, ObjectManager objectManager, IMovementEngine movementEngine, Vector3 ownBasePosition) : base(battlegroundEngine)
        {
            ObjectManager = objectManager;
            MovementEngine = movementEngine;
            OwnBasePosition = ownBasePosition;
        }

        private IMovementEngine MovementEngine { get; }

        private ObjectManager ObjectManager { get; }

        private Vector3 OwnBasePosition { get; }

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

                if (ObjectManager.Player.Position.GetDistance(OwnBasePosition) > 5)
                {
                    MovementEngine.SetState(MovementEngineState.Moving, OwnBasePosition);
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
        }
    }
}