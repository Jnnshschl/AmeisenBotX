using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Fsm.Routines;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateSelling : BasicState
    {
        public StateSelling(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
            InteractionEvent = new(TimeSpan.FromMilliseconds(1000));
            InventoryUpdateEvent = new(TimeSpan.FromSeconds(1));
        }

        private TimegatedEvent InteractionEvent { get; }

        private TimegatedEvent InventoryUpdateEvent { get; }

        private DateTime SellingFinished { get; set; }

        public override void Enter()
        {
            Bot.Events.Subscribe("MERCHANT_SHOW", OnMerchantShow);
        }

        public override void Execute()
        {
            if (InventoryUpdateEvent.Run())
            {
                Bot.Character.Inventory.Update();
            }

            if (!NeedToSell())
            {
                if (DateTime.UtcNow - SellingFinished > TimeSpan.FromSeconds(8))
                {
                    StateMachine.SetState(BotState.Idle);
                }

                return;
            }

            if (IsVendorNpcNear(out WowUnit selectedUnit))
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

        public bool IsVendorNpcNear(out WowUnit unit)
        {
            unit = Bot.Objects.WowObjects.OfType<WowUnit>()
                .Where(e => e.GetType() != typeof(WowPlayer)
                    && !e.IsDead
                    && e.IsVendor
                    && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Hostile
                    && e.Position.GetDistance(Bot.Player.Position) < Config.RepairNpcSearchRadius)
                .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                .FirstOrDefault();

            return unit != null;
        }

        public override void Leave()
        {
            Bot.Events.Unsubscribe("MERCHANT_SHOW", OnMerchantShow);

            Bot.Wow.WowClearTarget();
            Bot.Wow.LuaClickUiElement("MerchantFrameCloseButton");
        }

        internal bool NeedToSell()
        {
            return Bot.Character.Inventory.FreeBagSlots < Config.BagSlotsToGoSell
                && Bot.Character.Inventory.Items.Where(e => !Config.ItemSellBlacklist.Contains(e.Name)
                    && ((Config.SellGrayItems && e.ItemQuality == WowItemQuality.Poor)
                        || (Config.SellWhiteItems && e.ItemQuality == WowItemQuality.Common)
                        || (Config.SellGreenItems && e.ItemQuality == WowItemQuality.Uncommon)
                        || (Config.SellBlueItems && e.ItemQuality == WowItemQuality.Rare)
                        || (Config.SellPurpleItems && e.ItemQuality == WowItemQuality.Epic)))
                   .Any(e => e.Price > 0)
                && IsVendorNpcNear(out _);
        }

        private void OnMerchantShow(long timestamp, List<string> args)
        {
            if (Bot.Target != null)
            {
                if (Config.AutoRepair && Bot.Target.IsRepairVendor)
                {
                    Bot.Wow.LuaRepairAllItems();
                }

                if (Config.AutoSell)
                {
                    SellItemsRoutine.Run(Bot, Config);
                    SellingFinished = DateTime.UtcNow;
                }
            }
        }
    }
}