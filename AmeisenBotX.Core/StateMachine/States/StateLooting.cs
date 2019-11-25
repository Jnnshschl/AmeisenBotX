using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Common.Enums;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Pathfinding;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateLooting : State
    {
        public StateLooting(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, IOffsetList offsetList, ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IPathfindingHandler pathfindingHandler, IMovementEngine movementEngine, Queue<ulong> unitLootList) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            OffsetList = offsetList;
            PathfindingHandler = pathfindingHandler;
            MovementEngine = movementEngine;
            UnitLootList = unitLootList;
            UnitsAlreadyLootedList = new List<ulong>();
        }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private HookManager HookManager { get; }

        private IMovementEngine MovementEngine { get; set; }

        private ObjectManager ObjectManager { get; }

        private IOffsetList OffsetList { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private List<ulong> UnitsAlreadyLootedList { get; }

        private Queue<ulong> UnitLootList { get; }

        private int LootTryCount { get; set; }

        public override void Enter()
        {
            LootTryCount = 0;
        }

        public override void Execute()
        {
            if (UnitLootList.Count == 0)
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
            }
            else
            {
                if (UnitsAlreadyLootedList.Contains(UnitLootList.Peek()))
                {
                    UnitLootList.Dequeue();
                    return;
                }

                WowUnit selectedUnit = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == UnitLootList.Peek());
                if (selectedUnit != null && selectedUnit.IsDead && selectedUnit.IsLootable)
                {
                    if (ObjectManager.Player.Position.GetDistance(selectedUnit.Position) > 3.0)
                    {
                        MovementEngine.SetState(MovementEngineState.Moving, selectedUnit.Position);
                        MovementEngine.Execute();
                    }
                    else
                    {
                        do
                        {
                            HookManager.RightClickUnit(selectedUnit);
                            LootTryCount++;
                            Task.Delay(500).GetAwaiter().GetResult();
                        } while (AmeisenBotStateMachine.XMemory.ReadByte(OffsetList.LootWindowOpen, out byte lootOpen)
                                 && lootOpen == 0
                                 && LootTryCount < 2);

                        UnitsAlreadyLootedList.Add(UnitLootList.Dequeue());
                    }
                }
                else
                {
                    UnitLootList.Dequeue();
                }
            }
        }

        public override void Exit()
        {

        }
    }
}
