using System;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.StateMachine.CombatClasses.Shino;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Shino
{
    public class MageFrost : TemplateCombatClass
    {
        public MageFrost(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { arcaneIntellectSpell, () => TryCastSpell(arcaneIntellectSpell, 0, true) },
                { frostArmorSpell, () => TryCastSpell(frostArmorSpell, 0, true) },
                { iceArmorSpell, () => TryCastSpell(iceArmorSpell, 0, true) },
                { manaShieldSpell, () => TryCastSpell(manaShieldSpell, 0, true) },
                { iceBarrierSpell, () => TryCastSpell(iceBarrierSpell, 0, true) },
            };

            MyAuraManager.BuffsToKeepUpCondition = new Dictionary<string, Condition>()
            {
                { frostArmorSpell, () => !WowInterface.CharacterManager.SpellBook.IsSpellKnown(iceArmorSpell) },
                { manaShieldSpell, () => !WowInterface.CharacterManager.SpellBook.IsSpellKnown(iceBarrierSpell) },
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                //{ 0, (x) => TryCastSpell(counterspellSpell, x.Guid, true) }
            };
        }

        public override string Description => "Grinding and Leveling CombatClass.";

        public override string Displayname => "Frostmage";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(1, 2, 3) },
            },
            Tree2 = new Dictionary<int, Talent>(),
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 3) },
                { 2, new Talent(3, 2, 5) },
                { 3, new Talent(3, 3, 3) },
                { 4, new Talent(3, 4, 3) },
                { 5, new Talent(3, 5, 2) },
                { 6, new Talent(3, 6, 3) },
                { 7, new Talent(3, 7, 3) },
                { 8, new Talent(3, 8, 3) },
                { 9, new Talent(3, 9, 1) },
                { 11, new Talent(3, 11, 2) },
                { 12, new Talent(3, 12, 3) },
                { 13, new Talent(3, 13, 3) },
                { 14, new Talent(3, 14, 1) },
                { 15, new Talent(3, 15, 3) },
                { 17, new Talent(3, 17, 2) },
                { 18, new Talent(3, 18, 3) },
                { 19, new Talent(3, 19, 2) },
                { 20, new Talent(3, 20, 1) },
                { 21, new Talent(3, 21, 5) },
                { 22, new Talent(3, 22, 2) },
                { 23, new Talent(3, 23, 2) },
                { 24, new Talent(3, 24, 3) },
                { 25, new Talent(3, 25, 1) },
                { 26, new Talent(3, 26, 3) },
                { 27, new Talent(3, 27, 5) },
                { 28, new Talent(3, 28, 1) },
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

            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (SelectTarget(out WowUnit target))
            {
                if (WowInterface.ObjectManager.Player.ManaPercentage <= 25.0 && TryCastSpell(evocationSpell, 0, true))
                {
                    return;
                }

                if (CooldownManager.IsSpellOnCooldown(summonWaterElementalSpell) &&
                    CooldownManager.IsSpellOnCooldown(icyVeinsSpell) 
                    && !CooldownManager.IsSpellOnCooldown(coldSnapSpell)
                    && TryCastSpell(coldSnapSpell, 0))
                {
                    return;
                }
                
                if (WowInterface.ObjectManager.Pet != null
                    && !CooldownManager.IsSpellOnCooldown(summonWaterElementalSpell)
                    && TryCastSpell(summonWaterElementalSpell, target.Guid, true))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Player.HasBuffByName("Fireball!") && TryCastSpell(frostFireBolt, target.Guid, false))
                {
                    return;
                }

                /*
                 // Thats apparently not how to cast pet spells!
                if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(freezeSpell))
                {
                    TryCastAoeSpell(freezeSpell, target.Guid);
                }
                */

                var nearbyTargets = WowInterface.ObjectManager.GetEnemiesInCombatWithUs<WowUnit>(WowInterface.ObjectManager.Player.Position, 64.0);
                if (nearbyTargets.Count(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) <= 8.0) == 1
                    && !CooldownManager.IsSpellOnCooldown(frostNovaSpell)
                    && TryCastSpell(frostNovaSpell, 0, true))
                {
                    return;
                }

                if (DateTime.Now.Subtract(LastSheep).TotalMilliseconds >= 3000.0)
                {
                    if (nearbyTargets.Count() > 1 && !nearbyTargets.Any(e => e.Auras.Any(aura => aura.Name == polymorphSpell)))
                    {
                        var targetInDistance = nearbyTargets
                            .Where(e => e.Guid != WowInterface.ObjectManager.TargetGuid)
                            .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
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

                if (WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) <= 4.0)
                {
                    // TODO: Logic to check if the target blink location is dangerous
                    if (!CooldownManager.IsSpellOnCooldown(blinkSpell) && !TryCastSpell(blinkSpell, 0, true))
                    {

                    }
                    else
                    {
                        // TODO: Go away somehow if the enemy is freezed?
                        return;
                    }
                }

                if (WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) <= 4.0 
                    && WowInterface.ObjectManager.Player.HealthPercentage <= 50.0
                    && CooldownManager.IsSpellOnCooldown(blinkSpell) && TryCastSpell(iceBlockSpell, 0, true))
                {
                    return;
                }

                if (!WowInterface.Target.HasBuffByName(frostNovaSpell) && !CooldownManager.IsSpellOnCooldown(deepFreezeSpell) && TryCastSpell(deepFreezeSpell, target.Guid, true))
                {
                    return;
                }

                if (!CooldownManager.IsSpellOnCooldown(icyVeinsSpell))
                {
                    TryCastSpell(icyVeinsSpell, 0);
                }

                if (!CooldownManager.IsSpellOnCooldown(berserkingSpell))
                {
                    TryCastSpell(berserkingSpell, 0);
                }

                if (TryCastSpell(frostBoltSpell, target.Guid, true))
                {
                    return;
                }

                if (TryCastSpell(fireBlastSpell, target.Guid, true))
                {
                    return;
                }

                if (TryCastSpell(coneOfColdSpell, target.Guid, true))
                {
                    return;
                }

                if (TryCastSpell(fireballSpell, target.Guid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(conjureWaterSpell) && !WowInterface.CharacterManager.SpellBook.IsSpellKnown(conjureRefreshment))
            {
                Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(conjureWaterSpell);
                spell.GetRank(out int spellRank);
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

            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(conjureFoodSpell) && !WowInterface.CharacterManager.SpellBook.IsSpellKnown(conjureRefreshment))
            {
                Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(conjureFoodSpell);
                spell.GetRank(out int spellRank);
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
                spell.GetRank(out int spellRank);

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

            if (MyAuraManager.Tick()
                || GroupAuraManager.Tick())
            {
                return;
            }
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