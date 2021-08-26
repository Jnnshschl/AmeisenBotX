using AmeisenBotX.Core.Engines.Grinding.Profiles;

namespace AmeisenBotX.Core.Engines.Grinding
{
    public interface IGrindingEngine
    {
        IGrindingProfile Profile { get; set; }

        void Execute();

        void LoadProfile(IGrindingProfile profile);
    }
}