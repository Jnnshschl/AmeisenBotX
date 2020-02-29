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

                if (((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).OwnFlagCarrierPlayer != null)
                {
                    ulong ownFlagCarrierGuid = ((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).OwnFlagCarrierPlayer.Guid;
                    WowPlayer ownFlagCarrier = ObjectManager.GetWowObjectByGuid<WowPlayer>(ownFlagCarrierGuid);
                    IEnumerable<WowPlayer> nearEnemies = ObjectManager.GetNearEnemies(ownFlagCarrier.Position, 25);

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
                            HookManager.FacePosition(ObjectManager.Player, target.Position);
                            BattlegroundEngine.ForceCombat = true;
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
            MovementEngine.Reset();
            BattlegroundEngine.ForceCombat = false;
        }
    }
}