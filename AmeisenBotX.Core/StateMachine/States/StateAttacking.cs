using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.States
{
    internal class StateAttacking : BasicState
    {
        public StateAttacking(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, IPathfindingHandler pathfindingHandler, IMovementEngine movementEngine, MovementSettings movementSettings, ICombatClass combatClass) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            MovementEngine = movementEngine;
            CombatClass = combatClass;

            Enemies = new List<WowUnit>();

            // default distance values
            DistanceToTarget = combatClass == null || combatClass.IsMelee ? 3.0 : 25.0;
        }

        public double DistanceToTarget { get; private set; }

        private CharacterManager CharacterManager { get; }

        private ICombatClass CombatClass { get; }

        private AmeisenBotConfig Config { get; }

        private List<WowUnit> Enemies { get; set; }

        private HookManager HookManager { get; }

        private DateTime LastRotationCheck { get; set; }

        private DateTime LastValidTargetCheck { get; set; }

        private IMovementEngine MovementEngine { get; set; }

        private ObjectManager ObjectManager { get; }

        public override void Enter()
        {
            AmeisenBotStateMachine.XMemory.Write(AmeisenBotStateMachine.OffsetList.CvarMaxFps, Config.MaxFpsCombat);
        }

        public override void Execute()
        {
            if (IsTargetInValid() && !ObjectManager.Player.IsInCombat && !AmeisenBotStateMachine.IsAnyPartymemberInCombat())
            {
                AmeisenBotStateMachine.SetState(BotState.Idle);
                return;
            }

            // we can do nothing until the ObjectManager is initialzed
            if (ObjectManager != null
                && ObjectManager.Player != null)
            {
                // use the default Target selection algorithm if the CombatClass doenst

                // Note: this only works for dps classes, tanks and healers need
                //        to implement their own target selection algorithm
                if (CombatClass == null || !CombatClass.HandlesTargetSelection)
                {
                    // make sure we got both objects refreshed before we check them
                    ObjectManager.UpdateObject(ObjectManager.Player);
                    ObjectManager.UpdateObject(ObjectManager.Target);

                    // do we need to clear our target
                    if (DateTime.Now - LastValidTargetCheck > TimeSpan.FromMilliseconds(250))
                    {
                        if (IsTargetInValid())
                        {
                            HookManager.ClearTarget();
                            ObjectManager.UpdateObject(ObjectManager.Player);

                            // select a new target if our current target is invalid
                            if (SelectTargetToAttack(out WowUnit target))
                            {
                                HookManager.TargetGuid(target.Guid);

                                ObjectManager.UpdateObject(ObjectManager.Player);
                                ObjectManager.UpdateObject(ObjectManager.Target);
                            }
                        }

                        LastValidTargetCheck = DateTime.Now;
                    }

                    // use the default MovementEngine to move if the CombatClass doesnt
                    if ((CombatClass == null || !CombatClass.HandlesMovement) && ObjectManager.Target != null)
                    {
                        HandleMovement(ObjectManager.Target);
                    }

                    // if no CombatClass is loaded, just autoattack
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
            MovementEngine.Reset();

            Enemies.Clear();

            // set our normal maxfps
            AmeisenBotStateMachine.XMemory.Write(AmeisenBotStateMachine.OffsetList.CvarMaxFps, Config.MaxFps);

            // add nearby Units to the loot List
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

        private void HandleMovement(WowUnit target)
        {
            // we cant move to a null target
            if (target == null || target.IsDead || target.IsNotAttackable)
            {
                return;
            }

            // we don't want to move when we are casting or channeling something
            if (ObjectManager.Player.CurrentlyCastingSpellId > 0 || ObjectManager.Player.CurrentlyChannelingSpellId > 0)
            {
                return;
            }

            // if we are close enough, stop movement and start attacking
            double distance = ObjectManager.Player.Position.GetDistance2D(target.Position);
            if (distance <= DistanceToTarget)
            {
                // do we need to stop movement
                HookManager.StopClickToMove(ObjectManager.Player);

                // perform a facing check every 250ms, should be enough
                if (target.Guid != ObjectManager.PlayerGuid
                    && !BotMath.IsFacing(ObjectManager.Player.Position, ObjectManager.Player.Rotation, target.Position)
                    && DateTime.Now - LastRotationCheck > TimeSpan.FromMilliseconds(250))
                {
                    HookManager.FacePosition(ObjectManager.Player, target.Position);
                    CharacterManager.Face(target.Position, target.Guid);
                    LastRotationCheck = DateTime.Now;
                }
            }
            else
            {
                MovementEngine.SetState(MovementEngineState.DirectMoving, target.Position);
                MovementEngine.Execute();
            }
        }

        private bool IsTargetInValid()
            => !BotUtils.IsValidUnit(ObjectManager.Target)
                || ObjectManager.Target.IsDead
                || ObjectManager.Target.Guid == ObjectManager.PlayerGuid
                || ObjectManager.Player.Position.GetDistance(ObjectManager.Target.Position) > 50;

        private bool SelectTargetToAttack(out WowUnit target)
        {
            // TODO: need to handle duels, our target will
            // be friendly there but is attackable

            // get all targets that are not friendly to us
            List<WowUnit> nonFriendlyUnits = ObjectManager.WowObjects.OfType<WowUnit>()
                .Where(e => e.IsInCombat && HookManager.GetUnitReaction(ObjectManager.Player, e) != WowUnitReaction.Friendly)
                .ToList();

            // remove all invalid, dead units
            nonFriendlyUnits = nonFriendlyUnits.Where(e => BotUtils.IsValidUnit(e)).ToList();

            // if there are no non Friendly units, we can't attack anything
            if (nonFriendlyUnits.Count > 0)
            {
                List<WowUnit> unitsInCombat = nonFriendlyUnits.OrderBy(e => e.Position.GetDistance(ObjectManager.Player.Position)).ToList();
                target = unitsInCombat.FirstOrDefault();

                if (target != null)
                {
                    return true;
                }
                else
                {
                    // maybe we are able to assist our partymembers
                    /*if (ObjectManager.PartymemberGuids.Count > 0)
                    {
                        Dictionary<ulong, int> partymemberTargets = new Dictionary<ulong, int>();
                        ObjectManager.Partymembers.ForEach(e =>
                        {
                            ulong targetGuid = ((WowUnit)e).TargetGuid;
                            if (targetGuid > 0)
                            {
                                if (partymemberTargets.ContainsKey(targetGuid))
                                {
                                    partymemberTargets[targetGuid]++;
                                }
                                else
                                {
                                    partymemberTargets.Add(targetGuid, 1);
                                }
                            }
                        });

                        List<KeyValuePair<ulong, int>> selectedTargets = partymemberTargets.OrderByDescending(e => e.Value).ToList();

                        // filter out invalid, not in combat and friendly units
                        WowUnit validTarget = null;

                        selectedTargets.ForEach(e =>
                        {
                            WowUnit target = (WowUnit)ObjectManager.GetWowObjectByGuid(e.Key);
                            if (BotUtils.IsValidUnit(target)
                                && target.IsInCombat
                                && HookManager.GetUnitReaction(ObjectManager.Player, target) != WowUnitReaction.Friendly)
                            {
                                validTarget = target;
                            }
                        });

                        if (validTarget != null)
                        {
                            target = validTarget;
                            return true;
                        }
                    }*/

                    // last fallback, target our nearest enemy
                    HookManager.TargetNearestEnemy();
                    ObjectManager.UpdateObject(ObjectManager.Player);

                    target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.IsInCombat && e.Guid == ObjectManager.Player.Guid);
                    if (BotUtils.IsValidUnit(target)
                        && target.IsInCombat)
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