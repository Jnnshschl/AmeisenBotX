using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis335a
{
    public class WarriorFury : BasicCombatClass335a
    {
        public WarriorFury(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Warrior335a.BattleShout, () => TryCastSpell(Warrior335a.BattleShout, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Warrior335a.Hamstring, () => Bot.Target?.Type == WowObjectType.Player && TryCastSpell(Warrior335a.Hamstring, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Warrior335a.Rend, () => Bot.Target?.Type == WowObjectType.Player && Bot.Player.Rage > 75 && TryCastSpell(Warrior335a.Rend, Bot.Wow.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpellWarrior(Warrior335a.IntimidatingShout, Warrior335a.BerserkerStance, x.Guid, true) },
                { 1, (x) => TryCastSpellWarrior(Warrior335a.IntimidatingShout, Warrior335a.BattleStance, x.Guid, true) }
            };

            HeroicStrikeEvent = new(TimeSpan.FromSeconds(2));
        }

        public override string Description => "FCFS based CombatClass for the Fury Warrior spec.";

        public override string DisplayName => "Warrior Fury 3.3.5a";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new() { WowArmorType.Shield }, new() { WowWeaponType.Sword, WowWeaponType.Mace, WowWeaponType.Axe });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 3) },
                { 3, new(1, 3, 2) },
                { 5, new(1, 5, 2) },
                { 6, new(1, 6, 3) },
                { 9, new(1, 9, 2) },
                { 10, new(1, 10, 3) },
                { 11, new(1, 11, 3) },
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 3) },
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 6, new(2, 6, 3) },
                { 10, new(2, 10, 5) },
                { 13, new(2, 13, 3) },
                { 14, new(2, 14, 1) },
                { 16, new(2, 16, 1) },
                { 17, new(2, 17, 5) },
                { 18, new(2, 18, 3) },
                { 19, new(2, 19, 1) },
                { 20, new(2, 20, 2) },
                { 22, new(2, 22, 5) },
                { 23, new(2, 23, 1) },
                { 24, new(2, 24, 1) },
                { 25, new(2, 25, 3) },
                { 26, new(2, 26, 5) },
                { 27, new(2, 27, 1) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Warrior;

        private TimegatedEvent HeroicStrikeEvent { get; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if (Bot.Target != null)
                {
                    double distanceToTarget = Bot.Target.Position.GetDistance(Bot.Player.Position);

                    if ((Bot.Player.IsDazed
                        || Bot.Player.IsConfused
                        || Bot.Player.IsPossessed
                        || Bot.Player.IsFleeing)
                        && TryCastSpell(Warrior335a.HeroicFury, 0))
                    {
                        return;
                    }

                    if (distanceToTarget > 4.0)
                    {
                        if (TryCastSpellWarrior(Warrior335a.Charge, Warrior335a.BattleStance, Bot.Wow.TargetGuid, true)
                            || (TryCastSpell(Warrior335a.BerserkerRage, Bot.Wow.TargetGuid, true) && TryCastSpellWarrior(Warrior335a.Intercept, Warrior335a.BerserkerStance, Bot.Wow.TargetGuid, true)))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (HeroicStrikeEvent.Ready && !Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Warrior335a.Recklessness))
                        {
                            if ((Bot.Player.Rage > 50 && Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, 8.0f).Count() > 2 && TryCastSpellWarrior(Warrior335a.Cleave, Warrior335a.BerserkerStance, 0, true))
                                || (Bot.Player.Rage > 50 && TryCastSpellWarrior(Warrior335a.HeroicStrike, Warrior335a.BerserkerStance, Bot.Wow.TargetGuid, true)))
                            {
                                HeroicStrikeEvent.Run();
                                return;
                            }
                        }

                        if (TryCastSpellWarrior(Warrior335a.Bloodthirst, Warrior335a.BerserkerStance, Bot.Wow.TargetGuid, true)
                            || TryCastSpellWarrior(Warrior335a.Whirlwind, Warrior335a.BerserkerStance, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        // dont prevent BT or WW with GCD
                        if (CooldownManager.GetSpellCooldown(Warrior335a.Bloodthirst) <= 1200
                            || CooldownManager.GetSpellCooldown(Warrior335a.Whirlwind) <= 1200)
                        {
                            return;
                        }

                        if (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == $"{Warrior335a.Slam}!")
                           && TryCastSpell(Warrior335a.Slam, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        if (TryCastSpell(Warrior335a.BerserkerRage, 0))
                        {
                            return;
                        }

                        if (TryCastSpell(Warrior335a.Bloodrage, Bot.Wow.TargetGuid, true, Bot.Player.Health))
                        {
                            return;
                        }

                        if (TryCastSpell(Warrior335a.Recklessness, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        if (TryCastSpell(Warrior335a.DeathWish, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }

                        if (Bot.Player.Rage > 25
                           && Bot.Target.HealthPercentage < 20
                           && TryCastSpellWarrior(Warrior335a.Execute, Warrior335a.BerserkerStance, Bot.Wow.TargetGuid, true))
                        {
                            return;
                        }
                    }
                }
            }
        }
    }
}