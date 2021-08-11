﻿using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class DeathknightBlood : BasicCombatClass
    {
        public DeathknightBlood(AmeisenBotInterfaces bot, AmeisenBotFsm stateMachine) : base(bot, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, bloodPresenceSpell, () => TryCastSpellDk(bloodPresenceSpell, 0)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, hornOfWinterSpell, () => TryCastSpellDk(hornOfWinterSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, frostFeverSpell, () => TryCastSpellDk(icyTouchSpell, Bot.Wow.TargetGuid, false, false, false, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, bloodPlagueSpell, () => TryCastSpellDk(plagueStrikeSpell, Bot.Wow.TargetGuid, false, false, false, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpellDk(mindFreezeSpell, x.Guid, true) },
                { 1, (x) => TryCastSpellDk(strangulateSpell, x.Guid, false, true) }
            };

            BloodBoilEvent = new(TimeSpan.FromSeconds(2));
        }

        public override string Description => "FCFS based CombatClass for the Blood Deathknight spec.";

        public override string Displayname => "Deathknight Blood";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new() { WowArmorType.SHIELDS });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 2, new(1, 2, 3) },
                { 3, new(1, 3, 5) },
                { 4, new(1, 4, 5) },
                { 5, new(1, 5, 2) },
                { 6, new(1, 6, 2) },
                { 7, new(1, 7, 1) },
                { 8, new(1, 8, 5) },
                { 9, new(1, 9, 3) },
                { 13, new(1, 13, 3) },
                { 14, new(1, 14, 3) },
                { 16, new(1, 16, 3) },
                { 17, new(1, 17, 2) },
                { 18, new(1, 18, 3) },
                { 19, new(1, 19, 1) },
                { 21, new(1, 21, 2) },
                { 23, new(1, 23, 1) },
                { 24, new(1, 24, 3) },
                { 25, new(1, 25, 1) },
                { 26, new(1, 26, 3) },
                { 27, new(1, 27, 5) },
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 3) },
                { 3, new(2, 3, 5) },
            },
            Tree3 = new()
            {
                { 3, new(3, 3, 5) },
                { 4, new(3, 4, 2) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Deathknight;

        private TimegatedEvent BloodBoilEvent { get; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if (Bot.Target.TargetGuid != Bot.Wow.PlayerGuid
                    && TryCastSpellDk(darkCommandSpell, Bot.Wow.TargetGuid))
                {
                    return;
                }

                if (Bot.Target.Position.GetDistance(Bot.Player.Position) > 6.0
                    && TryCastSpellDk(deathGripSpell, Bot.Wow.TargetGuid, false, false, true))
                {
                    return;
                }

                if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == chainsOfIceSpell)
                    && Bot.Target.Position.GetDistance(Bot.Player.Position) > 2.0
                    && TryCastSpellDk(chainsOfIceSpell, Bot.Wow.TargetGuid, false, false, true))
                {
                    return;
                }

                if (TryCastSpellDk(empowerRuneWeapon, 0))
                {
                    return;
                }

                int nearEnemies = Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, 12.0f).Count();

                if ((Bot.Player.HealthPercentage < 70.0 && TryCastSpellDk(runeTapSpell, 0, false, false, true))
                    || (Bot.Player.HealthPercentage < 60.0 && (TryCastSpellDk(iceboundFortitudeSpell, 0, true) || TryCastSpellDk(antiMagicShellSpell, 0, true)))
                    || (Bot.Player.HealthPercentage < 50.0 && TryCastSpellDk(vampiricBloodSpell, 0, false, false, true))
                    || (nearEnemies > 2 && (TryCastAoeSpellDk(deathAndDecaySpell, 0) || (BloodBoilEvent.Run() && TryCastSpellDk(bloodBoilSpell, 0))))
                    || TryCastSpellDk(unbreakableArmorSpell, 0, false, false, true)
                    || TryCastSpellDk(deathStrike, Bot.Wow.TargetGuid, false, false, true, true)
                    || TryCastSpellDk(heartStrikeSpell, Bot.Wow.TargetGuid, false, false, true)
                    || TryCastSpellDk(deathCoilSpell, Bot.Wow.TargetGuid, true))
                {
                    return;
                }
            }
        }
    }
}