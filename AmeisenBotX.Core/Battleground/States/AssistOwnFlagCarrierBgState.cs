using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground.States
{
    public class AssistOwnFlagCarrierBgState : BasicBattlegroundState
    {
        public AssistOwnFlagCarrierBgState(BattlegroundEngine battlegroundEngine, ObjectManager objectManager, IMovementEngine movementEngine, HookManager hookManager) : base(battlegroundEngine)
        {
            ObjectManager = objectManager;
            MovementEngine = movementEngine;
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
                WowPlayer ownFlagCarrier = ((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).OwnFlagCarrierPlayer;
                IEnumerable<WowPlayer> nearEnemies = ObjectManager.GetNearEnemies(ownFlagCarrier.Position, 25);

                if (ownFlagCarrier != null
                    && nearEnemies.Count() > 1)
                {
                    if (ObjectManager.Player.Position.GetDistance(ownFlagCarrier.Position) > 10)
                    {
                        MovementEngine.SetState(MovementEngineState.Moving, ownFlagCarrier.Position);
                        MovementEngine.Execute();
                    }
                    else
                    {
                        WowPlayer target = nearEnemies.FirstOrDefault();
                        if (target != null)
                        {
                            HookManager.TargetGuid(target.Guid);
                            HookManager.StartAutoAttack();
                        }
                    }
                }
                else
                {
                    // there is no friendly flag carrier
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