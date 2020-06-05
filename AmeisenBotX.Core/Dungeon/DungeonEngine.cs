using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Dungeon.Profiles.Classic;
using AmeisenBotX.Core.Dungeon.Profiles.TBC;
using AmeisenBotX.Core.Dungeon.Profiles.WotLK;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Statemachine;
using AmeisenBotX.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Dungeon
{
    public class DungeonEngine
    {
        public DungeonEngine(WowInterface wowInterface, AmeisenBotStateMachine stateMachine)
        {
            WowInterface = wowInterface;
            StateMachine = stateMachine;

            CurrentNodes = new ConcurrentQueue<DungeonNode>();
            CompletedNodes = new List<DungeonNode>();

            ExitDungeonEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));

            Reset();
        }

        public bool AllPlayersArrived { get; private set; }

        public ConcurrentQueue<DungeonNode> CurrentNodes { get; private set; }

        public bool DidAllDie { get; private set; }

        public IDungeonProfile DungeonProfile { get; private set; }

        public bool Entered { get; private set; }

        public DateTime EntryTime { get; private set; }

        public bool HasFinishedDungeon => Progress == 100.0 || CurrentNodes.IsEmpty;

        public bool IgnoreEatDrink { get; private set; }

        public List<DungeonNode> Nodes => CurrentNodes?.ToList();

        public double Progress { get; private set; }

        public int TotalNodes { get; private set; }

        public bool Waiting { get; private set; }

        public DateTime WaitingSince { get; private set; }

        private int AllPlayerPresentDistance { get; set; }

        private List<DungeonNode> CompletedNodes { get; set; }

        private TimegatedEvent ExitDungeonEvent { get; set; }

        private AmeisenBotStateMachine StateMachine { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (!Entered)
            {
                EntryTime = DateTime.Now;
                Entered = true;
            }

            if (DungeonProfile != null)
            {
                if (!HasFinishedDungeon)
                {
                    if (CurrentNodes.Count() == 0)
                    {
                        LoadNodes();
                    }
                    else
                    {
                        bool isMePartyleader = WowInterface.ObjectManager.Player.Guid == WowInterface.ObjectManager.PartyleaderGuid || WowInterface.ObjectManager.PartyleaderGuid == 0;

                        if (isMePartyleader)
                        {
                            // wait for all players to arrive
                            if (AreAllPlayersPresent())
                            {
                                AllPlayerPresentDistance = 48;

                                if (!ShouldWaitForGroup()) // ShouldWaitForGroup()
                                {
                                    FollowNodePath(WowInterface.MovementSettings.WaypointCheckThreshold);
                                }
                            }
                            else
                            {
                                // wait until the players are near us
                                AllPlayerPresentDistance = 16;
                            }
                        }
                        else
                        {
                            if (!MoveToGroupLeader())
                            {
                                // wait for the group leader
                            }
                        }
                    }
                }
                else
                {
                    if (ExitDungeonEvent.Run())
                    {
                        // find a way to exit the dungeon, maybe hearthstone
                        if (WowInterface.HookManager.IsInLfgGroup())
                        {
                            WowInterface.HookManager.LuaDoString("LFGTeleport(true);");
                        }
                    }
                }
            }
            else
            {
                LoadProfile(TryLoadProfile());
            }
        }

        public void LoadProfile(IDungeonProfile profile)
        {
            if (!WowInterface.ObjectManager.IsWorldLoaded || DateTime.Now - EntryTime < TimeSpan.FromSeconds(3))
            {
                return;
            }

            Reset();

            DungeonProfile = profile;
            LoadNodes();

            WowInterface.CombatClass.PriorityTargets = profile.PriorityUnits;
            TotalNodes = CurrentNodes.Count;
        }

        public void OnDeath()
        {
            DidAllDie = WowInterface.ObjectManager.Partymembers.Any(e => !e.IsDead);
        }

        public void Reset()
        {
            Entered = false;
            AllPlayersArrived = false;
            DidAllDie = false;

            DungeonProfile = null;

            CurrentNodes = new ConcurrentQueue<DungeonNode>();
            CompletedNodes.Clear();

            Progress = 0.0;
            TotalNodes = 0;

            AllPlayerPresentDistance = 48;
        }

        private bool AreAllPlayersPresent()
        {
            return WowInterface.ObjectManager.GetNearPartymembers(WowInterface.ObjectManager.Player.Position, AllPlayerPresentDistance)
                   .Count(e => !e.IsDead) >= WowInterface.ObjectManager.Partymembers.Count;
        }

        private void FilterOutAlreadyCompletedNodes()
        {
            AmeisenLogger.Instance.Log("Dungeon", "FilterOutAlreadyCompletedNodes called...");

            DungeonNode closestDungeonNode = DungeonProfile.Path.OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).FirstOrDefault();
            bool shouldAddNodes = closestDungeonNode == null;

            foreach (DungeonNode d in DungeonProfile.Path)
            {
                // skip all already completed nodes
                if (!shouldAddNodes)
                {
                    if (d == closestDungeonNode)
                    {
                        shouldAddNodes = true;
                    }
                }
                else
                {
                    CurrentNodes.Enqueue(d);
                }
            }
        }

        private void FollowNodePath(double completionDistance)
        {
            if (CurrentNodes.TryPeek(out DungeonNode node))
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(node.Position) > completionDistance)
                {
                    WowInterface.MovementEngine.SetState(MovementEngineState.Moving, node.Position);
                    WowInterface.MovementEngine.Execute();
                }
                else
                {
                    DungeonNode dungeonNode = node;

                    if (dungeonNode.Type == Enums.DungeonNodeType.Door
                        || dungeonNode.Type == Enums.DungeonNodeType.Collect
                        || dungeonNode.Type == Enums.DungeonNodeType.Use)
                    {
                        WowGameobject obj = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                            .OrderBy(e => e.Position.GetDistance(dungeonNode.Position))
                            .FirstOrDefault();

                        if (obj != null && obj.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < completionDistance)
                        {
                            WowInterface.HookManager.WowObjectOnRightClick(obj);
                        }
                    }

                    if (CurrentNodes.TryDequeue(out DungeonNode completedNode))
                    {
                        CompletedNodes.Add(completedNode);
                        Progress = Math.Round(CompletedNodes.Count / (double)TotalNodes * 100.0);
                    }
                }
            }
        }

        private void LoadNodes()
        {
            CurrentNodes = new ConcurrentQueue<DungeonNode>();
            FilterOutAlreadyCompletedNodes();
        }

        private bool MoveToGroupLeader()
        {
            WowUnit partyLeader = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(WowInterface.ObjectManager.PartyleaderGuid);

            double distance = partyLeader != null ? partyLeader.Position.GetDistance(WowInterface.ObjectManager.Player.Position) : 0;

            if (distance > 0 && distance < 8)
            {
                return false;
            }
            else
            {
                if (distance > 0 && distance < 48)
                {
                    WowInterface.MovementEngine.SetState(MovementEngineState.Moving, partyLeader.Position);
                    WowInterface.MovementEngine.Execute();
                    return true;
                }
                else
                {
                    if (partyLeader != null)
                    {
                        DungeonNode closestDungeonNodeLeader = DungeonProfile.Path.OrderBy(e => e.Position.GetDistance(partyLeader.Position)).FirstOrDefault();
                        int nodeIndex = DungeonProfile.Path.IndexOf(closestDungeonNodeLeader);
                        int completedNodes = CompletedNodes.Count();

                        if (nodeIndex <= completedNodes)
                        {
                            return false;
                        }
                    }

                    if (CurrentNodes?.Count == 0)
                    {
                        LoadNodes();
                    }

                    FollowNodePath(WowInterface.MovementSettings.WaypointCheckThreshold);
                    return true;
                }
            }
        }

        private bool ShouldWaitForGroup()
        {
            if (!IgnoreEatDrink
                && CurrentNodes.TryPeek(out DungeonNode node))
            {
                // we need to be prepared for the bossfight
                double minPercentages = node.Type == Enums.DungeonNodeType.Boss ? 100.0 : 75.0;

                // wait for guys to start eating
                if (DateTime.Now - WaitingSince > TimeSpan.FromSeconds(3)
                    && !WowInterface.ObjectManager.Partymembers.Any(e => e.HasBuffByName("Food") || e.HasBuffByName("Drink")))
                {
                    IgnoreEatDrink = true;
                }

                // do we need to wait for some members to regen life or mana
                if (WowInterface.ObjectManager.Partymembers.OfType<WowUnit>().Any(e => e.HealthPercentage < minPercentages))
                {
                    Waiting = true;
                    WaitingSince = DateTime.Now;
                    return true;
                }
            }

            // are my group members not in range of the CurrentNode
            List<WowPlayer> nearUnits = WowInterface.ObjectManager.GetNearPartymembers(WowInterface.ObjectManager.Player.Position, 30).ToList();
            if (nearUnits.Count() < WowInterface.ObjectManager.PartymemberGuids.Count - 1)
            {
                Waiting = true;
                WaitingSince = DateTime.Now;
                return true;
            }

            // go ahead
            Waiting = false;
            return false;
        }

        private IDungeonProfile TryLoadProfile()
        {
            return WowInterface.ObjectManager.MapId switch
            {
                MapId.Deadmines => new DeadminesProfile(),
                MapId.HellfireRamparts => new HellfireRampartsProfile(),
                MapId.TheBloodFurnace => new TheBloodFurnaceProfile(),
                MapId.TheSlavePens => new TheSlavePensProfile(),
                MapId.TheUnderbog => new TheUnderbogProfile(),
                MapId.TheSteamvault => new TheSteamvaultProfile(),
                MapId.UtgardeKeep => new UtgardeKeepProfile(),
                MapId.AzjolNerub => new AzjolNerubProfile(),
                _ => null
            };
        }
    }
}