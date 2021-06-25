using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.CombatClasses.Shino;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Shino
{
    public class MageFrost : TemplateCombatClass
    {
        public MageFrost(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(arcaneIntellectSpell, () => TryCastSpell(arcaneIntellectSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(frostArmorSpell, () => TryCastSpell(frostArmorSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(iceArmorSpell, () => TryCastSpell(iceArmorSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(manaShieldSpell, () => TryCastSpell(manaShieldSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(iceBarrierSpell, () => TryCastSpell(iceBarrierSpell, 0, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(counterspellSpell, x.Guid, true) }
            };
        }

        public override string Description => "Grinding and Leveling CombatClass.";

        public override string Displayname => "Frostmage";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

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

            if (WowInterface.Player.IsCasting)
            {
                return;
            }

            if (SelectTarget(out WowUnit target))
            {
                if (WowInterface.Player.ManaPercentage <= 25.0 && TryCastSpell(evocationSpell, 0, true))
                {
                    return;
                }

                if (CooldownManager.IsSpellOnCooldown(summonWaterElementalSpell) &&
                    CooldownManager.IsSpellOnCooldown(icyVeinsSpell))
                {
                    TryCastSpell(coldSnapSpell, 0);
                }

                if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(freezeSpell))
                {
                    TryCastAoeSpell(freezeSpell, target.Guid);
                }

                var nearbyTargets = WowInterface.ObjectManager.GetEnemiesInCombatWithParty<WowUnit>(WowInterface.Player.Position, 64.0f);
                if (nearbyTargets.Count(e => e.Position.GetDistance(WowInterface.Player.Position) <= 9.0) == 1
                    && TryCastSpell(frostNovaSpell, 0, true))
                {
                    return;
                }

                if (DateTime.Now.Subtract(LastSheep).TotalMilliseconds >= 3000.0)
                {
                    if (nearbyTargets.Count() > 1 && !nearbyTargets.Any(e => e.Auras.Any(aura => aura.Name == polymorphSpell)))
                    {
                        var targetInDistance = nearbyTargets
                            .Where(e => e.Guid != WowInterface.TargetGuid)
                            .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                            .FirstOrDefault();
                        WowInterface.HookManager.WowTargetGuid(targetInDistance.Guid);
                        if (TryCastSpell(polymorphSpell, targetInDistance.Guid, true))
                        {
                            WowInterface.HookManager.WowTargetGuid(target.Guid);
                            LastSheep = DateTime.Now;
                            return;
                        }
                    }
                }

                if (WowInterface.Target.Position.GetDistance(WowInterface.Player.Position) <= 4.0)
                {
                    // TODO: Logic to check if the target blink location is dangerous
                    if (!TryCastSpell(blinkSpell, 0, true))
                    {
                    }
                    else
                    {
                        // TODO: Go away somehow if the enemy is freezed?
                        return;
                    }
                }

                if (WowInterface.Target.Position.GetDistance(WowInterface.Player.Position) <= 4.0
                    && WowInterface.Player.HealthPercentage <= 50.0
                    && CooldownManager.IsSpellOnCooldown(blinkSpell) && TryCastSpell(iceBlockSpell, 0, true))
                {
                    return;
                }

                if (TryCastSpell(summonWaterElementalSpell, target.Guid, true))
                {
                    return;
                }

                if (TryCastSpell(deepFreezeSpell, target.Guid, true))
                {
                    return;
                }

                TryCastSpell(icyVeinsSpell, 0, true);
                TryCastSpell(berserkingSpell, 0, true);

                if (TryCastSpell(frostBoltSpell, target.Guid, true))
                {
                    return;
                }

                TryCastSpell(fireballSpell, target.Guid, true);
            }
        }

        public override void OutOfCombatExecute()
        {
            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(conjureWaterSpell))
            {
                Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(conjureWaterSpell);
                spell.TryGetRank(out int spellRank);
                if (spellRank >= 2)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Water");
                }

                if (spellRank >= 3)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Fresh Water");
                }

                if (spellRank >= 4)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Spring Water");
                }

                if (spellRank >= 5)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Mineral Water");
                }

                if (spellRank >= 6)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Sparkling Water");
                }

                if (spellRank >= 7)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Crystal Water");
                }

                if (spellRank >= 8)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Mountain Spring Water");
                }

                if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(conjureRefreshment))
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Glacier Water");
                }

                if (
                    (spellRank == 1 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Water"))
                    || (spellRank == 2 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Fresh Water"))
                    || (spellRank == 3 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Spring Water"))
                    || (spellRank == 4 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Mineral Water"))
                    || (spellRank == 5 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Sparkling Water"))
                    || (spellRank == 6 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Crystal Water"))
                    || (spellRank == 7 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Mountain Spring Water"))
                    || (spellRank == 8 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Glacier Water"))
                    )
                {
                    TryCastSpell(conjureWaterSpell, 0, true);
                    return;
                }
            }

            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(conjureFoodSpell))
            {
                Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(conjureFoodSpell);
                spell.TryGetRank(out int spellRank);
                if (spellRank >= 2)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Muffin");
                }

                if (spellRank >= 3)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Bread");
                }

                if (spellRank >= 4)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Rye");
                }

                if (spellRank >= 5)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Pumpernickel");
                }

                if (spellRank >= 6)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Sourdough");
                }

                if (spellRank >= 7)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Sweet Roll");
                }

                if (spellRank >= 8)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Cinnamon Roll");
                }

                if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(conjureRefreshment))
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Croissant");
                }

                if (
                    (spellRank == 1 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Muffin"))
                    || (spellRank == 2 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Bread"))
                    || (spellRank == 3 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Rye"))
                    || (spellRank == 4 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Pumpernickel"))
                    || (spellRank == 5 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Sourdough"))
                    || (spellRank == 6 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Sweet Roll"))
                    || (spellRank == 7 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Cinnamon Roll"))
                    || (spellRank == 8 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Croissant"))
                    )
                {
                    TryCastSpell(conjureFoodSpell, 0, true);
                    return;
                }
            }

            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(conjureRefreshment))
            {
                Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(conjureRefreshment);
                spell.TryGetRank(out int spellRank);

                if (spellRank >= 2)
                {
                    WowInterface.CharacterManager.Inventory.DestroyItemByName("Conjured Mana Pie");
                }

                if (
                    (spellRank == 1 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Mana Pie"))
                    || (spellRank == 2 && !WowInterface.CharacterManager.Inventory.HasItemByName("Conjured Mana Strudel"))
                )
                {
                    TryCastSpell(conjureRefreshment, 0, true);
                    return;
                }
            }

            base.OutOfCombatExecute();
        }

        protected override Spell GetOpeningSpell()
        {
            Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(frostBoltSpell);
            if (spell != null)
            {
                return spell;
            }
            return WowInterface.CharacterManager.SpellBook.GetSpellByName(fireballSpell);
        }
    }
}