using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class DruidRestoration : ICombatClass
    {
        // author: Jannis Höschele

        private readonly string rejuvenationSpell = "Rejuvenation";
        private readonly string lifebloomSpell = "Lifebloom";
        private readonly string wildGrowthSpell = "Wild Growth";
        private readonly string regrowthSpell = "Regrowth";
        private readonly string healingTouchSpell = "Healing Touch";
        private readonly string nourishSpell = "Nourish";
        private readonly string treeOfLifeSpell = "Tree of Life";
        private readonly string markofTheWildSpell = "Mark of the Wild";
        private readonly string reviveSpell = "Revive";
        private readonly string tranquilitySpell = "Tranquility";
        private readonly string innervateSpell = "Innervate";
        private readonly string naturesSwiftnessSpell = "Nature's Swiftness";
        private readonly string swiftmendSpell = "Swiftmend";

        private readonly int buffCheckTime = 8;
        private readonly int deadPartymembersCheckTime = 4;

        public DruidRestoration(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, nourishSpell },
                { 3000, regrowthSpell },
                { 5000, healingTouchSpell },
            };
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => false;

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDeadPartymembersCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public void Execute()
        {
            if (NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing))
            {
                HandleTargetSelection(playersThatNeedHealing);
                ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);

                if (IsSpellKnown(tranquilitySpell) 
                    && playersThatNeedHealing.Count > 4
                    && !IsOnCooldown(tranquilitySpell))
                {
                    HookManager.CastSpell(tranquilitySpell);
                    return;
                }

                WowUnit target = (WowUnit)ObjectManager.WowObjects.FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

                if (target != null)
                {
                    ObjectManager.UpdateObject(target.Type, target.BaseAddress);
                    List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Target);

                    if (target.HealthPercentage < 15
                        && IsSpellKnown(naturesSwiftnessSpell)
                        && !IsOnCooldown(naturesSwiftnessSpell))
                    {
                        HookManager.CastSpell(naturesSwiftnessSpell);
                        HookManager.CastSpell(regrowthSpell);
                        return;
                    }

                    if (target.HealthPercentage < 90
                       && IsSpellKnown(rejuvenationSpell)
                       && !myBuffs.Any(e => e.Equals(rejuvenationSpell, StringComparison.OrdinalIgnoreCase))
                       && !IsOnCooldown(rejuvenationSpell))
                    {
                        HookManager.CastSpell(rejuvenationSpell);
                        return;
                    }

                    if (target.HealthPercentage < 85
                       && IsSpellKnown(wildGrowthSpell)
                       && !myBuffs.Any(e => e.Equals(wildGrowthSpell, StringComparison.OrdinalIgnoreCase))
                       && !IsOnCooldown(wildGrowthSpell))
                    {
                        HookManager.CastSpell(wildGrowthSpell);
                        return;
                    }

                    if (target.HealthPercentage < 85
                       && IsSpellKnown(lifebloomSpell)
                       && !myBuffs.Any(e => e.Equals(lifebloomSpell, StringComparison.OrdinalIgnoreCase))
                       && !IsOnCooldown(lifebloomSpell))
                    {
                        HookManager.CastSpell(lifebloomSpell);
                        return;
                    }

                    if (target.HealthPercentage < 70
                       && IsSpellKnown(swiftmendSpell)
                       && myBuffs.Any(e => e.Equals(regrowthSpell, StringComparison.OrdinalIgnoreCase))
                       || myBuffs.Any(e => e.Equals(rejuvenationSpell, StringComparison.OrdinalIgnoreCase))
                       && !IsOnCooldown(swiftmendSpell))
                    {
                        HookManager.CastSpell(swiftmendSpell);
                        return;
                    }

                    if (ObjectManager.Player.ManaPercentage < 30
                       && IsSpellKnown(innervateSpell)
                       && !IsOnCooldown(innervateSpell))
                    {
                        HookManager.CastSpell(innervateSpell);
                        return;
                    }

                    double healthDifference = target.MaxHealth - target.Health;
                    List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                    foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                    {
                        if (IsSpellKnown(keyValuePair.Value)
                            && HasEnoughMana(keyValuePair.Value)
                            && !IsOnCooldown(keyValuePair.Value))
                        {
                            HookManager.CastSpell(keyValuePair.Value);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
                {
                    HandleBuffing();
                }
            }
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();
            }

            if (DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime))
            {
                HandleDeadPartymembers();
            }
        }

        private void HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);
            HookManager.TargetGuid(ObjectManager.PlayerGuid);

            if (IsSpellKnown(markofTheWildSpell)
                && !myBuffs.Any(e => e.Equals(markofTheWildSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(markofTheWildSpell))
            {
                HookManager.CastSpell(markofTheWildSpell);
                return;
            }

            if (IsSpellKnown(treeOfLifeSpell)
                && !myBuffs.Any(e => e.Equals(treeOfLifeSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(treeOfLifeSpell))
            {
                HookManager.CastSpell(treeOfLifeSpell);
                return;
            }

            LastBuffCheck = DateTime.Now;
        }

        private void HandleDeadPartymembers()
        {
            if (IsSpellKnown(reviveSpell)
                && HasEnoughMana(reviveSpell)
                && !IsOnCooldown(reviveSpell))
            {
                IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
                List<WowPlayer> groupPlayers = players.Where(e => e.IsDead && e.Health == 0 && ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

                if (groupPlayers.Count > 0)
                {
                    HookManager.TargetGuid(groupPlayers.First().Guid);
                    HookManager.CastSpell(reviveSpell);
                }
            }
        }

        private void HandleTargetSelection(List<WowPlayer> possibleTargets)
        {
            // select the one with lowest hp
            WowUnit target = possibleTargets.OrderBy(e => e.HealthPercentage).First();

            if (target != null)
            {
                HookManager.TargetGuid(target.Guid);
            }
        }

        private bool CastSpellIfPossible(string spellname, bool needsMana = false)
        {
            if (IsSpellKnown(spellname)
                && (needsMana && HasEnoughMana(spellname))
                && !IsOnCooldown(spellname))
            {
                HookManager.CastSpell(spellname);
                return true;
            }

            return false;
        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells
            .OrderByDescending(e => e.Rank)
            .FirstOrDefault(e => e.Name.Equals(spellName))
            ?.Costs <= ObjectManager.Player.Mana;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells
            .Any(e => e.Name.Equals(spellName));

        private bool NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing)
        {
            IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
            List<WowPlayer> groupPlayers = players.Where(e => !e.IsDead && e.Health > 1 && ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

            groupPlayers.Add(ObjectManager.Player);

            playersThatNeedHealing = groupPlayers.Where(e => e.HealthPercentage < 90).ToList();

            return playersThatNeedHealing.Count > 0;
        }
    }
}
