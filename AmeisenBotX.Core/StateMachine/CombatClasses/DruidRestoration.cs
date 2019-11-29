using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.CombatClasses.Utils;
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
            CooldownManager = new CooldownManager(characterManager.SpellBook.Spells);

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, nourishSpell },
                { 3000, regrowthSpell },
                { 5000, healingTouchSpell },
            };

            Spells = new Dictionary<string, Spell>();
            CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                Spells.Clear();
                foreach (Spell spell in CharacterManager.SpellBook.Spells)
                {
                    Spells.Add(spell.Name, spell);
                }
            };
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => false;

        public IWowItemComparator ItemComparator => null;

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDeadPartymembersCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private CooldownManager CooldownManager { get; }

        private Dictionary<string, Spell> Spells { get; }

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (ObjectManager.Player.ManaPercentage < 30
                && CastSpellIfPossible(innervateSpell, true))
            {
                return;
            }

            if (NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing))
            {
                HandleTargetSelection(playersThatNeedHealing);
                ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);

                if (playersThatNeedHealing.Count > 4
                    && CastSpellIfPossible(tranquilitySpell, true))
                {
                    return;
                }

                WowUnit target = ObjectManager.Target;
                if (target != null)
                {
                    ObjectManager.UpdateObject(target.Type, target.BaseAddress);
                    List<string> targetBuffs = HookManager.GetBuffs(WowLuaUnit.Target);

                    if ((target.HealthPercentage < 15
                            && CastSpellIfPossible(naturesSwiftnessSpell, true))
                        || (target.HealthPercentage < 90
                            && !targetBuffs.Any(e => e.Equals(rejuvenationSpell, StringComparison.OrdinalIgnoreCase))
                            && CastSpellIfPossible(rejuvenationSpell, true))
                        || (target.HealthPercentage < 85
                            && !targetBuffs.Any(e => e.Equals(wildGrowthSpell, StringComparison.OrdinalIgnoreCase))
                            && CastSpellIfPossible(wildGrowthSpell, true))
                        || (target.HealthPercentage < 85
                            && !targetBuffs.Any(e => e.Equals(lifebloomSpell, StringComparison.OrdinalIgnoreCase))
                            && CastSpellIfPossible(reviveSpell, true))
                        || (target.HealthPercentage < 70
                            && (targetBuffs.Any(e => e.Equals(regrowthSpell, StringComparison.OrdinalIgnoreCase))
                                || targetBuffs.Any(e => e.Equals(rejuvenationSpell, StringComparison.OrdinalIgnoreCase))
                            && CastSpellIfPossible(swiftmendSpell, true))))
                    {
                        return;
                    }

                    double healthDifference = target.MaxHealth - target.Health;
                    List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                    foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                    {
                        if (CastSpellIfPossible(keyValuePair.Value, true))
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                    && HandleBuffing())
                {
                    return;
                }
            }
        }

        public void OutOfCombatExecute()
        {
            if ((DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                    && HandleBuffing())
                || (DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime)
                    && HandleDeadPartymembers()))
            {
                return;
            }
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);
            HookManager.TargetGuid(ObjectManager.PlayerGuid);

            if ((!myBuffs.Any(e => e.Equals(markofTheWildSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(markofTheWildSpell, true))
                || (!myBuffs.Any(e => e.Equals(treeOfLifeSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(treeOfLifeSpell, true)))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleDeadPartymembers()
        {
            if (!Spells.ContainsKey(reviveSpell))
            {
                Spells.Add(reviveSpell, CharacterManager.SpellBook.GetSpellByName(reviveSpell));
            }

            if (Spells[reviveSpell] != null
                && !CooldownManager.IsSpellOnCooldown(reviveSpell)
                && Spells[reviveSpell].Costs < ObjectManager.Player.Mana)
            {
                IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
                List<WowPlayer> groupPlayers = players.Where(e => e.IsDead && e.Health == 0 && ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

                if (groupPlayers.Count > 0)
                {
                    HookManager.TargetGuid(groupPlayers.First().Guid);
                    HookManager.CastSpell(reviveSpell);
                    CooldownManager.SetSpellCooldown(reviveSpell, (int)HookManager.GetSpellCooldown(reviveSpell));
                    return true;
                }
            }

            return false;
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

        private bool CastSpellIfPossible(string spellName, bool needsMana = false)
        {
            if (!Spells.ContainsKey(spellName))
            {
                Spells.Add(spellName, CharacterManager.SpellBook.GetSpellByName(spellName));
            }

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsMana || Spells[spellName].Costs < ObjectManager.Player.Mana))
            {
                HookManager.CastSpell(spellName);
                CooldownManager.SetSpellCooldown(spellName, (int)HookManager.GetSpellCooldown(spellName));
                return true;
            }

            return false;
        }

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
