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
                && IsSpellKnown(layOnHandsSpell)
                && !IsOnCooldown(layOnHandsSpell))
            {
                HookManager.CastSpell(layOnHandsSpell);
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 60
                && IsSpellKnown(holyLightSpell)
                && !IsOnCooldown(holyLightSpell))
            {
                HookManager.CastSpell(holyLightSpell);
                return;
            }

            if (IsSpellKnown(avengingWrathSpell)
                && HasEnoughMana(avengingWrathSpell)
                && !IsOnCooldown(avengingWrathSpell))
            {
                HookManager.CastSpell(avengingWrathSpell);
                return;
            }

            if (IsSpellKnown(divinePleaSpell)
                && ObjectManager.Player.ManaPercentage < 80
                && !IsOnCooldown(divinePleaSpell))
            {
                HookManager.CastSpell(divinePleaSpell);
                return;
            }

            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

            if (target != null)
            {
                if (ObjectManager.Player.HealthPercentage < 20
                && IsSpellKnown(hammerOfWrathSpell)
                && HasEnoughMana(hammerOfWrathSpell)
                && !IsOnCooldown(hammerOfWrathSpell))
                {
                    HookManager.CastSpell(hammerOfWrathSpell);
                    return;
                }
            }

            if (IsSpellKnown(crusaderStrikeSpell)
                && HasEnoughMana(crusaderStrikeSpell)
                && !IsOnCooldown(crusaderStrikeSpell))
            {
                HookManager.CastSpell(crusaderStrikeSpell);
                return;
            }

            if (IsSpellKnown(divineStormSpell)
                && HasEnoughMana(divineStormSpell)
                && !IsOnCooldown(divineStormSpell))
            {
                HookManager.CastSpell(divineStormSpell);
                return;
            }

            if (IsSpellKnown(divineStormSpell)
                && HasEnoughMana(divineStormSpell)
                && !IsOnCooldown(divineStormSpell))
            {
                HookManager.CastSpell(divineStormSpell);
                return;
            }

            if (IsSpellKnown(consecrationSpell)
                && HasEnoughMana(consecrationSpell)
                && !IsOnCooldown(consecrationSpell))
            {
                HookManager.CastSpell(consecrationSpell);
                return;
            }

            if (IsSpellKnown(exorcismSpell)
                && HasEnoughMana(exorcismSpell)
                && !IsOnCooldown(exorcismSpell))
            {
                HookManager.CastSpell(exorcismSpell);
                return;
            }

            if (IsSpellKnown(holyWrathSpell)
                && HasEnoughMana(holyWrathSpell)
                && !IsOnCooldown(holyWrathSpell))
            {
                HookManager.CastSpell(holyWrathSpell);
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

        private void HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }
            
            if (IsSpellKnown(sealOfVengeanceSpell)
                && !myBuffs.Any(e => e.Equals(sealOfVengeanceSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(sealOfVengeanceSpell))
            {
                HookManager.CastSpell(sealOfVengeanceSpell);
                return;
            }

            if (IsSpellKnown(judgementOfLightSpell)
                && myBuffs.Any(e => e.Equals(sealOfVengeanceSpell, StringComparison.OrdinalIgnoreCase))
                && HasEnoughMana(judgementOfLightSpell)
                && !IsOnCooldown(judgementOfLightSpell))
            {
                HookManager.CastSpell(judgementOfLightSpell);
                return;
            }

            if (IsSpellKnown(retributionAuraSpell)
                && !myBuffs.Any(e => e.Equals(retributionAuraSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(retributionAuraSpell))
            {
                HookManager.CastSpell(retributionAuraSpell);
                return;
            }

            if (IsSpellKnown(blessingOfMightSpell)
                && !myBuffs.Any(e => e.Equals(blessingOfMightSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(blessingOfMightSpell))
            {
                HookManager.CastSpell(blessingOfMightSpell);
                return;
            }

            LastBuffCheck = DateTime.Now;
        }

        private void HandleHammerOfWrath()
        {
            (string, int) castinInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);

            if (castinInfo.Item1.Length > 0
                && castinInfo.Item2 > 0
                && IsSpellKnown(hammerOfJusticeSpell)
                && !IsOnCooldown(hammerOfJusticeSpell))
            {
                HookManager.CastSpell(hammerOfJusticeSpell);
                return;
            }

            LastEnemyCastingCheck = DateTime.Now;
        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells.OrderByDescending(e => e.Rank).FirstOrDefault(e => e.Name.Equals(spellName))?.Costs <= ObjectManager.Player.Mana;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
                    => CharacterManager.SpellBook.Spells.Any(e => e.Name.Equals(spellName));
    }
}
