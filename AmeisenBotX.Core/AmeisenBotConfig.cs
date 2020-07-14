using AmeisenBotX.Memory.Win32;
using System;
using System.IO;

namespace AmeisenBotX.Core
{
    public class AmeisenBotConfig
    {
        public double AntiAfkMs { get; set; } = 1000;

        public bool AutoAcceptQuests { get; set; } = true;

        public bool AutoChangeRealmlist { get; set; } = true;

        public bool AutocloseWow { get; set; } = true;

        public bool AutoDisableRender { get; set; } = false;

        public bool AutoDodgeAoeSpells { get; set; } = false;

        public bool AutojoinBg { get; set; } = true;

        public bool AutoLogin { get; set; } = true;

        public bool Autopilot { get; set; } = false;

        public bool AutoPositionWow { get; set; } = false;

        public bool AutoRepair { get; set; } = true;

        public bool AutoSell { get; set; } = true;

        public bool AutostartWow { get; set; } = true;

        public bool AutoTalkToNearQuestgivers { get; set; } = true;

        public int BagSlotsToGoSell { get; set; } = 4;

        public string BattlegroundEngine { get; set; } = string.Empty;

        public bool BattlegroundUsePartyMode { get; set; } = false;

        public Rect BotWindowRect { get; set; } = new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 };

        public string BuiltInCombatClassName { get; set; } = string.Empty;

        public int CharacterSlot { get; set; } = 0;

        public string[] CustomCombatClassDependencies { get; set; } = { "System.dll", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AmeisenBotX.Core.dll") };

        public string CustomCombatClassFile { get; set; } = string.Empty;

        public double DrinkUntilPercent { get; set; } = 75;

        public bool DungeonUsePartyMode { get; set; } = false;

        public double EatUntilPercent { get; set; } = 75;

        public bool EnabledRconServer { get; set; } = false;

        public double EventPullMs { get; set; } = 1000;

        public bool FollowGroupLeader { get; set; } = false;

        public bool FollowGroupMembers { get; set; } = false;

        public bool FollowPositionDynamic { get; set; } = false;

        public bool FollowSpecificCharacter { get; set; } = false;

        public string Friends { get; set; } = string.Empty;

        public double GhostCheckMs { get; set; } = 5000;

        public double GhostPortalScanThreshold { get; set; } = 24;

        public double GhostPortalSearchMs { get; set; } = 1000;

        public double GhostResurrectThreshold { get; set; } = 24;

        public double ItemRepairThreshold { get; set; } = 25;

        public string[] ItemSellBlacklist { get; set; } = { "Hearthstone" };

        public string JobEngineMailHeader { get; set; } = string.Empty;

        public string JobEngineMailReceiver { get; set; } = string.Empty;

        public string JobEngineMailText { get; set; } = string.Empty;

        public string JobProfile { get; set; } = string.Empty;

        public bool LootUnits { get; set; } = true;

        public double LootUnitsRadius { get; set; } = 20.0;

        public int MaxFollowDistance { get; set; } = 100;

        public int MaxFps { get; set; } = 20;

        public int MaxFpsCombat { get; set; } = 30;

        public double MerchantNpcSearchRadius { get; set; } = 50;

        public int MinFollowDistance { get; set; } = 6;

        public int NameshServerPort { get; set; } = 47110;

        public string NavmeshServerIp { get; set; } = "127.0.0.1";

        public bool OnlyFriendsMode { get; set; } = false;

        public string Password { get; set; } = string.Empty;

        public string PathToWowExe { get; set; } = string.Empty;

        public bool PermanentNameCache { get; set; } = true;

        public bool PermanentReactionCache { get; set; } = true;

        public string RconServerAddress { get; set; } = "https://localhost:47111";

        public string RconServerGuid { get; set; } = Guid.NewGuid().ToString();

        public string RconServerImage { get; set; } = string.Empty;

        public double RconTickMs { get; set; } = 1000;

        public string Realm { get; set; } = "AmeisenRealm";

        public string Realmlist { get; set; } = "127.0.0.1";

        public bool ReleaseSpirit { get; set; } = false;

        public double RepairNpcSearchRadius { get; set; } = 50;

        public bool SaveBotWindowPosition { get; set; } = false;

        public bool SaveWowWindowPosition { get; set; } = false;

        public bool SellBlueItems { get; set; } = false;

        public bool SellGrayItems { get; set; } = true;

        public bool SellGreenItems { get; set; } = false;

        public bool SellPurpleItems { get; set; } = false;

        public bool SellWhiteItems { get; set; } = false;

        public string SpecificCharacterToFollow { get; set; } = string.Empty;

        public double StateMachineTickMs { get; set; } = 50;

        public bool UseBuiltInCombatClass { get; set; } = true;

        public string Username { get; set; } = string.Empty;

        public Rect WowWindowRect { get; set; } = new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 };
    }
}