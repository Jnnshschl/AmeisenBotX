using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Kamel
{
    class RestorationShaman : BasicKamelClass
    {
        private bool hasTotemItems = false;
        //Spells / DMG
        private const string LightningBoltSpell = "Lightning Bolt";
        //Spells / Heal
        private const string healingWaveSpell = "Healing Wave";
        private const string lesserHealingWaveSpell = "Lesser Healing Wave";
        private const string riptideSpell = "Riptide";
        private const string watershieldSpell = "Water shield";
        private const string LightningShieldSpell = "Lightning Shield";
        private const string ancestralSpiritSpell = "Ancestral Spirit";
        private const string chainHealSpell = "Chain Heal";
        private const string earthlivingBuff = "Earthliving ";
        private const string earthlivingWeaponSpell = "Earthliving Weapon";
        //CD|Buffs
        private const string naturesswiftSpell = "Nature's Swiftness";
        private const string Bloodlust = "Bloodlust";
        private const string earthShieldSpell = "Earth Shield";
        private const string tidalForceSpell = "Tidal Force";
        //Race (Troll)
        private const string BerserkingSpell = "Berserking";
        //Race (Draenei)
        private const string giftOfTheNaaruSpell = "Gift of the Naaru";
        //Totem
        private const string WindfuryTotemSpell = "Windfury Totem";
        private const string StrengthofEarthTotemSpell = "Strength of Earth Totem";
        private const string ManaSpringTotemSpell = "Mana Spring Totem";
        private const string CalloftheElementsSpell = "Call of the Elements";

        Dictionary<string, DateTime> spellCoolDown = new Dictionary<string, DateTime>();

        public RestorationShaman(WowInterface wowInterface) : base()
        {
            WowInterface = wowInterface;
            //Spells / DMG
            spellCoolDown.Add(LightningBoltSpell, DateTime.Now);
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
            //Race
            spellCoolDown.Add(giftOfTheNaaruSpell, DateTime.Now);
            //CD|Buffs
            spellCoolDown.Add(naturesswiftSpell, DateTime.Now);
            spellCoolDown.Add(BerserkingSpell, DateTime.Now);
            spellCoolDown.Add(Bloodlust, DateTime.Now);
            //Totem
            spellCoolDown.Add(WindfuryTotemSpell, DateTime.Now);
            spellCoolDown.Add(StrengthofEarthTotemSpell, DateTime.Now);
            spellCoolDown.Add(ManaSpringTotemSpell, DateTime.Now);
            spellCoolDown.Add(CalloftheElementsSpell, DateTime.Now);

            if (WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Earth Totem", StringComparison.OrdinalIgnoreCase)) &&
                 WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Air Totem", StringComparison.OrdinalIgnoreCase)) &&
                 WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Water Totem", StringComparison.OrdinalIgnoreCase)) &&
                 WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Fire Totem", StringComparison.OrdinalIgnoreCase)))
            {
                hasTotemItems = true;
            }

        }

        public override string Author => "Kamel";

        public override bool WalkBehindEnemy => false;

        public override WowClass Class => WowClass.Shaman;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Basic Resto Shaman";

        public override string Displayname => "Shaman Restoration";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override bool UseAutoAttacks => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override string Version => "2.0";
        public bool targetIsInRange { get; set; }

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

        public override void ExecuteCC()
        {
            if (!TargetInLineOfSight)
            {
                return;
            }
            Shield();
            StartHeal();
        }

        public override void OutOfCombatExecute()
        {
            List<WowUnit> partyMemberToHeal = WowInterface.ObjectManager.Partymembers.Where(e => e.IsDead).OrderBy(e => e.HealthPercentage).ToList();
            if (revivePlayerEvent.Run() && partyMemberToHeal.Count > 0)
            {
                WowInterface.HookManager.TargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                WowInterface.HookManager.CastSpell(ancestralSpiritSpell);
            }

            if (CheckForWeaponEnchantment(EquipmentSlot.INVSLOT_MAINHAND, earthlivingBuff, earthlivingWeaponSpell))
            {
                return;
            }
            Shield();
            StartHeal();
        }

        private void StartHeal()
        {
            List<WowUnit> partyMemberToHeal = WowInterface.ObjectManager.Partymembers.Where(e => e.HealthPercentage <= 99 && !e.IsDead).OrderBy(e => e.HealthPercentage).ToList();//FirstOrDefault => tolist

            if (partyMemberToHeal.Count > 0)
            {
                if (WowInterface.ObjectManager.TargetGuid != partyMemberToHeal.FirstOrDefault().Guid)
                {
                    WowInterface.HookManager.TargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                }

                targetIsInRange = WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(partyMemberToHeal.FirstOrDefault().Guid).Position) <= 30;
                if (targetIsInRange)
                {

                    if (WowInterface.MovementEngine.MovementAction != Movement.Enums.MovementAction.None)
                    {
                        WowInterface.HookManager.StopClickToMoveIfActive();
                        WowInterface.MovementEngine.Reset();
                    }
                    if (WowInterface.ObjectManager.Target != null)
                    {
                        if (hasTotemItems)
                        {
                            if (!WowInterface.ObjectManager.Player.HasBuffByName("Mana Spring")
                                || !WowInterface.ObjectManager.Player.HasBuffByName("Windfury Totem")
                                || !WowInterface.ObjectManager.Player.HasBuffByName("Strength of Earth")
                                && WowInterface.ObjectManager.Player.ManaPercentage >= 5)
                            {
                                if (CustomCastSpell(CalloftheElementsSpell))
                                {
                                    return;
                                }
                            }
                        }

                        if (WowInterface.ObjectManager.Target.HealthPercentage < 20 && CustomCastSpell(naturesswiftSpell) && CustomCastSpell(healingWaveSpell))
                        {
                            return;
                        }

                        if (WowInterface.ObjectManager.Target.HealthPercentage < 40 && CustomCastSpell(tidalForceSpell))
                        {
                            return;
                        }

                        if (partyMemberToHeal.Count >= 3 && WowInterface.ObjectManager.Target.HealthPercentage < 50 && CustomCastSpell(Bloodlust))
                        {
                            return;
                        }
                        //Race Draenei
                        if (WowInterface.ObjectManager.Player.Race == WowRace.Draenei && WowInterface.ObjectManager.Target.HealthPercentage < 50 && CustomCastSpell(giftOfTheNaaruSpell))
                        {
                            return;
                        }

                        if (partyMemberToHeal.Count >= 3 && WowInterface.ObjectManager.Target.HealthPercentage < 60 && CustomCastSpell(chainHealSpell))
                        {
                            return;
                        }

                        if (WowInterface.ObjectManager.Target.HealthPercentage < 60 && CustomCastSpell(healingWaveSpell))
                        {
                            return;
                        }

                        if (WowInterface.ObjectManager.Target.HealthPercentage < 85 && CustomCastSpell(lesserHealingWaveSpell))
                        {
                            return;
                        }

                        if (partyMemberToHeal.Count >= 3 && WowInterface.ObjectManager.Target.HealthPercentage < 90 && CustomCastSpell(earthShieldSpell))
                        {
                            return;
                        }

                        if (!WowInterface.ObjectManager.Target.HasBuffByName("Riptide") && WowInterface.ObjectManager.Target.HealthPercentage < 95 && CustomCastSpell(riptideSpell))
                        {
                            return;
                        }

                    }

                }
            }
            else
            {
                //WowInterface.HookManager.ClearTarget();
                //return;
                //Attacken
            }
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

        private void Shield() 
        {
            if (!WowInterface.ObjectManager.Player.HasBuffByName("Water Shield") && CustomCastSpell(watershieldSpell))
            {
                return;
            }
            //else if (!WowInterface.ObjectManager.Target.HasBuffByName("Water Shiel") && CustomCastSpell(LightningShieldSpell)) 
            //{
            //
            //}
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
    }
}
