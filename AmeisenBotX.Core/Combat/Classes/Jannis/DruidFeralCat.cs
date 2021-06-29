using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class DruidFeralCat : BasicCombatClass
    {
        public DruidFeralCat(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(markOfTheWildSpell, () => TryCastSpell(markOfTheWildSpell, WowInterface.Player.Guid, true, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(catFormSpell, () => TryCastSpell(catFormSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(savageRoarSpell, () => TryCastSpellRogue(savageRoarSpell, WowInterface.Target.Guid, true, true, 1)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(ripSpell, () => WowInterface.Player.ComboPoints == 5 && TryCastSpellRogue(ripSpell, WowInterface.Target.Guid, true, true, 5)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(rakeSpell, () => TryCastSpell(rakeSpell, WowInterface.Target.Guid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(mangleCatSpell, () => TryCastSpell(mangleCatSpell, WowInterface.Target.Guid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(faerieFireSpell, x.Guid, true) },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((markOfTheWildSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Feral (Cat) Druid spec.";

        public override string Displayname => "Druid Feral Cat";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicAgilityComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new(),
            Tree2 = new()
            {
                { 1, new(2, 1, 5) },
                { 2, new(2, 2, 5) },
                { 4, new(2, 4, 2) },
                { 6, new(2, 6, 2) },
                { 7, new(2, 7, 1) },
                { 8, new(2, 8, 3) },
                { 9, new(2, 9, 2) },
                { 10, new(2, 10, 3) },
                { 11, new(2, 11, 2) },
                { 12, new(2, 12, 2) },
                { 14, new(2, 14, 1) },
                { 17, new(2, 17, 5) },
                { 18, new(2, 18, 3) },
                { 19, new(2, 19, 1) },
                { 20, new(2, 20, 2) },
                { 23, new(2, 23, 3) },
                { 25, new(2, 25, 3) },
                { 26, new(2, 26, 1) },
                { 28, new(2, 28, 5) },
                { 29, new(2, 29, 1) },
                { 30, new(2, 30, 1) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 2) },
                { 3, new(3, 3, 5) },
                { 4, new(3, 4, 5) },
                { 6, new(3, 6, 3) },
                { 8, new(3, 8, 1) },
                { 9, new(3, 9, 2) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => true;

        public override WowClass WowClass => WowClass.Druid;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                double distanceToTarget = WowInterface.Player.Position.GetDistance(WowInterface.Target.Position);

                if (distanceToTarget > 9.0
                    && TryCastSpell(feralChargeBearSpell, WowInterface.Target.Guid, true))
                {
                    return;
                }

                if (distanceToTarget > 8.0
                    && TryCastSpell(dashSpell, 0))
                {
                    return;
                }

                if (WowInterface.Player.HealthPercentage < 40
                    && TryCastSpell(survivalInstinctsSpell, 0, true))
                {
                    return;
                }

                if (TryCastSpell(berserkSpell, 0))
                {
                    return;
                }

                if (NeedToHealMySelf())
                {
                    return;
                }

                if ((WowInterface.Player.EnergyPercentage > 70
                        && TryCastSpell(berserkSpell, 0))
                    || (WowInterface.Player.Energy < 30
                        && TryCastSpell(tigersFurySpell, 0))
                    || (WowInterface.Player.HealthPercentage < 70
                        && TryCastSpell(barkskinSpell, 0, true))
                    || (WowInterface.Player.HealthPercentage < 35
                        && TryCastSpell(survivalInstinctsSpell, 0, true))
                    || (WowInterface.Player.ComboPoints == 5
                        && TryCastSpellRogue(ferociousBiteSpell, WowInterface.Target.Guid, true, true, 5))
                    || TryCastSpell(shredSpell, WowInterface.Target.Guid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealMySelf())
            {
                return;
            }
        }

        private bool NeedToHealMySelf()
        {
            if (WowInterface.Player.HealthPercentage < 60
                && !WowInterface.Player.HasBuffByName(rejuvenationSpell)
                && TryCastSpell(rejuvenationSpell, 0, true))
            {
                return true;
            }

            if (WowInterface.Player.HealthPercentage < 40
                && TryCastSpell(healingTouchSpell, 0, true))
            {
                return true;
            }

            return false;
        }
    }
}