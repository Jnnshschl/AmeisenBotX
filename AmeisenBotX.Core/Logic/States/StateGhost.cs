﻿using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Logic.Enums;
using AmeisenBotX.Core.Logic.StaticDeathRoutes;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Logic.States
{
    public class StateGhost : BasicState
    {
        private readonly List<IStaticDeathRoute> StaticDeathRoutes = new()
        {
            new ForgeOfSoulsDeathRoute(),
            new PitOfSaronDeathRoute()
        };

        private IWowUnit playerToFollow = null;

        public StateGhost(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
            Selector dungeonSelector = new
            (
                () => Config.DungeonUsePartyMode,
                new Selector
                (
                    () => Bot.Dungeon.TryGetProfileByMapId(StateMachine.Get<StateDead>().LastDiedMap) != null,
                    new Leaf(RunToDungeonProfileEntry),
                    new Selector
                    (
                        () => StateMachine.Get<StateFollowing>().IsUnitToFollowThere(out playerToFollow),
                        new Leaf(FollowNearestUnit),
                        new Leaf(RunToCorpsePositionAndSearchForPortals)
                    )
                ),
                new Selector
                (
                    () => Bot.Dungeon.Profile.WorldEntry != default,
                    new Leaf(RunToDungeonEntry),
                    new Leaf(RunToCorpsePositionAndSearchForPortals)
                )
            );

            BehaviorTree = new
            (
                new Selector
                (
                    () => Bot.Objects.MapId.IsBattlegroundMap(),
                    new Leaf(() =>
                    {
                        Bot.Movement.StopMovement();
                        return BtStatus.Ongoing;
                    }),
                    new Selector
                    (
                        () => CanUseStaticPaths(),
                        new Leaf(FollowStaticPath),
                        new Selector
                        (
                            () => StateMachine.Get<StateDead>().LastDiedMap.IsDungeonMap(),
                            dungeonSelector,
                            new Leaf(RunToCorpseAndRetrieveIt)
                        )
                    )
                )
            );
        }

        private AmeisenBotBehaviorTree BehaviorTree { get; }

        private Vector3 CorpsePosition { get; set; }

        private IEnumerable<IWowGameobject> NearPortals { get; set; }

        private IWowUnit PlayerToFollow => playerToFollow;

        private bool SearchedStaticRoutes { get; set; }

        private IStaticDeathRoute StaticRoute { get; set; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (!Bot.Player.IsGhost)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            NearPortals = Bot.Objects.WowObjects
                .OfType<IWowGameobject>()
                .Where(e => e.DisplayId == (int)WowGameobjectDisplayId.UtgardeKeepDungeonPortalNormal
                         || e.DisplayId == (int)WowGameobjectDisplayId.UtgardeKeepDungeonPortalHeroic);

            if (Bot.Memory.Read(Bot.Wow.Offsets.CorpsePosition, out Vector3 corpsePosition))
            {
                CorpsePosition = corpsePosition;
            }

            BehaviorTree.Tick();
        }

        public override void Leave()
        {
            SearchedStaticRoutes = false;
            StaticRoute = null;
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
                SearchedStaticRoutes = true;

                Vector3 endPosition = Bot.Dungeon.Profile != null ? Bot.Dungeon.Profile.WorldEntry : CorpsePosition;
                IStaticDeathRoute staticRoute = StaticDeathRoutes.FirstOrDefault(e => e.IsUseable(Bot.Objects.MapId, Bot.Player.Position, endPosition));

                if (staticRoute != null)
                {
                    StaticRoute = staticRoute;
                    StaticRoute.Init(Bot.Player.Position);
                }
                else
                {
                    staticRoute = StaticDeathRoutes.FirstOrDefault(e => e.IsUseable(Bot.Objects.MapId, Bot.Player.Position, CorpsePosition));

                    if (staticRoute != null)
                    {
                        StaticRoute = staticRoute;
                        StaticRoute.Init(Bot.Player.Position);
                    }
                }
            }

            return StaticRoute != null;
        }

        private BtStatus FollowNearestUnit()
        {
            if (Bot.Player.Position.GetDistance(PlayerToFollow.Position) > Config.MinFollowDistance)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, PlayerToFollow.Position);
            }

            return BtStatus.Ongoing;
        }

        private BtStatus FollowStaticPath()
        {
            Vector3 nextPosition = StaticRoute.GetNextPoint(Bot.Player.Position);

            if (nextPosition != Vector3.Zero)
            {
                Bot.Movement.SetMovementAction(MovementAction.DirectMove, nextPosition);
                return BtStatus.Ongoing;
            }
            else
            {
                // we should be in the dungeon now
                return BtStatus.Ongoing;
            }
        }

        private BtStatus RunToAndExecute(Vector3 position, Action action, double distance = 20.0)
        {
            if (Bot.Player.Position.GetDistance(position) > distance)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, position);
                return BtStatus.Ongoing;
            }
            else
            {
                action();
                return BtStatus.Success;
            }
        }

        private BtStatus RunToCorpseAndRetrieveIt()
        {
            if (Bot.Player.Position.GetDistance(CorpsePosition) > Config.GhostResurrectThreshold)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, CorpsePosition);
                return BtStatus.Ongoing;
            }
            else
            {
                Bot.Wow.RetrieveCorpse();
                return BtStatus.Success;
            }
        }

        private BtStatus RunToCorpsePositionAndSearchForPortals()
        {
            // we need to uplift the corpse to ground level because
            // blizz decided its good to place the corpse -100000m
            // below the dungeon entry when a player dies inside
            Vector3 upliftedPosition = CorpsePosition;

            // TODO: get real ground level from maps
            upliftedPosition.Z = 0.0f;

            return RunToAndExecute(upliftedPosition, () => RunToNearestPortal());
        }

        private BtStatus RunToDungeonEntry()
        {
            return RunToAndExecute(Bot.Dungeon.Profile.WorldEntry, () => RunToNearestPortal());
        }

        private BtStatus RunToDungeonProfileEntry()
        {
            Vector3 position = Bot.Dungeon.TryGetProfileByMapId(StateMachine.Get<StateDead>().LastDiedMap).WorldEntry;
            return RunToAndExecute(position, () => RunToNearestPortal());
        }

        private void RunToNearestPortal()
        {
            if (NearPortals.Any())
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, BotUtils.MoveAhead(Bot.Player.Position, NearPortals.OrderBy(e => e.Position.GetDistance(Bot.Player.Position)).First().Position, 4f));
            }
        }
    }
}