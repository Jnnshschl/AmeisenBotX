using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.SMovementEngine.Enums;
using AmeisenBotX.Core.StateMachine.Routines;
using System;
using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Core.Data.Db.Objects;
using AmeisenBotX.Core.Data.Db.Structs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateSelling : BasicState
    {
        public StateSelling(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            Blacklist = new List<ulong>();
            InteractionEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));
            InventoryUpdateEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
        }

        public List<ulong> Blacklist { get; }

        private int BlacklistCounter { get; set; }

        private TimegatedEvent InteractionEvent { get; }

        private TimegatedEvent InventoryUpdateEvent { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (InventoryUpdateEvent.Run())
            {
                WowInterface.CharacterManager.Inventory.Update();
            }

            if (!NeedToSell())
            {
                WowInterface.HookManager.LuaClickUiElement("MerchantFrameCloseButton");
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (IsVendorNpcNear(out WowUnit selectedUnit))
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(selectedUnit.Position) > 3.5)
                {
                    if (!WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, selectedUnit.Position))
                    {
                        ++BlacklistCounter;

                        if (BlacklistCounter > 2)
                        {
                            WowInterface.MovementEngine.StopMovement();
                            Blacklist.Add(selectedUnit.Guid);
                            BlacklistCounter = 0;
                            return;
                        }
                    }
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

        public bool IsVendorNpcNear(out WowUnit unit)
        {
            unit = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .Where(e => e.GetType() != typeof(WowPlayer)
                              && !Blacklist.Contains(e.Guid)
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
        }

        internal bool NeedToSell()
        {
            return WowInterface.CharacterManager.Inventory.FreeBagSlots < Config.BagSlotsToGoSell
                && WowInterface.CharacterManager.Inventory.Items.Where(e => !Config.ItemSellBlacklist.Contains(e.Name)
                       && ((Config.SellGrayItems && e.ItemQuality == ItemQuality.Poor)
                           || (Config.SellWhiteItems && e.ItemQuality == ItemQuality.Common)
                           || (Config.SellGreenItems && e.ItemQuality == ItemQuality.Uncommon)
                           || (Config.SellBlueItems && e.ItemQuality == ItemQuality.Rare)
                           || (Config.SellPurpleItems && e.ItemQuality == ItemQuality.Epic)))
                   .Any(e => e.Price > 0);
        }
    }
}