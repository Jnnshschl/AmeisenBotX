using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Logging;
using System;
using System.Linq;
using System.Numerics;

namespace AmeisenBotX.Core.Statemachine.States
{
    internal class StateAttacking : BasicState
    {
        public StateAttacking(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            FacingCheck = new TimegatedEvent(TimeSpan.FromMilliseconds(100));
            LineOfSightCheck = new TimegatedEvent<bool>(TimeSpan.FromMilliseconds(1000));
            RandomPosEvent = new TimegatedEvent(TimeSpan.FromSeconds(5));
        }

        public double DistanceToKeep => WowInterface.CombatClass == null || WowInterface.CombatClass.IsMelee ? GetMeeleRange() : 28.0;

        public bool TargetInLos { get; private set; }

        private TimegatedEvent FacingCheck { get; set; }

        private TimegatedEvent<bool> LineOfSightCheck { get; set; }

        private TimegatedEvent RandomPosEvent { get; }

        public override void Enter()
        {
            WowInterface.MovementEngine.Reset();

            if (Config.MaxFps != Config.MaxFpsCombat)
            {
                WowInterface.HookManager.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFpsCombat});SetCVar(\"maxfpsbk\", {Config.MaxFpsCombat})");
            }
        }

        public override void Execute()
        {
            if (!(WowInterface.Globals.ForceCombat || WowInterface.ObjectManager.Player.IsInCombat || StateMachine.IsAnyPartymemberInCombat())
                || !WowInterface.ObjectManager.GetEnemiesInCombatWithUs<WowUnit>(WowInterface.ObjectManager.Player.Position, 100.0).Any())
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            // we can do nothing until the ObjectManager is initialzed
            if (WowInterface.ObjectManager != null && WowInterface.ObjectManager.Player != null)
            {
                // use the default MovementEngine to move if the CombatClass doesnt
                if (WowInterface.CombatClass == null || !WowInterface.CombatClass.HandlesMovement)
                {
                    HandleMovement(WowInterface.ObjectManager.Target);
                }

                // if no CombatClass is loaded, just autoattack
                if (WowInterface.CombatClass == null)
                {
                    if (!WowInterface.ObjectManager.Player.IsAutoAttacking)
                    {
                        WowInterface.HookManager.StartAutoAttack();
                    }
                }
                else
                {
                    WowInterface.CombatClass.Execute();
                }

                if (WowInterface.ObjectManager.TargetGuid == 0 || WowInterface.ObjectManager.Target == null)
                {
                    if (WowInterface.Globals.ForceCombat)
                    {
                        WowInterface.Globals.ForceCombat = false;
                    }

                    if (StateMachine.GetState<StateIdle>().IsUnitToFollowThere(out WowUnit player))
                    {
                        AmeisenLogger.I.Log("Combat", $"Following {player} because we have nothing else to do");
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Following, player.Position);
                    }
                }
            }
        }

        public override void Leave()
        {
            TargetInLos = true;
            WowInterface.MovementEngine.Reset();

            if (Config.MaxFps != Config.MaxFpsCombat)
            {
                // set our normal maxfps
                WowInterface.HookManager.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFps});SetCVar(\"maxfpsbk\", {Config.MaxFps})");
            }
        }

        private Vector3 GetMeanGroupPosition()
        {
            Vector3 meanGroupPosition = new Vector3();

            foreach (WowUnit unit in WowInterface.ObjectManager.Partymembers)
            {
                if (unit.Guid != WowInterface.ObjectManager.PlayerGuid)
                {
                    meanGroupPosition += unit.Position;
                }
            }

            return meanGroupPosition;
        }

        private double GetMeeleRange()
        {
            return WowInterface.ObjectManager.Target.Type == WowObjectType.Player ? 1.5 : Math.Max(3.0, (WowInterface.ObjectManager.Player.CombatReach + WowInterface.ObjectManager.Target.CombatReach) * 0.75);
        }

        private bool HandleMovement(WowUnit target)
        {
            // handle the LOS check
            if (target == null || target.Guid == WowInterface.ObjectManager.PlayerGuid)
            {
                TargetInLos = true;
            }
            else if (LineOfSightCheck.Run(out bool isInLos, () => WowInterface.HookManager.IsInLineOfSight(WowInterface.ObjectManager.Player.Position, target.Position)))
            {
                TargetInLos = isInLos;
            }

            // set LOS in CombatClass
            if (WowInterface.CombatClass != null)
            {
                WowInterface.CombatClass.TargetInLineOfSight = WowInterface.ObjectManager.TargetGuid == 0 || TargetInLos;
            }

            // check if we are facing the unit
            if (target != null
                && !WowInterface.HookManager.IsClickToMoveActive()
                && FacingCheck.Run()
                && target.Guid != WowInterface.ObjectManager.PlayerGuid
                && !BotMath.IsFacing(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, target.Position))
            {
                WowInterface.HookManager.FacePosition(WowInterface.ObjectManager.Player, target.Position);
            }

            // do we need to move
            if (target == null)
            {
                // just move to our group
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, GetMeanGroupPosition());
                return true;
            }
            else
            {
                double distance = WowInterface.ObjectManager.Player.Position.GetDistance(target.Position);

                if (distance > DistanceToKeep || !TargetInLos)
                {
                    Vector3 positionToGoTo = Vector3.Zero;

                    if (WowInterface.CombatClass != null)
                    {
                        // handle special movement needs
                        if (WowInterface.CombatClass.WalkBehindEnemy)
                        {
                            if (WowInterface.CombatClass.Role == CombatClassRole.Dps
                                && WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid) // prevent spinning
                            {
                                // walk behind enemy
                                positionToGoTo = BotMath.CalculatePositionBehind(target.Position, target.Rotation);
                            }
                            else if (WowInterface.CombatClass.Role == CombatClassRole.Tank
                                && WowInterface.ObjectManager.Partymembers.Any()) // no need to rotate
                            {
                                // rotate the boss away from the group
                                Vector3 meanGroupPosition = GetMeanGroupPosition();
                                positionToGoTo = BotMath.CalculatePositionBehind(target.Position, BotMath.GetFacingAngle2D(target.Position, meanGroupPosition));
                            }
                        }
                        else if (WowInterface.CombatClass.Role == CombatClassRole.Heal)
                        {
                            // move to group
                            positionToGoTo = target != null ? target.Position : GetMeanGroupPosition();
                        }
                        else
                        {
                            // just move to the enemies melee/ranged range
                            positionToGoTo = target.Position;
                        }

                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, positionToGoTo);
                        return true;
                    }

                    // just move to the enemies melee/ranged range
                    positionToGoTo = target.Position;
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, positionToGoTo);
                    return true;
                }
            }

            // no need to move
            WowInterface.MovementEngine.StopMovement();
            return false;
        }
    }
}