using AmeisenBotX.Memory.Win32;
using System;
using System.IO;

namespace AmeisenBotX.Core
{
    public class AmeisenBotConfig
    {
        public bool AutocloseWow { get; set; } = false;

        public bool AutoDodgeAoeSpells { get; set; } = false;

        public bool AutojoinBg { get; set; } = false;

        public bool AutoLogin { get; set; } = false;

        public bool Autopilot { get; set; } = false;

        public bool AutostartWow { get; set; } = false;

        public Rect BotWindowRect { get; set; } = new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 };

        public string BuiltInCombatClassName { get; set; } = "ClassSpec";

        public int CharacterSlot { get; set; } = 0;

        public string[] CustomCombatClassDependencies { get; set; } = { "System.dll", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AmeisenBotX.Core.dll") };

        public string CustomCombatClassFile { get; set; } = "";

        public bool FollowGroupLeader { get; set; } = false;

        public bool FollowGroupMembers { get; set; } = false;

        public bool FollowSpecificCharacter { get; set; } = false;

        public bool LootUnits { get; set; } = true;

        public double LootUnitsRadius { get; set; } = 20.0;

        public int MaxFollowDistance { get; set; } = 100;

        public int MaxFps { get; set; } = 20;

        public int MaxFpsCombat { get; set; } = 30;

        public int MinFollowDistance { get; set; } = 6;

        public int NameshServerPort { get; set; } = 47110;

        public string NavmeshServerIp { get; set; } = "127.0.0.1";

        public double ObjectUpdateMs { get; set; } = 0;

        public string Password { get; set; } = string.Empty;

        public string PathToWowExe { get; set; } = string.Empty;

        public bool PermanentNameCache { get; set; } = true;

        public bool PermanentReactionCache { get; set; } = true;

        public bool ReleaseSpirit { get; set; } = false;

        public bool SaveBotWindowPosition { get; set; } = false;

        public bool SaveWowWindowPosition { get; set; } = false;

        public string SpecificCharacterToFollow { get; set; } = string.Empty;

        public double StateMachineTickMs { get; set; } = 50;

        public bool UseBuiltInCombatClass { get; set; } = true;

        public bool UseClickToMove { get; set; } = true;

        public string Username { get; set; } = string.Empty;

        public Rect WowWindowRect { get; set; } = new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 };
    }
}