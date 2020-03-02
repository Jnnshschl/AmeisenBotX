using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground.States
{
    public class AssistOwnFlagCarrierBgState : BasicBattlegroundState
    {
        public AssistOwnFlagCarrierBgState(BattlegroundEngine battlegroundEngine, WowInterface wowInterface) : base(battlegroundEngine)
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

                if (((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).OwnFlagCarrierPlayer != null)
                {
                    ulong ownFlagCarrierGuid = ((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).OwnFlagCarrierPlayer.Guid;
                    WowPlayer ownFlagCarrier = WowInterface.ObjectManager.GetWowObjectByGuid<WowPlayer>(ownFlagCarrierGuid);
                    IEnumerable<WowPlayer> nearEnemies = WowInterface.ObjectManager.GetNearEnemies(ownFlagCarrier.Position, 25);

                    if (WowInterface.ObjectManager.Player.Position.GetDistance(ownFlagCarrier.Position) > 10)
                    {
                        WowInterface.MovementEngine.SetState(MovementEngineState.Moving, ownFlagCarrier.Position);
                        WowInterface.MovementEngine.Execute();
                    }
                    else
                    {
                        WowPlayer target = nearEnemies.FirstOrDefault();
                        if (target != null)
                        {
                            WowInterface.HookManager.TargetGuid(target.Guid);
                            WowInterface.HookManager.StartAutoAttack();
                            WowInterface.HookManager.FacePosition(WowInterface.ObjectManager.Player, target.Position);
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
            WowInterface.MovementEngine.Reset();
            BattlegroundEngine.ForceCombat = false;
        }
    }
}