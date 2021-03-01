using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class ShamanElemental : BasicCombatClass
    {
        public ShamanElemental(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(lightningShieldSpell, () => WowInterface.ObjectManager.Player.ManaPercentage > 60.0 && TryCastSpell(lightningShieldSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(waterShieldSpell, () => WowInterface.ObjectManager.Player.ManaPercentage < 20.0 && TryCastSpell(waterShieldSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(flameShockSpell, () => TryCastSpell(flameShockSpell, WowInterface.ObjectManager.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(windShearSpell, x.Guid, true) },
                { 1, (x) => TryCastSpell(hexSpell, x.Guid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Elemental Shaman spec.";

        public override string Displayname => "Shaman Elemental";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(null, new() { WowWeaponType.TWOHANDED_AXES, WowWeaponType.TWOHANDED_MACES, WowWeaponType.TWOHANDED_SWORDS });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 3) },
                { 2, new(1, 2, 5) },
                { 3, new(1, 3, 3) },
                { 7, new(1, 7, 1) },
                { 8, new(1, 8, 5) },
                { 9, new(1, 9, 2) },
                { 10, new(1, 10, 3) },
                { 11, new(1, 11, 2) },
                { 12, new(1, 12, 1) },
                { 13, new(1, 13, 3) },
                { 14, new(1, 14, 3) },
                { 15, new(1, 15, 5) },
                { 16, new(1, 16, 1) },
                { 17, new(1, 17, 3) },
                { 18, new(1, 18, 2) },
                { 19, new(1, 19, 2) },
                { 20, new(1, 20, 3) },
                { 22, new(1, 22, 1) },
                { 23, new(1, 23, 3) },
                { 24, new(1, 24, 5) },
                { 25, new(1, 25, 1) },
            },
            Tree2 = new()
            {
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 8, new(2, 8, 3) },
                { 9, new(2, 9, 1) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Shaman;

        private bool HexedTarget { get; set; }

        private DateTime LastDeadPartymembersCheck { get; set; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetManagerDps))
            {
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
                    if ((WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 6
                            && TryCastSpell(thunderstormSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || (WowInterface.ObjectManager.Target.MaxHealth > 10000000
                            && WowInterface.ObjectManager.Target.HealthPercentage < 25
                            && TryCastSpell(heroismSpell, 0))
                        || TryCastSpell(lavaBurstSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || TryCastSpell(elementalMasterySpell, 0))
                    {
                        return;
                    }

                    if ((WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 16).Count() > 2 && TryCastSpell(chainLightningSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || TryCastSpell(lightningBoltSpell, WowInterface.ObjectManager.TargetGuid, true))
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

            if (CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_MAINHAND, flametongueBuff, flametongueWeaponSpell))
            {
                return;
            }

            HexedTarget = false;
        }
    }
}