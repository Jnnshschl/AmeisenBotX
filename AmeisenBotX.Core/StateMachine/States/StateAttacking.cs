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

            DistanceToTarget = combatClass == null || combatClass.IsMelee ? 3.0 : 25.0;
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

        private DateTime LastCastingCheck { get; set; }

        private bool NeedToStopMovement { get; set; }

        private bool IsCasting { get; set; }

        private int TryCount { get; set; }

        public double DistanceToTarget { get; private set; }

        public override void Enter()
        {
            MovementEngine.CurrentPath.Clear();
            MovementEngine.Reset();
        }

        public override void Execute()
        {
            if (!ObjectManager.Player.IsInCombat && !AmeisenBotStateMachine.IsAnyPartymemberInCombat())
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
                return;
            }

            if (ObjectManager != null
                && ObjectManager.Player != null)
            {
                if (CombatClass == null || !CombatClass.HandlesTargetSelection)
                {
                    if (HookManager.GetUnitReaction(ObjectManager.Player, CurrentTarget) == WowUnitReaction.Friendly)
                    {
                        HookManager.ClearTarget();
                    }

                    // Select a new target if our current target is invalid
                    if ((!BotUtils.IsValidUnit(CurrentTarget)
                        || CurrentTarget == null
                        || !CurrentTarget.IsInCombat)
                        && SelectTargetToAttack(out WowUnit target))
                    {
                        HookManager.TargetGuid(target.Guid);
                        ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);
                        ObjectManager.UpdateObject(target.Type, target.BaseAddress);
                    }

                    if (CurrentTarget == null || CurrentTarget.IsDead)
                    {
                        HookManager.ClearTarget();
                    }
                }

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

        public override void Exit()
        {
            MovementEngine.Reset();
            MovementEngine.CurrentPath.Clear();

            if (CombatClass == null || !CombatClass.HandlesTargetSelection)
            {
                HookManager.ClearTarget();
            }
        }

        private void HandleMovement(WowUnit target)
        {
            if (target == null)
            {
                return;
            }

            // we don't want to move when we are casting/channeling something either
            if (target != null && DateTime.Now - LastRotationCheck > TimeSpan.FromMilliseconds(1000))
            {
                CharacterManager.Face(target.Position, target.Guid);
                LastRotationCheck = DateTime.Now;
            }

            if (ObjectManager.Player.CurrentlyCastingSpellId > 0 || ObjectManager.Player.CurrentlyChannelingSpellId > 0)
            {
                return;
            }

            // if we are close enough, face the target and start attacking
            double distance = ObjectManager.Player.Position.GetDistance(target.Position);
            if (distance <= DistanceToTarget)
            {
                // do we need to stop movement
                if (NeedToStopMovement)
                {
                    if (CharacterManager.GetCurrentClickToMovePoint(out Vector3 ctmPosition)
                        && (int)ctmPosition.X != (int)ObjectManager.Player.Position.Z
                        || (int)ctmPosition.Y != (int)ObjectManager.Player.Position.Y
                        || (int)ctmPosition.Z != (int)ObjectManager.Player.Position.Z)
                    {
                        CharacterManager.StopMovement(ctmPosition, ObjectManager.Player.Guid);
                        NeedToStopMovement = false;

                        return;
                    }
                }
            }
            else
            {
                if (target.Guid == ObjectManager.PlayerGuid
                    && target != null
                    && target.Guid != 0
                    && target.Health > 1
                    && !target.IsDead)
                {
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
                            NeedToStopMovement = true;

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
                target = wowUnits.FirstOrDefault(t => t.IsInCombat);
                if (BotUtils.IsValidUnit(target) && target.IsInCombat)
                {
                    return true;
                }
                else
                {
                    if (ObjectManager.PartymemberGuids.Count > 0)
                    {
                        // find a new target from group
                        WowUnit partytarget = (WowUnit)ObjectManager.WowObjects
                            .FirstOrDefault(a => a.Guid ==
                                ObjectManager.WowObjects.OfType<WowPlayer>()
                                    .Where(e => ObjectManager.PartymemberGuids.Contains(e.Guid))
                                    .Where(e => HookManager.GetUnitReaction(ObjectManager.Player, (WowUnit)ObjectManager.WowObjects.FirstOrDefault(r => r.Guid == e.TargetGuid)) != WowUnitReaction.Friendly)
                                    .FirstOrDefault(r => r.IsInCombat).TargetGuid);

                        if (partytarget != null)
                        {
                            target = (WowUnit)ObjectManager.WowObjects.FirstOrDefault(e => e.Guid == partytarget.Guid);
                        }

                        if (BotUtils.IsValidUnit(target) && target.IsInCombat)
                        {
                            return true;
                        }
                    }

                    HookManager.TargetNearestEnemy();
                    target = (WowUnit)ObjectManager.WowObjects.FirstOrDefault(e => e.Guid == ObjectManager.Player.Guid);

                    if (BotUtils.IsValidUnit(target) && target.IsInCombat)
                    {
                        return true;
                    }

                }
            }

            target = null;
            return false;
        }
    }
}