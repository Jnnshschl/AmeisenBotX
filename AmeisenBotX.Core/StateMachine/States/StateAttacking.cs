using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    internal class StateAttacking : BasicState
    {
        public StateAttacking(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            Enemies = new List<WowUnit>();

            // default distance values
            DistanceToTarget = WowInterface.CombatClass == null || WowInterface.CombatClass.IsMelee ? 2 : 25.0;
        }

        public double DistanceToTarget { get; private set; }

        private List<WowUnit> Enemies { get; set; }

        private DateTime LastFacingCheck { get; set; }

        private WowUnit LastTarget { get; set; }

        public override void Enter()
        {
            WowInterface.HookManager.ClearTargetIfDeadOrFriendly();
            WowInterface.MovementEngine.Reset();
            Enemies.Clear();

            WowInterface.XMemory.Write(WowInterface.OffsetList.CvarMaxFps, Config.MaxFpsCombat);
        }

        public override void Execute()
        {
            WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Player);

            if (!WowInterface.ObjectManager.Player.IsInCombat
                && !StateMachine.IsAnyPartymemberInCombat()
                && (WowInterface.BattlegroundEngine == null || !WowInterface.BattlegroundEngine.ForceCombat)
                && !Enemies.Any(e => !e.IsDead && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 60))
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            // we can do nothing until the ObjectManager is initialzed
            if (WowInterface.ObjectManager != null
                && WowInterface.ObjectManager.Player != null)
            {
                // use the default Target selection algorithm if the CombatClass doenst

                // Note: this only works for dps classes, tanks and healers need
                //        to implement their own target selection algorithm
                if (WowInterface.CombatClass == null || !WowInterface.CombatClass.HandlesTargetSelection)
                {
                    // get all targets that are not friendly to us
                    Enemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 100)
                        .Where(e => BotUtils.IsValidUnit(e) && e.TargetGuid != 0 && WowInterface.ObjectManager.PartymemberGuids.Contains(e.TargetGuid)).ToList();

                    // make sure we got both objects refreshed before we check them
                    WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Target);

                    // do we need to clear our target
                    if (IsTargetInvalid())
                    {
                        if (WowInterface.ObjectManager.TargetGuid != 0)
                        {
                            WowInterface.HookManager.ClearTargetIfDeadOrFriendly();
                            AmeisenLogger.Instance.Log("StateAttacking", $"Clearing invalid Target {WowInterface.ObjectManager.TargetGuid} {WowInterface.ObjectManager.Target.Name}", LogLevel.Verbose);
                        }

                        WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Player);

                        // select a new target if our current target is invalid
                        if (SelectTargetToAttack(out WowUnit target))
                        {
                            WowInterface.HookManager.TargetGuid(target.Guid);
                            AmeisenLogger.Instance.Log("StateAttacking", $"Selecting new Target {target.Guid} {target.Name}", LogLevel.Verbose);

                            WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Player);
                            WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Target);
                        }
                        else
                        {
                            // there is no valid target to attack
                            AmeisenLogger.Instance.Log("StateAttacking", $"No valid target found...", LogLevel.Verbose);
                            return;
                        }
                    }

                    // we cant move to a null target and we don't want to
                    // move when we are casting or channeling something
                    if (!BotUtils.IsValidUnit(WowInterface.ObjectManager.Target)) { return; }
                }

                // use the default MovementEngine to move if the CombatClass doesnt
                if ((WowInterface.CombatClass == null || !WowInterface.CombatClass.HandlesMovement))
                {
                    if (LastTarget == null || WowInterface.ObjectManager.Target.Guid != LastTarget.Guid)
                    {
                        WowInterface.MovementEngine.Reset();
                        LastTarget = WowInterface.ObjectManager.Target;
                    }

                    HandleMovement(WowInterface.ObjectManager.Target);
                }

                // if no CombatClass is loaded, just autoattack
                if (WowInterface.CombatClass != null)
                {
                    WowInterface.CombatClass.Execute();
                }
                else
                {
                    if (!WowInterface.ObjectManager.Player.IsAutoAttacking)
                    {
                        WowInterface.HookManager.StartAutoAttack();
                    }
                }
            }
        }

        public override void Exit()
        {
            WowInterface.MovementEngine.Reset();

            // set our normal maxfps
            WowInterface.XMemory.Write(WowInterface.OffsetList.CvarMaxFps, Config.MaxFps);
        }

        private void HandleMovement(WowUnit target)
        {
            if (target == null) { return; }

            // if we are close enough, stop movement and start attacking
            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(target.Position);
            if (distance <= DistanceToTarget)
            {
                WowInterface.HookManager.StopClickToMoveIfActive(WowInterface.ObjectManager.Player);

                if (DateTime.Now - LastFacingCheck > TimeSpan.FromSeconds(1) && !BotMath.IsFacing(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, target.Position))
                {
                    LastFacingCheck = DateTime.Now;
                    WowInterface.HookManager.FacePosition(WowInterface.ObjectManager.Player, target.Position);
                }
            }
            else
            {
                Vector3 positionToGoTo = target.Position; // WowInterface.CombatClass.IsMelee ? BotMath.CalculatePositionBehind(target.Position, target.Rotation, 4) :
                WowInterface.MovementEngine.SetState(distance > 4 ? MovementEngineState.Moving : MovementEngineState.DirectMoving, positionToGoTo);
                WowInterface.MovementEngine.Execute();
            }
        }

        private bool IsTargetInvalid()
            => !BotUtils.IsValidUnit(WowInterface.ObjectManager.Target)
                || WowInterface.ObjectManager.Target.IsDead
                || WowInterface.ObjectManager.TargetGuid == WowInterface.ObjectManager.PlayerGuid
                || WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target) == WowUnitReaction.Friendly
                || WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position) > 50;

        private bool SelectTargetToAttack(out WowUnit target)
        {
            // TODO: need to handle duels, our target will
            // be friendly there but is attackable

            if (Enemies.Count > 0)
            {
                target = Enemies.FirstOrDefault(e => BotUtils.IsValidUnit(e) && !e.IsDead);

                if (target != null)
                {
                    return true;
                }
            }

            // remove all invalid, dead units
            List<WowUnit> nonFriendlyUnits = Enemies.Where(e => BotUtils.IsValidUnit(e) || !e.IsDead || !e.IsNotAttackable).ToList();

            // if there are no non Friendly units, we can't attack anything
            if (nonFriendlyUnits.Count > 0)
            {
                List<WowUnit> unitsInCombatTargetingUs = nonFriendlyUnits
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).ToList();

                do
                {
                    target = unitsInCombatTargetingUs.FirstOrDefault();

                    if (target != null)
                    {
                        unitsInCombatTargetingUs.Remove(target);
                    }
                } while (target == null);

                if (target != null)
                {
                    return true;
                }
                else
                {
                    // maybe we are able to assist our partymembers
                    if (WowInterface.ObjectManager.PartymemberGuids.Count > 0)
                    {
                        Dictionary<WowUnit, int> partymemberTargets = new Dictionary<WowUnit, int>();
                        WowInterface.ObjectManager.Partymembers.ForEach(e =>
                        {
                            if (e.TargetGuid > 0)
                            {
                                WowUnit target = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(e.TargetGuid);

                                if (target != null
                                    && BotUtils.IsValidUnit(e)
                                    && target.IsInCombat
                                    && WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, target) != WowUnitReaction.Friendly)
                                {
                                    if (partymemberTargets.ContainsKey(target))
                                    {
                                        partymemberTargets[target]++;
                                    }
                                    else
                                    {
                                        partymemberTargets.Add(target, 1);
                                    }
                                }
                            }
                        });

                        List<KeyValuePair<WowUnit, int>> selectedTargets = partymemberTargets.OrderByDescending(e => e.Value).ToList();

                        // filter out invalid, not in combat and friendly units
                        WowUnit validTarget = selectedTargets.First().Key;

                        if (validTarget != null)
                        {
                            target = validTarget;
                            return true;
                        }
                    }
                }
            }

            target = null;
            WowInterface.HookManager.TargetGuid(WowInterface.ObjectManager.PlayerGuid);
            return false;
        }
    }
}