using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class DruidRestoration : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const int deadPartymembersCheckTime = 4;
        private const string healingTouchSpell = "Healing Touch";
        private const string innervateSpell = "Innervate";
        private const string lifebloomSpell = "Lifebloom";
        private const string markOfTheWildSpell = "Mark of the Wild";
        private const string naturesSwiftnessSpell = "Nature's Swiftness";
        private const string nourishSpell = "Nourish";
        private const string regrowthSpell = "Regrowth";
        private const string rejuvenationSpell = "Rejuvenation";
        private const string reviveSpell = "Revive";
        private const string swiftmendSpell = "Swiftmend";
        private const string tranquilitySpell = "Tranquility";
        private const string treeOfLifeSpell = "Tree of Life";
        private const string wildGrowthSpell = "Wild Growth";
#pragma warning restore IDE0051

        public DruidRestoration(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            UseDefaultTargetSelection = false;

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { treeOfLifeSpell, () => CastSpellIfPossible(treeOfLifeSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { markOfTheWildSpell, () => CastSpellIfPossible(markOfTheWildSpell, WowInterface.ObjectManager.PlayerGuid, true) }
            };

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, nourishSpell },
                { 3000, regrowthSpell },
                { 5000, healingTouchSpell },
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Druid;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Unholy Deathknight spec.";

        public override string Displayname => "Druid Restoration";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => true;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new List<ArmorType>() { ArmorType.SHIEDLS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override string Version => "1.0";

        private DateTime LastDeadPartymembersCheck { get; set; }

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override void ExecuteCC()
        {
            if (WowInterface.ObjectManager.Player.ManaPercentage < 30
                && CastSpellIfPossible(innervateSpell, WowInterface.ObjectManager.PlayerGuid, true))
            {
                return;
            }

            if (!NeedToHealSomeone())
            {
                if (MyAuraManager.Tick())
                {
                    return;
                }
            }
        }

        private bool NeedToHealSomeone()
        {
            if (TargetManager.GetUnitToTarget(out List<WowUnit> unitsToHeal))
            {
                WowInterface.HookManager.TargetGuid(unitsToHeal.First().Guid);
                WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Player);

                if (unitsToHeal.Count > 3
                    && CastSpellIfPossible(tranquilitySpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return true;
                }

                WowUnit target = WowInterface.ObjectManager.Target;
                if (target != null)
                {
                    WowInterface.ObjectManager.UpdateObject(target);
                    if ((target.HealthPercentage < 15
                            && CastSpellIfPossible(naturesSwiftnessSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || (target.HealthPercentage < 90
                            && !WowInterface.ObjectManager.Target.HasBuffByName(rejuvenationSpell)
                            && CastSpellIfPossible(rejuvenationSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || (target.HealthPercentage < 85
                            && !WowInterface.ObjectManager.Target.HasBuffByName(wildGrowthSpell)
                            && CastSpellIfPossible(wildGrowthSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || (target.HealthPercentage < 70
                            && (WowInterface.ObjectManager.Target.HasBuffByName(regrowthSpell)
                                || WowInterface.ObjectManager.Target.HasBuffByName(rejuvenationSpell)
                            && CastSpellIfPossible(swiftmendSpell, WowInterface.ObjectManager.TargetGuid, true))))
                    {
                        return true;
                    }

                    double healthDifference = target.MaxHealth - target.Health;
                    List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                    foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                    {
                        if (CastSpellIfPossible(keyValuePair.Value, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || NeedToHealSomeone()
                || (DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime)
                    && HandleDeadPartymembers(reviveSpell)))
            {
                return;
            }
        }
    }
}