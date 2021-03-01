using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class MageArcane : BasicCombatClass
    {
        public MageArcane(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(arcaneIntellectSpell, () => TryCastSpell(arcaneIntellectSpell, WowInterface.ObjectManager.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(mageArmorSpell, () => TryCastSpell(mageArmorSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(manaShieldSpell, () => TryCastSpell(manaShieldSpell, 0, true)));

            // TargetAuraManager.DispellBuffs = () => WowInterface.HookManager.LuaHasUnitStealableBuffs(WowLuaUnit.Target) && TryCastSpell(spellStealSpell, WowInterface.ObjectManager.TargetGuid, true);

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(counterspellSpell, x.Guid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((arcaneIntellectSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Arcane Mage spec.";

        public override string Displayname => "Mage Arcane";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public DateTime LastSpellstealCheck { get; private set; }

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 2) },
                { 2, new(1, 2, 3) },
                { 3, new(1, 3, 5) },
                { 6, new(1, 6, 5) },
                { 8, new(1, 8, 2) },
                { 9, new(1, 9, 1) },
                { 10, new(1, 10, 1) },
                { 13, new(1, 13, 2) },
                { 14, new(1, 14, 3) },
                { 16, new(1, 16, 1) },
                { 17, new(1, 17, 5) },
                { 19, new(1, 19, 3) },
                { 20, new(1, 20, 2) },
                { 23, new(1, 23, 3) },
                { 24, new(1, 24, 1) },
                { 25, new(1, 25, 5) },
                { 27, new(1, 27, 5) },
                { 28, new(1, 28, 3) },
                { 29, new(1, 29, 2) },
                { 30, new(1, 30, 1) },
            },
            Tree2 = new()
            {
                { 2, new(2, 2, 3) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 2) },
                { 3, new(3, 3, 3) },
                { 5, new(3, 5, 2) },
                { 6, new(3, 6, 3) },
                { 9, new(3, 9, 1) },
            },
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
                    if ((WowInterface.ObjectManager.Player.HealthPercentage < 16
                            && TryCastSpell(iceBlockSpell, 0))
                        || (WowInterface.ObjectManager.Player.ManaPercentage < 40
                            && TryCastSpell(evocationSpell, 0, true))
                        || TryCastSpell(mirrorImageSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || (WowInterface.ObjectManager.Player.HasBuffByName(missileBarrageSpell) && TryCastSpell(arcaneMissilesSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || TryCastSpell(arcaneBarrageSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || TryCastSpell(arcaneBlastSpell, WowInterface.ObjectManager.TargetGuid, true)
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