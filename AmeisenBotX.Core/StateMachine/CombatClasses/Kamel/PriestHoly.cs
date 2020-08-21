using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Kamel
{
    internal class PriestHoly : BasicKamelClass
    {
        //Spells / dmg
        private const string SmiteSpell = "Smite";
        private const string HolyFireSpell = "Holy Fire";

        //Spells / heal
        private const string ResurrectionSpell = "Resurrection";
        private const string RenewSpell = "Renew";
        private const string FlashHealSpell = "Flash Heal";
        private const string GreaterHealSpell = "Greater Heal";
        private const string PowerWordShieldSpell = "Power Word: Shield";
        private const string CircleOfHealingSpell = "Circle of Healing";
        private const string DesperatePrayerSpell = "Desperate Prayer";
        private const string FadeSpell = "Fade";

        //Buffs
        private const string DivineSpiritSpell = "Divine Spirit";
        private const string InnerFireSpell = "Inner Fire";
        private const string FearWardSpell = "Fear Ward";
        private const string PowerWordFortitudeSpell = "Power Word: Fortitude";
        private const string ShadowProtectionSpell = "Shadow Protection";

        private Dictionary<string, DateTime> spellCoolDown = new Dictionary<string, DateTime>();

        public PriestHoly(WowInterface wowInterface) : base()
        {
            WowInterface = wowInterface;

            //Spells / dmg
            spellCoolDown.Add(SmiteSpell, DateTime.Now);
            spellCoolDown.Add(HolyFireSpell, DateTime.Now);

            //Spells
            spellCoolDown.Add(ResurrectionSpell, DateTime.Now);
            spellCoolDown.Add(RenewSpell, DateTime.Now);
            spellCoolDown.Add(FlashHealSpell, DateTime.Now);
            spellCoolDown.Add(GreaterHealSpell, DateTime.Now);
            spellCoolDown.Add(PowerWordShieldSpell, DateTime.Now);
            spellCoolDown.Add(CircleOfHealingSpell, DateTime.Now);
            spellCoolDown.Add(DesperatePrayerSpell, DateTime.Now);
            spellCoolDown.Add(FadeSpell, DateTime.Now);

            //Buffs
            spellCoolDown.Add(DivineSpiritSpell, DateTime.Now);
            spellCoolDown.Add(InnerFireSpell, DateTime.Now);
            spellCoolDown.Add(FearWardSpell, DateTime.Now);
            spellCoolDown.Add(PowerWordFortitudeSpell, DateTime.Now);
            spellCoolDown.Add(ShadowProtectionSpell, DateTime.Now);


            //Time event
            revivePlayerEvent = new TimegatedEvent(TimeSpan.FromSeconds(4));
        }

        public override string Author => "Lukas";

        public override WowClass WowClass => WowClass.Priest;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Priest Holy";

        public override string Displayname => "Priest Holy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;
        public bool UseSpellOnlyInCombat { get; private set; }

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Heal;

        //Time event

        public TimegatedEvent revivePlayerEvent { get; private set; }

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>(),
            Tree2 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 7, new Talent(2, 7, 3) },
                { 8, new Talent(2, 8, 1) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 5) },
                { 5, new Talent(3, 5, 5) },
                { 6, new Talent(3, 6, 3) },
                { 7, new Talent(3, 7, 3) },
                { 8, new Talent(3, 8, 1) },
                { 9, new Talent(3, 9, 3) },
                { 10, new Talent(3, 10, 3) },
                { 11, new Talent(3, 11, 5) },
                { 12, new Talent(3, 12, 3) },
                { 13, new Talent(3, 13, 1) },
                { 15, new Talent(3, 15, 5) },
                { 17, new Talent(3, 17, 1) },
                { 19, new Talent(3, 19, 2) },
                { 20, new Talent(3, 20, 2) },
                { 21, new Talent(3, 21, 3) },
                { 22, new Talent(3, 22, 3) },
                { 23, new Talent(3, 23, 1) },
                { 24, new Talent(3, 24, 2) },
                { 25, new Talent(3, 25, 5) },
                { 26, new Talent(3, 26, 1) },
            },
        };

        public bool targetIsInRange { get; set; }

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override void ExecuteCC()
        {
            UseSpellOnlyInCombat = true;
            BuffMe();
            StartHeal();
        }

        public override void OutOfCombatExecute()
        {
            List<WowUnit> partyMemberToHeal = new List<WowUnit>(WowInterface.ObjectManager.Partymembers);
            partyMemberToHeal.Add(WowInterface.ObjectManager.Player);

            partyMemberToHeal = partyMemberToHeal.Where(e => e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

            if (revivePlayerEvent.Run() && partyMemberToHeal.Count > 0)
            {
                WowInterface.HookManager.TargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                WowInterface.HookManager.CastSpell(ResurrectionSpell);
            }

            UseSpellOnlyInCombat = false;
            BuffMe();
            StartHeal();
        }

        private bool CustomCastSpell(string spellName, bool castOnSelf = false)
        {
            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(spellName))
            {
                if (WowInterface.ObjectManager.Target != null)
                {
                    Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                    if ((WowInterface.ObjectManager.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        double distance = WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position);

                        if ((spell.MinRange == 0 && spell.MaxRange == 0) || (spell.MinRange <= distance && spell.MaxRange >= distance))
                        {
                            WowInterface.HookManager.CastSpell(spellName);
                            return true;
                        }
                    }
                }
                else
                {
                    WowInterface.HookManager.TargetGuid(WowInterface.ObjectManager.PlayerGuid);

                    Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                    if ((WowInterface.ObjectManager.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        WowInterface.HookManager.CastSpell(spellName);
                        return true;
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

        private void BuffMe()
        {
            //if ((!WowInterface.ObjectManager.Player.HasBuffByName("Power Word: Fortitude") || !WowInterface.ObjectManager.Target.HasBuffByName("Power Word: Fortitude")) && CustomCastSpell(PowerWordFortitudeSpell))
            //{
            //    return;
            //} 
            //if (!WowInterface.ObjectManager.Player.HasBuffByName("Divine Spirit") && CustomCastSpell(DivineSpiritSpell))
            //{
            //    return;
            //}  
            //if (!WowInterface.ObjectManager.Player.HasBuffByName("Inner Fire") && CustomCastSpell(InnerFireSpell))
            //{
            //    return;
            //} 
            //if (!WowInterface.ObjectManager.Player.HasBuffByName("Fear Ward") && CustomCastSpell(FearWardSpell))
            //{
            //    return;
            //}   
            //if (!WowInterface.ObjectManager.Player.HasBuffByName("Shadow Protection") && CustomCastSpell(ShadowProtectionSpell))
            //{
            //    return;
            //}
        }

        private void StartHeal()
        {

            List<WowUnit> partyMemberToHeal = new List<WowUnit>(WowInterface.ObjectManager.Partymembers);
            //healableUnits.AddRange(WowInterface.ObjectManager.PartyPets);
            partyMemberToHeal.Add(WowInterface.ObjectManager.Player);

            partyMemberToHeal = partyMemberToHeal.Where(e => e.HealthPercentage <= 94 && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

            if (partyMemberToHeal.Count > 0)
            {
                if (WowInterface.ObjectManager.TargetGuid != partyMemberToHeal.FirstOrDefault().Guid)
                {
                    WowInterface.HookManager.TargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                }

                if (WowInterface.ObjectManager.Target != null && WowInterface.ObjectManager.Target.HealthPercentage >= 90)
                {
                    WowInterface.HookManager.LuaDoString("SpellStopCasting()");
                    return;
                }


                if (WowInterface.ObjectManager.Target != null)
                {
                    targetIsInRange = WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(partyMemberToHeal.FirstOrDefault().Guid).Position) <= 30;
                    if (targetIsInRange)
                    {
                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                        if (WowInterface.MovementEngine.MovementAction != Movement.Enums.MovementAction.None)
                        {
                            WowInterface.HookManager.StopClickToMoveIfActive();
                            WowInterface.MovementEngine.Reset();
                        }

                        if (UseSpellOnlyInCombat && WowInterface.ObjectManager.Player.HealthPercentage < 50 && CustomCastSpell(FadeSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && WowInterface.ObjectManager.Target.HealthPercentage < 30 && CustomCastSpell(DesperatePrayerSpell))
                        {
                            return;
                        }

                        if (WowInterface.ObjectManager.Target.HealthPercentage < 55 && CustomCastSpell(GreaterHealSpell))
                        {
                            return;
                        }

                        if (WowInterface.ObjectManager.Target.HealthPercentage < 80 && CustomCastSpell(FlashHealSpell))
                        {
                            return;
                        }

                        if (partyMemberToHeal.Count >= 3 && WowInterface.ObjectManager.Target.HealthPercentage < 80 && CustomCastSpell(CircleOfHealingSpell))
                        {
                            return;
                        }

                        if (!WowInterface.ObjectManager.Target.HasBuffByName("Renew") && WowInterface.ObjectManager.Target.HealthPercentage < 90 && CustomCastSpell(RenewSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && !WowInterface.ObjectManager.Target.HasBuffByName("Weakened Soul") && !WowInterface.ObjectManager.Target.HasBuffByName("Power Word: Shield") && WowInterface.ObjectManager.Target.HealthPercentage < 90 && CustomCastSpell(PowerWordShieldSpell))
                        {
                            return;
                        }
                    }

                }
            }
            else
            {

                if (TargetSelectEvent.Run())
                {
                    WowUnit nearTarget = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 30)
                    .Where(e => e.IsInCombat && !e.IsNotAttackable && e.Name != "The Lich King" && !(WowInterface.ObjectManager.MapId == MapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346))//&& e.IsCasting 
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                    .FirstOrDefault();

                    if (WowInterface.ObjectManager.TargetGuid != 0 && WowInterface.ObjectManager.Target != null && nearTarget != null)
                    {
                        WowInterface.HookManager.TargetGuid(nearTarget.Guid);

                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                        if (WowInterface.MovementEngine.MovementAction != Movement.Enums.MovementAction.None)
                        {
                            WowInterface.HookManager.StopClickToMoveIfActive();
                            WowInterface.MovementEngine.Reset();
                        }
                        if (UseSpellOnlyInCombat && WowInterface.ObjectManager.Player.ManaPercentage >= 80 && CustomCastSpell(HolyFireSpell))
                        {
                            return;
                        }
                        if (UseSpellOnlyInCombat && WowInterface.ObjectManager.Player.ManaPercentage >= 80 && CustomCastSpell(SmiteSpell))
                        {
                            return;
                        }
                    }
                }
                //target gui id is bigger than null
                //{
                //WowInterface.HookManager.ClearTarget();
                //return;
                //}
                //Attacken
            }
        }
    }
}
