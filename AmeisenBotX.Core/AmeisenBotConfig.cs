using System;

namespace AmeisenBotX.Core
{
    public class AmeisenBotConfig
    {
        public AmeisenBotConfig()
        {
        }

        public bool AutoDodgeAoeSpells { get; set; } = false;
        public bool AutoLogin { get; set; } = false;
        public bool AutostartWow { get; set; } = false;
        public int CharacterSlot { get; set; } = 0;
        public string CombatClassName { get; set; } = "ClassSpec";
        public bool FollowGroupLeader { get; set; } = false;
        public bool FollowGroupMembers { get; set; } = false;
        public bool FollowSpecificCharacter { get; set; } = false;
        public int MaxFollowDistance { get; set; } = 100;
        public int MinFollowDistance { get; set; } = 6;
        public int NameshServerPort { get; set; } = 47110;
        public string NavmeshServerIp { get; set; } = "127.0.0.1";
        public double ObjectUpdateMs { get; set; } = 250;
        public string Password { get; set; } = "";
        public string PathToWowExe { get; set; } = "";
        public bool PermanentNameCache { get; set; } = false;
        public bool PermanentReactionCache { get; set; } = false;
        public bool ReleaseSpirit { get; set; } = false;
        public bool SaveBotWindowPosition { get; set; } = false;
        public bool SaveWowWindowPosition { get; set; } = false;
        public string SpecificCharacterToFollow { get; set; } = "";
        public double StateMachineTickMs { get; set; } = 50;
        public string Username { get; set; } = "";
    }
}