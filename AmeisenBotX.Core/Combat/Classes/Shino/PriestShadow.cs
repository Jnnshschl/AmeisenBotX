using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.CombatClasses.Shino;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Combat.Classes.Shino
{
    public class PriestShadow : TemplateCombatClass
    {
        public PriestShadow(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(shadowformSpell, () => TryCastSpell(shadowformSpell, WowInterface.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(powerWordFortitudeSpell, () => TryCastSpell(powerWordFortitudeSpell, WowInterface.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(vampiricEmbraceSpell, () => TryCastSpell(vampiricEmbraceSpell, WowInterface.PlayerGuid, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(vampiricTouchSpell, () => TryCastSpell(vampiricTouchSpell, WowInterface.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(devouringPlagueSpell, () => TryCastSpell(devouringPlagueSpell, WowInterface.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(shadowWordPainSpell, () => TryCastSpell(shadowWordPainSpell, WowInterface.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(mindBlastSpell, () => TryCastSpell(mindBlastSpell, WowInterface.TargetGuid, true)));

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((powerWordFortitudeSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Shadow Priest spec.";

        public override string Displayname => "Priest Shadow";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator
        {
            get =>
                new SimpleItemComparator((CharacterManager)WowInterface.CharacterManager, new Dictionary<string, double>()
                {
                    { WowStatType.INTELLECT, 2.5 },
                    { WowStatType.SPELL_POWER, 2.5 },
                    { WowStatType.ARMOR, 2.0 },
                    { WowStatType.MP5, 2.0 },
                    { WowStatType.HASTE, 2.0 },
                });
            set { }
        }

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 2, new(1, 2, 5) },
                { 4, new(1, 4, 3) },
                { 5, new(1, 5, 2) },
                { 7, new(1, 7, 3) },
            },
            Tree2 = new(),
            Tree3 = new()
            {
                { 1, new(3, 1, 3) },
                { 2, new(3, 2, 2) },
                { 3, new(3, 3, 5) },
                { 5, new(3, 5, 2) },
                { 6, new(3, 6, 3) },
                { 8, new(3, 8, 5) },
                { 9, new(3, 9, 1) },
                { 10, new(3, 10, 2) },
                { 11, new(3, 11, 2) },
                { 12, new(3, 12, 3) },
                { 14, new(3, 14, 1) },
                { 16, new(3, 16, 3) },
                { 17, new(3, 17, 2) },
                { 18, new(3, 18, 3) },
                { 19, new(3, 19, 1) },
                { 20, new(3, 20, 5) },
                { 21, new(3, 21, 2) },
                { 22, new(3, 22, 3) },
                { 24, new(3, 24, 1) },
                { 25, new(3, 25, 3) },
                { 26, new(3, 26, 5) },
                { 27, new(3, 27, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.2";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Priest;

        public override void Execute()
        {
            base.Execute();

            if (WowInterface.Target == null)
            {
                return;
            }

            if (WowInterface.Player.ManaPercentage < 90
                && TryCastSpell(shadowfiendSpell, WowInterface.TargetGuid))
            {
                return;
            }

            if (WowInterface.Player.ManaPercentage < 30
                && TryCastSpell(hymnOfHopeSpell, 0))
            {
                return;
            }

            if (WowInterface.Player.HealthPercentage < 70
                && TryCastSpell(flashHealSpell, WowInterface.TargetGuid, true))
            {
                return;
            }

            if (WowInterface.Player.ManaPercentage >= 50
                && TryCastSpell(berserkingSpell, WowInterface.TargetGuid))
            {
                return;
            }

            if (!WowInterface.Player.IsCasting
                && TryCastSpell(mindFlaySpell, WowInterface.TargetGuid, true))
            {
                return;
            }

            if (TryCastSpell(smiteSpell, WowInterface.TargetGuid, true))
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