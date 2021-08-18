﻿using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class DeathknightUnholy : BasicCombatClass
    {
        public DeathknightUnholy(AmeisenBotInterfaces bot, AmeisenBotFsm stateMachine) : base(bot, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, unholyPresenceSpell, () => TryCastSpellDk(unholyPresenceSpell, 0)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, hornOfWinterSpell, () => TryCastSpellDk(hornOfWinterSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, frostFeverSpell, () => TryCastSpellDk(icyTouchSpell, Bot.Wow.TargetGuid, false, false, false, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, bloodPlagueSpell, () => TryCastSpellDk(plagueStrikeSpell, Bot.Wow.TargetGuid, false, false, false, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpellDk(mindFreezeSpell, x.Guid, true) },
                { 1, (x) => TryCastSpellDk(strangulateSpell, x.Guid, false, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Unholy Deathknight spec.";

        public override string DisplayName => "Deathknight Unholy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new() { WowArmorType.SHIELDS });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 2) },
                { 2, new(1, 2, 3) },
                { 4, new(1, 4, 5) },
                { 6, new(1, 6, 2) },
                { 8, new(1, 8, 5) },
            },
            Tree2 = new(),
            Tree3 = new()
            {
                { 1, new(3, 1, 2) },
                { 2, new(3, 2, 3) },
                { 4, new(3, 4, 2) },
                { 7, new(3, 7, 3) },
                { 8, new(3, 8, 3) },
                { 9, new(3, 9, 5) },
                { 12, new(3, 12, 3) },
                { 13, new(3, 13, 2) },
                { 14, new(3, 14, 1) },
                { 15, new(3, 15, 5) },
                { 16, new(3, 16, 2) },
                { 20, new(3, 20, 1) },
                { 21, new(3, 21, 5) },
                { 25, new(3, 25, 3) },
                { 26, new(3, 26, 1) },
                { 27, new(3, 27, 3) },
                { 28, new(3, 28, 3) },
                { 29, new(3, 29, 1) },
                { 30, new(3, 30, 5) },
                { 31, new(3, 31, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Deathknight;

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

                if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == chainsOfIceSpell)
                    && Bot.Target.Position.GetDistance(Bot.Player.Position) > 2.0
                    && TryCastSpellDk(chainsOfIceSpell, Bot.Wow.TargetGuid, false, false, true))
                {
                    return;
                }

                if (Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == chainsOfIceSpell)
                    && TryCastSpellDk(chainsOfIceSpell, Bot.Wow.TargetGuid, false, false, true))
                {
                    return;
                }

                if (TryCastSpellDk(empowerRuneWeapon, 0))
                {
                    return;
                }

                if ((Bot.Player.HealthPercentage < 60
                        && TryCastSpellDk(iceboundFortitudeSpell, Bot.Wow.TargetGuid, true))
                    || TryCastSpellDk(bloodStrikeSpell, Bot.Wow.TargetGuid, false, true)
                    || TryCastSpellDk(scourgeStrikeSpell, Bot.Wow.TargetGuid, false, false, true, true)
                    || TryCastSpellDk(deathCoilSpell, Bot.Wow.TargetGuid, true)
                    || TryCastSpellDk(summonGargoyleSpell, Bot.Wow.TargetGuid, true)
                    || (Bot.Player.Runeenergy > 60
                        && TryCastSpellDk(runeStrikeSpell, Bot.Wow.TargetGuid)))
                {
                    return;
                }
            }
        }
    }
}