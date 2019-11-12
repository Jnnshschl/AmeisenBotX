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
    public class PaladinRetribution : ICombatClass
    {
        // author: Jannis Höschele

        private readonly string blessingOfMightSpell = "Blessing of Might";
        private readonly string retributionAuraSpell = "Retribution Aura";
        private readonly string avengingWrathSpell = "Avenging Wrath";
        private readonly string sealOfVengeanceSpell = "Seal of Vengeance";
        private readonly string hammerOfWrathSpell = "Hammer of Wrath";
        private readonly string hammerOfJusticeSpell = "Hammer of Justice";
        private readonly string judgementOfLightSpell = "Judgement of Light";
        private readonly string crusaderStrikeSpell = "Crusader Strike";
        private readonly string divineStormSpell = "Divine Storm";
        private readonly string consecrationSpell = "Consecration";
        private readonly string exorcismSpell = "Exorcism";
        private readonly string holyWrathSpell = "Holy Wrath";
        private readonly string divinePleaSpell = "Divine Plea";
        private readonly string holyLightSpell = "Holy Light";
        private readonly string layOnHandsSpell = "Lay on Hands";

        private readonly int buffCheckTime = 4;
        private readonly int enemyCastingCheckTime = 1;

        public PaladinRetribution(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => true;

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastEnemyCastingCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        public void Execute()
        {
            if (!ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
            }

            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();
            }

            if (IsSpellKnown(hammerOfWrathSpell) && DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime))
            {
                HandleHammerOfWrath();
            }

            if (ObjectManager.Player.HealthPercentage < 20
                && CastSpellIfPossible(layOnHandsSpell))
            {
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 60
                && CastSpellIfPossible(holyLightSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(avengingWrathSpell, true))
            {
                return;
            }

            if (ObjectManager.Player.ManaPercentage < 80
                && CastSpellIfPossible(divinePleaSpell, true))
            {
                return;
            }

            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

            if (target != null)
            {
                if (ObjectManager.Player.HealthPercentage < 20
                && CastSpellIfPossible(hammerOfWrathSpell,true))
                {
                    return;
                }
            }

            if (CastSpellIfPossible(crusaderStrikeSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(divineStormSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(divineStormSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(consecrationSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(exorcismSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(holyWrathSpell, true))
            {
                return;
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

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if (!myBuffs.Any(e => e.Equals(sealOfVengeanceSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(sealOfVengeanceSpell, true))
            {
                return true;
            }

            if (myBuffs.Any(e => e.Equals(sealOfVengeanceSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(judgementOfLightSpell, true))
            {
                return true;
            }

            if (!myBuffs.Any(e => e.Equals(retributionAuraSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(retributionAuraSpell, true))
            {
                return true;
            }

            if (!myBuffs.Any(e => e.Equals(blessingOfMightSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(blessingOfMightSpell, true))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleHammerOfWrath()
        {
            (string, int) castinInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);

            bool isCasting = castinInfo.Item1.Length > 0 && castinInfo.Item2 > 0;

            if (isCasting
                && CastSpellIfPossible(hammerOfJusticeSpell, true))
            {
                return true;
            }

            LastEnemyCastingCheck = DateTime.Now;
            return false;
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
    }
}
