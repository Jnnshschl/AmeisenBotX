using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Tactic.Dungeon.ForgeOfSouls
{
    public class BronjahmTactic : ITactic
    {
        public BronjahmTactic(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            Configureables = new()
            {
                { "isOffTank", false },
            };
        }

        public static Vector3 MidPosition { get; } = new Vector3(5297, 2506, 686);

        public Dictionary<string, dynamic> Configureables { get; private set; }

        public AmeisenBotInterfaces Bot { get; }

        private static List<int> BronjahmDisplayId { get; } = new List<int> { 30226 };

        public bool ExecuteTactic(WowRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            preventMovement = false;
            allowAttacking = true;

            WowUnit wowUnit = Bot.Objects.GetClosestWowUnitByDisplayId(Bot.Player.Position, BronjahmDisplayId, false);

            if (wowUnit != null)
            {
                if (wowUnit.CurrentlyCastingSpellId == 68872 || wowUnit.CurrentlyChannelingSpellId == 68872 || wowUnit.HasBuffById(68872)) // soulstorm
                {
                    if (Bot.Player.Position.GetDistance(MidPosition) > 8.0)
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Move, BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(Bot.Player.Position, MidPosition), -5.0f));

                        preventMovement = true;
                        allowAttacking = true;
                        return true;
                    }

                    // stay at the mid
                    return false;
                }

                if (role == WowRole.Tank)
                {
                    if (wowUnit.TargetGuid == Bot.Wow.PlayerGuid)
                    {
                        Vector3 modifiedCenterPosition = BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(Bot.Objects.MeanGroupPosition, MidPosition), 8.0f);
                        float distanceToMid = Bot.Player.Position.GetDistance(modifiedCenterPosition);

                        // flee from the corrupted souls target
                        bool needToFlee = wowUnit.CurrentlyChannelingSpellId == 68839
                            || Bot.Objects.WowObjects.OfType<WowUnit>().Any(e => e.DisplayId == 30233 && e.IsInCombat);

                        if (needToFlee)
                        {
                            if (distanceToMid < 16.0f)
                            {
                                Bot.Movement.SetMovementAction(MovementAction.Flee, modifiedCenterPosition);

                                preventMovement = true;
                                allowAttacking = false;
                                return true;
                            }

                            // we cant run away further
                            preventMovement = true;
                            return false;
                        }

                        if (distanceToMid > 5.0f && Bot.Player.Position.GetDistance(wowUnit.Position) < 3.5)
                        {
                            // move the boss to mid
                            Bot.Movement.SetMovementAction(MovementAction.Move, modifiedCenterPosition);

                            preventMovement = true;
                            allowAttacking = false;
                            return true;
                        }
                    }
                }
                else if (role == WowRole.Dps || role == WowRole.Heal)
                {
                    float distanceToMid = Bot.Player.Position.GetDistance(MidPosition);

                    if (!isMelee && distanceToMid < 20.0f)
                    {
                        // move to the outer ring of the arena
                        Bot.Movement.SetMovementAction(MovementAction.Move, BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(Bot.Player.Position, MidPosition), -22.0f));

                        preventMovement = true;
                        allowAttacking = false;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}