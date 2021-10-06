using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Storage;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Battleground;
using AmeisenBotX.Core.Engines.Battleground.einTyp;
using AmeisenBotX.Core.Engines.Battleground.Jannis;
using AmeisenBotX.Core.Engines.Battleground.KamelBG;
using AmeisenBotX.Core.Engines.Combat.Classes;
using AmeisenBotX.Core.Engines.Dungeon;
using AmeisenBotX.Core.Engines.Grinding;
using AmeisenBotX.Core.Engines.Grinding.Profiles;
using AmeisenBotX.Core.Engines.Grinding.Profiles.Alliance.Group;
using AmeisenBotX.Core.Engines.Grinding.Profiles.Horde;
using AmeisenBotX.Core.Engines.Jobs;
using AmeisenBotX.Core.Engines.Jobs.Profiles;
using AmeisenBotX.Core.Engines.Jobs.Profiles.Gathering;
using AmeisenBotX.Core.Engines.Jobs.Profiles.Gathering.Jannis;
using AmeisenBotX.Core.Engines.Movement;
using AmeisenBotX.Core.Engines.Movement.Pathfinding;
using AmeisenBotX.Core.Engines.PvP;
using AmeisenBotX.Core.Engines.Quest;
using AmeisenBotX.Core.Engines.Quest.Profiles;
using AmeisenBotX.Core.Engines.Quest.Profiles.Shino;
using AmeisenBotX.Core.Engines.Quest.Profiles.StartAreas;
using AmeisenBotX.Core.Engines.Tactic;
using AmeisenBotX.Core.Engines.Test;
using AmeisenBotX.Core.Logic;
using AmeisenBotX.Core.Logic.Idle.Actions;
using AmeisenBotX.Core.Logic.Routines;
using AmeisenBotX.Core.Managers.Character;
using AmeisenBotX.Core.Managers.Character.Inventory;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Core.Managers.Chat;
using AmeisenBotX.Core.Managers.Threat;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory;
using AmeisenBotX.Memory.Win32;
using AmeisenBotX.RconClient.Enums;
using AmeisenBotX.RconClient.Messages;
using AmeisenBotX.Wow.Cache;
using AmeisenBotX.Wow.Cache.Enums;
using AmeisenBotX.Wow.Combatlog;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a;
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
using System.Threading.Tasks;

namespace AmeisenBotX.Core
{
    public class AmeisenBot
    {
        private float currentExecutionMs;

