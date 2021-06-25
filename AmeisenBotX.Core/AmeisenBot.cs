using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Core.Battleground.einTyp;
using AmeisenBotX.Core.Battleground.Jannis;
using AmeisenBotX.Core.Battleground.KamelBG;
using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Combat.Classes;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Db;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Dungeon;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Core.Fsm.States;
using AmeisenBotX.Core.Grinding.Profiles;
using AmeisenBotX.Core.Grinding.Profiles.Profiles.Alliance.Group;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Jobs.Profiles.Gathering;
using AmeisenBotX.Core.Jobs.Profiles.Gathering.Jannis;
using AmeisenBotX.Core.Movement.AMovementEngine;
using AmeisenBotX.Core.Movement.Pathfinding;
using AmeisenBotX.Core.Offsets;
using AmeisenBotX.Core.Quest.Profiles;
using AmeisenBotX.Core.Quest.Profiles.Shino;
using AmeisenBotX.Core.Quest.Profiles.StartAreas;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Memory.Win32;
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
using System.Threading.Tasks;
using Timer = System.Threading.Timer;

namespace AmeisenBotX.Core
{
    public class AmeisenBot
    {
        private float currentExecutionMs;
        private int rconTimerBusy;
        private int stateMachineTimerBusy;

        /// <summary>
        /// Initializes a new bot.
        ///
        /// Call Start(), Pause(), Resume(), Dispose() to control the bots engine.
        /// </summary>
        /// <param name="config">The bot configuration.</param>
        /// <param name="logfilePath">Logfile path.</param>
        /// <param name="initialLogLevel">The initial LogLevel of the bots logger.</param>
        public AmeisenBot(AmeisenBotConfig config, string logfilePath = "", LogLevel initialLogLevel = LogLevel.Verbose)
        {
            Config = config ?? throw new ArgumentException("config cannot be null", nameof(config));
            if (string.IsNullOrWhiteSpace(config.Path)) { throw new ArgumentException("config.Path cannot be empty, make sure you set it after loading", nameof(config)); }

            DataFolder = Path.GetDirectoryName(config.Path);
            AccountName = Path.GetFileName(DataFolder);

            if (string.IsNullOrWhiteSpace(logfilePath)) { logfilePath = Path.Combine(DataFolder, "log/"); }

            AmeisenLogger.I.ChangeLogFolder(logfilePath);
            AmeisenLogger.I.ActiveLogLevel = initialLogLevel;
            AmeisenLogger.I.Start();

            AmeisenLogger.I.Log("AmeisenBot", $"AmeisenBot ({Assembly.GetExecutingAssembly().GetName().Version}) starting", LogLevel.Master);
            AmeisenLogger.I.Log("AmeisenBot", $"AccountName: {AccountName}", LogLevel.Master);
            AmeisenLogger.I.Log("AmeisenBot", $"BotDataPath: {DataFolder}", LogLevel.Verbose);

            ExecutionMsStopwatch = new();

            WowInterface = new();

            WowInterface.OffsetList = new OffsetList335a();
            AmeisenLogger.I.Log("AmeisenBot", $"Using OffsetList: {WowInterface.OffsetList.GetType()}", LogLevel.Master);

            WowInterface.Globals = new();
            WowInterface.XMemory = new();

            string dbPath = Path.Combine(DataFolder, "db.json");
            AmeisenLogger.I.Log("AmeisenBot", $"Loading DB from: {dbPath}", LogLevel.Master);
            WowInterface.Db = LocalAmeisenBotDb.FromJson(WowInterface, dbPath);

            WowInterface.Personality = new();
            WowInterface.ChatManager = new(Config, DataFolder);
            WowInterface.CombatLogParser = new(WowInterface);
            WowInterface.HookManager = new HookManager(WowInterface, Config);
            WowInterface.ObjectManager = new ObjectManager(WowInterface, Config);
            WowInterface.CharacterManager = new CharacterManager(WowInterface);
            WowInterface.EventHookManager = new(WowInterface);
            WowInterface.JobEngine = new(WowInterface, Config);
            WowInterface.DungeonEngine = new DungeonEngine(WowInterface);
            WowInterface.TacticEngine = new();
            WowInterface.PathfindingHandler = new NavmeshServerPathfindingHandler(Config.NavmeshServerIp, Config.NameshServerPort);
            WowInterface.MovementSettings = Config.MovementSettings;
            WowInterface.MovementEngine = new AMovementEngine(WowInterface, Config);

            StateMachine = new(DataFolder, Config, WowInterface);
            StateMachine.GetState<StateStartWow>().OnWoWStarted += () =>
            {
                if (Config.SaveWowWindowPosition)
                {
                    LoadWowWindowPosition();
                }
            };

            WowInterface.QuestEngine = new(WowInterface, Config, StateMachine);
            WowInterface.GrindingEngine = new(WowInterface, Config, StateMachine);

            AmeisenLogger.I.Log("AmeisenBot", "Finished setting up WowInterface", LogLevel.Verbose);

            if (Config.RconEnabled)
            {
                AmeisenLogger.I.Log("AmeisenBot", "Setting up RconClient", LogLevel.Verbose);
                RconScreenshotEvent = new(TimeSpan.FromMilliseconds(Config.RconScreenshotInterval));
                SetupRconClient();
            }

            AmeisenLogger.I.Log("AmeisenBot", "Setting CombatClasses", LogLevel.Verbose);
            InitCombatClasses();

            AmeisenLogger.I.Log("AmeisenBot", "Setting BattlegroundEngines", LogLevel.Verbose);
            InitBattlegroundEngines();

            AmeisenLogger.I.Log("AmeisenBot", "Setting JobProfiles", LogLevel.Verbose);
            InitJobProfiles();

            AmeisenLogger.I.Log("AmeisenBot", "Setting QuestProfiles", LogLevel.Verbose);
            InitQuestProfiles();

            AmeisenLogger.I.Log("AmeisenBot", "Setting GrindingProfiles", LogLevel.Verbose);
            InitGrindingProfiles();

            AmeisenLogger.I.Log("AmeisenBot", "Loading Profiles", LogLevel.Verbose);
            LoadProfiles();
        }

