using AmeisenBotX.Core.Data.Enums;
using System.Numerics;

namespace AmeisenBotX.Core.Movement.SMovementEngine.Extra.Shortcuts
{
    public interface IShortcut
    {
        bool Finished { get; }

        MapId MapToUseOn { get; }

        double MinDistanceUntilWorth { get; }

        bool IsUseable(Vector3 position, Vector3 targetPosition);

        bool UseShortcut(Vector3 position, Vector3 targetPosition, out Vector3 positionToGoTo, out bool usePathfinding);
    }
}