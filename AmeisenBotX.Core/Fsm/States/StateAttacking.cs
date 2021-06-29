using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Tactic.Bosses.Naxxramas10;
using AmeisenBotX.Core.Tactic.Bosses.TheObsidianDungeon;
using AmeisenBotX.Core.Tactic.Dungeon.ForgeOfSouls;
using AmeisenBotX.Core.Tactic.Dungeon.PitOfSaron;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    internal class StateAttacking : BasicState
    {
        public StateAttacking(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            FacingCheck = new(TimeSpan.FromMilliseconds(100));
        }

        public float DistanceToKeep => WowInterface.CombatClass == null || WowInterface.CombatClass.IsMelee ? GetMeeleRange() : 28f;

        private TimegatedEvent FacingCheck { get; set; }

        public override void Enter()
        {
            WowInterface.MovementEngine.Reset();

            if (Config.MaxFps != Config.MaxFpsCombat)
            {
                WowInterface.NewWowInterface.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFpsCombat});SetCVar(\"maxfpsbk\", {Config.MaxFpsCombat})");
            }

            LoadTactics();
        }

        public override void Execute()
        {
            if (!(WowInterface.Globals.ForceCombat
                || WowInterface.Player.IsInCombat
                || StateMachine.IsAnyPartymemberInCombat()
                || WowInterface.Objects.GetEnemiesInCombatWithParty<WowUnit>(WowInterface.Db.GetReaction, WowInterface.Player.Position, 100.0f).Any()))
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            // we can do nothing until the ObjectManager is initialzed
            if (WowInterface.Objects != null && WowInterface.Player != null)
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
                        if (WowInterface.Target.Guid == 0 || WowInterface.Target == null)
                        {
                            if (WowInterface.Globals.ForceCombat)
                            {
                                WowInterface.Globals.ForceCombat = false;
                            }

                            if (StateMachine.GetState<StateIdle>().IsUnitToFollowThere(out WowUnit player))
                            {
                                WowInterface.MovementEngine.SetMovementAction(MovementAction.Follow, player.Position);
                            }
                        }
                        else
                        {
                            HandleMovement(WowInterface.Target);
                        }
                    }
                }

                // if no CombatClass is loaded, just autoattack
                if (tacticsAllowAttacking)
                {
                    if (WowInterface.CombatClass == null)
                    {
                        if (!WowInterface.Player.IsAutoAttacking)
                        {
                            WowInterface.NewWowInterface.LuaStartAutoAttack();
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
            WowInterface.MovementEngine.Reset();
            WowInterface.TacticEngine.Reset();

            if (Config.MaxFps != Config.MaxFpsCombat)
            {
                // set our normal maxfps
                WowInterface.NewWowInterface.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFps});SetCVar(\"maxfpsbk\", {Config.MaxFps})");
            }
        }

        private float GetMeeleRange()
        {
            return WowInterface.Target.Type == WowObjectType.Player ? 1.5f : MathF.Min(3.0f, (WowInterface.Player.CombatReach + WowInterface.Target.CombatReach) * 0.9f);
        }

        private bool HandleDpsMovement(WowUnit target, Vector3 targetPosition)
        {
            // handle special movement needs
            if (WowInterface.CombatClass.WalkBehindEnemy
                && WowInterface.Target.TargetGuid != WowInterface.Player.Guid
                || WowInterface.Target.Type == WowObjectType.Player) // prevent spinning
            {
                // walk behind enemy
                Vector3 positionToGoTo = BotMath.CalculatePositionBehind(target.Position, target.Rotation);
                return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, positionToGoTo);
            }
            else
            {
                // just move to the enemies
                return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, targetPosition);
            }
        }

        private bool HandleHealMovement(WowUnit target, Vector3 targetPosition)
        {
            if (WowInterface.Objects.IsTargetInLineOfSight)
            {
                return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowInterface.Objects.MeanGroupPosition);
            }
            else
            {
                return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, targetPosition);
            }
        }

        private bool HandleMovement(WowUnit target)
        {
            // check if we are facing the unit
            if ((WowInterface.CombatClass == null || !WowInterface.CombatClass.HandlesFacing)
                && target != null
                && target.Guid != WowInterface.Player.Guid
                && FacingCheck.Run()
                && !WowInterface.NewWowInterface.WowIsClickToMoveActive()
                && !BotMath.IsFacing(WowInterface.Player.Position, WowInterface.Player.Rotation, target.Position))
            {
                WowInterface.NewWowInterface.WowFacePosition(WowInterface.Player.BaseAddress, WowInterface.Player.Position, target.Position);
            }

            // do we need to move
            if (target == null)
            {
                // just move to our group
                return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowInterface.Objects.MeanGroupPosition);
            }
            else if (WowInterface.CombatClass != null)
            {
                Vector3 targetPosition = BotUtils.MoveAhead(target.Position, target.Rotation, 0.5f);
                float distance = WowInterface.Player.Position.GetDistance(target.Position);

                if (distance > DistanceToKeep || !WowInterface.Objects.IsTargetInLineOfSight)
                {
                    switch (WowInterface.CombatClass.Role)
                    {
                        case WowRole.Dps:
                            return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, target.Position);
                        // return HandleDpsMovement(target, targetPosition);

                        case WowRole.Tank:
                            return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, target.Position);
                        // return HandleTankMovement(target, targetPosition);

                        case WowRole.Heal:
                            return HandleHealMovement(target, targetPosition);
                    }
                }

                if (distance < DistanceToKeep * 0.08f)
                {
                    // no need to move
                    WowInterface.MovementEngine.StopMovement();
                }

                return false;
            }

            WowInterface.MovementEngine.StopMovement();
            return false;
        }

        private bool HandleTankMovement(WowUnit target, Vector3 targetPosition)
        {
            // handle special movement needs
            if (WowInterface.CombatClass.WalkBehindEnemy
                && WowInterface.CombatClass.Role == WowRole.Tank
                && WowInterface.Objects.Partymembers.Any()) // no need to rotate
            {
                // rotate the boss away from the group
                // Vector3 meanGroupPosition = WowInterface.ObjectManager.MeanGroupPosition;
                // Vector3 positionToGoTo = BotMath.CalculatePositionBehind(target.Position, BotMath.GetFacingAngle(target.Position, meanGroupPosition));

                return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, targetPosition);
            }
            else
            {
                // just move to the enemies
                return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, targetPosition);
            }
        }

        private void LoadTactics()
        {
            if (WowInterface.Objects.MapId == WowMapId.TheForgeOfSouls)
            {
                if (WowInterface.Player.Position.GetDistance(new(5297, 2506, 686)) < 70.0f)
                {
                    // Corrupted Soul Fragements
                    WowInterface.CombatClass.PriorityTargetDisplayIds = new List<int>() { 30233 };
                    WowInterface.TacticEngine.LoadTactics(new BronjahmTactic(WowInterface));
                }
                else if (WowInterface.Player.Position.GetDistance(new(5662, 2507, 709)) < 120.0f)
                {
                    WowInterface.TacticEngine.LoadTactics(new DevourerOfSoulsTactic(WowInterface));
                }
            }
            else if (WowInterface.Objects.MapId == WowMapId.PitOfSaron)
            {
                if (WowInterface.Player.Position.GetDistance(new(823, 110, 509)) < 150.0f)
                {
                    WowInterface.TacticEngine.LoadTactics(new IckAndKrickTactic(WowInterface));
                }
            }
            else if (WowInterface.Objects.MapId == WowMapId.TheObsidianSanctum)
            {
                // Twilight Eggs
                WowInterface.CombatClass.PriorityTargetDisplayIds = new List<int>() { 27396 };
                WowInterface.TacticEngine.LoadTactics(new TwilightPortalTactic(WowInterface));
            }
            else if (WowInterface.Objects.MapId == WowMapId.Naxxramas)
            {
                if (WowInterface.Player.Position.GetDistance(new(3273, -3476, 287)) < 120.0f)
                {
                    WowInterface.TacticEngine.LoadTactics(new AnubRhekan10Tactic(WowInterface));
                }
            }
        }
    }
}