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
        }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private HookManager HookManager { get; }

        private IMovementEngine MovementEngine { get; set; }

        private ObjectManager ObjectManager { get; }

        private IOffsetList OffsetList { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private Queue<ulong> UnitLootList { get; }

        private int TryCount { get; set; }

        public override void Enter()
        {
            MovementEngine.CurrentPath.Clear();
        }

        public override void Execute()
        {
            if (UnitLootList.Count == 0)
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
            }
            else
            {
                WowUnit selectedUnit = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == UnitLootList.Peek());
                if (selectedUnit != null && selectedUnit.IsDead && selectedUnit.IsLootable)
                {
                    if (ObjectManager.Player.Position.GetDistance(selectedUnit.Position) > 3.0)
                    {
                        if (MovementEngine.CurrentPath?.Count == 0 || TryCount == 5)
                        {
                            BuildNewPath(selectedUnit.Position);
                            TryCount = 0;
                        }
                        else
                        {
                            if (MovementEngine.GetNextStep(ObjectManager.Player.Position, ObjectManager.Player.Rotation, out Vector3 positionToGoTo, out bool needToJump))
                            {
                                CharacterManager.MoveToPosition(positionToGoTo);

                                if (needToJump)
                                {
                                    CharacterManager.Jump();

                                    Random rnd = new Random();
                                    BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_S), 300, 1000);

                                    if (rnd.Next(10) >= 5)
                                    {
                                        BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_Q), 300, 600);
                                    }
                                    else
                                    {
                                        BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_E), 300, 600);
                                    }

                                    TryCount++;
                                }
                            }
                        }
                    }
                    else
                    {
                        HookManager.RightClickUnit(selectedUnit);
                        UnitLootList.Dequeue();
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
