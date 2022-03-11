using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Logic.CombatClasses.Shino;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Shino
{
    public class MageFrost : TemplateCombatClass
    {
        public MageFrost(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.ArcaneIntellect, () => TryCastSpell(Mage335a.ArcaneIntellect, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.FrostArmor, () => TryCastSpell(Mage335a.FrostArmor, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.IceArmor, () => TryCastSpell(Mage335a.IceArmor, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.ManaShield, () => TryCastSpell(Mage335a.ManaShield, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.IceBarrier, () => TryCastSpell(Mage335a.IceBarrier, 0, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(Mage335a.Counterspell, x.Guid, true) }
            };
        }

        public override string Description => "Grinding and Leveling CombatClass.";

        public override string DisplayName2 => "Frostmage";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.Shield }, new() { WowWeaponType.Sword, WowWeaponType.Mace, WowWeaponType.Axe });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 2, new(1, 2, 3) },
            },
            Tree2 = new(),
            Tree3 = new()
            {
                { 1, new(3, 1, 3) },
                { 2, new(3, 2, 5) },
                { 3, new(3, 3, 3) },
                { 4, new(3, 4, 3) },
                { 5, new(3, 5, 2) },
                { 6, new(3, 6, 3) },
                { 7, new(3, 7, 3) },
                { 8, new(3, 8, 3) },
                { 9, new(3, 9, 1) },
                { 11, new(3, 11, 2) },
                { 12, new(3, 12, 3) },
                { 13, new(3, 13, 3) },
                { 14, new(3, 14, 1) },
                { 15, new(3, 15, 3) },
                { 17, new(3, 17, 2) },
                { 18, new(3, 18, 3) },
                { 19, new(3, 19, 2) },
                { 20, new(3, 20, 1) },
                { 21, new(3, 21, 5) },
                { 22, new(3, 22, 2) },
                { 23, new(3, 23, 2) },
                { 24, new(3, 24, 3) },
                { 25, new(3, 25, 1) },
                { 26, new(3, 26, 3) },
                { 27, new(3, 27, 5) },
                { 28, new(3, 28, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Mage;

        private DateTime LastSheep { get; set; } = DateTime.Now;

        public override void Execute()
        {
            base.Execute();

            if (Bot.Player.IsCasting)
            {
                return;
            }

            if (SelectTarget(out IWowUnit target))
            {
                if (Bot.Player.ManaPercentage <= 25.0 && TryCastSpell(Mage335a.Evocation, 0, true))
                {
                    return;
                }

                if (CooldownManager.IsSpellOnCooldown(Mage335a.SummonWaterElemental) &&
                    CooldownManager.IsSpellOnCooldown(Mage335a.IcyVeins))
                {
                    TryCastSpell(Mage335a.ColdSnap, 0);
                }

                if (Bot.Character.SpellBook.IsSpellKnown(Mage335a.Freeze))
                {
                    TryCastAoeSpell(Mage335a.Freeze, target.Guid);
                }

                System.Collections.Generic.IEnumerable<IWowUnit> nearbyTargets = Bot.GetEnemiesInCombatWithParty<IWowUnit>(Bot.Player.Position, 64.0f);
                if (nearbyTargets.Count(e => e.Position.GetDistance(Bot.Player.Position) <= 9.0) == 1
                    && TryCastSpell(Mage335a.FrostNova, 0, true))
                {
                    return;
                }

                if (DateTime.Now.Subtract(LastSheep).TotalMilliseconds >= 3000.0)
                {
                    if (nearbyTargets.Count() > 1 && !nearbyTargets.Any(e => e.Auras.Any(aura => Bot.Db.GetSpellName(aura.SpellId) == Mage335a.Polymorph)))
                    {
                        IWowUnit targetInDistance = nearbyTargets
                            .Where(e => e.Guid != Bot.Wow.TargetGuid)
                            .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                            .FirstOrDefault();
                        Bot.Wow.ChangeTarget(targetInDistance.Guid);
                        if (TryCastSpell(Mage335a.Polymorph, targetInDistance.Guid, true))
                        {
                            Bot.Wow.ChangeTarget(target.Guid);
                            LastSheep = DateTime.Now;
                            return;
                        }
                    }
                }

                if (Bot.Target.Position.GetDistance(Bot.Player.Position) <= 4.0)
                {
                    // TODO: Logic to check if the target blink location is dangerous
                    if (!TryCastSpell(Mage335a.Blink, 0, true))
                    {
                    }
                    else
                    {
                        // TODO: Go away somehow if the enemy is freezed?
                        return;
                    }
                }

                if (Bot.Target.Position.GetDistance(Bot.Player.Position) <= 4.0
                    && Bot.Player.HealthPercentage <= 50.0
                    && CooldownManager.IsSpellOnCooldown(Mage335a.Blink) && TryCastSpell(Mage335a.IceBlock, 0, true))
                {
                    return;
                }

                if (TryCastSpell(Mage335a.SummonWaterElemental, target.Guid, true))
                {
                    return;
                }

                if (TryCastSpell(Mage335a.DeepFreeze, target.Guid, true))
                {
                    return;
                }

                TryCastSpell(Mage335a.IcyVeins, 0, true);
                TryCastSpell(Racials335a.Berserking, 0, true);

                if (TryCastSpell(Mage335a.FrostBolt, target.Guid, true))
                {
                    return;
                }

                TryCastSpell(Mage335a.Fireball, target.Guid, true);
            }
        }

        public override void OutOfCombatExecute()
        {
            if (Bot.Character.SpellBook.IsSpellKnown(Mage335a.ConjureWater))
            {
                Spell spell = Bot.Character.SpellBook.GetSpellByName(Mage335a.ConjureWater);
                spell.TryGetRank(out int spellRank);
                if (spellRank >= 2)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Water");
                }

                if (spellRank >= 3)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Fresh Water");
                }

                if (spellRank >= 4)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Spring Water");
                }

                if (spellRank >= 5)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Mineral Water");
                }

                if (spellRank >= 6)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Sparkling Water");
                }

                if (spellRank >= 7)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Crystal Water");
                }

                if (spellRank >= 8)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Mountain Spring Water");
                }

                if (Bot.Character.SpellBook.IsSpellKnown(Mage335a.ConjureRefreshment))
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Glacier Water");
                }

                if (
                    (spellRank == 1 && !Bot.Character.Inventory.HasItemByName("Conjured Water"))
                    || (spellRank == 2 && !Bot.Character.Inventory.HasItemByName("Conjured Fresh Water"))
                    || (spellRank == 3 && !Bot.Character.Inventory.HasItemByName("Conjured Spring Water"))
                    || (spellRank == 4 && !Bot.Character.Inventory.HasItemByName("Conjured Mineral Water"))
                    || (spellRank == 5 && !Bot.Character.Inventory.HasItemByName("Conjured Sparkling Water"))
                    || (spellRank == 6 && !Bot.Character.Inventory.HasItemByName("Conjured Crystal Water"))
                    || (spellRank == 7 && !Bot.Character.Inventory.HasItemByName("Conjured Mountain Spring Water"))
                    || (spellRank == 8 && !Bot.Character.Inventory.HasItemByName("Conjured Glacier Water"))
                    )
                {
                    TryCastSpell(Mage335a.ConjureWater, 0, true);
                    return;
                }
            }

            if (Bot.Character.SpellBook.IsSpellKnown(Mage335a.ConjureFood))
            {
                Spell spell = Bot.Character.SpellBook.GetSpellByName(Mage335a.ConjureFood);
                spell.TryGetRank(out int spellRank);
                if (spellRank >= 2)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Muffin");
                }

                if (spellRank >= 3)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Bread");
                }

                if (spellRank >= 4)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Rye");
                }

                if (spellRank >= 5)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Pumpernickel");
                }

                if (spellRank >= 6)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Sourdough");
                }

                if (spellRank >= 7)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Sweet Roll");
                }

                if (spellRank >= 8)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Cinnamon Roll");
                }

                if (Bot.Character.SpellBook.IsSpellKnown(Mage335a.ConjureRefreshment))
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Croissant");
                }

                if (
                    (spellRank == 1 && !Bot.Character.Inventory.HasItemByName("Conjured Muffin"))
                    || (spellRank == 2 && !Bot.Character.Inventory.HasItemByName("Conjured Bread"))
                    || (spellRank == 3 && !Bot.Character.Inventory.HasItemByName("Conjured Rye"))
                    || (spellRank == 4 && !Bot.Character.Inventory.HasItemByName("Conjured Pumpernickel"))
                    || (spellRank == 5 && !Bot.Character.Inventory.HasItemByName("Conjured Sourdough"))
                    || (spellRank == 6 && !Bot.Character.Inventory.HasItemByName("Conjured Sweet Roll"))
                    || (spellRank == 7 && !Bot.Character.Inventory.HasItemByName("Conjured Cinnamon Roll"))
                    || (spellRank == 8 && !Bot.Character.Inventory.HasItemByName("Conjured Croissant"))
                    )
                {
                    TryCastSpell(Mage335a.ConjureFood, 0, true);
                    return;
                }
            }

            if (Bot.Character.SpellBook.IsSpellKnown(Mage335a.ConjureRefreshment))
            {
                Spell spell = Bot.Character.SpellBook.GetSpellByName(Mage335a.ConjureRefreshment);
                spell.TryGetRank(out int spellRank);

                if (spellRank >= 2)
                {
                    Bot.Character.Inventory.DestroyItemByName("Conjured Mana Pie");
                }

                if (
                    (spellRank == 1 && !Bot.Character.Inventory.HasItemByName("Conjured Mana Pie"))
                    || (spellRank == 2 && !Bot.Character.Inventory.HasItemByName("Conjured Mana Strudel"))
                )
                {
                    TryCastSpell(Mage335a.ConjureRefreshment, 0, true);
                    return;
                }
            }

            base.OutOfCombatExecute();
        }

        protected override Spell GetOpeningSpell()
        {
            Spell spell = Bot.Character.SpellBook.GetSpellByName(Mage335a.FrostBolt);
            if (spell != null)
            {
                return spell;
            }
            return Bot.Character.SpellBook.GetSpellByName(Mage335a.Fireball);
        }
    }
}