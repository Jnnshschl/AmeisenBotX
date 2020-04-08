using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Statemachine.States
{
    internal class StateAttacking : BasicState
    {
        public StateAttacking(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            // default distance values
            DistanceToTarget = WowInterface.CombatClass == null || WowInterface.CombatClass.IsMelee ? 2 : 25.0;
        }

        public double DistanceToTarget { get; private set; }

        private DateTime LastFacingCheck { get; set; }

        private WowUnit LastTarget { get; set; }

        public override void Enter()
        {
            WowInterface.MovementEngine.Reset();

            WowInterface.XMemory.Write(WowInterface.OffsetList.CvarMaxFps, Config.MaxFpsCombat);
        }

        public override void Execute()
        {
            WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Player);

            if (!WowInterface.ObjectManager.Player.IsInCombat
                && !StateMachine.IsAnyPartymemberInCombat()
                && (WowInterface.BattlegroundEngine == null || !WowInterface.BattlegroundEngine.ForceCombat))
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            // we can do nothing until the ObjectManager is initialzed
            if (WowInterface.ObjectManager != null
                && WowInterface.ObjectManager.Player != null)
            {
                // use the default MovementEngine to move if the CombatClass doesnt
                if ((WowInterface.CombatClass == null || !WowInterface.CombatClass.HandlesMovement))
                {
                    // we cant move to a target that not exists
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
                    // default action to defend ourself
                    if (!WowInterface.ObjectManager.Player.IsAutoAttacking)
                    {
                        WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
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
    }
}