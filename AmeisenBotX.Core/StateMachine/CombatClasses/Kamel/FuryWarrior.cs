using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Kamel
{
    class FuryWarrior : BasicKamelClass
    {

        private const string battleStanceSpell = "Battle Stance";
        private const string berserkerStanceSpell = "Berserker Stance";
        private const string berserkerRageSpell = "Berserker Rage";
        private const string bladestormSpell = "Bladestorm";
        private const string bloodthirstSpell = "Bloodthirst";
        private const string chargeSpell = "Charge";
        private const string cleaveSpell = "Cleave";
        private const string commandingShoutSpell = "Commanding Shout";
        private const string disarmSpell = "Disarm";
        private const string executeSpell = "Execute";
        private const string hamstringSpell = "Hamstring";
        private const string heroicStrikeSpell = "Heroic Strike";
        private const string heroicThrowSpell = "Heroic Throw";
        private const string heroicFurySpell = "Heroic Fury";
        private const string interceptSpell = "Intercept";
        private const string intimidatingShoutSpell = "Intimidating Shout";
        private const string rendSpell = "Rend";
        private const string whirlwindSpell = "Whirlwind";
        private const string retaliationSpell = "Retaliation";
        private const string enragedregenerationSpell = "Enraged Regeneration";
        private const string bloodrageSpell = "Bloodrage";
        private const string pummelSpell = "Pummel";
        private const string slamSpell = "Slam";
        private const string recklessnessSpell = "Recklessness";
        private const string defensiveStanceSpell = "Defensive Stance";

        Dictionary<string, DateTime> spellCoolDown = new Dictionary<string, DateTime>();

        public FuryWarrior(WowInterface wowInterface) : base()
        {
            WowInterface = wowInterface;
            spellCoolDown.Add(interceptSpell, DateTime.Now);
            spellCoolDown.Add(heroicThrowSpell, DateTime.Now);
            spellCoolDown.Add(intimidatingShoutSpell, DateTime.Now);
            spellCoolDown.Add(retaliationSpell, DateTime.Now);
            spellCoolDown.Add(heroicStrikeSpell, DateTime.Now);
            spellCoolDown.Add(executeSpell, DateTime.Now);
            spellCoolDown.Add(enragedregenerationSpell, DateTime.Now);
            spellCoolDown.Add(bloodrageSpell, DateTime.Now);
            spellCoolDown.Add(pummelSpell, DateTime.Now);
            spellCoolDown.Add(bloodthirstSpell, DateTime.Now);
            spellCoolDown.Add(commandingShoutSpell, DateTime.Now);
            spellCoolDown.Add(slamSpell, DateTime.Now);
            spellCoolDown.Add(recklessnessSpell, DateTime.Now);
            spellCoolDown.Add(whirlwindSpell, DateTime.Now);
            spellCoolDown.Add(heroicFurySpell, DateTime.Now);
            spellCoolDown.Add(berserkerRageSpell, DateTime.Now);
            spellCoolDown.Add(disarmSpell, DateTime.Now);

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, Utils.AuraManager.CastFunction>();
            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(berserkerStanceSpell))
                {
                    MyAuraManager.BuffsToKeepActive.Add(berserkerStanceSpell, () => { WowInterface.HookManager.CastSpell(berserkerStanceSpell); return true; });
                }
                else if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(battleStanceSpell))
                {
                    MyAuraManager.BuffsToKeepActive.Add(battleStanceSpell, () => { WowInterface.HookManager.CastSpell(berserkerStanceSpell); return true; });
                }
            };
        }

        //public override string ToString()
        //{
        //    return $"[{Class}] [{Role}] {Displayname}";
        //}
        //
        public override string Author => "Kamel";

        public override bool WalkBehindEnemy => false;

        public override WowClass Class => WowClass.Warrior;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Basic Fury Warrior";

        public override string Displayname => "Warrior Fury";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override bool UseAutoAttacks => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 3) },
                { 3, new Talent(1, 3, 2) },
                { 5, new Talent(1, 5, 2) },
                { 6, new Talent(1, 6, 3) },
                { 9, new Talent(1, 9, 2) },
                { 10, new Talent(1, 10, 3) },
                { 11, new Talent(1, 11, 3) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 3) },
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 6, new Talent(2, 6, 3) },
                { 10, new Talent(2, 10, 5) },
                { 13, new Talent(2, 13, 3) },
                { 14, new Talent(2, 14, 1) },
                { 16, new Talent(2, 16, 1) },
                { 17, new Talent(2, 17, 5) },
                { 18, new Talent(2, 18, 3) },
                { 19, new Talent(2, 19, 1) },
                { 20, new Talent(2, 20, 2) },
                { 22, new Talent(2, 22, 5) },
                { 23, new Talent(2, 23, 1) },
                { 24, new Talent(2, 24, 1) },
                { 25, new Talent(2, 25, 3) },
                { 26, new Talent(2, 26, 5) },
                { 27, new Talent(2, 27, 1) },
            },
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override void ExecuteCC()
        {
            MyAuraManager.Tick();

            if (WowInterface.ObjectManager.TargetGuid != 0)
            {
                if (WowInterface.ObjectManager.Player.IsInMeleeRange(WowInterface.ObjectManager.Target))
                {
                    if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run())
                    {

                        WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
                    }
                    if (DateTime.Now > spellCoolDown[berserkerRageSpell] && WowInterface.ObjectManager.Player.IsFleeing)
                    {
                        WowInterface.HookManager.CastSpell(berserkerRageSpell);
                        spellCoolDown[berserkerRageSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(berserkerRageSpell));
                        return;
                    }
                    
                    if (DateTime.Now > spellCoolDown[disarmSpell])
                    {
                        WowInterface.HookManager.CastSpell(defensiveStanceSpell);
                        WowInterface.HookManager.CastSpell(disarmSpell);
                        spellCoolDown[disarmSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(disarmSpell));
                        return;
                    }  
                    
                    if (DateTime.Now > spellCoolDown[heroicFurySpell] && WowInterface.ObjectManager.Player.IsSilenced)
                    {
                        WowInterface.HookManager.CastSpell(heroicFurySpell);
                        spellCoolDown[heroicFurySpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(heroicFurySpell));
                        return;
                    }

                    if (DateTime.Now > spellCoolDown[retaliationSpell] && WowInterface.ObjectManager.Player.HealthPercentage < 60)
                    {
                        WowInterface.HookManager.CastSpell(battleStanceSpell);
                        WowInterface.HookManager.CastSpell(retaliationSpell);
                        spellCoolDown[retaliationSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(retaliationSpell));
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage <= 50)
                    {
                        if (DateTime.Now > spellCoolDown[bloodrageSpell] && WowInterface.ObjectManager.Player.Rage >= 15 && WowInterface.ObjectManager.Player.HealthPercentage <= 30)
                        {
                            WowInterface.HookManager.CastSpell(bloodrageSpell);
                            spellCoolDown[bloodrageSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(bloodrageSpell));
                            return;
                        }
                        if (WowInterface.ObjectManager.Player.HasBuffByName("Bloodrage"))
                        {
                            if (DateTime.Now > spellCoolDown[enragedregenerationSpell] && WowInterface.ObjectManager.Player.Rage >= 15 && WowInterface.ObjectManager.Player.HealthPercentage <= 30)
                            {
                                WowInterface.HookManager.CastSpell(enragedregenerationSpell);
                                spellCoolDown[enragedregenerationSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(enragedregenerationSpell));
                                return;
                            }
                        }
                    }

                    if (WowInterface.ObjectManager.Player.HasBuffByName("Slam!") && WowInterface.ObjectManager.Player.Rage >= 15)
                    {
                        if (DateTime.Now > spellCoolDown[recklessnessSpell])
                        {
                            WowInterface.HookManager.CastSpell(recklessnessSpell);
                            spellCoolDown[recklessnessSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(recklessnessSpell));
                            return;
                        }
                        if (DateTime.Now > spellCoolDown[slamSpell])
                        {
                            WowInterface.HookManager.CastSpell(slamSpell);
                            spellCoolDown[slamSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(slamSpell));
                            return;
                        }
                    }

                    if (DateTime.Now > spellCoolDown[bloodthirstSpell] && WowInterface.ObjectManager.Player.Rage >= 20 && WowInterface.ObjectManager.Player.HealthPercentage <= 80)
                    {
                        WowInterface.HookManager.CastSpell(bloodthirstSpell);
                        spellCoolDown[bloodthirstSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(bloodthirstSpell));
                        return;
                    }

                    if (DateTime.Now > spellCoolDown[whirlwindSpell] && WowInterface.ObjectManager.Player.Rage >= 25)
                    {
                        WowInterface.HookManager.CastSpell(whirlwindSpell);
                        spellCoolDown[whirlwindSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(whirlwindSpell));
                        return;
                    }

                    if (HeroicStrikeEvent.Run() && DateTime.Now > spellCoolDown[heroicStrikeSpell] && WowInterface.ObjectManager.Player.Rage >= 25)
                    {
                        WowInterface.HookManager.CastSpell(heroicStrikeSpell);
                        spellCoolDown[heroicStrikeSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(heroicStrikeSpell));
                    }

                    if (DateTime.Now > spellCoolDown[executeSpell] && WowInterface.ObjectManager.Player.Rage >= 15 && WowInterface.ObjectManager.Target.Health <= 20)
                    {
                        WowInterface.HookManager.CastSpell(executeSpell);
                        spellCoolDown[executeSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(executeSpell));
                        return;
                    }

                    if (DateTime.Now > spellCoolDown[pummelSpell] && WowInterface.ObjectManager.Player.Rage >= 10 && WowInterface.ObjectManager.Target.IsCasting)
                    {
                        WowInterface.HookManager.CastSpell(pummelSpell);
                        spellCoolDown[pummelSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(pummelSpell));
                        return;
                    }

                }
                else//Range
                {
                    double distance = WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position);

                    if (DateTime.Now > spellCoolDown[interceptSpell] && distance <= 20 && distance >= 8)
                    {
                        WowInterface.HookManager.CastSpell(interceptSpell);
                        spellCoolDown[interceptSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(interceptSpell));
                        return;
                    }

                    if (DateTime.Now > spellCoolDown[heroicThrowSpell] && distance <= 30)
                    {
                        WowInterface.HookManager.CastSpell(heroicThrowSpell);
                        spellCoolDown[heroicThrowSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(heroicThrowSpell));
                        //WowInterface.HookManager.SendChatMessage("/y Du kack haiter");
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.Rage >= 25 && distance <= 8)
                    {
                        if (DateTime.Now > spellCoolDown[intimidatingShoutSpell] && WowInterface.ObjectManager.Player.HealthPercentage < 60
                            || WowInterface.ObjectManager.Target.IsCasting)
                        {
                            WowInterface.HookManager.CastSpell(intimidatingShoutSpell);
                            spellCoolDown[intimidatingShoutSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(intimidatingShoutSpell));
                            //WowInterface.HookManager.SendChatMessage("/y OGERGRUNZEN");
                            return;
                        }
                    }
                }
            }
            else if (TargetSelectEvent.Run())
            {
                WowUnit nearTarget = WowInterface.ObjectManager.GetEnemiesTargetingPartymembers(WowInterface.ObjectManager.Player.Position, 20).FirstOrDefault();

                if (nearTarget != null)
                {
                    WowInterface.HookManager.TargetGuid(nearTarget.Guid);
                    // AmeisenLogger.Instance.Log("FuryWarri", $"Target: {nearTarget}");
                }
            }

        }

        public override void OutOfCombatExecute()
        {
            if (DateTime.Now > spellCoolDown[commandingShoutSpell] && WowInterface.ObjectManager.Player.Rage >= 10 && !WowInterface.ObjectManager.Player.HasBuffByName("Commanding Shout"))
            {
                WowInterface.HookManager.CastSpell(commandingShoutSpell);
                spellCoolDown[commandingShoutSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(commandingShoutSpell));
                return;
            }

            bool hasFlag = false;
            hasFlag = WowInterface.ObjectManager.Player.Auras != null && WowInterface.ObjectManager.Player.Auras.Any(e => e.SpellId == 23333 || e.SpellId == 23335);
            if (!hasFlag)
            {
                if (getonthemount.Run() && !WowInterface.ObjectManager.Player.HasBuffByName("Big Blizzard Bear") && WowInterface.HookManager.IsOutdoors())
                {
                    WowInterface.HookManager.StopClickToMoveIfActive();
                    WowInterface.MovementEngine.Reset();
                    WowInterface.HookManager.CastSpell("Big Blizzard Bear");
                }
                if (getonthemount.Run() && !WowInterface.ObjectManager.Player.HasBuffByName("Mechano-hog") && WowInterface.HookManager.IsOutdoors())
                {
                    WowInterface.HookManager.StopClickToMoveIfActive();
                    WowInterface.MovementEngine.Reset();
                    WowInterface.HookManager.CastSpell("Mechano-hog");
                }
            }

        }
    }
}
