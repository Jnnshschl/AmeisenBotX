using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Pathfinding.Objects;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateGhost : BasicState
    {
        public StateGhost(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
            WowUnit spiritHealer = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Name.ToUpper().Contains("SPIRIT HEALER"));

            if (spiritHealer != null)
            {
                WowInterface.HookManager.RightClickUnit(spiritHealer);
            }
        }

        public override void Execute()
        {
            if (WowInterface.ObjectManager.Player.Health > 1)
            {
                StateMachine.SetState(BotState.Idle);
            }

            if (StateMachine.IsOnBattleground())
            {
                // just wait for the mass ress
                return;
            }

            if (WowInterface.XMemory.ReadStruct(WowInterface.OffsetList.CorpsePosition, out Vector3 corpsePosition)
                && WowInterface.ObjectManager.Player.Position.GetDistance(corpsePosition) > 16)
            {
                WowInterface.MovementEngine.SetState(MovementEngineState.Moving, corpsePosition);
                WowInterface.MovementEngine.Execute();
            }
            else
            {
                WowInterface.HookManager.RetrieveCorpse();
            }
        }

        public override void Exit()
        {
        }
    }
}