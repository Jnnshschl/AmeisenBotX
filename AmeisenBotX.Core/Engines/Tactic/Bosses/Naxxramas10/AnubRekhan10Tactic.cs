using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Storage;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants.Raids.Wotlk.Naxxramas;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Tactic.Bosses.Naxxramas10
{
    public class AnubRekhan10Tactic : SimpleConfigureable, ITactic
    {
        public AnubRekhan10Tactic(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            TankingPathQueue = new();

            Configureables.TryAdd("isOffTank", false);
        }

        public Vector3 Area { get; } = new(3273, -3476, 287);

        public float AreaRadius { get; } = 120.0f;

        public DateTime LocustSwarmActivated { get; private set; }

        public WowMapId MapId { get; } = WowMapId.Naxxramas;

        private static List<int> AddsDisplayIds { get; } = new() { 14698, 27943 };

        private static List<int> AnubRekhanDisplayId { get; } = new() { 15931 };

        private AmeisenBotInterfaces Bot { get; }

        private Vector3 ImpaleDodgePos { get; set; }

        private bool LocustSwarmActive => (LocustSwarmActivated + TimeSpan.FromSeconds(20)) > DateTime.UtcNow;

        private bool MeleeDpsIsMovingToMid { get; set; }

        private List<Vector3> TankingKitingRouteA { get; } = new()
        {
            new Vector3(3323, -3497, 287),
            new Vector3(3312, -3514, 287),
            new Vector3(3294, -3526, 287),
            new Vector3(3273, -3530, 287),
            new Vector3(3252, -3526, 287),
            new Vector3(3235, -3514, 287),
            new Vector3(3223, -3497, 287),
            new Vector3(3220, -3484, 287),
        };

        private List<Vector3> TankingKitingRouteB { get; } = new()
        {
            new Vector3(3223, -3455, 287),
            new Vector3(3235, -3437, 287),
            new Vector3(3252, -3425, 287),
            new Vector3(3274, -3422, 287),
            new Vector3(3294, -3425, 287),
            new Vector3(3312, -3437, 287),
            new Vector3(3324, -3456, 287),
            new Vector3(3326, -3465, 287),
        };

        private Queue<Vector3> TankingPathQueue { get; }

        private Vector3 TankingSpotA { get; } = new(3325, -3486, 287);

        private Vector3 TankingSpotB { get; } = new(3222, -3464, 287);

        private bool TankIsKiting { get; set; }

        private bool TankIsUsingA { get; set; } = true;

        public bool ExecuteTactic(WowRole role, bool isMelee, out bool handlesMovement, out bool allowAttacking)
        {
            return role switch
            {
                WowRole.Tank => DoTank(out handlesMovement, out allowAttacking),
                WowRole.Heal => DoDpsHeal(false, out handlesMovement, out allowAttacking),
                WowRole.Dps => DoDpsHeal(isMelee, out handlesMovement, out allowAttacking),
            };
        }

        private bool DoDpsHeal(bool isMelee, out bool handlesMovement, out bool allowAttacking)
        {
            IWowUnit anubrekhan = Bot.GetClosestQuestGiverByDisplayId(Bot.Player.Position, AnubRekhanDisplayId, false);

            if (anubrekhan != null)
            {
                handlesMovement = true;
                allowAttacking = true;

                // Locust Swarm
                if (anubrekhan.CurrentlyCastingSpellId == AnubRekhan335a.LocustSwarmSpellId)
                {
                    LocustSwarmActivated = DateTime.UtcNow;
                    Bot.CombatClass.BlacklistedTargetDisplayIds = AnubRekhanDisplayId;

                    MeleeDpsIsMovingToMid = true;
                }

                if (!LocustSwarmActive)
                {
                    Bot.CombatClass.BlacklistedTargetDisplayIds = null;
                }

                if (!isMelee)
                {
                    if (anubrekhan.CurrentlyCastingSpellId == AnubRekhan335a.ImpaleSpellId)
                    {
                        if (ImpaleDodgePos == Vector3.Zero)
                        {
                            float angle = new Random().NextDouble() > 0.5 ? MathF.PI + (MathF.PI / 2.0f) : MathF.PI - (MathF.PI / 2.0f);
                            ImpaleDodgePos = BotMath.CalculatePositionAround(Bot.Player.Position, Bot.Player.Rotation, angle, 2.0f);
                        }

                        Bot.Movement.SetMovementAction(MovementAction.DirectMove, ImpaleDodgePos);
                        return true;
                    }
                    else
                    {
                        ImpaleDodgePos = Vector3.Zero;
                    }

                    Vector3 targetPosition = BotUtils.MoveAhead(Area, anubrekhan.Position, -30.0f);

                    if (!LocustSwarmActive && Bot.Player.Position.GetDistance(Area) > 6.0f)
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Move, targetPosition);
                        return true;
                    }
                }
                else
                {
                    if (MeleeDpsIsMovingToMid)
                    {
                        if (Bot.Player.Position.GetDistance(Area) > 24.0f)
                        {
                            Bot.Movement.SetMovementAction(MovementAction.Move, Area);
                            return true;
                        }
                        else
                        {
                            MeleeDpsIsMovingToMid = false;
                        }
                    }

                    handlesMovement = false;
                    allowAttacking = true;
                    return false;
                }
            }

            handlesMovement = false;
            allowAttacking = true;
            return false;
        }

        private bool DoTank(out bool handlesMovement, out bool allowAttacking)
        {
            handlesMovement = false;
            allowAttacking = true;

            IWowUnit anubrekhan = Bot.GetClosestQuestGiverByDisplayId(Bot.Player.Position, AnubRekhanDisplayId, false);

            if (anubrekhan != null)
            {
                if (Configureables["isOffTank"] == true)
                {
                    // offtank should only focus adds
                    Bot.CombatClass.BlacklistedTargetDisplayIds = AnubRekhanDisplayId;
                }
                else
                {
                    Bot.CombatClass.BlacklistedTargetDisplayIds = AddsDisplayIds;

                    // Locust Swarm
                    if (anubrekhan.CurrentlyCastingSpellId == AnubRekhan335a.LocustSwarmSpellId)
                    {
                        TankIsKiting = true;
                    }

                    if (!TankIsKiting)
                    {
                        if (anubrekhan.TargetGuid == Bot.Wow.PlayerGuid)
                        {
                            Vector3 tankingSpot = TankIsUsingA ? TankingSpotA : TankingSpotB;

                            if (Bot.Player.DistanceTo(tankingSpot) > 10.0f)
                            {
                                Bot.Movement.SetMovementAction(MovementAction.DirectMove, tankingSpot);
                                handlesMovement = true;
                            }
                            else
                            {
                                if (anubrekhan.CurrentlyCastingSpellId == AnubRekhan335a.ImpaleSpellId)
                                {
                                    if (ImpaleDodgePos == Vector3.Zero)
                                    {
                                        float angle = MathF.PI + new Random().NextDouble() > 0.5 ? BotMath.HALF_PI : -BotMath.HALF_PI;
                                        ImpaleDodgePos = BotMath.CalculatePositionAround(Bot.Player.Position, Bot.Player.Rotation, angle, 5.0f);
                                    }

                                    Bot.Movement.SetMovementAction(MovementAction.DirectMove, ImpaleDodgePos);
                                    return true;
                                }
                                else
                                {
                                    ImpaleDodgePos = Vector3.Zero;
                                }
                            }
                        }
                    }
                    else
                    {
                        allowAttacking = false;

                        if (TankingPathQueue.Count == 0)
                        {
                            foreach (Vector3 v in TankIsUsingA ? TankingKitingRouteA : TankingKitingRouteB)
                            {
                                TankingPathQueue.Enqueue(v);
                            }
                        }
                        else
                        {
                            Vector3 targetPosition = TankingPathQueue.Peek();

                            if (targetPosition.GetDistance2D(Bot.Player.Position) > 2.0f)
                            {
                                Bot.Movement.SetMovementAction(MovementAction.DirectMove, targetPosition);
                                handlesMovement = true;
                            }
                            else
                            {
                                TankingPathQueue.Dequeue();

                                if (TankingPathQueue.Count == 0)
                                {
                                    TankIsKiting = false;
                                    TankIsUsingA = !TankIsUsingA;
                                }
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }
}