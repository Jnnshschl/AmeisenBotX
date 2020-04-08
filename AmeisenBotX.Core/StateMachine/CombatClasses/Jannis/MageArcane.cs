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
    public class MageArcane : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string arcaneBarrageSpell = "Arcane Barrage";
        private readonly string arcaneBlastSpell = "Arcane Blast";
        private readonly string arcaneIntellectSpell = "Arcane Intellect";
        private readonly string arcaneMissilesSpell = "Arcane Missiles";
        private readonly string counterspellSpell = "Counterspell";
        private readonly string evocationSpell = "Evocation";
        private readonly string fireballSpell = "Fireball";
        private readonly string iceBlockSpell = "Ice Block";
        private readonly string icyVeinsSpell = "Icy Veins";
        private readonly string mageArmorSpell = "Mage Armor";
        private readonly string manaShieldSpell = "Mana Shield";
        private readonly string mirrorImageSpell = "Mirror Image";
        private readonly string missileBarrageSpell = "Missile Barrage";
        private readonly string spellStealSpell = "Spellsteal";

        public MageArcane(WowInterface wowInterface) : base(wowInterface)
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
                { 0, () => CastSpellIfPossible(counterspellSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Mage;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Arcane Mage spec.";

        public override string Displayname => "Mage Arcane";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIEDLS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public DateTime LastSpellstealCheck { get; private set; }

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (TargetManager.GetUnitToTarget(out List<WowUnit> targetToTarget))
            {
                WowInterface.HookManager.TargetGuid(targetToTarget.First().Guid);
                WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Player);
            }

            if (WowInterface.ObjectManager.Target == null || WowInterface.ObjectManager.Target.IsDead || !BotUtils.IsValidUnit(WowInterface.ObjectManager.Target))
            {
                return;
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
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
                    || (MyAuraManager.Buffs.Contains(missileBarrageSpell.ToLower()) && CastSpellIfPossible(arcaneMissilesSpell, WowInterface.ObjectManager.TargetGuid, true))
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
            if (MyAuraManager.Tick())
            {
                return;
            }
        }
    }
}