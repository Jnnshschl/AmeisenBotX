using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Tactic.Dungeon.PitOfSaron
{
    public class IckAndKrickTactic : ITactic
    {
        public DateTime ChasingActivated { get; private set; }

        private static List<int> IckDisplayId { get; } = new List<int> { 30347 };

        private bool ChasingActive => (ChasingActivated + TimeSpan.FromSeconds(14)) > DateTime.UtcNow;

        public static Vector3 MidPosition { get; } = new Vector3(823, 110, 509);

        public bool ExecuteTactic(CombatClassRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            preventMovement = false;
            allowAttacking = true;

            WowUnit wowUnit = WowInterface.I.ObjectManager.GetClosestWowUnitByDisplayId(IckDisplayId, false);

            if (wowUnit != null)
            {
                if (wowUnit.CurrentlyCastingSpellId == 68987 || wowUnit.CurrentlyChannelingSpellId == 68987) // chasing
                {
                    ChasingActivated = DateTime.UtcNow;
                    return true;
                }
                else if (ChasingActive && wowUnit.TargetGuid == WowInterface.I.ObjectManager.PlayerGuid && wowUnit.Position.GetDistance(WowInterface.I.ObjectManager.Player.Position) < 7.0f)
                {
                    WowInterface.I.MovementEngine.SetMovementAction(MovementAction.Fleeing, wowUnit.Position);

                    preventMovement = true;
                    allowAttacking = false;
                    return true;
                }

                WowUnit unitOrb = WowInterface.I.ObjectManager.WowObjects.OfType<WowUnit>()
                    .OrderBy(e=>e.Position.GetDistance(WowInterface.I.ObjectManager.Player.Position))
                    .FirstOrDefault(e => e.DisplayId == 11686 && e.HasBuffById(69017) && e.Position.GetDistance(WowInterface.I.ObjectManager.Player.Position) < 3.0f);

                if (unitOrb != null) // orbs
                {
                    WowInterface.I.MovementEngine.SetMovementAction(MovementAction.Fleeing, unitOrb.Position);

                    preventMovement = true;
                    allowAttacking = false;
                    return true;
                }

                if (role == CombatClassRole.Tank)
                {
                    if (wowUnit.TargetGuid == WowInterface.I.ObjectManager.PlayerGuid)
                    {
                        Vector3 modifiedCenterPosition = BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(BotUtils.GetMeanGroupPosition(), MidPosition), 8.0f);
                        float distanceToMid = WowInterface.I.ObjectManager.Player.Position.GetDistance(modifiedCenterPosition);

                        if (distanceToMid > 5.0f && WowInterface.I.ObjectManager.Player.Position.GetDistance(wowUnit.Position) < 3.5)
                        {
                            // move the boss to mid
                            WowInterface.I.MovementEngine.SetMovementAction(MovementAction.Moving, modifiedCenterPosition);

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
