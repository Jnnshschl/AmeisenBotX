using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
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

            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(4000));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Paladin;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Retribution Paladin spec.";

        public override string Displayname => "Paladin Retribution";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIEDLS }, new List<WeaponType>() { WeaponType.ONEHANDED_AXES, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_SWORDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private TimegatedEvent AutoAttackEvent { get; set; }

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run())
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

            if (MyAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || (MyAuraManager.Buffs.Contains(sealOfVengeanceSpell.ToLower())
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
            if (MyAuraManager.Tick())
            {
                return;
            }
        }
    }
}