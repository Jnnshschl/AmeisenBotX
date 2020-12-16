﻿using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Core.Battleground.einTyp;
using AmeisenBotX.Core.Battleground.Jannis;
using AmeisenBotX.Core.Battleground.KamelBG;
using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Chat;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.CombatLog;
using AmeisenBotX.Core.Data.Db;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Dungeon;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Grinding;
using AmeisenBotX.Core.Grinding.Profiles;
using AmeisenBotX.Core.Grinding.Profiles.Profiles.Alliance.Group;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Jobs;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Jobs.Profiles.Gathering;
using AmeisenBotX.Core.Jobs.Profiles.Gathering.Jannis;
using AmeisenBotX.Core.Movement.Pathfinding;
using AmeisenBotX.Core.Movement.SMovementEngine;
using AmeisenBotX.Core.Offsets;
using AmeisenBotX.Core.Personality;
using AmeisenBotX.Core.Quest;
using AmeisenBotX.Core.Quest.Profiles;
using AmeisenBotX.Core.Quest.Profiles.StartAreas;
using AmeisenBotX.Core.Statemachine;
using AmeisenBotX.Core.Statemachine.CombatClasses;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.States;
using AmeisenBotX.Core.Tactic;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Memory.Win32;
using AmeisenBotX.RconClient;
using AmeisenBotX.RconClient.Enums;
using AmeisenBotX.RconClient.Messages;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Timer = System.Threading.Timer;

namespace AmeisenBotX.Core
{
    public class AmeisenBot
    {
        private double currentExecutionMs;
        private int rconTimerBusy;
        private int stateMachineTimerBusy;

        public AmeisenBot(string botDataPath, string accountName, AmeisenBotConfig config, IntPtr mainWindowHandle)
        {
            Config = config;
            AccountName = accountName;
            BotDataPath = botDataPath;
            MainWindowHandle = mainWindowHandle;

            ExecutionMsStopwatch = new Stopwatch();

            if (!Directory.Exists(BotDataPath)) Directory.CreateDirectory(BotDataPath);

            SetupLogging(botDataPath, accountName);

            AmeisenLogger.I.Log("AmeisenBot", $"AmeisenBot ({Assembly.GetExecutingAssembly().GetName().Version}) starting", LogLevel.Master);
            AmeisenLogger.I.Log("AmeisenBot", $"AccountName: {accountName}", LogLevel.Master);
            AmeisenLogger.I.Log("AmeisenBot", $"BotDataPath: {botDataPath}", LogLevel.Verbose);

            // TODO: refactor this
            WowInterface = WowInterface.I;
            SetupWowInterface();

            StateMachine = new AmeisenBotStateMachine(BotDataPath, Config, WowInterface);
            StateMachine.GetState<StateStartWow>().OnWoWStarted += AmeisenBot_OnWoWStarted;

            RconScreenshotEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(Config.RconScreenshotInterval));

            AmeisenLogger.I.Log("AmeisenBot", $"Using OffsetList: {WowInterface.OffsetList.GetType()}", LogLevel.Master);

            if (Config.RconEnabled)
            {
                WowInterface.ObjectManager.OnObjectUpdateComplete += ObjectManager_OnObjectUpdateComplete;
            }

            InitCombatClasses();
            InitBattlegroundEngines();
            InitJobProfiles();
            InitQuestProfiles();
            InitGrindingProfiles();

            LoadProfiles();
        }

        public delegate void CombatClassCompilationStatus(bool succeeded, string heading, string message);

        public event CombatClassCompilationStatus OnCombatClassCompilationStatusChanged;

        public string AccountName { get; }

        public List<IBattlegroundEngine> BattlegroundEngines { get; private set; }

        public string BotDataPath { get; }

        public List<ICombatClass> CombatClasses { get; private set; }

        public AmeisenBotConfig Config { get; set; }

        public double CurrentExecutionMs
        {
            get
            {
                double avgTickTime = Math.Round(currentExecutionMs / CurrentExecutionCount, 2);
                CurrentExecutionCount = 0;
                return avgTickTime;
            }

            private set => currentExecutionMs = value;
        }

        public Stopwatch ExecutionMsStopwatch { get; private set; }

        public List<IGrindingProfile> GrindingProfiles { get; private set; }

        public bool IsRunning { get; private set; }

        public List<IJobProfile> JobProfiles { get; private set; }

        public IntPtr MainWindowHandle { get; private set; }

        public bool NeedToSetupRconClient { get; set; }

