using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Spells.Objects;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Kamel
{
    internal class PaladinProtection : BasicKamelClass
    {
        //Spell
        private const string avengersShieldSpell = "Avenger's Shield";

        private const string AvengingWrathSpell = "Avenging Wrath";

        //Buff
        private const string blessingofKingsSpell = "Blessing of Kings";

        private const string consecrationSpell = "Consecration";

        private const string devotionAuraSpell = "Devotion Aura";

        private const string DivinePleaSpell = "Divine Plea";

        private const string divineProtectionSpell = "Divine Protection";

        //Spells Race
        private const string EveryManforHimselfSpell = "Every Man for Himself";

        private const string exorcismSpell = "Exorcism";
        private const string hammerofJusticeSpell = "Hammer of Justice";
        private const string hammeroftheRighteousSpell = "Hammer of the Righteous";
        private const string hammerofWrathSpell = "Hammer of Wrath";
        private const string handofReckoningSpell = "Hand of Reckoning";
        private const string holyLightSpell = "Holy Light";
        private const string holyShieldSpell = "Holy Shield";
        private const string judgementofLightSpell = "Judgement of Light";
        private const string layonHandsSpell = "Lay on Hands";
        private const string righteousFurySpell = "Righteous Fury";
        private const string SacredShieldSpell = "Sacred Shield";
        private const string sealofLightSpell = "Seal of Light";
        private const string sealofWisdomSpell = "Seal of Wisdom";

        public PaladinProtection(AmeisenBotInterfaces bot) : base()
        {
            Bot = bot;

            //Spells Race
            //spellCoolDown.Add(EveryManforHimselfSpell, DateTime.Now);

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
            spellCoolDown.Add(AvengingWrathSpell, DateTime.Now);
            spellCoolDown.Add(DivinePleaSpell, DateTime.Now);
            spellCoolDown.Add(SacredShieldSpell, DateTime.Now);

            //Buff
            spellCoolDown.Add(blessingofKingsSpell, DateTime.Now);
            spellCoolDown.Add(sealofLightSpell, DateTime.Now);
            spellCoolDown.Add(sealofWisdomSpell, DateTime.Now);
            spellCoolDown.Add(devotionAuraSpell, DateTime.Now);
            spellCoolDown.Add(righteousFurySpell, DateTime.Now);

            //Time event
            ShieldEvent = new(TimeSpan.FromSeconds(8));
        }

        public override string Author => "Lukas";

        public override Dictionary<string, dynamic> C { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Paladin Protection 1.0";

        public override string DisplayName => "Paladin Protection";

        public TimegatedEvent ExecuteEvent { get; private set; }

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStaminaComparator(new() { WowArmorType.Shield }, new() { WowWeaponType.Sword, WowWeaponType.Mace, WowWeaponType.Axe, WowWeaponType.Staff, WowWeaponType.Dagger });

        public override WowRole Role => WowRole.Tank;

        public TimegatedEvent ShieldEvent { get; private set; }

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new(),
            Tree2 = new()
            {
                { 2, new(2, 2, 5) },
                { 5, new(2, 5, 5) },
                { 6, new(2, 6, 1) },
                { 7, new(2, 7, 3) },
                { 8, new(2, 8, 5) },
                { 9, new(2, 9, 2) },
                { 11, new(2, 11, 3) },
                { 12, new(2, 12, 1) },
                { 14, new(2, 14, 2) },
                { 15, new(2, 15, 3) },
                { 16, new(2, 16, 1) },
                { 17, new(2, 17, 1) },
                { 18, new(2, 18, 3) },
                { 19, new(2, 19, 3) },
                { 20, new(2, 20, 3) },
                { 21, new(2, 21, 3) },
                { 22, new(2, 22, 1) },
                { 23, new(2, 23, 2) },
                { 24, new(2, 24, 3) },
                { 25, new(2, 25, 2) },
                { 26, new(2, 26, 1) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 5) },
                { 3, new(3, 3, 2) },
                { 4, new(3, 4, 3) },
                { 7, new(3, 7, 5) },
                { 12, new(3, 12, 3) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Paladin;

        public override void ExecuteCC()
        {
            StartAttack();
        }

        public override void OutOfCombatExecute()
        {
            RevivePartyMember(redemptionSpell);
            BuffManager();
            TargetselectionTank();
            StartAttack();
        }

        private void BuffManager()
        {
            if (TargetSelectEvent.Run())
            {
                List<IWowUnit> CastBuff = new(Bot.Objects.Partymembers)
                {
                    Bot.Player
                };

                CastBuff = CastBuff.Where(e => !e.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Blessing of Kings") && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

                if (CastBuff != null)
                {
                    if (CastBuff.Count > 0)
                    {
                        if (Bot.Wow.TargetGuid != CastBuff.FirstOrDefault().Guid)
                        {
                            Bot.Wow.ChangeTarget(CastBuff.FirstOrDefault().Guid);
                        }
                    }
                    if (Bot.Wow.TargetGuid != 0 && Bot.Target != null)
                    {
                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                        if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Blessing of Kings") && CustomCastSpell(blessingofKingsSpell))
                        {
                            return;
                        }
                    }
                }
            }
            if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Seal of Wisdom") && CustomCastSpell(sealofWisdomSpell))
            {
                return;
            }
            if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Devotion Aura") && CustomCastSpell(devotionAuraSpell))
            {
                return;
            }
            if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Righteous Fury") && CustomCastSpell(righteousFurySpell))
            {
                return;
            }
        }

        private bool CustomCastSpell(string spellName)
        {
            if (Bot.Character.SpellBook.IsSpellKnown(spellName))
            {
                if (Bot.Target != null)
                {
                    double distance = Bot.Player.Position.GetDistance(Bot.Target.Position);
                    Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                    if ((Bot.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        if ((spell.MinRange == 0 && spell.MaxRange == 0) || (spell.MinRange <= distance && spell.MaxRange >= distance))
                        {
                            Bot.Wow.CastSpell(spellName);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void StartAttack()
        {
            // IWowUnit wowUnit = Bot.ObjectManager.GetClosestWowUnitByDisplayId(AnubRhekanDisplayId, false);

            if (Bot.Wow.TargetGuid != 0)
            {
                if (Bot.Wow.TargetGuid != Bot.Wow.PlayerGuid)
                {
                    TargetselectionTank();
                }

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

                    if ((Bot.Player.IsConfused || Bot.Player.IsSilenced || Bot.Player.IsDazed) && CustomCastSpell(EveryManforHimselfSpell))
                    {
                        return;
                    }

                    if (CustomCastSpell(AvengingWrathSpell))
                    {
                        return;
                    }

                    if (Bot.Player.ManaPercentage <= 20 && CustomCastSpell(DivinePleaSpell))
                    {
                        return;
                    }

                    if (ShieldEvent.Run() && CustomCastSpell(SacredShieldSpell))
                    {
                        return;
                    }

                    if (Bot.Player.HealthPercentage <= 15 && CustomCastSpell(layonHandsSpell))
                    {
                        return;
                    }
                    if (Bot.Player.HealthPercentage <= 25 && CustomCastSpell(holyLightSpell))
                    {
                        return;
                    }
                    if (Bot.Player.HealthPercentage <= 50 && CustomCastSpell(divineProtectionSpell))
                    {
                        return;
                    }
                    if (Bot.Target.HealthPercentage <= 20 && CustomCastSpell(hammerofWrathSpell))
                    {
                        return;
                    }
                    if ((Bot.Target.HealthPercentage <= 20 || Bot.Player.HealthPercentage <= 30 || Bot.Target.IsCasting) && CustomCastSpell(hammerofJusticeSpell))
                    {
                        return;
                    }
                    if (Bot.Db.GetUnitName(Bot.Target, out string name) && name != "Anub'Rekhan" && CustomCastSpell(handofReckoningSpell))
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
                TargetselectionTank();
            }
        }
    }
}