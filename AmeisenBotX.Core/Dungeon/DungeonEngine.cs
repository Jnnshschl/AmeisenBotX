using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Dungeon.Profiles.Classic;
using AmeisenBotX.Core.Dungeon.Profiles.TBC;
using AmeisenBotX.Core.Dungeon.Profiles.WotLK;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine;
using AmeisenBotX.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Dungeon
{
    public class DungeonEngine: IDungeonEngine
    {
        public DungeonEngine(WowInterface wowInterface, AmeisenBotStateMachine stateMachine)
        {
            WowInterface = wowInterface;
            StateMachine = stateMachine;

            CurrentNodes = new Queue<DungeonNode>();
            CompletedNodes = new List<DungeonNode>();

            ExitDungeonEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));

            RootSelector = new Selector<DungeonBlackboard>
            (
                "HasFinishedDungeon",
                (b) => Progress == 100.0,
                new Leaf<DungeonBlackboard>("LeaveDungeon", (b) => ExitDungeon()),
                new Selector<DungeonBlackboard>
                (
                    "IDied",
                    (b) => IDied,
                    new Sequence<DungeonBlackboard>
                    (
                        new Leaf<DungeonBlackboard>("RecoverDeathPosition", (b) => MoveToPosition(DeathPosition)),
                        new Leaf<DungeonBlackboard>("SetIDiedToFalse", (b) =>
                        {
                            IDied = false;
                            return BehaviorTreeStatus.Success;
                        })
                    ),
                    new Selector<DungeonBlackboard>
                    (
                        "AmITheLeader",
                        (b) => WowInterface.ObjectManager.PartyleaderGuid == WowInterface.ObjectManager.PlayerGuid,
                        new Selector<DungeonBlackboard>
                        (
                            "AreAllPlayersPresent",
                            (b) => AreAllPlayersPresent(48.0),
                            new Leaf<DungeonBlackboard>("FollowNodePath", (b) => FollowNodePath()),
                            new Leaf<DungeonBlackboard>("WaitForPlayersToArrive", (b) => { return BehaviorTreeStatus.Success; })
                        ),
                        new Selector<DungeonBlackboard>
                        (
                            "IsDungeonLeaderInRange",
                            (b) => WowInterface.ObjectManager.Partyleader != null,
                            new Leaf<DungeonBlackboard>("FollowLeader", (b) => MoveToPosition(WowInterface.ObjectManager.Partyleader.Position)),
                            new Leaf<DungeonBlackboard>("WaitForLeaderToArrive", (b) => { return BehaviorTreeStatus.Success; })
                        )
                    )
                )
            );

            BehaviorTree = new AmeisenBotBehaviorTree<DungeonBlackboard>
            (
                "DungeonBehaviorTree",
                RootSelector,
                DungeonBlackboard
            );
        }

        public void OnDeath()
        {
            IDied = true;
            DeathPosition = WowInterface.ObjectManager.Player.Position;
        }

        private BehaviorTreeStatus FollowNodePath()
        {
            BehaviorTreeStatus status = MoveToPosition(CurrentNodes.Peek().Position);

            if (status == BehaviorTreeStatus.Success)
            {
                CurrentNodes.Dequeue();
            }

            return status;
        }

        private BehaviorTreeStatus ExitDungeon()
        {
            if (ExitDungeonEvent.Run())
            {
                if (WowInterface.HookManager.IsInLfgGroup())
                {
                    WowInterface.HookManager.LuaDoString("LFGTeleport(true);");
                }
                else
                {
                    MoveToPosition(Profile.Nodes.First().Position);
                }
            }

            return BehaviorTreeStatus.Success;
        }

        private BehaviorTreeStatus MoveToPosition(Vector3 position, double minDistance = 2.5)
        {
            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(position);

            if (distance > minDistance)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, position);
                return BehaviorTreeStatus.Ongoing;
            }
            else
            {
                return BehaviorTreeStatus.Success;
            }
        }

        public DungeonBlackboard DungeonBlackboard { get; }

        public AmeisenBotBehaviorTree<DungeonBlackboard> BehaviorTree { get; }

        public Queue<DungeonNode> CurrentNodes { get; private set; }

        public IDungeonProfile Profile { get; private set; }

        public List<DungeonNode> Nodes => CurrentNodes?.ToList();

        public double Progress { get; private set; }

        public int TotalNodes { get; private set; }

        private List<DungeonNode> CompletedNodes { get; set; }

        private TimegatedEvent ExitDungeonEvent { get; set; }
        public Selector<DungeonBlackboard> RootSelector { get; }
        private AmeisenBotStateMachine StateMachine { get; }

        private WowInterface WowInterface { get; }
        public bool IDied { get; private set; }
        public Vector3 DeathPosition { get; private set; }

        public void Reset()
        {

        }

        public void Enter()
        {

        }

        public void Exit()
        {
            Reset();
        }

        public void Execute()
        {
            if (Profile != null)
            {
                BehaviorTree.Tick();
            }
            else
            {
                LoadProfile(TryLoadProfile());
            }
        }

        public void LoadProfile(IDungeonProfile profile)
        {
            Profile = profile;

            for (int i = 0; i < profile.Nodes.Count; ++i)
            {
                CurrentNodes.Enqueue(profile.Nodes[i]);
            }

            WowInterface.CombatClass.PriorityTargets = profile.PriorityUnits;
            TotalNodes = CurrentNodes.Count;
        }

        private bool AreAllPlayersPresent(double distance)
        {
            return WowInterface.ObjectManager.GetNearPartymembers(WowInterface.ObjectManager.Player.Position, distance)
                   .Count(e => !e.IsDead) >= WowInterface.ObjectManager.Partymembers.Count;
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