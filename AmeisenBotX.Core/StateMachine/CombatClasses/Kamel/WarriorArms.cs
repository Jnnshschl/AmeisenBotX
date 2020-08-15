using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Kamel
{
    class WarriorArms : BasicKamelClass
    {
        //Stances
        private const string defensiveStanceSpell = "Defensive Stance";
        private const string battleStanceSpell = "Battle Stance";
        private const string berserkerStanceSpell = "Berserker Stance";

        //Spells
        private const string chargeSpell = "Charge";
        private const string BladestormSpell = "Bladestorm";
        private const string MortalStrikeSpell = "Mortal Strike";
        private const string OverpowerSpell = "Overpower";
        private const string cleaveSpell = "Cleave";
        private const string disarmSpell = "Disarm";
        private const string executeSpell = "Execute";
        private const string hamstringSpell = "Hamstring";
        private const string heroicStrikeSpell = "Heroic Strike";
        private const string heroicThrowSpell = "Heroic Throw";
        private const string interceptSpell = "Intercept";
        private const string rendSpell = "Rend";
        private const string whirlwindSpell = "Whirlwind";
        private const string pummelSpell = "Pummel";
        private const string slamSpell = "Slam";
        private const string victoryRushSpell = "Victory Rush";

        //Buffs||Defensive||Enrage
        private const string retaliationSpell = "Retaliation";
        private const string berserkerRageSpell = "Berserker Rage";
        private const string commandingShoutSpell = "Commanding Shout";
        private const string deathWishSpell = "Death Wish";
        private const string enragedregenerationSpell = "Enraged Regeneration";
        private const string heroicFurySpell = "Heroic Fury";
        private const string intimidatingShoutSpell = "Intimidating Shout";
        private const string recklessnessSpell = "Recklessness";
        private const string bloodrageSpell = "Bloodrage";
        private const string battleShoutSpell = "Battle Shout";

        Dictionary<string, DateTime> spellCoolDown = new Dictionary<string, DateTime>();

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
            spellCoolDown.Add(whirlwindSpell, DateTime.Now);
            spellCoolDown.Add(disarmSpell, DateTime.Now);
            spellCoolDown.Add(rendSpell, DateTime.Now);
            spellCoolDown.Add(hamstringSpell, DateTime.Now);
            spellCoolDown.Add(victoryRushSpell, DateTime.Now);
            spellCoolDown.Add(chargeSpell, DateTime.Now);
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
            HeroicStrikeEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
            VictoryRushEvent = new TimegatedEvent(TimeSpan.FromSeconds(5));
            RendEvent = new TimegatedEvent(TimeSpan.FromSeconds(3));
            ExecuteEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
        }

        public override string Author => "Lukas";

        public override bool WalkBehindEnemy => false;

        public override WowClass Class => WowClass.Warrior;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Warrior Arms";

        public override string Displayname => "Warrior Arms Beta";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override bool UseAutoAttacks => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES, WeaponType.STAVES, WeaponType.DAGGERS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        //Time event
        public TimegatedEvent RendEvent { get; private set; }

        public TimegatedEvent HeroicStrikeEvent { get; private set; }

        public TimegatedEvent VictoryRushEvent { get; private set; }

        public TimegatedEvent ExecuteEvent { get; private set; }

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 3) },
                { 3, new Talent(1, 3, 2) },
                { 4, new Talent(1, 4, 2) },
                { 6, new Talent(1, 6, 3) },
                { 7, new Talent(1, 7, 2) },
                { 8, new Talent(1, 8, 1) },
                { 9, new Talent(1, 9, 2) },
                { 10, new Talent(1, 10, 3) },
                { 11, new Talent(1, 11, 3) },
                { 12, new Talent(1, 12, 3) },
                { 13, new Talent(1, 13, 5) },
                { 14, new Talent(1, 14, 1) },
                { 17, new Talent(1, 17, 2) },
                { 19, new Talent(1, 19, 2) },
                { 21, new Talent(1, 21, 1) },
                { 22, new Talent(1, 22, 2) },
                { 24, new Talent(1, 24, 1) },
                { 25, new Talent(1, 25, 3) },
                { 26, new Talent(1, 26, 2) },
                { 27, new Talent(1, 27, 3) },
                { 28, new Talent(1, 28, 1) },
                { 29, new Talent(1, 29, 2) },
                { 30, new Talent(1, 30, 5) },
                { 31, new Talent(1, 31, 1) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 3) },
                { 2, new Talent(2, 2, 2) },
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 7, new Talent(2, 7, 1) },
            },
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override void ExecuteCC()
        {
            StartAttack();
        }

        public override void OutOfCombatExecute()
        {
            Targetselection();
        }


        private void StartAttack()
        {
            if (WowInterface.ObjectManager.TargetGuid != 0)
            {
                if (WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target) == WowUnitReaction.Friendly)
                {
                    WowInterface.HookManager.ClearTarget();
                    return;
                }

                if (WowInterface.ObjectManager.Player.IsInMeleeRange(WowInterface.ObjectManager.Target))
                {
                    if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run())
                    {
                        WowInterface.HookManager.StartAutoAttack();
                    }

                    if (WowInterface.ObjectManager.Target.IsCasting && CustomCastSpell(pummelSpell))
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

                    if (WowInterface.ObjectManager.Player.HealthPercentage <= 50 && CustomCastSpell(intimidatingShoutSpell))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage <= 60 && CustomCastSpell(retaliationSpell, battleStanceSpell))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage <= 50 && WowInterface.ObjectManager.Target.HasBuffByName("Enrage") && CustomCastSpell(enragedregenerationSpell))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Target.GetType() == typeof(WowPlayer) && CustomCastSpell(disarmSpell, defensiveStanceSpell))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Target.GetType() == typeof(WowPlayer) && !WowInterface.ObjectManager.Target.HasBuffByName("Hamstring") && CustomCastSpell(hamstringSpell))
                    {
                        return;
                    }

                    if (VictoryRushEvent.Run() && CustomCastSpell(victoryRushSpell))
                    {
                        return;
                    }

                    if ((WowInterface.ObjectManager.Target.HealthPercentage <= 20 && CustomCastSpell(executeSpell)) || (WowInterface.ObjectManager.Player.HasBuffByName("Sudden Death") && CustomCastSpell(executeSpell)))
                    {
                        return;
                    }

                    if (RendEvent.Run() && !WowInterface.ObjectManager.Target.HasBuffByName("Rend") && CustomCastSpell(rendSpell))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HasBuffByName("Taste for Blood") && CustomCastSpell(OverpowerSpell))
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

                    int nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 10.0).Count();

                    if (nearEnemies > 2)
                    {
                        if (HeroicStrikeEvent.Run() && WowInterface.ObjectManager.Player.Rage >= 40 && CustomCastSpell(cleaveSpell))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (HeroicStrikeEvent.Run() && WowInterface.ObjectManager.Player.Rage >= 40 && CustomCastSpell(heroicStrikeSpell))
                        {
                            return;
                        }
                    }

                    if (!WowInterface.ObjectManager.Player.HasBuffByName("Battle Shout") && CustomCastSpell(battleShoutSpell))
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

        private void Targetselection()
        {
            if (TargetSelectEvent.Run())
            {
                WowUnit nearTarget = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 40)
                .Where(e => e.IsInCombat && !e.IsNotAttackable && e.Name != "The Lich King" && !(WowInterface.ObjectManager.MapId == MapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346))
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .FirstOrDefault();

                if (nearTarget != null)
                {
                    WowInterface.HookManager.TargetGuid(nearTarget.Guid);

                    if (!TargetInLineOfSight)
                    {
                        return;
                    }
                }
            }
        }
        private bool CustomCastSpell(string spellName, string stance = "Battle Stance")
        {
            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(spellName))
            {
                double distance = WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position);
                Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                if ((WowInterface.ObjectManager.Player.Rage >= spell.Costs && IsSpellReady(spellName)))
                {
                    if ((spell.MinRange == 0 && spell.MaxRange == 0) || (spell.MinRange <= distance && spell.MaxRange >= distance))
                    {
                        if (!WowInterface.ObjectManager.Player.HasBuffByName(stance))
                        {
                            WowInterface.HookManager.CastSpell(stance);
                            return true;
                        }
                        else
                        {
                            WowInterface.HookManager.CastSpell(spellName);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        private bool IsSpellReady(string spellName)
        {
            if (DateTime.Now > spellCoolDown[spellName])
            {
                spellCoolDown[spellName] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(spellName));
                return true;
            }

            return false;
        }
    }
}