        public event Action<bool, string, string> OnCombatClassCompilationStatusChanged;

        public string AccountName { get; }

        public IEnumerable<IBattlegroundEngine> BattlegroundEngines { get; private set; }

        public IEnumerable<ICombatClass> CombatClasses { get; private set; }

        public AmeisenBotConfig Config { get; set; }

        public float CurrentExecutionMs
        {
            get
            {
                float avgTickTime = MathF.Round(currentExecutionMs / (float)CurrentExecutionCount, 2);
                CurrentExecutionCount = 0;
                return avgTickTime;
            }

            private set => currentExecutionMs = value;
        }

        public string DataFolder { get; }

        public Stopwatch ExecutionMsStopwatch { get; private set; }

        public IEnumerable<IGrindingProfile> GrindingProfiles { get; private set; }

        public bool IsRunning { get; private set; }

        public IEnumerable<IJobProfile> JobProfiles { get; private set; }

        public bool NeedToSetupRconClient { get; set; }

        public IEnumerable<IQuestProfile> QuestProfiles { get; private set; }

        public TimegatedEvent RconScreenshotEvent { get; }

        public AmeisenBotFsm StateMachine { get; set; }

        public WowInterface WowInterface { get; set; }

        private TimegatedEvent BagUpdateEvent { get; set; }

        private int CurrentExecutionCount { get; set; }

        private TimegatedEvent EquipmentUpdateEvent { get; set; }

        private Timer RconClientTimer { get; set; }

        private Timer StateMachineTimer { get; set; }

        private bool TalentUpdateRunning { get; set; }

