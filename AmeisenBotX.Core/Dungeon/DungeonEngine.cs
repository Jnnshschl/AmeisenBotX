using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
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

            CurrentNodes = new();
            ExitDungeonEvent = new(TimeSpan.FromMilliseconds(1000));

            RootSelector = new
            (
                "HasFinishedDungeon",
                () => Progress == 100.0,
                new Leaf("LeaveDungeon", () => ExitDungeon()),
                new Selector
                (
                    "IDied",
                    () => IDied,
                    new Sequence
                    (
                        new Leaf("RecoverDeathPosition", () => MoveToPosition(DeathPosition)),
                        new Leaf("SetIDiedToFalse", () =>
                        {
                            IDied = false;
                            return BehaviorTreeStatus.Success;
                        })
                    ),
                    new Selector
                    (
                        "AmITheLeader",
                        () => WowInterface.ObjectManager.PartyleaderGuid == WowInterface.PlayerGuid || !WowInterface.ObjectManager.PartymemberGuids.Any(),
                        new Selector
                        (
                            "AreAllPlayersPresent",
                            () => AreAllPlayersPresent(18.0f, 11.0f),
                            new Selector
                            (
                                "IsAnyoneEating",
                                () => WowInterface.ObjectManager.Partymembers.Any(e => e.HasBuffByName("Food") || e.HasBuffByName("Drink")),
                                new Leaf("WaitForPlayersToArrive", () => { return BehaviorTreeStatus.Success; }),
                                new Leaf("FollowNodePath", () => FollowNodePath())
                            ),
                            new Leaf("WaitForPlayersToArrive", () => { return BehaviorTreeStatus.Success; })
                        ),
                        new Selector
                        (
                            "IsDungeonLeaderInRange",
                            () => WowInterface.ObjectManager.Partyleader != null,
                            new Leaf("FollowLeader", () => MoveToPosition(WowInterface.ObjectManager.Partyleader.Position + LeaderFollowOffset, 0f, MovementAction.Follow)),
                            new Leaf("WaitForLeaderToArrive", () => { return BehaviorTreeStatus.Success; })
                        )
                    )
                )
            );

            BehaviorTree = new
            (
                RootSelector
            );
        }

        public AmeisenBotBehaviorTree BehaviorTree { get; }

        public Queue<DungeonNode> CurrentNodes { get; private set; }

        public Vector3 DeathEntrancePosition { get; private set; }

        public Vector3 DeathPosition { get; private set; }

        public bool IDied { get; private set; }

        public bool IsWaitingForGroup { get; private set; }

        public Vector3 LeaderFollowOffset { get; set; }

        public List<DungeonNode> Nodes => CurrentNodes?.ToList();

        public IDungeonProfile Profile { get; private set; }

        public double Progress { get; private set; }

        public Selector RootSelector { get; }

        public int TotalNodes { get; private set; }

        private TimegatedEvent ExitDungeonEvent { get; set; }

        private WowInterface WowInterface { get; }

        public void Enter()
        {
            Reset();

            Random rnd = new();
            LeaderFollowOffset = new()
            {
                X = ((float)rnd.NextDouble() * (10.0f * 2)) - 10.0f,
                Y = ((float)rnd.NextDouble() * (10.0f * 2)) - 10.0f,
                Z = 0f
            };
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

            DungeonNode closestNode = profile.Nodes.OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position)).FirstOrDefault();
            int closestNodeIndex = profile.Nodes.IndexOf(closestNode);

            for (int i = closestNodeIndex; i < profile.Nodes.Count; ++i)
            {
                CurrentNodes.Enqueue(profile.Nodes[i]);
            }

            WowInterface.CombatClass.PriorityTargetDisplayIds = profile.PriorityUnits;
            TotalNodes = CurrentNodes.Count;
        }

        public void OnDeath()
        {
            IDied = true;
            DeathPosition = WowInterface.Player.Position;
            DeathEntrancePosition = Profile.WorldEntry;
        }

        public void Reset()
        {
            Profile = null;
        }

        public IDungeonProfile TryGetProfileByMapId(WowMapId mapId)
        {
            return mapId switch
            {
                WowMapId.RagefireChasm => new RagefireChasmProfile(),
                WowMapId.WailingCaverns => new WailingCavernsProfile(),
                WowMapId.Deadmines => new DeadminesProfile(),
                WowMapId.ShadowfangKeep => new ShadowfangKeepProfile(),
                WowMapId.StormwindStockade => new StockadeProfile(),

                WowMapId.HellfireRamparts => new HellfireRampartsProfile(),
                WowMapId.TheBloodFurnace => new TheBloodFurnaceProfile(),
                WowMapId.TheSlavePens => new TheSlavePensProfile(),
                WowMapId.TheUnderbog => new TheUnderbogProfile(),
                WowMapId.TheSteamvault => new TheSteamvaultProfile(),

                WowMapId.UtgardeKeep => new UtgardeKeepProfile(),
                WowMapId.AzjolNerub => new AzjolNerubProfile(),
                WowMapId.TheForgeOfSouls => new ForgeOfSoulsProfile(),

                _ => null
            };
        }

        private bool AreAllPlayersPresent(float distance, float distanceToStartRunning)
        {
            if (!WowInterface.ObjectManager.Partymembers.Any())
            {
                return true;
            }

            if (IsWaitingForGroup)
            {
                distance = distanceToStartRunning;
            }

            int nearPlayers = WowInterface.ObjectManager.GetNearPartymembers<WowPlayer>(WowInterface.Player.Position, distance).Count(e => !e.IsDead);

            if (nearPlayers >= WowInterface.ObjectManager.Partymembers.Count() - 1)
            {
                IsWaitingForGroup = false;
                return true;
            }
            else
            {
                IsWaitingForGroup = true;
                return false;
            }
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
            double distance = WowInterface.Player.Position.GetDistance(position);

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