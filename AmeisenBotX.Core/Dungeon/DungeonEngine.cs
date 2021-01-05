using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Dungeon.Profiles.Classic;
using AmeisenBotX.Core.Dungeon.Profiles.TBC;
using AmeisenBotX.Core.Dungeon.Profiles.WotLK;
using AmeisenBotX.Core.Jobs.Profiles;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Dungeon
{
    public class DungeonEngine : IDungeonEngine
    {
        public DungeonEngine(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            CurrentNodes = new Queue<DungeonNode>();
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
                        (b) => WowInterface.ObjectManager.PartyleaderGuid == WowInterface.ObjectManager.PlayerGuid || !WowInterface.ObjectManager.PartymemberGuids.Any(),
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
                            new Leaf<DungeonBlackboard>("FollowLeader", (b) => MoveToPosition(WowInterface.ObjectManager.Partyleader.Position, 0f, MovementAction.Follow)),
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

        public AmeisenBotBehaviorTree<DungeonBlackboard> BehaviorTree { get; }

        public Queue<DungeonNode> CurrentNodes { get; private set; }

        public Vector3 DeathEntrancePosition { get; private set; }

        public Vector3 DeathPosition { get; private set; }

        public DungeonBlackboard DungeonBlackboard { get; }

        public bool IDied { get; private set; }

        public List<DungeonNode> Nodes => CurrentNodes?.ToList();

        public IDungeonProfile Profile { get; private set; }

        public double Progress { get; private set; }

        public Selector<DungeonBlackboard> RootSelector { get; }

        public int TotalNodes { get; private set; }

        private TimegatedEvent ExitDungeonEvent { get; set; }

        private WowInterface WowInterface { get; }

        public void Enter()
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
                LoadProfile(TryGetProfileByMapId(WowInterface.ObjectManager.MapId));
            }
        }

        public void Exit()
        {
        }

        public void LoadProfile(IDungeonProfile profile)
        {
            Profile = profile;

            // DungeonNode closestNode = profile.Nodes.OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).FirstOrDefault();
            // int closestNodeIndex = profile.Nodes.IndexOf(closestNode);

            for (int i = 0; i < profile.Nodes.Count; ++i)
            {
                CurrentNodes.Enqueue(profile.Nodes[i]);
            }

            WowInterface.CombatClass.PriorityTargetDisplayIds = profile.PriorityUnits;
            TotalNodes = CurrentNodes.Count;
        }

        public void OnDeath()
        {
            IDied = true;
            DeathPosition = WowInterface.ObjectManager.Player.Position;
            DeathEntrancePosition = Profile.WorldEntry;
        }

        public void Reset()
        {
            Profile = null;
        }

        public IDungeonProfile TryGetProfileByMapId(MapId mapId)
        {
            return mapId switch
            {
                MapId.RagefireChasm => new RagefireChasmProfile(),
                MapId.WailingCaverns => new WailingCavernsProfile(),
                MapId.Deadmines => new DeadminesProfile(),
                MapId.ShadowfangKeep => new ShadowfangKeepProfile(),
                MapId.StormwindStockade => new StockadeProfile(),

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

        private bool AreAllPlayersPresent(double distance)
        {
            return !WowInterface.ObjectManager.Partymembers.Any()
                || WowInterface.ObjectManager.GetNearPartymembers<WowPlayer>(WowInterface.ObjectManager.Player.Position, distance)
                   .Count(e => !e.IsDead) >= WowInterface.ObjectManager.Partymembers.Count() - 1;
        }

        private BehaviorTreeStatus ExitDungeon()
        {
            if (ExitDungeonEvent.Run())
            {
                if (WowInterface.HookManager.LuaIsInLfgGroup())
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

        private BehaviorTreeStatus FollowNodePath()
        {
            BehaviorTreeStatus status = MoveToPosition(CurrentNodes.Peek().Position, 5.0);

            if (status == BehaviorTreeStatus.Success)
            {
                CurrentNodes.Dequeue();
            }

            return status;
        }

        private BehaviorTreeStatus MoveToPosition(Vector3 position, double minDistance = 2.5, MovementAction movementAction = MovementAction.Move)
        {
            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(position);

            if (distance > minDistance)
            {
                WowInterface.MovementEngine.SetMovementAction(movementAction, position);
                return BehaviorTreeStatus.Ongoing;
            }
            else
            {
                return BehaviorTreeStatus.Success;
            }
        }
    }
}