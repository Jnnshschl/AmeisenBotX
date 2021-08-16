using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Logic.StaticDeathRoutes;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Memory.Win32;
using AmeisenBotX.Wow.Objects;
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

            LoginAttemptEvent = new(TimeSpan.FromMilliseconds(500));
            AntiAfkEvent = new(TimeSpan.FromMilliseconds(1200));
            RenderSwitchEvent = new(TimeSpan.FromMilliseconds(1000));
            CharacterUpdateEvent = new(TimeSpan.FromMilliseconds(5000));
            OffsetCheckEvent = new(TimeSpan.FromMilliseconds(15000));

            Node ghostNode = new Selector
            (
                () => Bot.Objects.MapId.IsBattlegroundMap(),
                // on a battleground, wait for the mass ress
                new Leaf(() => { Bot.Movement.StopMovement(); return BehaviorTreeStatus.Ongoing; }),
                new Selector
                (
                    () => CanUseStaticPaths(),
                    // prefer static paths
                    new Leaf(FollowStaticPath),
                    new Selector
                    (
                        // TODO: handle instances here
                        () => false,
                        // find the instance entrance and go there, corpse wont be
                        // reachable if we die in a dungeon for example
                        null,
                        // run to corpse by position
                        new Leaf(RunToCorpseAndRetrieveIt)
                    )
                )
            );

            Node combatNode = new Selector
            (
                () => Bot.CombatClass == null,
                // start autoattacking if we have no combat class loaded
                new Leaf(() => { if (!Bot.Player.IsAutoAttacking) Bot.Wow.StartAutoAttack(); return BehaviorTreeStatus.Success; }),
                // TODO: handle tactics here
                // run combat class logic
                new Leaf(() => { Bot.CombatClass.Execute(); return BehaviorTreeStatus.Success; })
            );

            Node mainLogicNode = new Annotator
            (
                // run the update stuff before we execute the main logic
                // objects will be updated here for example
                new Leaf(UpdateStuff),
                new Selector
                (
                    () => Bot.Objects.IsWorldLoaded && Bot.Player != null,
                    new Waterfall
                    (
                        // do idle stuff as fallback
                        new Leaf(Idle),
                        // handle main states
                        (() => Bot.Player.IsDead, new Leaf(Dead)),
                        (() => Bot.Player.IsGhost, ghostNode),
                        (IsInCombat, combatNode),
                        (NeedToFollow, new Leaf(Follow))
                    ),
                    // we are in the loading screen or player is null
                    new Leaf(() => BehaviorTreeStatus.Success)
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
                    (() => NeedToLogin, new Leaf(Login))
                )
            );
        }

        public event Action OnWoWStarted;

        private TimegatedEvent AntiAfkEvent { get; set; }

        private AmeisenBotInterfaces Bot { get; }

        private TimegatedEvent CharacterUpdateEvent { get; set; }

        private AmeisenBotConfig Config { get; }

        private Vector3 FollowOffset { get; set; }

        private TimegatedEvent LoginAttemptEvent { get; set; }

        private bool NeedToLogin => Bot.Memory.Read(Bot.Wow.Offsets.IsIngame, out int isIngame) && isIngame == 0;

        private TimegatedEvent OffsetCheckEvent { get; }

        private ulong PlayerToFollowGuid { get; set; }

        private TimegatedEvent RenderSwitchEvent { get; set; }

        private bool SearchedStaticRoutes { get; set; }

        private IStaticDeathRoute StaticRoute { get; set; }

        private AmeisenBotBehaviorTree Tree { get; }

        public bool NeedToFollow()
        {
            if (PlayerToFollowGuid != 0)
            {
                IWowUnit unitToFollow = Bot.GetWowObjectByGuid<IWowUnit>(PlayerToFollowGuid);

                if (unitToFollow != null)
                {
                    float distance = Bot.Player.DistanceTo(unitToFollow);
                    return distance > Config.MinFollowDistance && distance <= Config.MaxFollowDistance;
                }
            }

            return false;
        }

        public bool IsInCombat()
        {
            return Bot.Player.IsInCombat
                || Bot.Objects.WowObjects.OfType<IWowPlayer>()
                       .Where(e => e.IsInCombat && Bot.Objects.PartymemberGuids.Contains(e.Guid) && e.DistanceTo(Bot.Player) < Config.SupportRange)
                       .Any()
                || Bot.GetEnemiesInCombatWithParty<IWowUnit>(Bot.Player.Position, 100.0f).Any();
        }

        public bool IsUnitToFollowThere(out IWowUnit playerToFollow, bool ignoreRange = false)
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

        public void Tick()
        {
            Tree.Tick();
        }

        private BehaviorTreeStatus AntiAfk()
        {
            if (AntiAfkEvent.Run())
            {
                Bot.Memory.Write(Bot.Wow.Offsets.TickCount, Environment.TickCount);
            }

            return BehaviorTreeStatus.Success;
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

        private BehaviorTreeStatus ChangeRealmlist()
        {
            if (!Config.AutoChangeRealmlist)
            {
                return BehaviorTreeStatus.Success;
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

                return BehaviorTreeStatus.Success;
            }
            catch
            {
                AmeisenLogger.I.Log("StartWow", "Cannot write realmlist to config.wtf");
            }

            return BehaviorTreeStatus.Failed;
        }

        private BehaviorTreeStatus CheckTosAndEula()
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

                return BehaviorTreeStatus.Success;
            }
            catch
            {
                AmeisenLogger.I.Log("StartWow", "Cannot write to config.wtf");
            }

            return BehaviorTreeStatus.Failed;
        }

        private BehaviorTreeStatus Dead()
        {
            if (Config.ReleaseSpirit || Bot.Objects.MapId.IsBattlegroundMap())
            {
                Bot.Wow.RepopMe();
            }

            SearchedStaticRoutes = false;
            return BehaviorTreeStatus.Success;
        }

        private BehaviorTreeStatus FollowStaticPath()
        {
            Vector3 nextPosition = StaticRoute.GetNextPoint(Bot.Player.Position);

            if (nextPosition != Vector3.Zero)
            {
                Bot.Movement.SetMovementAction(MovementAction.DirectMove, nextPosition);
                return BehaviorTreeStatus.Ongoing;
            }

            return BehaviorTreeStatus.Success;
        }

        private BehaviorTreeStatus Idle()
        {
            Bot.CombatClass.OutOfCombatExecute();

            if (CharacterUpdateEvent.Run())
            {
                Bot.Character.UpdateAll();
            }

            return BehaviorTreeStatus.Success;
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

        private BehaviorTreeStatus Login()
        {
            Bot.Wow.SetWorldLoadedCheck(true);

            // needed to prevent direct logout due to inactivity
            AntiAfk();

            if (LoginAttemptEvent.Run())
            {
                Bot.Wow.LuaDoString($"if CinematicFrame and CinematicFrame:IsShown()then StopCinematic()elseif TOSFrame and TOSFrame:IsShown()then TOSAccept:Enable()TOSAccept:Click()elseif ScriptErrors and ScriptErrors:IsShown()then ScriptErrors:Hide()elseif GlueDialog and GlueDialog:IsShown()then if GlueDialog.which=='OKAY'then GlueDialogButton1:Click()end elseif CharCreateRandomizeButton and CharCreateRandomizeButton:IsVisible()then CharacterCreate_Back()elseif RealmList and RealmList:IsVisible()then for a=1,#GetRealmCategories()do local found=false for b=1,GetNumRealms()do if string.lower(GetRealmInfo(a,b))==string.lower('{Config.Realm}')then ChangeRealm(a,b)RealmList:Hide()found=true break end end if found then break end end elseif CharacterSelectUI and CharacterSelectUI:IsVisible()then if string.find(string.lower(GetServerName()),string.lower('{Config.Realm}'))then CharacterSelect_SelectCharacter({Config.CharacterSlot + 1})CharacterSelect_EnterWorld()elseif RealmList and not RealmList:IsVisible()then CharSelectChangeRealmButton:Click()end elseif AccountLoginUI and AccountLoginUI:IsVisible()then DefaultServerLogin('{Config.Username}','{Config.Password}')end");
            }

            Bot.Wow.SetWorldLoadedCheck(false);
            return BehaviorTreeStatus.Success;
        }

        private BehaviorTreeStatus Follow()
        {
            IWowUnit unitToFollow = Bot.GetWowObjectByGuid<IWowUnit>(PlayerToFollowGuid);

            if (unitToFollow != null)
            {
                Vector3 pos = unitToFollow.Position;

                if (Config.FollowPositionDynamic)
                {
                    pos += FollowOffset;
                }

                Bot.Movement.SetMovementAction(MovementAction.Move, pos);
                return BehaviorTreeStatus.Success;
            }

            return BehaviorTreeStatus.Failed;
        }

        private BehaviorTreeStatus RunToCorpseAndRetrieveIt()
        {
            if (Bot.Memory.Read(Bot.Wow.Offsets.CorpsePosition, out Vector3 corpsePosition))
            {
                if (Bot.Player.Position.GetDistance(corpsePosition) > Config.GhostResurrectThreshold)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, corpsePosition);
                    return BehaviorTreeStatus.Ongoing;
                }
                else
                {
                    Bot.Wow.RetrieveCorpse();
                    return BehaviorTreeStatus.Success;
                }
            }

            return BehaviorTreeStatus.Failed;
        }

        private BehaviorTreeStatus SetupWowInterface()
        {
            return Bot.Wow.Setup() ? BehaviorTreeStatus.Success : BehaviorTreeStatus.Failed;
        }

        private bool ShouldIFollowPlayer(IWowUnit playerToFollow)
        {
            if (playerToFollow != null)
            {
                Vector3 pos = playerToFollow.Position;

                if (Config.FollowPositionDynamic)
                {
                    pos += FollowOffset;
                }

                double distance = pos.GetDistance(Bot.Player.Position);

                if (distance > Config.MinFollowDistance && distance < Config.MaxFollowDistance)
                {
                    return true;
                }
            }

            return false;
        }

        private BehaviorTreeStatus StartWow()
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

                    return BehaviorTreeStatus.Success;
                }
                else
                {
                    AmeisenLogger.I.Log("StartWow", $"Attaching XMemory failed...");
                    p.Kill();
                    return BehaviorTreeStatus.Failed;
                }
            }

            return BehaviorTreeStatus.Failed;
        }

        private BehaviorTreeStatus UpdateStuff()
        {
            Bot.Wow.Tick();
            Bot.Wow.Events.Tick();

            if (Bot.Player != null)
            {
                Bot.Movement.Execute();
            }

            if (Bot.Objects.WowObjects != null && IsUnitToFollowThere(out IWowUnit player))
            {
                PlayerToFollowGuid = player.Guid;
            }

            if (Config.FollowPositionDynamic && OffsetCheckEvent.Run())
            {
                Random rnd = new();
                FollowOffset = new()
                {
                    X = ((float)rnd.NextDouble() * ((float)Config.MinFollowDistance * 2.0f)) - (float)Config.MinFollowDistance,
                    Y = ((float)rnd.NextDouble() * ((float)Config.MinFollowDistance * 2.0f)) - (float)Config.MinFollowDistance,
                    Z = 0.0f
                };
            }

            // auto disable rendering when not in focus
            if (Config.AutoDisableRender && RenderSwitchEvent.Run())
            {
                IntPtr foregroundWindow = Bot.Memory.GetForegroundWindow();
                Bot.Wow.SetRenderState(foregroundWindow == Bot.Memory.Process.MainWindowHandle);
            }

            return BehaviorTreeStatus.Success;
        }
    }
}