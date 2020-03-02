using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.StateMachine.Enums;
using System;
using System.Collections.Generic;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;
using static AmeisenBotX.Core.StateMachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
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
                { arcaneIntellectSpell, () =>
                    {
                        HookManager.TargetGuid(WowInterface.ObjectManager.PlayerGuid);
                        return CastSpellIfPossible(arcaneIntellectSpell, true);
                    }
                },
                { mageArmorSpell, () => CastSpellIfPossible(mageArmorSpell, true) },
                { manaShieldSpell, () => CastSpellIfPossible(manaShieldSpell, true) }
            };

            TargetAuraManager.DispellBuffs = () => HookManager.HasUnitStealableBuffs(WowLuaUnit.Target) && CastSpellIfPossible(spellStealSpell, true);

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(counterspellSpell, true) }
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

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator();

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

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick())
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                if ((WowInterface.ObjectManager.Player.HealthPercentage < 16
                        && CastSpellIfPossible(iceBlockSpell))
                    || (WowInterface.ObjectManager.Player.ManaPercentage < 40
                        && CastSpellIfPossible(evocationSpell, true))
                    || CastSpellIfPossible(mirrorImageSpell, true)
                    || (MyAuraManager.Buffs.Contains(missileBarrageSpell.ToLower()) && CastSpellIfPossible(arcaneMissilesSpell, true))
                    || CastSpellIfPossible(arcaneBarrageSpell, true)
                    || CastSpellIfPossible(arcaneBlastSpell, true))
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