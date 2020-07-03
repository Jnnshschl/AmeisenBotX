using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
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
        //Spells
        private const string healingWaveSpell = "Healing Wave";
        private const string riptideSpell = "Riptide";
        private const string watershieldSpell = "Water shield";
        private const string LightningShieldSpell = "Lightning Shield";
        private const string ancestralSpiritSpell = "Ancestral Spirit";
        //CD|Buffs
        private const string naturesswiftSpell = "Nature's Swiftness";
        private const string Bloodlust = "Bloodlust";
        //Race (Troll)
        private const string BerserkingSpell = "Berserking";
        //Totem
        private const string WindfuryTotemSpell = "Windfury Totem";
        private const string StrengthofEarthTotemSpell = "Strength of Earth Totem";
        private const string ManaSpringTotemSpell = "Mana Spring Totem";
        private const string CalloftheElementsSpell = "Call of the Elements";

        Dictionary<string, DateTime> spellCoolDown = new Dictionary<string, DateTime>();

        public RestorationShaman(WowInterface wowInterface) : base()
        {
            WowInterface = wowInterface;
            //Spells
            spellCoolDown.Add(healingWaveSpell, DateTime.Now);
            spellCoolDown.Add(riptideSpell, DateTime.Now);
            spellCoolDown.Add(watershieldSpell, DateTime.Now);
            spellCoolDown.Add(LightningShieldSpell, DateTime.Now);
            spellCoolDown.Add(ancestralSpiritSpell, DateTime.Now);
            //CD|Buffs
            spellCoolDown.Add(naturesswiftSpell, DateTime.Now);
            spellCoolDown.Add(BerserkingSpell, DateTime.Now);
            spellCoolDown.Add(Bloodlust, DateTime.Now);
            //Totem
            spellCoolDown.Add(WindfuryTotemSpell, DateTime.Now);
            spellCoolDown.Add(StrengthofEarthTotemSpell, DateTime.Now);
            spellCoolDown.Add(ManaSpringTotemSpell, DateTime.Now);
            spellCoolDown.Add(CalloftheElementsSpell, DateTime.Now);

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, Utils.AuraManager.CastFunction>();
            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(watershieldSpell))
                {
                    MyAuraManager.BuffsToKeepActive.Add(watershieldSpell, () => { WowInterface.HookManager.CastSpell(watershieldSpell); return true; });
                }
                else if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(LightningShieldSpell))
                {
                    MyAuraManager.BuffsToKeepActive.Add(LightningShieldSpell, () => { WowInterface.HookManager.CastSpell(LightningShieldSpell); return true; });
                }
            };
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

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override string Version => "1.0";
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
            MyAuraManager.Tick();

            if (TargetSelectEvent.Run())
            {
                //WowUnit partyMember = WowInterface.ObjectManager.GetNearPartymembers(WowInterface.ObjectManager.Player.Position, 20).FirstOrDefault();
                WowUnit partyMemberToHeal = WowInterface.ObjectManager.Partymembers.Where(e => e.HealthPercentage < 95).OrderBy(e => e.HealthPercentage).FirstOrDefault();//FirstOrDefault => tolist

                if (partyMemberToHeal != null)
                {
                    if (WowInterface.ObjectManager.TargetGuid != partyMemberToHeal.Guid)
                    {
                        WowInterface.HookManager.TargetGuid(partyMemberToHeal.Guid);
                    }

                    targetIsInRange = WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(partyMemberToHeal.Guid).Position) <= 30;
                    if (targetIsInRange)
                    {
                        if (WowInterface.MovementEngine.MovementAction != Movement.Enums.MovementAction.None)
                        {
                            WowInterface.HookManager.StopClickToMoveIfActive();
                            WowInterface.MovementEngine.Reset();
                        }
                        if (WowInterface.ObjectManager.Target != null)
                        {
                            if (!WowInterface.ObjectManager.Player.HasBuffByName("Mana Spring")
                                || !WowInterface.ObjectManager.Player.HasBuffByName("Windfury Totem")
                                || !WowInterface.ObjectManager.Player.HasBuffByName("Strength of Earth")
                                &&  WowInterface.ObjectManager.Player.ManaPercentage >= 5)
                            {
                                WowInterface.HookManager.CastSpell(CalloftheElementsSpell);
                                spellCoolDown[CalloftheElementsSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(CalloftheElementsSpell));
                                return;
                            }

                            if (DateTime.Now > spellCoolDown[naturesswiftSpell] && !WowInterface.ObjectManager.Player.HasBuffByName("Nature's Swiftness") 
                                && WowInterface.ObjectManager.Target.HealthPercentage <= 30 
                                && WowInterface.ObjectManager.Player.ManaPercentage >= 2)
                            {
                                if (DateTime.Now > spellCoolDown[healingWaveSpell])
                                {
                                    WowInterface.HookManager.CastSpell(naturesswiftSpell);
                                    spellCoolDown[naturesswiftSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(naturesswiftSpell));
                                    WowInterface.HookManager.CastSpell(healingWaveSpell);
                                    spellCoolDown[healingWaveSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(healingWaveSpell));
                                    return;
                                }
                            }

                            if (DateTime.Now > spellCoolDown[riptideSpell] && WowInterface.ObjectManager.Target.HealthPercentage <= 95 && !WowInterface.ObjectManager.Target.HasBuffByName("Riptide"))
                            {
                                WowInterface.HookManager.CastSpell(riptideSpell);
                                spellCoolDown[riptideSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(riptideSpell));
                                return;
                            }

                            if (DateTime.Now > spellCoolDown[healingWaveSpell] && WowInterface.ObjectManager.Target.HealthPercentage < 70)
                            {
                                WowInterface.HookManager.CastSpell(healingWaveSpell);
                                spellCoolDown[healingWaveSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(healingWaveSpell));
                                return;
                            }
                        }
                    }
                }
                else
                {
                    //Attacken
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            MyAuraManager.Tick();

            if (!WowInterface.ObjectManager.Player.HasBuffByName("Mana Spring") && WowInterface.ObjectManager.Player.ManaPercentage < 50)
            {
                WowInterface.HookManager.CastSpell(ManaSpringTotemSpell);
                spellCoolDown[ManaSpringTotemSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(ManaSpringTotemSpell));
                return;
            }
            if (TargetSelectEvent.Run())
            {
                //WowUnit partyMember = WowInterface.ObjectManager.GetNearPartymembers(WowInterface.ObjectManager.Player.Position, 20).FirstOrDefault();
                WowUnit partyMemberToHeal = WowInterface.ObjectManager.Partymembers.Where(e => e.HealthPercentage < 95).OrderBy(e => e.HealthPercentage).FirstOrDefault();//FirstOrDefault => tolist

                if (partyMemberToHeal != null)
                {
                    if (WowInterface.ObjectManager.TargetGuid != partyMemberToHeal.Guid)
                    {
                        WowInterface.HookManager.TargetGuid(partyMemberToHeal.Guid);
                    }

                    targetIsInRange = WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(partyMemberToHeal.Guid).Position) <= 30;
                    if (targetIsInRange)
                    {
                        if (WowInterface.MovementEngine.MovementAction != Movement.Enums.MovementAction.None)
                        {
                            WowInterface.HookManager.StopClickToMoveIfActive();
                            WowInterface.MovementEngine.Reset();
                        }
                        if (WowInterface.ObjectManager.Target != null)
                        {
                            if (DateTime.Now > spellCoolDown[ancestralSpiritSpell] && WowInterface.ObjectManager.Target.IsDead && WowInterface.ObjectManager.Player.ManaPercentage >= 5)
                            {
                                WowInterface.HookManager.CastSpell(ancestralSpiritSpell);
                                spellCoolDown[ancestralSpiritSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(ancestralSpiritSpell));
                                return;
                            }   
                            
                            if (DateTime.Now > spellCoolDown[riptideSpell] && WowInterface.ObjectManager.Target.HealthPercentage <= 95 && !WowInterface.ObjectManager.Target.HasBuffByName("Riptide"))
                            {
                                WowInterface.HookManager.CastSpell(riptideSpell);
                                spellCoolDown[riptideSpell] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.GetSpellCooldown(riptideSpell));
                                return;
                            }
                        }
                    }
                }
                else
                {
                    //Attacken
                }
            }
        }
    }
}
