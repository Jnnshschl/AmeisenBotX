using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Tactic.Bosses.Naxxramas10;
using AmeisenBotX.Core.Tactic.Bosses.TheObsidianDungeon;
using AmeisenBotX.Core.Tactic.Dungeon.ForgeOfSouls;
using AmeisenBotX.Core.Tactic.Dungeon.PitOfSaron;
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
        }

        public float DistanceToKeep => WowInterface.CombatClass == null || WowInterface.CombatClass.IsMelee ? GetMeeleRange() : 28f;

        private TimegatedEvent FacingCheck { get; set; }

        public override void Enter()
        {
            WowInterface.MovementEngine.Reset();

            if (Config.MaxFps != Config.MaxFpsCombat)
            {
                WowInterface.HookManager.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFpsCombat});SetCVar(\"maxfpsbk\", {Config.MaxFpsCombat})");
            }

            LoadTactics();
        }

        public override void Execute()
        {
            if (!(WowInterface.Globals.ForceCombat
                || WowInterface.ObjectManager.Player.IsInCombat
                || StateMachine.IsAnyPartymemberInCombat()
                || WowInterface.ObjectManager.GetEnemiesInCombatWithUs<WowUnit>(WowInterface.ObjectManager.Player.Position, 100.0).Any()))
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
                                WowInterface.MovementEngine.SetMovementAction(MovementAction.Follow, player.Position);
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
            WowInterface.MovementEngine.Reset();
            WowInterface.TacticEngine.Reset();

            if (Config.MaxFps != Config.MaxFpsCombat)
            {
                // set our normal maxfps
                WowInterface.HookManager.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFps});SetCVar(\"maxfpsbk\", {Config.MaxFps})");
            }
        }

        private float GetMeeleRange()
        {
            return WowInterface.ObjectManager.Target.Type == WowObjectType.Player ? 1.5f : MathF.Max(3.0f, (WowInterface.ObjectManager.Player.CombatReach + WowInterface.ObjectManager.Target.CombatReach) * 0.9f);
        }

        private bool HandleDpsMovement(WowUnit target, Vector3 targetPosition)
        {
            // handle special movement needs
            if (WowInterface.CombatClass.WalkBehindEnemy
                && WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                || WowInterface.ObjectManager.Target.Type == WowObjectType.Player) // prevent spinning
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
            if (WowInterface.ObjectManager.IsTargetInLineOfSight)
            {
                return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowInterface.ObjectManager.MeanGroupPosition);
            }
            else
            {
                return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, targetPosition);
            }
        }

        private bool HandleMovement(WowUnit target)
        {
            // check if we are facing the unit
            if (target != null
                && target.Guid != WowInterface.ObjectManager.PlayerGuid
                && FacingCheck.Run()
                && !WowInterface.HookManager.WowIsClickToMoveActive()
                && !BotMath.IsFacing(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, target.Position))
            {
                WowInterface.HookManager.WowFacePosition(WowInterface.ObjectManager.Player, target.Position);
            }

            // do we need to move
            if (target == null)
            {
                // just move to our group
                return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowInterface.ObjectManager.MeanGroupPosition);
            }
            else if (WowInterface.CombatClass != null)
            {
                Vector3 targetPosition = BotUtils.MoveAhead(target.Position, target.Rotation, 1.5f);
                float distance = WowInterface.ObjectManager.Player.Position.GetDistance(target.Position);

                if (distance > DistanceToKeep || !WowInterface.ObjectManager.IsTargetInLineOfSight)
                {
                    return WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, target.Position);

                    switch (WowInterface.CombatClass.Role)
                    {
                        case CombatClassRole.Dps:
                            return HandleDpsMovement(target, targetPosition);

                        case CombatClassRole.Tank:
                            return HandleTankMovement(target, targetPosition);

                        case CombatClassRole.Heal:
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
                && WowInterface.CombatClass.Role == CombatClassRole.Tank
                && WowInterface.ObjectManager.Partymembers.Any()) // no need to rotate
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
            if (WowInterface.ObjectManager.MapId == MapId.TheForgeOfSouls)
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(new Vector3(5297, 2506, 686)) < 70.0f)
                {
                    // Corrupted Soul Fragements
                    WowInterface.I.CombatClass.PriorityTargetDisplayIds = new List<int>() { 30233 };
                    WowInterface.TacticEngine.LoadTactics(new BronjahmTactic());
                }
                else if (WowInterface.ObjectManager.Player.Position.GetDistance(new Vector3(5662, 2507, 709)) < 120.0f)
                {
                    WowInterface.TacticEngine.LoadTactics(new DevourerOfSoulsTactic());
                }
            }
            else if (WowInterface.ObjectManager.MapId == MapId.PitOfSaron)
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(new Vector3(823, 110, 509)) < 150.0f)
                {
                    WowInterface.TacticEngine.LoadTactics(new IckAndKrickTactic());
                }
            }
            else if (WowInterface.ObjectManager.MapId == MapId.TheObsidianSanctum)
            {
                // Twilight Eggs
                WowInterface.I.CombatClass.PriorityTargetDisplayIds = new List<int>() { 27396 };
                WowInterface.TacticEngine.LoadTactics(new TwilightPortalTactic(WowInterface));
            }
            else if (WowInterface.ObjectManager.MapId == MapId.Naxxramas)
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(new Vector3(3273, -3476, 287)) < 120.0f)
                {
                    WowInterface.TacticEngine.LoadTactics(new AnubRhekan10Tactic(WowInterface));
                }
            }
        }
    }
}