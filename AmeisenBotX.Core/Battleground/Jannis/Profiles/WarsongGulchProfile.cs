using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace AmeisenBotX.Core.Battleground.Jannis.Profiles
{
    public class WarsongGulchProfile : IBattlegroundProfile
    {
        public WarsongGulchProfile(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            ActionEvent = new(TimeSpan.FromMilliseconds(500));
            LosCheckEvent = new(TimeSpan.FromMilliseconds(1000));

            JBgBlackboard = new(UpdateBattlegroundInfo);

            KillEnemyFlagCarrierSelector = new
            (
                "EnemyTeamFlagCarrierInRange",
                (b) => b.EnemyTeamFlagCarrier != null,
                new Selector<CtfBlackboard>
                (
                    "HasFlag",
                    (b) => b.MyTeamHasFlag
                        || b.EnemyTeamFlagCarrier.Position.GetDistance(WsgDataset.EnemyBasePosition)
                         < Bot.Player.Position.GetDistance(WsgDataset.EnemyBasePosition),
                    new Leaf<CtfBlackboard>("KillEnemyFlagCarrier", KillEnemyFlagCarrier),
                    new Leaf<CtfBlackboard>("MoveToEnemyBaseAndGetFlag", MoveToEnemyBaseAndGetFlag)
                ),
                new Selector<CtfBlackboard>
                (
                    "IsFlagNearOwnOrEnemyBase",
                    (b) => b.EnemyTeamFlagPos.GetDistance2D(WsgDataset.EnemyBasePositionMapCoords)
                         < b.EnemyTeamFlagPos.GetDistance2D(WsgDataset.OwnBasePositionMapCoords),
                    new Leaf<CtfBlackboard>("MoveToEnemyBase", (b) => MoveToPosition(WsgDataset.EnemyBasePosition)),
                    new Leaf<CtfBlackboard>("MoveToOwnBase", (b) => MoveToPosition(WsgDataset.OwnBasePosition))
                )
            );

            FlagSelector = new DualSelector<CtfBlackboard>
            (
                "WhoHasTheFlag",
                (b) => b.MyTeamHasFlag,
                (b) => b.EnemyTeamHasFlag,

                // no team has the flag
                new Leaf<CtfBlackboard>("MoveToEnemyBaseAndGetFlag", MoveToEnemyBaseAndGetFlag),

                // only my team has the flag
                new Selector<CtfBlackboard>
                (
                    "AmITheFlagCarrier",
                    (b) => b.MyTeamFlagCarrier != null && b.MyTeamFlagCarrier.Guid == Bot.Wow.PlayerGuid,
                    new Leaf<CtfBlackboard>("MoveToOwnBase", (b) => MoveToPosition(WsgDataset.OwnBasePosition)),
                    new Selector<CtfBlackboard>
                    (
                        "IsTheFlagCarrierInRange",
                        (b) => b.MyTeamFlagCarrier != null,
                        new Leaf<CtfBlackboard>("MoveToOwnFlagCarrier", (b) => MoveToPosition(b.MyTeamFlagCarrier.Position)),
                        new Leaf<CtfBlackboard>("DefendOwnBase", DefendOwnBase)
                    )
                ),

                // only enemy team has the flag
                KillEnemyFlagCarrierSelector,

                // both teams have the flag
                new Selector<CtfBlackboard>
                (
                    "AmITheFlagCarrier",
                    (b) => b.MyTeamFlagCarrier != null && b.MyTeamFlagCarrier.Guid == Bot.Wow.PlayerGuid,
                    new Selector<CtfBlackboard>
                    (
                        "AmINearOwnBase",
                        (b) => Bot.Player.Position.GetDistance(WsgDataset.OwnBasePosition) < 128.0f,
                        new Leaf<CtfBlackboard>("MoveToHidingSpot", (b) => MoveToPosition(WsgDataset.FlagHidingSpot)),
                        new Leaf<CtfBlackboard>("MoveToOwnBase", (b) => MoveToPosition(WsgDataset.OwnBasePosition))
                    ),
                    KillEnemyFlagCarrierSelector
                )
            );

            MainSelector = new Selector<CtfBlackboard>
            (
                "IsGateOpen",
                (b) => IsGateOpen(),
                 new Selector<CtfBlackboard>
                 (
                     "IsFlagNear",
                     (b) => IsFlagNear(),
                     new Leaf<CtfBlackboard>("UseNearestFlag", UseNearestFlag),
                     new Selector<CtfBlackboard>
                     (
                         "IsAnyBuffNearMe",
                         (b) => IsAnyBuffNearMe(16.0f),
                         new Leaf<CtfBlackboard>("MoveToNearestBuff", MoveToNearestBuff),
                         new Selector<CtfBlackboard>
                         (
                             "DoWeOutnumberOurEnemies",
                             (b) => DoWeOutnumberOurEnemies(b),
                             new Leaf<CtfBlackboard>("AttackNearWeakestEnemy", AttackNearWeakestEnemy),
                             FlagSelector
                         )
                     )
                 ),
                 new Leaf<CtfBlackboard>("MoveToGatePosition", (b) => MoveToPosition(WsgDataset.GatePosition))
            );

            BehaviorTree = new
            (
                "JBgWarsongGulchBehaviorTree",
                MainSelector,
                JBgBlackboard,
                TimeSpan.FromSeconds(1)
            );
        }

        private interface IWsgDataset
        {
            Vector3 EnemyBasePosition { get; }

            Vector3 EnemyBasePositionMapCoords { get; }

            Vector3 FlagHidingSpot { get; }

            Vector3 GatePosition { get; }

            Vector3 OwnBasePosition { get; }

            Vector3 OwnBasePositionMapCoords { get; }
        }

        public AmeisenBotBehaviorTree<CtfBlackboard> BehaviorTree { get; }

        public DualSelector<CtfBlackboard> FlagSelector { get; }

        public CtfBlackboard JBgBlackboard { get; set; }

        public Selector<CtfBlackboard> MainSelector { get; }

        private TimegatedEvent ActionEvent { get; }

        private bool InLos { get; set; }

        private Selector<CtfBlackboard> KillEnemyFlagCarrierSelector { get; }

        private TimegatedEvent LosCheckEvent { get; }

        private AmeisenBotInterfaces Bot { get; }

        private IWsgDataset WsgDataset { get; set; }

        public void Execute()
        {
            if (WsgDataset == null)
            {
                if (Bot.Player.IsAlliance())
                {
                    WsgDataset = new AllianceWsgDataset();
                }
                else
                {
                    WsgDataset = new HordeWsgDataset();
                }
            }

            BehaviorTree.Tick();
        }

        private bool AmINearOwnFlagCarrier(CtfBlackboard blackboard)
        {
            return Bot.Objects.GetNearEnemies<WowUnit>(Bot.Db.GetReaction, blackboard.MyTeamFlagCarrier.Position, 48.0f).Any(e => !e.IsDead);
        }

        private bool AmIOneOfTheClosestToOwnFlagCarrier(CtfBlackboard blackboard, int memberCount)
        {
            if (memberCount <= 0 || blackboard.MyTeamFlagCarrier == null)
            {
                return false;
            }

            // check whether i'm part of the closest x (memberCount) members to the flag carrier
            int index = Bot.Objects.Partymembers
                            .OfType<WowPlayer>()
                            .Where(e => e.Guid != blackboard.MyTeamFlagCarrier.Guid)
                            .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                            .Select((player, id) => new { Player = player, Index = id })
                            .FirstOrDefault(_ => _.Player.Guid == Bot.Wow.PlayerGuid)?.Index ?? -1;

            return index > -1 && index <= memberCount;
        }

        private BehaviorTreeStatus AttackNearWeakestEnemy(CtfBlackboard blackboard)
        {
            WowPlayer weakestPlayer = Bot.Objects.GetNearEnemies<WowPlayer>(Bot.Db.GetReaction, Bot.Player.Position, 20.0f).OrderBy(e => e.Health).FirstOrDefault();

            if (weakestPlayer != null)
            {
                float distance = weakestPlayer.Position.GetDistance(Bot.Player.Position);
                float threshold = Bot.CombatClass.IsMelee ? 3.0f : 28.0f;

                if (distance > threshold)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, weakestPlayer.Position);
                }
                else if (ActionEvent.Run())
                {
                    Bot.Globals.ForceCombat = true;
                    Bot.Wow.WowTargetGuid(weakestPlayer.Guid);
                }
            }
            else
            {
                return BehaviorTreeStatus.Failed;
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private BehaviorTreeStatus DefendOwnBase(CtfBlackboard blackboard)
        {
            double distance = Bot.Player.Position.GetDistance(WsgDataset.OwnBasePosition);

            if (distance > 16.0)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, WsgDataset.OwnBasePosition);
            }
            else
            {
                WowUnit nearEnemy = Bot.Objects.GetNearEnemies<WowUnit>(Bot.Db.GetReaction, WsgDataset.OwnBasePosition, 16.0f).FirstOrDefault();

                if (nearEnemy != null)
                {
                    float distanceToEnemy = Bot.Player.Position.GetDistance(nearEnemy.Position);

                    if (distanceToEnemy > 2.0f)
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Move, nearEnemy.Position);
                    }
                    else if (ActionEvent.Run())
                    {
                        Bot.Globals.ForceCombat = true;
                        Bot.Wow.WowTargetGuid(nearEnemy.Guid);
                    }
                }
            }
            return BehaviorTreeStatus.Ongoing;
        }

        private bool DoWeOutnumberOurEnemies(CtfBlackboard blackboard)
        {
            if (blackboard.MyTeamFlagCarrier != null && blackboard.MyTeamFlagCarrier.Guid == Bot.Wow.PlayerGuid)
            {
                return false;
            }

            int friends = Bot.Objects.GetNearFriends<WowPlayer>(Bot.Db.GetReaction, Bot.Player.Position, 18.0f).Count();
            int enemies = Bot.Objects.GetNearEnemies<WowPlayer>(Bot.Db.GetReaction, Bot.Player.Position, 18.0f).Count();

            return enemies > 0 && friends >= enemies;
        }

        private bool EnemiesNearFlagCarrier(CtfBlackboard blackboard)
        {
            return blackboard.MyTeamFlagCarrier.Position.GetDistance(Bot.Player.Position) < 32.0;
        }

        private BehaviorTreeStatus FleeFromComingEnemies(CtfBlackboard blackboard)
        {
            WowUnit nearestEnemy = Bot.Objects.GetNearEnemies<WowUnit>(Bot.Db.GetReaction, Bot.Player.Position, 48.0f).OrderBy(e => e.Position.GetDistance(Bot.Player.Position)).FirstOrDefault();

            if (nearestEnemy != null)
            {
                Bot.Movement.SetMovementAction(MovementAction.Flee, nearestEnemy.Position, nearestEnemy.Rotation);
                return BehaviorTreeStatus.Ongoing;
            }
            else
            {
                return BehaviorTreeStatus.Success;
            }
        }

        private bool IsAnyBuffNearMe(float distance)
        {
            return Bot.Objects.GetClosestWowGameobjectByDisplayId(Bot.Player.Position, new List<int>() { 5991, 5995, 5931 })?.Position.GetDistance(Bot.Player.Position) < distance;
        }

        private bool IsFlagNear()
        {
            return JBgBlackboard.NearFlags != null && JBgBlackboard.NearFlags.Any(e => e.Position.GetDistance(Bot.Player.Position) < 8.0);
        }

        private bool IsGateOpen()
        {
            if (Bot.Player.IsAlliance())
            {
                WowGameobject obj = Bot.Objects.WowObjects.OfType<WowGameobject>()
                                    .Where(e => e.GameobjectType == WowGameobjectType.Door && e.DisplayId == 411)
                                    .FirstOrDefault();

                return obj == null || obj.Bytes0 == 0;
            }
            else
            {
                WowGameobject obj = Bot.Objects.WowObjects.OfType<WowGameobject>()
                                    .Where(e => e.GameobjectType == WowGameobjectType.Door && e.DisplayId == 850)
                                    .FirstOrDefault();

                return obj == null || obj.Bytes0 == 0;
            }
        }

        private BehaviorTreeStatus KillEnemyFlagCarrier(CtfBlackboard blackboard)
        {
            if (JBgBlackboard.EnemyTeamFlagCarrier == null)
            {
                Bot.Globals.ForceCombat = false;
                return BehaviorTreeStatus.Success;
            }

            float distance = Bot.Player.Position.GetDistance(JBgBlackboard.EnemyTeamFlagCarrier.Position);
            float threshold = Bot.CombatClass.IsMelee ? 3.0f : 28.0f;

            if (distance > threshold && !Bot.Player.IsCasting)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, BotUtils.MoveAhead(JBgBlackboard.EnemyTeamFlagCarrier.Position, JBgBlackboard.EnemyTeamFlagCarrier.Rotation, 1.0f));
            }
            else if (ActionEvent.Run())
            {
                Bot.Globals.ForceCombat = true;
                Bot.Wow.WowTargetGuid(JBgBlackboard.EnemyTeamFlagCarrier.Guid);
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private BehaviorTreeStatus MoveToEnemyBaseAndGetFlag(CtfBlackboard blackboard)
        {
            float distance = Bot.Player.Position.GetDistance(WsgDataset.EnemyBasePosition);

            if (distance > 2.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, WsgDataset.EnemyBasePosition);
            }
            else
            {
                return UseNearestFlag(blackboard);
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private BehaviorTreeStatus MoveToNearestBuff(CtfBlackboard blackboard)
        {
            WowGameobject buffObject = Bot.Objects.GetClosestWowGameobjectByDisplayId(Bot.Player.Position, new List<int>() { 5991, 5995, 5931 });

            if (buffObject != null
                && buffObject.Position.GetDistance(Bot.Player.Position) > 3.0)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, buffObject.Position);
            }
            else
            {
                return BehaviorTreeStatus.Failed;
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private BehaviorTreeStatus MoveToOwnFlagCarrierAndHelp(CtfBlackboard blackboard)
        {
            if (JBgBlackboard.MyTeamFlagCarrier == null)
            {
                return BehaviorTreeStatus.Failed;
            }

            float distance = Bot.Player.Position.GetDistance(JBgBlackboard.MyTeamFlagCarrier.Position);

            if (distance > 4.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, JBgBlackboard.MyTeamFlagCarrier.Position);
            }
            else
            {
                WowUnit nearEnemy = Bot.Objects.GetNearEnemies<WowUnit>(Bot.Db.GetReaction, JBgBlackboard.MyTeamFlagCarrier.Position, 32.0f).FirstOrDefault();

                if (nearEnemy != null)
                {
                    float distanceToEnemy = Bot.Player.Position.GetDistance(nearEnemy.Position);
                    float threshold = Bot.CombatClass.IsMelee ? 3.0f : 28.0f;

                    if (distanceToEnemy > threshold)
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Move, nearEnemy.Position);
                    }
                    else if (ActionEvent.Run())
                    {
                        Bot.Globals.ForceCombat = true;
                        Bot.Wow.WowTargetGuid(nearEnemy.Guid);
                    }
                }
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private BehaviorTreeStatus MoveToPosition(Vector3 position, float minDistance = 2.5f)
        {
            double distance = Bot.Player.Position.GetDistance(position);

            if (distance > minDistance)
            {
                float zDiff = position.Z - Bot.Player.Position.Z;

                if (LosCheckEvent.Run())
                {
                    if (Bot.Wow.WowIsInLineOfSight(Bot.Player.Position, position, 2.0f))
                    {
                        InLos = true;
                    }
                    else
                    {
                        InLos = false;
                    }
                }

                if (zDiff < -4.0 && InLos) // target is below us and in line of sight, just run down
                {
                    Bot.Movement.SetMovementAction(MovementAction.DirectMove, position);
                }
                else
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, position);
                }

                return BehaviorTreeStatus.Ongoing;
            }
            else
            {
                return BehaviorTreeStatus.Success;
            }
        }

        private void UpdateBattlegroundInfo()
        {
            try
            {
                if (Bot.Wow.WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=\"{{\"_,stateA,textA,_,_,_,_,_,_,_,_,_=GetWorldStateUIInfo(2)_,stateH,textH,_,_,_,_,_,_,_,_,_=GetWorldStateUIInfo(3)flagXA,flagYA=GetBattlefieldFlagPosition(1)flagXH,flagYH=GetBattlefieldFlagPosition(2){{v:0}}={{v:0}}..\"\\\"allianceState\\\" : \\\"\"..stateA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"allianceText\\\" : \\\"\"..textA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeState\\\" : \\\"\"..stateH..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeText\\\" : \\\"\"..textH..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"allianceFlagX\\\" : \\\"\"..flagXA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"allianceFlagY\\\" : \\\"\"..flagYA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeFlagX\\\" : \\\"\"..flagXH..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeFlagY\\\" : \\\"\"..flagYH..\"\\\"\"{{v:0}}={{v:0}}..\"}}\""), out string result))
                {
                    dynamic bgState = JsonConvert.DeserializeObject(result);

                    if (Bot.Player.IsAlliance())
                    {
                        string[] splittedScoreA = ((string)bgState.allianceText).Split('/');
                        JBgBlackboard.MyTeamScore = int.Parse(splittedScoreA[0], CultureInfo.InvariantCulture);
                        JBgBlackboard.MyTeamMaxScore = int.Parse(splittedScoreA[1], CultureInfo.InvariantCulture);

                        string[] splittedScoreH = ((string)bgState.hordeText).Split('/');
                        JBgBlackboard.EnemyTeamScore = int.Parse(splittedScoreH[0], CultureInfo.InvariantCulture);
                        JBgBlackboard.EnemyTeamMaxScore = int.Parse(splittedScoreH[1], CultureInfo.InvariantCulture);

                        JBgBlackboard.MyTeamHasFlag = int.Parse((string)bgState.allianceState, CultureInfo.InvariantCulture) == 2;
                        JBgBlackboard.EnemyTeamHasFlag = int.Parse((string)bgState.hordeState, CultureInfo.InvariantCulture) == 2;

                        JBgBlackboard.MyTeamFlagPos = new
                        (
                            float.Parse((string)bgState.allianceFlagX, NumberStyles.Any, CultureInfo.InvariantCulture) * 100.0f,
                            float.Parse((string)bgState.allianceFlagY, NumberStyles.Any, CultureInfo.InvariantCulture) * 100.0f,
                            0f
                        );

                        JBgBlackboard.EnemyTeamFlagPos = new
                        (
                            float.Parse((string)bgState.hordeFlagX, NumberStyles.Any, CultureInfo.InvariantCulture) * 100.0f,
                            float.Parse((string)bgState.hordeFlagY, NumberStyles.Any, CultureInfo.InvariantCulture) * 100.0f,
                            0f
                        );

                        JBgBlackboard.MyTeamFlagCarrier = Bot.Objects.WowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.HasBuffById(23333));
                        JBgBlackboard.EnemyTeamFlagCarrier = Bot.Objects.WowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.HasBuffById(23335));
                    }
                    else
                    {
                        string[] splittedScoreH = ((string)bgState.hordeText).Split('/');
                        JBgBlackboard.MyTeamScore = int.Parse(splittedScoreH[0], CultureInfo.InvariantCulture);
                        JBgBlackboard.MyTeamMaxScore = int.Parse(splittedScoreH[1], CultureInfo.InvariantCulture);

                        string[] splittedScoreA = ((string)bgState.allianceText).Split('/');
                        JBgBlackboard.EnemyTeamScore = int.Parse(splittedScoreA[0], CultureInfo.InvariantCulture);
                        JBgBlackboard.EnemyTeamMaxScore = int.Parse(splittedScoreA[1], CultureInfo.InvariantCulture);

                        JBgBlackboard.MyTeamHasFlag = int.Parse((string)bgState.hordeState, CultureInfo.InvariantCulture) == 2;
                        JBgBlackboard.EnemyTeamHasFlag = int.Parse((string)bgState.allianceState, CultureInfo.InvariantCulture) == 2;

                        JBgBlackboard.MyTeamFlagPos = new
                        (
                            float.Parse((string)bgState.hordeFlagX, NumberStyles.Any, CultureInfo.InvariantCulture) * 100.0f,
                            float.Parse((string)bgState.hordeFlagY, NumberStyles.Any, CultureInfo.InvariantCulture) * 100.0f,
                            0f
                        );

                        JBgBlackboard.EnemyTeamFlagPos = new
                        (
                            float.Parse((string)bgState.allianceFlagX, NumberStyles.Any, CultureInfo.InvariantCulture) * 100.0f,
                            float.Parse((string)bgState.allianceFlagY, NumberStyles.Any, CultureInfo.InvariantCulture) * 100.0f,
                            0f
                        );

                        JBgBlackboard.MyTeamFlagCarrier = Bot.Objects.WowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.HasBuffById(23335));
                        JBgBlackboard.EnemyTeamFlagCarrier = Bot.Objects.WowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.HasBuffById(23333));
                    }

                    JBgBlackboard.NearFlags = Bot.Objects.WowObjects
                                                 .OfType<WowGameobject>()
                                                 .Where(e => e.DisplayId == (int)WowGameobjectDisplayId.WsgAllianceFlag || e.DisplayId == (int)WowGameobjectDisplayId.WsgHordeFlag)
                                                 .ToList();
                }
            }
            catch { }
        }

        private BehaviorTreeStatus UseNearestFlag(CtfBlackboard blackboard)
        {
            WowGameobject nearestFlag = JBgBlackboard.NearFlags.OrderBy(e => e.Position.GetDistance(Bot.Player.Position)).FirstOrDefault();

            if (nearestFlag != null)
            {
                float distance = Bot.Player.Position.GetDistance(nearestFlag.Position);

                if (distance > 4.0f)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, nearestFlag.Position);
                }
                else if (ActionEvent.Run())
                {
                    Bot.Wow.WowObjectRightClick(nearestFlag.BaseAddress);
                }
            }
            else
            {
                return BehaviorTreeStatus.Failed;
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private class AllianceWsgDataset : IWsgDataset
        {
            public Vector3 EnemyBasePosition { get; } = new(916, 1434, 346);

            public Vector3 EnemyBasePositionMapCoords { get; } = new(53, 90, 0);

            public Vector3 FlagHidingSpot { get; } = new(1519, 1467, 374);

            public Vector3 GatePosition { get; } = new(1494, 1457, 343);

            public Vector3 OwnBasePosition { get; } = new(1539, 1481, 352);

            public Vector3 OwnBasePositionMapCoords { get; } = new(49, 15, 0);
        }

        private class HordeWsgDataset : IWsgDataset
        {
            public Vector3 EnemyBasePosition { get; } = new(1539, 1481, 352);

            public Vector3 EnemyBasePositionMapCoords { get; } = new(49, 15, 0);

            public Vector3 FlagHidingSpot { get; } = new(949, 1449, 367);

            public Vector3 GatePosition { get; } = new(951, 1459, 342);

            public Vector3 OwnBasePosition { get; } = new(916, 1434, 346);

            public Vector3 OwnBasePositionMapCoords { get; } = new(53, 90, 0);
        }
    }
}