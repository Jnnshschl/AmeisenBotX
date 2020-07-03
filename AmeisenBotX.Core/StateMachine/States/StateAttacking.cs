using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Statemachine.States
{
    internal class StateAttacking : BasicState
    {
        public StateAttacking(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            FacingCheck = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
            LineOfSightCheck = new TimegatedEvent<bool>(TimeSpan.FromMilliseconds(2500));
        }

        public double DistanceToTarget => WowInterface.CombatClass == null || WowInterface.CombatClass.IsMelee ? 3.0 : 28.0;

        public bool IsFacing { get; private set; }

        public bool TargetInLos { get; private set; }

        private TimegatedEvent FacingCheck { get; set; }

        private TimegatedEvent<bool> LineOfSightCheck { get; set; }

        public override void Enter()
        {
            WowInterface.MovementEngine.Reset();
            WowInterface.XMemory.Write(WowInterface.OffsetList.CvarMaxFps, Config.MaxFpsCombat);
        }

        public override void Execute()
        {
            if (WowInterface.Globals.ForceCombat && WowInterface.ObjectManager.TargetGuid == 0)
            {
                WowInterface.Globals.ForceCombat = false;
            }

            if (!WowInterface.ObjectManager.Player.IsInCombat
                && !StateMachine.IsAnyPartymemberInCombat()
                && !WowInterface.Globals.ForceCombat
                && StateMachine.SetState((int)BotState.Idle))
            {
                return;
            }

            // we can do nothing until the ObjectManager is initialzed
            if (WowInterface.ObjectManager != null
                && WowInterface.ObjectManager.Player != null)
            {
                // use the default MovementEngine to move if the CombatClass doesnt
                if (WowInterface.CombatClass == null || !WowInterface.CombatClass.HandlesMovement)
                {
                    if (WowInterface.ObjectManager.TargetGuid != 0)
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
                }

                // if no CombatClass is loaded, just autoattack
                if (WowInterface.CombatClass == null)
                {
                    if (!WowInterface.ObjectManager.Player.IsAutoAttacking)
                    {
                        WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
                    }
                }
                else
                {
                    WowInterface.CombatClass.Execute();
                }
            }
        }

        public override void Exit()
        {
            WowInterface.MovementEngine.Reset();

            // set our normal maxfps
            WowInterface.XMemory.Write(WowInterface.OffsetList.CvarMaxFps, Config.MaxFps);
        }

        private bool HandleMovement(WowUnit target)
        {
            if (LineOfSightCheck.Run(out bool isInLos, () => WowInterface.HookManager.IsInLineOfSight(WowInterface.ObjectManager.Player.Position, target.Position)))
            {
                TargetInLos = isInLos;
            }

            if (!WowInterface.HookManager.IsClickToMoveActive()
                && FacingCheck.Run()
                && !BotMath.IsFacing(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, target.Position))
            {
                WowInterface.HookManager.FacePosition(WowInterface.ObjectManager.Player, target.Position);
            }

            // if we are close enough, stop movement and start attacking
            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(target.Position);

            if (distance > DistanceToTarget || !TargetInLos)
            {
                Vector3 positionToGoTo = BotUtils.MoveAhead(target.Position, BotMath.GetFacingAngle2D(WowInterface.ObjectManager.Player.Position, target.Position), 2.0); // WowInterface.CombatClass.IsMelee ? BotMath.CalculatePositionBehind(target.Position, target.Rotation, 4) :
                WowInterface.MovementEngine.SetMovementAction(distance > 8.0 ? MovementAction.Moving : MovementAction.DirectMove, positionToGoTo);
                return true;
            }

            return false;
        }
    }
}