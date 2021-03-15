using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Kamel
{
    class ShamanEnhancement : BasicKamelClass
    {
        //Shield
        private const string lightningShieldSpell = "Lightning Shield";

        //Weapon Enhancement
        private const string windfuryBuff = "Windfury";
        private const string flametongueBuff = "Flametongue";
        private const string flametongueSpell = "Flametongue Weapon";
        private const string windfurySpell = "Windfury Weapon";

        //Heal Spells
        private const string healingWaveSpell = "Healing Wave";

        //Totem
        private const string fireElementalTotem = "Fire Elemental Totem";
        private const string earthElementalTotem = "Earth Elemental Totem";

        //Attack Spells
        private const string lightningBoltSpell = "Lightning Bolt";
        private const string lavaLashSpell = "Lava Lash";
        private const string stormstrikeSpell = "Stormstrike";
        private const string flameShockSpell = "Flame Shock";
        private const string frostShockSpell = "Frost Shock";
        private const string earthShockSpell = "Earth Shock";
        private const string feralSpiritSpell = "Feral Spirit";

        //Stunns|Interrupting
        private const string windShearSpell = "Wind Shear";

        //Buff
        private const string shamanisticRageSpell = "Shamanistic Rage";

        public ShamanEnhancement(WowInterface wowInterface) : base()
        {
            WowInterface = wowInterface;

            //Shield
            spellCoolDown.Add(lightningShieldSpell, DateTime.Now);

            //Weapon Enhancement
            spellCoolDown.Add(windfuryBuff, DateTime.Now);
            spellCoolDown.Add(flametongueBuff, DateTime.Now);
            spellCoolDown.Add(flametongueSpell, DateTime.Now);
            spellCoolDown.Add(windfurySpell, DateTime.Now);
            
            //Heal Spells
            spellCoolDown.Add(healingWaveSpell, DateTime.Now);

            //Totem
            spellCoolDown.Add(fireElementalTotem, DateTime.Now);
            spellCoolDown.Add(earthElementalTotem, DateTime.Now);

            //Attack Spells
            spellCoolDown.Add(lightningBoltSpell, DateTime.Now);
            spellCoolDown.Add(lavaLashSpell, DateTime.Now);
            spellCoolDown.Add(stormstrikeSpell, DateTime.Now);
            spellCoolDown.Add(flameShockSpell, DateTime.Now);
            spellCoolDown.Add(frostShockSpell, DateTime.Now);
            spellCoolDown.Add(earthShockSpell, DateTime.Now);
            spellCoolDown.Add(feralSpiritSpell, DateTime.Now);

            //Stunns|Interrupting
            spellCoolDown.Add(windShearSpell, DateTime.Now);

            //Buff
            spellCoolDown.Add(shamanisticRageSpell, DateTime.Now);
        }
        public override string Author => "Lukas";

        public override Dictionary<string, dynamic> C { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Shaman Enhancement";

        public override string Displayname => "Shaman Enhancement";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.TWOHANDED_AXES, WowWeaponType.TWOHANDED_MACES, WowWeaponType.TWOHANDED_SWORDS });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 2, new(1, 2, 5) },
                { 3, new(1, 3, 3) },
                { 5, new(1, 5, 3) },
                { 8, new(1, 8, 5) },
            },
            Tree2 = new()
            {
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 7, new(2, 7, 3) },
                { 8, new(2, 8, 3) },
                { 9, new(2, 9, 1) },
                { 11, new(2, 11, 5) },
                { 13, new(2, 13, 2) },
                { 14, new(2, 14, 1) },
                { 15, new(2, 15, 3) },
                { 16, new(2, 16, 3) },
                { 17, new(2, 17, 3) },
                { 19, new(2, 19, 3) },
                { 20, new(2, 20, 1) },
                { 21, new(2, 21, 1) },
                { 22, new(2, 22, 3) },
                { 23, new(2, 23, 1) },
                { 24, new(2, 24, 2) },
                { 25, new(2, 25, 3) },
                { 26, new(2, 26, 1) },
                { 28, new(2, 28, 5) },
                { 29, new(2, 29, 1) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Shaman;

        public override void ExecuteCC()
        {
            StartAttack();
        }

        public override void OutOfCombatExecute()
        {
            Shield();
            WeaponEnhancement();
            revivePartyMember(ancestralSpiritSpell);
            Targetselection();
            StartAttack();
        }

        private void StartAttack()
        {
            if (WowInterface.TargetGuid != 0)
            {
                ChangeTargetToAttack();

                if (WowInterface.HookManager.WowGetUnitReaction(WowInterface.Player, WowInterface.Target) == WowUnitReaction.Friendly)
                {
                    WowInterface.HookManager.WowClearTarget();
                    return;
                }

                if (WowInterface.Player.IsInMeleeRange(WowInterface.Target))
                {
                    if (!WowInterface.Player.IsAutoAttacking && AutoAttackEvent.Run())
                    {
                        WowInterface.HookManager.LuaStartAutoAttack();
                    }

                    if (WowInterface.Target.IsCasting && CustomCastSpellMana(windShearSpell))
                    {
                        return;
                    }    
                    if (totemItemCheck() && CustomCastSpellMana(fireElementalTotem))
                    {
                        return;
                    }     
                    if (totemItemCheck() && CustomCastSpellMana(earthElementalTotem))
                    {
                        return;
                    }          
                    if (CustomCastSpellMana(lavaLashSpell))
                    {
                        return;
                    }              
                    if (CustomCastSpellMana(stormstrikeSpell))
                    {
                        return;
                    }         
                    if (CustomCastSpellMana(feralSpiritSpell))
                    {
                        return;
                    }    
                    if (!WowInterface.Target.HasBuffByName("Flame Shock") && CustomCastSpellMana(flameShockSpell))
                    {
                        return;
                    }          
                    if (CustomCastSpellMana(frostShockSpell))
                    {
                        return;
                    }

                    if (WowInterface.Player.Auras.FirstOrDefault(e => e.Name == "Maelstrom Weapon")?.StackCount >= 5
                    && ((WowInterface.Player.HealthPercentage >= 50 && CustomCastSpellMana(lightningBoltSpell)) || CustomCastSpellMana(healingWaveSpell)))
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

        private void Shield()
        {
            if (!WowInterface.Player.HasBuffByName("Lightning Shield") && CustomCastSpellMana(lightningShieldSpell))
            {
                return;
            }
        }

        private void WeaponEnhancement()
        {
            if (CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_MAINHAND, windfuryBuff, windfurySpell))
            {
                return;
            }

            if (CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_OFFHAND, flametongueBuff, flametongueSpell))
            {
                return;
            }
        }
    }
}
