using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Kamel
{
    internal class RestorationShaman : BasicKamelClass
    {
        private const string ancestralSpiritSpell = "Ancestral Spirit";

        //Race (Troll)
        private const string BerserkingSpell = "Berserking";

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
        private readonly Dictionary<string, DateTime> spellCoolDown = new Dictionary<string, DateTime>();
        private bool hasTotemItems = false;

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
            earthShieldEvent = new(TimeSpan.FromSeconds(7));
            revivePlayerEvent = new(TimeSpan.FromSeconds(4));
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

        public TimegatedEvent naturesswiftEvent { get; private set; }

        public TimegatedEvent revivePlayerEvent { get; private set; }

        public TimegatedEvent riptideSpellEvent { get; private set; }

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
            List<WowUnit> partyMemberToHeal = new List<WowUnit>(WowInterface.ObjectManager.Partymembers)
            {
                WowInterface.Player
            };

            partyMemberToHeal = partyMemberToHeal.Where(e => e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

            if (revivePlayerEvent.Run() && partyMemberToHeal.Count > 0)
            {
                WowInterface.HookManager.WowTargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                WowInterface.HookManager.LuaCastSpell(ancestralSpiritSpell);
            }

            if (CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_MAINHAND, earthlivingBuff, earthlivingWeaponSpell))
            {
                return;
            }
            UseSpellOnlyInCombat = false;
            Shield();
            StartHeal();
        }

        protected bool CheckForWeaponEnchantment(WowEquipmentSlot slot, string enchantmentName, string spellToCastEnchantment)
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
                if (WowInterface.Target != null)
                {
                    Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                    if ((WowInterface.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        double distance = WowInterface.Player.Position.GetDistance(WowInterface.Target.Position);

                        if ((spell.MinRange == 0 && spell.MaxRange == 0) || (spell.MinRange <= distance && spell.MaxRange >= distance))
                        {
                            WowInterface.HookManager.LuaCastSpell(spellName);
                            return true;
                        }
                    }
                }
                else
                {
                    WowInterface.HookManager.WowTargetGuid(WowInterface.PlayerGuid);

                    Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                    if ((WowInterface.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
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
            if (!WowInterface.Player.HasBuffByName("Water Shield") && CustomCastSpell(watershieldSpell))
            {
                return;
            }
        }

        private void StartHeal()
        {
            // List<WowUnit> partyMemberToHeal = WowInterface.ObjectManager.Partymembers.Where(e => e.HealthPercentage <= 94 && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();//FirstOrDefault => tolist

            List<WowUnit> partyMemberToHeal = new List<WowUnit>(WowInterface.ObjectManager.Partymembers)
            {
                //healableUnits.AddRange(WowInterface.ObjectManager.PartyPets);
                WowInterface.Player
            };

            partyMemberToHeal = partyMemberToHeal.Where(e => e.HealthPercentage <= 94 && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

            if (partyMemberToHeal.Count > 0)
            {
                if (WowInterface.TargetGuid != partyMemberToHeal.FirstOrDefault().Guid)
                {
                    WowInterface.HookManager.WowTargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                }

                if (WowInterface.TargetGuid != 0 && WowInterface.Target != null)
                {
                    targetIsInRange = WowInterface.Player.Position.GetDistance(WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(partyMemberToHeal.FirstOrDefault().Guid).Position) <= 30;
                    if (targetIsInRange)
                    {
                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                        if (WowInterface.MovementEngine.Status != Movement.Enums.MovementAction.None)
                        {
                            WowInterface.HookManager.WowStopClickToMove();
                            WowInterface.MovementEngine.Reset();
                        }

                        if (WowInterface.Target != null && WowInterface.Target.HealthPercentage >= 90)
                        {
                            WowInterface.HookManager.LuaDoString("SpellStopCasting()");
                            return;
                        }

                        //if (UseSpellOnlyInCombat)
                        //{
                        //    WowInterface.HookManager.UseItemByName("Talisman of Resuregence");
                        //    return;
                        //}

                        if (UseSpellOnlyInCombat && WowInterface.Player.HealthPercentage < 20 && CustomCastSpell(heroismSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && WowInterface.Target.HealthPercentage < 20 && CustomCastSpell(naturesswiftSpell) && CustomCastSpell(healingWaveSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && WowInterface.Target.HealthPercentage < 40 && CustomCastSpell(tidalForceSpell))
                        {
                            return;
                        }

                        //if (partyMemberToHeal.Count >= 3 && WowInterface.Target.HealthPercentage < 40 && CustomCastSpell(Bloodlust))
                        //{
                        //    return;
                        //}
                        //Race Draenei
                        if (WowInterface.Player.Race == WowRace.Draenei && WowInterface.Target.HealthPercentage < 50 && CustomCastSpell(giftOfTheNaaruSpell))
                        {
                            return;
                        }

                        if (WowInterface.Target.HealthPercentage <= 50 && CustomCastSpell(healingWaveSpell))
                        {
                            return;
                        }

                        if (WowInterface.Target.HealthPercentage <= 75 && CustomCastSpell(lesserHealingWaveSpell))
                        {
                            return;
                        }

                        if (partyMemberToHeal.Count >= 4 && WowInterface.Target.HealthPercentage >= 80 && CustomCastSpell(chainHealSpell))
                        {
                            return;
                        }

                        if (UseSpellOnlyInCombat && earthShieldEvent.Run() && !WowInterface.Target.HasBuffByName("Earth Shield") && !WowInterface.Target.HasBuffByName("Water Shield") && WowInterface.Target.HealthPercentage < 90 && CustomCastSpell(earthShieldSpell))
                        {
                            return;
                        }

                        if (!WowInterface.Target.HasBuffByName("Riptide") && WowInterface.Target.HealthPercentage < 90 && CustomCastSpell(riptideSpell))
                        {
                            return;
                        }
                    }

                    totemItemCheck();

                    if (totemcastEvent.Run() && hasTotemItems)
                    {
                        if (WowInterface.Player.ManaPercentage <= 10 && CustomCastSpell(ManaTideTotemSpell))
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
                    if (WowInterface.Player.ManaPercentage >= 50
                        && !WowInterface.Player.HasBuffByName("Windfury Totem")
                        && !WowInterface.Player.HasBuffByName("Stoneskin")
                        && !WowInterface.Player.HasBuffByName("Flametongue Totem")
                        && CustomCastSpell(CalloftheElementsSpell))
                    {
                        return;
                    }
                }

                if (TargetSelectEvent.Run())
                {
                    WowUnit nearTarget = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.Player.Position, 30)
                    .Where(e => e.IsInCombat && !e.IsNotAttackable && e.Name != "The Lich King" && !(WowInterface.ObjectManager.MapId == WowMapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346))//&& e.IsCasting
                    .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                    .FirstOrDefault();

                    //if (nearTarget != null )
                    if (WowInterface.TargetGuid != 0 && WowInterface.Target != null && nearTarget != null)
                    {
                        WowInterface.HookManager.WowTargetGuid(nearTarget.Guid);

                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                        if (WowInterface.MovementEngine.Status != Movement.Enums.MovementAction.None)
                        {
                            WowInterface.HookManager.WowStopClickToMove();
                            WowInterface.MovementEngine.Reset();
                        }
                        if (UseSpellOnlyInCombat && WowInterface.Target.IsCasting && CustomCastSpell(windShearSpell))
                        {
                            return;
                        }
                        if (UseSpellOnlyInCombat && WowInterface.Player.ManaPercentage >= 80 && CustomCastSpell(flameShockSpell))
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

        private void totemItemCheck()
        {
            if (WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Earth Totem", StringComparison.OrdinalIgnoreCase)) &&
             WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Air Totem", StringComparison.OrdinalIgnoreCase)) &&
             WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Water Totem", StringComparison.OrdinalIgnoreCase)) &&
             WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Fire Totem", StringComparison.OrdinalIgnoreCase)) ||
             (WowInterface.CharacterManager.Equipment.Items.ContainsKey(WowEquipmentSlot.INVSLOT_RANGED) &&
             WowInterface.CharacterManager.Equipment.Items[WowEquipmentSlot.INVSLOT_RANGED] != null))
            {
                hasTotemItems = true;
            }
        }
    }
}