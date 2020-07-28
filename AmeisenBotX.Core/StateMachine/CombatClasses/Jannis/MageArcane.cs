using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class MageArcane : BasicCombatClass
    {
        public MageArcane(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { arcaneIntellectSpell, () => CastSpellIfPossible(arcaneIntellectSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { mageArmorSpell, () => CastSpellIfPossible(mageArmorSpell, 0, true) },
                { manaShieldSpell, () => CastSpellIfPossible(manaShieldSpell, 0, true) }
            };

            TargetAuraManager.DispellBuffs = () => WowInterface.HookManager.HasUnitStealableBuffs(WowLuaUnit.Target) && CastSpellIfPossible(spellStealSpell, WowInterface.ObjectManager.TargetGuid, true);

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossible(counterspellSpell, x.Guid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((arcaneIntellectSpell, (spellName, guid) => CastSpellIfPossible(spellName, guid, true)));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Mage;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Arcane Mage spec.";

        public override string Displayname => "Mage Arcane";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public DateTime LastSpellstealCheck { get; private set; }

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 2) },
                { 2, new Talent(1, 2, 3) },
                { 3, new Talent(1, 3, 5) },
                { 6, new Talent(1, 6, 5) },
                { 8, new Talent(1, 8, 2) },
                { 9, new Talent(1, 9, 1) },
                { 10, new Talent(1, 10, 1) },
                { 13, new Talent(1, 13, 2) },
                { 14, new Talent(1, 14, 3) },
                { 16, new Talent(1, 16, 1) },
                { 17, new Talent(1, 17, 5) },
                { 19, new Talent(1, 19, 3) },
                { 20, new Talent(1, 20, 2) },
                { 23, new Talent(1, 23, 3) },
                { 24, new Talent(1, 24, 1) },
                { 25, new Talent(1, 25, 5) },
                { 27, new Talent(1, 27, 5) },
                { 28, new Talent(1, 28, 3) },
                { 29, new Talent(1, 29, 2) },
                { 30, new Talent(1, 30, 1) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(2, 2, 3) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 2) },
                { 3, new Talent(3, 3, 3) },
                { 5, new Talent(3, 5, 2) },
                { 6, new Talent(3, 6, 3) },
                { 9, new Talent(3, 9, 1) },
            },
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override void ExecuteCC()
        {
            if (SelectTarget(DpsTargetManager))
            {
                if (WowInterface.ObjectManager.Target != null)
                {
                    if ((WowInterface.ObjectManager.Player.HealthPercentage < 16
                            && CastSpellIfPossible(iceBlockSpell, 0))
                        || (WowInterface.ObjectManager.Player.ManaPercentage < 40
                            && CastSpellIfPossible(evocationSpell, 0, true))
                        || CastSpellIfPossible(mirrorImageSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || (WowInterface.ObjectManager.Player.HasBuffByName(missileBarrageSpell) && CastSpellIfPossible(arcaneMissilesSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || CastSpellIfPossible(arcaneBarrageSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || CastSpellIfPossible(arcaneBlastSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || CastSpellIfPossible(fireballSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || GroupAuraManager.Tick())
            {
                return;
            }
        }
    }
}