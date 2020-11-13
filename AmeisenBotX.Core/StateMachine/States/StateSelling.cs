using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.SMovementEngine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateSelling : BasicState
    {
        public StateSelling(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            Blacklist = new List<ulong>();
            InteractionEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(2000));
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
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (IsVendorNpcNear(out WowUnit selectedUnit))
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(selectedUnit.Position) > 1.5)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, selectedUnit.Position);

                    if (WowInterface.MovementEngine.PathfindingStatus == PathfindingStatus.PathIncomplete)
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
                else if (InteractionEvent.Run())
                {
                    if (WowInterface.ObjectManager.TargetGuid != selectedUnit.Guid)
                    {
                        WowInterface.HookManager.WowTargetGuid(selectedUnit.Guid);
                        return;
                    }

                    WowInterface.MovementEngine.StopMovement();

                    if (!BotMath.IsFacing(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, selectedUnit.Position))
                    {
                        WowInterface.HookManager.WowFacePosition(WowInterface.ObjectManager.Player, selectedUnit.Position);
                        return;
                    }

                    // WowInterface.HookManager.UnitOnRightClick(selectedUnit);
                    WowInterface.CharacterManager.ClickToMove(selectedUnit.Position, selectedUnit.Guid, Character.Enums.ClickToMoveType.Interact, 20.9f, 1.5f);

                    if (Config.AutoRepair && WowInterface.ObjectManager.Target.IsRepairVendor)
                    {
                        WowInterface.HookManager.LuaRepairAllItems();
                    }

                    if (Config.AutoSell)
                    {
                        foreach (IWowItem item in WowInterface.CharacterManager.Inventory.Items.Where(e => e.Price > 0))
                        {
                            IWowItem itemToSell = item;

                            if (Config.ItemSellBlacklist.Any(e => e.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                                || (!Config.SellGrayItems && item.ItemQuality == ItemQuality.Poor)
                                || (!Config.SellWhiteItems && item.ItemQuality == ItemQuality.Common)
                                || (!Config.SellGreenItems && item.ItemQuality == ItemQuality.Uncommon)
                                || (!Config.SellBlueItems && item.ItemQuality == ItemQuality.Rare)
                                || (!Config.SellPurpleItems && item.ItemQuality == ItemQuality.Epic))
                            {
                                continue;
                            }

                            if (WowInterface.CharacterManager.IsItemAnImprovement(item, out IWowItem itemToReplace))
                            {
                                // equip item and sell the other after
                                itemToSell = itemToReplace;
                                WowInterface.HookManager.LuaEquipItem(item, itemToReplace);
                            }

                            if (itemToSell != null
                                && (WowInterface.ObjectManager.Player.Class != WowClass.Hunter || itemToSell.GetType() != typeof(WowProjectile)))
                            {
                                WowInterface.HookManager.LuaUseContainerItem(itemToSell.BagId, itemToSell.BagSlot);
                                WowInterface.HookManager.LuaCofirmStaticPopup();
                                Task.Delay(50).Wait();
                            }
                        }
                    }

                    foreach (IWowItem item in WowInterface.CharacterManager.Inventory.Items.Where(e => e.Price > 0))
                    {
                        IWowItem itemToSell = item;

                        if (Config.ItemSellBlacklist.Any(e => e.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                            || (!Config.SellGrayItems && item.ItemQuality == ItemQuality.Poor)
                            || (!Config.SellWhiteItems && item.ItemQuality == ItemQuality.Common)
                            || (!Config.SellGreenItems && item.ItemQuality == ItemQuality.Uncommon)
                            || (!Config.SellBlueItems && item.ItemQuality == ItemQuality.Rare)
                            || (!Config.SellPurpleItems && item.ItemQuality == ItemQuality.Epic))
                        {
                            continue;
                        }

                        if (WowInterface.CharacterManager.IsItemAnImprovement(item, out IWowItem itemToReplace))
                        {
                            // equip item and sell the other after
                            itemToSell = itemToReplace;
                            WowInterface.HookManager.LuaEquipItem(item, itemToReplace);
                        }

                        if (itemToSell != null
                            && (WowInterface.ObjectManager.Player.Class != WowClass.Hunter || itemToSell.GetType() != typeof(WowProjectile)))
                        {
                            WowInterface.HookManager.LuaUseContainerItem(itemToSell.BagId, itemToSell.BagSlot);
                            WowInterface.HookManager.LuaCofirmStaticPopup();
                        }
                    }

                    if (Config.AutoRepair && WowInterface.ObjectManager.Target.IsRepairVendor)
                    {
                        WowInterface.HookManager.LuaRepairAllItems();
                    }
                }
            }
            else
            {
                StateMachine.SetState(BotState.Idle);
            }
        }

        public bool IsVendorNpcNear(out WowUnit unit)
        {
            unit = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .FirstOrDefault(e => e.GetType() != typeof(WowPlayer)
                              && !Blacklist.Contains(e.Guid)
                              && !e.IsDead
                              && e.IsVendor
                              && WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Hostile
                              && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.RepairNpcSearchRadius);

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