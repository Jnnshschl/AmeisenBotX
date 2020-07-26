using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateSelling : BasicState
    {
        public StateSelling(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            WowInterface.CharacterManager.Inventory.Update();

            if (WowInterface.CharacterManager.Inventory.FreeBagSlots > Config.BagSlotsToGoSell
               || !WowInterface.CharacterManager.Inventory.Items.Where(e => !Config.ItemSellBlacklist.Contains(e.Name)
                       && ((Config.SellGrayItems && e.ItemQuality == ItemQuality.Poor)
                           || (Config.SellWhiteItems && e.ItemQuality == ItemQuality.Common)
                           || (Config.SellGreenItems && e.ItemQuality == ItemQuality.Uncommon)
                           || (Config.SellBlueItems && e.ItemQuality == ItemQuality.Rare)
                           || (Config.SellPurpleItems && e.ItemQuality == ItemQuality.Epic)))
               .Any(e => e.Price > 0))
            {
                StateMachine.SetState(BotState.Idle);
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
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, selectedUnit.Position);

                if (WowInterface.MovementEngine.IsAtTargetPosition)
                {
                    WowInterface.HookManager.UnitOnRightClick(selectedUnit);

                    if (!BotMath.IsFacing(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, selectedUnit.Position))
                    {
                        WowInterface.HookManager.FacePosition(WowInterface.ObjectManager.Player, selectedUnit.Position);
                    }

                    if (selectedUnit.IsGossip)
                    {
                        WowInterface.HookManager.UnitSelectGossipOption(1);
                    }
                }
            }
            else
            {
                WowInterface.CharacterManager.Inventory.Update();
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
        }

        internal bool IsVendorNpcNear()
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .Any(e => e.GetType() != typeof(WowPlayer)
                       && e.IsVendor
                       && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.MerchantNpcSearchRadius);
        }

        internal bool NeedToSell()
        {
            return IsVendorNpcNear()
                && WowInterface.CharacterManager.Inventory.FreeBagSlots < Config.BagSlotsToGoSell
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