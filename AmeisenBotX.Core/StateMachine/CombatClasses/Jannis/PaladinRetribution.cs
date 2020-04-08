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
                { blessingOfMightSpell, () => CastSpellIfPossible(blessingOfMightSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { retributionAuraSpell, () => CastSpellIfPossible(retributionAuraSpell, 0, true) },
                { sealOfVengeanceSpell, () => CastSpellIfPossible(sealOfVengeanceSpell, 0, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(hammerOfJusticeSpell, WowInterface.ObjectManager.TargetGuid, true) }
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

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIEDLS }, new List<WeaponType>() { WeaponType.ONEHANDED_AXES, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_SWORDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private DateTime LastAutoAttackCheck { get; set; }

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

            if (DateTime.Now - LastAutoAttackCheck > TimeSpan.FromSeconds(4) && !WowInterface.ObjectManager.Player.IsAutoAttacking)
            {
                LastAutoAttackCheck = DateTime.Now;
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