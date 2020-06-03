using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
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
    public class ShamanEnhancement : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const int deadPartymembersCheckTime = 4;
        private const string ancestralSpiritSpell = "Ancestral Spirit";
        private const string earthShockSpell = "Earth Shock";
        private const string feralSpiritSpell = "Feral Spirit";
        private const string flameShockSpell = "Flame Shock";
        private const string flametoungueBuff = "Flametongue ";
        private const string flametoungueWeaponSpell = "Flametongue Weapon";
        private const string healingWaveSpell = "Healing Wave";
        private const string heroismSpell = "Heroism";
        private const string hexSpell = "Hex";
        private const string lavaLashSpell = "Lava Lash";
        private const string lesserHealingWaveSpell = "Lesser Healing Wave";
        private const string lightningBoltSpell = "Lightning Bolt";
        private const string lightningShieldSpell = "Lightning Shield";
        private const string maelstromWeaponSpell = "Mealstrom Weapon";
        private const string shamanisticRageSpell = "Shamanistic Rage";
        private const string stormstrikeSpell = "Stormstrike";
        private const string waterShieldSpell = "Water Shield";
        private const string windfuryBuff = "Windfury ";
        private const string windfuryWeaponSpell = "Windfury Weapon";
        private const string windShearSpell = "Wind Shear";
#pragma warning restore IDE0051

        public ShamanEnhancement(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>();

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { flameShockSpell, () => CastSpellIfPossible(flameShockSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossible(windShearSpell, x.Guid, true) },
                { 1, (x) => CastSpellIfPossible(hexSpell, x.Guid, true) }
            };

            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(4000));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Shaman;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Enhancement Shaman spec.";

        public override string Displayname => "Shaman Enhancement";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIEDLS }, new List<WeaponType>() { WeaponType.TWOHANDED_AXES, WeaponType.TWOHANDED_MACES, WeaponType.TWOHANDED_SWORDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private bool HexedTarget { get; set; }

        private DateTime LastDeadPartymembersCheck { get; set; }

        private TimegatedEvent AutoAttackEvent { get; set; }

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run())
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if (TargetInterruptManager.Tick())
            {
                return;
            }

            if ((!WowInterface.ObjectManager.Player.HasBuffByName(lightningShieldSpell) && WowInterface.ObjectManager.Player.ManaPercentage > 60.0 && CastSpellIfPossible(lightningShieldSpell, 0))
                || !WowInterface.ObjectManager.Player.HasBuffByName(waterShieldSpell) && WowInterface.ObjectManager.Player.ManaPercentage < 20.0 && CastSpellIfPossible(waterShieldSpell, 0))
            {
                return;
            }

            if (CheckForWeaponEnchantment(EquipmentSlot.INVSLOT_MAINHAND, flametoungueBuff, flametoungueWeaponSpell)
                || CheckForWeaponEnchantment(EquipmentSlot.INVSLOT_OFFHAND, windfuryBuff, windfuryWeaponSpell))
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
                if ((WowInterface.ObjectManager.Target.MaxHealth > 10000000
                        && WowInterface.ObjectManager.Target.HealthPercentage < 25
                        && CastSpellIfPossible(heroismSpell, 0))
                    || CastSpellIfPossible(stormstrikeSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || CastSpellIfPossible(lavaLashSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || CastSpellIfPossible(earthShockSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Player.HasBuffByName(maelstromWeaponSpell)
                    && WowInterface.ObjectManager.Player.Auras.FirstOrDefault(e => e.Name == maelstromWeaponSpell)?.StackCount >= 5
                    && CastSpellIfPossible(lightningBoltSpell, WowInterface.ObjectManager.TargetGuid, true))
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

            if (CheckForWeaponEnchantment(EquipmentSlot.INVSLOT_MAINHAND, flametoungueBuff, flametoungueWeaponSpell)
                || CheckForWeaponEnchantment(EquipmentSlot.INVSLOT_OFFHAND, windfuryBuff, windfuryWeaponSpell))
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