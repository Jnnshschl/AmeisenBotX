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
    internal class PaladinProtection : BasicKamelClass
    {
        //Spell
        private const string avengersShieldSpell = "Avenger's Shield";
        private const string consecrationSpell = "Consecration";
        private const string judgementofLightSpell = "Judgement of Light";
        private const string holyShieldSpell = "Holy Shield";
        private const string hammeroftheRighteousSpell = "Hammer of the Righteous";
        private const string hammerofWrathSpell = "Hammer of Wrath";
        private const string exorcismSpell = "Exorcism";
        private const string divineProtectionSpell = "Divine Protection";
        private const string handofReckoningSpell = "Hand of Reckoning";
        private const string hammerofJusticeSpell = "Hammer of Justice";
        private const string layonHandsSpell = "Lay on Hands";
        private const string holyLightSpell = "Holy Light";
        private const string redemptionSpell = "Redemption";

        //Buff
        private const string blessingofKingsSpell = "Blessing of Kings";
        private const string sealofLightSpell = "Seal of Light";
        private const string devotionAuraSpell = "Devotion Aura";
        private const string righteousFurySpell = "Righteous Fury";

        private Dictionary<string, DateTime> spellCoolDown = new Dictionary<string, DateTime>();

        public PaladinProtection(WowInterface wowInterface) : base()
        {
            WowInterface = wowInterface;
            //Spell
            spellCoolDown.Add(avengersShieldSpell, DateTime.Now);
            spellCoolDown.Add(consecrationSpell, DateTime.Now);
            spellCoolDown.Add(judgementofLightSpell, DateTime.Now);
            spellCoolDown.Add(holyShieldSpell, DateTime.Now);
            spellCoolDown.Add(hammeroftheRighteousSpell, DateTime.Now);
            spellCoolDown.Add(hammerofWrathSpell, DateTime.Now);
            spellCoolDown.Add(exorcismSpell, DateTime.Now);
            spellCoolDown.Add(divineProtectionSpell, DateTime.Now);
            spellCoolDown.Add(handofReckoningSpell, DateTime.Now);
            spellCoolDown.Add(hammerofJusticeSpell, DateTime.Now);
            spellCoolDown.Add(layonHandsSpell, DateTime.Now);
            spellCoolDown.Add(holyLightSpell, DateTime.Now);
            spellCoolDown.Add(redemptionSpell, DateTime.Now);

            //Buff
            spellCoolDown.Add(blessingofKingsSpell, DateTime.Now);
            spellCoolDown.Add(sealofLightSpell, DateTime.Now);
            spellCoolDown.Add(devotionAuraSpell, DateTime.Now);
            spellCoolDown.Add(righteousFurySpell, DateTime.Now);

            //Time event
            revivePlayerEvent = new TimegatedEvent(TimeSpan.FromSeconds(4));
        }

        public override string Author => "Lukas";

        public override WowClass WowClass => WowClass.Paladin;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Paladin Protection";

        public override string Displayname => "Paladin Protection 1.0";

        public TimegatedEvent ExecuteEvent { get; private set; }

        public override bool HandlesMovement => false;

        public TimegatedEvent HeroicStrikeEvent { get; private set; }

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStaminaComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES, WeaponType.STAVES, WeaponType.DAGGERS });

        public override CombatClassRole Role => CombatClassRole.Tank;

        //Time event
        public TimegatedEvent revivePlayerEvent { get; private set; }

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>(),
            Tree2 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(2, 2, 5) },
                { 5, new Talent(2, 5, 5) },
                { 6, new Talent(2, 6, 1) },
                { 7, new Talent(2, 7, 3) },
                { 8, new Talent(2, 8, 5) },
                { 9, new Talent(2, 9, 2) },
                { 11, new Talent(2, 11, 3) },
                { 12, new Talent(2, 12, 1) },
                { 14, new Talent(2, 14, 2) },
                { 15, new Talent(2, 15, 3) },
                { 16, new Talent(2, 16, 1) },
                { 17, new Talent(2, 17, 1) },
                { 18, new Talent(2, 18, 3) },
                { 19, new Talent(2, 19, 3) },
                { 20, new Talent(2, 20, 3) },
                { 21, new Talent(2, 21, 3) },
                { 22, new Talent(2, 22, 1) },
                { 23, new Talent(2, 23, 2) },
                { 24, new Talent(2, 24, 3) },
                { 25, new Talent(2, 25, 2) },
                { 26, new Talent(2, 26, 1) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 5) },
                { 3, new Talent(3, 3, 2) },
                { 4, new Talent(3, 4, 3) },
                { 7, new Talent(3, 7, 5) },
                { 12, new Talent(3, 12, 3) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public TimegatedEvent VictoryRushEvent { get; private set; }

        public override bool WalkBehindEnemy => false;

        public override void ExecuteCC()
        {
            StartAttack();
        }

        public override void OutOfCombatExecute()
        {
            List<WowUnit> partyMemberToHeal = new List<WowUnit>(WowInterface.ObjectManager.Partymembers);
            partyMemberToHeal.Add(WowInterface.ObjectManager.Player);

            partyMemberToHeal = partyMemberToHeal.Where(e => e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

            if (revivePlayerEvent.Run() && partyMemberToHeal.Count > 0)
            {
                WowInterface.HookManager.TargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                WowInterface.HookManager.CastSpell(redemptionSpell);
            }

            BuffManager();
            Targetselection();
        }

        private bool CustomCastSpell(string spellName)
        {

            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(spellName))
            {
                if (WowInterface.ObjectManager.Target != null)
                {
                    double distance = WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position);
                    Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                    if ((WowInterface.ObjectManager.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        if ((spell.MinRange == 0 && spell.MaxRange == 0) || (spell.MinRange <= distance && spell.MaxRange >= distance))
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

        private void BuffManager()
        {
            if (TargetSelectEvent.Run())
            {
                List<WowUnit> CastBuff = new List<WowUnit>(WowInterface.ObjectManager.Partymembers);
                CastBuff.Add(WowInterface.ObjectManager.Player);

                CastBuff = CastBuff.Where(e => !e.HasBuffByName("Blessing of Kings") && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

                if (CastBuff != null)
                {
                    if (CastBuff.Count > 0)
                    {
                        if (WowInterface.ObjectManager.TargetGuid != CastBuff.FirstOrDefault().Guid)
                        {
                            WowInterface.HookManager.TargetGuid(CastBuff.FirstOrDefault().Guid);
                        }
                    }
                    if (WowInterface.ObjectManager.TargetGuid != 0 && WowInterface.ObjectManager.Target != null)
                    {
                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                        if (!WowInterface.ObjectManager.Target.HasBuffByName("Blessing of Kings") && CustomCastSpell(blessingofKingsSpell))
                        {
                            return;
                        }
                    }
                }
            }
            if (!WowInterface.ObjectManager.Player.HasBuffByName("Seal of Light") && CustomCastSpell(sealofLightSpell))
            {
                return;
            } 
            if (!WowInterface.ObjectManager.Player.HasBuffByName("Devotion Aura") && CustomCastSpell(devotionAuraSpell))
            {
                return;
            }   
            if (!WowInterface.ObjectManager.Player.HasBuffByName("Righteous Fury") && CustomCastSpell(righteousFurySpell))
            {
                return;
            } 
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

                    if (WowInterface.ObjectManager.Player.HealthPercentage <= 15 && CustomCastSpell(layonHandsSpell))
                    {
                        return;
                    }   
                    if (WowInterface.ObjectManager.Player.HealthPercentage <= 25 && CustomCastSpell(holyLightSpell))
                    {
                        return;
                    }   
                    if (WowInterface.ObjectManager.Player.HealthPercentage <= 50 && CustomCastSpell(divineProtectionSpell))
                    {
                        return;
                    }        
                    if (WowInterface.ObjectManager.Target.HealthPercentage <= 20 && CustomCastSpell(hammerofWrathSpell))
                    {
                        return;
                    }    
                    if ((WowInterface.ObjectManager.Target.HealthPercentage <= 20 || WowInterface.ObjectManager.Player.HealthPercentage <= 30 || WowInterface.ObjectManager.Target.IsCasting) && CustomCastSpell(hammerofJusticeSpell))
                    {
                        return;
                    }  
                    if (CustomCastSpell(handofReckoningSpell))
                    {
                        return;
                    }      
                    if (CustomCastSpell(avengersShieldSpell))
                    {
                        return;
                    }
                    if (CustomCastSpell(consecrationSpell))
                    {
                        return;
                    }
                    if (CustomCastSpell(judgementofLightSpell))
                    {
                        return;
                    }
                    if (CustomCastSpell(holyShieldSpell))
                    {
                        return;
                    }      
                    if (CustomCastSpell(exorcismSpell))
                    {
                        return;
                    }

                }
                else//Range
                {
                    if (CustomCastSpell(avengersShieldSpell))
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
                WowUnit nearTargetToTank = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 40)
                .Where(e => e.IsInCombat && !e.IsNotAttackable && e.TargetGuid != WowInterface.ObjectManager.PlayerGuid && e.Type != WowObjectType.Player && e.Name != "The Lich King" && !(WowInterface.ObjectManager.MapId == MapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346))
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .FirstOrDefault();//&& e.Type(Player)       

                if (nearTargetToTank != null)
                {
                    WowInterface.HookManager.TargetGuid(nearTargetToTank.Guid);

                    if (!TargetInLineOfSight)
                    {
                        return;
                    }
                }
                else
                {
                    WowUnit nearTarget = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 40)
                    .Where(e => e.IsInCombat && !e.IsNotAttackable && e.Name != "The Lich King" && !(WowInterface.ObjectManager.MapId == MapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346))
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                    .FirstOrDefault();//&& e.Type(Player)

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
        }
    }
}