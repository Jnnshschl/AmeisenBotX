using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Logic.Enums;
using AmeisenBotX.Core.Logic.Idle;
using AmeisenBotX.Core.Logic.Idle.Actions;
using AmeisenBotX.Core.Logic.Routines;
using AmeisenBotX.Core.Logic.StaticDeathRoutes;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Core.Objects;
using AmeisenBotX.Core.Objects.Enums;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory.Win32;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AmeisenBotX.Core.Logic
{
    public class AmeisenBotLogic : IAmeisenBotLogic
    {
        private readonly List<IStaticDeathRoute> StaticDeathRoutes = new()
        {
            new ForgeOfSoulsDeathRoute(),
            new PitOfSaronDeathRoute()
        };

        public AmeisenBotLogic(AmeisenBotConfig config, AmeisenBotInterfaces bot)
        {
            Config = config;
            Bot = bot;

            FirstStart = true;
            Random = new();

            Mode = BotMode.None;

            IdleActionManager = new(new List<IIdleAction>()
            {
                new AuctionHouseIdleAction(bot),
                new CheckMailsIdleAction(bot),
                new FishingIdleAction(bot),
                new LookAroundIdleAction(bot),
                new LookAtGroupIdleAction(bot),
                new RandomEmoteIdleAction(bot),
                new SitByCampfireIdleAction(bot),
                new SitToChairIdleAction(bot, Config.MinFollowDistance),
            });

            AntiAfkEvent = new(TimeSpan.FromMilliseconds(1200));
            CharacterUpdateEvent = new(TimeSpan.FromMilliseconds(5000));
            EatBlockEvent = new(TimeSpan.FromMilliseconds(30000));
            EatEvent = new(TimeSpan.FromMilliseconds(250));
            IdleActionEvent = new(TimeSpan.FromMilliseconds(1000));
            LoginAttemptEvent = new(TimeSpan.FromMilliseconds(500));
            LootTryEvent = new(TimeSpan.FromMilliseconds(750));
            NpcInteractionEvent = new(TimeSpan.FromMilliseconds(1000));
            OffsetCheckEvent = new(TimeSpan.FromMilliseconds(15000));
            PartymembersFightEvent = new(TimeSpan.FromMilliseconds(1000));
            RenderSwitchEvent = new(TimeSpan.FromMilliseconds(1000));
            UpdateFood = new(TimeSpan.FromMilliseconds(1000));

            UnitsLooted = new();
            UnitsToLoot = new();

            // OPEN WORLD -----------------------------

            INode openworldGhostNode = new Selector
            (
                () => CanUseStaticPaths(),
                // prefer static paths
                new Leaf(() => MoveToPosition(StaticRoute.GetNextPoint(Bot.Player.Position))),
                // run to corpse by position
                new Leaf(RunToCorpseAndRetrieveIt)
            );

            INode openworldCombatNode = new Selector
            (
                () => Bot.CombatClass == null,
                // start autoattacking if we have no combat class loaded
                new Selector
                (
                    () => !Bot.Player.IsInMeleeRange(Bot.Target),
                    new Leaf(() => MoveToPosition(Bot.Target.Position)),
                    new Selector
                    (
                        () => !Bot.Player.IsAutoAttacking,
                        new Leaf(() => { Bot.Wow.StartAutoAttack(); return BtStatus.Success; }),
                        new Leaf(() => { return BtStatus.Success; })
                    )
                ),
                // TODO: handle tactics here
                // run combat class logic
                new Selector
                (
                    // combatclass handles movement itself or has no target
                    () => Bot.CombatClass.HandlesMovement || Bot.Target == null,
                    new Leaf(() => { Bot.CombatClass.Execute(); return BtStatus.Success; }),
                    new Selector
                    (
                        // check whether we need to move
                        () => !Bot.Objects.IsTargetInLineOfSight,
                        new Leaf(() => MoveToPosition(Bot.Target.Position)),
                        new Waterfall
                        (
                            // fallback, run to the target unit
                            new Leaf(() => { Bot.CombatClass.Execute(); return BtStatus.Success; }),
                            // dps logic
                            (
                                () => Bot.CombatClass.Role == WowRole.Dps,
                                new Selector
                                (
                                    () => Bot.CombatClass.IsMelee,
                                    new Selector
                                    (
                                        () => !Bot.Player.IsInMeleeRange(Bot.Target),
                                        new Leaf(() => MoveToPosition(Bot.Target.Position)),
                                        new Leaf(() => { Bot.CombatClass.Execute(); return BtStatus.Success; })
                                    ),
                                    new Selector
                                    (
                                        () => Bot.Player.DistanceTo(Bot.Target) > 26.5f + Bot.Target.CombatReach,
                                        new Leaf(() => MoveToPosition(Bot.Target.Position)),
                                        new Leaf(() => { Bot.CombatClass.Execute(); return BtStatus.Success; })
                                    )
                                )
                            ),
                            // tank logic
                            (
                                () => Bot.CombatClass.Role == WowRole.Tank,
                                new Selector
                                (
                                    () => !Bot.Player.IsInMeleeRange(Bot.Target),
                                    new Leaf(() => MoveToPosition(Bot.Target.Position)),
                                    new Leaf(() => { Bot.CombatClass.Execute(); return BtStatus.Success; })
                                )
                            ),
                            // heal logic
                            (
                                () => Bot.CombatClass.Role == WowRole.Heal,
                                new Selector
                                (
                                    () => Bot.Player.DistanceTo(Bot.Target) > 23.5f + Bot.Target.CombatReach,
                                    new Leaf(() => MoveToPosition(Bot.Target.Position)),
                                    new Leaf(() => { Bot.CombatClass.Execute(); return BtStatus.Success; })
                                )
                            )
                        )
                    )
                )
            );

            INode grindingNode = new Waterfall
            (
                new Leaf(() => { Bot.Grinding.Execute(); return BtStatus.Success; }),
                (() => Bot.Player.IsDead, new Leaf(Dead)),
                (() => Bot.Player.IsGhost, openworldGhostNode),
                (NeedToFight, openworldCombatNode),
                (NeedToRepairOrSell, new Leaf(SpeakWithMerchant)),
                (NeedToTrainSpells, new Leaf(SpeakWithClassTrainer)),
                (NeedToLoot, new Leaf(LootNearUnits)),
                (NeedToEat, new Leaf(Eat))
            );

            INode questingNode = new Waterfall
            (
                new Leaf(() => { Bot.Quest.Execute(); return BtStatus.Success; }),
                (() => Bot.Player.IsDead, new Leaf(Dead)),
                (() => Bot.Player.IsGhost, openworldGhostNode),
                (NeedToFight, openworldCombatNode),
                (NeedToRepairOrSell, new Leaf(SpeakWithMerchant)),
                (NeedToLoot, new Leaf(LootNearUnits)),
                (NeedToEat, new Leaf(Eat))
            );

            INode testingNode = new Waterfall
            (
                new Leaf(() => { Bot.Test.Execute(); return BtStatus.Success; }),
                (() => Bot.Player.IsDead, new Leaf(Dead)),
                (() => Bot.Player.IsGhost, openworldGhostNode)
            );

            INode openworldNode = new Waterfall
            (
                // do idle stuff as fallback
                new Leaf(Idle),
                // handle main open world states
                (() => Bot.Player.IsDead, new Leaf(Dead)),
                (() => Bot.Player.IsGhost, openworldGhostNode),
                (NeedToFight, openworldCombatNode),
                (NeedToRepairOrSell, new Leaf(SpeakWithMerchant)),
                (NeedToLoot, new Leaf(LootNearUnits)),
                (NeedToEat, new Leaf(Eat)),
                (NeedToFollow, new Leaf(Follow)),
                (() => Config.IdleActions && IdleActionEvent.Run(), new Leaf(() => { IdleActionManager.Tick(Config.Autopilot); return BtStatus.Success; }))
            );

            // SPECIAL ENVIRONMENTS -----------------------------

            INode battlegroundNode = new Selector
            (
                IsBattlegroundFinished,
                // leave battleground once it is finished
                new Leaf(() => { Bot.Wow.LeaveBattleground(); Bot.Battleground.Leave(); return BtStatus.Success; }),
                // TODO: run bg engine here
                new Leaf(() => { Bot.Battleground.Execute(); return BtStatus.Success; })
            );

            INode dungeonNode = new Waterfall
            (
                new Selector
                (
                    () => Config.DungeonUsePartyMode && NeedToFollow(),
                    // just follow when we use party mode in dungeon
                    new Leaf(Follow),
                    new Leaf(() => { Bot.Dungeon.Execute(); return BtStatus.Success; })
                ),
                (() => Bot.Player.IsDead, new Leaf(DeadDungeon)),
                //TODO: implement specialized dungeon combat logic
                (NeedToFight, openworldCombatNode),
                (NeedToLoot, new Leaf(LootNearUnits)),
                (NeedToEat, new Leaf(Eat))
            );

            INode raidNode = new Waterfall
            (
                new Selector
                (
                    () => Config.DungeonUsePartyMode && NeedToFollow(),
                    // just follow when we use party mode in dungeon
                    new Leaf(Follow),
                    new Leaf(() => { Bot.Dungeon.Execute(); return BtStatus.Success; })
                ),
                //TODO: implement specialized raid combat logic
                (NeedToFight, openworldCombatNode),
                (NeedToLoot, new Leaf(LootNearUnits)),
                (NeedToEat, new Leaf(Eat))
            );

            // GENERIC -----------------------------

            INode mainLogicNode = new Annotator
            (
                // run the update stuff before we execute the main logic
                // objects will be updated here for example
                new Leaf(UpdateWowInterface),
                new Selector
                (
                    () => Bot.Objects.IsWorldLoaded && Bot.Player != null && Bot.Objects != null,
                    new Annotator
                    (
                        // update stuff that needs us to be ingame
                        new Leaf(UpdateIngame),
                        new Waterfall
                        (
                            // open world auto behavior as fallback
                            openworldNode,
                            // handle special environments
                            (() => Bot.Objects.MapId.IsBattlegroundMap(), battlegroundNode),
                            (() => Bot.Objects.MapId.IsDungeonMap(), dungeonNode),
                            (() => Bot.Objects.MapId.IsRaidMap(), raidNode),
                            // handle open world modes
                            (() => Mode == BotMode.Grinding, grindingNode),
                            (() => Mode == BotMode.Questing, questingNode),
                            (() => Mode == BotMode.Testing, testingNode)
                        )
                    ),
                    // we are most likely in the loading screen or player/objects are null
                    new Leaf(() => BtStatus.Success)
                )
            );

            Tree = new
            (
                new Waterfall
                (
                    // run the anti afk and main logic if wow is running
                    // and we are logged in
                    new Annotator
                    (
                        new Leaf(AntiAfk),
                        mainLogicNode
                    ),
                    // accept tos and eula, start wow
                    (
                        () => Bot.Memory.Process == null || Bot.Memory.Process.HasExited,
                        new Sequence
                        (
                            new Leaf(CheckTosAndEula),
                            new Leaf(ChangeRealmlist),
                            new Leaf(StartWow)
                        )
                    ),
                    // setup interface and login
                    (() => !Bot.Wow.IsReady, new Leaf(SetupWowInterface)),
                    (NeedToLogin, new Leaf(Login))
                )
            );
        }

        public event Action OnWoWStarted;

        public BotMode Mode { get; private set; }

        private TimegatedEvent AntiAfkEvent { get; }

        private bool ArePartymembersInFight { get; set; }

        private AmeisenBotInterfaces Bot { get; }

        private TimegatedEvent CharacterUpdateEvent { get; }

        private AmeisenBotConfig Config { get; }

        private DateTime DungeonDiedTimestamp { get; set; }

        private TimegatedEvent EatBlockEvent { get; }

        private TimegatedEvent EatEvent { get; }

        private bool FirstStart { get; set; }

        private Vector3 FollowOffset { get; set; }

        private IEnumerable<IWowInventoryItem> Food { get; set; }

        private TimegatedEvent IdleActionEvent { get; }

        private IdleActionManager IdleActionManager { get; }

        private TimegatedEvent LoginAttemptEvent { get; }

        private int LootTry { get; set; }

        private TimegatedEvent LootTryEvent { get; }

        private IWowUnit Merchant { get; set; }

        private IWowUnit ClassTrainer { get; set; }

        private TimegatedEvent NpcInteractionEvent { get; }

        private TimegatedEvent OffsetCheckEvent { get; }

        private TimegatedEvent PartymembersFightEvent { get; }

        private IWowUnit PlayerToFollow { get; set; }

        private Random Random { get; }

        private TimegatedEvent RenderSwitchEvent { get; }

        private bool SearchedStaticRoutes { get; set; }

        private IStaticDeathRoute StaticRoute { get; set; }

        private Tree Tree { get; }

        private List<ulong> UnitsLooted { get; }

        private Queue<ulong> UnitsToLoot { get; }

        private TimegatedEvent UpdateFood { get; }

        public void ChangeMode(BotMode mode)
        {
            Mode = mode;

            switch (Mode)
            {
                case BotMode.Questing:
                    Bot.Quest.Enter();
                    break;

                default:
                    break;
            }
        }

        public void Tick()
        {
            Tree.Tick();
        }

        private BtStatus AntiAfk()
        {
            if (AntiAfkEvent.Run())
            {
                Bot.Memory.Write(Bot.Wow.Offsets.TickCount, Environment.TickCount);
                AntiAfkEvent.Timegate = TimeSpan.FromMilliseconds(Random.Next(300, 2300));
            }

            return BtStatus.Success;
        }

        /// <summary>
        /// This method searches for static death routes, this is needed when pathfinding
        /// cannot find a good route from the graveyard to th dungeon entry. For example
        /// the ICC dungeons are only reachable by flying, its easier to use static routes.
        /// </summary>
        /// <returns>True when a static path can be used, false if not</returns>
        private bool CanUseStaticPaths()
        {
            if (!SearchedStaticRoutes)
            {
                if (Bot.Memory.Read(Bot.Wow.Offsets.CorpsePosition, out Vector3 corpsePosition))
                {
                    SearchedStaticRoutes = true;

                    Vector3 endPosition = Bot.Dungeon.Profile != null ? Bot.Dungeon.Profile.WorldEntry : corpsePosition;
                    IStaticDeathRoute staticRoute = StaticDeathRoutes.FirstOrDefault(e => e.IsUseable(Bot.Objects.MapId, Bot.Player.Position, endPosition));

                    if (staticRoute != null)
                    {
                        StaticRoute = staticRoute;
                        StaticRoute.Init(Bot.Player.Position);
                    }
                    else
                    {
                        staticRoute = StaticDeathRoutes.FirstOrDefault(e => e.IsUseable(Bot.Objects.MapId, Bot.Player.Position, corpsePosition));

                        if (staticRoute != null)
                        {
                            StaticRoute = staticRoute;
                            StaticRoute.Init(Bot.Player.Position);
                        }
                    }
                }
            }

            return StaticRoute != null;
        }

        private BtStatus ChangeRealmlist()
        {
            if (!Config.AutoChangeRealmlist)
            {
                return BtStatus.Success;
            }

            try
            {
                AmeisenLogger.I.Log("StartWow", "Changing Realmlist");
                string configWtfPath = Path.Combine(Directory.GetParent(Config.PathToWowExe).FullName, "wtf", "config.wtf");

                if (File.Exists(configWtfPath))
                {
                    bool editedFile = false;
                    List<string> content = File.ReadAllLines(configWtfPath).ToList();

                    if (!content.Any(e => e.Contains($"SET REALMLIST {Config.Realmlist}", StringComparison.OrdinalIgnoreCase)))
                    {
                        bool found = false;

                        for (int i = 0; i < content.Count; ++i)
                        {
                            if (content[i].Contains("SET REALMLIST", StringComparison.OrdinalIgnoreCase))
                            {
                                editedFile = true;
                                content[i] = $"SET REALMLIST {Config.Realmlist}";
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            editedFile = true;
                            content.Add($"SET REALMLIST {Config.Realmlist}");
                        }
                    }

                    if (editedFile)
                    {
                        File.SetAttributes(configWtfPath, FileAttributes.Normal);
                        File.WriteAllLines(configWtfPath, content);
                        File.SetAttributes(configWtfPath, FileAttributes.ReadOnly);
                    }
                }

                return BtStatus.Success;
            }
            catch
            {
                AmeisenLogger.I.Log("StartWow", "Cannot write realmlist to config.wtf");
            }

            return BtStatus.Failed;
        }

        private BtStatus CheckTosAndEula()
        {
            try
            {
                string configWtfPath = Path.Combine(Directory.GetParent(Config.PathToWowExe).FullName, "wtf", "config.wtf");

                if (File.Exists(configWtfPath))
                {
                    bool editedFile = false;
                    string content = File.ReadAllText(configWtfPath);

                    if (!content.Contains("SET READEULA \"0\"", StringComparison.OrdinalIgnoreCase))
                    {
                        editedFile = true;

                        if (content.Contains("SET READEULA", StringComparison.OrdinalIgnoreCase))
                        {
                            content = content.Replace("SET READEULA \"0\"", "SET READEULA \"1\"", StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            content += "\nSET READEULA \"1\"";
                        }
                    }

                    if (!content.Contains("SET READTOS \"0\"", StringComparison.OrdinalIgnoreCase))
                    {
                        editedFile = true;

                        if (content.Contains("SET READTOS", StringComparison.OrdinalIgnoreCase))
                        {
                            content = content.Replace("SET READTOS \"0\"", "SET READTOS \"1\"", StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            content += "\nSET READTOS \"1\"";
                        }
                    }

                    if (!content.Contains("SET MOVIE \"0\"", StringComparison.OrdinalIgnoreCase))
                    {
                        editedFile = true;

                        if (content.Contains("SET MOVIE", StringComparison.OrdinalIgnoreCase))
                        {
                            content = content.Replace("SET MOVIE \"0\"", "SET MOVIE \"1\"", StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            content += "\nSET MOVIE \"1\"";
                        }
                    }

                    if (editedFile)
                    {
                        File.SetAttributes(configWtfPath, FileAttributes.Normal);
                        File.WriteAllText(configWtfPath, content);
                        File.SetAttributes(configWtfPath, FileAttributes.ReadOnly);
                    }
                }

                return BtStatus.Success;
            }
            catch
            {
                AmeisenLogger.I.Log("StartWow", "Cannot write to config.wtf");
            }

            return BtStatus.Failed;
        }

        private BtStatus Dead()
        {
            if (Config.ReleaseSpirit || Bot.Objects.MapId.IsBattlegroundMap())
            {
                Bot.Wow.RepopMe();

                SearchedStaticRoutes = false;
                return BtStatus.Success;
            }

            return BtStatus.Ongoing;
        }

        private BtStatus DeadDungeon()
        {
            if (!ArePartymembersInFight)
            {
                if (DungeonDiedTimestamp == default)
                {
                    DungeonDiedTimestamp = DateTime.UtcNow;
                }
                else if (DateTime.UtcNow - DungeonDiedTimestamp > TimeSpan.FromSeconds(30))
                {
                    Bot.Wow.RepopMe();
                    SearchedStaticRoutes = false;
                    return BtStatus.Success;
                }
            }

            if (!ArePartymembersInFight && DateTime.UtcNow - DungeonDiedTimestamp > TimeSpan.FromSeconds(30)
                || Bot.Objects.Partymembers.Any(e => !e.IsDead
                    && (e.Class == WowClass.Paladin || e.Class == WowClass.Druid || e.Class == WowClass.Priest || e.Class == WowClass.Shaman)))
            {
                // if we died 30s ago or no one that can ress us is alive
                Bot.Wow.RepopMe();
                SearchedStaticRoutes = false;
                return BtStatus.Success;
            }

            return BtStatus.Ongoing;
        }

        private BtStatus Eat()
        {
            if (EatEvent.Run())
            {
                bool needToEat = Bot.Player.HealthPercentage < Config.EatUntilPercent;
                bool needToDrink = Bot.Player.ManaPercentage < Config.DrinkUntilPercent;

                bool isEating = Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Food");
                bool isDrinking = Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Drink");

                if (isEating && isDrinking)
                {
                    return BtStatus.Ongoing;
                }

                IWowInventoryItem refreshment = Food.FirstOrDefault(e => Enum.IsDefined(typeof(WowRefreshment), e.Id));

                if (needToEat && needToDrink && refreshment != null)
                {
                    if (refreshment != null)
                    {
                        Bot.Wow.UseItemByName(refreshment.Name);
                        return BtStatus.Ongoing;
                    }
                }

                IWowInventoryItem food = Food.FirstOrDefault(e => Enum.IsDefined(typeof(WowFood), e.Id));

                if (!isEating && needToEat && (food != null || refreshment != null))
                {
                    // only use food if its not very lowlevel, otherwise try to use a refreshment
                    if (food != null && (refreshment == null || food.RequiredLevel >= Bot.Player.Level - 5))
                    {
                        Bot.Wow.UseItemByName(food.Name);
                        return BtStatus.Ongoing;
                    }

                    if (refreshment != null)
                    {
                        Bot.Wow.UseItemByName(refreshment.Name);
                        return BtStatus.Ongoing;
                    }
                }

                IWowInventoryItem water = Food.FirstOrDefault(e => Enum.IsDefined(typeof(WowWater), e.Id));

                if (!isDrinking && needToDrink && (water != null || refreshment != null))
                {
                    // only use water if its not very lowlevel, otherwise try to use a refreshment
                    if (water != null && (refreshment == null || water.RequiredLevel >= Bot.Player.Level - 5))
                    {
                        Bot.Wow.UseItemByName(water.Name);
                        return BtStatus.Ongoing;
                    }

                    if (refreshment != null)
                    {
                        Bot.Wow.UseItemByName(refreshment.Name);
                        return BtStatus.Ongoing;
                    }
                }
            }

            return BtStatus.Success;
        }

        private BtStatus Follow()
        {
            Vector3 pos = Config.FollowPositionDynamic ? PlayerToFollow.Position + FollowOffset : PlayerToFollow.Position;
            return MoveToPosition(pos);
        }

        private IEnumerable<IWowUnit> GetLootableUnits()
        {
            return Bot.Objects.WowObjects.OfType<IWowUnit>()
                .Where(e => e.IsLootable
                    && !UnitsLooted.Contains(e.Guid)
                    && e.Position.GetDistance(Bot.Player.Position) < Config.LootUnitsRadius);
        }

        private BtStatus Idle()
        {
            Bot.CombatClass.OutOfCombatExecute();

            return BtStatus.Success;
        }

        private bool IsBattlegroundFinished()
        {
            return Bot.Memory.Read(Bot.Wow.Offsets.BattlegroundFinished, out int bgFinished)
                && bgFinished == 1;
        }

        private bool IsRepairNpcNear(out IWowUnit unit)
        {
            unit = Bot.Objects.WowObjects.OfType<IWowUnit>()
                    .FirstOrDefault(e => e.GetType() != typeof(IWowPlayer)
                                         && !e.IsDead
                                         && e.IsRepairer
                && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Hostile
                && Bot.Player.DistanceTo(e) <= Config.RepairNpcSearchRadius);

            return unit != null;
        }

        private bool IsUnitToFollowThere(out IWowUnit playerToFollow, bool ignoreRange = false)
        {
            IEnumerable<IWowPlayer> wowPlayers = Bot.Objects.WowObjects.OfType<IWowPlayer>().Where(e => !e.IsDead);

            if (wowPlayers.Any())
            {
                IWowUnit[] playersToTry =
                {
                    Config.FollowSpecificCharacter ? wowPlayers.FirstOrDefault(p => Bot.Db.GetUnitName(p, out string name) && name.Equals(Config.SpecificCharacterToFollow, StringComparison.OrdinalIgnoreCase)) : null,
                    Config.FollowGroupLeader ? Bot.Objects.Partyleader : null,
                    Config.FollowGroupMembers ? Bot.Objects.Partymembers.FirstOrDefault() : null
                };

                for (int i = 0; i < playersToTry.Length; ++i)
                {
                    if (playersToTry[i] != null && (ignoreRange || ShouldIFollowPlayer(playersToTry[i])))
                    {
                        playerToFollow = playersToTry[i];
                        return true;
                    }
                }
            }

            playerToFollow = null;
            return false;
        }

        private bool IsVendorNpcNear(out IWowUnit unit)
        {
            unit = Bot.Objects.WowObjects.OfType<IWowUnit>()
                .FirstOrDefault(e => e.GetType() != typeof(IWowPlayer)
                    && !e.IsDead
                    && e.IsVendor
                    && Bot.Db.GetReaction(Bot.Player, e) != WowUnitReaction.Hostile
                    && e.Position.GetDistance(Bot.Player.Position) < Config.RepairNpcSearchRadius);

            return unit != null;
        }

        private void LoadWowWindowPosition()
        {
            if (Config.SaveWowWindowPosition && !Config.AutoPositionWow)
            {
                if (Bot.Memory.Process.MainWindowHandle != IntPtr.Zero && Config.WowWindowRect != new Rect() { Left = -1, Top = -1, Right = -1, Bottom = -1 })
                {
                    Bot.Memory.SetWindowPosition(Bot.Memory.Process.MainWindowHandle, Config.WowWindowRect);
                    AmeisenLogger.I.Log("AmeisenBot", $"Loaded window position: {Config.WowWindowRect}", LogLevel.Verbose);
                }
                else
                {
                    AmeisenLogger.I.Log("AmeisenBot", $"Unable to load window position of {Bot.Memory.Process.MainWindowHandle} to {Config.WowWindowRect}", LogLevel.Warning);
                }
            }
        }

        private BtStatus Login()
        {
            Bot.Wow.SetWorldLoadedCheck(true);

            // needed to prevent direct logout due to inactivity
            AntiAfk();

            if (LoginAttemptEvent.Run())
            {
                Bot.Wow.LuaDoString($"if CinematicFrame and CinematicFrame:IsShown()then StopCinematic()elseif TOSFrame and TOSFrame:IsShown()then TOSAccept:Enable()TOSAccept:Click()elseif ScriptErrors and ScriptErrors:IsShown()then ScriptErrors:Hide()elseif GlueDialog and GlueDialog:IsShown()then if GlueDialog.which=='OKAY'then GlueDialogButton1:Click()end elseif CharCreateRandomizeButton and CharCreateRandomizeButton:IsVisible()then CharacterCreate_Back()elseif RealmList and RealmList:IsVisible()then for a=1,#GetRealmCategories()do local found=false for b=1,GetNumRealms()do if string.lower(GetRealmInfo(a,b))==string.lower('{Config.Realm}')then ChangeRealm(a,b)RealmList:Hide()found=true break end end if found then break end end elseif CharacterSelectUI and CharacterSelectUI:IsVisible()then if string.find(string.lower(GetServerName()),string.lower('{Config.Realm}'))then CharacterSelect_SelectCharacter({Config.CharacterSlot + 1})CharacterSelect_EnterWorld()elseif RealmList and not RealmList:IsVisible()then CharSelectChangeRealmButton:Click()end elseif AccountLoginUI and AccountLoginUI:IsVisible()then DefaultServerLogin('{Config.Username}','{Config.Password}')end");
            }

            Bot.Wow.SetWorldLoadedCheck(false);
            return BtStatus.Success;
        }

        private BtStatus LootNearUnits()
        {
            IWowUnit unit = Bot.GetWowObjectByGuid<IWowUnit>(UnitsToLoot.Peek());

            if (unit == null || !unit.IsLootable || LootTry > 2)
            {
                UnitsLooted.Add(UnitsToLoot.Dequeue());
                LootTry = 0;
                return BtStatus.Failed;
            }

            if (unit.Position != Vector3.Zero && Bot.Player.DistanceTo(unit) > 3.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, unit.Position);
                return BtStatus.Ongoing;
            }
            else if (LootTryEvent.Run())
            {
                if (Bot.Memory.Read(Bot.Wow.Offsets.LootWindowOpen, out byte lootOpen)
                    && lootOpen > 0)
                {
                    Bot.Wow.LootEverything();

                    UnitsLooted.Add(UnitsToLoot.Dequeue());
                    LootTry = 0;

                    Bot.Wow.ClickUiElement("LootCloseButton");
                    return BtStatus.Success;
                }
                else
                {
                    Bot.Wow.StopClickToMove();
                    Bot.Wow.InteractWithUnit(unit.BaseAddress);
                    ++LootTry;
                }
            }

            return BtStatus.Ongoing;
        }

        private BtStatus MoveToPosition(Vector3 position, MovementAction movementAction = MovementAction.Move)
        {
            if (position != Vector3.Zero && Bot.Player.DistanceTo(position) > 3.0f)
            {
                Bot.Movement.SetMovementAction(movementAction, position);
                return BtStatus.Ongoing;
            }

            return BtStatus.Success;
        }

        private bool NeedToEat()
        {
            // is eating blocked, used to prevent shredding of food
            if (!EatBlockEvent.Ready)
            {
                return false;
            }

            // when we are in a group an they move too far away, abort eating
            // and dont start eating for 30s
            if (Bot.Objects.PartymemberGuids.Any() && Bot.Player.DistanceTo(Bot.Objects.CenterPartyPosition) > 25.0f)
            {
                EatBlockEvent.Run();
                return false;
            }

            if (UpdateFood.Run())
            {
                Food = Bot.Character.Inventory.Items
                    .Where(e => e.RequiredLevel <= Bot.Player.Level)
                    .OrderByDescending(e => e.ItemLevel);
            }

            return Bot.Player.HealthPercentage < Config.EatUntilPercent
                   && (Food.Any(e => Enum.IsDefined(typeof(WowFood), e.Id)) 
                       || Food.Any(e => Enum.IsDefined(typeof(WowRefreshment), e.Id)))
                || Bot.Player.MaxMana > 0 && Bot.Player.ManaPercentage < Config.DrinkUntilPercent
                   && (Food.Any(e => Enum.IsDefined(typeof(WowWater), e.Id)) 
                       || Food.Any(e => Enum.IsDefined(typeof(WowRefreshment), e.Id)));
        }

        private bool NeedToFight()
        {
            if (PartymembersFightEvent.Run())
            {
                ArePartymembersInFight = Bot.Objects.Partymembers.Any(e => e.IsInCombat && e.DistanceTo(Bot.Player) < Config.SupportRange)
                    || Bot.Objects.WowObjects.OfType<IWowUnit>().Any(e => (e.IsInCombat && (e.IsTaggedByMe || !e.IsTaggedByOther)
                        || e.TargetGuid == Bot.Player.Guid
                        || Bot.Objects.Partymembers.Any(x => x.Guid == e.TargetGuid))
                        && Bot.Wow.GetReaction(Bot.Player.BaseAddress, e.BaseAddress) == WowUnitReaction.Hostile);
            }

            return Bot.Player.IsInCombat
                || ArePartymembersInFight;
        }

        private bool NeedToFollow()
        {
            if (!Config.Autopilot && Bot.Objects.WowObjects != null && IsUnitToFollowThere(out IWowUnit player))
            {
                PlayerToFollow = player;
                float distance = Bot.Player.DistanceTo(player);
                return distance > Config.MinFollowDistance && distance <= Config.MaxFollowDistance;
            }

            return false;
        }

        private bool NeedToLogin()
        {
            return Bot.Memory.Read(Bot.Wow.Offsets.IsIngame, out int isIngame) && isIngame == 0;
        }

        private bool NeedToLoot()
        {
            IEnumerable<IWowUnit> units = GetLootableUnits();

            if (units.Any())
            {
                foreach (IWowUnit unit in units)
                {
                    if (!UnitsLooted.Contains(unit.Guid) && !UnitsToLoot.Contains(unit.Guid))
                    {
                        UnitsToLoot.Enqueue(unit.Guid);
                    }
                }
            }

            return UnitsToLoot.Count > 0;
        }

        private bool NeedToTrainSpells()
        {
            IWowUnit classTrainer = null;
            Npc profileTrainer = null;

            if (Bot.Grinding.Profile != null)
                profileTrainer = Bot.Grinding.Profile.NpcsOfInterest.FirstOrDefault(e =>
                    e.Type == NpcType.ClassTrainer && e.SubType == DecideClassTrainer(Bot.Player.Class));

            if (profileTrainer != null)
                classTrainer = Bot.GetClosestTrainerByEntryId(profileTrainer.EntryId);

            if (classTrainer == null) 
                return false;

            ClassTrainer = classTrainer;
            return Bot.Character.LastLevelTrained != 0 && Bot.Character.LastLevelTrained < Bot.Player.Level;
        }

        public static NpcSubType DecideClassTrainer(WowClass myClass)
        {
            switch (myClass)
            {
                case WowClass.Warrior:
                    return NpcSubType.WarriorTrainer;
                case WowClass.Paladin:
                    return NpcSubType.PaladinTrainer;
                case WowClass.Hunter:
                    return NpcSubType.HunterTrainer;
                case WowClass.Rogue:
                    return NpcSubType.RougeTrainer;
                case WowClass.Priest:
                    return NpcSubType.PriestTrainer;
                case WowClass.Deathknight:
                    return NpcSubType.DeathKnightTrainer;
                case WowClass.Shaman:
                    return NpcSubType.ShamanTrainer;
                case WowClass.Mage:
                    return NpcSubType.MageTrainer;
                case WowClass.Warlock:
                    return NpcSubType.WarlockTrainer;
                case WowClass.Druid:
                    return NpcSubType.DruidTrainer;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private BtStatus SpeakWithClassTrainer()
        {
            if (ClassTrainer == null) 
                return BtStatus.Failed;

            if (Bot.Player.Position.GetDistance(ClassTrainer.Position) > 3.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, ClassTrainer.Position);
                return BtStatus.Success;
            }

            Bot.Movement.StopMovement();

            if (!NpcInteractionEvent.Run()) 
                return BtStatus.Failed;

            SpeakToClassTrainerRoutine.Run(Bot, ClassTrainer);
            return BtStatus.Success;
        }

        private bool NeedToRepairOrSell()
        {
            bool needToRepair = Bot.Character.Equipment.Items.Any(e =>
                e.Value.MaxDurability > 0 && e.Value.Durability / (double)e.Value.MaxDurability * 100.0 <= Config.ItemRepairThreshold);

            bool needToSell = Bot.Character.Inventory.FreeBagSlots < Config.BagSlotsToGoSell
                              && Bot.Character.Inventory.Items
                              .Any(e => e.Price > 0 && !Config.ItemSellBlacklist.Contains(e.Name)
                                      && (Config.SellGrayItems && e.ItemQuality == (int)WowItemQuality.Poor 
                                      || Config.SellWhiteItems && e.ItemQuality == (int)WowItemQuality.Common
                                      || Config.SellGreenItems && e.ItemQuality == (int)WowItemQuality.Uncommon
                                      || Config.SellBlueItems && e.ItemQuality == (int)WowItemQuality.Rare 
                                      || Config.SellPurpleItems && e.ItemQuality == (int)WowItemQuality.Epic));
            
            IWowUnit vendorRepair = null;
            IWowUnit vendorSell = null;

            if (Mode != BotMode.None && (Bot.Grinding.Profile == null || !Bot.Grinding.Profile.NpcsOfInterest.Any()))
                return false;

            switch (Mode)
            {
                case BotMode.Grinding:
                {
                    Npc repairNpcEntry = Bot.Grinding.Profile.NpcsOfInterest.FirstOrDefault(e => e.Type == NpcType.VendorRepair);
                    if (repairNpcEntry != null)
                        vendorRepair = Bot.GetClosestVendorByEntryId(repairNpcEntry.EntryId); 

                    Npc sellNpcEntry = Bot.Grinding.Profile.NpcsOfInterest.FirstOrDefault(e => e.Type == NpcType.VendorRepair || e.Type == NpcType.VendorSellBuy);
                    if (sellNpcEntry != null)
                        vendorSell = Bot.GetClosestVendorByEntryId(sellNpcEntry.EntryId);

                    break;
                }
                case BotMode.None:
                    IsRepairNpcNear(out IWowUnit repairNpc);
                    vendorRepair = repairNpc;

                    IsVendorNpcNear(out IWowUnit sellNpc);
                    vendorSell = sellNpc;
                    break;
                case BotMode.Questing:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (needToRepair && vendorRepair != null)
            {
                Merchant = vendorRepair;
                return true;
            }
            if (needToSell && vendorSell != null)
            {
                Merchant = vendorSell;
                return true;
            }

            return false;
        }

        private BtStatus RunToCorpseAndRetrieveIt()
        {
            if (Bot.Memory.Read(Bot.Wow.Offsets.CorpsePosition, out Vector3 corpsePosition))
            {
                if (Bot.Player.Position.GetDistance(corpsePosition) > Config.GhostResurrectThreshold)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, corpsePosition);
                    return BtStatus.Ongoing;
                }
                else
                {
                    Bot.Wow.RetrieveCorpse();
                    return BtStatus.Success;
                }
            }

            return BtStatus.Failed;
        }

        private void SetUlowGfxSettings()
        {
            Bot.Wow.LuaDoString("SetCVar(\"gxcolorbits\",\"16\");SetCVar(\"gxdepthbits\",\"16\");SetCVar(\"skycloudlod\",\"0\");SetCVar(\"particledensity\",\"0.3\");SetCVar(\"lod\",\"0\");SetCVar(\"mapshadows\",\"0\");SetCVar(\"maxlights\",\"0\");SetCVar(\"specular\",\"0\");SetCVar(\"waterlod\",\"0\");SetCVar(\"basemip\",\"1\");SetCVar(\"shadowlevel\",\"1\")");
        }

        private BtStatus SetupWowInterface()
        {
            return Bot.Wow.Setup() ? BtStatus.Success : BtStatus.Failed;
        }

        private bool ShouldIFollowPlayer(IWowUnit playerToFollow)
        {
            if (playerToFollow != null)
            {
                Vector3 pos = Config.FollowPositionDynamic ? playerToFollow.Position + FollowOffset : playerToFollow.Position;
                double distance = Bot.Player.DistanceTo(pos);

                if (distance > Config.MinFollowDistance && distance < Config.MaxFollowDistance)
                {
                    return true;
                }
            }

            return false;
        }

        private BtStatus SpeakWithMerchant()
        {
            if (Merchant != null)
            {
                float distance = Bot.Player.Position.GetDistance(Merchant.Position);

                if (distance > 3.0f)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, Merchant.Position);
                    return BtStatus.Success;
                }
                else
                {
                    Bot.Movement.StopMovement();

                    if (NpcInteractionEvent.Run())
                    {
                        SpeakToMerchantRoutine.Run(Bot, Merchant);
                    }

                    return BtStatus.Success;
                }
            }

            return BtStatus.Failed;
        }

        private BtStatus StartWow()
        {
            if (File.Exists(Config.PathToWowExe))
            {
                AmeisenLogger.I.Log("StartWow", "Starting WoW Process");
                Process p = Bot.Memory.StartProcessNoActivate($"\"{Config.PathToWowExe}\" -windowed -d3d9", out IntPtr processHandle, out IntPtr mainThreadHandle);
                p.WaitForInputIdle();

                AmeisenLogger.I.Log("StartWow", $"Attaching XMemory to {p.ProcessName} ({p.Id})");

                if (Bot.Memory.Init(p, processHandle, mainThreadHandle))
                {
                    OnWoWStarted?.Invoke();

                    if (Config.SaveWowWindowPosition)
                    {
                        LoadWowWindowPosition();
                    }

                    return BtStatus.Success;
                }
                else
                {
                    AmeisenLogger.I.Log("StartWow", $"Attaching XMemory failed...");
                    p.Kill();
                    return BtStatus.Failed;
                }
            }

            return BtStatus.Failed;
        }

        private BtStatus UpdateIngame()
        {
            if (FirstStart)
            {
                FirstStart = false;

                Bot.Wow.Events.Start();

                Bot.Wow.LuaDoString($"SetCVar(\"maxfps\", {Config.MaxFps});SetCVar(\"maxfpsbk\", {Config.MaxFps})");
                Bot.Wow.EnableClickToMove();

                if (Config.AutoSetUlowGfxSettings)
                {
                    SetUlowGfxSettings();
                }
            }

            Bot.Wow.Events.Tick();

            Bot.Movement.Execute();

            if (CharacterUpdateEvent.Run())
            {
                Bot.Character.UpdateAll();
            }

            if (!Bot.Player.IsDead)
            {
                DungeonDiedTimestamp = default;
            }

            if (Config.FollowPositionDynamic && OffsetCheckEvent.Run())
            {
                float factor = Bot.Player.IsOutdoors ? 2.0f : 1.0f;

                FollowOffset = new()
                {
                    X = ((float)Random.NextDouble() * ((float)Config.MinFollowDistance * factor) - (float)Config.MinFollowDistance * (0.5f * factor)) * 0.7071f,
                    Y = ((float)Random.NextDouble() * ((float)Config.MinFollowDistance * factor) - (float)Config.MinFollowDistance * (0.5f * factor)) * 0.7071f,
                    Z = 0.0f
                };
            }

            // auto disable rendering when not in focus
            if (Config.AutoDisableRender && RenderSwitchEvent.Run())
            {
                IntPtr foregroundWindow = Bot.Memory.GetForegroundWindow();
                Bot.Wow.SetRenderState(foregroundWindow == Bot.Memory.Process.MainWindowHandle);
            }

            return BtStatus.Success;
        }

        private BtStatus UpdateWowInterface()
        {
            Bot.Wow.Tick();
            return BtStatus.Success;
        }
    }
}