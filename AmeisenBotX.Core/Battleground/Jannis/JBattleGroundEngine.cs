using AmeisenBotX.Core.Battleground.Jannis.Profiles;

namespace AmeisenBotX.Core.Battleground.Jannis
{
    public class JBattleGroundEngine : IBattlegroundEngine
    {
        public JBattleGroundEngine(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            Profile = new WarsongGulchProfile(WowInterface);
        }

        public IBattlegroundProfile Profile { get; set; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            WowInterface.CombatClass.OutOfCombatExecute();

            Profile?.Execute();
        }
    }
}