        /// <summary>
        /// Initializes a new bot.
        ///
        /// Call Start(), Pause(), Resume(), Dispose() to control the bots engine.
        ///
        /// More stuff of the bot can be reached via its "Bot" property.
        /// </summary>
        /// <param name="config">The bot configuration.</param>
        /// <param name="logfilePath">Logfile folder path, not a file path. Leave DEFAULT to put it into bots profile folder. Set to string.Empty to disable logging.</param>
        /// <param name="initialLogLevel">The initial LogLevel of the bots logger.</param>
        public AmeisenBot(AmeisenBotConfig config, string logfilePath = "DEFAULT", LogLevel initialLogLevel = LogLevel.Verbose)
        {
            Config = config ?? throw new ArgumentException("config cannot be null", nameof(config));
            if (string.IsNullOrWhiteSpace(config.Path)) { throw new ArgumentException("config.Path cannot be empty, make sure you set it after loading the config", nameof(config)); }
            if (!File.Exists(config.Path)) { throw new ArgumentException("config.Path does not exist", nameof(config)); }

            ProfileFolder = Path.GetDirectoryName(config.Path);
            AccountName = Path.GetFileName(ProfileFolder);

            if (logfilePath == "DEFAULT")
            {
                logfilePath = Path.Combine(ProfileFolder, "log/");
            }

            if (!string.IsNullOrWhiteSpace(logfilePath) && Directory.Exists(logfilePath))
            {
                AmeisenLogger.I.ChangeLogFolder(logfilePath);
                AmeisenLogger.I.ActiveLogLevel = initialLogLevel;
                AmeisenLogger.I.Start();
            }

            AmeisenLogger.I.Log("AmeisenBot", $"AmeisenBot ({Assembly.GetExecutingAssembly().GetName().Version}) starting", LogLevel.Master);
            AmeisenLogger.I.Log("AmeisenBot", $"AccountName: {AccountName}", LogLevel.Master);
            AmeisenLogger.I.Log("AmeisenBot", $"BotDataPath: {ProfileFolder}", LogLevel.Verbose);

            ExecutionMsStopwatch = new();

            string dataFolder = Path.Combine(ProfileFolder, "data");

            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            // start initializing the wow interface
            Bot = new();
            Bot.Memory = new XMemory();
            Bot.Storage = new(dataFolder, new List<string>() { "AmeisenBotX.Core.Engines.Combat.Classes." });

            Bot.IdleActions = new(Config, new List<IIdleAction>()
            {
                new AuctionHouseIdleAction(Bot),
                new CheckMailsIdleAction(Bot),
                new FishingIdleAction(Bot),
                new LookAroundIdleAction(Bot),
                new LookAtGroupIdleAction(Bot),
                new RandomEmoteIdleAction(Bot),
                new SitByCampfireIdleAction(Bot),
                new SitToChairIdleAction(Bot, Config.MinFollowDistance),
            });

            Logic = new AmeisenBotLogic(Config, Bot);

            Bot.Chat = new DefaultChatManager(Config, ProfileFolder);
            Bot.Tactic = new DefaultTacticEngine(Bot);

            // load the wow specific interface based on file version (build number)
            Bot.Wow = FileVersionInfo.GetVersionInfo(config.PathToWowExe).FilePrivatePart switch
            {
                12340 => new WowInterface335a(Bot.Memory),
                _ => throw new ArgumentException("Unsupported wow version", nameof(config)),
            };

            Bot.Wow.OnStaticPopup += OnStaticPopup;
            Bot.Wow.OnBattlegroundStatus += OnBattlegroundStatusChanged;

            AmeisenLogger.I.Log("AmeisenBot", $"Using OffsetList: {Bot.Wow.Offsets.GetType().Name}", LogLevel.Master);

            Bot.Character = new DefaultCharacterManager(Bot.Wow, Bot.Memory);

            string dbPath = Path.Combine(ProfileFolder, "db.json");
            AmeisenLogger.I.Log("AmeisenBot", $"Loading DB from: {dbPath}", LogLevel.Master);
            Bot.Db = LocalAmeisenBotDb.FromJson(dbPath, Bot.Wow, Bot.Memory);

            PoiCacheEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
            Bot.Objects.OnObjectUpdateComplete += OnObjectUpdateComplete;

            Bot.CombatLog = new DefaultCombatlogParser();

            // setup all instances that use the whole Bot class last
            Bot.Dungeon = new DefaultDungeonEngine(Bot, Config);
            Bot.Jobs = new DefaultJobEngine(Bot, Config);
            Bot.Quest = new DefaultQuestEngine(Bot);
            Bot.Grinding = new DefaultGrindEngine(Bot, Config);
            Bot.Pvp = new DefaultPvpEngine(Bot, Config);
            Bot.Threat = new ThreatManager(Bot, Config);
            Bot.Test = new DefaultTestEngine(Bot, Config);

            Bot.PathfindingHandler = new AmeisenNavigationHandler(Config.NavmeshServerIp, Config.NameshServerPort);
            Bot.Movement = new DefaultMovementEngine(Bot, Config);
            // wow interface setup done

            AmeisenLogger.I.Log("AmeisenBot", "Finished setting up Bot", LogLevel.Verbose);

            AmeisenLogger.I.Log("AmeisenBot", "Loading CombatClasses", LogLevel.Verbose);
            InitCombatClasses();

            AmeisenLogger.I.Log("AmeisenBot", "Loading BattlegroundEngines", LogLevel.Verbose);
            InitBattlegroundEngines();

            AmeisenLogger.I.Log("AmeisenBot", "Loading JobProfiles", LogLevel.Verbose);
            InitJobProfiles();

            AmeisenLogger.I.Log("AmeisenBot", "Loading QuestProfiles", LogLevel.Verbose);
            InitQuestProfiles();

            AmeisenLogger.I.Log("AmeisenBot", "Loading GrindingProfiles", LogLevel.Verbose);
            InitGrindingProfiles();

            AmeisenLogger.I.Log("AmeisenBot", "Loading Profiles", LogLevel.Verbose);
            LoadProfiles();

            Bot.Storage.LoadAll();

            if (Config.RconEnabled)
            {
                AmeisenLogger.I.Log("AmeisenBot", "Setting up RconClient", LogLevel.Verbose);
                RconEvent = new(TimeSpan.FromMilliseconds(Config.RconInterval));
                SetupRconClient();
            }
        }

        /// <summary>
        /// Fires when a custom BombatClass was compiled.
        /// </summary>
        public event Action<bool, string, string> OnCombatClassCompilationResult;

        /// <summary>
        /// Fires the bot changes from running to pause or vice versa.
        /// </summary>
        public event Action OnStatusChanged;

        /// <summary>
        /// Current account name used.
        /// </summary>
        public string AccountName { get; }

        /// <summary>
        /// All currently loaded battleground engines.
        /// </summary>
        public IEnumerable<IBattlegroundEngine> BattlegroundEngines { get; private set; }

        /// <summary>
        /// Collection of all useful interfaces used to control the bots behavior.
        /// </summary>
        public AmeisenBotInterfaces Bot { get; private set; }

        /// <summary>
        /// Folder where  all profile relevant stuff is stored.
        /// </summary>
        public string BotFolder { get; }

        /// <summary>
        /// All currently loaded combat classes.
        /// </summary>
        public IEnumerable<ICombatClass> CombatClasses { get; private set; }

        /// <summary>
        /// Current configuration.
        /// </summary>
        public AmeisenBotConfig Config { get; private set; }

