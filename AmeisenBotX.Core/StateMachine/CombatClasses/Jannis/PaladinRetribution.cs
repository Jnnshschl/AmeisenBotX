using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.StateMachine.Enums;
using System.Collections.Generic;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;
using static AmeisenBotX.Core.StateMachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class PaladinRetribution : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string avengingWrathSpell = "Avenging Wrath";
        private readonly string blessingOfMightSpell = "Blessing of Might";
        private readonly string consecrationSpell = "Consecration";
        private readonly string crusaderStrikeSpell = "Crusader Strike";
        private readonly string divinePleaSpell = "Divine Plea";
        private readonly string divineStormSpell = "Divine Storm";
        private readonly string exorcismSpell = "Exorcism";
        private readonly string hammerOfJusticeSpell = "Hammer of Justice";
        private readonly string hammerOfWrathSpell = "Hammer of Wrath";
        private readonly string holyLightSpell = "Holy Light";
        private readonly string holyWrathSpell = "Holy Wrath";
        private readonly string judgementOfLightSpell = "Judgement of Light";
        private readonly string layOnHandsSpell = "Lay on Hands";
        private readonly string retributionAuraSpell = "Retribution Aura";
        private readonly string sealOfVengeanceSpell = "Seal of Vengeance";

        public PaladinRetribution(WowInterface wowInterface) : base(wowInterface)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { blessingOfMightSpell, () =>
                    {
                        HookManager.TargetGuid(WowInterface.ObjectManager.PlayerGuid);
                        return CastSpellIfPossible(blessingOfMightSpell, true);
                    }
                },
                { retributionAuraSpell, () => CastSpellIfPossible(retributionAuraSpell, true) },
                { sealOfVengeanceSpell, () => CastSpellIfPossible(sealOfVengeanceSpell, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(hammerOfJusticeSpell, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Paladin;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Retribution Paladin spec.";

        public override string Displayname => "Paladin Retribution";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator();

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (!WowInterface.ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
            }

            if (MyAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || (MyAuraManager.Buffs.Contains(sealOfVengeanceSpell.ToLower())
                    && CastSpellIfPossible(judgementOfLightSpell))
                || (WowInterface.ObjectManager.Player.HealthPercentage < 20
                    && CastSpellIfPossible(layOnHandsSpell))
                || (WowInterface.ObjectManager.Player.HealthPercentage < 60
                    && CastSpellIfPossible(holyLightSpell, true))
                || CastSpellIfPossible(avengingWrathSpell, true)
                || (WowInterface.ObjectManager.Player.ManaPercentage < 80
                    && CastSpellIfPossible(divinePleaSpell, true)))
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                if ((WowInterface.ObjectManager.Player.HealthPercentage < 20
                        && CastSpellIfPossible(hammerOfWrathSpell, true))
                    || CastSpellIfPossible(crusaderStrikeSpell, true)
                    || CastSpellIfPossible(divineStormSpell, true)
                    || CastSpellIfPossible(divineStormSpell, true)
                    || CastSpellIfPossible(consecrationSpell, true)
                    || CastSpellIfPossible(exorcismSpell, true)
                    || CastSpellIfPossible(holyWrathSpell, true))
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