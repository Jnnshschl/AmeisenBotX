using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Tactic.Bosses.TheObsidianDungeon
{
    public class TwilightPortalTactic : ITactic
    {
        public TwilightPortalTactic(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            PortalClickEvent = new(TimeSpan.FromSeconds(1));

            Configureables = new()
            {
                { "isOffTank", false },
            };
        }

        public Dictionary<string, dynamic> Configureables { get; private set; }

        private static List<int> DragonDisplayId { get; } = new() { 27421, 27039 };

        private AmeisenBotInterfaces Bot { get; }

        private IWowGameobject NearestPortal => Bot.Objects.WowObjects.OfType<IWowGameobject>().FirstOrDefault(e => e.DisplayId == 1327 && e.Position.GetDistance(Bot.Player.Position) < 80.0);

        private TimegatedEvent PortalClickEvent { get; }

        public bool ExecuteTactic(WowRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            if (role == WowRole.Dps)
            {
                IWowUnit wowUnit = Bot.GetClosestQuestGiverByDisplayId(Bot.Player.Position, DragonDisplayId, false);
                IWowGameobject portal = NearestPortal;

                if (wowUnit != null)
                {
                    if (portal != null && Bot.Player.HealthPercentage > 80.0)
                    {
                        preventMovement = true;
                        allowAttacking = false;

                        UsePortal(portal);

                        return true;
                    }
                }
                else if (portal != null && Bot.Player.HealthPercentage < 25.0)
                {
                    preventMovement = true;
                    allowAttacking = false;

                    UsePortal(portal);

                    return true;
                }
            }

            preventMovement = false;
            allowAttacking = true;
            return false;
        }

        private void UsePortal(IWowGameobject portal)
        {
            if (!Bot.Player.IsInRange(portal, 3.0f))
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, portal.Position);
            }
            else if (PortalClickEvent.Run())
            {
                Bot.Wow.InteractWithObject(portal.BaseAddress);
            }
        }
    }
}