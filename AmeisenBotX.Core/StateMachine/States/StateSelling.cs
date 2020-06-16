using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateSelling : BasicState
    {
        public StateSelling(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        private bool IsAtNpc { get; set; }

        private DateTime SellActionGo { get; set; }

        public override void Enter()
        {
            IsAtNpc = false;
        }

        public override void Execute()
        {
            if (WowInterface.HookManager.GetFreeBagSlotCount() > 4
               || !WowInterface.CharacterManager.Inventory.Items.Any(e => e.Price > 0))
            {
                WowInterface.CharacterManager.Inventory.Update();
                StateMachine.SetState((int)BotState.Idle);
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
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, selectedUnit.Position);

                    if (WowInterface.MovementEngine.IsAtTargetPosition)
                    {
                        WowInterface.HookManager.UnitOnRightClick(selectedUnit);
                        SellActionGo = DateTime.Now + TimeSpan.FromSeconds(1);
                        IsAtNpc = true;
                    }
                }
                else if (DateTime.Now > SellActionGo)
                {
                    WowInterface.HookManager.SellAllGrayItems();
                    WowInterface.HookManager.RepairAllItems();

                    foreach (IWowItem item in WowInterface.CharacterManager.Inventory.Items.Where(e => e.Price > 0))
                    {
                        IWowItem itemToSell = item;
                        if (WowInterface.CharacterManager.IsItemAnImprovement(item, out IWowItem itemToReplace))
                        {
                            itemToSell = itemToReplace;
                            WowInterface.HookManager.ReplaceItem(null, item);
                        }

                        if (itemToSell != null 
                            && itemToSell.EquipSlot != EquipmentSlot.INVSLOT_AMMO 
                            && itemToSell.ItemQuality != ItemQuality.Epic)
                        {
                            WowInterface.HookManager.UseItemByBagAndSlot(itemToSell.BagId, itemToSell.BagSlot);
                            WowInterface.HookManager.CofirmBop();
                        }
                    }
                }
            }
            else
            {
                WowInterface.CharacterManager.Inventory.Update();
                StateMachine.SetState((int)BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}