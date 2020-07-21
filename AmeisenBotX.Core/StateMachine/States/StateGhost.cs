using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Interfaces;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateGhost : BasicState
    {
        public StateGhost(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            Blackboard = new GhostBlackboard(wowInterface);

            DungeonSelector = new Selector<GhostBlackboard>
            (
                (b) => Config.DungeonUsePartyMode,
                new Selector<GhostBlackboard>
                (
                    (b) => IsUnitToFollowNear(out b.playerToFollowGuid),
                    new Leaf<GhostBlackboard>(FollowNearestUnit),
                    new Selector<GhostBlackboard>
                    (
                        (b) => WowInterface.DungeonEngine.TryGetProfileByMapId(StateMachine.LastDiedMap) != null,
                        new Leaf<GhostBlackboard>(RunToDungeonProfileEntry),
                        new Leaf<GhostBlackboard>(RunToCorpsePositionAndSearchForPortals)
                    )
                ),
                new Selector<GhostBlackboard>
                (
                    (b) => WowInterface.DungeonEngine.DeathEntrancePosition != default,
                    new Leaf<GhostBlackboard>(RunToDungeonEntry),
                    new Leaf<GhostBlackboard>(RunToCorpsePositionAndSearchForPortals)
                )
            );

            BehaviorTree = new AmeisenBotBehaviorTree<GhostBlackboard>
            (
                new Selector<GhostBlackboard>
                (
                    (b) => WowInterface.ObjectManager.MapId.IsBattlegroundMap(),
                    new Leaf<GhostBlackboard>((b) =>
                    {
                        WowInterface.MovementEngine.StopMovement();
                        return BehaviorTreeStatus.Ongoing;
                    }),
                    new Selector<GhostBlackboard>
                    (
                        (b) => StateMachine.LastDiedMap.IsDungeonMap(),
                        DungeonSelector,
                        new Leaf<GhostBlackboard>(RunToCorpseAndRetrieveIt)
                    )
                ),
                Blackboard,
                TimeSpan.FromSeconds(1)
            );
        }

        public bool NeedToEnterPortal { get; private set; }

        public TimegatedEvent PortalSearchEvent { get; private set; }

        private AmeisenBotBehaviorTree<GhostBlackboard> BehaviorTree { get; }

        private GhostBlackboard Blackboard { get; set; }

        private Selector<GhostBlackboard> DungeonSelector { get; }

        public override void Enter()
        {
            Blackboard = new GhostBlackboard(WowInterface);
        }

        public override void Execute()
        {
            if (WowInterface.ObjectManager.Player.Health > 1)
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            BehaviorTree.Tick();
        }

        public override void Exit()
        {
            NeedToEnterPortal = false;
        }

        private BehaviorTreeStatus FollowNearestUnit(GhostBlackboard blackboard)
        {
            WowPlayer player = blackboard.PlayerToFollow;

            if (WowInterface.ObjectManager.Player.Position.GetDistance(player.Position) > Config.MinFollowDistance)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, player.Position);
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private bool IsUnitOutOfRange(WowPlayer playerToFollow)
        {
            double distance = playerToFollow.Position.GetDistance(WowInterface.ObjectManager.Player.Position);
            return distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance;
        }

        private bool IsUnitToFollowNear(out ulong guid)
        {
            guid = 0;
            List<WowPlayer> wowPlayers = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>().ToList();

            if (wowPlayers.Count > 0)
            {
                if (Config.FollowSpecificCharacter)
                {
                    WowPlayer specificPlayer = wowPlayers.FirstOrDefault(p => p.Name == Config.SpecificCharacterToFollow && !IsUnitOutOfRange(p));

                    if (specificPlayer != null)
                    {
                        guid = specificPlayer.Guid;
                    }
                }

                // check the group/raid leader
                if (guid == 0 && Config.FollowGroupLeader)
                {
                    WowPlayer groupLeader = wowPlayers.FirstOrDefault(p => p.Name == Config.SpecificCharacterToFollow && !IsUnitOutOfRange(p));

                    if (groupLeader != null)
                    {
                        guid = groupLeader.Guid;
                    }
                }

                // check the group members
                if (guid == 0 && Config.FollowGroupMembers)
                {
                    WowPlayer groupMember = wowPlayers.FirstOrDefault(p => p.Name == Config.SpecificCharacterToFollow && !IsUnitOutOfRange(p));

                    if (groupMember != null)
                    {
                        guid = groupMember.Guid;
                    }
                }
            }

            return guid != 0;
        }

        private BehaviorTreeStatus RunToAndExecute(Vector3 position, Action action, double distance = 20.0)
        {
            if (WowInterface.ObjectManager.Player.Position.GetDistance(position) > distance)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, position);
                return BehaviorTreeStatus.Ongoing;
            }
            else
            {
                action();
                return BehaviorTreeStatus.Success;
            }
        }

        private BehaviorTreeStatus RunToCorpseAndRetrieveIt(GhostBlackboard blackboard)
        {
            if (WowInterface.ObjectManager.Player.Position.GetDistance(blackboard.CorpsePosition) > Config.GhostResurrectThreshold)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, blackboard.CorpsePosition);
                return BehaviorTreeStatus.Ongoing;
            }
            else
            {
                WowInterface.HookManager.RetrieveCorpse();
                return BehaviorTreeStatus.Success;
            }
        }

        private BehaviorTreeStatus RunToCorpsePositionAndSearchForPortals(GhostBlackboard blackboard)
        {
            Vector3 upliftedPosition = blackboard.CorpsePosition;
            upliftedPosition.Z = 0f;
            return RunToAndExecute(upliftedPosition, () => RunToNearestPortal(blackboard));
        }

        private BehaviorTreeStatus RunToDungeonEntry(GhostBlackboard blackboard)
        {
            return RunToAndExecute(WowInterface.DungeonEngine.DeathEntrancePosition, () => RunToNearestPortal(blackboard));
        }

        private BehaviorTreeStatus RunToDungeonProfileEntry(GhostBlackboard blackboard)
        {
            Vector3 position = WowInterface.DungeonEngine.TryGetProfileByMapId(StateMachine.LastDiedMap).WorldEntry;
            return RunToAndExecute(position, () => RunToNearestPortal(blackboard));
        }

        private void RunToNearestPortal(GhostBlackboard blackboard)
        {
            if (blackboard.NearPortals?.Count > 0)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, BotUtils.MoveAhead(WowInterface.ObjectManager.Player.Position, blackboard.NearPortals.OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).First().Position, 4f));
            }
        }

        private class GhostBlackboard : IBlackboard
        {
            public ulong playerToFollowGuid = 0;

            public GhostBlackboard(WowInterface wowInterface)
            {
                WowInterface = wowInterface;
            }

            public Vector3 CorpsePosition { get; private set; }

            public List<WowGameobject> NearPortals { get; private set; }

            public WowPlayer PlayerToFollow => WowInterface.ObjectManager.GetWowObjectByGuid<WowPlayer>(playerToFollowGuid);

            private WowInterface WowInterface { get; }

            public void Update()
            {
                NearPortals = WowInterface.ObjectManager.WowObjects
                    .OfType<WowGameobject>()
                    .Where(e => e.DisplayId == (int)GameobjectDisplayId.DungeonPortalNormal
                             || e.DisplayId == (int)GameobjectDisplayId.DungeonPortalHeroic)
                    .ToList();

                if (WowInterface.XMemory.ReadStruct(WowInterface.OffsetList.CorpsePosition, out Vector3 corpsePosition))
                {
                    CorpsePosition = corpsePosition;
                }
            }
        }
    }
}