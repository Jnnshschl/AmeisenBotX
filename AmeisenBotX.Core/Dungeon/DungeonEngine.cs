using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Dungeon.Profiles.Classic;
using AmeisenBotX.Core.Dungeon.Profiles.TBC;
using AmeisenBotX.Core.Dungeon.Profiles.WotLK;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Statemachine;
using System;
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

            CurrentNodes = new Queue<DungeonNode>();
            CompletedNodes = new List<DungeonNode>();

            Reset();
        }

        public bool AllPlayersArrived { get; private set; }

        public Queue<DungeonNode> CurrentNodes { get; private set; }

        public IDungeonProfile DungeonProfile { get; private set; }

        public bool Entered { get; private set; }

        public DateTime EntryTime { get; private set; }

        public bool HasFinishedDungeon { get; private set; }

        public bool IgnoreEatDrink { get; private set; }

        public double Progress { get; private set; }

        public int TotalNodes { get; private set; }

        public bool Waiting { get; private set; }

        public DateTime WaitingSince { get; private set; }

        private List<DungeonNode> CompletedNodes { get; set; }

        private AmeisenBotStateMachine StateMachine { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (!Entered)
            {
                EntryTime = DateTime.Now;
                Entered = true;
            }

            if (!HasFinishedDungeon && DungeonProfile != null && CurrentNodes.Count > 0)
            {
                // we are fighting
                if ((WowInterface.ObjectManager.Player.IsInCombat && StateMachine.IsAnyPartymemberInCombat())
                    || WowInterface.ObjectManager.Player.IsCasting)
                {
                    return;
                }

                if (Progress == 100.0)
                {
                    // exit dungeon
                    HasFinishedDungeon = true;
                    return;
                }

                // wait for all players to arrive
                if (!AllPlayersArrived)
                {
                    AllPlayersArrived = AreAllPlayersPresent();
                }
                else
                {
                    bool isMePartyleader = WowInterface.ObjectManager.Player.Guid == WowInterface.ObjectManager.PartyleaderGuid || WowInterface.ObjectManager.PartyleaderGuid == 0;

                    if (isMePartyleader)
                    {
                        if (!ShouldWaitForGroup())
                        {
                            if (WowInterface.ObjectManager.Player.Position.GetDistance(CurrentNodes.Peek().Position) > 5)
                            {
                                WowInterface.MovementEngine.SetState(MovementEngineState.Moving, CurrentNodes.Peek().Position);
                                WowInterface.MovementEngine.Execute();
                                return;
                            }
                            else
                            {
                                FollowNodePath(5);
                            }
                        }
                    }
                    else
                    {
                        NeedToMoveToGroupLeader();
                    }
                }
            }
            else if (!HasFinishedDungeon && DungeonProfile == null && CurrentNodes.Count == 0)
            {
                LoadProfile(TryLoadProfile());
            }
            else
            {
                // find a way to exit the dungeon, maybe hearthstone
                WowInterface.HookManager.LuaDoString("LFGTeleport(true);");
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

        public void Reset()
        {
            Entered = false;
            HasFinishedDungeon = false;
            AllPlayersArrived = false;
            DungeonProfile = null;
            CurrentNodes.Clear();
            Progress = 0.0;
            TotalNodes = 0;
        }

        private bool AreAllPlayersPresent()
            => WowInterface.ObjectManager.GetNearPartymembers(WowInterface.ObjectManager.Player.Position, 50)
            .Count() >= WowInterface.ObjectManager.Partymembers.Count;

        private void FollowNodePath(int completionDistance)
        {
            if (WowInterface.ObjectManager.Player.Position.GetDistance(CurrentNodes.Peek().Position) > completionDistance)
            {
                WowInterface.MovementEngine.SetState(MovementEngineState.Moving, CurrentNodes.Peek().Position);
                WowInterface.MovementEngine.Execute();
            }
            else
            {
                DungeonNode dungeonNode = CurrentNodes.Peek();

                if (dungeonNode.Type == Enums.DungeonNodeType.Door
                    || dungeonNode.Type == Enums.DungeonNodeType.Collect
                    || dungeonNode.Type == Enums.DungeonNodeType.Use)
                {
                    WowGameobject obj = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                        .OrderBy(e => e.Position.GetDistance(dungeonNode.Position))
                        .FirstOrDefault();

                    if (obj != null && obj.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < completionDistance)
                    {
                        WowInterface.HookManager.GameobjectOnRightClick(obj);
                    }
                }

                CompletedNodes.Add(CurrentNodes.Dequeue());
            }
        }

        private void LoadNodes()
        {
            CurrentNodes.Clear();

            // filter out already checked nodes
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

        private bool NeedToMoveToGroupLeader()
        {
            WowUnit partyLeader = WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(WowInterface.ObjectManager.PartyleaderGuid);

            if (partyLeader != null && partyLeader.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 64)
            {
                WowInterface.MovementEngine.SetState(MovementEngineState.Moving, partyLeader.Position);
                WowInterface.MovementEngine.Execute();

                return true;
            }

            DungeonNode closestDungeonNode = CurrentNodes.OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).FirstOrDefault();

            if (closestDungeonNode.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 20)
            {
                LoadNodes();
            }
            else
            {
                FollowNodePath(12);
                return true;
            }

            return false;
        }

        private bool ShouldWaitForGroup()
        {
            if (!IgnoreEatDrink)
            {
                // we need to be prepared for the bossfight
                double minPercentages = CurrentNodes.Peek().Type == Enums.DungeonNodeType.Boss ? 100.0 : 75.0;

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
            => WowInterface.ObjectManager.MapId switch
            {
                MapId.Deadmines => new DeadminesProfile(),
                MapId.HellfireRamparts => new HellfireRampartsProfile(),
                MapId.UtgardeKeep => new UtgardeKeepProfile(),
                MapId.AzjolNerub => new AzjolNerubProfile(),
                _ => null
            };
    }
}