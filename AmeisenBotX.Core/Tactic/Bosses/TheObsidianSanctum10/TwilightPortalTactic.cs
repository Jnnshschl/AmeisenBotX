using AmeisenBotX.Common.Utils;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Data.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Tactic.Bosses.TheObsidianDungeon
{
    public class TwilightPortalTactic : ITactic
    {
        public TwilightPortalTactic(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            PortalClickEvent = new(TimeSpan.FromSeconds(1));

            Configureables = new()
            {
                { "isOffTank", false },
            };
        }

        public Dictionary<string, dynamic> Configureables { get; private set; }

        private static List<int> DragonDisplayId { get; } = new() { 27421, 27039 };

        private WowGameobject NearestPortal => WowInterface.Objects.WowObjects.OfType<WowGameobject>().FirstOrDefault(e => e.DisplayId == 1327 && e.Position.GetDistance(WowInterface.Player.Position) < 80.0);

        private TimegatedEvent PortalClickEvent { get; }

        private WowInterface WowInterface { get; }

        public bool ExecuteTactic(WowRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            if (role == WowRole.Dps)
            {
                WowUnit wowUnit = WowInterface.Objects.GetClosestWowUnitByDisplayId(WowInterface.Player.Position, DragonDisplayId, false);
                WowGameobject portal = NearestPortal;

                if (wowUnit != null)
                {
                    if (portal != null && WowInterface.Player.HealthPercentage > 80.0)
                    {
                        preventMovement = true;
                        allowAttacking = false;

                        UsePortal(portal);

                        return true;
                    }
                }
                else if (portal != null && WowInterface.Player.HealthPercentage < 25.0)
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

        private void UsePortal(WowGameobject portal)
        {
            if (!WowInterface.Player.IsInRange(portal, 3.0f))
            {
                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, portal.Position);
            }
            else if (PortalClickEvent.Run())
            {
                WowInterface.NewWowInterface.WowObjectRightClick(portal.BaseAddress);
            }
        }
    }
}