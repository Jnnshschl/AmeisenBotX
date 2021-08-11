using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class DruidRestoration : BasicCombatClass
    {
        public DruidRestoration(AmeisenBotInterfaces bot, AmeisenBotFsm stateMachine) : base(bot, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, treeOfLifeSpell, () => Bot.Objects.PartymemberGuids.Any() && TryCastSpell(treeOfLifeSpell, Bot.Wow.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, markOfTheWildSpell, () => TryCastSpell(markOfTheWildSpell, Bot.Wow.PlayerGuid, true)));

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((markOfTheWildSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));

            SwiftmendEvent = new(TimeSpan.FromSeconds(15));
        }

        public override string Description => "FCFS based CombatClass for the Druid Restoration spec.";

        public override string Displayname => "Druid Restoration";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicComparator
        (
            new() { WowArmorType.SHIELDS },
            new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES },
            new Dictionary<string, double>()
            {
                { "ITEM_MOD_CRIT_RATING_SHORT", 1.2 },
                { "ITEM_MOD_INTELLECT_SHORT", 1.0 },
                { "ITEM_MOD_SPELL_POWER_SHORT", 1.6 },
                { "ITEM_MOD_HASTE_RATING_SHORT", 1.8 },
                { "ITEM_MOD_SPIRIT_SHORT ", 1.4 },
                { "ITEM_MOD_POWER_REGEN0_SHORT", 1.4 },
            }
        );

        public override WowRole Role => WowRole.Heal;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 2, new(1, 2, 5) },
                { 3, new(1, 3, 3) },
                { 4, new(1, 4, 2) },
                { 8, new(1, 8, 1) },
            },
            Tree2 = new(),
            Tree3 = new()
            {
                { 1, new(3, 1, 2) },
                { 2, new(3, 2, 3) },
                { 5, new(3, 5, 3) },
                { 6, new(3, 6, 3) },
                { 7, new(3, 7, 3) },
                { 8, new(3, 8, 1) },
                { 9, new(3, 9, 2) },
                { 11, new(3, 11, 3) },
                { 12, new(3, 12, 1) },
                { 13, new(3, 13, 5) },
                { 14, new(3, 14, 2) },
                { 16, new(3, 16, 5) },
                { 17, new(3, 17, 3) },
                { 18, new(3, 18, 1) },
                { 20, new(3, 20, 5) },
                { 21, new(3, 21, 3) },
                { 22, new(3, 22, 3) },
                { 23, new(3, 23, 1) },
                { 24, new(3, 24, 3) },
                { 25, new(3, 25, 2) },
                { 26, new(3, 26, 5) },
                { 27, new(3, 27, 1) },
            },
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.1";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Druid;

        private TimegatedEvent SwiftmendEvent { get; }

        public override void Execute()
        {
            base.Execute();

            if (Bot.Player.ManaPercentage < 30.0
                && TryCastSpell(innervateSpell, 0, true))
            {
                return;
            }

            if (Bot.Player.HealthPercentage < 50.0
                && TryCastSpell(barkskinSpell, 0, true))
            {
                return;
            }

            if (Bot.Objects.Partymembers.Any(e => !e.IsDead))
            {
                if (NeedToHealSomeone())
                {
                    return;
                }
            }
            else
            {
                // when we're solo, we don't need to heal as much as we would do in a dungeon group
                if (Bot.Player.HealthPercentage < 75.0 && NeedToHealSomeone())
                {
                    return;
                }

                if (SelectTarget(TargetProviderDps))
                {
                    if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == moonfireSpell)
                        && TryCastSpell(moonfireSpell, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    if (TryCastSpell(starfireSpell, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    if (TryCastSpell(wrathSpell, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealSomeone()
                || HandleDeadPartymembers(reviveSpell))
            {
                return;
            }
        }

        private bool NeedToHealSomeone()
        {
            if (TargetProviderHeal.Get(out IEnumerable<IWowUnit> unitsToHeal))
            {
                if (unitsToHeal.Count(e => e.HealthPercentage < 40.0) > 3
                    && TryCastSpell(tranquilitySpell, 0, true))
                {
                    return true;
                }

                IWowUnit target = unitsToHeal.First();

                if (target.HealthPercentage < 90.0
                    && target.HealthPercentage > 75.0
                    && unitsToHeal.Count(e => e.HealthPercentage < 90.0) > 1
                    && TryCastSpell(wildGrowthSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 20.0
                    && TryCastSpell(naturesSwiftnessSpell, target.Guid, true)
                    && TryCastSpell(healingTouchSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 50.0
                    && (target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == regrowthSpell) || target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == rejuvenationSpell))
                    && SwiftmendEvent.Ready
                    && TryCastSpell(swiftmendSpell, target.Guid, true)
                    && SwiftmendEvent.Run())
                {
                    return true;
                }

                if (target.HealthPercentage < 95.0
                    && target.HealthPercentage > 70.0
                    && !target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == rejuvenationSpell)
                    && TryCastSpell(rejuvenationSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 98.0
                    && target.HealthPercentage > 70.0
                    && !target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == lifebloomSpell)
                    && TryCastSpell(lifebloomSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 65.0
                    && TryCastSpell(nourishSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 65.0
                    && TryCastSpell(healingTouchSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 65.0
                    && !target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == regrowthSpell)
                    && TryCastSpell(regrowthSpell, target.Guid, true))
                {
                    return true;
                }
            }

            return false;
        }
    }
}