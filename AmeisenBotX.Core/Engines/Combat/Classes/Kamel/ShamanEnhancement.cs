using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Kamel
{
    internal class ShamanEnhancement : BasicKamelClass
    {
        private const string earthbindTotem = "Earthbind Totem";

        private const string earthElementalTotem = "Earth Elemental Totem";

        private const string earthShockSpell = "Earth Shock";

        private const string feralSpiritSpell = "Feral Spirit";

        //Totem
        private const string fireElementalTotem = "Fire Elemental Totem";

        private const string flameShockSpell = "Flame Shock";

        private const string flametongueBuff = "Flametongue";

        private const string flametongueSpell = "Flametongue Weapon";

        private const string frostShockSpell = "Frost Shock";

        private const string groundingTotem = "Grounding Totem";

        //Heal Spells
        private const string healingWaveSpell = "Healing Wave";

        private const string lavaLashSpell = "Lava Lash";

        //Attack Spells
        private const string lightningBoltSpell = "Lightning Bolt";

        //Shield
        private const string lightningShieldSpell = "Lightning Shield";

        private const string purgeSpell = "Purge";

        //Buff
        private const string shamanisticRageSpell = "Shamanistic Rage";

        private const string stormstrikeSpell = "Stormstrike";

        //Weapon Enhancement
        private const string windfuryBuff = "Windfury";

        private const string windfurySpell = "Windfury Weapon";

        //Stunns|Interrupting
        private const string windShearSpell = "Wind Shear";

        public ShamanEnhancement(AmeisenBotInterfaces bot) : base()
        {
            Bot = bot;

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
            spellCoolDown.Add(groundingTotem, DateTime.Now);
            spellCoolDown.Add(earthbindTotem, DateTime.Now);

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
            spellCoolDown.Add(purgeSpell, DateTime.Now);

            //Buff
            spellCoolDown.Add(shamanisticRageSpell, DateTime.Now);

            //Event
            EnhancementEvent = new(TimeSpan.FromSeconds(2));
            PurgeEvent = new(TimeSpan.FromSeconds(1));
        }

        public override string Author => "Lukas";

        public override Dictionary<string, dynamic> C { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Shaman Enhancement";

        public override string Displayname => "Shaman Enhancement";

        //Event
        public TimegatedEvent EnhancementEvent { get; private set; }

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.TWOHANDED_AXES, WowWeaponType.TWOHANDED_MACES, WowWeaponType.TWOHANDED_SWORDS });

        public TimegatedEvent PurgeEvent { get; private set; }

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

        private void Shield()
        {
            if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Lightning Shield") && CustomCastSpellMana(lightningShieldSpell))
            {
                return;
            }
        }

        private void StartAttack()
        {
            if (Bot.Wow.TargetGuid != 0)
            {
                ChangeTargetToAttack();

                if (Bot.Db.GetReaction(Bot.Player, Bot.Target) == WowUnitReaction.Friendly)
                {
                    Bot.Wow.WowClearTarget();
                    return;
                }

                if (Bot.Player.IsInMeleeRange(Bot.Target))
                {
                    if (!Bot.Player.IsAutoAttacking && AutoAttackEvent.Run())
                    {
                        Bot.Wow.LuaStartAutoAttack();
                    }

                    if (Bot.Player.Auras.FirstOrDefault(e => Bot.Db.GetSpellName(e.SpellId) == "Maelstrom Weapon").StackCount >= 5
                    && ((Bot.Player.HealthPercentage >= 50 && CustomCastSpellMana(lightningBoltSpell)) || CustomCastSpellMana(healingWaveSpell)))
                    {
                        return;
                    }
                    if (Bot.Target.IsCasting && CustomCastSpellMana(windShearSpell))
                    {
                        return;
                    }
                    if (PurgeEvent.Run() &&
                        (Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Mana Shield")
                      || Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Power Word: Shield")
                      || Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Renew")
                      || Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Riptide")
                      || Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Earth Shield")) && CustomCastSpellMana(purgeSpell))
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
                    if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Flame Shock") && CustomCastSpellMana(flameShockSpell))
                    {
                        return;
                    }
                    if (CustomCastSpellMana(frostShockSpell))
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

        private void WeaponEnhancement()
        {
            if (EnhancementEvent.Run())
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
}