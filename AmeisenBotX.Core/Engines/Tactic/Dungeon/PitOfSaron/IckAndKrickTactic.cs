using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Tactic.Dungeon.PitOfSaron
{
    public class IckAndKrickTactic : ITactic
    {
        public IckAndKrickTactic(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            Configureables = new()
            {
                { "isOffTank", false },
            };
        }

        public static Vector3 MidPosition { get; } = new Vector3(823, 110, 509);

        public DateTime ChasingActivated { get; private set; }

        public Dictionary<string, dynamic> Configureables { get; private set; }

        private static List<int> IckDisplayId { get; } = new List<int> { 30347 };

        private AmeisenBotInterfaces Bot { get; }

        private bool ChasingActive => (ChasingActivated + TimeSpan.FromSeconds(14)) > DateTime.UtcNow;

        public bool ExecuteTactic(WowRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            preventMovement = false;
            allowAttacking = true;

            IWowUnit wowUnit = Bot.GetClosestQuestgiverByDisplayId(Bot.Player.Position, IckDisplayId, false);

            if (wowUnit != null)
            {
                if (wowUnit.CurrentlyCastingSpellId == 68987 || wowUnit.CurrentlyChannelingSpellId == 68987) // chasing
                {
                    ChasingActivated = DateTime.UtcNow;
                    return true;
                }
                else if (ChasingActive && wowUnit.TargetGuid == Bot.Wow.PlayerGuid && wowUnit.Position.GetDistance(Bot.Player.Position) < 7.0f)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Flee, wowUnit.Position);

                    preventMovement = true;
                    allowAttacking = false;
                    return true;
                }

                IWowUnit unitOrb = Bot.Objects.WowObjects.OfType<IWowUnit>()
                    .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                    .FirstOrDefault(e => e.DisplayId == 11686 && e.HasBuffById(69017) && e.Position.GetDistance(Bot.Player.Position) < 3.0f);

                if (unitOrb != null) // orbs
                {
                    Bot.Movement.SetMovementAction(MovementAction.Flee, unitOrb.Position);

                    preventMovement = true;
                    allowAttacking = false;
                    return true;
                }

                if (role == WowRole.Tank)
                {
                    if (wowUnit.TargetGuid == Bot.Wow.PlayerGuid)
                    {
                        Vector3 modifiedCenterPosition = BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(Bot.Objects.CenterPartyPosition, MidPosition), 8.0f);
                        float distanceToMid = Bot.Player.Position.GetDistance(modifiedCenterPosition);

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
            }

            return false;
        }
    }
}