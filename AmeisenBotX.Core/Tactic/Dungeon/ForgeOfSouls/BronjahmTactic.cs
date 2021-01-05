using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Tactic.Dungeon.ForgeOfSouls
{
    public class BronjahmTactic : ITactic
    {
        public BronjahmTactic()
        {
            Configureables = new Dictionary<string, dynamic>()
            {
                { "isOffTank", false },
            };
        }

        public static Vector3 MidPosition { get; } = new Vector3(5297, 2506, 686);

        public Dictionary<string, dynamic> Configureables { get; private set; }

        private static List<int> BronjahmDisplayId { get; } = new List<int> { 30226 };

        public bool ExecuteTactic(CombatClassRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            preventMovement = false;
            allowAttacking = true;

            WowUnit wowUnit = WowInterface.I.ObjectManager.GetClosestWowUnitByDisplayId(BronjahmDisplayId, false);

            if (wowUnit != null)
            {
                if (wowUnit.CurrentlyCastingSpellId == 68872 || wowUnit.CurrentlyChannelingSpellId == 68872 || wowUnit.HasBuffById(68872)) // soulstorm
                {
                    if (WowInterface.I.ObjectManager.Player.Position.GetDistance(MidPosition) > 8.0)
                    {
                        WowInterface.I.MovementEngine.SetMovementAction(MovementAction.Move, BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(WowInterface.I.ObjectManager.Player.Position, MidPosition), -5.0f));

                        preventMovement = true;
                        allowAttacking = true;
                        return true;
                    }

                    // stay at the mid
                    return false;
                }

                if (role == CombatClassRole.Tank)
                {
                    if (wowUnit.TargetGuid == WowInterface.I.ObjectManager.PlayerGuid)
                    {
                        Vector3 modifiedCenterPosition = BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(WowInterface.I.ObjectManager.MeanGroupPosition, MidPosition), 8.0f);
                        float distanceToMid = WowInterface.I.ObjectManager.Player.Position.GetDistance(modifiedCenterPosition);

                        // flee from the corrupted souls target
                        bool needToFlee = wowUnit.CurrentlyChannelingSpellId == 68839
                            || WowInterface.I.ObjectManager.WowObjects.OfType<WowUnit>().Any(e => e.DisplayId == 30233 && e.IsInCombat);

                        if (needToFlee)
                        {
                            if (distanceToMid < 16.0f)
                            {
                                WowInterface.I.MovementEngine.SetMovementAction(MovementAction.Flee, modifiedCenterPosition);

                                preventMovement = true;
                                allowAttacking = false;
                                return true;
                            }

                            // we cant run away further
                            preventMovement = true;
                            return false;
                        }

                        if (distanceToMid > 5.0f && WowInterface.I.ObjectManager.Player.Position.GetDistance(wowUnit.Position) < 3.5)
                        {
                            // move the boss to mid
                            WowInterface.I.MovementEngine.SetMovementAction(MovementAction.Move, modifiedCenterPosition);

                            preventMovement = true;
                            allowAttacking = false;
                            return true;
                        }
                    }
                }
                else if (role == CombatClassRole.Dps || role == CombatClassRole.Heal)
                {
                    float distanceToMid = WowInterface.I.ObjectManager.Player.Position.GetDistance(MidPosition);

                    if (!isMelee && distanceToMid < 20.0f)
                    {
                        // move to the outer ring of the arena
                        WowInterface.I.MovementEngine.SetMovementAction(MovementAction.Move, BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(WowInterface.I.ObjectManager.Player.Position, MidPosition), -22.0f));

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