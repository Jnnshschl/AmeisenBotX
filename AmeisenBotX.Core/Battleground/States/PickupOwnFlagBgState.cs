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
    public class PickupOwnFlagBgState : BasicBattlegroundState
    {
        public PickupOwnFlagBgState(BattlegroundEngine battlegroundEngine, ObjectManager objectManager, IMovementEngine movementEngine, HookManager hookManager) : base(battlegroundEngine)
        {
            ObjectManager = objectManager;
            HookManager = hookManager;
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
                IEnumerable<WowGameobject> flags = BattlegroundEngine.GetBattlegroundFlags();
                if (flags.Count() > 0)
                {
                    if (!((ICtfBattlegroundProfile)BattlegroundEngine.BattlegroundProfile).IsMeFlagCarrier)
                    {
                        WowGameobject flagObject = flags.First();
                        if (flagObject != null && flagObject.Position.GetDistance(ObjectManager.Player.Position) < 8)
                        {
                            HookManager.RightClickObject(flagObject);
                        }
                        else
                        {
                            MovementEngine.SetState(MovementEngineState.Moving, flagObject.Position);
                            MovementEngine.Execute();
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
        }
    }
}