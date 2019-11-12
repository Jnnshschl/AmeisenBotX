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
        // author: Jannis Höschele

        private readonly string blessingOfWisdomSpell = "Blessing of Wisdom";
        private readonly string devotionAuraSpell = "Devotion Aura";
        private readonly string divineFavorSpell = "Divine Favor";
        private readonly string divineIlluminationSpell = "Divine Illumination";
        private readonly string divinePleaSpell = "Divine Plea";
        private readonly string flashOfLightSpell = "Flash of Light";
        private readonly string holyLightSpell = "Holy Light";
        private readonly string holyShockSpell = "Holy Shock";
        private readonly string layOnHandsSpell = "Lay on Hands";

        private readonly int buffCheckTime = 8;

        public PaladinHoly(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, flashOfLightSpell },
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
            if (ObjectManager.Player.ManaPercentage < 80
                && CastSpellIfPossible(divinePleaSpell, true))
            {
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
                        && CastSpellIfPossible(layOnHandsSpell))
                    {
                        return;
                    }

                    if (target.HealthPercentage < 50)
                    {
                        CastSpellIfPossible(divineFavorSpell, true);
                    }

                    if (ObjectManager.Player.ManaPercentage < 50
                       && ObjectManager.Player.ManaPercentage > 20)
                    {
                        CastSpellIfPossible(divineIlluminationSpell, true);
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
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();
            }
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);
            HookManager.TargetGuid(ObjectManager.PlayerGuid);

            if (!myBuffs.Any(e => e.Equals(devotionAuraSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(devotionAuraSpell, true))
            {
                return true;
            }

            if (!myBuffs.Any(e => e.Equals(blessingOfWisdomSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(blessingOfWisdomSpell, true))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
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
