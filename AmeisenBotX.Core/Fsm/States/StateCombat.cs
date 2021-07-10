using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Tactic.Bosses.Naxxramas10;
using AmeisenBotX.Core.Engines.Tactic.Bosses.TheObsidianDungeon;
using AmeisenBotX.Core.Engines.Tactic.Dungeon.ForgeOfSouls;
using AmeisenBotX.Core.Engines.Tactic.Dungeon.PitOfSaron;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States
{
    internal class StateCombat : BasicState
    {
        public StateCombat(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
            FacingCheck = new(TimeSpan.FromMilliseconds(100));
        }

        public float DistanceToKeep => Bot.CombatClass == null || Bot.CombatClass.IsMelee ? GetMeeleRange() : 28f;

        private TimegatedEvent FacingCheck { get; set; }

        public override void Enter()
        {
            Bot.Movement.Reset();

            if (Config.MaxFps != Config.MaxFpsCombat)
            {
                Bot.Wow.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFpsCombat});SetCVar(\"maxfpsbk\", {Config.MaxFpsCombat})");
            }

            LoadTactics();
        }

        public override void Execute()
        {
            if (!(Bot.Globals.ForceCombat
                || Bot.Player.IsInCombat
                || IsAnyPartymemberInCombat()
                || Bot.GetEnemiesInCombatWithParty<IWowUnit>(Bot.Player.Position, 100.0f).Any()))
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            // we can do nothing until the ObjectManager is initialzed
            if (Bot.Objects != null && Bot.Player != null)
            {
                bool tacticsMovement = false;
                bool tacticsAllowAttacking = false;

                if (Bot.CombatClass != null)
                {
                    Bot.Tactic.Execute(Bot.CombatClass.Role, Bot.CombatClass.IsMelee, out tacticsMovement, out tacticsAllowAttacking);
                }

                // use the default MovementEngine to move if the CombatClass doesnt
                if (Bot.CombatClass == null || !Bot.CombatClass.HandlesMovement)
                {
                    if (!tacticsMovement)
                    {
                        if (Bot.Wow.TargetGuid == 0 || Bot.Target == null)
                        {
                            if (Bot.Globals.ForceCombat)
                            {
                                Bot.Globals.ForceCombat = false;
                            }

                            if (StateMachine.GetState<StateIdle>().IsUnitToFollowThere(out IWowUnit player))
                            {
                                Bot.Movement.SetMovementAction(MovementAction.Follow, player.Position);
                            }
                        }
                        else
                        {
                            HandleMovement(Bot.Target);
                        }
                    }
                }

                // if no CombatClass is loaded, just autoattack
                if (tacticsAllowAttacking)
                {
                    if (Bot.CombatClass == null)
                    {
                        if (!Bot.Player.IsAutoAttacking)
                        {
                            Bot.Wow.LuaStartAutoAttack();
                        }
                    }
                    else
                    {
                        Bot.CombatClass.Execute();
                    }
                }
            }
        }

        public override void Leave()
        {
            Bot.Movement.Reset();
            Bot.Tactic.Reset();

            if (Config.MaxFps != Config.MaxFpsCombat)
            {
                // set our normal maxfps
                Bot.Wow.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFps});SetCVar(\"maxfpsbk\", {Config.MaxFps})");
            }
        }

        internal bool IsAnyPartymemberInCombat()
        {
            return !Config.OnlySupportMaster && Bot.Objects.WowObjects.OfType<IWowPlayer>()
                .Where(e => Bot.Objects.PartymemberGuids.Contains(e.Guid) && e.Position.GetDistance(Bot.Player.Position) < Config.SupportRange)
                .Any(r => r.IsInCombat);
        }

        private float GetMeeleRange()
        {
            return Bot.Target.Type == WowObjectType.Player ? 1.5f : MathF.Min(3.0f, (Bot.Player.CombatReach + Bot.Target.CombatReach) * 0.9f);
        }

        private bool HandleDpsMovement(IWowUnit target, Vector3 targetPosition)
        {
            // handle special movement needs
            if (Bot.CombatClass.WalkBehindEnemy
                && Bot.Target.TargetGuid != Bot.Wow.PlayerGuid
                || Bot.Target.Type == WowObjectType.Player) // prevent spinning
            {
                // walk behind enemy
                Vector3 positionToGoTo = BotMath.CalculatePositionBehind(target.Position, target.Rotation);
                return Bot.Movement.SetMovementAction(MovementAction.Move, positionToGoTo);
            }
            else
            {
                // just move to the enemies
                return Bot.Movement.SetMovementAction(MovementAction.Move, targetPosition);
            }
        }

        private bool HandleHealMovement(Vector3 targetPosition)
        {
            return Bot.Movement.SetMovementAction(MovementAction.Move, Bot.Objects.IsTargetInLineOfSight ? Bot.Objects.CenterPartyPosition : targetPosition);
        }

        private bool HandleMovement(IWowUnit target)
        {
            // check if we are facing the unit
            if ((Bot.CombatClass == null || !Bot.CombatClass.HandlesFacing)
                && target != null
                && target.Guid != Bot.Wow.PlayerGuid
                && FacingCheck.Run()
                && !Bot.Wow.WowIsClickToMoveActive()
                && !BotMath.IsFacing(Bot.Player.Position, Bot.Player.Rotation, target.Position))
            {
                Bot.Wow.WowFacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);
            }

            // do we need to move
            if (target == null)
            {
                // just move to our group
                return Bot.Movement.SetMovementAction(MovementAction.Move, Bot.Objects.CenterPartyPosition);
            }
            else if (Bot.CombatClass != null)
            {
                Vector3 targetPosition = BotUtils.MoveAhead(target.Position, target.Rotation, 0.5f);
                float distance = Bot.Player.Position.GetDistance(target.Position);

                if (distance > DistanceToKeep || !Bot.Objects.IsTargetInLineOfSight)
                {
                    switch (Bot.CombatClass.Role)
                    {
                        case WowRole.Dps:
                            return Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);
                        // return HandleDpsMovement(target, targetPosition);

                        case WowRole.Tank:
                            return Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);
                        // return HandleTankMovement(target, targetPosition);

                        case WowRole.Heal:
                            return HandleHealMovement(targetPosition);
                    }
                }

                if (distance < DistanceToKeep * 0.08f)
                {
                    // no need to move
                    Bot.Movement.StopMovement();
                }

                return false;
            }

            Bot.Movement.StopMovement();
            return false;
        }

        private bool HandleTankMovement(Vector3 targetPosition)
        {
            // handle special movement needs
            if (Bot.CombatClass.WalkBehindEnemy
                && Bot.CombatClass.Role == WowRole.Tank
                && Bot.Objects.Partymembers.Any()) // no need to rotate
            {
                // rotate the boss away from the group
                // Vector3 meanGroupPosition = Bot.ObjectManager.MeanGroupPosition;
                // Vector3 positionToGoTo = BotMath.CalculatePositionBehind(target.Position, BotMath.GetFacingAngle(target.Position, meanGroupPosition));

                return Bot.Movement.SetMovementAction(MovementAction.Move, targetPosition);
            }
            else
            {
                // just move to the enemies
                return Bot.Movement.SetMovementAction(MovementAction.Move, targetPosition);
            }
        }

        private void LoadTactics()
        {
            if (Bot.Objects.MapId == WowMapId.TheForgeOfSouls)
            {
                if (Bot.Player.Position.GetDistance(new(5297, 2506, 686)) < 70.0f)
                {
                    // Corrupted Soul Fragements
                    Bot.CombatClass.PriorityTargetDisplayIds = new List<int>() { 30233 };
                    Bot.Tactic.LoadTactics(new BronjahmTactic(Bot));
                }
                else if (Bot.Player.Position.GetDistance(new(5662, 2507, 709)) < 120.0f)
                {
                    Bot.Tactic.LoadTactics(new DevourerOfSoulsTactic(Bot));
                }
            }
            else if (Bot.Objects.MapId == WowMapId.PitOfSaron)
            {
                if (Bot.Player.Position.GetDistance(new(823, 110, 509)) < 150.0f)
                {
                    Bot.Tactic.LoadTactics(new IckAndKrickTactic(Bot));
                }
            }
            else if (Bot.Objects.MapId == WowMapId.TheObsidianSanctum)
            {
                // Twilight Eggs
                Bot.CombatClass.PriorityTargetDisplayIds = new List<int>() { 27396 };
                Bot.Tactic.LoadTactics(new TwilightPortalTactic(Bot));
            }
            else if (Bot.Objects.MapId == WowMapId.Naxxramas)
            {
                if (Bot.Player.Position.GetDistance(new(3273, -3476, 287)) < 120.0f)
                {
                    Bot.Tactic.LoadTactics(new AnubRhekan10Tactic(Bot));
                }
            }
        }
    }
}