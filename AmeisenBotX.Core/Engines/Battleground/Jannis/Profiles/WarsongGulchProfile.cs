using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles
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
                (b) => b.EnemyTeamFlagCarrier != null,
                new Selector<CtfBlackboard>
                (
                    (b) => b.MyTeamHasFlag
                        || b.EnemyTeamFlagCarrier.Position.GetDistance(WsgDataset.EnemyBasePosition)
                         < Bot.Player.Position.GetDistance(WsgDataset.EnemyBasePosition),
                    new Leaf<CtfBlackboard>(KillEnemyFlagCarrier),
                    new Leaf<CtfBlackboard>(MoveToEnemyBaseAndGetFlag)
                ),
                new Selector<CtfBlackboard>
                (
                    (b) => b.EnemyTeamFlagPos.GetDistance2D(WsgDataset.EnemyBasePositionMapCoords)
                         < b.EnemyTeamFlagPos.GetDistance2D(WsgDataset.OwnBasePositionMapCoords),
                    new Leaf<CtfBlackboard>((b) => MoveToPosition(WsgDataset.EnemyBasePosition)),
                    new Leaf<CtfBlackboard>((b) => MoveToPosition(WsgDataset.OwnBasePosition))
                )
            );

            FlagSelector = new DualSelector<CtfBlackboard>
            (
                (b) => b.MyTeamHasFlag,
                (b) => b.EnemyTeamHasFlag,

                // no team has the flag
                new Leaf<CtfBlackboard>(MoveToEnemyBaseAndGetFlag),

                // only my team has the flag
                new Selector<CtfBlackboard>
                (
                    (b) => b.MyTeamFlagCarrier != null && b.MyTeamFlagCarrier.Guid == Bot.Wow.PlayerGuid,
                    new Leaf<CtfBlackboard>((b) => MoveToPosition(WsgDataset.OwnBasePosition)),
                    new Selector<CtfBlackboard>
                    (
                        (b) => b.MyTeamFlagCarrier != null,
                        new Leaf<CtfBlackboard>((b) => MoveToPosition(b.MyTeamFlagCarrier.Position)),
                        new Leaf<CtfBlackboard>(DefendOwnBase)
                    )
                ),

                // only enemy team has the flag
                KillEnemyFlagCarrierSelector,

                // both teams have the flag
                new Selector<CtfBlackboard>
                (
                    (b) => b.MyTeamFlagCarrier != null && b.MyTeamFlagCarrier.Guid == Bot.Wow.PlayerGuid,
                    new Selector<CtfBlackboard>
                    (
                        (b) => Bot.Player.Position.GetDistance(WsgDataset.OwnBasePosition) < 128.0f,
                        new Leaf<CtfBlackboard>((b) => MoveToPosition(WsgDataset.FlagHidingSpot)),
                        new Leaf<CtfBlackboard>((b) => MoveToPosition(WsgDataset.OwnBasePosition))
                    ),
                    KillEnemyFlagCarrierSelector
                )
            );

            MainSelector = new Selector<CtfBlackboard>
            (
                (b) => IsGateOpen(),
                 new Selector<CtfBlackboard>
                 (
                     (b) => IsFlagNear(),
                     new Leaf<CtfBlackboard>(UseNearestFlag),
                     new Selector<CtfBlackboard>
                     (
                         (b) => IsAnyBuffNearMe(16.0f),
                         new Leaf<CtfBlackboard>(MoveToNearestBuff),
                         new Selector<CtfBlackboard>
                         (
                             (b) => DoWeOutnumberOurEnemies(b),
                             new Leaf<CtfBlackboard>(AttackNearWeakestEnemy),
                             FlagSelector
                         )
                     )
                 ),
                 new Leaf<CtfBlackboard>((b) => MoveToPosition(WsgDataset.GatePosition))
            );

            BehaviorTree = new
            (
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

        public BehaviorTree<CtfBlackboard> BehaviorTree { get; }

        public DualSelector<CtfBlackboard> FlagSelector { get; }

        public CtfBlackboard JBgBlackboard { get; set; }

        public Selector<CtfBlackboard> MainSelector { get; }

        private TimegatedEvent ActionEvent { get; }

        private AmeisenBotInterfaces Bot { get; }

        private bool InLos { get; set; }

        private Selector<CtfBlackboard> KillEnemyFlagCarrierSelector { get; }

        private TimegatedEvent LosCheckEvent { get; }

        private IWsgDataset WsgDataset { get; set; }

        public void Execute()
        {
            if (WsgDataset == null)
            {
                WsgDataset = Bot.Player.IsAlliance() ? new AllianceWsgDataset() : new HordeWsgDataset();
            }

            BehaviorTree.Tick();
        }

        private bool AmINearOwnFlagCarrier(CtfBlackboard blackboard)
        {
            return Bot.GetNearEnemies<IWowUnit>(blackboard.MyTeamFlagCarrier.Position, 48.0f).Any(e => !e.IsDead);
        }

        private bool AmIOneOfTheClosestToOwnFlagCarrier(CtfBlackboard blackboard, int memberCount)
        {
            if (memberCount <= 0 || blackboard.MyTeamFlagCarrier == null)
            {
                return false;
            }

            // check whether i'm part of the closest x (memberCount) members to the flag carrier
            int index = Bot.Objects.Partymembers
                            .OfType<IWowPlayer>()
                            .Where(e => e.Guid != blackboard.MyTeamFlagCarrier.Guid)
                            .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                            .Select((player, id) => new { Player = player, Index = id })
                            .FirstOrDefault(_ => _.Player.Guid == Bot.Wow.PlayerGuid)?.Index ?? -1;

            return index > -1 && index <= memberCount;
        }

        private BtStatus AttackNearWeakestEnemy(CtfBlackboard blackboard)
        {
            IWowPlayer weakestPlayer = Bot.GetNearEnemies<IWowPlayer>(Bot.Player.Position, 20.0f).OrderBy(e => e.Health).FirstOrDefault();

            if (weakestPlayer != null)
            {
                float distance = weakestPlayer.Position.GetDistance(Bot.Player.Position);
                float threshold = Bot.CombatClass.IsMelee ? 3.0f : 28.0f;

                if (distance > threshold)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, weakestPlayer.Position);
                }
                else if (Bot.Player.TargetGuid != 0)
                {
                    WowUnitReaction reaction = Bot.Wow.GetReaction(Bot.Target.BaseAddress, Bot.Player.BaseAddress);

                    if (reaction is WowUnitReaction.Hostile or WowUnitReaction.Neutral)
                    {
                        Bot.CombatClass.Execute();
                    }
                    else
                    {
                        Bot.Wow.ChangeTarget(weakestPlayer.Guid);
                    }
                }
                else
                {
                    Bot.Wow.ChangeTarget(weakestPlayer.Guid);
                }
            }
            else
            {
                return BtStatus.Failed;
            }

            return BtStatus.Ongoing;
        }

        private BtStatus DefendOwnBase(CtfBlackboard blackboard)
        {
            double distance = Bot.Player.Position.GetDistance(WsgDataset.OwnBasePosition);

            if (distance > 16.0)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, WsgDataset.OwnBasePosition);
            }
            else
            {
                IWowUnit nearEnemy = Bot.GetNearEnemies<IWowUnit>(WsgDataset.OwnBasePosition, 16.0f).FirstOrDefault();

                if (nearEnemy != null)
                {
                    float distanceToEnemy = Bot.Player.Position.GetDistance(nearEnemy.Position);

                    if (distanceToEnemy > 2.0f)
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Move, nearEnemy.Position);
                    }
                    else if (Bot.Player.TargetGuid != 0)
                    {
                        WowUnitReaction reaction = Bot.Wow.GetReaction(Bot.Target.BaseAddress, Bot.Player.BaseAddress);

                        if (reaction is WowUnitReaction.Hostile or WowUnitReaction.Neutral)
                        {
                            Bot.CombatClass.Execute();
                        }
                        else
                        {
                            Bot.Wow.ChangeTarget(nearEnemy.Guid);
                        }
                    }
                    else
                    {
                        Bot.Wow.ChangeTarget(nearEnemy.Guid);
                    }
                }
            }
            return BtStatus.Ongoing;
        }

        private bool DoWeOutnumberOurEnemies(CtfBlackboard blackboard)
        {
            if (blackboard.MyTeamFlagCarrier != null && blackboard.MyTeamFlagCarrier.Guid == Bot.Wow.PlayerGuid)
            {
                return false;
            }

            int friends = Bot.GetNearFriends<IWowPlayer>(Bot.Player.Position, 18.0f).Count();
            int enemies = Bot.GetNearEnemies<IWowPlayer>(Bot.Player.Position, 18.0f).Count();

            return enemies > 0 && friends >= enemies;
        }

        private bool EnemiesNearFlagCarrier(CtfBlackboard blackboard)
        {
            return blackboard.MyTeamFlagCarrier.Position.GetDistance(Bot.Player.Position) < 32.0;
        }

        private BtStatus FleeFromComingEnemies()
        {
            IWowUnit nearestEnemy = Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, 48.0f).OrderBy(e => e.Position.GetDistance(Bot.Player.Position)).FirstOrDefault();

            if (nearestEnemy != null)
            {
                Bot.Movement.SetMovementAction(MovementAction.Flee, nearestEnemy.Position, nearestEnemy.Rotation);
                return BtStatus.Ongoing;
            }
            else
            {
                return BtStatus.Success;
            }
        }

        private bool IsAnyBuffNearMe(float distance)
        {
            return Bot.GetClosestGameObjectByDisplayId(Bot.Player.Position, new List<int>() { 5991, 5995, 5931 })?.Position.GetDistance(Bot.Player.Position) < distance;
        }

        private bool IsFlagNear()
        {
            return JBgBlackboard.NearFlags != null && JBgBlackboard.NearFlags.Any(e => e.Position.GetDistance(Bot.Player.Position) < 8.0);
        }

        private bool IsGateOpen()
        {
            if (Bot.Player.IsAlliance())
            {
                IWowGameobject obj = Bot.Objects.WowObjects.OfType<IWowGameobject>()
                                    .FirstOrDefault(e => e.GameObjectType == WowGameObjectType.Door && e.DisplayId == 411);

                return obj == null || obj.Bytes0 == 0;
            }
            else
            {
                IWowGameobject obj = Bot.Objects.WowObjects.OfType<IWowGameobject>()
                                    .FirstOrDefault(e => e.GameObjectType == WowGameObjectType.Door && e.DisplayId == 850);

                return obj == null || obj.Bytes0 == 0;
            }
        }

        private BtStatus KillEnemyFlagCarrier(CtfBlackboard blackboard)
        {
            if (JBgBlackboard.EnemyTeamFlagCarrier == null)
            {
                return BtStatus.Success;
            }

            float distance = Bot.Player.Position.GetDistance(JBgBlackboard.EnemyTeamFlagCarrier.Position);
            float threshold = Bot.CombatClass.IsMelee ? 3.0f : 28.0f;

            if (distance > threshold && !Bot.Player.IsCasting)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, BotUtils.MoveAhead(JBgBlackboard.EnemyTeamFlagCarrier.Position, JBgBlackboard.EnemyTeamFlagCarrier.Rotation, 1.0f));
            }
            else if (Bot.Player.TargetGuid != 0)
            {
                WowUnitReaction reaction = Bot.Wow.GetReaction(Bot.Target.BaseAddress, Bot.Player.BaseAddress);

                if (reaction is WowUnitReaction.Hostile or WowUnitReaction.Neutral)
                {
                    Bot.CombatClass.Execute();
                }
                else
                {
                    Bot.Wow.ChangeTarget(JBgBlackboard.EnemyTeamFlagCarrier.Guid);
                }
            }
            else
            {
                Bot.Wow.ChangeTarget(JBgBlackboard.EnemyTeamFlagCarrier.Guid);
            }

            return BtStatus.Ongoing;
        }

        private BtStatus MoveToEnemyBaseAndGetFlag(CtfBlackboard blackboard)
        {
            float distance = Bot.Player.Position.GetDistance(WsgDataset.EnemyBasePosition);

            if (distance > 2.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, WsgDataset.EnemyBasePosition);
            }
            else if (JBgBlackboard.NearFlags != null)
            {
                return UseNearestFlag(blackboard);
            }

            return BtStatus.Ongoing;
        }

        private BtStatus MoveToNearestBuff(CtfBlackboard blackboard)
        {
            IWowGameobject buffObject = Bot.GetClosestGameObjectByDisplayId(Bot.Player.Position, new List<int>() { 5991, 5995, 5931 });

            if (buffObject != null
                && buffObject.Position.GetDistance(Bot.Player.Position) > 3.0)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, buffObject.Position);
            }
            else
            {
                return BtStatus.Failed;
            }

            return BtStatus.Ongoing;
        }

        private BtStatus MoveToOwnFlagCarrierAndHelp()
        {
            if (JBgBlackboard.MyTeamFlagCarrier == null)
            {
                return BtStatus.Failed;
            }

            float distance = Bot.Player.Position.GetDistance(JBgBlackboard.MyTeamFlagCarrier.Position);

            if (distance > 4.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, JBgBlackboard.MyTeamFlagCarrier.Position);
            }
            else
            {
                IWowUnit nearEnemy = Bot.GetNearEnemies<IWowUnit>(JBgBlackboard.MyTeamFlagCarrier.Position, 32.0f).FirstOrDefault();

                if (nearEnemy != null)
                {
                    float distanceToEnemy = Bot.Player.Position.GetDistance(nearEnemy.Position);
                    float threshold = Bot.CombatClass.IsMelee ? 3.0f : 28.0f;

                    if (distanceToEnemy > threshold)
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Move, nearEnemy.Position);
                    }
                    else if (Bot.Player.TargetGuid != 0)
                    {
                        WowUnitReaction reaction = Bot.Wow.GetReaction(Bot.Target.BaseAddress, Bot.Player.BaseAddress);

                        if (reaction is WowUnitReaction.Hostile or WowUnitReaction.Neutral)
                        {
                            Bot.CombatClass.Execute();
                        }
                        else
                        {
                            Bot.Wow.ChangeTarget(nearEnemy.Guid);
                        }
                    }
                    else
                    {
                        Bot.Wow.ChangeTarget(nearEnemy.Guid);
                    }
                }
            }

            return BtStatus.Ongoing;
        }

        private BtStatus MoveToPosition(Vector3 position, float minDistance = 2.5f)
        {
            double distance = Bot.Player.Position.GetDistance(position);

            if (distance > minDistance)
            {
                float zDiff = position.Z - Bot.Player.Position.Z;

                if (LosCheckEvent.Run())
                {
                    InLos = Bot.Wow.IsInLineOfSight(Bot.Player.Position, position, 2.0f);
                }

                if (zDiff < -4.0 && InLos) // target is below us and in line of sight, just run down
                {
                    Bot.Movement.SetMovementAction(MovementAction.DirectMove, position);
                }
                else
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, position);
                }

                return BtStatus.Ongoing;
            }
            else
            {
                return BtStatus.Success;
            }
        }

        private void UpdateBattlegroundInfo()
        {
            try
            {
                if (Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=\"{{\"_,stateA,textA,_,_,_,_,_,_,_,_,_=GetWorldStateUIInfo(2)_,stateH,textH,_,_,_,_,_,_,_,_,_=GetWorldStateUIInfo(3)flagXA,flagYA=GetBattlefieldFlagPosition(1)flagXH,flagYH=GetBattlefieldFlagPosition(2){{v:0}}={{v:0}}..\"\\\"allianceState\\\" : \\\"\"..stateA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"allianceText\\\" : \\\"\"..textA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeState\\\" : \\\"\"..stateH..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeText\\\" : \\\"\"..textH..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"allianceFlagX\\\" : \\\"\"..flagXA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"allianceFlagY\\\" : \\\"\"..flagYA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeFlagX\\\" : \\\"\"..flagXH..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeFlagY\\\" : \\\"\"..flagYH..\"\\\"\"{{v:0}}={{v:0}}..\"}}\""), out string result))
                {
                    Dictionary<string, dynamic> bgState = JsonSerializer.Deserialize<JsonElement>(result, new() { AllowTrailingCommas = true, NumberHandling = JsonNumberHandling.AllowReadingFromString }).ToDyn();

                    string[] splittedScoreH = ((string)bgState["hordeText"]).Split('/');
                    string[] splittedScoreA = ((string)bgState["allianceText"]).Split('/');

                    float allianceFlagX = float.Parse((string)bgState["allianceFlagX"], NumberStyles.Any) * 100.0f;
                    float allianceFlagY = float.Parse((string)bgState["allianceFlagY"], NumberStyles.Any) * 100.0f;

                    float hordeFlagX = float.Parse((string)bgState["hordeFlagX"], NumberStyles.Any) * 100.0f;
                    float hordeFlagY = float.Parse((string)bgState["hordeFlagY"], NumberStyles.Any) * 100.0f;

                    if (Bot.Player.IsAlliance())
                    {
                        JBgBlackboard.MyTeamScore = int.Parse(splittedScoreA[0]);
                        JBgBlackboard.MyTeamMaxScore = int.Parse(splittedScoreA[1]);

                        JBgBlackboard.EnemyTeamScore = int.Parse(splittedScoreH[0]);
                        JBgBlackboard.EnemyTeamMaxScore = int.Parse(splittedScoreH[1]);

                        JBgBlackboard.MyTeamHasFlag = int.Parse((string)bgState["allianceState"]) == 2;
                        JBgBlackboard.EnemyTeamHasFlag = int.Parse((string)bgState["hordeState"]) == 2;

                        JBgBlackboard.MyTeamFlagPos = new(allianceFlagX, allianceFlagY, 0.0f);
                        JBgBlackboard.EnemyTeamFlagPos = new(hordeFlagX, hordeFlagY, 0.0f);

                        JBgBlackboard.MyTeamFlagCarrier = Bot.Objects.WowObjects.OfType<IWowPlayer>().FirstOrDefault(e => e.HasBuffById(23333));
                        JBgBlackboard.EnemyTeamFlagCarrier = Bot.Objects.WowObjects.OfType<IWowPlayer>().FirstOrDefault(e => e.HasBuffById(23335));
                    }
                    else
                    {
                        JBgBlackboard.MyTeamScore = int.Parse(splittedScoreH[0]);
                        JBgBlackboard.MyTeamMaxScore = int.Parse(splittedScoreH[1]);

                        JBgBlackboard.EnemyTeamScore = int.Parse(splittedScoreA[0]);
                        JBgBlackboard.EnemyTeamMaxScore = int.Parse(splittedScoreA[1]);

                        JBgBlackboard.MyTeamHasFlag = int.Parse((string)bgState["hordeState"]) == 2;
                        JBgBlackboard.EnemyTeamHasFlag = int.Parse((string)bgState["allianceState"]) == 2;

                        JBgBlackboard.MyTeamFlagPos = new(hordeFlagX, hordeFlagY, 0.0f);
                        JBgBlackboard.EnemyTeamFlagPos = new(allianceFlagX, allianceFlagY, 0.0f);

                        JBgBlackboard.MyTeamFlagCarrier = Bot.Objects.WowObjects.OfType<IWowPlayer>().FirstOrDefault(e => e.HasBuffById(23335));
                        JBgBlackboard.EnemyTeamFlagCarrier = Bot.Objects.WowObjects.OfType<IWowPlayer>().FirstOrDefault(e => e.HasBuffById(23333));
                    }

                    JBgBlackboard.NearFlags = Bot.Objects.WowObjects.OfType<IWowGameobject>()
                                                 .Where(e => e.DisplayId == (int)WowGameObjectDisplayId.WsgAllianceFlag
                                                          || e.DisplayId == (int)WowGameObjectDisplayId.WsgHordeFlag);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private BtStatus UseNearestFlag(CtfBlackboard blackboard)
        {
            IWowGameobject nearestFlag = JBgBlackboard.NearFlags.OrderBy(e => e.Position.GetDistance(Bot.Player.Position)).FirstOrDefault();

            if (nearestFlag != null)
            {
                float distance = Bot.Player.Position.GetDistance(nearestFlag.Position);

                if (distance > 4.0f)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, nearestFlag.Position);
                }
                else if (ActionEvent.Run())
                {
                    Bot.Wow.InteractWithObject(nearestFlag.BaseAddress);
                }
            }
            else
            {
                return BtStatus.Failed;
            }

            return BtStatus.Ongoing;
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