using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateRepairing : BasicState
    {
        public StateRepairing(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        private bool IsAtNpc { get; set; }

        private DateTime RepairActionGo { get; set; }

        public override void Enter()
        {
            IsAtNpc = false;
        }

        public override void Execute()
        {
            if (!WowInterface.CharacterManager.Equipment.Items.Any(e => e.Value.MaxDurability > 0 && ((double)e.Value.Durability * (double)e.Value.MaxDurability * 100.0) <= Config.ItemRepairThreshold))
            {
                WowInterface.CharacterManager.Equipment.Update();
                StateMachine.SetState((int)BotState.Idle);
                return;
            }

            WowUnit selectedUnit = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .FirstOrDefault(e => e.GetType() != typeof(WowPlayer)
                    && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Hostile
                    && e.IsRepairVendor
                    && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 50);

            if (selectedUnit != null && !selectedUnit.IsDead)
            {
                if (!IsAtNpc)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, selectedUnit.Position);

                    if (WowInterface.MovementEngine.IsAtTargetPosition)
                    {
                        WowInterface.HookManager.UnitOnRightClick(selectedUnit);
                        RepairActionGo = DateTime.Now + TimeSpan.FromSeconds(1);
                        IsAtNpc = true;
                    }
                }
                else if (DateTime.Now > RepairActionGo)
                {
                    WowInterface.HookManager.RepairAllItems();
                    WowInterface.CharacterManager.Equipment.Update();
                }
            }
            else
            {
                WowInterface.CharacterManager.Equipment.Update();
                StateMachine.SetState((int)BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}