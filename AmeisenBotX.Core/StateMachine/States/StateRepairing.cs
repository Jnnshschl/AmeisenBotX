using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.StateMachine.Routines;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateRepairing : BasicState
    {
        public StateRepairing(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            InteractionEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));
            EquipmentUpdateEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
        }

        private TimegatedEvent EquipmentUpdateEvent { get; }

        private TimegatedEvent InteractionEvent { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (EquipmentUpdateEvent.Run())
            {
                WowInterface.CharacterManager.Equipment.Update();
            }

            if (!NeedToRepair())
            {
                WowInterface.HookManager.LuaClickUiElement("MerchantFrameCloseButton");
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (IsRepairNpcNear(out WowUnit selectedUnit))
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(selectedUnit.Position) > 3.0f)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, selectedUnit.Position);
                }
                else if (InteractionEvent.Run() && SpeakToMerchantRoutine.Run(WowInterface, selectedUnit))
                {
                    WowInterface.I.MovementEngine.StopMovement();

                    if (Config.AutoRepair && WowInterface.ObjectManager.Target.IsRepairVendor)
                    {
                        WowInterface.HookManager.LuaRepairAllItems();
                    }

                    if (Config.AutoSell)
                    {
                        SellItemsRoutine.Run(WowInterface, Config);
                    }
                }
            }
        }

        public bool IsRepairNpcNear(out WowUnit unit)
        {
            unit = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .FirstOrDefault(e => e.GetType() != typeof(WowPlayer)
                              && !e.IsDead
                              && e.IsRepairVendor
                              && WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Hostile
                              && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.RepairNpcSearchRadius);

            return unit != null;
        }

        public override void Leave()
        {
        }

        internal bool NeedToRepair()
        {
            return WowInterface.CharacterManager.Equipment.Items
                       .Any(e => e.Value.MaxDurability > 0 && ((double)e.Value.Durability / (double)e.Value.MaxDurability * 100.0) <= Config.ItemRepairThreshold)
                   && IsRepairNpcNear(out _);
        }
    }
}