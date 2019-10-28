using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Pathfinding;
using AmeisenBotX.Pathfinding.Objects;
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

        private bool IsMelee { get; set; }

        private ObjectManager ObjectManager { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private IMovementEngine MovementEngine { get; set; }

        private WowUnit SelectedTarget { get; set; }

        private WowUnit CurrentTarget => ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.Player.TargetGuid);

        public override void Enter()
        {
            IsMelee = BotUtils.IsMeeleeClass(ObjectManager.Player.Class);
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

                // Select a new target if our current target is invalid
                if (!CombatClass.HandlesTargetSelection 
                    && !BotUtils.IsValidUnit(CurrentTarget) 
                    && SelectTargetToAttack(out WowUnit target))
                {
                    HookManager.TargetGuid(target.Guid);
                    SelectedTarget = target;
                }

                if (SelectedTarget != null)
                {
                    if (CombatClass == null || !CombatClass.HandlesMovement)
                    {
                        // use the default MovementEngine to move if 
                        // the CombatClass doesnt handle Movement
                        HandleMovement(SelectedTarget);
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
        }

        private void HandleMovement(WowUnit target)
        {
            if (MovementEngine.CurrentPath?.Count == 0 && IsMelee)
            {
                // keep close to target
                double distance = ObjectManager.Player.Position.GetDistance(target.Position);
                if (distance > 3.0)
                {
                    BuildNewPath(target);
                }
            }
            else if (MovementEngine.CurrentPath?.Count == 0 && !IsMelee)
            {
                // keep distance to target
                // TODO: implement a IMovementEngine for ranged classes
            }
            else
            {
                if (MovementEngine.CurrentPath != null
                    && MovementEngine.GetNextStep(ObjectManager.Player.Position, ObjectManager.Player.Rotation, out Vector3 positionToGoTo, out bool needToJump))
                {
                    CharacterManager.MoveToPosition(positionToGoTo);

                    if (needToJump)
                    {
                        CharacterManager.Jump();
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
            List<WowUnit> wowUnits = ObjectManager.WowObjects.OfType<WowUnit>().ToList();
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
                    target = ObjectManager.WowObjects.OfType<WowPlayer>()
                        .Where(e => ObjectManager.PartymemberGuids.Contains(e.Guid))
                        .FirstOrDefault(r => r.IsInCombat);

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