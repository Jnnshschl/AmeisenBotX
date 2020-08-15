using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.Utils;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class PaladinHoly : BasicCombatClass
    {
        public PaladinHoly(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { blessingOfWisdomSpell, () => CastSpellIfPossible(blessingOfWisdomSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { devotionAuraSpell, () => CastSpellIfPossible(devotionAuraSpell, WowInterface.ObjectManager.PlayerGuid, true) }
            };

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, flashOfLightSpell },
                { 300, holyLightSpell },
                { 2000, holyShockSpell },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((blessingOfWisdomSpell, (spellName, guid) => CastSpellIfPossible(spellName, guid, true)));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Paladin;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Holy Paladin spec.";

        public override string Displayname => "Paladin Holy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicComparator
        (
            null,
            new List<WeaponType>() { WeaponType.TWOHANDED_AXES, WeaponType.TWOHANDED_MACES, WeaponType.TWOHANDED_SWORDS },
            new Dictionary<string, double>()
            {
                { "ITEM_MOD_CRIT_RATING_SHORT", 0.88 },
                { "ITEM_MOD_INTELLECT_SHORT", 0.2 },
                { "ITEM_MOD_SPELL_POWER_SHORT", 0.68 },
                { "ITEM_MOD_HASTE_RATING_SHORT", 0.71},
            }
        );

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 5) },
                { 3, new Talent(1, 3, 3) },
                { 4, new Talent(1, 4, 5) },
                { 6, new Talent(1, 6, 1) },
                { 7, new Talent(1, 7, 5) },
                { 8, new Talent(1, 8, 1) },
                { 10, new Talent(1, 10, 2) },
                { 13, new Talent(1, 13, 1) },
                { 14, new Talent(1, 14, 3) },
                { 16, new Talent(1, 16, 5) },
                { 17, new Talent(1, 17, 3) },
                { 18, new Talent(1, 18, 1) },
                { 21, new Talent(1, 21, 5) },
                { 22, new Talent(1, 22, 1) },
                { 23, new Talent(1, 23, 5) },
                { 24, new Talent(1, 24, 2) },
                { 25, new Talent(1, 25, 2) },
                { 26, new Talent(1, 26, 1) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 5) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(3, 2, 5) },
                { 4, new Talent(3, 4, 3) },
                { 5, new Talent(3, 5, 2) },
                { 7, new Talent(3, 7, 5) },
            },
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override void ExecuteCC()
        {
            if ((WowInterface.ObjectManager.PartymemberGuids.Any() || WowInterface.ObjectManager.Player.HealthPercentage < 75.0)
                && NeedToHealSomeone())
            {
                return;
            }

            if ((!WowInterface.ObjectManager.PartymemberGuids.Any() || WowInterface.ObjectManager.Player.ManaPercentage > 50) && SelectTarget(DpsTargetManager))
            {
                if (WowInterface.ObjectManager.Player.IsAutoAttacking
                    && WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 3.0)
                {
                    WowInterface.HookManager.StartAutoAttack();
                    return;
                }

                if (CastSpellIfPossible(judgementOfLightSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }

                if (CastSpellIfPossible(exorcismSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || GroupAuraManager.Tick()
                || NeedToHealSomeone())
            {
                return;
            }
        }

        private bool NeedToHealSomeone()
        {
            if (HealTargetManager.GetUnitToTarget(out IEnumerable<WowUnit> unitsToHeal))
            {
                WowUnit targetUnit = unitsToHeal.Any() ? unitsToHeal.First(e => !e.HasBuffByName(beaconOfLightSpell)) : unitsToHeal.First();

                if (targetUnit.HealthPercentage < 12.0
                    && CastSpellIfPossible(layOnHandsSpell, 0))
                {
                    return true;
                }

                if (unitsToHeal.Count(e => !e.HasBuffByName(beaconOfLightSpell)) > 1
                    && CastSpellIfPossible(beaconOfLightSpell, targetUnit.Guid, true))
                {
                    return true;
                }

                // TODO: bugged need to figure out why cooldown is always wrong
                // if (targetUnit.HealthPercentage < 50
                //     && CastSpellIfPossible(divineFavorSpell, targetUnit.Guid, true))
                // {
                //     LastHealAction = DateTime.Now;
                //     return true;
                // }

                if (WowInterface.ObjectManager.Player.ManaPercentage < 50
                   && WowInterface.ObjectManager.Player.ManaPercentage > 20
                   && CastSpellIfPossible(divineIlluminationSpell, 0, true))
                {
                    return true;
                }

                if (WowInterface.ObjectManager.Player.ManaPercentage < 60
                    && CastSpellIfPossible(divinePleaSpell, 0, true))
                {
                    return true;
                }

                double healthDifference = targetUnit.MaxHealth - targetUnit.Health;
                List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                {
                    if (CastSpellIfPossible(keyValuePair.Value, targetUnit.Guid, true))
                    {
                        break;
                    }
                }

                return true;
            }

            return false;
        }
    }
}