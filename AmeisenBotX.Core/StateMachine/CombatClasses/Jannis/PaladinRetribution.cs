using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
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
        private readonly int judgementCheckTime = 1;
        private readonly int enemyCastingCheckTime = 1;

        public PaladinRetribution(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            CooldownManager = new CooldownManager(characterManager.SpellBook.Spells);

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

        public bool HandlesTargetSelection => false;

        public bool IsMelee => true;

        public IWowItemComparator ItemComparator => null;

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastJudgementCheck { get; set; }

        private DateTime LastEnemyCastingCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private CooldownManager CooldownManager { get; }

        private Dictionary<string, Spell> Spells { get; }

        public string Displayname => "Paladin Retribution";

        public string Version => "1.0";

        public string Author => "Jannis";

        public string Description => "FCFS based CombatClass for the Retribution Paladin spec.";

        public WowClass Class => WowClass.Paladin;

        public CombatClassRole Role => CombatClassRole.Dps;

        public Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public void Execute()
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

            if ((DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                    && HandleBuffing())
                || (DateTime.Now - LastJudgementCheck > TimeSpan.FromSeconds(judgementCheckTime)
                    && HandleJudgement())
                || (DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime)
                    && HandleHammerOfJustice())
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

        private bool HandleJudgement()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (myBuffs.Any(e => e.Equals(sealOfVengeanceSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(judgementOfLightSpell, true))
            {
                return true;
            }

            LastJudgementCheck = DateTime.Now;
            return false;
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                && HandleBuffing())
            {
                return;
            }
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if ((!myBuffs.Any(e => e.Equals(sealOfVengeanceSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(sealOfVengeanceSpell, true))
                || (!myBuffs.Any(e => e.Equals(retributionAuraSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(retributionAuraSpell, true))
                || (!myBuffs.Any(e => e.Equals(blessingOfMightSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(blessingOfMightSpell, true)))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleHammerOfJustice()
        {
            //WowUnit castingUnit = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.IsInCombat && e.IsCasting);
            if (ObjectManager.Target.IsCasting)
            {
                if (CastSpellIfPossible(hammerOfJusticeSpell, true))
                {
                    return true;
                }
            }

            LastEnemyCastingCheck = DateTime.Now;
            return false;
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
    }
}
