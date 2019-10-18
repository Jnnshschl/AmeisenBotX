using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Pathfinding;
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

        private bool IsMelee { get; set; }

        private ObjectManager ObjectManager { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private IMovementEngine MovementEngine { get; set; }

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

                if (MovementEngine.CurrentPath != null
                    && MovementEngine.GetNextStep(ObjectManager.Player.Position, out Vector3 positionToGoTo))
                {
                    CharacterManager.MoveToPosition(positionToGoTo);
                }

                if (SelectTarget(out WowUnit target)
                && target != null)
                {
                    if (CombatClass.HandlesMovement)
                    {
                        // we wont do anything movement related
                        // if the CombatClass takes care of it
                        CombatClass.Execute();
                    }
                    else
                    {
                        // otherwise the MovementEngine will be used
                        HandleMovement(target);
                    }
                }
            }
        }

        public override void Exit()
        {
        }

        private void HandleMovement(WowUnit target)
        {
            if (MovementEngine.CurrentPath == null && IsMelee)
            {
                // keep close to target
                double distance = ObjectManager.Player.Position.GetDistance(target.Position);
                if (distance > 3.0)
                {
                    BuildNewPath(target);
                }
            }
            else
            {
                // keep distance to target
                // TODO: implement a IMovementEngine for ranged classes
            }
        }

        private void BuildNewPath(WowUnit target)
        {
            List<Vector3> path = PathfindingHandler.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, target.Position);
            MovementEngine.LoadPath(path);
        }

        private bool SelectTarget(out WowUnit target)
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
                    // find a new target
                    //// HookManager.TargetNearestEnemy();
                }
            }

            target = null;
            return false;
        }
    }
}