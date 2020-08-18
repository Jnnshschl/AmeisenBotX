using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
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
        public ShamanEnhancement(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { lightningShieldSpell, () => WowInterface.ObjectManager.Player.ManaPercentage > 60.0 && TryCastSpell(lightningShieldSpell, 0, true) },
                { waterShieldSpell, () => WowInterface.ObjectManager.Player.ManaPercentage < 20.0 && TryCastSpell(waterShieldSpell, 0, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { flameShockSpell, () => TryCastSpell(flameShockSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpell(windShearSpell, x.Guid, true) },
                { 1, (x) => TryCastSpell(hexSpell, x.Guid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Enhancement Shaman spec.";

        public override string Displayname => "Shaman Enhancement";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.TWOHANDED_AXES, WeaponType.TWOHANDED_MACES, WeaponType.TWOHANDED_SWORDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(1, 2, 5) },
                { 3, new Talent(1, 3, 3) },
                { 5, new Talent(1, 5, 3) },
                { 8, new Talent(1, 8, 5) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 7, new Talent(2, 7, 3) },
                { 8, new Talent(2, 8, 3) },
                { 9, new Talent(2, 9, 1) },
                { 11, new Talent(2, 11, 5) },
                { 13, new Talent(2, 13, 2) },
                { 14, new Talent(2, 14, 1) },
                { 15, new Talent(2, 15, 3) },
                { 16, new Talent(2, 16, 3) },
                { 17, new Talent(2, 17, 3) },
                { 19, new Talent(2, 19, 3) },
                { 20, new Talent(2, 20, 1) },
                { 21, new Talent(2, 21, 1) },
                { 22, new Talent(2, 22, 3) },
                { 23, new Talent(2, 23, 1) },
                { 24, new Talent(2, 24, 2) },
                { 25, new Talent(2, 25, 3) },
                { 26, new Talent(2, 26, 1) },
                { 28, new Talent(2, 28, 5) },
                { 29, new Talent(2, 29, 1) },
            },
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Shaman;

        private bool HexedTarget { get; set; }

        private DateTime LastDeadPartymembersCheck { get; set; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(DpsTargetManager))
            {
                if (CheckForWeaponEnchantment(EquipmentSlot.INVSLOT_MAINHAND, flametoungueBuff, flametoungueWeaponSpell)
                    || CheckForWeaponEnchantment(EquipmentSlot.INVSLOT_OFFHAND, windfuryBuff, windfuryWeaponSpell))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Player.HealthPercentage < 30
                    && WowInterface.ObjectManager.Target.Type == WowObjectType.Player
                    && TryCastSpell(hexSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    HexedTarget = true;
                    return;
                }

                if (WowInterface.ObjectManager.Player.HealthPercentage < 60
                    && TryCastSpell(healingWaveSpell, WowInterface.ObjectManager.PlayerGuid, true))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Target != null)
                {
                    if ((WowInterface.ObjectManager.Target.MaxHealth > 10000000
                            && WowInterface.ObjectManager.Target.HealthPercentage < 25
                            && TryCastSpell(heroismSpell, 0))
                        || TryCastSpell(stormstrikeSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || TryCastSpell(lavaLashSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || TryCastSpell(earthShockSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HasBuffByName(maelstromWeaponSpell)
                        && WowInterface.ObjectManager.Player.Auras.FirstOrDefault(e => e.Name == maelstromWeaponSpell).StackCount >= 5
                        && TryCastSpell(lightningBoltSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (HandleDeadPartymembers(ancestralSpiritSpell))
            {
                return;
            }

            if (CheckForWeaponEnchantment(EquipmentSlot.INVSLOT_MAINHAND, flametoungueBuff, flametoungueWeaponSpell)
                || CheckForWeaponEnchantment(EquipmentSlot.INVSLOT_OFFHAND, windfuryBuff, windfuryWeaponSpell))
            {
                return;
            }

            HexedTarget = false;
        }
    }
}