        /// <summary>
        /// How long the bot needed to execute one tick.
        /// </summary>
        public float CurrentExecutionMs
        {
            get
            {
                float avgTickTime = MathF.Round(currentExecutionMs / CurrentExecutionCount, 2);
                CurrentExecutionCount = 0;
                return avgTickTime;
            }

            private set => currentExecutionMs = value;
        }

        /// <summary>
        /// All currently loaded grinding profiles.
        /// </summary>
        public IEnumerable<IGrindingProfile> GrindingProfiles { get; private set; }

        /// <summary>
        /// Whether the bot is running or paused.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// All currently loaded job profiles.
        /// </summary>
        public IEnumerable<IJobProfile> JobProfiles { get; private set; }

        /// <summary>
        /// State machine of the bot.
        /// </summary>
        public IAmeisenBotLogic Logic { get; private set; }

        /// <summary>
        /// Folder where  all profile relevant stuff is stored.
        /// </summary>
        public string ProfileFolder { get; }

        /// <summary>
        /// All currently loaded quest profiles.
        /// </summary>
        public IEnumerable<IQuestProfile> QuestProfiles { get; private set; }

        private TimegatedEvent BagUpdateEvent { get; set; }

        private int CurrentExecutionCount { get; set; }

        private TimegatedEvent EquipmentUpdateEvent { get; set; }

        private Stopwatch ExecutionMsStopwatch { get; }

        private bool NeedToSetupRconClient { get; set; }

        private TimegatedEvent PoiCacheEvent { get; }

        private TimegatedEvent RconEvent { get; }

        private LockedTimer StateMachineTimer { get; set; }

        private bool TalentUpdateRunning { get; set; }

        /// <summary>
        /// Use this method to destroy the bots instance
        /// </summary>
        public void Dispose()
        {
            IsRunning = false;
            AmeisenLogger.I.Log("AmeisenBot", "Stopping", LogLevel.Debug);

            Bot.Storage.SaveAll();

            if (Config.SaveWowWindowPosition)
            {
                SaveWowWindowPosition();
            }

            if (Config.AutocloseWow || Config.AutoPositionWow)
            {
                if (Bot.Wow.IsReady)
                {
                    Bot.Wow.LuaDoString("ForceQuit()");

                    // wait 3 sec for wow to exit, otherwise we kill it
                    TimeSpan timeToWait = TimeSpan.FromSeconds(3);
                    DateTime exited = DateTime.UtcNow;

                    while (!Bot.Memory.Process.HasExited)
                    {
                        if (DateTime.UtcNow - exited > timeToWait)
                        {
                            Bot.Memory.Process.Kill();
                            break;
                        }
                        else
                        {
                            Task.Delay(50).Wait();
                        }
                    }
                }
                else
                {
                    Bot.Memory.Process?.Kill();
                }
            }

            Bot.PathfindingHandler.Stop();

            Bot.Wow.Dispose();
            Bot.Memory.Dispose();

            Bot.Db.Save(Path.Combine(ProfileFolder, "db.json"));

            AmeisenLogger.I.Log("AmeisenBot", $"Exiting AmeisenBot", LogLevel.Debug);

            OnStatusChanged?.Invoke();
        }

        /// <summary>
        /// Pauses the bots engine, nothing will be executed, call Resume() to resume the engine.
        /// </summary>
        public void Pause()
        {
            if (IsRunning)
            {
                IsRunning = false;
                AmeisenLogger.I.Log("AmeisenBot", "Pausing", LogLevel.Debug);

                // if (StateMachine.CurrentState.Key is not BotState.StartWow
                //     and not BotState.Login
                //     and not BotState.LoadingScreen)
                // {
                //     StateMachine.SetState(BotState.Idle);
                // }

                OnStatusChanged?.Invoke();
            }
        }

        /// <summary>
        /// Reloads the bots config
        /// </summary>
        /// <param name="newConfig">New config to load</param>
        public void ReloadConfig(AmeisenBotConfig newConfig)
        {
            Bot.Storage.SaveAll();

            bool oldRconState = Config.RconEnabled;

            Config = newConfig;
            StateMachineTimer.SetInterval(Config.StateMachineTickMs);
            LoadProfiles();

            if (!oldRconState && Config.RconEnabled)
            {
                AmeisenLogger.I.Log("Rcon", "Starting Rcon", LogLevel.Debug);
                StateMachineTimer.OnTick += RconClientTimerTick;
            }
            else if (oldRconState && !Config.RconEnabled)
            {
                AmeisenLogger.I.Log("Rcon", "Stopping Rcon", LogLevel.Debug);
                StateMachineTimer.OnTick -= RconClientTimerTick;
            }

            Bot.Storage.LoadAll();
        }

        /// <summary>
        /// Resumes the bots engine, call Pause() to pause the engine.
        /// </summary>
        public void Resume()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                AmeisenLogger.I.Log("AmeisenBot", "Resuming", LogLevel.Debug);

