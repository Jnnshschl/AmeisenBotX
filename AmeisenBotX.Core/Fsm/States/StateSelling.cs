using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Fsm.Routines;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    public class StateSelling : BasicState
    {
        public StateSelling(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            InteractionEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));
            InventoryUpdateEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
        }

        private TimegatedEvent InteractionEvent { get; }

        private TimegatedEvent InventoryUpdateEvent { get; }

        private DateTime SellingFinished { get; set; }

        public override void Enter()
        {
            WowInterface.EventHookManager.Subscribe("MERCHANT_SHOW", OnMerchantShow);
        }

        public override void Execute()
        {
            if (InventoryUpdateEvent.Run())
            {
                WowInterface.CharacterManager.Inventory.Update();
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
                float distance = WowInterface.ObjectManager.Player.Position.GetDistance(selectedUnit.Position);

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

        public bool IsVendorNpcNear(out WowUnit unit)
        {
            unit = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                .Where(e => e.GetType() != typeof(WowPlayer)
                    && !e.IsDead
                    && e.IsVendor
                    && WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Hostile
                    && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.RepairNpcSearchRadius)
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .FirstOrDefault();

            return unit != null;
        }

        public override void Leave()
        {
            WowInterface.EventHookManager.Unsubscribe("MERCHANT_SHOW", OnMerchantShow);

            WowInterface.HookManager.WowClearTarget();
            WowInterface.HookManager.LuaClickUiElement("MerchantFrameCloseButton");
        }

        internal bool NeedToSell()
        {
            return WowInterface.CharacterManager.Inventory.FreeBagSlots < Config.BagSlotsToGoSell
                && WowInterface.CharacterManager.Inventory.Items.Where(e => !Config.ItemSellBlacklist.Contains(e.Name)
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
            if (WowInterface.ObjectManager.Target != null)
            {
                if (Config.AutoRepair && WowInterface.ObjectManager.Target.IsRepairVendor)
                {
                    WowInterface.HookManager.LuaRepairAllItems();
                }

                if (Config.AutoSell)
                {
                    SellItemsRoutine.Run(WowInterface, Config);
                    SellingFinished = DateTime.UtcNow;
                }
            }
        }
    }
}