        public List<IQuestProfile> QuestProfiles { get; private set; }

        public TimegatedEvent RconScreenshotEvent { get; }

        public AmeisenBotStateMachine StateMachine { get; set; }

        public WowInterface WowInterface { get; set; }

        private TimegatedEvent BagUpdateEvent { get; set; }

        private int CurrentExecutionCount { get; set; }

        private TimegatedEvent EquipmentUpdateEvent { get; set; }

        private Timer RconClientTimer { get; set; }

        private Timer StateMachineTimer { get; set; }

        private bool TalentUpdateRunning { get; set; }

        public void Pause()
        {
            AmeisenLogger.I.Log("AmeisenBot", "Pausing", LogLevel.Debug);
            IsRunning = false;

            if (StateMachine.CurrentState.Key != BotState.StartWow
                && StateMachine.CurrentState.Key != BotState.Login
                && StateMachine.CurrentState.Key != BotState.LoadingScreen)
            {
                StateMachine.SetState(BotState.Idle);
            }
        }

        public void ReloadConfig()
        {
            StateMachineTimer = new Timer(StateMachineTimerTick, null, 0, (int)Config.StateMachineTickMs);
            LoadProfiles();
        }

        public void Resume()
        {
            AmeisenLogger.I.Log("AmeisenBot", "Resuming", LogLevel.Debug);
            IsRunning = true;
            stateMachineTimerBusy = 0;
        }

        public void Start()
        {
            AmeisenLogger.I.Log("AmeisenBot", "Starting", LogLevel.Debug);

            if (Config.RconEnabled)
            {
                AmeisenLogger.I.Log("Rcon", "Starting Rcon Timer", LogLevel.Debug);
                RconClientTimer = new Timer(RconClientTimerTick, null, 0, (int)Config.RconTickMs);
            }

            SubscribeToWowEvents();

            if (Config.SaveBotWindowPosition)
            {
                LoadWindowPosition(Config.BotWindowRect, MainWindowHandle);
            }

            StateMachineTimer = new Timer(StateMachineTimerTick, null, 0, (int)Config.StateMachineTickMs);
            stateMachineTimerBusy = 0;
            IsRunning = true;

            AmeisenLogger.I.Log("AmeisenBot", "Setup done", LogLevel.Debug);
        }

        public void Stop()
        {
            AmeisenLogger.I.Log("AmeisenBot", "Stopping", LogLevel.Debug);

            if (Config.SaveWowWindowPosition && !StateMachine.WowCrashed)
            {
                SaveWowWindowPosition();
            }

            if (Config.SaveBotWindowPosition)
            {
                SaveBotWindowPosition();
            }

            StateMachine.ShouldExit = true;
            StateMachineTimer.Dispose();

            if (Config.RconEnabled)
            {
                RconClientTimer.Dispose();
            }

            WowInterface.EventHookManager.Stop();

            if (Config.AutocloseWow)
            {
                WowInterface.HookManager.LuaDoString("ForceQuit()");
            }

            WowInterface.HookManager.DisposeHook();

            WowInterface.Db.Save(Path.Combine(BotDataPath, AccountName, "db.json"));

            AmeisenLogger.I.Log("AmeisenBot", $"Exiting AmeisenBot", LogLevel.Debug);
            AmeisenLogger.I.Stop();
        }

        private static T LoadClassByName<T>(List<T> profiles, string profileName)
        {
            AmeisenLogger.I.Log("AmeisenBot", $"Loading {nameof(T),-24} {profileName}", LogLevel.Verbose);
            return profiles.FirstOrDefault(e => e.ToString().Equals(profileName, StringComparison.OrdinalIgnoreCase));
        }

