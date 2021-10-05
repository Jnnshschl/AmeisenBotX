using AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis
{
    public class UniversalBattlegroundEngine : IBattlegroundEngine
    {
        public UniversalBattlegroundEngine(AmeisenBotInterfaces bot)
        {
            Bot = bot;
        }

        public string Author => "Jannis";

        public string Description => "Working battlegrounds:\n - Warsong Gulch";

        public string Name => "Universal Battleground Engine";

        public IBattlegroundProfile Profile { get; set; }

        private AmeisenBotInterfaces Bot { get; }

        public void Execute()
        {
            if (Profile == null)
            {
                TryLoadProfile();
            }

            Bot.CombatClass?.OutOfCombatExecute();
            Profile?.Execute();
        }

        public void Reset()
        {
            Profile = null;
        }

        public override string ToString()
        {
            return $"{Name} ({Author})";
        }

        private bool TryLoadProfile()
        {
            switch (Bot.Objects.MapId)
            {
                case WowMapId.WarsongGulch:
                    Profile = new WarsongGulchProfile(Bot);
                    return true;

                default:
                    Profile = null;
                    return false;
            }
        }
    }
}