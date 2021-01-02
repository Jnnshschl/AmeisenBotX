using AmeisenBotX.Core.Common;
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
    public class Shadron10Tactic : ITactic
    {
        public Shadron10Tactic(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            PortalClickEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
        }

        private static List<int> ShadronDisplayId { get; } = new List<int> { 27421 };

        private TimegatedEvent PortalClickEvent { get; }

        private WowInterface WowInterface { get; }

        private WowGameobject NearestPortal => WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>().FirstOrDefault(e => e.DisplayId == 1327 && e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 80.0);

        public bool ExecuteTactic(CombatClassRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            if (role == CombatClassRole.Dps)
            {
                return DoDps(out preventMovement, out allowAttacking);
            }
            else
            {
                preventMovement = false;
                allowAttacking = true;
                return false;
            }

        }

        private bool DoDps(out bool handlesMovement, out bool allowAttacking)
        {
            WowUnit wowUnit = WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(ShadronDisplayId, false);

            if (wowUnit != null)
            {
                WowGameobject portal = NearestPortal;

                if (portal != null)
                {
                    handlesMovement = true;
                    allowAttacking = false;

                    if (portal.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 3.0)
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

            handlesMovement = false;
            allowAttacking = true;
            return false;
        }
    }
}