                OnStatusChanged?.Invoke();
            }
        }

        /// <summary>
        /// Starts the bots engine, only call this once, use Pause() and
        /// Resume() to control the execution of the engine afterwards
        /// </summary>
        public void Start()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                AmeisenLogger.I.Log("AmeisenBot", "Starting", LogLevel.Debug);

                SubscribeToWowEvents();

                StateMachineTimer = new(Config.StateMachineTickMs, StateMachineTimerTick);

                if (Config.RconEnabled)
                {
                    AmeisenLogger.I.Log("Rcon", "Starting Rcon Timer", LogLevel.Debug);
                    StateMachineTimer.OnTick += RconClientTimerTick;
                }

                Bot.Storage.LoadAll();

                AmeisenLogger.I.Log("AmeisenBot", "Setup done", LogLevel.Debug);
                OnStatusChanged?.Invoke();
            }
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
                CompilerErrorCollection errors = results.Errors;
                StringBuilder sb = new();

                for (int i = 0; i < errors.Count; ++i)
                {
                    sb.AppendLine($"Error {errors[i].ErrorNumber} Line: {errors[i].Line}: {errors[i].ErrorText}");
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
                new UniversalBattlegroundEngine(Bot),
                new ArathiBasin(Bot),
                new StrandOfTheAncients(Bot),
                new EyeOfTheStorm(Bot),
                new RunBoyRunEngine(Bot)
            };
        }

        private void InitCombatClasses()
        {
            string combatClassNamespace = "AmeisenBotX.Core.Engines.Combat.Classes";

            IEnumerable<Type> combatClassTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.GetInterfaces().Contains(typeof(ICombatClass))
                         && x.Namespace != null
                         && x.Namespace.Contains(combatClassNamespace));

            List<ICombatClass> combatClassInstances = new();

            // Add combat classes with bot parameter
            combatClassInstances.AddRange(combatClassTypes.Where(x => x.GetConstructor(new Type[] { typeof(AmeisenBotInterfaces) }) != null)
                .Select(x => (ICombatClass)Activator.CreateInstance(x, Bot)));

            CombatClasses = combatClassInstances;
        }

        private void InitGrindingProfiles()
        {
            // add grinding profiles here
            GrindingProfiles = new List<IGrindingProfile>()
            {
                new UltimateGrinding1To80(),
                new DurotarGrindTo6(),
                new DurotarGrindTo11(),
                new DurotarGrindTo14(),
                new BarrensGrindTo17()
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
                new DeathknightStartAreaQuestProfile(Bot),
                new X5Horde1To80Profile(Bot),
                new Horde1To60GrinderProfile(Bot)
            };
        }

        private void LoadCustomCombatClass()
        {
            AmeisenLogger.I.Log("AmeisenBot", $"Loading custom CombatClass: {Config.CustomCombatClassFile}", LogLevel.Debug);

            if (Config.CustomCombatClassFile.Length == 0
                || !File.Exists(Config.CustomCombatClassFile))
            {
                AmeisenLogger.I.Log("AmeisenBot", "Loading default CombatClass", LogLevel.Debug);
                Bot.CombatClass = LoadClassByName(CombatClasses, Config.BuiltInCombatClassName);
            }
            else
            {
                try
                {
                    Bot.CombatClass = CompileCustomCombatClass();
                    OnCombatClassCompilationResult?.Invoke(true, string.Empty, string.Empty);
                    AmeisenLogger.I.Log("AmeisenBot", $"Compiling custom CombatClass successful", LogLevel.Debug);
                }
                catch (Exception e)
                {
                    AmeisenLogger.I.Log("AmeisenBot", $"Compiling custom CombatClass failed:\n{e}", LogLevel.Error);
                    OnCombatClassCompilationResult?.Invoke(false, e.GetType().Name, e.ToString());
                    Bot.CombatClass = LoadClassByName(CombatClasses, Config.BuiltInCombatClassName);
                }
            }
        }

        private void LoadProfiles()
        {
            if (Config.UseBuiltInCombatClass)
            {
                Bot.CombatClass = LoadClassByName(CombatClasses, Config.BuiltInCombatClassName);
            }
            else
            {
                LoadCustomCombatClass();
            }

            // if a combatclass specified an ItemComparator
            // use it instead of the default one
            if (Bot.CombatClass?.ItemComparator != null)
            {
                Bot.Character.ItemComparator = Bot.CombatClass.ItemComparator;

                if (Bot.CombatClass is IStoreable s)
                {
                    Bot.Storage.Register(s);
                }
            }

            Bot.Battleground = LoadClassByName(BattlegroundEngines, Config.BattlegroundEngine);
            Bot.Grinding.Profile = LoadClassByName(GrindingProfiles, Config.GrindingProfile);
            Bot.Jobs.Profile = LoadClassByName(JobProfiles, Config.JobProfile);
            Bot.Quest.Profile = LoadClassByName(QuestProfiles, Config.QuestProfile);
        }

        private void OnBagChanged(long timestamp, List<string> args)
        {
            if (BagUpdateEvent.Run())
            {
                Bot.Character.Inventory.Update();
                Bot.Character.Equipment.Update();

                Bot.Character.UpdateGear();
                Bot.Character.UpdateBags();

                Bot.Character.Inventory.Update();
            }
        }

        private void OnBattlegroundStatusChanged(string s)
        {
            AmeisenLogger.I.Log("AmeisenBot", $"OnBattlegroundStatusChanged: {s}");
        }

        private void OnClassTrainerShow(long timestamp, List<string> args)
        {
            // todo: Config.TrainSpells
            if (!Bot.Target.IsClassTrainer) return;

            TrainAllSpellsRoutine.Run(Bot, Config);
            Bot.Character.LastLevelTrained = Bot.Player.Level;
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
                Bot.Wow.ClickUiElement("LFDDungeonReadyDialogEnterDungeonButton");
            }
        }

        private void OnLfgRoleCheckShow(long timestamp, List<string> args)
        {
            if (Config.AutojoinLfg)
            {
                Bot.Wow.SetLfgRole(Bot.CombatClass != null ? Bot.CombatClass.Role : WowRole.Dps);
            }
        }

        private void OnLootRollStarted(long timestamp, List<string> args)
        {
            if (int.TryParse(args[0], out int rollId))
            {
                string itemLink = Bot.Wow.GetLootRollItemLink(rollId);
                string itemJson = Bot.Wow.GetItemByNameOrLink(itemLink);

                WowBasicItem item = ItemFactory.BuildSpecificItem(ItemFactory.ParseItem(itemJson));

                if (item.Name == "0" || item.ItemLink == "0")
                {
                    // get the item id and try again
                    itemJson = Bot.Wow.GetItemByNameOrLink
                    (
                        itemLink.Split(new string[] { "Hitem:" }, StringSplitOptions.RemoveEmptyEntries)[1]
                            .Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]
                    );

                    item = ItemFactory.BuildSpecificItem(ItemFactory.ParseItem(itemJson));
                }

                if (Bot.Character.IsItemAnImprovement(item, out IWowInventoryItem itemToReplace))
                {
                    AmeisenLogger.I.Log("WoWEvents", $"Would like to replace item {item?.Name} with {itemToReplace?.Name}, rolling need", LogLevel.Verbose);

                    Bot.Wow.RollOnLoot(rollId, WowRollType.Need);
                    return;
                }
            }

            Bot.Wow.RollOnLoot(rollId, WowRollType.Pass);
        }

        private void OnLootWindowOpened(long timestamp, List<string> args)
        {
            if (Config.LootOnlyMoneyAndQuestitems)
            {
                Bot.Wow.LootMoneyAndQuestItems();
                return;
            }

            Bot.Wow.LootEverything();
        }

        private void OnMerchantShow(long timestamp, List<string> args)
        {
            if (Config.AutoRepair && Bot.Target != null && Bot.Target.IsRepairer)
            {
                Bot.Wow.RepairAllItems();
            }

            if (Config.AutoSell)
            {
                SellItemsRoutine.Run(Bot, Config);
            }
        }

        private void OnObjectUpdateComplete(IEnumerable<IWowObject> wowObjects)
        {
            if (Config.CachePointsOfInterest && PoiCacheEvent.Run())
            {
                IEnumerable<IWowGameobject> wowGameobjects = wowObjects.OfType<IWowGameobject>();
                IEnumerable<IWowUnit> wowUnits = wowObjects.OfType<IWowUnit>();

                // Remember Ore/Herb positions for farming
                foreach (IWowGameobject gameobject in wowGameobjects.Where(e => Enum.IsDefined(typeof(WowOreId), e.DisplayId)))
                {
                    Bot.Db.CacheOre(Bot.Objects.MapId, (WowOreId)gameobject.DisplayId, gameobject.Position);
                }

                foreach (IWowGameobject gameobject in wowGameobjects.Where(e => Enum.IsDefined(typeof(WowHerbId), e.DisplayId)))
                {
                    Bot.Db.CacheHerb(Bot.Objects.MapId, (WowHerbId)gameobject.DisplayId, gameobject.Position);
                }

                // Remember Mailboxes
                foreach (IWowGameobject gameobject in wowGameobjects.Where(e => e.GameObjectType == WowGameObjectType.Mailbox))
                {
                    Bot.Db.CachePoi(Bot.Objects.MapId, PoiType.Mailbox, gameobject.Position);
                }

                // Remember Auctioneers
                foreach (IWowUnit unit in wowUnits.Where(e => e.IsAuctioneer))
                {
                    Bot.Db.CachePoi(Bot.Objects.MapId, PoiType.Auctioneer, unit.Position);
                }

                // Remember Fishingspots and places where people fished at
                foreach (IWowGameobject gameobject in wowGameobjects.Where(e => e.GameObjectType is WowGameObjectType.FishingHole or WowGameObjectType.FishingBobber))
                {
                    IWowUnit originUnit = wowObjects.OfType<IWowUnit>().FirstOrDefault(e => e.Guid == gameobject.CreatedBy);

                    // dont cache positions too close to eachother
                    if (originUnit != null && !Bot.Db.TryGetPointsOfInterest(Bot.Objects.MapId, PoiType.FishingSpot, originUnit.Position, 5.0f, out IEnumerable<Vector3> pois))
                    {
                        Bot.Db.CachePoi(Bot.Objects.MapId, PoiType.FishingSpot, originUnit.Position);
                    }
                }

                // Remember Vendors
                foreach (IWowUnit unit in wowUnits.Where(e => e.IsVendor))
                {
                    Bot.Db.CachePoi(Bot.Objects.MapId, PoiType.Vendor, unit.Position);
                }

                // Remember Repair Vendors
                foreach (IWowUnit unit in wowUnits.Where(e => e.IsRepairer))
                {
                    Bot.Db.CachePoi(Bot.Objects.MapId, PoiType.Repair, unit.Position);
                }
            }
        }

        private void OnPartyInvitation(long timestamp, List<string> args)
        {
            if (!Config.OnlyFriendsMode || (args.Count >= 1 && Config.Friends.Split(',').Any(e => e.Equals(args[0], StringComparison.OrdinalIgnoreCase))))
            {
                Bot.Wow.AcceptPartyInvite();
            }
        }

        private void OnPvpQueueShow(long timestamp, List<string> args)
        {
            if (Config.AutojoinBg && args.Count == 1 && args[0] == "1")
            {
                Bot.Wow.AcceptBattlegroundInvite();
            }
        }

        private void OnQuestAcceptConfirm(long timestamp, List<string> args)
        {
            if (Config.AutoAcceptQuests)
            {
                Bot.Wow.LuaDoString("ConfirmAcceptQuest();");
            }
        }

        private void OnQuestGreeting(long timestamp, List<string> args)
        {
            if (Config.AutoAcceptQuests)
            {
                Bot.Wow.AcceptQuests();
            }
        }

        private void OnQuestProgress(long timestamp, List<string> args)
        {
            if (Config.AutoAcceptQuests)
            {
                Bot.Wow.ClickUiElement("QuestFrameCompleteQuestButton");
            }
        }

        private void OnReadyCheck(long timestamp, List<string> args)
        {
            Bot.Wow.CofirmReadyCheck(true);
        }

        private void OnShowQuestFrame(long timestamp, List<string> args)
        {
            if (Config.AutoAcceptQuests)
            {
                Bot.Wow.LuaDoString("AcceptQuest();");
            }
        }

        private void OnStaticPopup(string s)
        {
            AmeisenLogger.I.Log("AmeisenBot", $"OnStaticPopup: {s}");

            foreach (string popup in s.Split(';'))
            {
                // 0 = slot of the popup, 1 = type of the popup
                string[] parts = popup.Split(':');

                if (int.TryParse(parts[0], out int id))
                {
                    AmeisenLogger.I.Log("AmeisenBot", $"Static Popup => ID: {id} -> {parts[1]}");

                    switch (parts[1].ToUpper())
                    {
                        case "AUTOEQUIP_BIND":
                        case "BFMGR_INVITED_TO_ENTER":
                        case "CONFIRM_BATTLEFIELD_ENTRY":
                        case "CONFIRM_LOOT_ROLL":
                        case "EQUIP_BIND":
                        case "LOOT_BIND":
                        case "RESURRECT":
                        case "USE_BIND":
                            Bot.Wow.ClickUiElement($"StaticPopup{parts[0]}Button1");
                            break;

                        case "TOO_MANY_LUA_ERRORS":
                            Bot.Wow.ClickUiElement($"StaticPopup{parts[0]}Button2");
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private void OnSummonRequest(long timestamp, List<string> args)
        {
            Bot.Wow.AcceptSummon();
        }

        private void OnTalentPointsChange(long timestamp, List<string> args)
        {
            if (Bot.CombatClass != null && Bot.CombatClass.Talents != null && !TalentUpdateRunning)
            {
                TalentUpdateRunning = true;
                Bot.Character.TalentManager.Update();
                Bot.Character.TalentManager.SelectTalents(Bot.CombatClass.Talents, Bot.Wow.GetUnspentTalentPoints());
                TalentUpdateRunning = false;
            }
        }

        private void OnTradeAcceptUpdate(long timestamp, List<string> args)
        {
            Bot.Wow.LuaDoString("AcceptTrade();");
        }

        private void RconClientTimerTick()
        {
            if (IsRunning && Bot.Rcon != null && RconEvent.Run())
            {
                try
                {
                    if (Bot.Rcon.NeedToRegister)
                    {
                        Bot.Rcon.Register();
                    }
                    else
                    {
                        int currentResource = Bot.Player.Class switch
                        {
                            WowClass.Deathknight => Bot.Player.Runeenergy,
                            WowClass.Rogue => Bot.Player.Energy,
                            WowClass.Warrior => Bot.Player.Rage,
                            _ => Bot.Player.Mana,
                        };

                        int maxResource = Bot.Player.Class switch
                        {
                            WowClass.Deathknight => Bot.Player.MaxRuneenergy,
                            WowClass.Rogue => Bot.Player.MaxEnergy,
                            WowClass.Warrior => Bot.Player.MaxRage,
                            _ => Bot.Player.MaxMana,
                        };

                        Bot.Rcon.SendData(new DataMessage()
                        {
                            BagSlotsFree = 0,
                            CombatClass = Bot.CombatClass != null ? Bot.CombatClass.Role.ToString() : "NoCombatClass",
                            CurrentProfile = string.Empty,
                            Energy = currentResource,
                            Exp = Bot.Player.Xp,
                            Health = Bot.Player.Health,
                            ItemLevel = (int)MathF.Round(Bot.Character.Equipment.AverageItemLevel),
                            Level = Bot.Player.Level,
                            MapName = Bot.Objects.MapId.ToString(),
                            MaxEnergy = maxResource,
                            MaxExp = Bot.Player.NextLevelXp,
                            MaxHealth = Bot.Player.MaxHealth,
                            Money = Bot.Character.Money,
                            PosX = Bot.Player.Position.X,
                            PosY = Bot.Player.Position.Y,
                            PosZ = Bot.Player.Position.Z,
                            State = string.Empty, // StateMachine.CurrentState.Key.ToString(),
                            SubZoneName = Bot.Objects.ZoneSubName,
                            ZoneName = Bot.Objects.ZoneName,
                        });

                        Rect rc = Bot.Memory.GetClientSize();

                        using Bitmap bmp = new(rc.Right - rc.Left, rc.Bottom - rc.Top, PixelFormat.Format32bppArgb);
                        using Graphics g = Graphics.FromImage(bmp);
                        g.CopyFromScreen(rc.Left, rc.Top, 0, 0, new(rc.Right - rc.Left, rc.Bottom - rc.Top));

                        using MemoryStream ms = new();
                        bmp.Save(ms, ImageFormat.Png);

                        Bot.Rcon.SendImage($"data:image/png;base64,{Convert.ToBase64String(ms.GetBuffer())}");

                        Bot.Rcon.PullPendingActions();

                        if (Bot.Rcon.PendingActions.Any())
                        {
                            switch (Bot.Rcon.PendingActions.First())
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

                                default:
                                    break;
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private void SaveWowWindowPosition()
        {
            if (!Config.AutoPositionWow)
            {
                try
                {
                    Config.WowWindowRect = Bot.Memory.GetWindowPosition();
                }
                catch (Exception e)
                {
                    AmeisenLogger.I.Log("AmeisenBot", $"Failed to save wow window position:\n{e}", LogLevel.Error);
                }
            }
        }

        private void SetupRconClient()
        {
            Bot.Objects.OnObjectUpdateComplete += delegate
            {
                if (!NeedToSetupRconClient && Bot.Player != null)
                {
                    NeedToSetupRconClient = true;
                    Bot.Rcon = new
                    (
                        Config.RconServerAddress,
                        Bot.Db.GetUnitName(Bot.Player, out string name) ? name : "unknown",
                        Bot.Player.Race.ToString(),
                        Bot.Player.Gender.ToString(),
                        Bot.Player.Class.ToString(),
                        Bot.CombatClass != null ? Bot.CombatClass.Role.ToString() : "dps",
                        Config.RconServerImage,
                        Config.RconServerGuid
                    );
                }
            };
        }

        private void StateMachineTimerTick()
        {
            if (IsRunning)
            {
                ExecutionMsStopwatch.Restart();
                Logic.Tick();
                CurrentExecutionMs = ExecutionMsStopwatch.ElapsedMilliseconds;
                CurrentExecutionCount++;
            }
        }

        private void SubscribeToWowEvents()
        {
            BagUpdateEvent = new(TimeSpan.FromSeconds(1));
            EquipmentUpdateEvent = new(TimeSpan.FromSeconds(1));

            // Request Events
            Bot.Wow.Events.Subscribe("PARTY_INVITE_REQUEST", OnPartyInvitation);
            Bot.Wow.Events.Subscribe("CONFIRM_SUMMON", OnSummonRequest);
            Bot.Wow.Events.Subscribe("READY_CHECK", OnReadyCheck);

            // Loot/Item Events
            Bot.Wow.Events.Subscribe("LOOT_OPENED", OnLootWindowOpened);
            Bot.Wow.Events.Subscribe("START_LOOT_ROLL", OnLootRollStarted);
            Bot.Wow.Events.Subscribe("BAG_UPDATE", OnBagChanged);
            Bot.Wow.Events.Subscribe("PLAYER_EQUIPMENT_CHANGED", OnEquipmentChanged);
            // Bot.EventHookManager.Subscribe("DELETE_ITEM_CONFIRM", OnConfirmDeleteItem);

            // Merchant Events
            // Bot.EventHookManager.Subscribe("MERCHANT_SHOW", OnMerchantShow);

            // PvP Events
            Bot.Wow.Events.Subscribe("UPDATE_BATTLEFIELD_STATUS", OnPvpQueueShow);
            Bot.Wow.Events.Subscribe("PVPQUEUE_ANYWHERE_SHOW", OnPvpQueueShow);

            // Dungeon Events
            Bot.Wow.Events.Subscribe("LFG_ROLE_CHECK_SHOW", OnLfgRoleCheckShow);
            Bot.Wow.Events.Subscribe("LFG_PROPOSAL_SHOW", OnLfgProposalShow);

            // Quest Events
            Bot.Wow.Events.Subscribe("QUEST_DETAIL", OnShowQuestFrame);
            Bot.Wow.Events.Subscribe("QUEST_ACCEPT_CONFIRM", OnQuestAcceptConfirm);
            Bot.Wow.Events.Subscribe("QUEST_GREETING", OnQuestGreeting);
            Bot.Wow.Events.Subscribe("QUEST_COMPLETE", OnQuestProgress);
            Bot.Wow.Events.Subscribe("QUEST_PROGRESS", OnQuestProgress);
            Bot.Wow.Events.Subscribe("GOSSIP_SHOW", OnQuestGreeting);

            // Trading Events
            Bot.Wow.Events.Subscribe("TRADE_ACCEPT_UPDATE", OnTradeAcceptUpdate);

            // Chat Events
            Bot.Wow.Events.Subscribe("CHAT_MSG_ADDON", (t, a) => Bot.Chat.TryParseMessage(WowChat.ADDON, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_CHANNEL", (t, a) => Bot.Chat.TryParseMessage(WowChat.CHANNEL, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_EMOTE", (t, a) => Bot.Chat.TryParseMessage(WowChat.EMOTE, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_FILTERED", (t, a) => Bot.Chat.TryParseMessage(WowChat.FILTERED, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_GUILD", (t, a) => Bot.Chat.TryParseMessage(WowChat.GUILD, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_GUILD_ACHIEVEMENT", (t, a) => Bot.Chat.TryParseMessage(WowChat.GUILD_ACHIEVEMENT, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_IGNORED", (t, a) => Bot.Chat.TryParseMessage(WowChat.IGNORED, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_MONSTER_EMOTE", (t, a) => Bot.Chat.TryParseMessage(WowChat.MONSTER_EMOTE, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_MONSTER_PARTY", (t, a) => Bot.Chat.TryParseMessage(WowChat.MONSTER_PARTY, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_MONSTER_SAY", (t, a) => Bot.Chat.TryParseMessage(WowChat.MONSTER_SAY, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_MONSTER_WHISPER", (t, a) => Bot.Chat.TryParseMessage(WowChat.MONSTER_WHISPER, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_MONSTER_YELL", (t, a) => Bot.Chat.TryParseMessage(WowChat.MONSTER_YELL, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_OFFICER", (t, a) => Bot.Chat.TryParseMessage(WowChat.OFFICER, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_PARTY", (t, a) => Bot.Chat.TryParseMessage(WowChat.PARTY, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_PARTY_LEADER", (t, a) => Bot.Chat.TryParseMessage(WowChat.PARTY_LEADER, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_RAID", (t, a) => Bot.Chat.TryParseMessage(WowChat.RAID, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_RAID_BOSS_EMOTE", (t, a) => Bot.Chat.TryParseMessage(WowChat.RAID_BOSS_EMOTE, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_RAID_BOSS_WHISPER", (t, a) => Bot.Chat.TryParseMessage(WowChat.RAID_BOSS_WHISPER, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_RAID_LEADER", (t, a) => Bot.Chat.TryParseMessage(WowChat.RAID_LEADER, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_RAID_WARNING", (t, a) => Bot.Chat.TryParseMessage(WowChat.RAID_WARNING, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_SAY", (t, a) => Bot.Chat.TryParseMessage(WowChat.SAY, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_SYSTEM", (t, a) => Bot.Chat.TryParseMessage(WowChat.SYSTEM, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_TEXT_EMOTE", (t, a) => Bot.Chat.TryParseMessage(WowChat.TEXT_EMOTE, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_WHISPER", (t, a) => Bot.Chat.TryParseMessage(WowChat.WHISPER, t, a));
            Bot.Wow.Events.Subscribe("CHAT_MSG_YELL", (t, a) => Bot.Chat.TryParseMessage(WowChat.YELL, t, a));

            // Misc Events
            Bot.Wow.Events.Subscribe("CHARACTER_POINTS_CHANGED", OnTalentPointsChange);
            Bot.Wow.Events.Subscribe("COMBAT_LOG_EVENT_UNFILTERED", Bot.CombatLog.Parse);

            // NPC Events
            Bot.Wow.Events.Subscribe("MERCHANT_SHOW", OnMerchantShow);
            Bot.Wow.Events.Subscribe("TRAINER_SHOW", OnClassTrainerShow);
        }
    }
}