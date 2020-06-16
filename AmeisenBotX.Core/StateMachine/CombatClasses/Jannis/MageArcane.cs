using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
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
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string arcaneBarrageSpell = "Arcane Barrage";
        private const string arcaneBlastSpell = "Arcane Blast";
        private const string arcaneIntellectSpell = "Arcane Intellect";
        private const string arcaneMissilesSpell = "Arcane Missiles";
        private const string counterspellSpell = "Counterspell";
        private const string evocationSpell = "Evocation";
        private const string fireballSpell = "Fireball";
        private const string iceBlockSpell = "Ice Block";
        private const string icyVeinsSpell = "Icy Veins";
        private const string mageArmorSpell = "Mage Armor";
        private const string manaShieldSpell = "Mana Shield";
        private const string mirrorImageSpell = "Mirror Image";
        private const string missileBarrageSpell = "Missile Barrage";
        private const string spellStealSpell = "Spellsteal";
#pragma warning restore IDE0051

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

        public override string Version => "1.0";

        public override void ExecuteCC()
        {
            if (TargetAuraManager.Tick()
                || TargetInterruptManager.Tick())
            {
                return;
            }

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