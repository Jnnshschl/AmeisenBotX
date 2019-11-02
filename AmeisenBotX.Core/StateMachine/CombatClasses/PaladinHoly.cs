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
    public class PaladinHoly : ICombatClass
    {
        private readonly string devotionAuraSpell = "Devotion Aura";
        private readonly string divinePleaSpell = "Divine Plea";
        private readonly string holyShockSpell = "Holy Shock";
        private readonly string holyLightSpell = "Holy Light";
        private readonly string flashOfLight = "Flash of Light";
        private readonly string layOnHands = "Lay on Hands";

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public PaladinHoly(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, flashOfLight},
                { 2000, holyShockSpell},
                { 6000, holyLightSpell}
            };
        }

        public bool HandlesMovement => true;

        public bool HandlesTargetSelection => true;

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        public void Execute()
        {
            if (IsSpellKnown(divinePleaSpell)
                && ObjectManager.Player.ManaPercentage < 80
                && !IsOnCooldown(divinePleaSpell))
            {
                HookManager.CastSpell(divinePleaSpell);
                return;
            }

            if (NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing))
            {
                HandleTargetSelection(playersThatNeedHealing);
                ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);

                WowUnit target = (WowUnit)ObjectManager.WowObjects.FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

                if (target == null)
                {
                    return;
                }

                ObjectManager.UpdateObject(target.Type, target.BaseAddress);

                if (target.HealthPercentage < 12
                    && IsSpellKnown(layOnHands)
                    && !IsOnCooldown(layOnHands))
                {
                    HookManager.CastSpell(layOnHands);
                    return;
                }

                double healthDifference = target.MaxHealth - target.Health;
                List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                foreach (KeyValuePair<int, string> keyValuePair in spellsToTry)
                {
                    if (IsSpellKnown(keyValuePair.Value)
                        && HasEnoughMana(keyValuePair.Value)
                        && !IsOnCooldown(keyValuePair.Value))
                    {
                        HookManager.CastSpell(holyLightSpell);
                        break;
                    }
                }
            }
            else
            {
                List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player.ToString());

                if (IsSpellKnown(devotionAuraSpell)
                    && !myBuffs.Any(e => e.Equals(devotionAuraSpell))
                    && !IsOnCooldown(devotionAuraSpell))
                {
                    HookManager.CastSpell(devotionAuraSpell);
                    return;
                }
            }
        }

        private bool NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing)
        {
            IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
            List<WowPlayer> groupPlayers = players.Where(e => !e.IsDead && e.Health > 1 && ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

            groupPlayers.Add(ObjectManager.Player);

            playersThatNeedHealing = groupPlayers.Where(e => e.HealthPercentage < 90).ToList();

            return playersThatNeedHealing.Count > 0;
        }

        private void HandleTargetSelection(List<WowPlayer> possibleTargets)
        {
            // select the one with lowest hp
            HookManager.TargetGuid(possibleTargets.OrderBy(e => e.HealthPercentage).First().Guid);
        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells.FirstOrDefault(e => e.Name.Equals(spellName))?.Costs <= ObjectManager.Player.Mana;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells.Any(e => e.Name.Equals(spellName));

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;
    }
}
