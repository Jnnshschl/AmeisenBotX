using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.StateMachine.CombatClasses.Shino;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Shino
{
    public class PriestShadow : TemplateCombatClass
    {
        public PriestShadow(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { shadowformSpell, () => TryCastSpell(shadowformSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { powerWordFortitudeSpell, () => TryCastSpell(powerWordFortitudeSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { vampiricEmbraceSpell, () => TryCastSpell(vampiricEmbraceSpell, WowInterface.ObjectManager.PlayerGuid, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { vampiricTouchSpell, () => TryCastSpell(vampiricTouchSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { devouringPlagueSpell, () => TryCastSpell(devouringPlagueSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { shadowWordPainSpell, () => TryCastSpell(shadowWordPainSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { mindBlastSpell, () => TryCastSpell(mindBlastSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((powerWordFortitudeSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Shadow Priest spec.";

        public override string Displayname => "Priest Shadow";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(1, 2, 5) },
                { 4, new Talent(1, 4, 3) },
                { 5, new Talent(1, 5, 2) },
                { 7, new Talent(1, 7, 3) },
            },
            Tree2 = new Dictionary<int, Talent>(),
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 3) },
                { 2, new Talent(3, 2, 2) },
                { 3, new Talent(3, 3, 5) },
                { 5, new Talent(3, 5, 2) },
                { 6, new Talent(3, 6, 3) },
                { 8, new Talent(3, 8, 5) },
                { 9, new Talent(3, 9, 1) },
                { 10, new Talent(3, 10, 2) },
                { 11, new Talent(3, 11, 2) },
                { 12, new Talent(3, 12, 3) },
                { 14, new Talent(3, 14, 1) },
                { 16, new Talent(3, 16, 3) },
                { 17, new Talent(3, 17, 2) },
                { 18, new Talent(3, 18, 3) },
                { 19, new Talent(3, 19, 1) },
                { 20, new Talent(3, 20, 5) },
                { 21, new Talent(3, 21, 2) },
                { 22, new Talent(3, 22, 3) },
                { 24, new Talent(3, 24, 1) },
                { 25, new Talent(3, 25,3 ) },
                { 26, new Talent(3, 26, 5) },
                { 27, new Talent(3, 27, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.2";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Priest;

        private DateTime LastDeadPartymembersCheck { get; set; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetManagerDps))
            {
                if (WowInterface.ObjectManager.Player.ManaPercentage < 90
                    && TryCastSpell(shadowfiendSpell, WowInterface.ObjectManager.TargetGuid))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Player.ManaPercentage < 30
                    && TryCastSpell(hymnOfHopeSpell, 0))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Player.HealthPercentage < 70
                    && TryCastSpell(flashHealSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Player.ManaPercentage >= 50 
                    && TryCastSpell(berserkingSpell, WowInterface.ObjectManager.TargetGuid))
                {
                    return;
                }

                if (!WowInterface.ObjectManager.Player.IsCasting
                    && TryCastSpell(mindFlaySpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(smiteSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (HandleDeadPartymembers(resurrectionSpell))
            {
                return;
            }
        }

        protected override Spell GetOpeningSpell()
        {
            Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(shadowWordPainSpell);
            if (spell != null)
            {
                return spell;
            }
            return WowInterface.CharacterManager.SpellBook.GetSpellByName(smiteSpell);
        }
    }
}