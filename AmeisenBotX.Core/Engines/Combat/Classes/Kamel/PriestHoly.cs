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
    internal class PriestHoly : BasicKamelClass
    {
        private const string CircleOfHealingSpell = "Circle of Healing";

        private const string DesperatePrayerSpell = "Desperate Prayer";

        private const string DivineHymnSpell = "Divine Hymn";

        //Buffs
        private const string DivineSpiritSpell = "Divine Spirit";

        //Spells Race
        private const string EveryManforHimselfSpell = "Every Man for Himself";

        private const string FadeSpell = "Fade";

        private const string FearWardSpell = "Fear Ward";

        private const string FlashHealSpell = "Flash Heal";

        private const string GreaterHealSpell = "Greater Heal";

        private const string GuardianSpiritSpell = "Guardian Spirit";

        private const string HolyFireSpell = "Holy Fire";

        private const string HymnofHopeSpell = "Hymn of Hope";

        private const string InnerFireSpell = "Inner Fire";

        private const string PowerWordFortitudeSpell = "Power Word: Fortitude";

        private const string PowerWordShieldSpell = "Power Word: Shield";

        private const string PrayerofFortitude = "Prayer of Fortitude";

        private const string PrayerofHealingSpell = "Prayer of Healing";

        private const string PrayerofMendingSpell = "Prayer of Mending";

        private const string PrayerofShadowProtection = "Prayer of Shadow Protection";

        private const string RenewSpell = "Renew";

        private const string ShadowProtectionSpell = "Shadow Protection";

        //Spells / dmg
        private const string SmiteSpell = "Smite";

        public PriestHoly(AmeisenBotInterfaces bot) : base()
        {
            Bot = bot;

            //Spells Race
            //spellCoolDown.Add(EveryManforHimselfSpell, DateTime.Now);

            //Spells / dmg
            spellCoolDown.Add(SmiteSpell, DateTime.Now);
            spellCoolDown.Add(HolyFireSpell, DateTime.Now);

            //Spells
            spellCoolDown.Add(RenewSpell, DateTime.Now);
            spellCoolDown.Add(FlashHealSpell, DateTime.Now);
            spellCoolDown.Add(GreaterHealSpell, DateTime.Now);
            spellCoolDown.Add(PowerWordShieldSpell, DateTime.Now);
            spellCoolDown.Add(CircleOfHealingSpell, DateTime.Now);
            spellCoolDown.Add(DesperatePrayerSpell, DateTime.Now);
            spellCoolDown.Add(FadeSpell, DateTime.Now);
            spellCoolDown.Add(PrayerofHealingSpell, DateTime.Now);
            spellCoolDown.Add(PrayerofMendingSpell, DateTime.Now);
            spellCoolDown.Add(GuardianSpiritSpell, DateTime.Now);
            spellCoolDown.Add(HymnofHopeSpell, DateTime.Now);
            spellCoolDown.Add(DivineHymnSpell, DateTime.Now);

            //Buffs
            spellCoolDown.Add(DivineSpiritSpell, DateTime.Now);
            spellCoolDown.Add(InnerFireSpell, DateTime.Now);
            spellCoolDown.Add(FearWardSpell, DateTime.Now);
            spellCoolDown.Add(PowerWordFortitudeSpell, DateTime.Now);
            spellCoolDown.Add(ShadowProtectionSpell, DateTime.Now);
            spellCoolDown.Add(PrayerofFortitude, DateTime.Now);
            spellCoolDown.Add(PrayerofShadowProtection, DateTime.Now);
        }

        public override string Author => "Lukas";

        public override Dictionary<string, dynamic> C { get; set; } = [];

        public override string Description => "Priest Holy";

        public override string DisplayName => "Priest Holy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicSpiritComparator([WowArmorType.Shield], [WowWeaponType.Sword, WowWeaponType.Mace, WowWeaponType.Axe]);

        public override WowRole Role => WowRole.Heal;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = [],
            Tree2 = new()
            {
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 7, new(2, 7, 3) },
                { 8, new(2, 8, 1) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 5) },
                { 5, new(3, 5, 5) },
                { 6, new(3, 6, 3) },
                { 7, new(3, 7, 3) },
                { 8, new(3, 8, 1) },
                { 9, new(3, 9, 3) },
                { 10, new(3, 10, 3) },
                { 11, new(3, 11, 5) },
                { 12, new(3, 12, 3) },
                { 13, new(3, 13, 1) },
                { 15, new(3, 15, 5) },
                { 17, new(3, 17, 1) },
                { 19, new(3, 19, 2) },
                { 20, new(3, 20, 2) },
                { 21, new(3, 21, 3) },
                { 22, new(3, 22, 3) },
                { 23, new(3, 23, 1) },
                { 24, new(3, 24, 2) },
                { 25, new(3, 25, 5) },
                { 26, new(3, 26, 1) },
            },
        };

        public bool TargetIsInRange { get; set; }

        public override bool UseAutoAttacks => false;

        public bool UseSpellOnlyInCombat { get; private set; }

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Priest;

        public override void ExecuteCC()
        {
            UseSpellOnlyInCombat = true;
            BuffManager();
            StartHeal();
        }

        public override void OutOfCombatExecute()
        {
            RevivePartyMember(resurrectionSpell);
            UseSpellOnlyInCombat = false;
            BuffManager();
            StartHeal();
        }

        private void BuffManager()
        {
            if (TargetSelectEvent.Run())
            {
                List<IWowUnit> CastBuff = new(Bot.Objects.Partymembers)
                {
                    Bot.Player
                };

                CastBuff = CastBuff.Where(e => (!e.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Prayer of Fortitude") || !e.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Prayer of Shadow Protection")) && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

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
                        if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Prayer of Fortitude") && CustomCastSpell(PrayerofFortitude))
                        {
                            return;
                        }
                        if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Prayer of Shadow Protection") && CustomCastSpell(PrayerofShadowProtection))
                        {
                            return;
                        }
                    }
                }
            }
            //if ((!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Power Word: Fortitude") || !Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Power Word: Fortitude")) && CustomCastSpell(PowerWordFortitudeSpell))
            //{
            //    return;
            //}
            if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Divine Spirit") && CustomCastSpell(DivineSpiritSpell, true))
            {
                return;
            }
            if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Inner Fire") && CustomCastSpell(InnerFireSpell, true))
            {
                return;
            }
            if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Fear Ward") && CustomCastSpell(FearWardSpell, true))
            {
                return;
            }
            //if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Shadow Protection") && CustomCastSpell(ShadowProtectionSpell))
            //{
            //    return;
            //}
        }

        private bool CustomCastSpell(string spellName, bool castOnSelf = false)
        {
            if (Bot.Character.SpellBook.IsSpellKnown(spellName))
            {
                if (Bot.Target != null)
                {
                    Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                    if ((Bot.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        double distance = Bot.Player.Position.GetDistance(Bot.Target.Position);

                        if ((spell.MinRange == 0 && spell.MaxRange == 0) || (spell.MinRange <= distance && spell.MaxRange >= distance))
                        {
                            Bot.Wow.CastSpell(spellName, castOnSelf);
                            return true;
                        }
                    }
                }
                else
                {
                    Bot.Wow.ChangeTarget(Bot.Wow.PlayerGuid);

                    Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                    if ((Bot.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        Bot.Wow.CastSpell(spellName);
                        return true;
                    }
                }
            }

            return false;
        }

        private void StartHeal()
        {
            List<IWowUnit> partyMemberToHeal = new(Bot.Objects.Partymembers)
            {
                //healableUnits.AddRange(Bot.ObjectManager.PartyPets);
                Bot.Player
            };

            partyMemberToHeal = partyMemberToHeal.Where(e => e.HealthPercentage <= 94 && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

            if (partyMemberToHeal.Count > 0)
            {
                if (Bot.Wow.TargetGuid != partyMemberToHeal.FirstOrDefault().Guid)
                {
                    Bot.Wow.ChangeTarget(partyMemberToHeal.FirstOrDefault().Guid);
                }

                if (Bot.Wow.TargetGuid != 0 && Bot.Target != null)
                {
                    TargetIsInRange = Bot.Player.Position.GetDistance(Bot.GetWowObjectByGuid<IWowUnit>(partyMemberToHeal.FirstOrDefault().Guid).Position) <= 30;
                    if (TargetIsInRange)
                    {
                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                        if (Bot.Movement.Status != Movement.Enums.MovementAction.None)
                        {
                            Bot.Wow.StopClickToMove();
                            Bot.Movement.Reset();
                        }

                        if (Bot.Target != null && Bot.Target.HealthPercentage >= 90)
                        {
                            Bot.Wow.LuaDoString("SpellStopCasting()");
                            return;
                        }

                        if (UseSpellOnlyInCombat && (Bot.Player.IsConfused || Bot.Player.IsSilenced || Bot.Player.IsDazed) && CustomCastSpell(EveryManforHimselfSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && Bot.Player.ManaPercentage <= 20 && CustomCastSpell(HymnofHopeSpell))
                        {
                            return;
                        }

                        if (partyMemberToHeal.Count >= 5 && Bot.Target.HealthPercentage < 50 && CustomCastSpell(DivineHymnSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && Bot.Player.HealthPercentage < 50 && CustomCastSpell(FadeSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && Bot.Target.HealthPercentage < 30 && CustomCastSpell(GuardianSpiritSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && Bot.Target.HealthPercentage < 30 && CustomCastSpell(DesperatePrayerSpell))
                        {
                            return;
                        }

                        if (Bot.Target.HealthPercentage < 55 && CustomCastSpell(GreaterHealSpell))
                        {
                            return;
                        }

                        if (Bot.Target.HealthPercentage < 80 && CustomCastSpell(FlashHealSpell))
                        {
                            return;
                        }

                        if (partyMemberToHeal.Count >= 3 && Bot.Target.HealthPercentage < 80 && CustomCastSpell(CircleOfHealingSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && partyMemberToHeal.Count >= 2 && Bot.Target.HealthPercentage < 80 && CustomCastSpell(PrayerofMendingSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && partyMemberToHeal.Count >= 3 && Bot.Target.HealthPercentage < 80 && CustomCastSpell(PrayerofHealingSpell))
                        {
                            return;
                        }

                        if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Renew") && Bot.Target.HealthPercentage < 90 && CustomCastSpell(RenewSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && !Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Weakened Soul") && !Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Power Word: Shield") && Bot.Target.HealthPercentage < 90 && CustomCastSpell(PowerWordShieldSpell))
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
                    IWowUnit nearTarget = Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, 30)
                    .Where(e => e.IsInCombat && !e.IsNotAttackable && Bot.Db.GetUnitName(e, out string name) && name != "The Lich King" && !(Bot.Objects.MapId == WowMapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346))//&& e.IsCasting
                    .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                    .FirstOrDefault();

                    if (Bot.Wow.TargetGuid != 0 && Bot.Target != null && nearTarget != null)
                    {
                        Bot.Wow.ChangeTarget(nearTarget.Guid);

                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                        if (Bot.Movement.Status != Movement.Enums.MovementAction.None)
                        {
                            Bot.Wow.StopClickToMove();
                            Bot.Movement.Reset();
                        }
                        if (UseSpellOnlyInCombat && Bot.Player.ManaPercentage >= 80 && CustomCastSpell(HolyFireSpell))
                        {
                            return;
                        }
                        if (UseSpellOnlyInCombat && Bot.Player.ManaPercentage >= 80 && CustomCastSpell(SmiteSpell))
                        {
                            return;
                        }
                    }
                }
                //target gui id is bigger than null
                //{
                //Bot.NewBot.ClearTarget();
                //return;
                //}
                //Attacken
            }
        }
    }
}