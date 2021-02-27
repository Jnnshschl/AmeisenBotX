using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
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
            PortalClickEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            Configureables = new Dictionary<string, dynamic>()
            {
                { "isOffTank", false },
            };
        }

        public Dictionary<string, dynamic> Configureables { get; private set; }

        private static List<int> DragonDisplayId { get; } = new List<int> { 27421, 27039 };

        private WowGameobject NearestPortal => WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>().FirstOrDefault(e => e.DisplayId == 1327 && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 80.0);

        private TimegatedEvent PortalClickEvent { get; }

        private WowInterface WowInterface { get; }

        public bool ExecuteTactic(WowRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            if (role == WowRole.Dps)
            {
                WowUnit wowUnit = WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(DragonDisplayId, false);
                WowGameobject portal = NearestPortal;

                if (wowUnit != null)
                {
                    if (portal != null && WowInterface.ObjectManager.Player.HealthPercentage > 80.0)
                    {
                        preventMovement = true;
                        allowAttacking = false;

                        UsePortal(portal);

                        return true;
                    }
                }
                else if (portal != null && WowInterface.ObjectManager.Player.HealthPercentage < 25.0)
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
            if (!WowInterface.ObjectManager.Player.IsInRange(portal, 3.0f))
            {
                WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Move, portal.Position);
            }
            else if (PortalClickEvent.Run())
            {
                WowInterface.HookManager.WowObjectRightClick(portal);
            }
        }
    }
}