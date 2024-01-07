using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Movement.Providers.Basic
{
    public class SimpleCombatMovementProvider(AmeisenBotInterfaces bot) : IMovementProvider
    {
        private AmeisenBotInterfaces Bot { get; } = bot;

        public bool Get(out Vector3 position, out MovementAction type)
        {
            if (Bot.CombatClass != null
                && !Bot.CombatClass.HandlesMovement
                && IWowUnit.IsValidAliveInCombat(Bot.Player)
                && IWowUnit.IsValidAlive(Bot.Target)
                && !Bot.Player.IsGhost)
            {
                float distance = Bot.Player.DistanceTo(Bot.Target);

                switch (Bot.CombatClass.Role)
                {
                    case WowRole.Dps:
                        if (Bot.CombatClass.IsMelee)
                        {
                            if (distance > Bot.Player.MeleeRangeTo(Bot.Target))
                            {
                                position = Bot.Target.Position;
                                type = MovementAction.Chase;
                                return true;
                            }
                        }
                        else
                        {
                            if (distance > 26.5f + Bot.Target.CombatReach)
                            {
                                position = Bot.Target.Position;
                                type = MovementAction.Chase;
                                return true;
                            }
                        }
                        break;

                    case WowRole.Heal:
                        if (distance > 26.5f + Bot.Target.CombatReach)
                        {
                            position = Bot.Target.Position;
                            type = MovementAction.Chase;
                            return true;
                        }
                        break;

                    case WowRole.Tank:
                        if (distance > Bot.Player.MeleeRangeTo(Bot.Target))
                        {
                            position = Bot.Target.Position;
                            type = MovementAction.Chase;
                            return true;
                        }
                        break;
                }
            }

            type = MovementAction.None;
            position = Vector3.Zero;
            return false;
        }
    }
}