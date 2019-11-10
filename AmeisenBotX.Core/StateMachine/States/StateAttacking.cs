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

        public double DistanceToTarget { get; private set; }

        private CharacterManager CharacterManager { get; }

        private ICombatClass CombatClass { get; }

        private AmeisenBotConfig Config { get; }

        private WowUnit CurrentTarget => ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.Player.TargetGuid);

        private HookManager HookManager { get; }

        private DateTime LastRotationCheck { get; set; }

        private IMovementEngine MovementEngine { get; set; }

        private bool NeedToStopMovement { get; set; }

        private ObjectManager ObjectManager { get; }

        private IPathfindingHandler PathfindingHandler { get; }

        private int TryCount { get; set; }

        private Vector3 LastPosition { get; set; }

        public override void Enter()
        {
            AmeisenBotStateMachine.XMemory.Write(AmeisenBotStateMachine.OffsetList.CvarMaxFps, Config.MaxFpsCombat);

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
                    if ((ObjectManager.TargetGuid == 0
                        || (CurrentTarget == null || CurrentTarget.Guid == ObjectManager.PlayerGuid))
                        && SelectTargetToAttack(out WowUnit target)
                        && target.Guid != ObjectManager.PlayerGuid)
                    {
                        HookManager.TargetGuid(target.Guid);
                        ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);
                        ObjectManager.UpdateObject(target.Type, target.BaseAddress);
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
            AmeisenBotStateMachine.XMemory.Write(AmeisenBotStateMachine.OffsetList.CvarMaxFps, Config.MaxFps);
            MovementEngine.Reset();

            if (CombatClass == null || !CombatClass.HandlesTargetSelection)
            {
                HookManager.ClearTarget();
            }

            if (Config.LootUnits)
            {
                foreach (WowUnit lootableUnit in ObjectManager.WowObjects.OfType<WowUnit>().Where(e => e.IsLootable && e.Position.GetDistance2D(ObjectManager.Player.Position) < Config.LootUnitsRadius))
                {
                    if (!AmeisenBotStateMachine.UnitLootList.Contains(lootableUnit.Guid))
                    {
                        AmeisenBotStateMachine.UnitLootList.Enqueue(lootableUnit.Guid);
                    }
                }
            }
        }

        private void BuildNewPath(WowUnit target)
        {
            List<Vector3> path = PathfindingHandler.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, target.Position);
            MovementEngine.LoadPath(path);
        }

        private void HandleMovement(WowUnit target)
        {
            if (target == null)
            {
                return;
            }

            if (target.Guid != ObjectManager.PlayerGuid
                && !BotMath.IsFacing(ObjectManager.Player.Position, ObjectManager.Player.Rotation, target.Position)
                && DateTime.Now - LastRotationCheck > TimeSpan.FromMilliseconds(1000))
            {
                CharacterManager.Face(target.Position, target.Guid);
                LastRotationCheck = DateTime.Now;
            }

            // we don't want to move when we are casting/channeling something either
            if (ObjectManager.Player.CurrentlyCastingSpellId > 0 || ObjectManager.Player.CurrentlyChannelingSpellId > 0)
            {
                return;
            }

            // if we are close enough, face the target and start attacking
            double distance = ObjectManager.Player.Position.GetDistance2D(target.Position);
            if (distance <= DistanceToTarget)
            {
                // do we need to stop movement
                if (!CombatClass.IsMelee && NeedToStopMovement)
                {
                    if (CharacterManager.GetCurrentClickToMovePoint(out Vector3 ctmPosition)
                        && (int)ctmPosition.X != (int)ObjectManager.Player.Position.Z
                        || (int)ctmPosition.Y != (int)ObjectManager.Player.Position.Y
                        || (int)ctmPosition.Z != (int)ObjectManager.Player.Position.Z)
                    {
                        CharacterManager.StopMovement(ctmPosition, ObjectManager.Player.Guid);
                        NeedToStopMovement = false;
                    }
                }

                NeedToStopMovement = LastPosition != ObjectManager.Player.Position;
                LastPosition = ObjectManager.Player.Position;
                return;
            }

            if (target.Guid != ObjectManager.PlayerGuid
                && target != null
                && target.Guid != 0
                && target.Health > 1
                && !target.IsDead)
            {
                CharacterManager.MoveToPosition(target.Position, 20.9f, 0.2f);

                //// if (MovementEngine.CurrentPath?.Count < 1 || TryCount > 1)
                //// {
                ////     BuildNewPath(target);
                //// }
                //// 
                //// if (MovementEngine.CurrentPath.Count >= 1)
                //// {
                ////     CharacterManager.MoveToPosition(target.Position, 20.9f, 0.2f);
                //// }                
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

        private bool SelectTargetToAttack(out WowUnit target)
        {
            List<WowUnit> wowUnits = ObjectManager.WowObjects.OfType<WowUnit>().Where(e => HookManager.GetUnitReaction(ObjectManager.Player, e) != WowUnitReaction.Friendly && e.Guid != ObjectManager.PlayerGuid).ToList();
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
                                    .Where(e => ObjectManager.PartymemberGuids.Contains(e.Guid) && e.Guid != ObjectManager.PlayerGuid)
                                    .Where(e => HookManager.GetUnitReaction(ObjectManager.Player, (WowUnit)ObjectManager.WowObjects.FirstOrDefault(r => r.Guid == e.TargetGuid)) != WowUnitReaction.Friendly)
                                    .FirstOrDefault(r => r.IsInCombat)?.TargetGuid);

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
