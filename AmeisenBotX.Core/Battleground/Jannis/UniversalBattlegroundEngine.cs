﻿using AmeisenBotX.Core.Battleground.Jannis.Profiles;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Battleground.Jannis
{
    public class UniversalBattlegroundEngine : IBattlegroundEngine
    {
        public UniversalBattlegroundEngine(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

        public string Author => "Jannis";

        public string Description => "Working battlegrounds:\n - Warsong Gulch";

        public string Name => "Universal Battleground Engine";

        public IBattlegroundProfile Profile { get; set; }

        private WowInterface WowInterface { get; }

        public void Enter()
        {
            TryLoadProfile();
        }

        public void Execute()
        {
            WowInterface.CombatClass.OutOfCombatExecute();

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
            switch (WowInterface.Objects.MapId)
            {
                case WowMapId.WarsongGulch:
                    Profile = new WarsongGulchProfile(WowInterface);
                    return true;

                default:
                    Profile = null;
                    return false;
            }
        }
    }
}