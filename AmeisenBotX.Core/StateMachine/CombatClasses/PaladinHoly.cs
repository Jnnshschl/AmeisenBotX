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
        private readonly string holyLightSpell = "Holy Light";
        private readonly string flashOfLight = "Flash of Light";

        public PaladinHoly(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
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

                double healthDifference = target.MaxHealth - target.Health;

                if (healthDifference > 2000)
                {
                    if (IsSpellKnown(holyLightSpell)
                        && HasEnoughMana(holyLightSpell)
                        && !IsOnCooldown(holyLightSpell))
                    {
                        HookManager.CastSpell(holyLightSpell);
                        return;
                    }
                }
                else
                {
                    if (IsSpellKnown(flashOfLight)
                        && HasEnoughMana(flashOfLight)
                        && !IsOnCooldown(flashOfLight))
                    {
                        HookManager.CastSpell(flashOfLight);
                        return;
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
