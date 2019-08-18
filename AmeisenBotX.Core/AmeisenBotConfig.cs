namespace AmeisenBotX.Core
{
    public class AmeisenBotConfig
    {
        public bool AutostartWow { get; set; } = false;
        public string PathToWowExe { get; set; } = "";

        public bool AutoLogin { get; set; } = false;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public int CharacterSlot { get; set; } = 0;

        public double StateMachineTickMs { get; set; } = 50;
        public double ObjectUpdateMs { get; set; } = 250;

        public bool FollowGroupLeader { get; set; } = false;
        public bool FollowGroupMembers { get; set; } = false;
        public bool FollowSpecificCharacter { get; set; } = false;
        public string SpecificCharacterToFollow { get; set; } = "";

        public int MinFollowDistance { get; set; } = 6;
        public int MaxFollowDistance { get; set; } = 100;

        public string NavmeshServerIp { get; set; } = "127.0.0.1";
        public int NameshServerPort { get; set; } = 47110;

        public AmeisenBotConfig()
        {
        }
    }
}