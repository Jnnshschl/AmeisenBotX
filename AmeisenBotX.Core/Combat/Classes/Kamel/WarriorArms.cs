using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Kamel
{
    internal class WarriorArms : BasicKamelClass
    {
        private const string battleShoutSpell = "Battle Shout";

        private const string battleStanceSpell = "Battle Stance";

        private const string berserkerRageSpell = "Berserker Rage";

        private const string berserkerStanceSpell = "Berserker Stance";

        private const string BladestormSpell = "Bladestorm";

        private const string bloodrageSpell = "Bloodrage";

        //Spells
        private const string chargeSpell = "Charge";

        private const string cleaveSpell = "Cleave";

        private const string commandingShoutSpell = "Commanding Shout";

        private const string deathWishSpell = "Death Wish";

        //Stances
        private const string defensiveStanceSpell = "Defensive Stance";

        private const string disarmSpell = "Disarm";
        private const string enragedregenerationSpell = "Enraged Regeneration";
        private const string executeSpell = "Execute";
        private const string hamstringSpell = "Hamstring";
        private const string heroicFurySpell = "Heroic Fury";
        private const string heroicStrikeSpell = "Heroic Strike";
        private const string heroicThrowSpell = "Heroic Throw";
        private const string interceptSpell = "Intercept";
        private const string intimidatingShoutSpell = "Intimidating Shout";
        private const string MortalStrikeSpell = "Mortal Strike";
        private const string OverpowerSpell = "Overpower";
        private const string pummelSpell = "Pummel";
        private const string recklessnessSpell = "Recklessness";
        private const string rendSpell = "Rend";

        //Buffs||Defensive||Enrage
        private const string retaliationSpell = "Retaliation";

        private const string slamSpell = "Slam";
        private const string victoryRushSpell = "Victory Rush";

        public WarriorArms(WowInterface wowInterface) : base()
        {
            WowInterface = wowInterface;
            //Stances
            spellCoolDown.Add(defensiveStanceSpell, DateTime.Now);
            spellCoolDown.Add(battleStanceSpell, DateTime.Now);
            spellCoolDown.Add(berserkerStanceSpell, DateTime.Now);
            //Spells
            spellCoolDown.Add(heroicStrikeSpell, DateTime.Now);
            spellCoolDown.Add(BladestormSpell, DateTime.Now);
            spellCoolDown.Add(OverpowerSpell, DateTime.Now);
            spellCoolDown.Add(MortalStrikeSpell, DateTime.Now);
            spellCoolDown.Add(interceptSpell, DateTime.Now);
            spellCoolDown.Add(heroicThrowSpell, DateTime.Now);
            spellCoolDown.Add(executeSpell, DateTime.Now);
            spellCoolDown.Add(pummelSpell, DateTime.Now);
            spellCoolDown.Add(slamSpell, DateTime.Now);
            spellCoolDown.Add(disarmSpell, DateTime.Now);
            spellCoolDown.Add(rendSpell, DateTime.Now);
            spellCoolDown.Add(hamstringSpell, DateTime.Now);
            spellCoolDown.Add(victoryRushSpell, DateTime.Now);
            spellCoolDown.Add(chargeSpell, DateTime.Now);
            spellCoolDown.Add(cleaveSpell, DateTime.Now);
            //Buffs||Defensive||Enrage
            spellCoolDown.Add(intimidatingShoutSpell, DateTime.Now);
            spellCoolDown.Add(retaliationSpell, DateTime.Now);
            spellCoolDown.Add(enragedregenerationSpell, DateTime.Now);
            spellCoolDown.Add(bloodrageSpell, DateTime.Now);
            spellCoolDown.Add(commandingShoutSpell, DateTime.Now);
            spellCoolDown.Add(recklessnessSpell, DateTime.Now);
            spellCoolDown.Add(heroicFurySpell, DateTime.Now);
            spellCoolDown.Add(berserkerRageSpell, DateTime.Now);
            spellCoolDown.Add(deathWishSpell, DateTime.Now);
            spellCoolDown.Add(battleShoutSpell, DateTime.Now);

            //Time event
            HeroicStrikeEvent = new(TimeSpan.FromSeconds(2));
            VictoryRushEvent = new(TimeSpan.FromSeconds(5));
            RendEvent = new(TimeSpan.FromSeconds(3));
            ExecuteEvent = new(TimeSpan.FromSeconds(1));
        }

        public override string Author => "Lukas";

        public override Dictionary<string, dynamic> C { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Warrior Arms";

        public override string Displayname => "Warrior Arms Beta";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES, WowWeaponType.STAVES, WowWeaponType.DAGGERS });

        //Time event
        public TimegatedEvent RendEvent { get; private set; }

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 3) },
                { 3, new(1, 3, 2) },
                { 4, new(1, 4, 2) },
                { 6, new(1, 6, 3) },
                { 7, new(1, 7, 2) },
                { 8, new(1, 8, 1) },
                { 9, new(1, 9, 2) },
                { 10, new(1, 10, 3) },
                { 11, new(1, 11, 3) },
                { 12, new(1, 12, 3) },
                { 13, new(1, 13, 5) },
                { 14, new(1, 14, 1) },
                { 17, new(1, 17, 2) },
                { 19, new(1, 19, 2) },
                { 21, new(1, 21, 1) },
                { 22, new(1, 22, 2) },
                { 24, new(1, 24, 1) },
                { 25, new(1, 25, 3) },
                { 26, new(1, 26, 2) },
                { 27, new(1, 27, 3) },
                { 28, new(1, 28, 1) },
                { 29, new(1, 29, 2) },
                { 30, new(1, 30, 5) },
                { 31, new(1, 31, 1) },
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 3) },
                { 2, new(2, 2, 2) },
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 7, new(2, 7, 1) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Warrior;

        public TimegatedEvent HeroicStrikeEvent { get; private set; }
        public TimegatedEvent VictoryRushEvent { get; private set; }
        public TimegatedEvent ExecuteEvent { get; private set; }

        public override void ExecuteCC()
        {
            StartAttack();
        }

        public override void OutOfCombatExecute()
        {
            Targetselection();
            StartAttack();
        }

        private void StartAttack()
        {
            if (WowInterface.TargetGuid != 0 && WowInterface.Target != null)
            {
                if (WowInterface.HookManager.WowGetUnitReaction(WowInterface.Player, WowInterface.Target) == WowUnitReaction.Friendly)
                {
                    WowInterface.HookManager.WowClearTarget();
                    return;
                }

                if (WowInterface.Player.IsInMeleeRange(WowInterface.Target))
                {
                    if (!WowInterface.Player.IsAutoAttacking && AutoAttackEvent.Run())
                    {
                        WowInterface.HookManager.LuaStartAutoAttack();
                    }

                    if (WowInterface.Target.IsCasting && CustomCastSpell(pummelSpell))
                    {
                        return;
                    }

                    if (CustomCastSpell(bloodrageSpell))
                    {
                        return;
                    }

                    if (CustomCastSpell(berserkerRageSpell))
                    {
                        return;
                    }

                    if (CustomCastSpell(recklessnessSpell, berserkerStanceSpell))
                    {
                        return;
                    }

                    if (WowInterface.Player.HealthPercentage <= 50 && CustomCastSpell(intimidatingShoutSpell))
                    {
                        return;
                    }

                    if (WowInterface.Player.HealthPercentage <= 60 && CustomCastSpell(retaliationSpell, battleStanceSpell))
                    {
                        return;
                    }

                    if (WowInterface.Player.HealthPercentage <= 50 && WowInterface.Player.HasBuffByName("Enrage") && CustomCastSpell(enragedregenerationSpell))
                    {
                        return;
                    }

                    if (WowInterface.Target.GetType() == typeof(WowPlayer) && CustomCastSpell(disarmSpell, defensiveStanceSpell))
                    {
                        return;
                    }

                    if (WowInterface.Target.GetType() == typeof(WowPlayer) && !WowInterface.Target.HasBuffByName("Hamstring") && CustomCastSpell(hamstringSpell))
                    {
                        return;
                    }

                    if (VictoryRushEvent.Run() && CustomCastSpell(victoryRushSpell))
                    {
                        return;
                    }

                    if ((WowInterface.Target.HealthPercentage <= 20 && CustomCastSpell(executeSpell)) || (WowInterface.Player.HasBuffByName("Sudden Death") && CustomCastSpell(executeSpell)))
                    {
                        return;
                    }

                    if (RendEvent.Run() && !WowInterface.Target.HasBuffByName("Rend") && CustomCastSpell(rendSpell))
                    {
                        return;
                    }

                    if (WowInterface.Player.HasBuffByName("Taste for Blood") && CustomCastSpell(OverpowerSpell))
                    {
                        return;
                    }

                    if (CustomCastSpell(MortalStrikeSpell))
                    {
                        return;
                    }

                    if (CustomCastSpell(BladestormSpell))
                    {
                        return;
                    }

                    if (!WowInterface.Player.HasBuffByName("Battle Shout") && CustomCastSpell(battleShoutSpell))
                    {
                        return;
                    }
                }
                else//Range
                {
                    if (CustomCastSpell(interceptSpell))
                    {
                        return;
                    }
                    if (CustomCastSpell(chargeSpell, battleStanceSpell))
                    {
                        return;
                    }
                    if (CustomCastSpell(heroicThrowSpell))
                    {
                        return;
                    }
                }
            }
            else
            {
                Targetselection();
            }
        }
        private bool CustomCastSpell(string spellName, string stance = "Battle Stance")
        {
            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(spellName))
            {
                double distance = WowInterface.Player.Position.GetDistance(WowInterface.Target.Position);
                Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                if ((WowInterface.Player.Rage >= spell.Costs && IsSpellReady(spellName)))
                {
                    if ((spell.MinRange == 0 && spell.MaxRange == 0) || (spell.MinRange <= distance && spell.MaxRange >= distance))
                    {
                        if (!WowInterface.Player.HasBuffByName(stance))
                        {
                            WowInterface.HookManager.LuaCastSpell(stance);
                            return true;
                        }
                        else
                        {
                            WowInterface.HookManager.LuaCastSpell(spellName);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}