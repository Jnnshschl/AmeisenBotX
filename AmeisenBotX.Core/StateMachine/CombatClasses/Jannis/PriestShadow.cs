using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class PriestShadow : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly int deadPartymembersCheckTime = 4;
        private readonly string devouringPlagueSpell = "Devouring Plague";
        private readonly string flashHealSpell = "Flash Heal";
        private readonly string hymnOfHopeSpell = "Hymn of Hope";
        private readonly string mindBlastSpell = "Mind Blast";
        private readonly string mindFlaySpell = "Mind Flay";
        private readonly string powerWordFortitudeSpell = "Power Word: Fortitude";
        private readonly string resurrectionSpell = "Resurrection";
        private readonly string shadowfiendSpell = "Shadowfiend";
        private readonly string shadowformSpell = "Shadowform";
        private readonly string shadowWordPainSpell = "Shadow Word: Pain";
        private readonly string vampiricEmbraceSpell = "Vampiric Embrace";
        private readonly string vampiricTouchSpell = "Vampiric Touch";

        public PriestShadow(WowInterface wowInterface) : base(wowInterface)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { shadowformSpell, () => CastSpellIfPossible(shadowformSpell, true) },
                { powerWordFortitudeSpell, () =>
                    {
                        HookManager.TargetGuid(WowInterface.ObjectManager.PlayerGuid);
                        return CastSpellIfPossible(powerWordFortitudeSpell, true);
                    }
                },
                { vampiricEmbraceSpell, () => CastSpellIfPossible(vampiricEmbraceSpell, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { vampiricTouchSpell, () => CastSpellIfPossible(vampiricTouchSpell, true) },
                { devouringPlagueSpell, () => CastSpellIfPossible(devouringPlagueSpell, true) },
                { shadowWordPainSpell, () => CastSpellIfPossible(shadowWordPainSpell, true) },
                { mindBlastSpell, () => CastSpellIfPossible(mindBlastSpell, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Priest;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Shadow Priest spec.";

        public override string Displayname => "Priest Shadow";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator();

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private DateTime LastDeadPartymembersCheck { get; set; }

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick())
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.ManaPercentage < 30
                && CastSpellIfPossible(hymnOfHopeSpell))
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.ManaPercentage < 90
                && CastSpellIfPossible(shadowfiendSpell))
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 70
                && CastSpellIfPossible(flashHealSpell))
            {
                HookManager.CastSpell(flashHealSpell);
                return;
            }

            if (!WowInterface.ObjectManager.Player.IsCasting
                && CastSpellIfPossible(mindFlaySpell, true))
            {
                return;
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick())
            {
                return;
            }

            if (DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime)
                && HandleDeadPartymembers())
            {
                return;
            }
        }

        private bool HandleDeadPartymembers()
        {
            if (!Spells.ContainsKey(resurrectionSpell))
            {
                Spells.Add(resurrectionSpell, WowInterface.CharacterManager.SpellBook.GetSpellByName(resurrectionSpell));
            }

            if (Spells[resurrectionSpell] != null
                && !CooldownManager.IsSpellOnCooldown(resurrectionSpell)
                && Spells[resurrectionSpell].Costs < WowInterface.ObjectManager.Player.Mana)
            {
                IEnumerable<WowPlayer> players = WowInterface.ObjectManager.WowObjects.OfType<WowPlayer>();
                List<WowPlayer> groupPlayers = players.Where(e => e.IsDead && e.Health == 0 && WowInterface.ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

                if (groupPlayers.Count > 0)
                {
                    HookManager.TargetGuid(groupPlayers.First().Guid);
                    HookManager.CastSpell(resurrectionSpell);
                    CooldownManager.SetSpellCooldown(resurrectionSpell, (int)HookManager.GetSpellCooldown(resurrectionSpell));
                }
            }

            LastDeadPartymembersCheck = DateTime.Now;
            return true;
        }
    }
}