        private static void LoadWindowPosition(Rect rect, IntPtr windowHandle)
        {
            if (windowHandle != IntPtr.Zero && rect != new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 })
            {
                XMemory.SetWindowPosition(windowHandle, rect);
                AmeisenLogger.I.Log("AmeisenBot", $"Loaded window position: {rect}", LogLevel.Verbose);
            }
            else
            {
                AmeisenLogger.I.Log("AmeisenBot", $"Unable to load window position of {windowHandle} to {rect}", LogLevel.Warning);
            }
        }

        private static void SetupLogging(string botDataPath, string accountName)
        {
            AmeisenLogger.I.ChangeLogFolder(Path.Combine(botDataPath, accountName, "log/"));
            AmeisenLogger.I.ActiveLogLevel = LogLevel.Verbose;
            AmeisenLogger.I.Start();
        }

        private void AmeisenBot_OnWoWStarted()
        {
            if (Config.SaveWowWindowPosition)
            {
                LoadWowWindowPosition();
            }
        }

        private ICombatClass CompileCustomCombatClass()
        {
            CompilerParameters parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false
            };

            for (int i = 0; i < Config.CustomCombatClassDependencies.Count; ++i)
            {
                parameters.ReferencedAssemblies.Add(Config.CustomCombatClassDependencies[i]);
            }

            using CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, File.ReadAllText(Config.CustomCombatClassFile));

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();
                CompilerErrorCollection list = results.Errors;

                for (int i = 0; i < list.Count; ++i)
                {
                    CompilerError error = list[i];
                    sb.AppendLine($"Error {error.ErrorNumber} Line: {error.Line}: {error.ErrorText}");
                }

                throw new InvalidOperationException(sb.ToString());
            }

            return (ICombatClass)results.CompiledAssembly.CreateInstance(typeof(ICombatClass).ToString());
        }

        private void InitBattlegroundEngines()
        {
            // Add your custom battleground engines here!
            // ------------------------------------------ >
            BattlegroundEngines = new List<IBattlegroundEngine>
            {
                new UniversalBattlegroundEngine(WowInterface),
                new KummelEngine(WowInterface),
                new RunBoyRunEngine(WowInterface)
            };
        }

        private void InitCombatClasses()
        {
            // Add your custom combat classes here!
            // ------------------------------------ >
            CombatClasses = new List<ICombatClass>
            {
                new Statemachine.CombatClasses.Jannis.DeathknightBlood(StateMachine),
                new Statemachine.CombatClasses.Jannis.DeathknightFrost(StateMachine),
                new Statemachine.CombatClasses.Jannis.DeathknightUnholy(StateMachine),
                new Statemachine.CombatClasses.Jannis.DruidBalance(StateMachine),
                new Statemachine.CombatClasses.Jannis.DruidFeralBear(StateMachine),
                new Statemachine.CombatClasses.Jannis.DruidFeralCat(StateMachine),
                new Statemachine.CombatClasses.Jannis.DruidRestoration(StateMachine),
                new Statemachine.CombatClasses.Jannis.HunterBeastmastery(StateMachine),
                new Statemachine.CombatClasses.Jannis.HunterMarksmanship(StateMachine),
                new Statemachine.CombatClasses.Jannis.HunterSurvival(StateMachine),
                new Statemachine.CombatClasses.Jannis.MageArcane(StateMachine),
                new Statemachine.CombatClasses.Jannis.MageFire(StateMachine),
                new Statemachine.CombatClasses.Jannis.PaladinHoly(StateMachine),
                new Statemachine.CombatClasses.Jannis.PaladinProtection(StateMachine),
                new Statemachine.CombatClasses.Jannis.PaladinRetribution(StateMachine),
                new Statemachine.CombatClasses.Jannis.PriestDiscipline(StateMachine),
                new Statemachine.CombatClasses.Jannis.PriestHoly(StateMachine),
                new Statemachine.CombatClasses.Jannis.PriestShadow(StateMachine),
                new Statemachine.CombatClasses.Jannis.RogueAssassination(StateMachine),
                new Statemachine.CombatClasses.Jannis.ShamanElemental(StateMachine),
                new Statemachine.CombatClasses.Jannis.ShamanEnhancement(StateMachine),
                new Statemachine.CombatClasses.Jannis.ShamanRestoration(StateMachine),
                new Statemachine.CombatClasses.Jannis.WarlockAffliction(StateMachine),
                new Statemachine.CombatClasses.Jannis.WarlockDemonology(StateMachine),
                new Statemachine.CombatClasses.Jannis.WarlockDestruction(StateMachine),
                new Statemachine.CombatClasses.Jannis.WarriorArms(StateMachine),
                new Statemachine.CombatClasses.Jannis.WarriorFury(StateMachine),
                new Statemachine.CombatClasses.Jannis.WarriorProtection(StateMachine),
                new Statemachine.CombatClasses.Kamel.DeathknightBlood(WowInterface),
                new Statemachine.CombatClasses.Kamel.RestorationShaman (WowInterface),
                new Statemachine.CombatClasses.Kamel.PaladinProtection (WowInterface),
                new Statemachine.CombatClasses.Kamel.PriestHoly (WowInterface),
                new Statemachine.CombatClasses.Kamel.WarriorFury(WowInterface),
                new Statemachine.CombatClasses.Kamel.WarriorArms(WowInterface),
                new Statemachine.CombatClasses.einTyp.PaladinProtection(WowInterface),
                new Statemachine.CombatClasses.einTyp.RogueAssassination(WowInterface),
                new Statemachine.CombatClasses.einTyp.WarriorArms(WowInterface),
                new Statemachine.CombatClasses.einTyp.WarriorFury(WowInterface),
                new Statemachine.CombatClasses.ToadLump.Rogue(StateMachine),
            };
        }

        private void InitGrindingProfiles()
        {
            // Add your custom grinding profiles here!
            // --------------------------------------- >
            GrindingProfiles = new List<IGrindingProfile>
            {
                new UltimateGrinding1To80(),
            };
        }

        private void InitJobProfiles()
        {
            // Add your custom job profiles here!
            // ---------------------------------- >
            JobProfiles = new List<IJobProfile>
            {
                new CopperElwynnForestProfile(),
                new CopperTinSilverWestfallProfile(),
                new ElwynnRedridgeMining(),
            };
        }

        private void InitQuestProfiles()
        {
            // Add your custom quest profiles here!
            // ------------------------------------ >
            QuestProfiles = new List<IQuestProfile>
            {
                new DeathknightStartAreaQuestProfile(WowInterface)
            };
        }

        private void LoadCustomCombatClass()
        {
            AmeisenLogger.I.Log("AmeisenBot", $"Loading custom CombatClass: {Config.CustomCombatClassFile}", LogLevel.Verbose);
            if (Config.CustomCombatClassFile.Length == 0
                || !File.Exists(Config.CustomCombatClassFile))
            {
                AmeisenLogger.I.Log("AmeisenBot", "Loading default CombatClass", LogLevel.Warning);
                WowInterface.CombatClass = LoadClassByName(CombatClasses, Config.BuiltInCombatClassName);
            }
            else
            {
                try
                {
                    WowInterface.CombatClass = CompileCustomCombatClass();
                    OnCombatClassCompilationStatusChanged?.Invoke(true, string.Empty, string.Empty);
                    AmeisenLogger.I.Log("AmeisenBot", $"Compiling custom CombatClass successful", LogLevel.Warning);
                }
                catch (Exception e)
                {
                    AmeisenLogger.I.Log("AmeisenBot", $"Compiling custom CombatClass failed:\n{e}", LogLevel.Warning);
                    OnCombatClassCompilationStatusChanged?.Invoke(false, e.GetType().Name, e.ToString());
                    WowInterface.CombatClass = LoadClassByName(CombatClasses, Config.BuiltInCombatClassName);
                }
            }
        }

        private void LoadProfiles()
        {
            if (Config.UseBuiltInCombatClass)
            {
                WowInterface.CombatClass = LoadClassByName(CombatClasses, Config.BuiltInCombatClassName);
            }
            else
            {
                LoadCustomCombatClass();
            }

            // if a combatclass specified an ItemComparator
            // use it instead of the default one
            if (WowInterface.CombatClass?.ItemComparator != null)
            {
                WowInterface.CharacterManager.ItemComparator = WowInterface.CombatClass.ItemComparator;
            }

            WowInterface.BattlegroundEngine = LoadClassByName(BattlegroundEngines, Config.BattlegroundEngine);
            WowInterface.GrindingEngine.Profile = LoadClassByName(GrindingProfiles, Config.GrindingProfile);
            WowInterface.JobEngine.Profile = LoadClassByName(JobProfiles, Config.JobProfile);
            WowInterface.QuestEngine.Profile = LoadClassByName(QuestProfiles, Config.QuestProfile);
        }

        private void LoadWowWindowPosition()
        {
            if (Config.SaveWowWindowPosition && !Config.AutoPositionWow)
            {
                LoadWindowPosition(Config.WowWindowRect, WowInterface.XMemory.Process.MainWindowHandle);
            }
        }

        private void ObjectManager_OnObjectUpdateComplete(IEnumerable<WowObject> wowObjects)
        {
            if (!NeedToSetupRconClient && WowInterface.ObjectManager.Player != null)
            {
                NeedToSetupRconClient = true;
                WowInterface.RconClient = new AmeisenBotRconClient
                (
                    Config.RconServerAddress,
                    WowInterface.ObjectManager.Player.Name,
                    WowInterface.ObjectManager.Player.Race.ToString(),
                    WowInterface.ObjectManager.Player.Gender.ToString(),
                    WowInterface.ObjectManager.Player.Class.ToString(),
                    WowInterface.CombatClass != null ? WowInterface.CombatClass.Role.ToString() : "dps",
                    Config.RconServerImage,
                    Config.RconServerGuid
                );
            }
        }

        private void OnBagChanged(long timestamp, List<string> args)
        {
            if (BagUpdateEvent.Run())
            {
                WowInterface.CharacterManager.Inventory.Update();
                WowInterface.CharacterManager.Equipment.Update();

                WowInterface.CharacterManager.UpdateCharacterGear();

                WowInterface.CharacterManager.Inventory.Update();
            }
        }

        private void OnConfirmBindOnPickup(long timestamp, List<string> args)
        {
            WowInterface.HookManager.LuaCofirmStaticPopup();
        }

        private void OnConfirmLootRoll(long timestamp, List<string> args)
        {
            WowInterface.HookManager.LuaCofirmLootRoll();
        }

        private void OnEquipmentChanged(long timestamp, List<string> args)
        {
            if (EquipmentUpdateEvent.Run())
            {
                OnBagChanged(timestamp, args);
            }
        }

        private void OnLfgProposalShow(long timestamp, List<string> args)
        {
            if (Config.AutojoinLfg)
            {
                WowInterface.HookManager.LuaClickUiElement("LFDDungeonReadyDialogEnterDungeonButton");
            }
        }

        private void OnLfgRoleCheckShow(long timestamp, List<string> args)
        {
            if (Config.AutojoinLfg)
            {
                WowInterface.HookManager.LuaSetLfgRole(WowInterface.CombatClass != null ? WowInterface.CombatClass.Role : CombatClassRole.Dps);
            }
        }

        private void OnLootRollStarted(long timestamp, List<string> args)
        {
            if (int.TryParse(args[0], out int rollId))
            {
                string itemLink = WowInterface.HookManager.LuaGetLootRollItemLink(rollId);
                string itemJson = WowInterface.HookManager.LuaGetItemJsonByNameOrLink(itemLink);

                WowBasicItem item = ItemFactory.BuildSpecificItem(ItemFactory.ParseItem(itemJson));

                if (item.Name == "0" || item.ItemLink == "0")
                {
                    // get the item id and try again
                    itemJson = WowInterface.HookManager.LuaGetItemJsonByNameOrLink(
                        itemLink.Split(new string[] { "Hitem:" }, StringSplitOptions.RemoveEmptyEntries)[1]
                        .Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);

                    item = ItemFactory.BuildSpecificItem(ItemFactory.ParseItem(itemJson));
                }

                if (WowInterface.CharacterManager.IsItemAnImprovement(item, out IWowItem itemToReplace))
                {
                    AmeisenLogger.I.Log("WoWEvents", $"Would like to replace item {item?.Name} with {itemToReplace?.Name}, rolling need", LogLevel.Verbose);

                    WowInterface.HookManager.LuaRollOnLoot(rollId, RollType.Need);
                    return;
                }
            }

            WowInterface.HookManager.LuaRollOnLoot(rollId, RollType.Pass);
        }

        private void OnLootWindowOpened(long timestamp, List<string> args)
        {
            if (Config.LootOnlyMoneyAndQuestitems)
            {
                WowInterface.HookManager.LuaLootMoneyAndQuestItems();
                return;
            }

            WowInterface.HookManager.LuaLootEveryThing();
        }

        private void OnPartyInvitation(long timestamp, List<string> args)
        {
            if (Config.OnlyFriendsMode && (args.Count < 1 || !Config.Friends.Split(',').Any(e => e.Equals(args[0], StringComparison.OrdinalIgnoreCase))))
            {
                return;
            }

            WowInterface.HookManager.LuaAcceptPartyInvite();
        }

        private void OnPvpQueueShow(long timestamp, List<string> args)
        {
            if (Config.AutojoinBg)
            {
                if (args.Count == 1 && args[0] == "1")
                {
                    WowInterface.HookManager.LuaAcceptBattlegroundInvite();
                }
            }
        }

        private void OnQuestAcceptConfirm(long timestamp, List<string> args)
        {
            if (Config.AutoAcceptQuests && StateMachine.CurrentState.Key != BotState.Questing)
            {
                WowInterface.HookManager.LuaDoString("ConfirmAcceptQuest();");
            }
        }

        private void OnQuestGreeting(long timestamp, List<string> args)
        {
            if (Config.AutoAcceptQuests
                && StateMachine.CurrentState.Key != BotState.Selling
                && StateMachine.CurrentState.Key != BotState.Repairing)
            {
                WowInterface.HookManager.LuaAcceptQuests();
            }
        }

        private void OnQuestProgress(long timestamp, List<string> args)
        {
            if (Config.AutoAcceptQuests && StateMachine.CurrentState.Key != BotState.Questing)
            {
                WowInterface.HookManager.LuaClickUiElement("QuestFrameCompleteQuestButton");
            }
        }

        private void OnReadyCheck(long timestamp, List<string> args)
        {
            WowInterface.HookManager.LuaCofirmReadyCheck(true);
        }

        private void OnResurrectRequest(long timestamp, List<string> args)
        {
            WowInterface.HookManager.LuaAcceptResurrect();
        }

        private void OnShowQuestFrame(long timestamp, List<string> args)
        {
            if (Config.AutoAcceptQuests && StateMachine.CurrentState.Key != BotState.Questing)
            {
                WowInterface.HookManager.LuaDoString("AcceptQuest();");
            }
        }

        private void OnSummonRequest(long timestamp, List<string> args)
        {
            WowInterface.HookManager.LuaAcceptSummon();
        }

        private void OnTalentPointsChange(long timestamp, List<string> args)
        {
            if (WowInterface.CombatClass != null && WowInterface.CombatClass.Talents != null && !TalentUpdateRunning)
            {
                TalentUpdateRunning = true;
                WowInterface.CharacterManager.TalentManager.Update();
                WowInterface.CharacterManager.TalentManager.SelectTalents(WowInterface.CombatClass.Talents, WowInterface.HookManager.LuaGetUnspentTalentPoints());
                TalentUpdateRunning = false;
            }
        }

        private void OnTradeAcceptUpdate(long timestamp, List<string> args)
        {
            WowInterface.HookManager.LuaDoString("AcceptTrade();");
        }

        private void RconClientTimerTick(object state)
        {
            // only start one timer tick at a time
            if (Interlocked.CompareExchange(ref rconTimerBusy, 1, 0) == 1)
            {
                return;
            }

            try
            {
                if (WowInterface.RconClient != null)
                {
                    try
                    {
                        if (WowInterface.RconClient.NeedToRegister)
                        {
                            WowInterface.RconClient.Register();
                        }
                        else
                        {
                            int currentResource = WowInterface.ObjectManager.Player.Class switch
                            {
                                WowClass.Deathknight => WowInterface.ObjectManager.Player.Runeenergy,
                                WowClass.Rogue => WowInterface.ObjectManager.Player.Energy,
                                WowClass.Warrior => WowInterface.ObjectManager.Player.Rage,
                                _ => WowInterface.ObjectManager.Player.Mana,
                            };

                            int maxResource = WowInterface.ObjectManager.Player.Class switch
                            {
                                WowClass.Deathknight => WowInterface.ObjectManager.Player.MaxRuneenergy,
                                WowClass.Rogue => WowInterface.ObjectManager.Player.MaxEnergy,
                                WowClass.Warrior => WowInterface.ObjectManager.Player.MaxRage,
                                _ => WowInterface.ObjectManager.Player.MaxMana,
                            };

                            WowInterface.RconClient.SendData(new DataMessage()
                            {
                                BagSlotsFree = 0,
                                CombatClass = WowInterface.CombatClass != null ? WowInterface.CombatClass.Role.ToString() : "NoCombatClass",
                                CurrentProfile = "",
                                Energy = currentResource,
                                Exp = WowInterface.ObjectManager.Player.Xp,
                                Health = WowInterface.ObjectManager.Player.Health,
                                ItemLevel = (int)Math.Round(WowInterface.CharacterManager.Equipment.AverageItemLevel),
                                Level = WowInterface.ObjectManager.Player.Level,
                                MapName = WowInterface.ObjectManager.MapId.ToString(),
                                MaxEnergy = maxResource,
                                MaxExp = WowInterface.ObjectManager.Player.NextLevelXp,
                                MaxHealth = WowInterface.ObjectManager.Player.MaxHealth,
                                Money = WowInterface.CharacterManager.Money,
                                PosX = WowInterface.ObjectManager.Player.Position.X,
                                PosY = WowInterface.ObjectManager.Player.Position.Y,
                                PosZ = WowInterface.ObjectManager.Player.Position.Z,
                                State = StateMachine.CurrentState.Key.ToString(),
                                SubZoneName = WowInterface.ObjectManager.ZoneSubName,
                                ZoneName = WowInterface.ObjectManager.ZoneName,
                            });

                            if (Config.RconSendScreenshots && RconScreenshotEvent.Run())
                            {
                                Bitmap bmp = WowInterface.XMemory.GetScreenshot();

                                using MemoryStream ms = new MemoryStream();
                                bmp.Save(ms, ImageFormat.Png);

                                WowInterface.RconClient.SendImage($"data:image/png;base64,{Convert.ToBase64String(ms.GetBuffer())}");
                            }

                            WowInterface.RconClient.PullPendingActions();

                            if (WowInterface.RconClient.PendingActions.Any())
                            {
                                switch (WowInterface.RconClient.PendingActions.First())
                                {
                                    case ActionType.PauseResume:
                                        if (IsRunning) Pause(); else Resume();
                                        break;

                                    default: break;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            finally
            {
                rconTimerBusy = 0;
            }
        }

        private void SaveBotWindowPosition()
        {
            try
            {
                Config.BotWindowRect = XMemory.GetWindowPosition(MainWindowHandle);
            }
            catch (Exception e)
            {
                AmeisenLogger.I.Log("AmeisenBot", $"Failed to save bot window position:\n{e}", LogLevel.Error);
            }
        }

        private void SaveWowWindowPosition()
        {
            if (!Config.AutoPositionWow)
            {
                try
                {
                    Config.WowWindowRect = XMemory.GetWindowPosition(WowInterface.XMemory.Process.MainWindowHandle);
                }
                catch (Exception e)
                {
                    AmeisenLogger.I.Log("AmeisenBot", $"Failed to save wow window position:\n{e}", LogLevel.Error);
                }
            }
        }

        private void SetupWowInterface()
        {
            WowInterface.Globals = new AmeisenBotGlobals();

            WowInterface.OffsetList = new OffsetList335a();
            WowInterface.XMemory = new XMemory();

            WowInterface.Db = LocalAmeisenBotDb.FromJson(Path.Combine(BotDataPath, AccountName, "db.json"));
            WowInterface.Personality = new BotPersonality();

            WowInterface.ChatManager = new ChatManager(Config, Path.Combine(BotDataPath, AccountName));
            WowInterface.CombatLogParser = new CombatLogParser(WowInterface);

            WowInterface.ObjectManager = new ObjectManager(WowInterface, Config);
            WowInterface.HookManager = new HookManager(WowInterface);
            WowInterface.CharacterManager = new CharacterManager(Config, WowInterface);
            WowInterface.EventHookManager = new EventHook(WowInterface);

            WowInterface.JobEngine = new JobEngine(WowInterface, Config);
            WowInterface.DungeonEngine = new DungeonEngine(WowInterface);
            WowInterface.QuestEngine = new QuestEngine(WowInterface, Config, StateMachine);
            WowInterface.GrindingEngine = new GrindingEngine(WowInterface, Config, StateMachine);
            WowInterface.TacticEngine = new TacticEngine();

            WowInterface.PathfindingHandler = new NavmeshServerPathfindingHandler(Config.NavmeshServerIp, Config.NameshServerPort);
            WowInterface.MovementSettings = Config.MovementSettings;
            WowInterface.MovementEngine = new SickMovementEngine(WowInterface, Config);
        }

        private void StateMachineTimerTick(object state)
        {
            // only start one timer tick at a time
            if (Interlocked.CompareExchange(ref stateMachineTimerBusy, 1, 0) == 1
                || !IsRunning)
            {
                return;
            }

            try
            {
                ExecutionMsStopwatch.Restart();
                StateMachine.Execute();
                CurrentExecutionMs = ExecutionMsStopwatch.ElapsedMilliseconds;
                CurrentExecutionCount++;
            }
            finally
            {
                stateMachineTimerBusy = 0;
            }
        }

        private void SubscribeToWowEvents()
        {
            BagUpdateEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            EquipmentUpdateEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            // Request Events
            // -------------- >
            WowInterface.EventHookManager.Subscribe("PARTY_INVITE_REQUEST", OnPartyInvitation);
            WowInterface.EventHookManager.Subscribe("RESURRECT_REQUEST", OnResurrectRequest);
            WowInterface.EventHookManager.Subscribe("CONFIRM_SUMMON", OnSummonRequest);
            WowInterface.EventHookManager.Subscribe("READY_CHECK", OnReadyCheck);

            // Loot/Item Events
            // ---------------- >
            WowInterface.EventHookManager.Subscribe("LOOT_OPENED", OnLootWindowOpened);
            WowInterface.EventHookManager.Subscribe("LOOT_BIND_CONFIRM", OnConfirmBindOnPickup);
            WowInterface.EventHookManager.Subscribe("AUTOEQUIP_BIND_CONFIRM", OnConfirmBindOnPickup);
            WowInterface.EventHookManager.Subscribe("CONFIRM_LOOT_ROLL", OnConfirmLootRoll);
            WowInterface.EventHookManager.Subscribe("START_LOOT_ROLL", OnLootRollStarted);
            WowInterface.EventHookManager.Subscribe("BAG_UPDATE", OnBagChanged);
            WowInterface.EventHookManager.Subscribe("PLAYER_EQUIPMENT_CHANGED", OnEquipmentChanged);
            // WowInterface.EventHookManager.Subscribe("DELETE_ITEM_CONFIRM", OnConfirmDeleteItem);

            // Merchant Events
            // --------------- >
            // WowInterface.EventHookManager.Subscribe("MERCHANT_SHOW", OnMerchantShow);

            // PvP Events
            // ---------- >
            WowInterface.EventHookManager.Subscribe("UPDATE_BATTLEFIELD_STATUS", OnPvpQueueShow);
            WowInterface.EventHookManager.Subscribe("PVPQUEUE_ANYWHERE_SHOW", OnPvpQueueShow);

            // Dungeon Events
            // -------------- >
            WowInterface.EventHookManager.Subscribe("LFG_ROLE_CHECK_SHOW", OnLfgRoleCheckShow);
            WowInterface.EventHookManager.Subscribe("LFG_PROPOSAL_SHOW", OnLfgProposalShow);

            // Quest Events
            // ------------ >
            WowInterface.EventHookManager.Subscribe("QUEST_DETAIL", OnShowQuestFrame);
            WowInterface.EventHookManager.Subscribe("QUEST_ACCEPT_CONFIRM", OnQuestAcceptConfirm);
            WowInterface.EventHookManager.Subscribe("QUEST_GREETING", OnQuestGreeting);
            WowInterface.EventHookManager.Subscribe("QUEST_COMPLETE", OnQuestProgress);
            WowInterface.EventHookManager.Subscribe("QUEST_PROGRESS", OnQuestProgress);
            WowInterface.EventHookManager.Subscribe("GOSSIP_SHOW", OnQuestGreeting);

            // Trading Events
            // -------------- >
            WowInterface.EventHookManager.Subscribe("TRADE_ACCEPT_UPDATE", OnTradeAcceptUpdate);

            // Chat Events
            // ----------- >
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_ADDON", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.ADDON, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_CHANNEL", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.CHANNEL, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_EMOTE", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.EMOTE, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_FILTERED", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.FILTERED, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_GUILD", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.GUILD, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_GUILD_ACHIEVEMENT", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.GUILD_ACHIEVEMENT, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_IGNORED", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.IGNORED, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_MONSTER_EMOTE", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.MONSTER_EMOTE, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_MONSTER_PARTY", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.MONSTER_PARTY, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_MONSTER_SAY", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.MONSTER_SAY, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_MONSTER_WHISPER", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.MONSTER_WHISPER, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_MONSTER_YELL", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.MONSTER_YELL, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_OFFICER", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.OFFICER, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_PARTY", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.PARTY, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_PARTY_LEADER", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.PARTY_LEADER, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_RAID", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.RAID, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_RAID_BOSS_EMOTE", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.RAID_BOSS_EMOTE, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_RAID_BOSS_WHISPER", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.RAID_BOSS_WHISPER, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_RAID_LEADER", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.RAID_LEADER, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_RAID_WARNING", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.RAID_WARNING, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_SAY", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.SAY, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_SYSTEM", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.SYSTEM, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_TEXT_EMOTE", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.TEXT_EMOTE, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_WHISPER", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.WHISPER, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_YELL", (t, a) => WowInterface.ChatManager.TryParseMessage(ChatMessageType.YELL, t, a));

            // Misc Events
            // ----------- >
            WowInterface.EventHookManager.Subscribe("CHARACTER_POINTS_CHANGED", OnTalentPointsChange);
            // WowInterface.EventHookManager.Subscribe("COMBAT_LOG_EVENT", WowInterface.CombatLogParser.Parse);
        }
    }
}