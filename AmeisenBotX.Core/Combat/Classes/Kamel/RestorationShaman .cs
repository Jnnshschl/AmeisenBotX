using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Data.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Kamel
{
    internal class RestorationShaman : BasicKamelClass
    {

        private const string Bloodlust = "Bloodlust";

        private const string CalloftheElementsSpell = "Call of the Elements";

        private const string chainHealSpell = "Chain Heal";

        private const string earthlivingBuff = "Earthliving ";

        private const string earthlivingWeaponSpell = "Earthliving Weapon";

        private const string earthShieldSpell = "Earth Shield";

        private const string earthShockSpell = "Earth Shock";

        private const string flameShockSpell = "Flame Shock";

        //Race (Draenei)
        private const string giftOfTheNaaruSpell = "Gift of the Naaru";

        //Spells
        private const string healingWaveSpell = "Healing Wave";

        private const string heroismSpell = "Heroism";
        private const string lesserHealingWaveSpell = "Lesser Healing Wave";

        //Spells / DMG
        private const string LightningBoltSpell = "Lightning Bolt";

        private const string LightningShieldSpell = "Lightning Shield";
        private const string ManaSpringTotemSpell = "Mana Spring Totem";
        private const string ManaTideTotemSpell = "Mana Tide Totem";

        //CD|Buffs
        private const string naturesswiftSpell = "Nature's Swiftness";

        private const string riptideSpell = "Riptide";
        private const string StrengthofEarthTotemSpell = "Strength of Earth Totem";
        private const string tidalForceSpell = "Tidal Force";
        private const string watershieldSpell = "Water shield";

        //Totem
        private const string WindfuryTotemSpell = "Windfury Totem";

        private const string windShearSpell = "Wind Shear";

        public RestorationShaman(WowInterface wowInterface) : base()
        {
            WowInterface = wowInterface;

            //Race
            //spellCoolDown.Add(giftOfTheNaaruSpell, DateTime.Now);

            //Spells / DMG
            spellCoolDown.Add(LightningBoltSpell, DateTime.Now);
            spellCoolDown.Add(flameShockSpell, DateTime.Now);
            spellCoolDown.Add(earthShockSpell, DateTime.Now);

            //Spells
            spellCoolDown.Add(healingWaveSpell, DateTime.Now);
            spellCoolDown.Add(lesserHealingWaveSpell, DateTime.Now);
            spellCoolDown.Add(riptideSpell, DateTime.Now);
            spellCoolDown.Add(watershieldSpell, DateTime.Now);
            spellCoolDown.Add(LightningShieldSpell, DateTime.Now);
            spellCoolDown.Add(chainHealSpell, DateTime.Now);
            spellCoolDown.Add(earthlivingBuff, DateTime.Now);
            spellCoolDown.Add(earthlivingWeaponSpell, DateTime.Now);
            spellCoolDown.Add(windShearSpell, DateTime.Now);

            //CD|Buffs
            spellCoolDown.Add(naturesswiftSpell, DateTime.Now);
            spellCoolDown.Add(heroismSpell, DateTime.Now);
            spellCoolDown.Add(Bloodlust, DateTime.Now);
            spellCoolDown.Add(tidalForceSpell, DateTime.Now);
            spellCoolDown.Add(earthShieldSpell, DateTime.Now);

            //Totem
            spellCoolDown.Add(WindfuryTotemSpell, DateTime.Now);
            spellCoolDown.Add(StrengthofEarthTotemSpell, DateTime.Now);
            spellCoolDown.Add(ManaSpringTotemSpell, DateTime.Now);
            spellCoolDown.Add(ManaTideTotemSpell, DateTime.Now);
            spellCoolDown.Add(CalloftheElementsSpell, DateTime.Now);

            //Time event
            earthShieldEvent = new(TimeSpan.FromSeconds(7));
            manaTideTotemEvent = new(TimeSpan.FromSeconds(12));
            totemcastEvent = new(TimeSpan.FromSeconds(4));
        }

        public override string Author => "Lukas";

        public override Dictionary<string, dynamic> C { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Resto Shaman";

        public override string Displayname => "Shaman Restoration";

        //Time event
        public TimegatedEvent earthShieldEvent { get; private set; }

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public TimegatedEvent manaTideTotemEvent { get; private set; }

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

        public TimegatedEvent totemcastEvent { get; private set; }

        public override bool UseAutoAttacks => false;

        public bool UseSpellOnlyInCombat { get; private set; }

        public override string Version => "2.1";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Shaman;

        public override void ExecuteCC()
        {
            UseSpellOnlyInCombat = true;
            Shield();
            StartHeal();
        }

        public override void OutOfCombatExecute()
        {
            revivePartyMember(ancestralSpiritSpell);

            if (CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_MAINHAND, earthlivingBuff, earthlivingWeaponSpell))
            {
                return;
            }
            UseSpellOnlyInCombat = false;
            Shield();
            StartHeal();
        }

        private void StartHeal()
        {
            // List<WowUnit> partyMemberToHeal = WowInterface.ObjectManager.Partymembers.Where(e => e.HealthPercentage <= 94 && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();//FirstOrDefault => tolist

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

                        if (UseSpellOnlyInCombat && WowInterface.Player.HealthPercentage < 20 && CustomCastSpellMana(heroismSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && WowInterface.Target.HealthPercentage < 20 && CustomCastSpellMana(naturesswiftSpell) && CustomCastSpellMana(healingWaveSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && WowInterface.Target.HealthPercentage < 40 && CustomCastSpellMana(tidalForceSpell))
                        {
                            return;
                        }

                        //if (partyMemberToHeal.Count >= 3 && WowInterface.Target.HealthPercentage < 40 && CustomCastSpell(Bloodlust))
                        //{
                        //    return;
                        //}
                        //Race Draenei
                        if (WowInterface.Player.Race == WowRace.Draenei && WowInterface.Target.HealthPercentage < 50 && CustomCastSpellMana(giftOfTheNaaruSpell))
                        {
                            return;
                        }

                        if (WowInterface.Target.HealthPercentage <= 50 && CustomCastSpellMana(healingWaveSpell))
                        {
                            return;
                        }

                        if (WowInterface.Target.HealthPercentage <= 75 && CustomCastSpellMana(lesserHealingWaveSpell))
                        {
                            return;
                        }

                        if (partyMemberToHeal.Count >= 4 && WowInterface.Target.HealthPercentage >= 80 && CustomCastSpellMana(chainHealSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && earthShieldEvent.Run() && !WowInterface.Target.HasBuffByName("Earth Shield") && !WowInterface.Target.HasBuffByName("Water Shield") && WowInterface.Target.HealthPercentage < 90 && CustomCastSpellMana(earthShieldSpell))
                        {
                            return;
                        }

                        if (!WowInterface.Target.HasBuffByName("Riptide") && WowInterface.Target.HealthPercentage < 90 && CustomCastSpellMana(riptideSpell))
                        {
                            return;
                        }
                    }

                    if (totemcastEvent.Run() && totemItemCheck())
                    {
                        if (WowInterface.Player.ManaPercentage <= 10 && CustomCastSpellMana(ManaTideTotemSpell))
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                totemItemCheck();

                if (totemcastEvent.Run() && totemItemCheck())
                {
                    if (WowInterface.Player.ManaPercentage >= 50
                        && !WowInterface.Player.HasBuffByName("Windfury Totem")
                        && !WowInterface.Player.HasBuffByName("Stoneskin")
                        && !WowInterface.Player.HasBuffByName("Flametongue Totem")
                        && CustomCastSpellMana(CalloftheElementsSpell))
                    {
                        return;
                    }
                }

                if (TargetSelectEvent.Run())
                {
                    WowUnit nearTarget = WowInterface.Objects.GetNearEnemies<WowUnit>(WowInterface.NewWowInterface, WowInterface.Player.Position, 30)
                    .Where(e => e.IsInCombat && !e.IsNotAttackable && e.IsCasting && e.Name != "The Lich King" && !(WowInterface.Objects.MapId == WowMapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346))
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
                        if (UseSpellOnlyInCombat && WowInterface.Target.IsCasting && CustomCastSpellMana(windShearSpell))
                        {
                            return;
                        }
                        if (UseSpellOnlyInCombat && WowInterface.Player.ManaPercentage >= 80 && CustomCastSpellMana(flameShockSpell))
                        {
                            return;
                        }
                        //if (UseSpellOnlyInCombat && WowInterface.Player.ManaPercentage >= 90 && CustomCastSpell(earthShockSpell))
                        //{
                        //    return;
                        //}
                    }
                }
            }
        }

        private void Shield()
        {
            if (!WowInterface.Player.HasBuffByName("Water Shield") && CustomCastSpellMana(watershieldSpell))
            {
                return;
            }
        }
    }
}