using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Common.Enums;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Pathfinding;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.States
{
    internal class StateAttacking : State
    {
        public StateAttacking(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IPathfindingHandler pathfindingHandler, IMovementEngine movementEngine, ICombatClass combatClass) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            PathfindingHandler = pathfindingHandler;
            MovementEngine = movementEngine;
            CombatClass = combatClass;
        }

        private CharacterManager CharacterManager { get; }

        private ICombatClass CombatClass { get; }

        private AmeisenBotConfig Config { get; }

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private IMovementEngine MovementEngine { get; set; }

        private WowUnit CurrentTarget => ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.Player.TargetGuid);

        private DateTime LastRotationCheck { get; set; }

        private int TryCount { get; set; }

        public override void Enter()
        {
            HookManager.ClearTarget();
            MovementEngine.CurrentPath.Clear();
        }

        public override void Execute()
        {
            if (ObjectManager != null
                && ObjectManager.Player != null)
            {
                if (!ObjectManager.Player.IsInCombat && !AmeisenBotStateMachine.IsAnyPartymemberInCombat())
                {
                    AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
                    return;
                }

                if (HookManager.GetUnitReaction(ObjectManager.Player, CurrentTarget) == WowUnitReaction.Friendly)
                {
                    HookManager.ClearTarget();
                }

                // Select a new target if our current target is invalid
                if (((CombatClass == null
                    || !CombatClass.HandlesTargetSelection)
                    || !BotUtils.IsValidUnit(CurrentTarget)
                    || CurrentTarget == null
                    || !CurrentTarget.IsInCombat)
                    && SelectTargetToAttack(out WowUnit target))
                {
                    HookManager.TargetGuid(target.Guid);
                }

                if (CurrentTarget == null || CurrentTarget.IsDead)
                {
                    HookManager.ClearTarget();
                    return;
                }

                if (CurrentTarget != null)
                {
                    if (CombatClass == null || !CombatClass.HandlesMovement)
                    {
                        // use the default MovementEngine to move if 
                        // the CombatClass doesnt handle Movement
                        HandleMovement(CurrentTarget);
                    }

                    if (CombatClass != null)
                    {
                        CombatClass.Execute();
                    }
                    else
                    {
                        if (!ObjectManager.Player.IsAutoAttacking)
                        {
                            HookManager.StartAutoAttack();
                        }
                    }
                }
            }
        }

        public override void Exit()
        {
            MovementEngine.CurrentPath.Clear();
            HookManager.ClearTarget();
        }

        private void HandleMovement(WowUnit target)
        {
            if (ObjectManager.Player.Position.GetDistance(target.Position) < 3.0)
            {
                if (DateTime.Now - LastRotationCheck > TimeSpan.FromMilliseconds(10000))
                {
                    // HookManager.FaceUnit(ObjectManager.Player, CurrentTarget.Position);
                    CharacterManager.Face(target.Position, target.Guid);
                    LastRotationCheck = DateTime.Now;
                }

                return;
            }

            if (MovementEngine.CurrentPath?.Count == 0 || TryCount == 5)
            {
                BuildNewPath(target);
            }
            else
            {
                if (MovementEngine.CurrentPath?.Count > 0
                    && MovementEngine.GetNextStep(ObjectManager.Player.Position, ObjectManager.Player.Rotation, out Vector3 positionToGoTo, out bool needToJump))
                {
                    CharacterManager.MoveToPosition(positionToGoTo, 6.28f);

                    if (needToJump)
                    {
                        CharacterManager.Jump();

                        BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_Q), 200, 500);
                        BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_E), 200, 500);
                        TryCount++;
                    }
                }
            }
        }

        private void BuildNewPath(WowUnit target)
        {
            List<Vector3> path = PathfindingHandler.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, target.Position);
            MovementEngine.LoadPath(path);
        }

        private bool SelectTargetToAttack(out WowUnit target)
        {
            List<WowUnit> wowUnits = ObjectManager.WowObjects.OfType<WowUnit>().Where(e => HookManager.GetUnitReaction(ObjectManager.Player, e) != WowUnitReaction.Friendly).ToList();
            if (wowUnits.Count > 0)
            {
                target = wowUnits.FirstOrDefault(t => t.Guid == ObjectManager.TargetGuid);
                if (BotUtils.IsValidUnit(target) && target.IsInCombat)
                {
                    return true;
                }
                else
                {
                    // find a new target from group
                    WowPlayer partytarget = ObjectManager.WowObjects.OfType<WowPlayer>()
                        .Where(e => ObjectManager.PartymemberGuids.Contains(e.Guid))
                        .FirstOrDefault(r => r.IsInCombat);

                    if (partytarget != null)
                    {
                        target = (WowUnit)ObjectManager.WowObjects.FirstOrDefault(e => e.Guid == partytarget.Guid);
                    }

                    if (BotUtils.IsValidUnit(target) && target.IsInCombat)
                    {
                        return true;
                    }
                    else
                    {
                        HookManager.TargetNearestEnemy();
                        target = (WowUnit)ObjectManager.WowObjects.FirstOrDefault(e => e.Guid == ObjectManager.Player.Guid);

                        if (BotUtils.IsValidUnit(target) && target.IsInCombat)
                        {
                            return true;
                        }
                    }
                }
            }

            target = null;
            return false;
        }
    }
}