using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;

namespace AmeisenBotX.Core.Common.Extensions
{
    public static class WowObjectExtensions
    {
        public static float DistanceTo(this WowObject a, WowObject b) => a.Position.GetDistance(b.Position);

        public static bool IsInRange(this WowObject a, WowObject b, float range) => a.DistanceTo(b) < range;

        public static void Interact(this WowInterface wowInterface, WowGameobject gameobject, float minRange = 3.0f)
        {
            if (wowInterface.ObjectManager.Player.IsInRange(gameobject, minRange))
            {
                wowInterface.HookManager.WowObjectRightClick(gameobject);
            }
            else
            {
                wowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, gameobject.Position);
            }
        }
    }
}