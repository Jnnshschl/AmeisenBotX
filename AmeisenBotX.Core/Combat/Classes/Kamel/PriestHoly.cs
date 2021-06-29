using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Kamel
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

        public PriestHoly(WowInterface wowInterface) : base()
        {
            WowInterface = wowInterface;

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

        public override Dictionary<string, dynamic> C { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Priest Holy";

        public override string Displayname => "Priest Holy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public override WowRole Role => WowRole.Heal;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new(),
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

        public bool targetIsInRange { get; set; }

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
            revivePartyMember(resurrectionSpell);
            UseSpellOnlyInCombat = false;
            BuffManager();
            StartHeal();
        }

        private void BuffManager()
        {
            if (TargetSelectEvent.Run())
            {
                List<WowUnit> CastBuff = new List<WowUnit>(WowInterface.Objects.Partymembers)
                {
                    WowInterface.Player
                };

                CastBuff = CastBuff.Where(e => (!e.HasBuffByName("Prayer of Fortitude") || !e.HasBuffByName("Prayer of Shadow Protection")) && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

                if (CastBuff != null)
                {
                    if (CastBuff.Count > 0)
                    {
                        if (WowInterface.Target.Guid != CastBuff.FirstOrDefault().Guid)
                        {
                            WowInterface.NewWowInterface.WowTargetGuid(CastBuff.FirstOrDefault().Guid);
                        }
                    }
                    if (WowInterface.Target.Guid != 0 && WowInterface.Target != null)
                    {
                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                        if (!WowInterface.Target.HasBuffByName("Prayer of Fortitude") && CustomCastSpell(PrayerofFortitude))
                        {
                            return;
                        }
                        if (!WowInterface.Target.HasBuffByName("Prayer of Shadow Protection") && CustomCastSpell(PrayerofShadowProtection))
                        {
                            return;
                        }
                    }
                }
            }
            //if ((!WowInterface.Player.HasBuffByName("Power Word: Fortitude") || !WowInterface.Target.HasBuffByName("Power Word: Fortitude")) && CustomCastSpell(PowerWordFortitudeSpell))
            //{
            //    return;
            //}
            if (!WowInterface.Player.HasBuffByName("Divine Spirit") && CustomCastSpell(DivineSpiritSpell, true))
            {
                return;
            }
            if (!WowInterface.Player.HasBuffByName("Inner Fire") && CustomCastSpell(InnerFireSpell, true))
            {
                return;
            }
            if (!WowInterface.Player.HasBuffByName("Fear Ward") && CustomCastSpell(FearWardSpell, true))
            {
                return;
            }
            //if (!WowInterface.Player.HasBuffByName("Shadow Protection") && CustomCastSpell(ShadowProtectionSpell))
            //{
            //    return;
            //}
        }

        private bool CustomCastSpell(string spellName, bool castOnSelf = false)
        {
            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(spellName))
            {
                if (WowInterface.Target != null)
                {
                    Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                    if ((WowInterface.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        double distance = WowInterface.Player.Position.GetDistance(WowInterface.Target.Position);

                        if ((spell.MinRange == 0 && spell.MaxRange == 0) || (spell.MinRange <= distance && spell.MaxRange >= distance))
                        {
                            WowInterface.NewWowInterface.LuaCastSpell(spellName, castOnSelf);
                            return true;
                        }
                    }
                }
                else
                {
                    WowInterface.NewWowInterface.WowTargetGuid(WowInterface.Player.Guid);

                    Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                    if ((WowInterface.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        WowInterface.NewWowInterface.LuaCastSpell(spellName);
                        return true;
                    }
                }
            }

            return false;
        }

        private void StartHeal()
        {
            List<WowUnit> partyMemberToHeal = new List<WowUnit>(WowInterface.Objects.Partymembers)
            {
                //healableUnits.AddRange(WowInterface.ObjectManager.PartyPets);
                WowInterface.Player
            };

            partyMemberToHeal = partyMemberToHeal.Where(e => e.HealthPercentage <= 94 && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

            if (partyMemberToHeal.Count > 0)
            {
                if (WowInterface.Target.Guid != partyMemberToHeal.FirstOrDefault().Guid)
                {
                    WowInterface.NewWowInterface.WowTargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                }

                if (WowInterface.Target.Guid != 0 && WowInterface.Target != null)
                {
                    targetIsInRange = WowInterface.Player.Position.GetDistance(WowInterface.Objects.GetWowObjectByGuid<WowUnit>(partyMemberToHeal.FirstOrDefault().Guid).Position) <= 30;
                    if (targetIsInRange)
                    {
                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                        if (WowInterface.MovementEngine.Status != Movement.Enums.MovementAction.None)
                        {
                            WowInterface.NewWowInterface.WowStopClickToMove();
                            WowInterface.MovementEngine.Reset();
                        }

                        if (WowInterface.Target != null && WowInterface.Target.HealthPercentage >= 90)
                        {
                            WowInterface.NewWowInterface.LuaDoString("SpellStopCasting()");
                            return;
                        }

                        if (UseSpellOnlyInCombat && (WowInterface.Player.IsConfused || WowInterface.Player.IsSilenced || WowInterface.Player.IsDazed) && CustomCastSpell(EveryManforHimselfSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && WowInterface.Player.ManaPercentage <= 20 && CustomCastSpell(HymnofHopeSpell))
                        {
                            return;
                        }

                        if (partyMemberToHeal.Count >= 5 && WowInterface.Target.HealthPercentage < 50 && CustomCastSpell(DivineHymnSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && WowInterface.Player.HealthPercentage < 50 && CustomCastSpell(FadeSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && WowInterface.Target.HealthPercentage < 30 && CustomCastSpell(GuardianSpiritSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && WowInterface.Target.HealthPercentage < 30 && CustomCastSpell(DesperatePrayerSpell))
                        {
                            return;
                        }

                        if (WowInterface.Target.HealthPercentage < 55 && CustomCastSpell(GreaterHealSpell))
                        {
                            return;
                        }

                        if (WowInterface.Target.HealthPercentage < 80 && CustomCastSpell(FlashHealSpell))
                        {
                            return;
                        }

                        if (partyMemberToHeal.Count >= 3 && WowInterface.Target.HealthPercentage < 80 && CustomCastSpell(CircleOfHealingSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && partyMemberToHeal.Count >= 2 && WowInterface.Target.HealthPercentage < 80 && CustomCastSpell(PrayerofMendingSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && partyMemberToHeal.Count >= 3 && WowInterface.Target.HealthPercentage < 80 && CustomCastSpell(PrayerofHealingSpell))
                        {
                            return;
                        }

                        if (!WowInterface.Target.HasBuffByName("Renew") && WowInterface.Target.HealthPercentage < 90 && CustomCastSpell(RenewSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && !WowInterface.Target.HasBuffByName("Weakened Soul") && !WowInterface.Target.HasBuffByName("Power Word: Shield") && WowInterface.Target.HealthPercentage < 90 && CustomCastSpell(PowerWordShieldSpell))
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
                    WowUnit nearTarget = WowInterface.Objects.GetNearEnemies<WowUnit>(WowInterface.Db.GetReaction, WowInterface.Player.Position, 30)
                    .Where(e => e.IsInCombat && !e.IsNotAttackable && WowInterface.Db.GetUnitName(e, out string name) && name != "The Lich King" && !(WowInterface.Objects.MapId == WowMapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346))//&& e.IsCasting
                    .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                    .FirstOrDefault();

                    if (WowInterface.Target.Guid != 0 && WowInterface.Target != null && nearTarget != null)
                    {
                        WowInterface.NewWowInterface.WowTargetGuid(nearTarget.Guid);

                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                        if (WowInterface.MovementEngine.Status != Movement.Enums.MovementAction.None)
                        {
                            WowInterface.NewWowInterface.WowStopClickToMove();
                            WowInterface.MovementEngine.Reset();
                        }
                        if (UseSpellOnlyInCombat && WowInterface.Player.ManaPercentage >= 80 && CustomCastSpell(HolyFireSpell))
                        {
                            return;
                        }
                        if (UseSpellOnlyInCombat && WowInterface.Player.ManaPercentage >= 80 && CustomCastSpell(SmiteSpell))
                        {
                            return;
                        }
                    }
                }
                //target gui id is bigger than null
                //{
                //WowInterface.NewWowInterface.ClearTarget();
                //return;
                //}
                //Attacken
            }
        }
    }
}