using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Character.Inventory.Objects;
using AmeisenBotX.Core.Engines.Dungeon.Enums;
using AmeisenBotX.Core.Engines.Dungeon.Objects;
using AmeisenBotX.Core.Engines.Dungeon.Profiles.Classic;
using AmeisenBotX.Core.Engines.Dungeon.Profiles.TBC;
using AmeisenBotX.Core.Engines.Dungeon.Profiles.WotLK;
using AmeisenBotX.Core.Engines.Jobs.Profiles;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Dungeon
{
    public class DefaultDungeonEngine : IDungeonEngine
    {
        private AmeisenBotConfig Config { get; }

        public DefaultDungeonEngine(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            Bot = bot;
            Config = config;

            CurrentNodes = new();
            ExitDungeonEvent = new(TimeSpan.FromMilliseconds(1000));
            InteractionEvent = new(TimeSpan.FromMilliseconds(1000));

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
                        () => Bot.Objects.Partyleader == null || Bot.Objects.Partyleader.Guid == Bot.Wow.PlayerGuid || !Bot.Objects.PartymemberGuids.Any(),
                        new Selector
                        (
                            "AreAllPlayersPresent",
                            () => AreAllPlayersPresent(20.0f, 14.0f),
                            new Selector
                            (
                                "IsAnyoneEating",
                                () => Bot.Objects.Partymembers.Any(e => e.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Food") || e.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Drink")),
                                new Leaf("WaitForPlayersToArrive", () => { return BehaviorTreeStatus.Success; }),
                                new Leaf("FollowNodePath", () => FollowNodePath())
                            ),
                            new Leaf("WaitForPlayersToArrive", () => { return BehaviorTreeStatus.Success; })
                        ),
                        new Selector
                        (
                            "IsDungeonLeaderInRange",
                            () => Bot.Objects.Partyleader != null,
                            new Leaf("FollowLeader", () => MoveToPosition(Bot.Objects.Partyleader.Position + LeaderFollowOffset, 0f, MovementAction.Follow)),
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

        ///<inheritdoc cref="IDungeonEngine.Nodes"/>
        public List<DungeonNode> Nodes => CurrentNodes?.ToList();

        ///<inheritdoc cref="IDungeonEngine.Profile"/>
        public IDungeonProfile Profile { get; private set; }

        private AmeisenBotBehaviorTree BehaviorTree { get; }

        private AmeisenBotInterfaces Bot { get; }

        private Queue<DungeonNode> CurrentNodes { get; set; }

        private Vector3 DeathPosition { get; set; }

        private TimegatedEvent ExitDungeonEvent { get; set; }
        private TimegatedEvent InteractionEvent { get; set; }

        private bool IDied { get; set; }

        private bool IsWaitingForGroup { get; set; }

        private Vector3 LeaderFollowOffset { get; set; }

        private double Progress { get; set; }

        private Selector RootSelector { get; }

        ///<inheritdoc cref="IDungeonEngine.Enter"/>
        public void Enter()
        {
            Profile = null;
            Random rnd = new();

            LeaderFollowOffset = new()
            {
                X = ((float)rnd.NextDouble() * (10.0f * 2)) - 10.0f,
                Y = ((float)rnd.NextDouble() * (10.0f * 2)) - 10.0f,
                Z = 0f
            };
        }

        ///<inheritdoc cref="IDungeonEngine.Execute"/>
        public void Execute()
        {
            if (Profile != null)
            {
                BehaviorTree.Tick();
            }
            else
            {
                LoadProfile(TryGetProfileByMapId(Bot.Objects.MapId));
            }
        }

        ///<inheritdoc cref="IDungeonEngine.Exit"/>
        public void Exit()
        {
        }

        ///<inheritdoc cref="IDungeonEngine.OnDeath"/>
        public void OnDeath()
        {
            IDied = true;
            DeathPosition = Bot.Player.Position;
        }

        ///<inheritdoc cref="IDungeonEngine.TryGetProfileByMapId(WowMapId)"/>
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
                WowMapId.PitOfSaron => new PitOfSaronProfile(),

                _ => null
            };
        }

        private bool AreAllPlayersPresent(float distance, float distanceToStartRunning)
        {
            if (!Bot.Objects.Partymembers.Any())
            {
                return true;
            }

            if (IsWaitingForGroup)
            {
                distance = distanceToStartRunning;
            }

            int nearPlayers = Bot.GetNearPartymembers<IWowPlayer>(Bot.Player.Position, distance).Count(e => !e.IsDead);

            if (nearPlayers >= Bot.Objects.Partymembers.Count() - 1)
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
                if (Bot.Wow.IsInLfgGroup())
                {
                    Bot.Wow.LuaDoString("LFGTeleport(true);");
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
            if (CurrentNodes.Any())
            {
                if (Bot.Player.IsCasting)
                {
                    return BehaviorTreeStatus.Ongoing;
                }

                DungeonNode node = CurrentNodes.Peek();

                if (node.Position.GetDistance(Bot.Player.Position) < 4.0f)
                {
                    if (node.Type == DungeonNodeType.Use
                        || node.Type == DungeonNodeType.Door)
                    {
                        IWowGameobject nearestGameobject = Bot.Objects.WowObjects.OfType<IWowGameobject>()
                            .OrderBy(e => e.Position.GetDistance(node.Position))
                            .FirstOrDefault();

                        if (nearestGameobject.Position.GetDistance(node.Position) < 5.0f && nearestGameobject != null && nearestGameobject.Bytes0 != 0)
                        {
                            if (InteractionEvent.Run())
                            {
                                Bot.Movement.Reset();
                                Bot.Wow.StopClickToMove();

                                Bot.Wow.InteractWithObject(nearestGameobject.BaseAddress);
                            }

                            return BehaviorTreeStatus.Ongoing;
                        }
                    }
                    else if (node.Type == DungeonNodeType.Jump)
                    {
                        Bot.Character.Jump();
                    }
                    else if (node.Type == DungeonNodeType.Collect)
                    {
                        if (!Bot.Character.Inventory.HasItemByName(node.Extra))
                        {
                            IWowGameobject nearestGameobject = Bot.Objects.WowObjects.OfType<IWowGameobject>()
                                .OrderBy(e => e.Position.GetDistance(node.Position))
                                .FirstOrDefault();

                            if (nearestGameobject.Position.GetDistance(node.Position) < 5.0f)
                            {
                                if (Bot.Character.Inventory.FreeBagSlots == 0)
                                {
                                    // delete the most worthless item
                                    IWowInventoryItem itemToDelete = Bot.Character.Inventory.Items
                                        .Where(e => !Config.ItemSellBlacklist.Contains(e.Name))
                                        .OrderBy(e => e.ItemQuality).ThenBy(e => e.Price)
                                        .FirstOrDefault();

                                    if (itemToDelete != null)
                                    {
                                        Bot.Wow.DeleteItemByName(itemToDelete.Name);
                                    }
                                }

                                if (nearestGameobject != null && nearestGameobject.Bytes0 != 0)
                                {
                                    if (InteractionEvent.Run())
                                    {
                                        Bot.Movement.Reset();
                                        Bot.Wow.StopClickToMove();

                                        Bot.Wow.InteractWithObject(nearestGameobject.BaseAddress);
                                        Bot.Wow.LootEverything();
                                    }

                                    return BehaviorTreeStatus.Ongoing;
                                }
                            }

                            return BehaviorTreeStatus.Ongoing;
                        }
                    }
                }

                BehaviorTreeStatus status = MoveToPosition(node.Position, 3.0f);

                if (status == BehaviorTreeStatus.Success)
                {
                    CurrentNodes.Dequeue();
                }

                return status;
            }
            else
            {
                return MoveToPosition(Profile.DungeonExit, 2.5f);
            }
        }

        private void LoadProfile(IDungeonProfile profile)
        {
            Profile = profile;

            DungeonNode closestNode = profile.Nodes.OrderBy(e => e.Position.GetDistance(Bot.Player.Position)).FirstOrDefault();
            int closestNodeIndex = profile.Nodes.IndexOf(closestNode);

            for (int i = closestNodeIndex; i < profile.Nodes.Count; ++i)
            {
                CurrentNodes.Enqueue(profile.Nodes[i]);
            }

            Bot.CombatClass.PriorityTargetDisplayIds = profile.PriorityUnits;
        }

        private BehaviorTreeStatus MoveToPosition(Vector3 position, float minDistance = 2.5f, MovementAction movementAction = MovementAction.Move)
        {
            float distance = Bot.Player.Position.GetDistance(position);

            if (distance > minDistance)
            {
                Bot.Movement.SetMovementAction(movementAction, position);
                return BehaviorTreeStatus.Ongoing;
            }
            else
            {
                return BehaviorTreeStatus.Success;
            }
        }
    }
}