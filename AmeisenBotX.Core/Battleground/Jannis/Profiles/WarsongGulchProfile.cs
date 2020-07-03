using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
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
        public WarsongGulchProfile(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            ActionEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));

            JBgBlackboard = new JBgBlackboard(UpdateBattlegroundInfo);

            KillEnemyFlagCarrierSelector = new Selector<JBgBlackboard>
            (
                "EnemyTeamFlagCarrierInRange",
                (b) => b.EnemyTeamFlagCarrier != null,
                new Selector<JBgBlackboard>
                (
                    "HasFlag",
                    (b) => b.MyTeamHasFlag
                        || b.EnemyTeamFlagCarrier.Position.GetDistance(WsgDataset.EnemyBasePosition)
                         < WowInterface.ObjectManager.Player.Position.GetDistance(WsgDataset.EnemyBasePosition),
                    new Leaf<JBgBlackboard>("KillEnemyFlagCarrier", KillEnemyFlagCarrier),
                    new Leaf<JBgBlackboard>("MoveToEnemyBaseAndGetFlag", MoveToEnemyBaseAndGetFlag)
                ),
                new Selector<JBgBlackboard>
                (
                    "IsFlagNearOwnOrEnemyBase",
                    (b) => b.EnemyTeamFlagPos.GetDistance2D(WsgDataset.EnemyBasePositionMapCoords)
                         < b.EnemyTeamFlagPos.GetDistance2D(WsgDataset.OwnBasePositionMapCoords),
                    new Leaf<JBgBlackboard>("MoveToEnemyBase", (b) => MoveToPosition(WsgDataset.EnemyBasePosition)),
                    new Leaf<JBgBlackboard>("MoveToOwnBase", (b) => MoveToPosition(WsgDataset.OwnBasePosition))
                )
            );

            FlagSelector = new DualSelector<JBgBlackboard>
            (
                "WhoHasTheFlag",
                (b) => b.MyTeamHasFlag,
                (b) => b.EnemyTeamHasFlag,

                // no team has the flag
                new Leaf<JBgBlackboard>("MoveToEnemyBaseAndGetFlag", MoveToEnemyBaseAndGetFlag),

                // only my team has the flag
                new Selector<JBgBlackboard>
                (
                    "AmITheFlagCarrier",
                    (b) => b.MyTeamFlagCarrier != null && b.MyTeamFlagCarrier.Guid == WowInterface.ObjectManager.PlayerGuid,
                    new Leaf<JBgBlackboard>("MoveToOwnBase", (b) => MoveToPosition(WsgDataset.OwnBasePosition)),
                    new Selector<JBgBlackboard>
                    (
                        "IsTheFlagCarrierInRange",
                        (b) => b.MyTeamFlagCarrier != null,
                        new Leaf<JBgBlackboard>("MoveToOwnFlagCarrier", (b) => MoveToPosition(b.MyTeamFlagCarrier.Position)),
                        new Leaf<JBgBlackboard>("DefendOwnBase", DefendOwnBase)
                    )
                ),

                // only enemy team has the flag
                KillEnemyFlagCarrierSelector,

                // both teams have the flag
                new Selector<JBgBlackboard>
                (
                    "AmITheFlagCarrier",
                    (b) => b.MyTeamFlagCarrier != null && b.MyTeamFlagCarrier.Guid == WowInterface.ObjectManager.PlayerGuid,
                    new Selector<JBgBlackboard>
                    (
                        "AmINearOwnBase",
                        (b) => WowInterface.ObjectManager.Player.Position.GetDistance(WsgDataset.OwnBasePosition) < 128.0,
                        new Leaf<JBgBlackboard>("MoveToHidingSpot", (b) => MoveToPosition(WsgDataset.FlagHidingSpot)),
                        new Leaf<JBgBlackboard>("MoveToOwnBase", (b) => MoveToPosition(WsgDataset.OwnBasePosition))
                    ),
                    KillEnemyFlagCarrierSelector
                )
            );

            MainSelector = new Selector<JBgBlackboard>
            (
                "IsGateOpen",
                (b) => IsGateOpen(),
                 new Selector<JBgBlackboard>
                 (
                     "IsFlagNear",
                     (b) => IsFlagNear(),
                     new Leaf<JBgBlackboard>("UseNearestFlag", UseNearestFlag),
                     new Selector<JBgBlackboard>
                     (
                         "IsAnyBuffNearMe",
                         (b) => IsAnyBuffNearMe(16.0),
                         new Leaf<JBgBlackboard>("MoveToNearestBuff", MoveToNearestBuff),
                         new Selector<JBgBlackboard>
                         (
                             "DoWeOutnumberOurEnemies",
                             (b) => DoWeOutnumberOurEnemies(b),
                             new Leaf<JBgBlackboard>("AttackNearWeakestEnemy", AttackNearWeakestEnemy),
                             FlagSelector
                         )
                     )
                 ),
                 new Leaf<JBgBlackboard>("MoveToGatePosition", (b) => MoveToPosition(WsgDataset.GatePosition))
            );

            BehaviorTree = new AmeisenBotBehaviorTree<JBgBlackboard>
            (
                "JBgWarsongGulchBehaviorTree",
                MainSelector,
                JBgBlackboard,
                TimeSpan.FromSeconds(1)
            );
        }

        public interface IWsgDataset
        {
            Vector3 EnemyBasePosition { get; }

            Vector3 EnemyBasePositionMapCoords { get; }

            Vector3 FlagHidingSpot { get; }

            Vector3 GatePosition { get; }

            Vector3 OwnBasePosition { get; }

            Vector3 OwnBasePositionMapCoords { get; }
        }

        public AmeisenBotBehaviorTree<JBgBlackboard> BehaviorTree { get; }

        public DualSelector<JBgBlackboard> FlagSelector { get; }

        public JBgBlackboard JBgBlackboard { get; set; }

        public Selector<JBgBlackboard> MainSelector { get; }

        public IWsgDataset WsgDataset { get; set; }

        private TimegatedEvent ActionEvent { get; }

        private Selector<JBgBlackboard> KillEnemyFlagCarrierSelector { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (WsgDataset == null)
            {
                if (WowInterface.ObjectManager.Player.IsAlliance())
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

        private bool AmINearOwnFlagCarrier(JBgBlackboard blackboard)
        {
            return WowInterface.ObjectManager.GetNearEnemies<WowUnit>(blackboard.MyTeamFlagCarrier.Position, 48.0).Count(e => !e.IsDead) > 0;
        }

        private bool AmIOneOfTheClosestToOwnFlagCarrier(JBgBlackboard blackboard, int memberCount)
        {
            if (memberCount <= 0 || blackboard.MyTeamFlagCarrier == null)
            {
                return false;
            }

            // check wether i'm part of the closest x (memberCount) members to the flag carrier
            int index = WowInterface.ObjectManager.Partymembers
                            .OfType<WowPlayer>()
                            .Where(e => e.Guid != blackboard.MyTeamFlagCarrier.Guid)
                            .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                            .Select((player, id) => new { Player = player, Index = id })
                            .FirstOrDefault(_ => _.Player.Guid == WowInterface.ObjectManager.PlayerGuid)?.Index ?? -1;

            return index > -1 && index <= memberCount;
        }

        private BehaviorTreeStatus AttackNearWeakestEnemy(JBgBlackboard blackboard)
        {
            WowPlayer weakestPlayer = WowInterface.ObjectManager.GetNearEnemies<WowPlayer>(WowInterface.ObjectManager.Player.Position, 20.0).OrderBy(e => e.Health).FirstOrDefault();

            if (weakestPlayer != null)
            {
                double distance = weakestPlayer.Position.GetDistance(WowInterface.ObjectManager.Player.Position);
                double threshold = WowInterface.CombatClass.IsMelee ? 3.0 : 28.0;

                if (distance > threshold)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, weakestPlayer.Position);
                }
                else if (ActionEvent.Run())
                {
                    WowInterface.Globals.ForceCombat = true;
                    WowInterface.HookManager.TargetGuid(weakestPlayer.Guid);
                }
            }
            else
            {
                return BehaviorTreeStatus.Failed;
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private BehaviorTreeStatus DefendOwnBase(JBgBlackboard blackboard)
        {
            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(WsgDataset.OwnBasePosition);

            if (distance > 16.0)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, WsgDataset.OwnBasePosition);
            }
            else
            {
                WowUnit nearEnemy = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WsgDataset.OwnBasePosition, 16.0).FirstOrDefault();

                if (nearEnemy != null)
                {
                    double distanceToEnemy = WowInterface.ObjectManager.Player.Position.GetDistance(nearEnemy.Position);

                    if (distanceToEnemy > 2.0)
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, nearEnemy.Position);
                    }
                    else if (ActionEvent.Run())
                    {
                        WowInterface.Globals.ForceCombat = true;
                        WowInterface.HookManager.TargetGuid(nearEnemy.Guid);
                    }
                }
            }
            return BehaviorTreeStatus.Ongoing;
        }

        private bool DoWeOutnumberOurEnemies(JBgBlackboard blackboard)
        {
            if (blackboard.MyTeamFlagCarrier != null && blackboard.MyTeamFlagCarrier.Guid == WowInterface.ObjectManager.PlayerGuid)
            {
                return false;
            }

            int friends = WowInterface.ObjectManager.GetNearFriends<WowPlayer>(WowInterface.ObjectManager.Player.Position, 18.0).Count;
            int enemies = WowInterface.ObjectManager.GetNearEnemies<WowPlayer>(WowInterface.ObjectManager.Player.Position, 18.0).Count;

            return enemies > 0 && friends >= enemies;
        }

        private bool EnemiesNearFlagCarrier(JBgBlackboard blackboard)
        {
            return blackboard.MyTeamFlagCarrier.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 32.0;
        }

        private BehaviorTreeStatus FleeFromComingEnemies(JBgBlackboard blackboard)
        {
            WowUnit nearestEnemy = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 48.0).OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).FirstOrDefault();

            if (nearestEnemy != null)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Fleeing, nearestEnemy.Position, nearestEnemy.Rotation);
                return BehaviorTreeStatus.Ongoing;
            }
            else
            {
                return BehaviorTreeStatus.Success;
            }
        }

        private bool IsAnyBuffNearMe(double distance)
        {
            return WowInterface.ObjectManager.GetClosestWowGameobjectByDisplayId(new List<int>() { 5991, 5995, 5931 })?.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < distance;
        }

        private bool IsFlagNear()
        {
            return JBgBlackboard.NearFlags != null && JBgBlackboard.NearFlags.Any(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 8.0);
        }

        private bool IsGateOpen()
        {
            if (WowInterface.ObjectManager.Player.IsAlliance())
            {
                WowGameobject obj = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                                    .Where(e => e.GameobjectType == WowGameobjectType.Door && e.DisplayId == 411)
                                    .FirstOrDefault();

                return obj == null || obj.Bytes0 == 0;
            }
            else
            {
                WowGameobject obj = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                                    .Where(e => e.GameobjectType == WowGameobjectType.Door && e.DisplayId == 850)
                                    .FirstOrDefault();

                return obj == null || obj.Bytes0 == 0;
            }
        }

        private BehaviorTreeStatus KillEnemyFlagCarrier(JBgBlackboard blackboard)
        {
            if (JBgBlackboard.EnemyTeamFlagCarrier == null)
            {
                WowInterface.Globals.ForceCombat = false;
                return BehaviorTreeStatus.Success;
            }

            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(JBgBlackboard.EnemyTeamFlagCarrier.Position);
            double threshold = WowInterface.CombatClass.IsMelee ? 3.0 : 28.0;

            if (distance > threshold && !WowInterface.ObjectManager.Player.IsCasting)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, BotUtils.MoveAhead(JBgBlackboard.EnemyTeamFlagCarrier.Position, JBgBlackboard.EnemyTeamFlagCarrier.Rotation, 1.0));
            }
            else if (ActionEvent.Run())
            {
                WowInterface.Globals.ForceCombat = true;
                WowInterface.HookManager.TargetGuid(JBgBlackboard.EnemyTeamFlagCarrier.Guid);
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private BehaviorTreeStatus MoveToEnemyBaseAndGetFlag(JBgBlackboard blackboard)
        {
            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(WsgDataset.EnemyBasePosition);

            if (distance > 2.0)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, WsgDataset.EnemyBasePosition);
            }
            else
            {
                return UseNearestFlag(blackboard);
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private BehaviorTreeStatus MoveToNearestBuff(JBgBlackboard blackboard)
        {
            WowGameobject buffObject = WowInterface.ObjectManager.GetClosestWowGameobjectByDisplayId(new List<int>() { 5991, 5995, 5931 });

            if (buffObject != null
                && buffObject.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 3.0)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, buffObject.Position);
            }
            else
            {
                return BehaviorTreeStatus.Failed;
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private BehaviorTreeStatus MoveToOwnFlagCarrierAndHelp(JBgBlackboard blackboard)
        {
            if (JBgBlackboard.MyTeamFlagCarrier == null)
            {
                return BehaviorTreeStatus.Failed;
            }

            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(JBgBlackboard.MyTeamFlagCarrier.Position);

            if (distance > 4.0)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, JBgBlackboard.MyTeamFlagCarrier.Position);
            }
            else
            {
                WowUnit nearEnemy = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(JBgBlackboard.MyTeamFlagCarrier.Position, 32.0).FirstOrDefault();

                if (nearEnemy != null)
                {
                    double distanceToEnemy = WowInterface.ObjectManager.Player.Position.GetDistance(nearEnemy.Position);
                    double threshold = WowInterface.CombatClass.IsMelee ? 3.0 : 28.0;

                    if (distanceToEnemy > threshold)
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, nearEnemy.Position);
                    }
                    else if (ActionEvent.Run())
                    {
                        WowInterface.Globals.ForceCombat = true;
                        WowInterface.HookManager.TargetGuid(nearEnemy.Guid);
                    }
                }
            }

            return BehaviorTreeStatus.Ongoing;
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

        private void UpdateBattlegroundInfo()
        {
            try
            {
                string result = WowInterface.HookManager.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=\"{{\"_,stateA,textA,_,_,_,_,_,_,_,_,_=GetWorldStateUIInfo(2)_,stateH,textH,_,_,_,_,_,_,_,_,_=GetWorldStateUIInfo(3)flagXA,flagYA=GetBattlefieldFlagPosition(1)flagXH,flagYH=GetBattlefieldFlagPosition(2){{v:0}}={{v:0}}..\"\\\"allianceState\\\" : \\\"\"..stateA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"allianceText\\\" : \\\"\"..textA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeState\\\" : \\\"\"..stateH..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeText\\\" : \\\"\"..textH..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"allianceFlagX\\\" : \\\"\"..flagXA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"allianceFlagY\\\" : \\\"\"..flagYA..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeFlagX\\\" : \\\"\"..flagXH..\"\\\",\"{{v:0}}={{v:0}}..\"\\\"hordeFlagY\\\" : \\\"\"..flagYH..\"\\\"\"{{v:0}}={{v:0}}..\"}}\""));
                dynamic bgState = JsonConvert.DeserializeObject(result);

                if (WowInterface.ObjectManager.Player.IsAlliance())
                {
                    string[] splittedScoreA = ((string)bgState.allianceText).Split('/');
                    JBgBlackboard.MyTeamScore = int.Parse(splittedScoreA[0]);
                    JBgBlackboard.MyTeamMaxScore = int.Parse(splittedScoreA[1]);

                    string[] splittedScoreH = ((string)bgState.hordeText).Split('/');
                    JBgBlackboard.EnemyTeamScore = int.Parse(splittedScoreH[0]);
                    JBgBlackboard.EnemyTeamMaxScore = int.Parse(splittedScoreH[1]);

                    JBgBlackboard.MyTeamHasFlag = int.Parse((string)bgState.allianceState) == 2;
                    JBgBlackboard.EnemyTeamHasFlag = int.Parse((string)bgState.hordeState) == 2;

                    JBgBlackboard.MyTeamFlagPos = new Vector3
                    (
                        float.Parse((string)bgState.allianceFlagX, NumberStyles.Any, CultureInfo.InvariantCulture) * 100f,
                        float.Parse((string)bgState.allianceFlagY, NumberStyles.Any, CultureInfo.InvariantCulture) * 100f,
                        0f
                    );

                    JBgBlackboard.EnemyTeamFlagPos = new Vector3
                    (
                        float.Parse((string)bgState.hordeFlagX, NumberStyles.Any, CultureInfo.InvariantCulture) * 100f,
                        float.Parse((string)bgState.hordeFlagY, NumberStyles.Any, CultureInfo.InvariantCulture) * 100f,
                        0f
                    );

                    JBgBlackboard.MyTeamFlagCarrier = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.HasBuffById(23333));
                    JBgBlackboard.EnemyTeamFlagCarrier = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.HasBuffById(23335));
                }
                else
                {
                    string[] splittedScoreH = ((string)bgState.hordeText).Split('/');
                    JBgBlackboard.MyTeamScore = int.Parse(splittedScoreH[0]);
                    JBgBlackboard.MyTeamMaxScore = int.Parse(splittedScoreH[1]);

                    string[] splittedScoreA = ((string)bgState.allianceText).Split('/');
                    JBgBlackboard.EnemyTeamScore = int.Parse(splittedScoreA[0]);
                    JBgBlackboard.EnemyTeamMaxScore = int.Parse(splittedScoreA[1]);

                    JBgBlackboard.MyTeamHasFlag = int.Parse((string)bgState.hordeState) == 2;
                    JBgBlackboard.EnemyTeamHasFlag = int.Parse((string)bgState.allianceState) == 2;

                    JBgBlackboard.MyTeamFlagPos = new Vector3
                    (
                        float.Parse((string)bgState.hordeFlagX, NumberStyles.Any, CultureInfo.InvariantCulture) * 100f,
                        float.Parse((string)bgState.hordeFlagY, NumberStyles.Any, CultureInfo.InvariantCulture) * 100f,
                        0f
                    );

                    JBgBlackboard.EnemyTeamFlagPos = new Vector3
                    (
                        float.Parse((string)bgState.allianceFlagX, NumberStyles.Any, CultureInfo.InvariantCulture) * 100f,
                        float.Parse((string)bgState.allianceFlagY, NumberStyles.Any, CultureInfo.InvariantCulture) * 100f,
                        0f
                    );

                    JBgBlackboard.MyTeamFlagCarrier = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.HasBuffById(23335));
                    JBgBlackboard.EnemyTeamFlagCarrier = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.HasBuffById(23333));
                }

                JBgBlackboard.NearFlags = WowInterface.ObjectManager.WowObjects
                                              .OfType<WowGameobject>()
                                              .Where(e => e.DisplayId == (int)GameobjectDisplayId.WsgAllianceFlag || e.DisplayId == (int)GameobjectDisplayId.WsgHordeFlag)
                                              .ToList();
            }
            catch { }
        }

        private BehaviorTreeStatus UseNearestFlag(JBgBlackboard blackboard)
        {
            WowGameobject nearestFlag = JBgBlackboard.NearFlags.OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).FirstOrDefault();

            if (nearestFlag != null)
            {
                double distance = WowInterface.ObjectManager.Player.Position.GetDistance(nearestFlag.Position);

                if (distance > 4.0)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, nearestFlag.Position);
                }
                else if (ActionEvent.Run())
                {
                    WowInterface.HookManager.WowObjectOnRightClick(nearestFlag);
                }
            }
            else
            {
                return BehaviorTreeStatus.Failed;
            }

            return BehaviorTreeStatus.Ongoing;
        }

        public class AllianceWsgDataset : IWsgDataset
        {
            public Vector3 EnemyBasePosition { get; } = new Vector3(916, 1434, 346);

            public Vector3 EnemyBasePositionMapCoords { get; } = new Vector3(53, 90, 0);

            public Vector3 FlagHidingSpot { get; } = new Vector3(1519, 1467, 374);

            public Vector3 GatePosition { get; } = new Vector3(1494, 1457, 343);

            public Vector3 OwnBasePosition { get; } = new Vector3(1539, 1481, 352);

            public Vector3 OwnBasePositionMapCoords { get; } = new Vector3(49, 15, 0);
        }

        public class HordeWsgDataset : IWsgDataset
        {
            public Vector3 EnemyBasePosition { get; } = new Vector3(1539, 1481, 352);

            public Vector3 EnemyBasePositionMapCoords { get; } = new Vector3(49, 15, 0);

            public Vector3 FlagHidingSpot { get; } = new Vector3(949, 1449, 367);

            public Vector3 GatePosition { get; } = new Vector3(951, 1459, 342);

            public Vector3 OwnBasePosition { get; } = new Vector3(916, 1434, 346);

            public Vector3 OwnBasePositionMapCoords { get; } = new Vector3(53, 90, 0);
        }
    }
}