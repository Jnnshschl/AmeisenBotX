using AmeisenBotX.Core;

namespace AmeisenBotX.StateConfig
{
    public interface IStateConfigWindow
    {
        AmeisenBotConfig Config { get; }

        bool ShouldSave { get; }
    }
}