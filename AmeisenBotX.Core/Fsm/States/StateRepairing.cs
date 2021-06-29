using AmeisenBotX.Common.Utils;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Fsm.Routines;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateRepairing : BasicState
    {
        public StateRepairing(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            InteractionEvent = new(TimeSpan.FromMilliseconds(1000));
            EquipmentUpdateEvent = new(TimeSpan.FromSeconds(1));
        }

        private TimegatedEvent EquipmentUpdateEvent { get; }

        private TimegatedEvent InteractionEvent { get; }

        private DateTime SellingFinished { get; set; }

        public override void Enter()
        {
            WowInterface.EventHookManager.Subscribe("MERCHANT_SHOW", OnMerchantShow);
        }

        public override void Execute()
        {
            if (EquipmentUpdateEvent.Run())
            {
                WowInterface.CharacterManager.Equipment.Update();
            }

            if (!NeedToRepair())
            {
                if (DateTime.UtcNow - SellingFinished > TimeSpan.FromSeconds(8))
                {
                    StateMachine.SetState(BotState.Idle);
                }

                return;
            }

            if (IsRepairNpcNear(out WowUnit selectedUnit))
            {
                float distance = WowInterface.Player.Position.GetDistance(selectedUnit.Position);

                if (distance > 3.0f)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, selectedUnit.Position);
                }
                else if (distance < 2.25f)
                {
                    WowInterface.MovementEngine.StopMovement();

                    if (InteractionEvent.Run())
                    {
                        SpeakToMerchantRoutine.Run(WowInterface, selectedUnit);
                    }
                }
            }
        }

        public bool IsRepairNpcNear(out WowUnit unit)
        {
            unit = WowInterface.Objects.WowObjects.OfType<WowUnit>()
                .FirstOrDefault(e => e.GetType() != typeof(WowPlayer)
                    && !e.IsDead
                    && e.IsRepairVendor
                    && WowInterface.NewWowInterface.GetReaction(WowInterface.Player.BaseAddress, e.BaseAddress) != WowUnitReaction.Hostile
                    && e.Position.GetDistance(WowInterface.Player.Position) < Config.RepairNpcSearchRadius);

            return unit != null;
        }

        public override void Leave()
        {
            WowInterface.EventHookManager.Unsubscribe("MERCHANT_SHOW", OnMerchantShow);
        }

        internal bool NeedToRepair()
        {
            return WowInterface.CharacterManager.Equipment.Items
                       .Any(e => e.Value.MaxDurability > 0 && ((double)e.Value.Durability / (double)e.Value.MaxDurability * 100.0) <= Config.ItemRepairThreshold)
                   && IsRepairNpcNear(out _);
        }

        private void OnMerchantShow(long timestamp, List<string> args)
        {
            if (Config.AutoRepair && WowInterface.Target.IsRepairVendor)
            {
                WowInterface.NewWowInterface.LuaRepairAllItems();
            }

            if (Config.AutoSell)
            {
                SellItemsRoutine.Run(WowInterface, Config);
                SellingFinished = DateTime.UtcNow;
            }
        }
    }
}