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
            if (!WowInterface.CharacterManager.Equipment.Items.Any(e => e.Value.MaxDurability > 0 && e.Value.Durability == 0))
            {
                WowInterface.CharacterManager.Equipment.Update();
                StateMachine.SetState(BotState.Idle);
                return;
            }

            WowUnit selectedUnit = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .FirstOrDefault(e => e.GetType() != typeof(WowPlayer)
                    && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, e) == WowUnitReaction.Friendly
                    && e.IsRepairVendor
                    && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 50);

            if (selectedUnit != null && !selectedUnit.IsDead)
            {
                if (!IsAtNpc)
                {
                    double distance = WowInterface.ObjectManager.Player.Position.GetDistance(selectedUnit.Position);
                    if (distance > 5.0)
                    {
                        WowInterface.MovementEngine.SetState(MovementEngineState.Moving, selectedUnit.Position);
                        WowInterface.MovementEngine.Execute();
                    }
                    else
                    {
                        if (distance > 3)
                        {
                            WowInterface.CharacterManager.InteractWithUnit(selectedUnit, 20.9f, 0.2f);
                        }
                        else
                        {
                            WowInterface.HookManager.UnitOnRightClick(selectedUnit);
                            RepairActionGo = DateTime.Now + TimeSpan.FromSeconds(1);
                            IsAtNpc = true;
                        }
                    }
                }
                else if (DateTime.Now > RepairActionGo)
                {
                    WowInterface.HookManager.RepairAllItems();
                    WowInterface.HookManager.SellAllGrayItems();
                    WowInterface.CharacterManager.Equipment.Update();
                }
            }
            else
            {
                WowInterface.CharacterManager.Equipment.Update();
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}