using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateSelling : BasicState
    {
        public StateSelling(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            Blacklist = new List<ulong>();
            InteractionEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
        }

        public List<ulong> Blacklist { get; }

        private int BlacklistCounter { get; set; }

        private TimegatedEvent InteractionEvent { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            WowInterface.CharacterManager.Inventory.Update();

            if (!NeedToSell())
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (IsVendorNpcNear(out WowUnit selectedUnit))
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, selectedUnit.Position);

                if (WowInterface.MovementEngine.IsPathIncomplete)
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

                if (WowInterface.MovementEngine.IsAtTargetPosition && InteractionEvent.Run())
                {
                    if (WowInterface.ObjectManager.TargetGuid != selectedUnit.Guid)
                    {
                        WowInterface.HookManager.TargetGuid(selectedUnit.Guid);
                    }

                    WowInterface.HookManager.UnitOnRightClick(selectedUnit);

                    if (!BotMath.IsFacing(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, selectedUnit.Position))
                    {
                        WowInterface.HookManager.FacePosition(WowInterface.ObjectManager.Player, selectedUnit.Position);
                        return;
                    }

                    if (selectedUnit.IsGossip)
                    {
                        WowInterface.HookManager.UnitSelectGossipOption(1);
                        return;
                    }
                }
            }
            else
            {
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
        }

        public bool IsVendorNpcNear(out WowUnit unit)
        {
            unit = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .FirstOrDefault(e => e.GetType() != typeof(WowPlayer)
                              && !Blacklist.Contains(e.Guid)
                              && !e.IsDead
                              && e.IsVendor
                              && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Hostile
                              && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.RepairNpcSearchRadius);

            return unit != null;
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