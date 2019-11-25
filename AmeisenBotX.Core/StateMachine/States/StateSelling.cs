using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Common.Enums;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Pathfinding;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateSelling : State
    {
        public StateSelling(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, HookManager hookmanager, CharacterManager characterManager, IPathfindingHandler pathfindingHandler, IMovementEngine movementEngine) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            HookManager = hookmanager;
            CharacterManager = characterManager;
            PathfindingHandler = pathfindingHandler;
            MovementEngine = movementEngine;
        }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private IMovementEngine MovementEngine { get; set; }

        private ObjectManager ObjectManager { get; }

        private HookManager HookManager { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        public override void Enter()
        {

        }

        public override void Execute()
        {
            if (HookManager.GetFreeBagSlotCount() > 4
               || !CharacterManager.Inventory.Items.Any(e => e.Price > 0))
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
                return;
            }

            WowUnit selectedUnit = ObjectManager.WowObjects.OfType<WowUnit>()
                .OrderBy(e => e.Position.GetDistance(ObjectManager.Player.Position))
                .FirstOrDefault(e => e.GetType() != typeof(WowPlayer) && e.IsRepairVendor && e.Position.GetDistance(ObjectManager.Player.Position) < 50);

            if (selectedUnit != null && !selectedUnit.IsDead)
            {
                double distance = ObjectManager.Player.Position.GetDistance(selectedUnit.Position);
                if (distance > 5.0)
                {
                    MovementEngine.SetState(MovementEngineState.Moving, selectedUnit.Position);
                    MovementEngine.Execute();
                }
                else
                {
                    if (distance > 4)
                    {
                        CharacterManager.InteractWithUnit(selectedUnit, 20.9f, 2f);
                    }
                    else
                    {
                        HookManager.RightClickUnit(selectedUnit);
                        Task.Delay(1000).GetAwaiter().GetResult();

                        HookManager.SellAllGrayItems();
                        foreach (IWowItem item in CharacterManager.Inventory.Items.Where(e => e.Price > 0))
                        {
                            IWowItem itemToSell = item;
                            if (CharacterManager.IsItemAnImprovement(item, out IWowItem itemToReplace))
                            {
                                itemToSell = itemToReplace;
                                HookManager.ReplaceItem(null, item);
                            }

                            HookManager.UseItemByBagAndSlot(itemToSell.BagId, itemToSell.BagSlot);
                        }
                    }
                }
            }
            else
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
            }
        }

        public override void Exit()
        {

        }
    }
}
