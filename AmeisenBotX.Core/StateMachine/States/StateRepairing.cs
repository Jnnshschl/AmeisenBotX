using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.SMovementEngine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateRepairing : BasicState
    {
        public StateRepairing(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            Blacklist = new List<ulong>();
            InteractionEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));
            EquipmentUpdateEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
        }

        public List<ulong> Blacklist { get; }

        private int BlacklistCounter { get; set; }

        private TimegatedEvent InteractionEvent { get; }

        private TimegatedEvent EquipmentUpdateEvent { get; }

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
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (IsRepairNpcNear(out WowUnit selectedUnit))
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

                    WowInterface.HookManager.RepairAllItems();
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

        public bool IsRepairNpcNear(out WowUnit unit)
        {
            unit = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .FirstOrDefault(e => e.GetType() != typeof(WowPlayer)
                              && !Blacklist.Contains(e.Guid)
                              && !e.IsDead
                              && e.IsRepairVendor
                              && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Hostile
                              && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.RepairNpcSearchRadius);

            return unit != null;
        }

        internal bool NeedToRepair()
        {
            return WowInterface.CharacterManager.Equipment.Items
                       .Any(e => e.Value.MaxDurability > 0 && ((double)e.Value.Durability * (double)e.Value.MaxDurability * 100.0) <= Config.ItemRepairThreshold);
        }
    }
}