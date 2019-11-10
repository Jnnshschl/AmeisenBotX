using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Common.Enums;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
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

        private int TryCount { get; set; }

        private int LootTryCount { get; set; }

        private Vector3 LastPosition { get; set; }

        public override void Enter()
        {
            MovementEngine.CurrentPath.Clear();
            TryCount = 0;
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
                        CharacterManager.MoveToPosition(selectedUnit.Position, 20.9f, 0.2f);
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

        private void DoRandomUnstuckMovement()
        {
            Random rnd = new Random();
            if (rnd.Next(10) >= 5)
            {
                BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_A), 300, 600);
            }
            else
            {
                BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_S), 300, 600);
            }

            if (rnd.Next(10) >= 5)
            {
                BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_Q), 300, 600);
            }
            else
            {
                BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_E), 300, 600);
            }
        }

        public override void Exit()
        {
            MovementEngine.CurrentPath.Clear();
        }

        private void BuildNewPath(Vector3 corpsePosition)
        {
            List<Vector3> path = PathfindingHandler.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, corpsePosition);
            MovementEngine.LoadPath(path);
            MovementEngine.PostProcessPath();
        }
    }
}
