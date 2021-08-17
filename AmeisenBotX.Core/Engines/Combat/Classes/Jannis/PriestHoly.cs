﻿using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Logic.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class PriestHoly : BasicCombatClass
    {
        public PriestHoly(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, powerWordFortitudeSpell, () => TryCastSpell(powerWordFortitudeSpell, Bot.Wow.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, innerFireSpell, () => TryCastSpell(innerFireSpell, 0, true)));

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, healSpell },
                { 100, flashHealSpell },
                { 5000, greaterHealSpell },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((powerWordFortitudeSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Holy Priest spec.";

        public override string Displayname => "Priest Holy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public override WowRole Role => WowRole.Heal;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 2, new(1, 2, 5) },
                { 4, new(1, 4, 3) },
                { 5, new(1, 5, 2) },
                { 7, new(1, 7, 3) },
                { 8, new(1, 8, 1) },
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 2) },
                { 2, new(2, 2, 3) },
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 8, new(2, 8, 3) },
                { 9, new(2, 9, 2) },
                { 10, new(2, 10, 3) },
                { 12, new(2, 12, 2) },
                { 13, new(2, 13, 1) },
                { 14, new(2, 14, 5) },
                { 15, new(2, 15, 2) },
                { 16, new(2, 16, 5) },
                { 17, new(2, 17, 3) },
                { 22, new(2, 22, 3) },
                { 23, new(2, 23, 3) },
                { 24, new(2, 24, 1) },
                { 25, new(2, 25, 3) },
                { 26, new(2, 26, 5) },
                { 27, new(2, 27, 1) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Priest;

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override void Execute()
        {
            base.Execute();

            if ((Bot.Objects.PartymemberGuids.Any() || Bot.Player.HealthPercentage < 75.0)
                && NeedToHealSomeone())
            {
                return;
            }

            if (Bot.Player.ManaPercentage < 20
                && TryCastSpell(hymnOfHopeSpell, 0))
            {
                return;
            }

            if ((!Bot.Objects.PartymemberGuids.Any() || Bot.Player.ManaPercentage > 50) && SelectTarget(TargetProviderDps))
            {
                if (Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == shadowWordPainSpell)
                    && TryCastSpell(shadowWordPainSpell, Bot.Wow.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(smiteSpell, Bot.Wow.TargetGuid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealSomeone()
                || HandleDeadPartymembers(resurrectionSpell))
            {
                return;
            }
        }

        private bool NeedToHealSomeone()
        {
            if (TargetProviderHeal.Get(out IEnumerable<IWowUnit> unitsToHeal))
            {
                IWowUnit target = unitsToHeal.First();

                if (unitsToHeal.Count() > 3
                    && target.HealthPercentage > 80.0
                    && TryCastSpell(prayerOfHealingSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 25.0
                    && TryCastSpell(guardianSpiritSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.Guid != Bot.Wow.PlayerGuid
                    && target.HealthPercentage < 70.0
                    && Bot.Player.HealthPercentage < 70.0
                    && TryCastSpell(bindingHealSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 90.0
                    && target.HealthPercentage > 75.0
                    && !target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == renewSpell)
                    && TryCastSpell(renewSpell, target.Guid, true))
                {
                    return true;
                }

                double healthDifference = target.MaxHealth - target.Health;
                List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).OrderByDescending(e => e.Key).ToList();

                foreach (KeyValuePair<int, string> keyValuePair in spellsToTry)
                {
                    if (TryCastSpell(keyValuePair.Value, target.Guid, true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}