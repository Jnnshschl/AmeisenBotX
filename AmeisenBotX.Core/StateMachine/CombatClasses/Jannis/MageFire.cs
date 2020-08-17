using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class MageFire : BasicCombatClass
    {
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

        public override WowClass WowClass => WowClass.Mage;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Fire Mage spec.";

        public override string Displayname => "Mage Fire";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 2) },
                { 2, new Talent(1, 2, 3) },
                { 6, new Talent(1, 6, 5) },
                { 8, new Talent(1, 8, 3) },
                { 9, new Talent(1, 9, 1) },
                { 10, new Talent(1, 10, 1) },
                { 14, new Talent(1, 14, 3) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(2, 3, 5) },
                { 4, new Talent(2, 4, 5) },
                { 6, new Talent(2, 6, 3) },
                { 7, new Talent(2, 7, 2) },
                { 9, new Talent(2, 9, 1) },
                { 10, new Talent(2, 10, 2) },
                { 11, new Talent(2, 11, 3) },
                { 13, new Talent(2, 13, 3) },
                { 14, new Talent(2, 14, 3) },
                { 15, new Talent(2, 15, 3) },
                { 18, new Talent(2, 18, 5) },
                { 19, new Talent(2, 19, 3) },
                { 20, new Talent(2, 20, 1) },
                { 21, new Talent(2, 21, 2) },
                { 23, new Talent(2, 23, 3) },
                { 27, new Talent(2, 27, 5) },
                { 28, new Talent(2, 28, 1) },
            },
            Tree3 = new Dictionary<int, Talent>(),
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
                    if (CastSpellIfPossible(mirrorImageSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || (WowInterface.ObjectManager.Player.HealthPercentage < 16
                            && CastSpellIfPossible(iceBlockSpell, 0, true))
                        || (WowInterface.ObjectManager.Player.HasBuffByName(hotstreakSpell.ToLowerInvariant()) && CastSpellIfPossible(pyroblastSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || (WowInterface.ObjectManager.Player.ManaPercentage < 40
                            && CastSpellIfPossible(evocationSpell, WowInterface.ObjectManager.TargetGuid, true))
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