using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class ShamanElemental : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string ancestralSpiritSpell = "Ancestral Spirit";
        private const string chainLightningSpell = "Chain Lightning";
        private const int deadPartymembersCheckTime = 4;
        private const string elementalMasterySpell = "Elemental Mastery";
        private const string flameShockSpell = "Flame Shock";
        private const string flametongueBuff = "Flametongue ";
        private const string flametongueWeaponSpell = "Flametongue Weapon";
        private const string healingWaveSpell = "Healing Wave";
        private const string heroismSpell = "Heroism";
        private const string hexSpell = "Hex";
        private const string lavaBurstSpell = "Lava Burst";
        private const string lesserHealingWaveSpell = "Lesser Healing Wave";
        private const string lightningBoltSpell = "Lightning Bolt";
        private const string lightningShieldSpell = "Lightning Shield";
        private const string thunderstormSpell = "Thunderstorm";
        private const string waterShieldSpell = "Water Shield";
        private const string windShearSpell = "Wind Shear";
#pragma warning restore IDE0051

        public ShamanElemental(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { lightningShieldSpell, () => CastSpellIfPossible(lightningShieldSpell, 0, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { flameShockSpell, () => CastSpellIfPossible(flameShockSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossible(windShearSpell, x.Guid, true) },
                { 1, (x) => CastSpellIfPossible(hexSpell, x.Guid, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Shaman;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Elemental Shaman spec.";

        public override string Displayname => "Shaman Elemental";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(null, new List<WeaponType>() { WeaponType.TWOHANDED_AXES, WeaponType.TWOHANDED_MACES, WeaponType.TWOHANDED_SWORDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private bool HexedTarget { get; set; }

        private DateTime LastDeadPartymembersCheck { get; set; }

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 3) },
                { 2, new Talent(1, 2, 5) },
                { 3, new Talent(1, 3, 3) },
                { 7, new Talent(1, 7, 1) },
                { 8, new Talent(1, 8, 5) },
                { 9, new Talent(1, 9, 2) },
                { 10, new Talent(1, 10, 3) },
                { 11, new Talent(1, 11, 2) },
                { 12, new Talent(1, 12, 1) },
                { 13, new Talent(1, 13, 3) },
                { 14, new Talent(1, 14, 3) },
                { 15, new Talent(1, 15, 5) },
                { 16, new Talent(1, 16, 1) },
                { 17, new Talent(1, 17, 3) },
                { 18, new Talent(1, 18, 2) },
                { 19, new Talent(1, 19, 2) },
                { 20, new Talent(1, 20, 3) },
                { 22, new Talent(1, 22, 1) },
                { 23, new Talent(1, 23, 3) },
                { 24, new Talent(1, 24, 5) },
                { 25, new Talent(1, 25, 1) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 8, new Talent(2, 8, 3) },
                { 9, new Talent(2, 9, 1) },
            },
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override bool UseAutoAttacks => false;

        public override bool WalkBehindEnemy => false;

        public override void ExecuteCC()
        {
            if ((!WowInterface.ObjectManager.Player.HasBuffByName(lightningShieldSpell) && WowInterface.ObjectManager.Player.ManaPercentage > 60.0 && CastSpellIfPossible(lightningShieldSpell, 0))
                || !WowInterface.ObjectManager.Player.HasBuffByName(waterShieldSpell) && WowInterface.ObjectManager.Player.ManaPercentage < 20.0 && CastSpellIfPossible(waterShieldSpell, 0))
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 30
                && WowInterface.ObjectManager.Target.Type == WowObjectType.Player
                && CastSpellIfPossible(hexSpell, WowInterface.ObjectManager.TargetGuid, true))
            {
                HexedTarget = true;
                return;
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 60
                && CastSpellIfPossible(healingWaveSpell, WowInterface.ObjectManager.PlayerGuid, true))
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                if ((WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 6
                        && CastSpellIfPossible(thunderstormSpell, WowInterface.ObjectManager.TargetGuid, true))
                    || (WowInterface.ObjectManager.Target.MaxHealth > 10000000
                        && WowInterface.ObjectManager.Target.HealthPercentage < 25
                        && CastSpellIfPossible(heroismSpell, 0))
                    || CastSpellIfPossible(lavaBurstSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || CastSpellIfPossible(elementalMasterySpell, 0))
                {
                    return;
                }

                if ((WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 16).Count() > 2 && CastSpellIfPossible(chainLightningSpell, WowInterface.ObjectManager.TargetGuid, true))
                    || CastSpellIfPossible(lightningBoltSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime)
                && HandleDeadPartymembers(ancestralSpiritSpell))
            {
                return;
            }

            if (CheckForWeaponEnchantment(EquipmentSlot.INVSLOT_MAINHAND, flametongueBuff, flametongueWeaponSpell))
            {
                return;
            }

            if (HexedTarget)
            {
                HexedTarget = false;
            }
        }
    }
}