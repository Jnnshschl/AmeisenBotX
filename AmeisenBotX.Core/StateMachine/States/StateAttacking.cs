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

            LoadTactics();
        }

        public override void Execute()
        {
            if (!(WowInterface.Globals.ForceCombat || WowInterface.ObjectManager.Player.IsInCombat || StateMachine.IsAnyPartymemberInCombat()
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
            TargetInLos = true;

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

        private bool HandleMovement(WowUnit target)
        {
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
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowInterface.ObjectManager.MeanGroupPosition);
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
                                Vector3 meanGroupPosition = WowInterface.ObjectManager.MeanGroupPosition;
                                positionToGoTo = BotMath.CalculatePositionBehind(target.Position, BotMath.GetFacingAngle(target.Position, meanGroupPosition));
                            }
                        }
                        else if (WowInterface.CombatClass.Role == CombatClassRole.Heal)
                        {
                            // move to group
                            positionToGoTo = target != null ? target.Position : WowInterface.ObjectManager.MeanGroupPosition;
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

                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, positionToGoTo);
                        return true;
                    }

                    if (TargetInLos)
                    {
                        positionToGoTo = BotUtils.MoveAhead(WowInterface.ObjectManager.Player.Position, positionToGoTo, -(DistanceToKeep * 0.8f));
                    }

                    // just move to the enemies melee/ranged range
                    positionToGoTo = target.Position;
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, positionToGoTo);
                    return true;
                }
            }

            // no need to move
            WowInterface.MovementEngine.StopMovement();
            return false;
        }

        private void LoadTactics()
        {
            if (WowInterface.ObjectManager.MapId == MapId.TheForgeOfSouls)
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(new Vector3(5297, 2506, 686)) < 70.0)
                {
                    // Corrupted Soul Fragements
                    WowInterface.I.CombatClass.PriorityTargetDisplayIds = new List<int>() { 30233 };
                    WowInterface.TacticEngine.LoadTactics(new BronjahmTactic());
                }
                else if (WowInterface.ObjectManager.Player.Position.GetDistance(new Vector3(5662, 2507, 709)) < 120.0)
                {
                    WowInterface.TacticEngine.LoadTactics(new DevourerOfSoulsTactic());
                }
            }
            else if (WowInterface.ObjectManager.MapId == MapId.PitOfSaron)
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(new Vector3(823, 110, 509)) < 150.0)
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
                if (WowInterface.ObjectManager.Player.Position.GetDistance(new Vector3(3273, -3476, 287)) < 120.0)
                {
                    WowInterface.TacticEngine.LoadTactics(new AnubRhekan10Tactic(WowInterface));
                }
            }
        }
    }
}