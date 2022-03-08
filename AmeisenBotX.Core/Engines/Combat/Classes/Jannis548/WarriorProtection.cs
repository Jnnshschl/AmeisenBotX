using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow548.Constants;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis548
{
    public class WarriorProtection : BasicCombatClass548
    {
        public WarriorProtection(AmeisenBotInterfaces bot) : base(bot)
        {
        }

        public override string Description => "Beta CombatClass for the Protection Warrior spec. For Dungeons and Questing";

        public override string DisplayName => "Warrior Protection 5.4.8";

        public override bool HandlesMovement => true;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStaminaComparator(new()
        {
            WowArmorType.Idol,
            WowArmorType.Libram,
            WowArmorType.Sigil,
            WowArmorType.Totem,
            WowArmorType.Cloth,
            WowArmorType.Leather
        }, new()
        {
            WowWeaponType.SwordTwoHand,
            WowWeaponType.MaceTwoHand,
            WowWeaponType.AxeTwoHand,
            WowWeaponType.Misc,
            WowWeaponType.Staff,
            WowWeaponType.Polearm,
            WowWeaponType.Thrown,
            WowWeaponType.Wand,
            WowWeaponType.Dagger
        });

        public override WowRole Role => WowRole.Tank;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
            },
            Tree2 = new()
            {
            },
            Tree3 = new()
            {
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.1";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Warrior;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderTank))
            {
                float distanceToTarget = Bot.Target.Position.GetDistance(Bot.Player.Position);

                if (!Bot.Tactic.PreventMovement)
                {
                    IWowUnit targetOfTarget = Bot.GetWowObjectByGuid<IWowUnit>(Bot.Target.TargetGuid);

                    if (targetOfTarget != null && targetOfTarget.Guid == Bot.Player.Guid)
                    {
                        // if we have aggro, pull the unit to the best tanking spot
                        Vector3 direction = Bot.Player.Position - Bot.Objects.CenterPartyPosition;
                        direction.Normalize();

                        Vector3 bestTankingSpot = Bot.Objects.CenterPartyPosition + (direction * 12.0f);
                        float distanceToBestTankingSpot = Bot.Player.DistanceTo(bestTankingSpot);

                        if (distanceToBestTankingSpot > 4.0f)
                        {
                            Bot.Movement.SetMovementAction(MovementAction.Move, Bot.Target.Position);
                        }
                    }
                    else
                    {
                        // target is not targeting us, we need to get aggro
                        if (distanceToTarget > Bot.Player.MeleeRangeTo(Bot.Target))
                        {
                            Bot.Movement.SetMovementAction(MovementAction.Chase, Bot.Target.Position);
                        }
                    }
                }

                if (distanceToTarget > 8.0)
                {
                    if (TryCastSpellWarrior(Warrior548.Charge, Warrior548.BattleStance, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }
                }
                else
                {
                    if (Bot.Player.Rage > 30)
                    {
                        TryCastSpell(Warrior548.HeroicStrike, Bot.Wow.TargetGuid, true);
                    }
                }
            }
        }
    }
}