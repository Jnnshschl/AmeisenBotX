using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow548.Constants;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis.Mop548
{
    public class WarriorProtection : BasicCombatClass548
    {
        public WarriorProtection(AmeisenBotInterfaces bot) : base(bot)
        {
            Configurables.TryAdd("FartOnCharge", false);
        }

        public override string Description => "Beta CombatClass for the Protection Warrior spec. For Dungeons and Questing";

        public override string DisplayName2 => "Warrior Protection";

        public override bool HandlesMovement => true;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStaminaComparator
        (
            new()
            {
                WowArmorType.Cloth,
                WowArmorType.Leather
            },
            new()
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
            }
        );

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

        private DateTime LastFarted { get; set; }

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
                    if (TryCastSpell(Warrior548.HeroicThrow, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    if (TryCastSpellWarrior(Warrior548.Charge, Warrior548.DefensiveStance, Bot.Wow.TargetGuid, true))
                    {
                        if (Configurables["FartOnCharge"] && DateTime.Now - LastFarted > TimeSpan.FromSeconds(8))
                        {
                            LastFarted = DateTime.Now;
                            Bot.Wow.SendChatMessage($"/rude");
                        }

                        return;
                    }
                }
                else
                {
                    if (Bot.Player.HealthPercentage < 50.0
                        && TryCastSpell(Warrior548.ShieldBlock, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    if (Bot.Target.HealthPercentage < 20.0
                        && TryCastSpell(Warrior548.Execute, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    bool hasVictoriousBuff = Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Warrior548.Victorious);

                    if (hasVictoriousBuff
                        && Bot.Player.HealthPercentage < 80.0
                        && TryCastSpell(Warrior548.VictoryRush, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    if (Bot.Target.TargetGuid != Bot.Wow.PlayerGuid
                        && TryCastSpell(Warrior548.Taunt, Bot.Wow.TargetGuid))
                    {
                        return;
                    }

                    if (TryCastSpellWarrior(Warrior548.ShieldSlam, Warrior548.DefensiveStance, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    if (TryCastSpellWarrior(Warrior548.Revenge, Warrior548.DefensiveStance, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    // 60 rage is used because we want to still be able to instantly cast Execute
                    // below 30% hp
                    int rageToSave = Bot.Target.HealthPercentage < 30.0 ? 30 : 0;
                    int nearEnemies = Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, 10.0f).Count();

                    if ((nearEnemies > 2 || Bot.Player.Rage > (rageToSave + 30))
                        && TryCastSpell(Warrior548.ThunderClap, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    if (Bot.Player.Rage > (rageToSave + 15)
                        && !Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Warrior548.WeakenedAmor)
                        && (TryCastSpell(Warrior548.SunderAmor, Bot.Wow.TargetGuid, true)
                            || TryCastSpell(Warrior548.Devastate, Bot.Wow.TargetGuid, true)))
                    {
                        return;
                    }

                    if (Bot.Player.Rage > (30 + rageToSave) && TryCastSpell(Warrior548.HeroicStrike, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    // when we got nothing to do, use Victory Rush
                    if (hasVictoriousBuff
                        && TryCastSpell(Warrior548.VictoryRush, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }
    }
}