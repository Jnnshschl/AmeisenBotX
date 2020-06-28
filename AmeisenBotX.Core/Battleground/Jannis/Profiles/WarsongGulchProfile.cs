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

            JBgBlackboard = new JBgBlackboard();

            KillEnemyFlagCarrierSelector = new Selector<JBgBlackboard>
            (
                (b) => b.EnemyTeamFlagCarrier != null,

                // is he further away from flag than me
                new Selector<JBgBlackboard>
                (
                    (b) => b.MyTeamHasFlag || b.EnemyTeamFlagCarrier.Position.GetDistance(WsgDataset.EnemyBasePosition) < WowInterface.ObjectManager.Player.Position.GetDistance(WsgDataset.EnemyBasePosition),

                    // try to kill flag carrier
                    new Leaf<JBgBlackboard>
                    (
                        (b) =>
                        {
                            KillEnemyFlagCarrier();
                            return BehaviorTreeStatus.Success;
                        }
                    ),

                    // get the flag asap
                    new Leaf<JBgBlackboard>
                    (
                        (b) =>
                        {
                            MoveToEnemyBaseAndGetFlag();
                            return BehaviorTreeStatus.Success;
                        }
                    )
                ),

                new Selector<JBgBlackboard>
                (
                    (b) => b.EnemyTeamFlagPos.GetDistanceIgnoreZ(WsgDataset.EnemyBasePositionMapCoords) < b.EnemyTeamFlagPos.GetDistanceIgnoreZ(WsgDataset.OwnBasePositionMapCoords),

                    // go to enemy base to locate enemy flag carrier
                    new Leaf<JBgBlackboard>
                    (
                        (b) =>
                        {
                            MoveToEnemyBase();
                            return BehaviorTreeStatus.Success;
                        }
                    ),

                    // go to own base to locate enemy flag carrier
                    new Leaf<JBgBlackboard>
                    (
                        (b) =>
                        {
                            MoveToOwnBase();
                            return BehaviorTreeStatus.Success;
                        }
                    )
                )
            );

            FlagSelector = new DuoSelector<JBgBlackboard>
            (
                (b) => b.MyTeamHasFlag,
                (b) => b.EnemyTeamHasFlag,

                // only my team has the flag
                new Selector<JBgBlackboard>
                (
                    (b) => b.MyTeamFlagCarrier != null && b.MyTeamFlagCarrier.Guid == WowInterface.ObjectManager.PlayerGuid,

                    // i'm the flag carrier
                    new Leaf<JBgBlackboard>
                    (
                         (b) =>
                         {
                             MoveToOwnBase();
                             return BehaviorTreeStatus.Success;
                         }
                    ),

                    // help flag carrier or go to base
                    new Selector<JBgBlackboard>
                    (
                        (b) => b.MyTeamFlagCarrier != null,

                        // assist the carrier
                        new Leaf<JBgBlackboard>
                        (
                            (b) =>
                            {
                                MoveToOwnFlagCarrier();
                                return BehaviorTreeStatus.Success;
                            }
                        ),

                        // go to base to defend it
                        new Leaf<JBgBlackboard>
                        (
                            (b) =>
                            {
                                DefendOwnBase();
                                return BehaviorTreeStatus.Success;
                            }
                        )
                    )
                ),

                // only enemy team has the flag
                // if flag carrier is visible
                KillEnemyFlagCarrierSelector,

                // both teams have the flag
                // is me flag carrier
                new Selector<JBgBlackboard>
                (
                    (b) => b.MyTeamFlagCarrier != null && b.MyTeamFlagCarrier.Guid == WowInterface.ObjectManager.PlayerGuid,

                    new Selector<JBgBlackboard>
                    (
                        (b) => WowInterface.ObjectManager.GetNearEnemies<WowUnit>(b.MyTeamFlagCarrier.Position, 48.0).Count(e => !e.IsDead) > 0,

                        // am i close to the base
                        new Selector<JBgBlackboard>
                        (
                            (b) => WowInterface.ObjectManager.Player.Position.GetDistance(WsgDataset.OwnBasePosition) < 128.0,

                            // move to hiding spot
                            new Leaf<JBgBlackboard>
                            (
                                (b) =>
                                {
                                    MoveToOwnBaseHidingSpot();
                                    return BehaviorTreeStatus.Success;
                                }
                            ),

                            // move to own base
                            new Leaf<JBgBlackboard>
                            (
                                (b) =>
                                {
                                    MoveToOwnBase();
                                    return BehaviorTreeStatus.Success;
                                }
                            )
                        ),

                        // flee from enemies
                        new Leaf<JBgBlackboard>
                        (
                            (b) =>
                            {
                                FleeFromComingEnemies();
                                return BehaviorTreeStatus.Success;
                            }
                        )
                    ),

                    // do we need to help out carrier
                    new Selector<JBgBlackboard>
                    (
                        (b) => b.MyTeamFlagCarrier.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 32.0
                               && WowInterface.ObjectManager.GetNearEnemies<WowUnit>(b.MyTeamFlagCarrier.Position, 48.0).Count(e => !e.IsDead) > 0,

                        // go to own carrier and help
                        new Leaf<JBgBlackboard>
                        (
                            (b) =>
                            {
                                MoveToCarrierAndHelp();
                                return BehaviorTreeStatus.Success;
                            }
                        ),

                        // go to kill enemy flag carrier
                        KillEnemyFlagCarrierSelector
                    )
                ),

                // no team has the flag
                // go to enemy base and get flag
                new Leaf<JBgBlackboard>
                (
                    (b) =>
                    {
                        MoveToEnemyBaseAndGetFlag();
                        return BehaviorTreeStatus.Success;
                    }
                )
            );

            BehaviorTree = new AmeisenBotBehaviorTree<JBgBlackboard>
            (
                // is flag near
                new Selector<JBgBlackboard>
                (
                    (b) => IsFlagNear(),

                    // use the flag
                    new Leaf<JBgBlackboard>
                    (
                        (b) =>
                        {
                            UseNearestFlag();
                            return BehaviorTreeStatus.Success;
                        }
                    ),

                    // check for buffs
                    new Selector<JBgBlackboard>
                    (
                        (b) => IsAnyBuffNearMe(16.0),

                        // get the buff
                        new Leaf<JBgBlackboard>
                        (
                            (b) =>
                            {
                                MoveToNearestBuff();
                                return BehaviorTreeStatus.Success;
                            }
                        ),

                        // check for near enemies
                        new Selector<JBgBlackboard>
                        (
                            (b) =>
                            {
                                if (b.MyTeamFlagCarrier.Guid == WowInterface.ObjectManager.PlayerGuid)
                                {
                                    return false;
                                }

                                int friends = WowInterface.ObjectManager.GetNearFriends<WowPlayer>(WowInterface.ObjectManager.Player.Position, 18.0).Count;
                                int enemies = WowInterface.ObjectManager.GetNearEnemies<WowPlayer>(WowInterface.ObjectManager.Player.Position, 18.0).Count;

                                return enemies > 0 && friends > enemies;
                            },

                            // attack near enemies
                            new Leaf<JBgBlackboard>
                            (
                                (b) =>
                                {
                                    AttackNearWeakestEnemy();
                                    return BehaviorTreeStatus.Success;
                                }
                            ),

                            FlagSelector
                        )
                    )
                ),
                JBgBlackboard
            );

            RefreshScoreEvent = new TimegatedEvent(TimeSpan.FromSeconds(1), UpdateBattlegroundInfo);
            SetTargetEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
        }

        private void AttackNearWeakestEnemy()
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
        }

        public interface IWsgDataset
        {
            Vector3 EnemyBasePosition { get; }

            Vector3 EnemyBasePositionMapCoords { get; }

            Vector3 FlagHidingSpot { get; }

            Vector3 OwnBasePosition { get; }

            Vector3 OwnBasePositionMapCoords { get; }
        }

        public AmeisenBotBehaviorTree<JBgBlackboard> BehaviorTree { get; }

        public DuoSelector<JBgBlackboard> FlagSelector { get; }

        public JBgBlackboard JBgBlackboard { get; set; }

        public IWsgDataset WsgDataset { get; set; }

        private Selector<JBgBlackboard> KillEnemyFlagCarrierSelector { get; }

        private TimegatedEvent RefreshScoreEvent { get; }

        private TimegatedEvent SetTargetEvent { get; }

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

            if (RefreshScoreEvent.Run())
            {
                return;
            }

            BehaviorTree.Tick();
        }

        private void DefendOwnBase()
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
        }

        private void FleeFromComingEnemies()
        {
            WowUnit nearestEnemy = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 48.0).OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position)).FirstOrDefault();

            if (nearestEnemy != null)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Fleeing, nearestEnemy.Position, nearestEnemy.Rotation);
            }
        }

        private bool IsAnyBuffNearMe(double distance)
        {
            return WowInterface.ObjectManager.GetClosestWowGameobjectByDisplayId(new List<int>() { 5991, 5995, 5931 })?.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < distance;
        }

        private bool IsFlagNear()
        {
            return JBgBlackboard.NearFlags != null && JBgBlackboard.NearFlags.Any(e => e.Position.GetDistance(WsgDataset.OwnBasePosition) > 3.0 && e.Position.GetDistance(WsgDataset.EnemyBasePosition) > 3.0);
        }

        private void KillEnemyFlagCarrier()
        {
            if (JBgBlackboard.EnemyTeamFlagCarrier == null)
            {
                WowInterface.Globals.ForceCombat = false;
                return;
            }

            if (SetTargetEvent.Run())
            {
                WowInterface.HookManager.TargetGuid(JBgBlackboard.EnemyTeamFlagCarrier.Guid);
            }

            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(JBgBlackboard.EnemyTeamFlagCarrier.Position);
            double threshold = WowInterface.CombatClass.IsMelee ? 3.0 : 28.0;

            if (distance > threshold && !WowInterface.ObjectManager.Player.IsCasting)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, BotUtils.MoveAhead(JBgBlackboard.EnemyTeamFlagCarrier.Rotation, JBgBlackboard.EnemyTeamFlagCarrier.Position, 1.0));
            }
            else if (ActionEvent.Run())
            {
                WowInterface.Globals.ForceCombat = true;
                WowInterface.HookManager.TargetGuid(JBgBlackboard.EnemyTeamFlagCarrier.Guid);
            }
        }

        private void MoveToCarrierAndHelp()
        {
            if (JBgBlackboard.MyTeamFlagCarrier == null)
            {
                return;
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
        }

        private void MoveToEnemyBase()
        {
            WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, WsgDataset.EnemyBasePosition);
        }

        private void MoveToEnemyBaseAndGetFlag()
        {
            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(WsgDataset.EnemyBasePosition);

            if (distance > 4.0)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, WsgDataset.EnemyBasePosition);
            }
            else
            {
                UseNearestFlag();
            }
        }

        private void MoveToNearestBuff()
        {
            WowGameobject buffObject = WowInterface.ObjectManager.GetClosestWowGameobjectByDisplayId(new List<int>() { 5991, 5995, 5931 });

            if (buffObject != null
                && buffObject.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 3.0)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, buffObject.Position);
            }
        }

        private void MoveToOwnBase()
        {
            WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, WsgDataset.OwnBasePosition);
        }

        private void MoveToOwnBaseHidingSpot()
        {
            double distance = WowInterface.ObjectManager.Player.Position.GetDistance(WsgDataset.FlagHidingSpot);

            if (distance > 2.0)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, WsgDataset.FlagHidingSpot);
            }
        }

        private void MoveToOwnFlagCarrier()
        {
            WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, JBgBlackboard.MyTeamFlagCarrier.Position);
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

        private TimegatedEvent ActionEvent { get; }

        private void UseNearestFlag()
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
        }

        public class AllianceWsgDataset : IWsgDataset
        {
            public Vector3 EnemyBasePosition { get; } = new Vector3(916, 1434, 346);

            public Vector3 EnemyBasePositionMapCoords { get; } = new Vector3(53, 90, 0);

            public Vector3 FlagHidingSpot { get; } = new Vector3(1519, 1467, 374);

            public Vector3 OwnBasePosition { get; } = new Vector3(1539, 1481, 352);

            public Vector3 OwnBasePositionMapCoords { get; } = new Vector3(49, 15, 0);
        }

        public class HordeWsgDataset : IWsgDataset
        {
            public Vector3 EnemyBasePosition { get; } = new Vector3(1539, 1481, 352);

            public Vector3 EnemyBasePositionMapCoords { get; } = new Vector3(49, 15, 0);

            public Vector3 FlagHidingSpot { get; } = new Vector3(949, 1449, 367);

            public Vector3 OwnBasePosition { get; } = new Vector3(916, 1434, 346);

            public Vector3 OwnBasePositionMapCoords { get; } = new Vector3(53, 90, 0);
        }
    }
}