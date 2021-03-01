using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class MageFire : BasicCombatClass
    {
        public MageFire(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(arcaneIntellectSpell, () => TryCastSpell(arcaneIntellectSpell, WowInterface.ObjectManager.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(moltenArmorSpell, () => TryCastSpell(moltenArmorSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(manaShieldSpell, () => TryCastSpell(manaShieldSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(scorchSpell, () => TryCastSpell(scorchSpell, WowInterface.ObjectManager.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(livingBombSpell, () => TryCastSpell(livingBombSpell, WowInterface.ObjectManager.TargetGuid, true)));

            // TargetAuraManager.DispellBuffs = () => WowInterface.HookManager.LuaHasUnitStealableBuffs(WowLuaUnit.Target) && TryCastSpell(spellStealSpell, WowInterface.ObjectManager.TargetGuid, true);

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(counterspellSpell, x.Guid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((arcaneIntellectSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Fire Mage spec.";

        public override string Displayname => "Mage Fire";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 2) },
                { 2, new(1, 2, 3) },
                { 6, new(1, 6, 5) },
                { 8, new(1, 8, 3) },
                { 9, new(1, 9, 1) },
                { 10, new(1, 10, 1) },
                { 14, new(1, 14, 3) },
            },
            Tree2 = new()
            {
                { 3, new(2, 3, 5) },
                { 4, new(2, 4, 5) },
                { 6, new(2, 6, 3) },
                { 7, new(2, 7, 2) },
                { 9, new(2, 9, 1) },
                { 10, new(2, 10, 2) },
                { 11, new(2, 11, 3) },
                { 13, new(2, 13, 3) },
                { 14, new(2, 14, 3) },
                { 15, new(2, 15, 3) },
                { 18, new(2, 18, 5) },
                { 19, new(2, 19, 3) },
                { 20, new(2, 20, 1) },
                { 21, new(2, 21, 2) },
                { 23, new(2, 23, 3) },
                { 27, new(2, 27, 5) },
                { 28, new(2, 28, 1) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Mage;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetManagerDps))
            {
                if (WowInterface.ObjectManager.Target != null)
                {
                    if (TryCastSpell(mirrorImageSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || (WowInterface.ObjectManager.Player.HealthPercentage < 16
                            && TryCastSpell(iceBlockSpell, 0, true))
                        || (WowInterface.ObjectManager.Player.HasBuffByName(hotstreakSpell.ToLowerInvariant()) && TryCastSpell(pyroblastSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || (WowInterface.ObjectManager.Player.ManaPercentage < 40
                            && TryCastSpell(evocationSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || TryCastSpell(fireballSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
        }
    }
}