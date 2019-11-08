using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class PaladinHoly : ICombatClass
    {
        private readonly string blessingOfWisdom = "Blessing of Wisdom";
        private readonly int buffCheckTime = 30;
        private readonly string devotionAuraSpell = "Devotion Aura";
        private readonly string divineFavorSpell = "Divine Favor";
        private readonly string divineIlluminationSpell = "Divine Illumination";
        private readonly string divinePleaSpell = "Divine Plea";
        private readonly string flashOfLight = "Flash of Light";
        private readonly string holyLightSpell = "Holy Light";
        private readonly string holyShockSpell = "Holy Shock";
        private readonly string layOnHands = "Lay on Hands";

        public PaladinHoly(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, flashOfLight },
                { 2000, holyShockSpell },
                { 10000, holyLightSpell }
            };
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => true;

        public bool IsMelee => false;

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private Dictionary<int, string> SpellUsageHealDict { get; }

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

                if (target != null)
                {
                    ObjectManager.UpdateObject(target.Type, target.BaseAddress);

                    if (target.HealthPercentage < 12
                        && IsSpellKnown(layOnHands)
                        && !IsOnCooldown(layOnHands))
                    {
                        HookManager.CastSpell(layOnHands);
                        return;
                    }

                    if (target.HealthPercentage < 50
                       && IsSpellKnown(divineFavorSpell)
                       && !IsOnCooldown(divineFavorSpell))
                    {
                        HookManager.CastSpell(divineFavorSpell);
                    }

                    if (ObjectManager.Player.ManaPercentage < 50
                       && ObjectManager.Player.ManaPercentage > 20
                       && IsSpellKnown(divineIlluminationSpell)
                       && !IsOnCooldown(divineIlluminationSpell))
                    {
                        HookManager.CastSpell(divineIlluminationSpell);
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
        }

        private void HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);
            HookManager.TargetGuid(ObjectManager.PlayerGuid);

            if (IsSpellKnown(devotionAuraSpell)
                && !myBuffs.Any(e => e.Equals(devotionAuraSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(devotionAuraSpell))
            {
                HookManager.CastSpell(devotionAuraSpell);
                return;
            }

            if (IsSpellKnown(blessingOfWisdom)
                && !myBuffs.Any(e => e.Equals(blessingOfWisdom, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(blessingOfWisdom))
            {
                HookManager.CastSpell(blessingOfWisdom);
                return;
            }

            LastBuffCheck = DateTime.Now;
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

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells.OrderByDescending(e => e.Rank).FirstOrDefault(e => e.Name.Equals(spellName))?.Costs <= ObjectManager.Player.Mana;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells.Any(e => e.Name.Equals(spellName));

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