        /// <summary>
        /// Use this method to destroy the bots instance
        /// </summary>
        public void Dispose()
        {
            AmeisenLogger.I.Log("AmeisenBot", "Stopping", LogLevel.Debug);

            if (Config.SaveWowWindowPosition && !StateMachine.WowCrashed)
            {
                SaveWowWindowPosition();
            }

            StateMachine.ShouldExit = true;
            StateMachineTimer.Dispose();

            if (Config.RconEnabled)
            {
                RconClientTimer.Dispose();
            }

            WowInterface.EventHookManager.Stop();

            if (Config.AutocloseWow || Config.AutoPositionWow)
            {
                WowInterface.HookManager.LuaDoString("ForceQuit()");

                // wait 5 sec for wow to exit, otherwise we kill it
                TimeSpan timeToWait = TimeSpan.FromSeconds(5);
                DateTime exited = DateTime.UtcNow;

                while (!WowInterface.WowProcess.HasExited)
                {
                    if (DateTime.UtcNow - exited > timeToWait)
                    {
                        WowInterface.WowProcess.Kill();
                        break;
                    }
                    else
                    {
                        Task.Delay(50).Wait();
                    }
                }
            }

            WowInterface.HookManager.Unhook();
            WowInterface.XMemory.Dispose();

            WowInterface.Db.Save(Path.Combine(DataFolder, "db.json"));

            AmeisenLogger.I.Log("AmeisenBot", $"Exiting AmeisenBot", LogLevel.Debug);
            AmeisenLogger.I.Stop();
        }

        /// <summary>
        /// Pauses the bots engine, nothing will be executed, call Resume() to resume the engine.
        /// </summary>
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

        /// <summary>
        /// Reloads the bots config
        /// </summary>
        /// <param name="newConfig">New config to load</param>
        public void ReloadConfig(AmeisenBotConfig newConfig)
        {
            Config = newConfig;
            StateMachineTimer = new(StateMachineTimerTick, null, 0, (int)Config.StateMachineTickMs);
            LoadProfiles();
        }

        /// <summary>
        /// Resumes the bots engine, call Pause() to pause the engine.
        /// </summary>
        public void Resume()
        {
            AmeisenLogger.I.Log("AmeisenBot", "Resuming", LogLevel.Debug);
            IsRunning = true;
            stateMachineTimerBusy = 0;
        }

        /// <summary>
        /// Starts the bots engine, only call this once, use Pause() and 
        /// Resume() to control the execution of the engine afterwards
        /// </summary>
        public void Start()
        {
            AmeisenLogger.I.Log("AmeisenBot", "Starting", LogLevel.Debug);

            if (Config.RconEnabled)
            {
                AmeisenLogger.I.Log("Rcon", "Starting Rcon Timer", LogLevel.Debug);
                RconClientTimer = new(RconClientTimerTick, null, 0, (int)Config.RconTickMs);
            }

            SubscribeToWowEvents();

            StateMachineTimer = new(StateMachineTimerTick, null, 0, (int)Config.StateMachineTickMs);
            stateMachineTimerBusy = 0;
            IsRunning = true;

            AmeisenLogger.I.Log("AmeisenBot", "Setup done", LogLevel.Debug);
        }

        private static T LoadClassByName<T>(IEnumerable<T> profiles, string profileName)
        {
            AmeisenLogger.I.Log("AmeisenBot", $"Loading {typeof(T).Name,-24} {profileName}", LogLevel.Verbose);
            return profiles.FirstOrDefault(e => e.ToString().Equals(profileName, StringComparison.OrdinalIgnoreCase));
        }

        private ICombatClass CompileCustomCombatClass()
        {
            CompilerParameters parameters = new()
            {
                GenerateInMemory = true,
                GenerateExecutable = false
            };

            for (int i = 0; i < Config.CustomCombatClassDependencies.Count; ++i)
            {
                parameters.ReferencedAssemblies.Add(Config.CustomCombatClassDependencies[i]);
            }

            using CSharpCodeProvider codeProvider = new();
            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, File.ReadAllText(Config.CustomCombatClassFile));

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new();
                CompilerErrorCollection list = results.Errors;

                for (int i = 0; i < list.Count; ++i)
                {
                    CompilerError error = list[i];
                    sb.AppendLine($"Error {error.ErrorNumber} Line: {error.Line}: {error.ErrorText}");
                }

                throw new(sb.ToString());
            }

