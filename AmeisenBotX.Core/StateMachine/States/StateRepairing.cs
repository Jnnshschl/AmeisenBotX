using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.SMovementEngine.Enums;
using AmeisenBotX.Core.StateMachine.Routines;
using System;
using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Core.Data.Db.Objects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;

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

        private TimegatedEvent EquipmentUpdateEvent { get; }

        private TimegatedEvent InteractionEvent { get; }

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
                WowInterface.HookManager.LuaClickUiElement("MerchantFrameCloseButton");
                StateMachine.SetState(BotState.Idle);
                return;
            }

            if (IsRepairNpcNear(out WowUnit selectedUnit))
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(selectedUnit.Position) > 3.5)
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
            else if (WowInterface.MovementEngine.IsAtTargetPosition || WowInterface.MovementEngine.MovementAction == MovementAction.None)
            {
                int playerMapId = (int) WowInterface.ObjectManager.MapId;
                Vector3 playerPosition = WowInterface.ObjectManager.Player.Position;
                Vector3 nearestVendorPosition = StaticDB.Vendors.Where(e => playerMapId == e.MapId && e.IsRepairer && e.LikesUnit(WowInterface.ObjectManager.Player))
                    .OrderBy(e => playerPosition.GetDistance(e.Position)).First().Position;
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, nearestVendorPosition);
            }
        }

        public bool IsRepairNpcNear(out WowUnit unit)
        {
            unit = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                       .FirstOrDefault(e => e.GetType() != typeof(WowPlayer)
                              && !Blacklist.Contains(e.Guid)
                              && !e.IsDead
                              && e.IsRepairVendor
                              && WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, e) != WowUnitReaction.Hostile
                              && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < Config.RepairNpcSearchRadius);

            return unit != null;
        }

        public override void Leave()
        {
        }

        internal bool NeedToRepair()
        {
            return WowInterface.CharacterManager.Equipment.Items
                       .Any(e => e.Value.MaxDurability > 0 && ((double)e.Value.Durability / (double)e.Value.MaxDurability * 100.0) <= Config.ItemRepairThreshold);
        }
    }
}