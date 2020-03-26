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

        private readonly int deadPartymembersCheckTime = 4;
        private readonly string healingTouchSpell = "Healing Touch";
        private readonly string innervateSpell = "Innervate";
        private readonly string lifebloomSpell = "Lifebloom";
        private readonly string markOfTheWildSpell = "Mark of the Wild";
        private readonly string naturesSwiftnessSpell = "Nature's Swiftness";
        private readonly string nourishSpell = "Nourish";
        private readonly string regrowthSpell = "Regrowth";
        private readonly string rejuvenationSpell = "Rejuvenation";
        private readonly string reviveSpell = "Revive";
        private readonly string swiftmendSpell = "Swiftmend";
        private readonly string tranquilitySpell = "Tranquility";
        private readonly string treeOfLifeSpell = "Tree of Life";
        private readonly string wildGrowthSpell = "Wild Growth";

        public DruidRestoration(WowInterface wowInterface) : base(wowInterface)
        {
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

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.ManaPercentage < 30
                && CastSpellIfPossible(innervateSpell, WowInterface.ObjectManager.PlayerGuid, true))
            {
                return;
            }

            if (NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing))
            {
                HandleTargetSelection(playersThatNeedHealing);
                WowInterface.ObjectManager.UpdateObject(WowInterface.ObjectManager.Player);

                if (playersThatNeedHealing.Count > 4
                    && CastSpellIfPossible(tranquilitySpell, 0, true))
                {
                    return;
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
                        return;
                    }

                    double healthDifference = target.MaxHealth - target.Health;
                    List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                    foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                    {
                        if (CastSpellIfPossible(keyValuePair.Value, WowInterface.ObjectManager.TargetGuid, true))
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                if (MyAuraManager.Tick())
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || (DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime)
                    && HandleDeadPartymembers(reviveSpell)))
            {
                return;
            }
        }

        private void HandleTargetSelection(List<WowPlayer> possibleTargets)
        {
            // select the one with lowest hp
            WowUnit target = possibleTargets.Where(e => !e.IsDead && e.Health > 1).OrderBy(e => e.HealthPercentage).First();

            if (target != null)
            {
                WowInterface.HookManager.TargetGuid(target.Guid);
            }
        }

        private bool NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing)
        {
            IEnumerable<WowPlayer> players = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>();
            List<WowPlayer> groupPlayers = players.Where(e => !e.IsDead && e.Health > 1 && WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

            groupPlayers.Add(WowInterface.ObjectManager.Player);
            playersThatNeedHealing = groupPlayers.Where(e => e.HealthPercentage < 90).ToList();

            return playersThatNeedHealing.Count > 0;
        }
    }
}