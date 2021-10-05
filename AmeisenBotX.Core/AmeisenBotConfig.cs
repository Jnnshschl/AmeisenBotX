using AmeisenBotX.Common.Keyboard.Objects;
using AmeisenBotX.Core.Engines.Movement.Settings;
using AmeisenBotX.Memory.Win32;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Core
{
    public class AmeisenBotConfig
    {
        public int AntiAfkMs { get; set; } = 1000;

        public bool AoeDetectionAvoid { get; set; } = false;

        public float AoeDetectionExtends { get; set; } = 1.0f;

        public bool AoeDetectionIncludePlayers { get; set; } = false;

        public bool AutoAcceptQuests { get; set; } = true;

        public bool AutoChangeRealmlist { get; set; } = true;

        public bool AutocloseWow { get; set; } = true;

        public bool AutoDisableRender { get; set; } = false;

        public bool AutojoinBg { get; set; } = true;

        public bool AutojoinLfg { get; set; } = true;

        public bool AutoLogin { get; set; } = true;

        public bool Autopilot { get; set; } = false;

        public bool AutoPositionWow { get; set; } = false;

        public bool AutoRepair { get; set; } = true;

        public bool AutoSell { get; set; } = true;

        public bool AutoSetUlowGfxSettings { get; set; } = true;

        public bool AutostartWow { get; set; } = true;

        public bool AutoTalkToNearQuestgivers { get; set; } = true;

        public int BagSlotsToGoSell { get; set; } = 4;

        public string BattlegroundEngine { get; set; } = string.Empty;

        public bool BattlegroundUsePartyMode { get; set; } = false;

        public Rect BotWindowRect { get; set; } = new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 };

        public string BuiltInCombatClassName { get; set; } = string.Empty;

        public bool CachePointsOfInterest { get; set; } = true;

        public int CharacterSlot { get; set; } = 0;

        public bool ChatProtocols { get; set; } = false;

        public List<string> CustomCombatClassDependencies { get; set; } = new();

        public string CustomCombatClassFile { get; set; } = string.Empty;

        public double DrinkUntilPercent { get; set; } = 75;

        public bool DungeonUsePartyMode { get; set; } = false;

        public double EatUntilPercent { get; set; } = 75;

        public int EventPullMs { get; set; } = 500;

        public bool FollowGroupLeader { get; set; } = false;

        public bool FollowGroupMembers { get; set; } = false;

        public bool FollowPositionDynamic { get; set; } = false;

        public bool FollowSpecificCharacter { get; set; } = false;

        public string Friends { get; set; } = string.Empty;

        public float GhostResurrectThreshold { get; set; } = 24.0f;

        public string GrindingProfile { get; set; } = string.Empty;

        public Dictionary<string, Keybind> Hotkeys { get; set; } = new();

        public bool IdleActions { get; set; } = false;

        public bool IgnoreCombatWhileMounted { get; set; } = true;

        public double ItemRepairThreshold { get; set; } = 25.0;

        public List<string> ItemSellBlacklist { get; set; }

        public string JobEngineMailHeader { get; set; } = string.Empty;

        public string JobEngineMailReceiver { get; set; } = string.Empty;

        public string JobEngineMailText { get; set; } = string.Empty;

        public string JobProfile { get; set; } = string.Empty;

        public bool LootOnlyMoneyAndQuestitems { get; set; } = false;

        public bool LootUnits { get; set; } = true;

        public float LootUnitsRadius { get; set; } = 20.0f;

        public bool MapRenderCurrentPath { get; set; } = true;

        public bool MapRenderDungeonNodes { get; set; } = false;

        public bool MapRenderHerbs { get; set; } = true;

        public bool MapRenderMe { get; set; } = true;

        public bool MapRenderOres { get; set; } = true;

        public bool MapRenderPlayerExtra { get; set; } = false;

        public bool MapRenderPlayerNames { get; set; } = true;

        public bool MapRenderPlayers { get; set; } = true;

        public bool MapRenderUnitExtra { get; set; } = false;

        public bool MapRenderUnitNames { get; set; } = false;

        public bool MapRenderUnits { get; set; } = true;

        public int MaxFollowDistance { get; set; } = 100;

        public int MaxFps { get; set; } = 60;

        public int MaxFpsCombat { get; set; } = 60;

        public float MerchantNpcSearchRadius { get; set; } = 50.0f;

        public int MinFollowDistance { get; set; } = 6;

        public string Mounts { get; set; } = string.Empty;

        public MovementSettings MovementSettings { get; set; } = new();

        public int NameshServerPort { get; set; } = 47110;

        public string NavmeshServerIp { get; set; } = "127.0.0.1";

        public bool OnlyFriendsMode { get; set; } = false;

        public bool OnlySupportMaster { get; set; } = false;

        public string Password { get; set; } = string.Empty;

        [JsonIgnore()]
        public string Path { get; set; } = string.Empty;

        public string PathToWowExe { get; set; } = string.Empty;

        public bool PermanentNameCache { get; set; } = true;

        public bool PermanentReactionCache { get; set; } = true;

        public string QuestProfile { get; set; } = string.Empty;

        public bool RconEnabled { get; set; } = false;

        public int RconInterval { get; set; } = 5000;

        public bool RconSendScreenshots { get; set; } = false;

        public string RconServerAddress { get; set; } = "https://localhost:47111";

        public string RconServerGuid { get; set; } = Guid.NewGuid().ToString();

        public string RconServerImage { get; set; } = string.Empty;

        public double RconTickMs { get; set; } = 1000;

        public string Realm { get; set; } = "AmeisenRealm";

        public string Realmlist { get; set; } = "127.0.0.1";

        public bool ReleaseSpirit { get; set; } = false;

        public float RepairNpcSearchRadius { get; set; } = 50.0f;

        public bool SaveBotWindowPosition { get; set; } = false;

        public bool SaveWowWindowPosition { get; set; } = false;

        public bool SellBlueItems { get; set; } = false;

        public bool SellGrayItems { get; set; } = true;

        public bool SellGreenItems { get; set; } = false;

        public bool SellPurpleItems { get; set; } = false;

        public bool SellWhiteItems { get; set; } = false;

        public string SpecificCharacterToFollow { get; set; } = string.Empty;

        public int StateMachineTickMs { get; set; } = 20;

        public bool StayCloseToGroupInCombat { get; set; } = false;

        public float SupportRange { get; set; } = 64.0f;

        public bool UseBuiltInCombatClass { get; set; } = true;

        public bool UseMounts { get; set; } = true;

        public bool UseMountsInParty { get; set; } = true;

        public bool UseOnlySpecificMounts { get; set; } = false;

        public string Username { get; set; } = string.Empty;

        public Rect WowWindowRect { get; set; } = new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 };
    }
}