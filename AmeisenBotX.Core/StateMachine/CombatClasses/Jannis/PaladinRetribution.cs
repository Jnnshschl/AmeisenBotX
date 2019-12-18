using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;
using static AmeisenBotX.Core.StateMachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class PaladinRetribution : BasicCombatClass
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
        
        public PaladinRetribution(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager) : base(objectManager, characterManager, hookManager)
        {
            BuffsToKeepOnMe = new Dictionary<string, CastFunction>()
            {
                { blessingOfMightSpell, () => CastSpellIfPossible(blessingOfMightSpell, true) },
                { retributionAuraSpell, () => CastSpellIfPossible(retributionAuraSpell, true) },
                { sealOfVengeanceSpell, () => CastSpellIfPossible(sealOfVengeanceSpell, true) }
            };
            
            InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(hammerOfJusticeSpell, true) }
            };
        }

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator();

        public override string Displayname => "Paladin Retribution";

        public override string Version => "1.0";

        public override string Author => "Jannis";

        public override string Description => "FCFS based CombatClass for the Retribution Paladin spec.";

        public override WowClass Class => WowClass.Paladin;

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (!ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
            }

            if (MyAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || (MyAuraManager.Buffs.Contains(sealOfVengeanceSpell.ToLower())
                    && CastSpellIfPossible(judgementOfLightSpell))
                || (ObjectManager.Player.HealthPercentage < 20
                    && CastSpellIfPossible(layOnHandsSpell))
                || (ObjectManager.Player.HealthPercentage < 60
                    && CastSpellIfPossible(holyLightSpell, true))
                || CastSpellIfPossible(avengingWrathSpell, true)
                || (ObjectManager.Player.ManaPercentage < 80
                    && CastSpellIfPossible(divinePleaSpell, true)))
            {
                return;
            }

            if (ObjectManager.Target != null)
            {
                if ((ObjectManager.Player.HealthPercentage < 20
                        && CastSpellIfPossible(hammerOfWrathSpell, true))
                    || CastSpellIfPossible(crusaderStrikeSpell, true)
                    || CastSpellIfPossible(divineStormSpell, true)
                    || CastSpellIfPossible(divineStormSpell, true)
                    || CastSpellIfPossible(consecrationSpell, true)
                    || CastSpellIfPossible(exorcismSpell, true)
                    || CastSpellIfPossible(holyWrathSpell, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick())
            {
                return;
            }
        }

        private bool CastSpellIfPossible(string spellName, bool needsMana = false)
        {
            AmeisenLogger.Instance.Log($"[{Displayname}]: Trying to cast \"{spellName}\" on \"{ObjectManager.Target?.Name}\"", LogLevel.Verbose);

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
                AmeisenLogger.Instance.Log($"[{Displayname}]: Casting Spell \"{spellName}\" on \"{ObjectManager.Target?.Name}\"", LogLevel.Verbose);
                return true;
            }

            return false;
        }
    }
}
