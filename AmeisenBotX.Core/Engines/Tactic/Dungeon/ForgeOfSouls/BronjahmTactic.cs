using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Tactic.Dungeon.ForgeOfSouls
{
    public class BronjahmTactic : ITactic
    {
        public BronjahmTactic(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            Configurables = new()
            {
                { "isOffTank", false },
            };
        }

        public Vector3 Area { get; } = new(5297, 2506, 686);

        public float AreaRadius { get; } = 70.0f;

        public AmeisenBotInterfaces Bot { get; }

        public Dictionary<string, dynamic> Configurables { get; private set; }

        public WowMapId MapId { get; } = WowMapId.TheForgeOfSouls;

        private static List<int> BronjahmDisplayId { get; } = [30226];

        public bool ExecuteTactic(WowRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            preventMovement = false;
            allowAttacking = true;

            IWowUnit wowUnit = Bot.GetClosestQuestGiverByDisplayId(Bot.Player.Position, BronjahmDisplayId, false);

            if (wowUnit != null)
            {
                if (wowUnit.CurrentlyCastingSpellId == 68872 || wowUnit.CurrentlyChannelingSpellId == 68872 || wowUnit.HasBuffById(68872)) // soulstorm
                {
                    if (Bot.Player.Position.GetDistance(Area) > 8.0)
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Move, BotUtils.MoveAhead(Area, BotMath.GetFacingAngle(Bot.Player.Position, Area), -5.0f));

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
                        Vector3 modifiedCenterPosition = BotUtils.MoveAhead(Area, BotMath.GetFacingAngle(Bot.Objects.CenterPartyPosition, Area), 8.0f);
                        float distanceToMid = Bot.Player.Position.GetDistance(modifiedCenterPosition);

                        // flee from the corrupted souls target
                        bool needToFlee = wowUnit.CurrentlyChannelingSpellId == 68839
                            || Bot.Objects.All.OfType<IWowUnit>().Any(e => e.DisplayId == 30233 && e.IsInCombat);

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
                else if (role is WowRole.Dps or WowRole.Heal)
                {
                    float distanceToMid = Bot.Player.Position.GetDistance(Area);

                    if (!isMelee && distanceToMid < 20.0f)
                    {
                        // move to the outer ring of the arena
                        Bot.Movement.SetMovementAction(MovementAction.Move, BotUtils.MoveAhead(Area, BotMath.GetFacingAngle(Bot.Player.Position, Area), -22.0f));

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