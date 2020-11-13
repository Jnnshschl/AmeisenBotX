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
    internal class RestorationShaman : BasicKamelClass
    {
        private const string ancestralSpiritSpell = "Ancestral Spirit";

        //Race (Troll)
        private const string BerserkingSpell = "Berserking";

        //Race (Draenei)
        private const string giftOfTheNaaruSpell = "Gift of the Naaru";

        //Spells
        private const string healingWaveSpell = "Healing Wave";
        private const string windShearSpell = "Wind Shear";
        private const string lesserHealingWaveSpell = "Lesser Healing Wave";
        private const string Bloodlust = "Bloodlust";
        private const string CalloftheElementsSpell = "Call of the Elements";
        private const string chainHealSpell = "Chain Heal";
        private const string earthlivingBuff = "Earthliving ";
        private const string earthlivingWeaponSpell = "Earthliving Weapon";
        private const string earthShieldSpell = "Earth Shield";

        //Spells / DMG
        private const string LightningBoltSpell = "Lightning Bolt";
        private const string flameShockSpell = "Flame Shock";
        private const string earthShockSpell = "Earth Shock";

        //CD|Buffs
        private const string naturesswiftSpell = "Nature's Swiftness";
        private const string heroismSpell = "Heroism";
        private const string riptideSpell = "Riptide";
        private const string tidalForceSpell = "Tidal Force";
        private const string watershieldSpell = "Water shield";
        private const string LightningShieldSpell = "Lightning Shield";

        //Totem
        private const string WindfuryTotemSpell = "Windfury Totem";
        private const string ManaTideTotemSpell = "Mana Tide Totem";
        private const string StrengthofEarthTotemSpell = "Strength of Earth Totem";
        private const string ManaSpringTotemSpell = "Mana Spring Totem";

        private bool hasTotemItems = false;
        private Dictionary<string, DateTime> spellCoolDown = new Dictionary<string, DateTime>();

        public RestorationShaman(WowInterface wowInterface) : base()
        {
            WowInterface = wowInterface;

            //Race
            spellCoolDown.Add(giftOfTheNaaruSpell, DateTime.Now);

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
            spellCoolDown.Add(ancestralSpiritSpell, DateTime.Now);
            spellCoolDown.Add(chainHealSpell, DateTime.Now);
            spellCoolDown.Add(earthlivingBuff, DateTime.Now);
            spellCoolDown.Add(earthlivingWeaponSpell, DateTime.Now);
            spellCoolDown.Add(windShearSpell, DateTime.Now);

            //CD|Buffs
            spellCoolDown.Add(naturesswiftSpell, DateTime.Now);
            spellCoolDown.Add(heroismSpell, DateTime.Now);
            spellCoolDown.Add(BerserkingSpell, DateTime.Now);
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
            earthShieldEvent = new TimegatedEvent(TimeSpan.FromSeconds(7));
            revivePlayerEvent = new TimegatedEvent(TimeSpan.FromSeconds(4));
            manaTideTotemEvent = new TimegatedEvent(TimeSpan.FromSeconds(12));
            totemcastEvent = new TimegatedEvent(TimeSpan.FromSeconds(4));
        }

        public override string Author => "Lukas";

        public override WowClass WowClass => WowClass.Shaman;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Resto Shaman";

        public override string Displayname => "Shaman Restoration";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;
        public bool UseSpellOnlyInCombat { get; private set; }

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Heal;

        //Time event
        public TimegatedEvent earthShieldEvent { get; private set; }

        public TimegatedEvent manaTideTotemEvent { get; private set; }

        public TimegatedEvent naturesswiftEvent { get; private set; }

        public TimegatedEvent revivePlayerEvent { get; private set; }

        public TimegatedEvent riptideSpellEvent { get; private set; }
        public TimegatedEvent totemcastEvent { get; private set; }

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

        public override string Version => "2.1";

        public override bool WalkBehindEnemy => false;

        public override void ExecuteCC()
        {
            UseSpellOnlyInCombat = true;
            Shield();
            StartHeal();
        }

        public override void OutOfCombatExecute()
        {
            List<WowUnit> partyMemberToHeal = new List<WowUnit>(WowInterface.ObjectManager.Partymembers);
            partyMemberToHeal.Add(WowInterface.ObjectManager.Player);

            partyMemberToHeal = partyMemberToHeal.Where(e => e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

            if (revivePlayerEvent.Run() && partyMemberToHeal.Count > 0)
            {
                WowInterface.HookManager.WowTargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                WowInterface.HookManager.LuaCastSpell(ancestralSpiritSpell);
            }

            if (CheckForWeaponEnchantment(EquipmentSlot.INVSLOT_MAINHAND, earthlivingBuff, earthlivingWeaponSpell))
            {
                return;
            }
            UseSpellOnlyInCombat = false;
            Shield();
            StartHeal();
        }

        protected bool CheckForWeaponEnchantment(EquipmentSlot slot, string enchantmentName, string spellToCastEnchantment)
        {
            if (WowInterface.CharacterManager.Equipment.Items.ContainsKey(slot))
            {
                int itemId = WowInterface.CharacterManager.Equipment.Items[slot].Id;

                if (itemId > 0)
                {
                    WowItem item = WowInterface.ObjectManager.WowObjects.OfType<WowItem>().FirstOrDefault(e => e.EntryId == itemId);

                    if (item != null
                        && !item.GetEnchantmentStrings().Any(e => e.Contains(enchantmentName))
                        && CustomCastSpell(spellToCastEnchantment))
                    {
                        return true;
                    }
                }
            }

            return false;
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
                            WowInterface.HookManager.LuaCastSpell(spellName);
                            return true;
                        }
                    }
                }
                else
                {
                    WowInterface.HookManager.WowTargetGuid(WowInterface.ObjectManager.PlayerGuid);

                    Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                    if ((WowInterface.ObjectManager.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        WowInterface.HookManager.LuaCastSpell(spellName);
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
                spellCoolDown[spellName] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.LuaGetSpellCooldown(spellName));
                return true;
            }

            return false;
        }

        private void Shield()
        {
            if (!WowInterface.ObjectManager.Player.HasBuffByName("Water Shield") && CustomCastSpell(watershieldSpell))
            {
                return;
            }
        }

        private void totemItemCheck()
        {
            if (WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Earth Totem", StringComparison.OrdinalIgnoreCase)) &&
             WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Air Totem", StringComparison.OrdinalIgnoreCase)) &&
             WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Water Totem", StringComparison.OrdinalIgnoreCase)) &&
             WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Fire Totem", StringComparison.OrdinalIgnoreCase)) ||
             (WowInterface.CharacterManager.Equipment.Items.ContainsKey(EquipmentSlot.INVSLOT_RANGED) &&
             WowInterface.CharacterManager.Equipment.Items[EquipmentSlot.INVSLOT_RANGED] != null))
            {
                hasTotemItems = true;
            }
        }

        private void StartHeal()
        {
            // List<WowUnit> partyMemberToHeal = WowInterface.ObjectManager.Partymembers.Where(e => e.HealthPercentage <= 94 && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();//FirstOrDefault => tolist

            List<WowUnit> partyMemberToHeal = new List<WowUnit>(WowInterface.ObjectManager.Partymembers);
            //healableUnits.AddRange(WowInterface.ObjectManager.PartyPets);
            partyMemberToHeal.Add(WowInterface.ObjectManager.Player);

            partyMemberToHeal = partyMemberToHeal.Where(e => e.HealthPercentage <= 94 && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

            if (partyMemberToHeal.Count > 0)
            {
                if (WowInterface.ObjectManager.TargetGuid != partyMemberToHeal.FirstOrDefault().Guid)
                {
                    WowInterface.HookManager.WowTargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                }

                if (WowInterface.ObjectManager.TargetGuid != 0 && WowInterface.ObjectManager.Target != null)
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
                            WowInterface.HookManager.WowStopClickToMove();
                            WowInterface.MovementEngine.Reset();
                        }

                        if (WowInterface.ObjectManager.Target != null && WowInterface.ObjectManager.Target.HealthPercentage >= 90)
                        {
                            WowInterface.HookManager.LuaDoString("SpellStopCasting()");
                            return;
                        }

                        if (UseSpellOnlyInCombat && WowInterface.ObjectManager.Player.HealthPercentage < 20 && CustomCastSpell(heroismSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && WowInterface.ObjectManager.Target.HealthPercentage < 20 && CustomCastSpell(naturesswiftSpell) && CustomCastSpell(healingWaveSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && WowInterface.ObjectManager.Target.HealthPercentage < 40 && CustomCastSpell(tidalForceSpell))
                        {
                            return;
                        }

                        //if (partyMemberToHeal.Count >= 3 && WowInterface.ObjectManager.Target.HealthPercentage < 40 && CustomCastSpell(Bloodlust))
                        //{
                        //    return;
                        //}
                        //Race Draenei
                        if (WowInterface.ObjectManager.Player.Race == WowRace.Draenei && WowInterface.ObjectManager.Target.HealthPercentage < 50 && CustomCastSpell(giftOfTheNaaruSpell))
                        {
                            return;
                        }

                        if (WowInterface.ObjectManager.Target.HealthPercentage <= 50 && CustomCastSpell(healingWaveSpell))
                        {
                            return;
                        }

                        if (WowInterface.ObjectManager.Target.HealthPercentage <= 75 && CustomCastSpell(lesserHealingWaveSpell))
                        {
                            return;
                        }

                        if (partyMemberToHeal.Count >= 4 && WowInterface.ObjectManager.Target.HealthPercentage >= 80 && CustomCastSpell(chainHealSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && earthShieldEvent.Run() && !WowInterface.ObjectManager.Target.HasBuffByName("Earth Shield") && !WowInterface.ObjectManager.Target.HasBuffByName("Water Shield") && WowInterface.ObjectManager.Target.HealthPercentage < 90 && CustomCastSpell(earthShieldSpell))
                        {
                            return;
                        }

                        if (!WowInterface.ObjectManager.Target.HasBuffByName("Riptide") && WowInterface.ObjectManager.Target.HealthPercentage < 90 && CustomCastSpell(riptideSpell))
                        {
                            return;
                        }
                    }

                    totemItemCheck();

                    if (totemcastEvent.Run() && hasTotemItems)
                    {
                        if (WowInterface.ObjectManager.Player.ManaPercentage <= 10 && CustomCastSpell(ManaTideTotemSpell))
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                totemItemCheck();

                if (totemcastEvent.Run() && hasTotemItems)
                {
                    if (WowInterface.ObjectManager.Player.ManaPercentage >= 50
                        && !WowInterface.ObjectManager.Player.HasBuffByName("Windfury Totem")
                        && !WowInterface.ObjectManager.Player.HasBuffByName("Stoneskin")
                        && !WowInterface.ObjectManager.Player.HasBuffByName("Flametongue Totem")
                        && CustomCastSpell(CalloftheElementsSpell))
                    {
                        return;
                    }
                }

                if (TargetSelectEvent.Run())
                {
                    WowUnit nearTarget = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 30)
                    .Where(e => e.IsInCombat && !e.IsNotAttackable && e.Name != "The Lich King" && !(WowInterface.ObjectManager.MapId == MapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346))//&& e.IsCasting 
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                    .FirstOrDefault();

                    //if (nearTarget != null )
                    if (WowInterface.ObjectManager.TargetGuid != 0 && WowInterface.ObjectManager.Target != null && nearTarget != null)
                    {
                        WowInterface.HookManager.WowTargetGuid(nearTarget.Guid);

                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                        if (WowInterface.MovementEngine.MovementAction != Movement.Enums.MovementAction.None)
                        {
                            WowInterface.HookManager.WowStopClickToMove();
                            WowInterface.MovementEngine.Reset();
                        }
                        if (UseSpellOnlyInCombat && WowInterface.ObjectManager.Target.IsCasting && CustomCastSpell(windShearSpell))
                        {
                            return;
                        }
                        if (UseSpellOnlyInCombat && WowInterface.ObjectManager.Player.ManaPercentage >= 80 && CustomCastSpell(flameShockSpell))
                        {
                            return;
                        }
                        //if (UseSpellOnlyInCombat && WowInterface.ObjectManager.Player.ManaPercentage >= 90 && CustomCastSpell(earthShockSpell))
                        //{
                        //    return;
                        //}
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