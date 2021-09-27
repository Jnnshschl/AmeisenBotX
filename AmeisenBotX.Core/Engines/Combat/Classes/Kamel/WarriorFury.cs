using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Kamel
{
    internal class WarriorFury : BasicKamelClass
    {
        private const string battleShoutSpell = "Battle Shout";
        private const string battleStanceSpell = "Battle Stance";
        private const string berserkerRageSpell = "Berserker Rage";
        private const string berserkerStanceSpell = "Berserker Stance";
        private const string bloodrageSpell = "Bloodrage";

        //Spells
        private const string bloodthirstSpell = "Bloodthirst";

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
        private const string pummelSpell = "Pummel";
        private const string recklessnessSpell = "Recklessness";
        private const string rendSpell = "Rend";

        //Buffs||Defensive||Enrage
        private const string retaliationSpell = "Retaliation";

        private const string ShatteringThrowSpell = "Shattering Throw";
        private const string ShootSpell = "Shoot";
        private const string slamSpell = "Slam";
        private const string victoryRushSpell = "Victory Rush";
        private const string whirlwindSpell = "Whirlwind";

        public WarriorFury(AmeisenBotInterfaces bot) : base()
        {
            Bot = bot;
            spellCoolDown.Add(ShootSpell, DateTime.Now);
            //Stances
            spellCoolDown.Add(defensiveStanceSpell, DateTime.Now);
            spellCoolDown.Add(battleStanceSpell, DateTime.Now);
            spellCoolDown.Add(berserkerStanceSpell, DateTime.Now);
            //Spells
            spellCoolDown.Add(heroicStrikeSpell, DateTime.Now);
            spellCoolDown.Add(interceptSpell, DateTime.Now);
            spellCoolDown.Add(heroicThrowSpell, DateTime.Now);
            spellCoolDown.Add(ShatteringThrowSpell, DateTime.Now);
            spellCoolDown.Add(executeSpell, DateTime.Now);
            spellCoolDown.Add(pummelSpell, DateTime.Now);
            spellCoolDown.Add(bloodthirstSpell, DateTime.Now);
            spellCoolDown.Add(slamSpell, DateTime.Now);
            spellCoolDown.Add(whirlwindSpell, DateTime.Now);
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
            RendEvent = new(TimeSpan.FromSeconds(6));
            ExecuteEvent = new(TimeSpan.FromSeconds(1));
        }

        public override string Author => "Lukas";

        public override Dictionary<string, dynamic> C { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Warrior Fury";

        public override string DisplayName => "Warrior Fury Final";

        public TimegatedEvent ExecuteEvent { get; private set; }

        public override bool HandlesMovement => false;

        public TimegatedEvent HeroicStrikeEvent { get; private set; }

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new() { WowArmorType.Shield }, new() { WowWeaponType.Sword, WowWeaponType.Mace, WowWeaponType.Axe, WowWeaponType.Staff, WowWeaponType.Dagger });

        //Time event
        public TimegatedEvent RendEvent { get; private set; }

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 3) },
                { 3, new(1, 3, 2) },
                { 5, new(1, 5, 2) },
                { 6, new(1, 6, 3) },
                { 9, new(1, 9, 2) },
                { 10, new(1, 10, 3) },
                { 11, new(1, 11, 3) },
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 3) },
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 6, new(2, 6, 3) },
                { 10, new(2, 10, 5) },
                { 13, new(2, 13, 3) },
                { 14, new(2, 14, 1) },
                { 16, new(2, 16, 1) },
                { 17, new(2, 17, 5) },
                { 18, new(2, 18, 3) },
                { 19, new(2, 19, 1) },
                { 20, new(2, 20, 2) },
                { 22, new(2, 22, 5) },
                { 23, new(2, 23, 1) },
                { 24, new(2, 24, 1) },
                { 25, new(2, 25, 3) },
                { 26, new(2, 26, 5) },
                { 27, new(2, 27, 1) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "3.0";

        public TimegatedEvent VictoryRushEvent { get; private set; }

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Warrior;

        public override void ExecuteCC()
        {
            StartAttack();
        }

        public override void OutOfCombatExecute()
        {
            Targetselection();
            StartAttack();
        }

        private bool CustomCastSpell(string spellName, string stance = "Berserker Stance")
        {
            if (!Bot.Character.SpellBook.IsSpellKnown(stance))
            {
                stance = "Battle Stance";
            }

            if (Bot.Character.SpellBook.IsSpellKnown(spellName))
            {
                if (Bot.Target != null)
                {
                    double distance = Bot.Player.Position.GetDistance(Bot.Target.Position);
                    Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                    if ((Bot.Player.Rage >= spell.Costs && IsSpellReady(spellName)))
                    {
                        if ((spell.MinRange == 0 && spell.MaxRange == 0) || (spell.MinRange <= distance && spell.MaxRange >= distance))
                        {
                            if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == stance))
                            {
                                Bot.Wow.CastSpell(stance);
                                return true;
                            }
                            else
                            {
                                Bot.Wow.CastSpell(spellName);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void StartAttack()
        {
            if (Bot.Wow.TargetGuid != 0)
            {
                ChangeTargetToAttack();

                if (Bot.Db.GetReaction(Bot.Player, Bot.Target) == WowUnitReaction.Friendly)
                {
                    Bot.Wow.ClearTarget();
                    return;
                }

                if (Bot.Player.IsInMeleeRange(Bot.Target))
                {
                    if (!Bot.Player.IsAutoAttacking && AutoAttackEvent.Run())
                    {
                        Bot.Wow.StartAutoAttack();
                    }

                    if (CustomCastSpell(bloodrageSpell))
                    {
                        return;
                    }

                    if (CustomCastSpell(berserkerRageSpell))
                    {
                        return;
                    }

                    if (CustomCastSpell(deathWishSpell))
                    {
                        return;
                    }

                    if (Bot.Target.IsCasting && CustomCastSpell(pummelSpell))
                    {
                        return;
                    }

                    if (Bot.Target.GetType() == typeof(IWowPlayer) && !Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Hamstring") && CustomCastSpell(hamstringSpell))
                    {
                        return;
                    }

                    if (Bot.Target.HealthPercentage <= 20 && CustomCastSpell(executeSpell))
                    {
                        return;
                    }

                    if (Bot.Player.HealthPercentage <= 50 && (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Bloodrage") || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Recklessness") || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Berserker Rage")))
                    {
                        if (CustomCastSpell(enragedregenerationSpell))
                        {
                            return;
                        }
                    }

                    if ((Bot.Player.HealthPercentage <= 30) || (Bot.Target.GetType() == typeof(IWowPlayer)) && CustomCastSpell(intimidatingShoutSpell))
                    {
                        return;
                    }

                    if (Bot.Player.HealthPercentage <= 60 && CustomCastSpell(retaliationSpell, battleStanceSpell))
                    {
                        return;
                    }

                    if (Bot.Target.GetType() == typeof(IWowPlayer) && CustomCastSpell(disarmSpell, defensiveStanceSpell))
                    {
                        return;
                    }

                    if (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Slam!") && CustomCastSpell(slamSpell) && CustomCastSpell(recklessnessSpell))
                    {
                        return;
                    }

                    if (CustomCastSpell(whirlwindSpell))
                    {
                        return;
                    }

                    if (CustomCastSpell(bloodthirstSpell))
                    {
                        return;
                    }

                    if (VictoryRushEvent.Run() && CustomCastSpell(victoryRushSpell))
                    {
                        return;
                    }

                    if (RendEvent.Run() && !Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Rend") && CustomCastSpell(rendSpell))
                    {
                        return;
                    }

                    if (HeroicStrikeEvent.Run() && Bot.Player.Rage >= 60 && CustomCastSpell(heroicStrikeSpell))
                    {
                        return;
                    }

                    IEnumerable<IWowUnit> unitsNearPlayer = Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, 5);

                    if (unitsNearPlayer != null)
                    {
                        if (unitsNearPlayer.Count() >= 3 && Bot.Player.Rage >= 50 && CustomCastSpell(cleaveSpell))
                        {
                            return;
                        }
                    }

                    if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Battle Shout") && CustomCastSpell(battleShoutSpell))
                    {
                        return;
                    }
                }
                else//Range
                {
                    if ((Bot.Player.IsDazed
                        || Bot.Player.IsFleeing
                        || Bot.Player.IsInfluenced
                        || Bot.Player.IsPossessed)
                        || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Frost Nova")
                        || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Frost Trap Aura")
                        || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Hamstring")
                        || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Concussive Shot")
                        || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Frostbolt")
                        || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Frost Shock")
                        || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Frostfire Bolt")
                        || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Slow")
                        || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Entangling Roots"))
                    {
                        if (CustomCastSpell(heroicFurySpell))
                        {
                            return;
                        }
                    }
                    if (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Entangling Roots")
                        || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Frost Nova"))
                    {
                        if (Bot.Movement.Status != Movement.Enums.MovementAction.None)
                        {
                            Bot.Wow.StopClickToMove();
                            Bot.Movement.Reset();
                        }

                        if (CustomCastSpell(ShootSpell))
                        {
                            return;
                        }

                        if (CustomCastSpell(ShatteringThrowSpell, battleStanceSpell))
                        {
                            return;
                        }
                    }
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
    }
}