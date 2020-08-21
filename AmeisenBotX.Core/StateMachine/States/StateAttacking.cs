using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Linq;

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

        public Vector3 Offset { get; set; }

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
            if (WowInterface.ObjectManager != null
                && WowInterface.ObjectManager.Player != null)
            {
                // use the default MovementEngine to move if the CombatClass doesnt
                if ((WowInterface.CombatClass == null || !WowInterface.CombatClass.HandlesMovement)
                    && WowInterface.ObjectManager.TargetGuid != 0 && WowInterface.ObjectManager.Target != null)
                {
                    if (WowInterface.ObjectManager.Target.Guid == WowInterface.ObjectManager.PlayerGuid
                        || WowInterface.ObjectManager.Target.IsDead
                        || !BotUtils.IsValidUnit(WowInterface.ObjectManager.Target))
                    {
                        WowInterface.MovementEngine.Reset();
                    }
                    else
                    {
                        HandleMovement(WowInterface.ObjectManager.Target);
                    }
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

                if (WowInterface.ObjectManager.TargetGuid == 0)
                {
                    if (WowInterface.Globals.ForceCombat)
                    {
                        WowInterface.Globals.ForceCombat = false;
                    }

                    if (StateMachine.GetState<StateIdle>().IsUnitToFollowThere(out WowUnit player))
                    {
                        if (RandomPosEvent.Run())
                        {
                            Offset = WowInterface.PathfindingHandler.GetRandomPointAround((int)WowInterface.ObjectManager.MapId, player.Position, Config.MinFollowDistance * 0.3f);
                        }

                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Following, player.Position + Offset);
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

        private double GetMeeleRange()
        {
            return WowInterface.ObjectManager.Target.Type == WowObjectType.Player ? 1.5 : Math.Max(3.0, (WowInterface.ObjectManager.Player.CombatReach + WowInterface.ObjectManager.Target.CombatReach) * 0.75);
        }

        private bool HandleMovement(WowUnit target)
        {
            if (target.Guid == WowInterface.ObjectManager.PlayerGuid)
            {
                TargetInLos = true;
            }
            else if (LineOfSightCheck.Run(out bool isInLos, () => WowInterface.HookManager.IsInLineOfSight(WowInterface.ObjectManager.Player.Position, target.Position)))
            {
                TargetInLos = isInLos;
            }

            if (WowInterface.CombatClass != null)
            {
                WowInterface.CombatClass.TargetInLineOfSight = WowInterface.ObjectManager.TargetGuid == 0 || TargetInLos;
            }

            if (!WowInterface.HookManager.IsClickToMoveActive()
                && FacingCheck.Run()
                && target.Guid != WowInterface.ObjectManager.PlayerGuid
                && !BotMath.IsFacing(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, target.Position))
            {
                WowInterface.HookManager.FacePosition(WowInterface.ObjectManager.Player, target.Position);
            }

            // if we are close enough, stop movement and start attacking
            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(target.Position);

            bool needToRotate = false;
            Vector3 meanGroupPosition = new Vector3();

            // let the tank rotate the boss away from the group
            if (WowInterface.CombatClass != null
                && WowInterface.CombatClass.Role == CombatClassRole.Tank
                && WowInterface.CombatClass.WalkBehindEnemy
                && WowInterface.ObjectManager.Partymembers.Any())
            {
                foreach (WowUnit unit in WowInterface.ObjectManager.Partymembers)
                {
                    if (unit.Guid != WowInterface.ObjectManager.PlayerGuid)
                    {
                        meanGroupPosition += unit.Position;
                    }
                }

                needToRotate = BotMath.IsFacing(target.Position, target.Rotation, meanGroupPosition);
            }

            if (distance > DistanceToKeep || !TargetInLos || needToRotate)
            {
                Vector3 positionToGoTo = Vector3.Zero;

                if (WowInterface.CombatClass != null && WowInterface.CombatClass.Role != CombatClassRole.Heal && WowInterface.CombatClass.WalkBehindEnemy)
                {
                    if (WowInterface.CombatClass.Role == CombatClassRole.Dps)
                    {
                        positionToGoTo = BotMath.CalculatePositionBehind(target.Position, target.Rotation);
                    }
                    else
                    {
                        positionToGoTo = BotMath.CalculatePositionBehind(target.Position, BotMath.GetFacingAngle2D(target.Position, meanGroupPosition));
                    }
                }
                else if (WowInterface.CombatClass.Role != CombatClassRole.Heal)
                {
                    positionToGoTo = BotUtils.MoveAhead(target.Position, BotMath.GetFacingAngle2D(WowInterface.ObjectManager.Player.Position, target.Position), (float)GetMeeleRange() / 2.0f * -1f);
                }
                else
                {
                    positionToGoTo = BotUtils.MoveAhead(target.Position, BotMath.GetFacingAngle2D(target.Position, WowInterface.ObjectManager.Player.Position), 24.0f);
                }

                WowInterface.MovementEngine.SetMovementAction(distance > 6.0 ? MovementAction.Moving : MovementAction.DirectMove, positionToGoTo);
                return true;
            }

            return false;
        }
    }
}