            return (ICombatClass)results.CompiledAssembly.CreateInstance(typeof(ICombatClass).ToString());
        }

        private void InitBattlegroundEngines()
        {
            // add battleground engines here
            BattlegroundEngines = new List<IBattlegroundEngine>()
            {
                new UniversalBattlegroundEngine(WowInterface),
                new ArathiBasin(WowInterface),
                new StrandOfTheAncients(WowInterface),
                new EyeOfTheStorm(WowInterface),
                new RunBoyRunEngine(WowInterface)
            };
        }

        private void InitCombatClasses()
        {
            // add combat classes here
            CombatClasses = new List<ICombatClass>()
            {
                new Combat.Classes.Jannis.DeathknightBlood(WowInterface, StateMachine),
                new Combat.Classes.Jannis.DeathknightFrost(WowInterface, StateMachine),
                new Combat.Classes.Jannis.DeathknightUnholy(WowInterface, StateMachine),
                new Combat.Classes.Jannis.DruidBalance(WowInterface, StateMachine),
                new Combat.Classes.Jannis.DruidFeralBear(WowInterface, StateMachine),
                new Combat.Classes.Jannis.DruidFeralCat(WowInterface, StateMachine),
                new Combat.Classes.Jannis.DruidRestoration(WowInterface, StateMachine),
                new Combat.Classes.Jannis.HunterBeastmastery(WowInterface, StateMachine),
                new Combat.Classes.Jannis.HunterMarksmanship(WowInterface, StateMachine),
                new Combat.Classes.Jannis.HunterSurvival(WowInterface, StateMachine),
                new Combat.Classes.Jannis.MageArcane(WowInterface, StateMachine),
                new Combat.Classes.Jannis.MageFire(WowInterface, StateMachine),
                new Combat.Classes.Jannis.PaladinHoly(WowInterface, StateMachine),
                new Combat.Classes.Jannis.PaladinProtection(WowInterface, StateMachine),
                new Combat.Classes.Jannis.PaladinRetribution(WowInterface, StateMachine),
                new Combat.Classes.Jannis.PriestDiscipline(WowInterface, StateMachine),
                new Combat.Classes.Jannis.PriestHoly(WowInterface, StateMachine),
                new Combat.Classes.Jannis.PriestShadow(WowInterface, StateMachine),
                new Combat.Classes.Jannis.RogueAssassination(WowInterface, StateMachine),
                new Combat.Classes.Jannis.ShamanElemental(WowInterface, StateMachine),
                new Combat.Classes.Jannis.ShamanEnhancement(WowInterface, StateMachine),
                new Combat.Classes.Jannis.ShamanRestoration(WowInterface, StateMachine),
                new Combat.Classes.Jannis.WarlockAffliction(WowInterface, StateMachine),
                new Combat.Classes.Jannis.WarlockDemonology(WowInterface, StateMachine),
                new Combat.Classes.Jannis.WarlockDestruction(WowInterface, StateMachine),
                new Combat.Classes.Jannis.WarriorArms(WowInterface, StateMachine),
                new Combat.Classes.Jannis.WarriorFury(WowInterface, StateMachine),
                new Combat.Classes.Jannis.WarriorProtection(WowInterface, StateMachine),
                new Combat.Classes.Kamel.DeathknightBlood(WowInterface),
                new Combat.Classes.Kamel.RestorationShaman(WowInterface),
                new Combat.Classes.Kamel.ShamanEnhancement(WowInterface),
                new Combat.Classes.Kamel.PaladinProtection(WowInterface),
                new Combat.Classes.Kamel.PriestHoly(WowInterface),
                new Combat.Classes.Kamel.WarriorFury(WowInterface),
                new Combat.Classes.Kamel.WarriorArms(WowInterface),
                new Combat.Classes.Kamel.RogueAssassination(WowInterface),
                new Combat.Classes.einTyp.PaladinProtection(WowInterface),
                new Combat.Classes.einTyp.RogueAssassination(WowInterface),
                new Combat.Classes.einTyp.WarriorArms(WowInterface),
                new Combat.Classes.einTyp.WarriorFury(WowInterface),
                new Combat.Classes.ToadLump.Rogue(WowInterface, StateMachine),
                new Combat.Classes.Shino.PriestShadow(WowInterface, StateMachine),
                new Combat.Classes.Shino.MageFrost(WowInterface, StateMachine),
            };
        }

        private void InitGrindingProfiles()
        {
            // add grinding profiles here
            GrindingProfiles = new List<IGrindingProfile>()
            {
                new UltimateGrinding1To80(),
            };
        }

        private void InitJobProfiles()
        {
            // add job profiles here
            JobProfiles = new List<IJobProfile>()
            {
                new CopperElwynnForestProfile(),
                new CopperTinSilverWestfallProfile(),
                new ElwynnRedridgeMining(),
            };
        }

        private void InitQuestProfiles()
        {
            // add quest profiles here
            QuestProfiles = new List<IQuestProfile>()
            {
                new DeathknightStartAreaQuestProfile(WowInterface),
                new X5Horde1To80Profile(WowInterface),
                new Horde1To60GrinderProfile(WowInterface)
            };
        }

        private void LoadCustomCombatClass()
        {
            AmeisenLogger.I.Log("AmeisenBot", $"Loading custom CombatClass: {Config.CustomCombatClassFile}", LogLevel.Debug);

            if (Config.CustomCombatClassFile.Length == 0
                || !File.Exists(Config.CustomCombatClassFile))
            {
                AmeisenLogger.I.Log("AmeisenBot", "Loading default CombatClass", LogLevel.Debug);
                WowInterface.CombatClass = LoadClassByName(CombatClasses, Config.BuiltInCombatClassName);
            }
            else
            {
                try
                {
                    WowInterface.CombatClass = CompileCustomCombatClass();
                    OnCombatClassCompilationStatusChanged?.Invoke(true, string.Empty, string.Empty);
                    AmeisenLogger.I.Log("AmeisenBot", $"Compiling custom CombatClass successful", LogLevel.Debug);
                }
                catch (Exception e)
                {
                    AmeisenLogger.I.Log("AmeisenBot", $"Compiling custom CombatClass failed:\n{e}", LogLevel.Error);
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
                if (WowInterface.XMemory.Process.MainWindowHandle != IntPtr.Zero && Config.WowWindowRect != new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 })
                {
                    XMemory.SetWindowPosition(WowInterface.XMemory.Process.MainWindowHandle, Config.WowWindowRect);
                    AmeisenLogger.I.Log("AmeisenBot", $"Loaded window position: {Config.WowWindowRect}", LogLevel.Verbose);
                }
                else
                {
                    AmeisenLogger.I.Log("AmeisenBot", $"Unable to load window position of {WowInterface.XMemory.Process.MainWindowHandle} to {Config.WowWindowRect}", LogLevel.Warning);
                }
            }
        }

        private void OnBagChanged(long timestamp, List<string> args)
        {
            if (BagUpdateEvent.Run())
            {
                WowInterface.CharacterManager.Inventory.Update();
                WowInterface.CharacterManager.Equipment.Update();

                WowInterface.CharacterManager.UpdateCharacterGear();
                WowInterface.CharacterManager.UpdateCharacterBags();

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
                WowInterface.HookManager.LuaSetLfgRole(WowInterface.CombatClass != null ? WowInterface.CombatClass.Role : WowRole.Dps);
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

                    WowInterface.HookManager.LuaRollOnLoot(rollId, WowRollType.Need);
                    return;
                }
            }

            WowInterface.HookManager.LuaRollOnLoot(rollId, WowRollType.Pass);
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
                            int currentResource = WowInterface.Player.Class switch
                            {
                                WowClass.Deathknight => WowInterface.Player.Runeenergy,
                                WowClass.Rogue => WowInterface.Player.Energy,
                                WowClass.Warrior => WowInterface.Player.Rage,
                                _ => WowInterface.Player.Mana,
                            };

                            int maxResource = WowInterface.Player.Class switch
                            {
                                WowClass.Deathknight => WowInterface.Player.MaxRuneenergy,
                                WowClass.Rogue => WowInterface.Player.MaxEnergy,
                                WowClass.Warrior => WowInterface.Player.MaxRage,
                                _ => WowInterface.Player.MaxMana,
                            };

                            WowInterface.RconClient.SendData(new DataMessage()
                            {
                                BagSlotsFree = 0,
                                CombatClass = WowInterface.CombatClass != null ? WowInterface.CombatClass.Role.ToString() : "NoCombatClass",
                                CurrentProfile = "",
                                Energy = currentResource,
                                Exp = WowInterface.Player.Xp,
                                Health = WowInterface.Player.Health,
                                ItemLevel = (int)Math.Round(WowInterface.CharacterManager.Equipment.AverageItemLevel),
                                Level = WowInterface.Player.Level,
                                MapName = WowInterface.ObjectManager.MapId.ToString(),
                                MaxEnergy = maxResource,
                                MaxExp = WowInterface.Player.NextLevelXp,
                                MaxHealth = WowInterface.Player.MaxHealth,
                                Money = WowInterface.CharacterManager.Money,
                                PosX = WowInterface.Player.Position.X,
                                PosY = WowInterface.Player.Position.Y,
                                PosZ = WowInterface.Player.Position.Z,
                                State = StateMachine.CurrentState.Key.ToString(),
                                SubZoneName = WowInterface.ObjectManager.ZoneSubName,
                                ZoneName = WowInterface.ObjectManager.ZoneName,
                            });

                            if (Config.RconSendScreenshots && RconScreenshotEvent.Run())
                            {
                                Rect rc = new();
                                Win32Imports.GetWindowRect(WowInterface.XMemory.Process.MainWindowHandle, ref rc);
                                Bitmap bmp = new(rc.Right - rc.Left, rc.Bottom - rc.Top, PixelFormat.Format32bppArgb);

                                using (Graphics g = Graphics.FromImage(bmp))
                                    g.CopyFromScreen(rc.Left, rc.Top, 0, 0, new(rc.Right - rc.Left, rc.Bottom - rc.Top));

                                using MemoryStream ms = new();
                                bmp.Save(ms, ImageFormat.Png);

                                WowInterface.RconClient.SendImage($"data:image/png;base64,{Convert.ToBase64String(ms.GetBuffer())}");
                            }

                            WowInterface.RconClient.PullPendingActions();

                            if (WowInterface.RconClient.PendingActions.Any())
                            {
                                switch (WowInterface.RconClient.PendingActions.First())
                                {
                                    case ActionType.PauseResume:
                                        if (IsRunning)
                                        {
                                            Pause();
                                        }
                                        else
                                        {
                                            Resume();
                                        }

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

        private void SetupRconClient()
        {
            WowInterface.ObjectManager.OnObjectUpdateComplete += delegate
            {
                if (!NeedToSetupRconClient && WowInterface.Player != null)
                {
                    NeedToSetupRconClient = true;
                    WowInterface.RconClient = new
                    (
                        Config.RconServerAddress,
                        WowInterface.Player.Name,
                        WowInterface.Player.Race.ToString(),
                        WowInterface.Player.Gender.ToString(),
                        WowInterface.Player.Class.ToString(),
                        WowInterface.CombatClass != null ? WowInterface.CombatClass.Role.ToString() : "dps",
                        Config.RconServerImage,
                        Config.RconServerGuid
                    );
                }
            };
        }

        private void StateMachineTimerTick(object state)
        {
            // only start one timer tick at a time
            if (Interlocked.CompareExchange(ref stateMachineTimerBusy, 1, 0) == 1 || !IsRunning)
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
            BagUpdateEvent = new(TimeSpan.FromSeconds(1));
            EquipmentUpdateEvent = new(TimeSpan.FromSeconds(1));

            // Request Events
            WowInterface.EventHookManager.Subscribe("PARTY_INVITE_REQUEST", OnPartyInvitation);
            WowInterface.EventHookManager.Subscribe("RESURRECT_REQUEST", OnResurrectRequest);
            WowInterface.EventHookManager.Subscribe("CONFIRM_SUMMON", OnSummonRequest);
            WowInterface.EventHookManager.Subscribe("READY_CHECK", OnReadyCheck);

            // Loot/Item Events
            WowInterface.EventHookManager.Subscribe("LOOT_OPENED", OnLootWindowOpened);
            WowInterface.EventHookManager.Subscribe("LOOT_BIND_CONFIRM", OnConfirmBindOnPickup);
            WowInterface.EventHookManager.Subscribe("AUTOEQUIP_BIND_CONFIRM", OnConfirmBindOnPickup);
            WowInterface.EventHookManager.Subscribe("CONFIRM_LOOT_ROLL", OnConfirmLootRoll);
            WowInterface.EventHookManager.Subscribe("START_LOOT_ROLL", OnLootRollStarted);
            WowInterface.EventHookManager.Subscribe("BAG_UPDATE", OnBagChanged);
            WowInterface.EventHookManager.Subscribe("PLAYER_EQUIPMENT_CHANGED", OnEquipmentChanged);
            // WowInterface.EventHookManager.Subscribe("DELETE_ITEM_CONFIRM", OnConfirmDeleteItem);

            // Merchant Events
            // WowInterface.EventHookManager.Subscribe("MERCHANT_SHOW", OnMerchantShow);

            // PvP Events
            WowInterface.EventHookManager.Subscribe("UPDATE_BATTLEFIELD_STATUS", OnPvpQueueShow);
            WowInterface.EventHookManager.Subscribe("PVPQUEUE_ANYWHERE_SHOW", OnPvpQueueShow);

            // Dungeon Events
            WowInterface.EventHookManager.Subscribe("LFG_ROLE_CHECK_SHOW", OnLfgRoleCheckShow);
            WowInterface.EventHookManager.Subscribe("LFG_PROPOSAL_SHOW", OnLfgProposalShow);

            // Quest Events
            WowInterface.EventHookManager.Subscribe("QUEST_DETAIL", OnShowQuestFrame);
            WowInterface.EventHookManager.Subscribe("QUEST_ACCEPT_CONFIRM", OnQuestAcceptConfirm);
            WowInterface.EventHookManager.Subscribe("QUEST_GREETING", OnQuestGreeting);
            WowInterface.EventHookManager.Subscribe("QUEST_COMPLETE", OnQuestProgress);
            WowInterface.EventHookManager.Subscribe("QUEST_PROGRESS", OnQuestProgress);
            WowInterface.EventHookManager.Subscribe("GOSSIP_SHOW", OnQuestGreeting);

            // Trading Events
            WowInterface.EventHookManager.Subscribe("TRADE_ACCEPT_UPDATE", OnTradeAcceptUpdate);

            // Chat Events
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_ADDON", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.ADDON, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_CHANNEL", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.CHANNEL, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_EMOTE", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.EMOTE, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_FILTERED", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.FILTERED, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_GUILD", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.GUILD, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_GUILD_ACHIEVEMENT", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.GUILD_ACHIEVEMENT, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_IGNORED", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.IGNORED, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_MONSTER_EMOTE", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.MONSTER_EMOTE, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_MONSTER_PARTY", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.MONSTER_PARTY, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_MONSTER_SAY", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.MONSTER_SAY, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_MONSTER_WHISPER", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.MONSTER_WHISPER, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_MONSTER_YELL", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.MONSTER_YELL, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_OFFICER", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.OFFICER, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_PARTY", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.PARTY, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_PARTY_LEADER", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.PARTY_LEADER, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_RAID", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.RAID, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_RAID_BOSS_EMOTE", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.RAID_BOSS_EMOTE, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_RAID_BOSS_WHISPER", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.RAID_BOSS_WHISPER, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_RAID_LEADER", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.RAID_LEADER, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_RAID_WARNING", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.RAID_WARNING, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_SAY", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.SAY, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_SYSTEM", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.SYSTEM, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_TEXT_EMOTE", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.TEXT_EMOTE, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_WHISPER", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.WHISPER, t, a));
            WowInterface.EventHookManager.Subscribe("CHAT_MSG_YELL", (t, a) => WowInterface.ChatManager.TryParseMessage(WowChat.YELL, t, a));

            // Misc Events
            WowInterface.EventHookManager.Subscribe("CHARACTER_POINTS_CHANGED", OnTalentPointsChange);
            // WowInterface.EventHookManager.Subscribe("COMBAT_LOG_EVENT_UNFILTERED", WowInterface.CombatLogParser.Parse);
        }
    }
}