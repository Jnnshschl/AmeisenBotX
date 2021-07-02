using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Engines.Grinding.Profiles;

namespace AmeisenBotX.Core.Engines.Grinding
{
    public interface IGrindingEngine
    {
        GrindingSpot GrindingSpot { get; }

        IGrindingProfile Profile { get; set; }

        Vector3 TargetPosition { get; }

        void Enter();

        void Execute();

        void Exit();

        void LoadProfile(IGrindingProfile questProfile);
    }
}