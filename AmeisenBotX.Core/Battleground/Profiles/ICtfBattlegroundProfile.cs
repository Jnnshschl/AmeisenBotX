using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Pathfinding.Objects;

namespace AmeisenBotX.Core.Battleground.Profiles
{
    public interface ICtfBattlegroundProfile : IBattlegroundProfile
    {
        Vector3 EnemyBasePosition { get; }

        WowPlayer EnemyFlagCarrierPlayer { get; }

        bool IsMeFlagCarrier { get; }

        Vector3 OwnBasePosition { get; }

        WowPlayer OwnFlagCarrierPlayer { get; }
    }
}