using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Logic.Enums;
using AmeisenBotX.Core.Logic.Routines;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Logic.States
{
    public class StateRepairing : BasicState
    {
        public StateRepairing(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
            InteractionEvent = new(TimeSpan.FromMilliseconds(1000));
            EquipmentUpdateEvent = new(TimeSpan.FromSeconds(1));
        }

        private TimegatedEvent EquipmentUpdateEvent { get; }

        private TimegatedEvent InteractionEvent { get; }

        private DateTime SellingFinished { get; set; }

        public override void Enter()
        {
            Bot.Wow.Events.Subscribe("MERCHANT_SHOW", OnMerchantShow);
        }

        public override void Execute()
        {
            if (EquipmentUpdateEvent.Run())
            {
                Bot.Character.Equipment.Update();
            }

            if (!NeedToRepair())
            {
                if (DateTime.UtcNow - SellingFinished > TimeSpan.FromSeconds(8))
                {
                    StateMachine.SetState(BotState.Idle);
                }

                return;
            }

            if (IsRepairNpcNear(out IWowUnit selectedUnit))
            {
                float distance = Bot.Player.Position.GetDistance(selectedUnit.Position);

                if (distance > 3.0f)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, selectedUnit.Position);
                }
                else if (distance < 2.25f)
                {
                    Bot.Movement.StopMovement();

                    if (InteractionEvent.Run())
                    {
                        SpeakToMerchantRoutine.Run(Bot, selectedUnit);
                    }
                }
            }
        }

        public bool IsRepairNpcNear(out IWowUnit unit)
        {
            unit = Bot.Objects.WowObjects.OfType<IWowUnit>()
                .FirstOrDefault(e => e.GetType() != typeof(IWowPlayer)
                    && !e.IsDead
                    && e.IsRepairVendor
                    && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Hostile
                    && e.Position.GetDistance(Bot.Player.Position) < Config.RepairNpcSearchRadius);

            return unit != null;
        }

        public override void Leave()
        {
            Bot.Wow.Events.Unsubscribe("MERCHANT_SHOW", OnMerchantShow);
        }

        internal bool NeedToRepair()
        {
            return Bot.Character.Equipment.Items
                       .Any(e => e.Value.MaxDurability > 0 && (e.Value.Durability / (double)e.Value.MaxDurability * 100.0) <= Config.ItemRepairThreshold)
                   && IsRepairNpcNear(out _);
        }

        private void OnMerchantShow(long timestamp, List<string> args)
        {
            if (Config.AutoRepair && Bot.Target.IsRepairVendor)
            {
                Bot.Wow.RepairAllItems();
            }

            if (Config.AutoSell)
            {
                SellItemsRoutine.Run(Bot, Config);
                SellingFinished = DateTime.UtcNow;
            }
        }
    }
}