using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow548.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis.Mop548
{
    public class WarriorProtection : BasicCombatClass
    {
        public WarriorProtection(AmeisenBotInterfaces bot) : base(bot)
        {
            Configurables.TryAdd("FartOnCharge", false);

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(Warrior548.Pummel, x.Guid, true) },
                { 1, (x) => TryCastSpell(Warrior548.DragonRoar, x.Guid, true) },
            };
        }

        public override string Description => "Beta CombatClass for the Protection Warrior spec. For Dungeons and Questing";

        public override string DisplayName2 => "Warrior Protection";

        public override bool HandlesMovement => true;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStaminaComparator
        (
            null,
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

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Warrior;

        public override WowVersion WowVersion => WowVersion.MoP548;

        private DateTime LastFarted { get; set; }

        public override void Execute()
        {
            base.Execute();

            if (FindTarget(TargetProviderTank))
            {
                float distanceToTarget = Bot.Target.Position.GetDistance(Bot.Player.Position);

                if (!Bot.Tactic.PreventMovement)
                {
                    IWowUnit targetOfTarget = Bot.GetWowObjectByGuid<IWowUnit>(Bot.Target.TargetGuid);

                    if (targetOfTarget != null && targetOfTarget.Guid == Bot.Player.Guid)
                    {
                        // if we have aggro, pull the unit to the best tanking spot
                        if (Bot.Objects.Partymembers.Any())
                        {
                            Vector3 direction = Bot.Player.Position - Bot.Objects.CenterPartyPosition;
                            direction.Normalize();

                            Vector3 bestTankingSpot = Bot.Objects.CenterPartyPosition + (direction * 12.0f);

                            if (Bot.Player.DistanceTo(bestTankingSpot) > 2.75f)
                            {
                                Bot.Movement.SetMovementAction(MovementAction.Move, Bot.Target.Position);
                            }
                        }
                        else
                        {
                            if (Bot.Player.DistanceTo(Bot.Target.Position) > Bot.Player.MeleeRangeTo(Bot.Target))
                            {
                                Bot.Movement.SetMovementAction(MovementAction.Move, Bot.Target.Position);
                            }
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

                // go all ham, maybe filter for bossfights?
                TryCastSpell(Warrior548.BattleShout, 0);
                TryCastSpell(Warrior548.Recklessness, 0);

                // is anyone casting stuff on me, try to reflect
                IEnumerable<IWowUnit> castingUnits = Bot.Objects.WowObjects.OfType<IWowUnit>().Where(e => e.IsCasting && e.TargetGuid == Bot.Player.Guid && e.DistanceTo(Bot.Player) < 38.0);

                if (castingUnits.Any())
                {
                    TryCastSpell(Warrior548.SpellReflection, 0);
                }

                if (distanceToTarget > 8.0)
                {
                    if (TryCastSpellWarrior(Warrior548.Charge, Warrior548.DefensiveStance, Bot.Wow.TargetGuid))
                    {
                        if (Configurables["FartOnCharge"] && DateTime.UtcNow - LastFarted > TimeSpan.FromSeconds(8))
                        {
                            LastFarted = DateTime.UtcNow;
                            Bot.Wow.SendChatMessage("/rude");
                        }

                        return;
                    }

                    if (TryCastSpell(Warrior548.HeroicThrow, Bot.Wow.TargetGuid))
                    {
                        return;
                    }
                }
                else
                {
                    if (Bot.Player.HealthPercentage < 50.0
                        && TryCastSpell(Warrior548.ShieldBlock, 0, true))
                    {
                        return;
                    }

                    if (Bot.Player.HealthPercentage < 35.0)
                    {
                        TryCastSpell(Warrior548.ShieldWall, 0);
                    }

                    if (Bot.Player.HealthPercentage < 25.0)
                    {
                        TryCastSpell(Warrior548.LastStand, 0);
                    }

                    if (Bot.Target.HealthPercentage < 20.0
                        && TryCastSpell(Warrior548.Execute, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    bool hasVictoriousBuff = Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Warrior548.Victorious);

                    if (hasVictoriousBuff
                        && Bot.Player.HealthPercentage < 80.0
                        && TryCastSpell(Warrior548.VictoryRush, Bot.Wow.TargetGuid))
                    {
                        return;
                    }

                    if (Bot.Target.TargetGuid != Bot.Wow.PlayerGuid
                        && TryCastSpell(Warrior548.Taunt, Bot.Wow.TargetGuid))
                    {
                        return;
                    }

                    if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Warrior548.DemoralizingShout)
                        && TryCastSpell(Warrior548.DemoralizingShout, Bot.Wow.TargetGuid))
                    {
                        return;
                    }

                    if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Warrior548.PiercingHowl)
                        && TryCastSpell(Warrior548.PiercingHowl, Bot.Wow.TargetGuid))
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
                        && TryCastSpell(Warrior548.ThunderClap, Bot.Wow.TargetGuid))
                    {
                        return;
                    }

                    if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Warrior548.WeakenedAmor && e.StackCount < 3)
                        && (TryCastSpell(Warrior548.SunderAmor, Bot.Wow.TargetGuid, true, Bot.Player.Rage - 15))
                            || TryCastSpell(Warrior548.Devastate, Bot.Wow.TargetGuid, true, Bot.Player.Rage - 15))
                    {
                        return;
                    }

                    if (TryCastSpell(Warrior548.HeroicStrike, Bot.Wow.TargetGuid, true, Bot.Player.Rage - 30))
                    {
                        return;
                    }

                    if (nearEnemies > 1
                        && TryCastSpell(Warrior548.DragonRoar, Bot.Wow.TargetGuid))
                    {
                        return;
                    }

                    if ((nearEnemies > 1 || Bot.Player.Rage > (rageToSave + 30))
                        && TryCastSpell(Warrior548.Cleave, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    // when we got nothing to do, use Victory Rush
                    if (hasVictoriousBuff
                        && TryCastSpell(Warrior548.VictoryRush, Bot.Wow.TargetGuid))
                    {
                        return;
                    }
                }
            }
        }
    }
}