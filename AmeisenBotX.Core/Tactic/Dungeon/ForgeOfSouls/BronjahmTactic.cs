using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Tactic.Dungeon.ForgeOfSouls
{
    public class BronjahmTactic : ITactic
    {
        public BronjahmTactic(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            Configureables = new()
            {
                { "isOffTank", false },
            };
        }

        public static Vector3 MidPosition { get; } = new Vector3(5297, 2506, 686);

        public Dictionary<string, dynamic> Configureables { get; private set; }

        public WowInterface WowInterface { get; }

        private static List<int> BronjahmDisplayId { get; } = new List<int> { 30226 };

        public bool ExecuteTactic(WowRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            preventMovement = false;
            allowAttacking = true;

            WowUnit wowUnit = WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(BronjahmDisplayId, false);

            if (wowUnit != null)
            {
                if (wowUnit.CurrentlyCastingSpellId == 68872 || wowUnit.CurrentlyChannelingSpellId == 68872 || wowUnit.HasBuffById(68872)) // soulstorm
                {
                    if (WowInterface.ObjectManager.Player.Position.GetDistance(MidPosition) > 8.0)
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(WowInterface.ObjectManager.Player.Position, MidPosition), -5.0f));

                        preventMovement = true;
                        allowAttacking = true;
                        return true;
                    }

                    // stay at the mid
                    return false;
                }

                if (role == WowRole.Tank)
                {
                    if (wowUnit.TargetGuid == WowInterface.ObjectManager.PlayerGuid)
                    {
                        Vector3 modifiedCenterPosition = BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(WowInterface.ObjectManager.MeanGroupPosition, MidPosition), 8.0f);
                        float distanceToMid = WowInterface.ObjectManager.Player.Position.GetDistance(modifiedCenterPosition);

                        // flee from the corrupted souls target
                        bool needToFlee = wowUnit.CurrentlyChannelingSpellId == 68839
                            || WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Any(e => e.DisplayId == 30233 && e.IsInCombat);

                        if (needToFlee)
                        {
                            if (distanceToMid < 16.0f)
                            {
                                WowInterface.MovementEngine.SetMovementAction(MovementAction.Flee, modifiedCenterPosition);

                                preventMovement = true;
                                allowAttacking = false;
                                return true;
                            }

                            // we cant run away further
                            preventMovement = true;
                            return false;
                        }

                        if (distanceToMid > 5.0f && WowInterface.ObjectManager.Player.Position.GetDistance(wowUnit.Position) < 3.5)
                        {
                            // move the boss to mid
                            WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, modifiedCenterPosition);

                            preventMovement = true;
                            allowAttacking = false;
                            return true;
                        }
                    }
                }
                else if (role == WowRole.Dps || role == WowRole.Heal)
                {
                    float distanceToMid = WowInterface.ObjectManager.Player.Position.GetDistance(MidPosition);

                    if (!isMelee && distanceToMid < 20.0f)
                    {
                        // move to the outer ring of the arena
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(WowInterface.ObjectManager.Player.Position, MidPosition), -22.0f));

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