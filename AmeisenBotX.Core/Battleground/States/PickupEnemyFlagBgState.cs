using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.Profiles;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Battleground.States
{
    public class PickupEnemyFlagBgState : BasicBattlegroundState
    {
        public PickupEnemyFlagBgState(BattlegroundEngine battlegroundEngine, WowInterface wowInterface) : base(battlegroundEngine)
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
                IEnumerable<WowGameobject> flags = BattlegroundEngine.GetBattlegroundFlags();
                if (flags.Count() > 0)
                {
                    if (!((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).IsMeFlagCarrier)
                    {
                        WowGameobject flagObject = flags.First();
                        if (flagObject != null && flagObject.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 8)
                        {
                            WowInterface.HookManager.WowObjectOnRightClick(flagObject);
                        }
                        else
                        {
                            WowInterface.MovementEngine.SetState(MovementEngineState.Moving, flagObject.Position);
                            WowInterface.MovementEngine.Execute();
                        }
                    }
                    else
                    {
                        BattlegroundEngine.SetState(BattlegroundState.MoveToOwnBase);
                    }
                }
                else
                {
                    BattlegroundEngine.SetState(BattlegroundState.DefendMyself);
                }
            }
        }

        public override void Exit()
        {
            WowInterface.MovementEngine.Reset();
        }
    }
}