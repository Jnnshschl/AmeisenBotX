using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Tactic;
using AmeisenBotX.Core.Tactic.Bosses.Naxxramas10;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    internal class StateAttacking : BasicState
    {
        public StateAttacking(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            FacingCheck = new TimegatedEvent(TimeSpan.FromMilliseconds(100));
            LineOfSightCheck = new TimegatedEvent<bool>(TimeSpan.FromMilliseconds(1000));
        }

        public float DistanceToKeep => WowInterface.CombatClass == null || WowInterface.CombatClass.IsMelee ? GetMeeleRange() : 28f;

        public bool TargetInLos { get; private set; }

        private TimegatedEvent FacingCheck { get; set; }

        private TimegatedEvent<bool> LineOfSightCheck { get; set; }

        public override void Enter()
        {
            WowInterface.MovementEngine.Reset();

            if (Config.MaxFps != Config.MaxFpsCombat)
            {
                WowInterface.HookManager.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFpsCombat});SetCVar(\"maxfpsbk\", {Config.MaxFpsCombat})");
            }

            if (WowInterface.ObjectManager.MapId == MapId.Naxxramas)
            {
                // Anub Rhekan
                if (WowInterface.ObjectManager.Player.Position.GetDistance(new Vector3(3273, -3476, 287)) < 100.0)
                {
                    WowInterface.TacticEngine.LoadTactics(new SortedList<int, ITactic>() { { 0, new AnubRhekan10Tactic(WowInterface) } });
                }
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
                bool tacticsMovement = false;
                bool tacticsAllowAttacking = false;

                if (WowInterface.CombatClass != null)
                {
                    WowInterface.TacticEngine.Execute(WowInterface.CombatClass.Role, WowInterface.CombatClass.IsMelee, out tacticsMovement, out tacticsAllowAttacking);
                }

                // use the default MovementEngine to move if the CombatClass doesnt
                if (WowInterface.CombatClass == null || !WowInterface.CombatClass.HandlesMovement)
                {
                    if (!tacticsMovement)
                    {
                        if (WowInterface.ObjectManager.TargetGuid == 0 || WowInterface.ObjectManager.Target == null)
                        {
                            if (WowInterface.Globals.ForceCombat)
                            {
                                WowInterface.Globals.ForceCombat = false;
                            }

                            if (StateMachine.GetState<StateIdle>().IsUnitToFollowThere(out WowUnit player))
                            {
                                WowInterface.MovementEngine.SetMovementAction(MovementAction.Following, player.Position);
                            }
                        }
                        else
                        {
                            HandleMovement(WowInterface.ObjectManager.Target);
                        }
                    }
                }

                // if no CombatClass is loaded, just autoattack
                if (tacticsAllowAttacking)
                {
                    if (WowInterface.CombatClass == null)
                    {
                        if (!WowInterface.ObjectManager.Player.IsAutoAttacking)
                        {
                            WowInterface.HookManager.LuaStartAutoAttack();
                        }
                    }
                    else
                    {
                        WowInterface.CombatClass.Execute();
                    }
                }
            }
        }

        public override void Leave()
        {
            TargetInLos = true;

            WowInterface.MovementEngine.Reset();
            WowInterface.TacticEngine.Reset();

            if (Config.MaxFps != Config.MaxFpsCombat)
            {
                // set our normal maxfps
                WowInterface.HookManager.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFps});SetCVar(\"maxfpsbk\", {Config.MaxFps})");
            }
        }

        private Vector3 GetMeanGroupPosition()
        {
            Vector3 meanGroupPosition = new Vector3();
            int count = 0;

            foreach (WowUnit unit in WowInterface.ObjectManager.Partymembers)
            {
                if (unit.Guid != WowInterface.ObjectManager.PlayerGuid && unit.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 100.0)
                {
                    meanGroupPosition += unit.Position;
                    ++count;
                }
            }

            return meanGroupPosition / count;
        }

        private float GetMeeleRange()
        {
            return WowInterface.ObjectManager.Target.Type == WowObjectType.Player ? 1.5f : MathF.Max(3.0f, (WowInterface.ObjectManager.Player.CombatReach + WowInterface.ObjectManager.Target.CombatReach) * 0.9f);
        }

        private bool HandleMovement(WowUnit target)
        {
            // handle the LOS check
            if (target == null || target.Guid == WowInterface.ObjectManager.PlayerGuid)
            {
                TargetInLos = true;
            }
            else if (LineOfSightCheck.Run(out bool isInLos, () => WowInterface.HookManager.WowIsInLineOfSight(WowInterface.ObjectManager.Player.Position, target.Position)))
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
                && !WowInterface.HookManager.WowIsClickToMoveActive()
                && FacingCheck.Run()
                && target.Guid != WowInterface.ObjectManager.PlayerGuid
                && !BotMath.IsFacing(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, target.Position))
            {
                WowInterface.HookManager.WowFacePosition(WowInterface.ObjectManager.Player, target.Position);
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
                float distance = WowInterface.ObjectManager.Player.Position.GetDistance(target.Position);

                if (distance > DistanceToKeep || !TargetInLos)
                {
                    Vector3 positionToGoTo = Vector3.Zero;

                    if (WowInterface.CombatClass != null)
                    {
                        // handle special movement needs
                        if (WowInterface.CombatClass.WalkBehindEnemy)
                        {
                            if (WowInterface.CombatClass.Role == CombatClassRole.Dps
                                && (WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                                    || WowInterface.ObjectManager.Target.Type == WowObjectType.Player)) // prevent spinning
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

                        if (TargetInLos)
                        {
                            positionToGoTo = BotUtils.MoveAhead(WowInterface.ObjectManager.Player.Position, positionToGoTo, -(DistanceToKeep * 0.8f));
                        }

                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, positionToGoTo);
                        return true;
                    }

                    if (TargetInLos)
                    {
                        positionToGoTo = BotUtils.MoveAhead(WowInterface.ObjectManager.Player.Position, positionToGoTo, -(DistanceToKeep * 0.8f));
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