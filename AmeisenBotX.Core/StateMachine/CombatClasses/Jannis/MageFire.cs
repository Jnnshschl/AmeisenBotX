using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class MageFire : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string arcaneIntellectSpell = "Arcane Intellect";
        private const string counterspellSpell = "Counterspell";
        private const string evocationSpell = "Evocation";
        private const string fireballSpell = "Fireball";
        private const string hotstreakSpell = "Hot Streak";
        private const string iceBlockSpell = "Ice Block";
        private const string livingBombSpell = "Living Bomb";
        private const string manaShieldSpell = "Mana Shield";
        private const string mirrorImageSpell = "Mirror Image";
        private const string moltenArmorSpell = "Molten Armor";
        private const string pyroblastSpell = "Pyroblast";
        private const string scorchSpell = "Scorch";
        private const string spellStealSpell = "Spellsteal";
#pragma warning restore IDE0051

        public MageFire(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { arcaneIntellectSpell, () => CastSpellIfPossible(arcaneIntellectSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { moltenArmorSpell, () => CastSpellIfPossible(moltenArmorSpell, 0, true) },
                { manaShieldSpell, () => CastSpellIfPossible(manaShieldSpell, 0, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { scorchSpell, () => CastSpellIfPossible(scorchSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { livingBombSpell, () => CastSpellIfPossible(livingBombSpell, WowInterface.ObjectManager.TargetGuid, true) }
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

        public override string Description => "FCFS based CombatClass for the Fire Mage spec.";

        public override string Displayname => "Mage Fire";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIEDLS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

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
                if (CastSpellIfPossible(mirrorImageSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 16
                        && CastSpellIfPossible(iceBlockSpell, 0, true))
                    || (WowInterface.ObjectManager.Player.HasBuffByName(hotstreakSpell.ToLower()) && CastSpellIfPossible(pyroblastSpell, WowInterface.ObjectManager.TargetGuid, true))
                    || (WowInterface.ObjectManager.Player.ManaPercentage < 40
                        && CastSpellIfPossible(evocationSpell, WowInterface.ObjectManager.TargetGuid, true))
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