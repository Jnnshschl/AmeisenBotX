using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.States
{
    internal class StateAttacking : State
    {
        public StateAttacking(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IPathfindingHandler pathfindingHandler, ICombatClass combatClass) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            PathfindingHandler = pathfindingHandler;
            CurrentPath = new Queue<Vector3>();
            CombatClass = combatClass;
        }

        public Vector3 LastPosition { get; private set; }

        private CharacterManager CharacterManager { get; }

        private ICombatClass CombatClass { get; }

        private AmeisenBotConfig Config { get; }

        private Queue<Vector3> CurrentPath { get; set; }

        private HookManager HookManager { get; }

        private bool IsMelee { get; set; }

        private ObjectManager ObjectManager { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private int TryCount { get; set; }

        public override void Enter()
        {
            IsMelee = BotUtils.IsMeeleeClass(ObjectManager.Player.Class);
            CurrentPath.Clear();
            TryCount = 0;
        }

        public override void Execute()
        {
            if (ObjectManager != null
                && ObjectManager.Player != null)
            {
                if (!ObjectManager.Player.IsInCombat)
                {
                    AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
                    return;
                }

                if (CombatClass == null)
                {
                    if (CurrentPath.Count > 0)
                    {
                        HandleMovement();
                    }
                }

                if (SelectTarget(out WowUnit target)
                && target != null)
                {
                    if (CombatClass == null)
                    {
                        if (CurrentPath.Count == 0 && IsMelee)
                        {
                            // keep close to target
                            double distance = ObjectManager.Player.Position.GetDistance(target.Position);
                            if (distance > 3.2)
                            {
                                BuildNewPath(target);
                            }
                        }
                        else
                        {
                            // keep distance to target
                        }
                    }
                    else
                    {
                        CombatClass.Execute();
                    }
                }
            }
        }

        public override void Exit()
        {
        }

        private void BuildNewPath(WowUnit target)
        {
            List<Vector3> path = PathfindingHandler.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, target.Position);
            if (path.Count > 0)
            {
                foreach (Vector3 pos in path)
                {
                    CurrentPath.Enqueue(pos);
                }
            }
        }

        private void HandleMovement()
        {
            Vector3 pos = CurrentPath.Peek();
            double distance = pos.GetDistance2D(ObjectManager.Player.Position);
            double distTraveled = LastPosition.GetDistance2D(ObjectManager.Player.Position);

            if (distance <= 3
                || TryCount > 5)
            {
                CurrentPath.Dequeue();
                TryCount = 0;
            }
            else
            {
                CharacterManager.MoveToPosition(pos);

                if (distTraveled != 0 && distTraveled < 0.08)
                {
                    TryCount++;
                }

                // if the thing is too far away, drop the whole Path
                if (pos.Z - ObjectManager.Player.Position.Z > 2
                    && distance > 2)
                {
                    CurrentPath.Clear();
                }

                // jump if the node is higher than us
                if (pos.Z - ObjectManager.Player.Position.Z > 1.2
                    && distance < 3)
                {
                    CharacterManager.Jump();
                }
            }

            if (distTraveled != 0
                && distTraveled < 0.08)
            {
                // go forward
                BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr(0x26), 500, 750);
                CharacterManager.Jump();
            }

            LastPosition = ObjectManager.Player.Position;
        }

        private bool SelectTarget(out WowUnit target)
        {
            List<WowUnit> wowUnits = ObjectManager.WowObjects.OfType<WowUnit>().ToList();
            if (wowUnits.Count > 0)
            {
                target = wowUnits.FirstOrDefault(t => t.Guid == ObjectManager.TargetGuid);
                bool validUnit = BotUtils.IsValidUnit(target);

                if (validUnit && target.IsInCombat)
                {
                    if (!ObjectManager.Player.IsAutoAttacking)
                    {
                        HookManager.StartAutoAttack();
                    }

                    return true;
                }
                else
                {
                    // find a new target
                    HookManager.TargetNearestEnemy();
                }
            }

            target = null;
            return false;
        }
    }
}