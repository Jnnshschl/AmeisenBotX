using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class PaladinRetribution : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string avengingWrathSpell = "Avenging Wrath";
        private const string blessingOfMightSpell = "Blessing of Might";
        private const string consecrationSpell = "Consecration";
        private const string crusaderStrikeSpell = "Crusader Strike";
        private const string divinePleaSpell = "Divine Plea";
        private const string divineStormSpell = "Divine Storm";
        private const string exorcismSpell = "Exorcism";
        private const string hammerOfJusticeSpell = "Hammer of Justice";
        private const string hammerOfWrathSpell = "Hammer of Wrath";
        private const string holyLightSpell = "Holy Light";
        private const string holyWrathSpell = "Holy Wrath";
        private const string judgementOfLightSpell = "Judgement of Light";
        private const string layOnHandsSpell = "Lay on Hands";
        private const string retributionAuraSpell = "Retribution Aura";
        private const string sealOfVengeanceSpell = "Seal of Vengeance";
#pragma warning restore IDE0051

        public PaladinRetribution(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { blessingOfMightSpell, () => CastSpellIfPossible(blessingOfMightSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { retributionAuraSpell, () => CastSpellIfPossible(retributionAuraSpell, 0, true) },
                { sealOfVengeanceSpell, () => CastSpellIfPossible(sealOfVengeanceSpell, 0, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossible(hammerOfJusticeSpell, x.Guid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((blessingOfMightSpell, (spellName, guid) => CastSpellIfPossible(spellName, guid, true)));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Paladin;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Retribution Paladin spec.";

        public override string Displayname => "Paladin Retribution";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override bool WalkBehindEnemy => false;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(1, 2, 5) },
                { 4, new Talent(1, 4, 5) },
                { 6, new Talent(1, 6, 1) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(2, 2, 5) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(3, 2, 5) },
                { 3, new Talent(3, 3, 2) },
                { 4, new Talent(3, 4, 3) },
                { 5, new Talent(3, 5, 2) },
                { 7, new Talent(3, 7, 5) },
                { 8, new Talent(3, 8, 1) },
                { 9, new Talent(3, 9, 2) },
                { 11, new Talent(3, 11, 3) },
                { 12, new Talent(3, 12, 3) },
                { 13, new Talent(3, 13, 3) },
                { 14, new Talent(3, 14, 1) },
                { 15, new Talent(3, 15, 3) },
                { 17, new Talent(3, 17, 2) },
                { 18, new Talent(3, 18, 1) },
                { 19, new Talent(3, 19, 3) },
                { 20, new Talent(3, 20, 3) },
                { 21, new Talent(3, 21, 2) },
                { 22, new Talent(3, 22, 3) },
                { 23, new Talent(3, 23, 1) },
                { 24, new Talent(3, 24, 3) },
                { 25, new Talent(3, 25, 3) },
                { 26, new Talent(3, 26, 1) },
            },
        };

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_AXES, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_SWORDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        public override bool UseAutoAttacks => true;

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run() && WowInterface.ObjectManager.Player.IsInMeleeRange(WowInterface.ObjectManager.Target))
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if ((WowInterface.ObjectManager.Player.HealthPercentage < 20
                    && CastSpellIfPossible(layOnHandsSpell, WowInterface.ObjectManager.PlayerGuid))
                || (WowInterface.ObjectManager.Player.HealthPercentage < 60
                    && CastSpellIfPossible(holyLightSpell, WowInterface.ObjectManager.PlayerGuid, true)))
            {
                return;
            }

            if ((WowInterface.ObjectManager.Player.HasBuffByName(sealOfVengeanceSpell)
                    && CastSpellIfPossible(judgementOfLightSpell, 0))
                || CastSpellIfPossible(avengingWrathSpell, 0, true)
                || (WowInterface.ObjectManager.Player.ManaPercentage < 80
                    && CastSpellIfPossible(divinePleaSpell, 0, true)))
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                if ((WowInterface.ObjectManager.Player.HealthPercentage < 20
                        && CastSpellIfPossible(hammerOfWrathSpell, WowInterface.ObjectManager.TargetGuid, true))
                    || CastSpellIfPossible(crusaderStrikeSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || CastSpellIfPossible(divineStormSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || CastSpellIfPossible(consecrationSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || CastSpellIfPossible(exorcismSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || CastSpellIfPossible(holyWrathSpell, WowInterface.ObjectManager.TargetGuid, true))
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