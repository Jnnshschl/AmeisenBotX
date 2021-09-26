using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Engines.Grinding.Profiles;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Npc;
using AmeisenBotX.Core.Logic;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Grinding
{
    public class DefaultGrindEngine : IGrindingEngine
    {
        public DefaultGrindEngine(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            Bot = bot;
            Config = config;

            RootSelector = new Selector
            (
                () => Profile == null,
                new Leaf(ReportNoProfile),
                new Selector
                    (
                        () => Bot.Character.LastLevelTrained == 0,
                        new Leaf(InitLastTrainingLevel),
                        new Selector
                            (
                                () => NeedToRepair(),
                                new Leaf(GoToNpcAndRepair),
                                new Selector
                                (
                                    () => NeedToSell(),
                                    new Leaf(GoToNpcAndSell),
                                    new Selector
                                    (
                                        () => NeedToDismount(),
                                        new Leaf(Dismount),
                                        new Selector
                                        (
                                            () => NeedToTrainSpells(),
                                            new Leaf(GoToNpcAndTrain),
                                            new Selector
                                            (
                                                () => ThreatsNearby(),
                                                new Leaf(FightTarget),
                                                new Selector
                                                (
                                                    () => TargetsNearby(),
                                                    new Selector
                                                    (
                                                        () => SelectTarget(),
                                                        new Leaf(FightTarget),
                                                        new Leaf(() => BtStatus.Failed)
                                                    ),
                                                    new Leaf(MoveToNextGrindNode)
                                                )
                                            )
                                       )
                                  )
                              )
                         )
                    )
            );

            GrindingTree = new Tree
            (
                RootSelector
            );
        }

        public AmeisenBotInterfaces Bot { get; }

        public AmeisenBotConfig Config { get; }

        public IGrindingProfile Profile { get; set; }

        private Tree GrindingTree { get; }

        private Selector RootSelector { get; }

        private GrindingSpot NextSpot { get; set; } = new();

        private bool GoingToNextSpot { get; set; }

        private int CurrentSpotIndex { get; set; }

        public void Execute()
        {
            GrindingTree.Tick();
        }

        public void LoadProfile(IGrindingProfile profile)
        {
            Profile = profile;
        }

        private BtStatus ReportNoProfile()
        {
            //TODO: warn no profile
            return BtStatus.Failed;
        }

        private bool NeedToRepair()
        {
            return Bot.Character.Equipment.Items.Any(e => e.Value.MaxDurability > 0
                   && (e.Value.Durability / (double)e.Value.MaxDurability * 100.0) <= Config.ItemRepairThreshold);
        }

        private BtStatus GoToNpcAndRepair()
        {
            List<Vector3> repairNpcsPos = (
                from vendor in Profile.Vendors 
                where vendor.Type == NpcType.VendorRepair 
                select vendor.Position).ToList();

            Vector3 repairNpc = repairNpcsPos.OrderBy(e => e.GetDistance(Bot.Player.Position)).First();
            if (repairNpc.GetDistance(Bot.Player.Position) > 5.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, repairNpc);
                return BtStatus.Ongoing;
            }
            if (repairNpc.GetDistance(Bot.Player.Position) < 5.0f)
            {
                Bot.Movement.StopMovement();
            }

            return BtStatus.Success;
        }

        private bool NeedToSell()
        {
            return Bot.Character.Inventory.FreeBagSlots < Config.BagSlotsToGoSell;
        }

        private BtStatus GoToNpcAndSell()
        {
            List<Vendor> profileVendors = Profile.Vendors;
            if (!profileVendors.Any()) return BtStatus.Failed;

            Vendor firstVendor = profileVendors
                .Where(e => e.Type is NpcType.VendorSellBuy or NpcType.VendorRepair)
                .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                .FirstOrDefault();
            if (firstVendor == null) return BtStatus.Failed;

            if (firstVendor.Position.GetDistance(Bot.Player.Position) > 5.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, firstVendor.Position);
                return BtStatus.Ongoing;
            }
            if (firstVendor.Position.GetDistance(Bot.Player.Position) < 5.0f)
            {
                Bot.Movement.StopMovement();
            }

            return BtStatus.Success;
        }

        private bool NeedToDismount()
        {
            return Bot.Player.IsInCombat && Bot.Player.IsMounted;
        }

        private BtStatus Dismount()
        {
            Bot.Wow.DismissCompanion("MOUNT");
            return BtStatus.Success;
        }

        private bool NeedToTrainSpells()
        {
            return Bot.Character.LastLevelTrained != 0 && Bot.Character.LastLevelTrained < Bot.Player.Level;
        }

        private BtStatus GoToNpcAndTrain()
        {
            List<Trainer> profileTrainers = Profile.Trainers;
            if (!profileTrainers.Any()) return BtStatus.Failed;

            Trainer firstTrainer = profileTrainers
                .Where(e => e.Type == NpcType.ClassTrainer && e.SubType == AmeisenBotLogic.DecideClassTrainer(Bot.Player.Class))
                .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                .FirstOrDefault();
            if (firstTrainer == null) return BtStatus.Failed;

            if (firstTrainer.Position.GetDistance(Bot.Player.Position) > 5.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, firstTrainer.Position);
                return BtStatus.Ongoing;
            }
            if (firstTrainer.Position.GetDistance(Bot.Player.Position) < 5.0f)
            {
                Bot.Movement.StopMovement();
            }

            return BtStatus.Success;
        }

        private bool ThreatsNearby()
        {
            IEnumerable<IWowUnit> enemiesFightingMe = Bot.GetEnemiesInCombatWithMe<IWowUnit>(Bot.Player.Position, 40)
                .OrderBy(e => e.Position.GetDistance2D(Bot.Player.Position))
                .ToList();
            IEnumerable<IWowUnit> enemiesTargetingMe = Bot.GetEnemiesTargetingMe<IWowUnit>(Bot.Player.Position, 40)
                .OrderBy(e => e.Position.GetDistance2D(Bot.Player.Position))
                .ToList();
            IEnumerable<IWowUnit> enemiesAround = Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, 40)
                .OrderBy(e => e.Position.GetDistance2D(Bot.Player.Position))
                .ToList();

            if (enemiesFightingMe.Any())
            {
                Bot.Wow.ChangeTarget(enemiesFightingMe.FirstOrDefault().Guid);
                return true;
            }
            if (enemiesTargetingMe.Any())
            {
                Bot.Wow.ChangeTarget(enemiesTargetingMe.FirstOrDefault().Guid);
                return true;
            }
            if (enemiesAround.Any())
            {
                Bot.Wow.ChangeTarget(enemiesAround.FirstOrDefault().Guid);
                return true;
            }

            return false;
        }

        private bool TargetsNearby()
        {
            GrindingSpot nearestGrindSpot = Profile.Spots
                .Where(e => e.Position.GetDistance(Bot.Player.Position) <= e.Radius)
                .OrderBy(e => e.Position.GetDistance2D(Bot.Player.Position))
                .FirstOrDefault();
            if (nearestGrindSpot == null) return false;

            IEnumerable<IWowUnit> nearUnits = Bot.GetNearEnemiesOrNeutrals<IWowUnit>(nearestGrindSpot.Position, nearestGrindSpot.Radius)
                .Where(e => UnitWithinGrindSpotLvlLimit(e, nearestGrindSpot) && ObjectWithinGrindSpotRadius(e, nearestGrindSpot))
                .OrderBy(e => e.Position.GetDistance2D(Bot.Player.Position));

            return nearUnits.Any();
        }

        private bool SelectTarget()
        {
            if (Bot.Target != null) return true;

            GrindingSpot nearestGrindSpot = Profile.Spots
                .Where(e => e.Position.GetDistance(Bot.Player.Position) <= e.Radius)
                .OrderBy(e => e.Position.GetDistance2D(Bot.Player.Position)).FirstOrDefault();

            if (nearestGrindSpot == null) return false;

            IWowUnit possibleTarget = Bot.GetNearEnemiesOrNeutrals<IWowUnit>(nearestGrindSpot.Position, nearestGrindSpot.Radius)
                .Where(e => UnitWithinGrindSpotLvlLimit(e, nearestGrindSpot) && ObjectWithinGrindSpotRadius(e, nearestGrindSpot))
                .OrderBy(e => e.Position.GetDistance2D(Bot.Player.Position))
                .FirstOrDefault();

            if (possibleTarget == null) return false;

            Bot.Wow.ChangeTarget(possibleTarget.Guid);
            return true;
        }

        private BtStatus MoveToNextGrindNode()
        {
            Bot.CombatClass.OutOfCombatExecute();

            List<GrindingSpot> spots = Profile.Spots.Where(e =>
                Bot.Player.Level >= e.MinLevel && Bot.Player.Level <= e.MaxLevel)
                .ToList();

            if (spots.Count == 0)
                spots.AddRange(Profile.Spots.Where(e =>
                    e.MinLevel >= Profile.Spots.Max(e => e.MinLevel)));

            switch (Profile.RandomizeSpots)
            {
                case true when !GoingToNextSpot:
                {
                    Random rnd = new();
                    NextSpot = spots[rnd.Next(0, spots.Count)];
                    GoingToNextSpot = true;
                    break;
                }
                case false when !GoingToNextSpot:
                {
                    ++CurrentSpotIndex;

                    if (CurrentSpotIndex >= spots.Count)
                        CurrentSpotIndex = 0;

                    NextSpot = spots[CurrentSpotIndex];
                    GoingToNextSpot = true;
                    break;
                }
            }

            if (Bot.Player.Position.GetDistance(NextSpot.Position) < 5.0f)
            {
                GoingToNextSpot = false;
                NextSpot = new GrindingSpot();
                return BtStatus.Success;
            }
            if (Bot.Player.Position.GetDistance(NextSpot.Position) > 5.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, NextSpot.Position);
                return BtStatus.Ongoing;
            }

            return BtStatus.Success;
        }

        private BtStatus FightTarget()
        {
            if (Bot.Target == null) 
                return BtStatus.Failed;

            Bot.CombatClass.Execute();
            return BtStatus.Success;
        }

        private BtStatus InitLastTrainingLevel()
        {
            if (Bot.Character.LastLevelTrained != 0)
                return BtStatus.Failed;

            Bot.Character.LastLevelTrained = Bot.Player.Level;
            return BtStatus.Success;
        }

        private static bool UnitWithinGrindSpotLvlLimit(IWowUnit unit, GrindingSpot grindSpot)
        {
            return unit.Level >= grindSpot.MinLevel && unit.Level <= grindSpot.MaxLevel;
        }

        private static bool ObjectWithinGrindSpotRadius(IWowObject wowObject, GrindingSpot grindSpot)
        {
            return wowObject.Position.GetDistance(grindSpot.Position) <= grindSpot.Radius;
        }
    }
}