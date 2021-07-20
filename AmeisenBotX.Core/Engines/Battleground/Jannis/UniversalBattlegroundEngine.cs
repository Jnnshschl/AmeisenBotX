using AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis
{
    public class UniversalBattlegroundEngine : IBattlegroundEngine
    {
        public UniversalBattlegroundEngine(AmeisenBotInterfaces bot, AmeisenBotFsm stateMachine)
        {
            Bot = bot;
            StateMachine = stateMachine;
        }

        private AmeisenBotFsm StateMachine { get; }

        public string Author => "Jannis";

        public string Description => "Working battlegrounds:\n - Warsong Gulch";

        public string Name => "Universal Battleground Engine";

        public IBattlegroundProfile Profile { get; set; }

        private AmeisenBotInterfaces Bot { get; }

        public void Enter()
        {
            TryLoadProfile();
        }

        public void Execute()
        {
            Bot.CombatClass.OutOfCombatExecute();

            Profile?.Execute();
        }

        public void Leave()
        {
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
                    Profile = new WarsongGulchProfile(Bot, StateMachine);
                    return true;

                default:
                    Profile = null;
                    return false;
            }
        }
    }
}