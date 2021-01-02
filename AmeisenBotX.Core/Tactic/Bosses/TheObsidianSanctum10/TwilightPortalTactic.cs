using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Common.Extensions;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Tactic.Bosses.TheObsidianDungeon
{
    public class TwilightPortalTactic : ITactic
    {
        public TwilightPortalTactic(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            PortalClickEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
        }

        private static List<int> DragonDisplayId { get; } = new List<int> { 27421, 27039 };

        private TimegatedEvent PortalClickEvent { get; }

        private WowInterface WowInterface { get; }

        private WowGameobject NearestPortal => WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>().FirstOrDefault(e => e.DisplayId == 1327 && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 80.0);

        public bool ExecuteTactic(CombatClassRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            WowUnit wowUnit = WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(DragonDisplayId, false);

            if (wowUnit != null)
            {
                WowGameobject portal = NearestPortal;

                if (portal != null)
                {
                    preventMovement = true;
                    allowAttacking = false;

                    if (!WowInterface.ObjectManager.Player.IsInRange(portal, 3.0f))
                    {
                        WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, portal.Position);
                    }
                    else if (PortalClickEvent.Run())
                    {
                        WowInterface.HookManager.WowObjectRightClick(portal);
                    }

                    return true;
                }
            }

            preventMovement = false;
            allowAttacking = true;
            return false;
        }
    }
}
