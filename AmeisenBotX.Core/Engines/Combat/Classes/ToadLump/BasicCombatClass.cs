using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Inventory.Objects;
using AmeisenBotX.Core.Engines.Character.Spells.Objects;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Engines.Combat.Helpers;
using AmeisenBotX.Core.Engines.Combat.Helpers.Aura;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets;
using AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Logics;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Enums;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.ToadLump
{
    public abstract class BasicCombatClass : ICombatClass
    {
        #region Deathknight

        protected const string antiMagicShellSpell = "Anti-Magic Shell";
        protected const string armyOfTheDeadSpell = "Army of the Dead";
        protected const string bloodBoilSpell = "Blood Boil";
        protected const string bloodPlagueSpell = "Blood Plague";
        protected const string bloodPresenceSpell = "Blood Presence";
        protected const string bloodStrikeSpell = "Blood Strike";
        protected const string chainsOfIceSpell = "Chains of Ice";
        protected const string darkCommandSpell = "Dark Command";
        protected const string deathAndDecaySpell = "Death and Decay";
        protected const string deathCoilSpell = "Death Coil";
        protected const string deathGripSpell = "Death Grip";
        protected const string deathStrike = "Death Strike";
        protected const string empowerRuneWeapon = "Empower Rune Weapon";
        protected const string frostFeverSpell = "Frost Fever";
        protected const string frostPresenceSpell = "Frost Presence";
        protected const string heartStrikeSpell = "Heart Strike";
        protected const string hornOfWinterSpell = "Horn of Winter";
        protected const string iceboundFortitudeSpell = "Icebound Fortitude";
        protected const string icyTouchSpell = "Icy Touch";
        protected const string mindFreezeSpell = "Mind Freeze";
        protected const string obliterateSpell = "Obliterate";
        protected const string plagueStrikeSpell = "Plague Strike";
        protected const string runeStrikeSpell = "Rune Strike";
        protected const string runeTapSpell = "Rune Tap";
        protected const string scourgeStrikeSpell = "Scourge Strike";
        protected const string strangulateSpell = "Strangulate";
        protected const string summonGargoyleSpell = "Summon Gargoyle";
        protected const string unbreakableArmorSpell = "Unbreakable Armor";
        protected const string unholyPresenceSpell = "Unholy Presence";
        protected const string vampiricBloodSpell = "Vampiric Blood";

        #endregion Deathknight

        #region Druid

        protected const string barkskinSpell = "Barkskin";
        protected const string bashSpell = "Bash";
        protected const string berserkSpell = "Berserk";
        protected const string catFormSpell = "Cat Form";
        protected const string challengingRoarSpell = "Challenging Roar";
        protected const string dashSpell = "Dash";
        protected const string direBearFormSpell = "Dire Bear Form";
        protected const string eclipseLunarSpell = "Eclipse (Lunar)";
        protected const string eclipseSolarSpell = "Eclipse (Solar)";
        protected const string enrageSpell = "Enrage";
        protected const string entanglingRootsSpell = "Entangling Roots";
        protected const string faerieFireFeralSpell = "Faerie Fire (Feral)";
        protected const string faerieFireSpell = "Faerie Fire";
        protected const string feralChargeBearSpell = "Feral Charge - Bear";
        protected const string feralChargeCatSpell = "Feral Charge - Cat";
        protected const string ferociousBiteSpell = "Ferocious Bite";
        protected const string forceOfNatureSpell = "Force of Nature";
        protected const string frenziedRegenerationSpell = "Frenzied Regeneration";
        protected const string growlSpell = "Growl";
        protected const string healingTouchSpell = "Healing Touch";
        protected const string hurricaneSpell = "Hurricane";
        protected const string innervateSpell = "Innervate";
        protected const string insectSwarmSpell = "Insect Swarm";
        protected const string lacerateSpell = "Lacerate";
        protected const string lifebloomSpell = "Lifebloom";
        protected const string mangleBearSpell = "Mangle (Bear)";
        protected const string mangleCatSpell = "Mangle (Cat)";
        protected const string markOfTheWildSpell = "Mark of the Wild";
        protected const string moonfireSpell = "Moonfire";
        protected const string moonkinFormSpell = "Moonkin Form";
        protected const string naturesGraspSpell = "Nature's Grasp";
        protected const string naturesSwiftnessSpell = "Nature's Swiftness";
        protected const string nourishSpell = "Nourish";
        protected const string rakeSpell = "Rake";
        protected const string regrowthSpell = "Regrowth";
        protected const string rejuvenationSpell = "Rejuvenation";
        protected const string reviveSpell = "Revive";
        protected const string ripSpell = "Rip";
        protected const string savageRoarSpell = "Savage Roar";
        protected const string shredSpell = "Shred";
        protected const string starfallSpell = "Starfall";
        protected const string starfireSpell = "Starfire";
        protected const string survivalInstinctsSpell = "Survival Instincts";
        protected const string swiftmendSpell = "Swiftmend";
        protected const string swipeSpell = "Swipe (Bear)";
        protected const string thornsSpell = "Thorns";
        protected const string tigersFurySpell = "Tiger's Fury";
        protected const string tranquilitySpell = "Tranquility";
        protected const string treeOfLifeSpell = "Tree of Life";
        protected const string wildGrowthSpell = "Wild Growth";
        protected const string wrathSpell = "Wrath";

        #endregion Druid

        #region Hunter

        protected const string aimedShotSpell = "Aimed Shot";
        protected const string arcaneShotSpell = "Arcane Shot";
        protected const string aspectOfTheDragonhawkSpell = "Aspect of the Dragonhawk";
        protected const string aspectOfTheHawkSpell = "Aspect of the Hawk";
        protected const string aspectOfTheViperSpell = "Aspect of the Viper";
        protected const string beastialWrathSpell = "Beastial Wrath";
        protected const string blackArrowSpell = "Black Arrow";
        protected const string callPetSpell = "Call Pet";
        protected const string chimeraShotSpell = "Chimera Shot";
        protected const string concussiveShotSpell = "Concussive Shot";
        protected const string deterrenceSpell = "Deterrence";
        protected const string disengageSpell = "Disengage";
        protected const string explosiveShotSpell = "Explosive Shot";
        protected const string feignDeathSpell = "Feign Death";
        protected const string frostTrapSpell = "Frost Trap";
        protected const string huntersMarkSpell = "Hunter's Mark";
        protected const string intimidationSpell = "Intimidation";
        protected const string killCommandSpell = "Kill Command";
        protected const string killShotSpell = "Kill Shot";
        protected const string mendPetSpell = "Mend Pet";
        protected const string mongooseBiteSpell = "Mongoose Bite";
        protected const string multiShotSpell = "Multi-Shot";
        protected const string rapidFireSpell = "Rapid Fire";
        protected const string raptorStrikeSpell = "Raptor Strike";
        protected const string revivePetSpell = "Revive Pet";
        protected const string scatterShotSpell = "Scatter Shot";
        protected const string serpentStingSpell = "Serpent Sting";
        protected const string silencingShotSpell = "Silencing Shot";
        protected const string steadyShotSpell = "Steady Shot";
        protected const string wingClipSpell = "Wing Clip";
        protected const string wyvernStingSpell = "Wyvern Sting";

        #endregion Hunter

        #region Mage

        protected const string arcaneBarrageSpell = "Arcane Barrage";
        protected const string arcaneBlastSpell = "Arcane Blast";
        protected const string arcaneIntellectSpell = "Arcane Intellect";
        protected const string arcaneMissilesSpell = "Arcane Missiles";
        protected const string counterspellSpell = "Counterspell";
        protected const string evocationSpell = "Evocation";
        protected const string fireballSpell = "Fireball";
        protected const string hotstreakSpell = "Hot Streak";
        protected const string iceBlockSpell = "Ice Block";
        protected const string icyVeinsSpell = "Icy Veins";
        protected const string livingBombSpell = "Living Bomb";
        protected const string mageArmorSpell = "Mage Armor";
        protected const string manaShieldSpell = "Mana Shield";
        protected const string mirrorImageSpell = "Mirror Image";
        protected const string missileBarrageSpell = "Missile Barrage";
        protected const string moltenArmorSpell = "Molten Armor";
        protected const string pyroblastSpell = "Pyroblast";
        protected const string scorchSpell = "Scorch";
        protected const string spellStealSpell = "Spellsteal";

        #endregion Mage

        #region Paladin

        protected const string avengersShieldSpell = "Avenger\'s Shield";
        protected const string avengingWrathSpell = "Avenging Wrath";
        protected const string beaconOfLightSpell = "Beacon of Light";
        protected const string blessingOfKingsSpell = "Blessing of Kings";
        protected const string blessingOfMightSpell = "Blessing of Might";
        protected const string blessingOfWisdomSpell = "Blessing of Wisdom";
        protected const string consecrationSpell = "Consecration";
        protected const string crusaderStrikeSpell = "Crusader Strike";
        protected const string devotionAuraSpell = "Devotion Aura";
        protected const string divineFavorSpell = "Divine Favor";
        protected const string divineIlluminationSpell = "Divine Illumination";
        protected const string divinePleaSpell = "Divine Plea";
        protected const string divineStormSpell = "Divine Storm";
        protected const string exorcismSpell = "Exorcism";
        protected const string flashOfLightSpell = "Flash of Light";
        protected const string hammerOfJusticeSpell = "Hammer of Justice";
        protected const string hammerOfTheRighteousSpell = "Hammer of the Righteous";
        protected const string hammerOfWrathSpell = "Hammer of Wrath";
        protected const string handOfReckoningSpell = "Hand of Reckoning";
        protected const string holyLightSpell = "Holy Light";
        protected const string holyShieldSpell = "Holy Shield";
        protected const string holyShockSpell = "Holy Shock";
        protected const string holyWrathSpell = "Holy Wrath";
        protected const string judgementOfLightSpell = "Judgement of Light";
        protected const string layOnHandsSpell = "Lay on Hands";
        protected const string retributionAuraSpell = "Retribution Aura";
        protected const string righteousFurySpell = "Righteous Fury";
        protected const string sacredShieldSpell = "Sacred Shield";
        protected const string sealOfVengeanceSpell = "Seal of Vengeance";
        protected const string sealOfWisdomSpell = "Seal of Wisdom";
        protected const string shieldOfTheRighteousnessSpell = "Shield of the Righteousness";

        #endregion Paladin

        #region Priest

        protected const string bindingHealSpell = "Binding Heal";
        protected const string desperatePrayerSpell = "Desperate Prayer";
        protected const string devouringPlagueSpell = "Devouring Plague";
        protected const string flashHealSpell = "Flash Heal";
        protected const string greaterHealSpell = "Greater Heal";
        protected const string guardianSpiritSpell = "Guardian Spirit";
        protected const string healSpell = "Lesser Heal";
        protected const string hymnOfHopeSpell = "Hymn of Hope";
        protected const string innerFireSpell = "Inner Fire";
        protected const string mindBlastSpell = "Mind Blast";
        protected const string mindFlaySpell = "Mind Flay";
        protected const string penanceSpell = "Penance";
        protected const string powerWordFortitudeSpell = "Power Word: Fortitude";
        protected const string powerWordShieldSpell = "Power Word: Shield";
        protected const string prayerOfHealingSpell = "Prayer of Healing";
        protected const string prayerOfMendingSpell = "Prayer of Mending";
        protected const string renewSpell = "Renew";
        protected const string resurrectionSpell = "Resurrection";
        protected const string shadowfiendSpell = "Shadowfiend";
        protected const string shadowformSpell = "Shadowform";
        protected const string shadowWordPainSpell = "Shadow Word: Pain";
        protected const string smiteSpell = "Smite";
        protected const string vampiricEmbraceSpell = "Vampiric Embrace";
        protected const string vampiricTouchSpell = "Vampiric Touch";
        protected const string weakenedSoulSpell = "Weakened Soul";

        #endregion Priest

        #region Rogue

        protected const string backstabSpell = "Backstab";
        protected const string cloakOfShadowsSpell = "Cloak of Shadows";
        protected const string coldBloodSpell = "Cold Blood";
        protected const string eviscerateSpell = "Eviscerate";
        protected const string feintSpell = "Feint";
        protected const string hungerForBloodSpell = "Hunger for Blood";
        protected const string kickSpell = "Kick";
        protected const string mutilateSpell = "Mutilate";
        protected const string sinisterStrikeSpell = "Sinister Strike";
        protected const string sliceAndDiceSpell = "Slice and Dice";
        protected const string sprintSpell = "Sprint";
        protected const string stealthSpell = "Stealth";

        #endregion Rogue

        #region Shaman

        protected const string ancestralSpiritSpell = "Ancestral Spirit";
        protected const string chainHealSpell = "Chain Heal";
        protected const string chainLightningSpell = "Chain Lightning";
        protected const string earthlivingBuff = "Earthliving ";
        protected const string earthlivingWeaponSpell = "Earthliving Weapon";
        protected const string earthShieldSpell = "Earth Shield";
        protected const string earthShockSpell = "Earth Shock";
        protected const string elementalMasterySpell = "Elemental Mastery";
        protected const string feralSpiritSpell = "Feral Spirit";
        protected const string flameShockSpell = "Flame Shock";
        protected const string flametongueBuff = "Flametongue ";
        protected const string flametongueWeaponSpell = "Flametongue Weapon";
        protected const string flametoungueBuff = "Flametongue ";
        protected const string flametoungueWeaponSpell = "Flametongue Weapon";
        protected const string healingWaveSpell = "Healing Wave";
        protected const string heroismSpell = "Heroism";
        protected const string hexSpell = "Hex";
        protected const string lavaBurstSpell = "Lava Burst";
        protected const string lavaLashSpell = "Lava Lash";
        protected const string lesserHealingWaveSpell = "Lesser Healing Wave";
        protected const string lightningBoltSpell = "Lightning Bolt";
        protected const string lightningShieldSpell = "Lightning Shield";
        protected const string maelstromWeaponSpell = "Mealstrom Weapon";
        protected const string riptideSpell = "Riptide";
        protected const string shamanisticRageSpell = "Shamanistic Rage";
        protected const string stormstrikeSpell = "Stormstrike";
        protected const string thunderstormSpell = "Thunderstorm";
        protected const string tidalForceSpell = "Tidal Force";
        protected const string waterShieldSpell = "Water Shield";
        protected const string windfuryBuff = "Windfury";
        protected const string windfuryWeaponSpell = "Windfury Weapon";
        protected const string windShearSpell = "Wind Shear";

        #endregion Shaman

        #region Warlock

        protected const string chaosBoltSpell = "Chaos Bolt";
        protected const string conflagrateSpell = "Conflagrate";
        protected const string corruptionSpell = "Corruption";
        protected const string curseOfAgonySpell = "Curse of Agony";
        protected const string curseOfDoomSpell = "Curse of Doom";
        protected const string curseOfTheElementsSpell = "Curse of the Elements";
        protected const string curseOfTonguesSpell = "Curse of Tongues";
        protected const string decimationSpell = "Decimation";
        protected const string demonArmorSpell = "Demon Armor";
        protected const string demonicEmpowermentSpell = "Demonic Empowerment";
        protected const string demonSkinSpell = "Demon Skin";
        protected const string drainLifeSpell = "Drain Life";
        protected const string drainSoulSpell = "Drain Soul";
        protected const string fearSpell = "Fear";
        protected const string felArmorSpell = "Fel Armor";
        protected const string hauntSpell = "Haunt";
        protected const string howlOfTerrorSpell = "Howl of Terror";
        protected const string immolateSpell = "Immolate";
        protected const string immolationAuraSpell = "Immolation Aura";
        protected const string incinerateSpell = "Incinerate";
        protected const string lifeTapSpell = "Life Tap";
        protected const string metamorphosisSpell = "Metamorphosis";
        protected const string moltenCoreSpell = "Molten Core";
        protected const string seedOfCorruptionSpell = "Seed of Corruption";
        protected const string shadowBoltSpell = "Shadow Bolt";
        protected const string shadowMasterySpell = "Shadow Mastery";
        protected const string soulfireSpell = "Soul Fire";
        protected const string summonFelguardSpell = "Summon Felguard";
        protected const string summonFelhunterSpell = "Summon Felhunter";
        protected const string summonImpSpell = "Summon Imp";
        protected const string unstableAfflictionSpell = "Unstable Affliction";

        #endregion Warlock

        #region Warrior

        protected const string battleShoutSpell = "Battle Shout";
        protected const string battleStanceSpell = "Battle Stance";
        protected const string berserkerRageSpell = "Berserker Rage";
        protected const string berserkerStanceSpell = "Berserker Stance";
        protected const string bladestormSpell = "Bladestorm";
        protected const string bloodrageSpell = "Bloodrage";
        protected const string bloodthirstSpell = "Bloodthirst";
        protected const string challengingShoutSpell = "Challenging Shout";
        protected const string chargeSpell = "Charge";
        protected const string cleaveSpell = "Cleave";
        protected const string commandingShoutSpell = "Commanding Shout";
        protected const string concussionBlowSpell = "Concussion Blow";
        protected const string deathWishSpell = "Death Wish";
        protected const string defensiveStanceSpell = "Defensive Stance";
        protected const string demoralizingShoutSpell = "Demoralizing Shout";
        protected const string devastateSpell = "Devastate";
        protected const string disarmSpell = "Disarm";
        protected const string enragedRegenerationSpell = "Enraged Regeneration";
        protected const string executeSpell = "Execute";
        protected const string hamstringSpell = "Hamstring";
        protected const string heroicFurySpell = "Heroic Fury";
        protected const string heroicStrikeSpell = "Heroic Strike";
        protected const string heroicThrowSpell = "Heroic Throw";
        protected const string interceptSpell = "Intercept";
        protected const string intimidatingShoutSpell = "Intimidating Shout";
        protected const string lastStandSpell = "Last Stand";
        protected const string mockingBlowSpell = "Mocking Blow";
        protected const string mortalStrikeSpell = "Mortal Strike";
        protected const string overpowerSpell = "Overpower";
        protected const string pummelSpell = "Pummel";
        protected const string recklessnessSpell = "Recklessness";
        protected const string rendSpell = "Rend";
        protected const string retaliationSpell = "Retaliation";
        protected const string revengeSpell = "Revenge";
        protected const string shieldBashSpell = "Shield Bash";
        protected const string shieldBlockSpell = "Shield Block";
        protected const string shieldSlamSpell = "Shield Slam";
        protected const string shieldWallSpell = "Shield Wall";
        protected const string shockwaveSpell = "Shockwave";
        protected const string slamSpell = "Slam";
        protected const string spellReflectionSpell = "Spell Reflection";
        protected const string tauntSpell = "Taunt";
        protected const string thunderClapSpell = "Thunder Clap";
        protected const string victoryRushSpell = "Victory Rush";
        protected const string whirlwindSpell = "Whirlwind";

        #endregion Warrior

        #region Racials

        protected const string berserkingSpell = "Berserking"; // Troll
        protected const string bloodFurySpell = "Blood Fury"; // Orc

        #endregion Racials

        private readonly float maxAngle = (float)(Math.PI * 2.0);

        private readonly int[] useableHealingItems = new int[]
        {
            // potions
            118, 929, 1710, 2938, 3928, 4596, 5509, 13446, 22829, 33447,
            // healthstones
            5509, 5510, 5511, 5512, 9421, 19013, 22103, 36889, 36892,
        };

        private readonly int[] useableManaItems = new int[]
        {
            // potions
            2245, 3385, 3827, 6149, 13443, 13444, 33448, 22832,
        };

        protected BasicCombatClass(AmeisenBotInterfaces bot, AmeisenBotFsm stateMachine)
        {
            Bot = bot;
            StateMachine = stateMachine;

            C = new Dictionary<string, dynamic>()
            {
                { "HealingItemHealthThreshold", 30.0 },
                { "HealingItemManaThreshold", 30.0 }
            };

            CooldownManager = new CooldownManager(Bot.Character.SpellBook.Spells);
            RessurrectionTargets = new Dictionary<string, DateTime>();

            TargetManagerDps = new TargetManager(new DpsTargetSelectionLogic(bot), TimeSpan.FromMilliseconds(250));
            TargetManagerTank = new TargetManager(new TankTargetSelectionLogic(bot), TimeSpan.FromMilliseconds(250));
            TargetManagerHeal = new TargetManager(new HealTargetSelectionLogic(bot), TimeSpan.FromMilliseconds(250));

            MyAuraManager = new AuraManager(bot);
            TargetAuraManager = new AuraManager(bot);

            GroupAuraManager = new GroupAuraManager(bot);

            TargetInterruptManager = new InterruptManager();

            EventCheckFacing = new(TimeSpan.FromMilliseconds(500));
            EventAutoAttack = new(TimeSpan.FromMilliseconds(500));
        }

        public string Author { get; } = "ToadLump";

        public IEnumerable<int> BlacklistedTargetDisplayIds { get => TargetManagerDps.BlacklistedTargets; set => TargetManagerDps.BlacklistedTargets = value; }

        public Dictionary<string, dynamic> C { get; set; }

        public CooldownManager CooldownManager { get; private set; }

        public abstract string Description { get; }

        public abstract string Displayname { get; }

        public TimegatedEvent EventAutoAttack { get; private set; }

        public TimegatedEvent EventCheckFacing { get; set; }

        public GroupAuraManager GroupAuraManager { get; private set; }

        public bool HandlesFacing => false;

        public abstract bool HandlesMovement { get; }

        public abstract bool IsMelee { get; }

        public bool IsWanding { get; private set; } = false;

        public abstract IItemComparator ItemComparator { get; set; }

        public AuraManager MyAuraManager { get; private set; }

        public IEnumerable<int> PriorityTargetDisplayIds { get => TargetManagerDps.PriorityTargets; set => TargetManagerDps.PriorityTargets = value; }

        public Dictionary<string, DateTime> RessurrectionTargets { get; private set; }

        public abstract WowRole Role { get; }

        public abstract TalentTree Talents { get; }

        public AuraManager TargetAuraManager { get; private set; }

        public InterruptManager TargetInterruptManager { get; private set; }

        public ITargetProvider TargetManagerDps { get; private set; }

        public ITargetProvider TargetManagerHeal { get; private set; }

        public ITargetProvider TargetManagerTank { get; private set; }

        public abstract bool UseAutoAttacks { get; }

        public abstract string Version { get; }

        public abstract bool WalkBehindEnemy { get; }

        public abstract WowClass WowClass { get; }

        protected AmeisenBotInterfaces Bot { get; }

        private AmeisenBotFsm StateMachine { get; }

        public void AttackTarget()
        {
            IWowUnit target = Bot.Target;
            if (target == null)
            {
                return;
            }

            if (Bot.Player.Position.GetDistance(target.Position) <= 3.0)
            {
                Bot.Wow.WowStopClickToMove();
                Bot.Movement.Reset();
                Bot.Wow.WowUnitRightClick(target.BaseAddress);
            }
            else
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);
            }
        }

        public virtual void Execute()
        {
            if (Bot.Player.IsCasting)
            {
                if (!Bot.Objects.IsTargetInLineOfSight)
                {
                    Bot.Wow.LuaSpellStopCasting();
                }

                return;
            }

            if (Bot.Target != null && EventCheckFacing.Run())
            {
                CheckFacing(Bot.Target);
            }

            // Update Priority Units
            // --------------------------- >

            if (StateMachine.CurrentState.Key == BotState.Dungeon
                && Bot.Dungeon != null
                && Bot.Dungeon.Profile.PriorityUnits != null
                && Bot.Dungeon.Profile.PriorityUnits.Count > 0)
            {
                TargetManagerDps.PriorityTargets = Bot.Dungeon.Profile.PriorityUnits;
            }

            // Autoattacks
            // --------------------------- >
            if (UseAutoAttacks)
            {
                IsWanding = Bot.Character.SpellBook.IsSpellKnown("Shoot")
                    && Bot.Character.Equipment.Items.ContainsKey(WowEquipmentSlot.INVSLOT_RANGED)
                    && (WowClass == WowClass.Priest || WowClass == WowClass.Mage || WowClass == WowClass.Warlock)
                    && (IsWanding || TryCastSpell("Shoot", Bot.Wow.TargetGuid));

                if (!IsWanding
                    && EventAutoAttack.Run()
                    && !Bot.Player.IsAutoAttacking
                    && Bot.Player.IsInMeleeRange(Bot.Target))
                {
                    Bot.Wow.LuaStartAutoAttack();
                }
            }

            // Buffs, Debuffs, Interrupts
            // --------------------------- >

            if (TargetAuraManager.Tick(Bot.Target.Auras)
                || TargetInterruptManager.Tick(Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, IsMelee ? 5.0f : 30.0f).ToList()))
            {
                return;
            }

            // Useable items, potions, etc.
            // ---------------------------- >

            if (Bot.Player.HealthPercentage < C["HealingItemHealthThreshold"])
            {
                IWowInventoryItem healthItem = Bot.Character.Inventory.Items.FirstOrDefault(e => useableHealingItems.Contains(e.Id));

                if (healthItem != null)
                {
                    Bot.Wow.LuaUseItemByName(healthItem.Name);
                }
            }

            if (Bot.Player.ManaPercentage < C["HealingItemManaThreshold"])
            {
                IWowInventoryItem manaItem = Bot.Character.Inventory.Items.FirstOrDefault(e => useableManaItems.Contains(e.Id));

                if (manaItem != null)
                {
                    Bot.Wow.LuaUseItemByName(manaItem.Name);
                }
            }

            // Race abilities
            // -------------- >

            if (Bot.Player.Race == WowRace.Human
                && (Bot.Player.IsDazed
                    || Bot.Player.IsFleeing
                    || Bot.Player.IsInfluenced
                    || Bot.Player.IsPossessed)
                && TryCastSpell("Every Man for Himself", 0))
            {
                return;
            }

            if (Bot.Player.HealthPercentage < 50.0
                && ((Bot.Player.Race == WowRace.Draenei && TryCastSpell("Gift of the Naaru", 0))
                    || (Bot.Player.Race == WowRace.Dwarf && TryCastSpell("Stoneform", 0))))
            {
                return;
            }
        }

        public virtual void OutOfCombatExecute()
        {
            if ((Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Food") && Bot.Player.HealthPercentage < 100.0)
                || (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == "Drink") && Bot.Player.ManaPercentage < 100.0))
            {
                return;
            }

            if (MyAuraManager.Tick(Bot.Player.Auras)
                || GroupAuraManager.Tick())
            {
                return;
            }
        }

        public override string ToString()
        {
            return $"[{WowClass}] [{Role}] {Displayname} ({Author})";
        }

        protected bool CheckForWeaponEnchantment(WowEquipmentSlot slot, string enchantmentName, string spellToCastEnchantment)
        {
            if (Bot.Character.Equipment.Items.ContainsKey(slot))
            {
                int itemId = Bot.Character.Equipment.Items[slot].Id;

                if (itemId > 0)
                {
                    IWowItem item = Bot.Objects.WowObjects.OfType<IWowItem>().FirstOrDefault(e => e.EntryId == itemId);

                    if (item != null
                        && !item.GetEnchantmentStrings().Any(e => e.Contains(enchantmentName, StringComparison.OrdinalIgnoreCase))
                        && TryCastSpell(spellToCastEnchantment, 0, true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool HandleDeadPartymembers(string spellName)
        {
            Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

            if (spell != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && spell.Costs < Bot.Player.Mana)
            {
                IEnumerable<IWowPlayer> groupPlayers = Bot.Objects.Partymembers
                    .OfType<IWowPlayer>()
                    .Where(e => e.IsDead);

                if (groupPlayers.Any())
                {
                    IWowPlayer player = groupPlayers.FirstOrDefault(e => Bot.Db.GetUnitName(e, out string name) && !RessurrectionTargets.ContainsKey(name) || RessurrectionTargets[name] < DateTime.Now);

                    if (player != null)
                    {
                        if (Bot.Db.GetUnitName(player, out string name))
                        {
                            if (!RessurrectionTargets.ContainsKey(name))
                            {
                                RessurrectionTargets.Add(name, DateTime.Now + TimeSpan.FromSeconds(10));
                                return TryCastSpell(spellName, player.Guid, true);
                            }

                            if (RessurrectionTargets[name] < DateTime.Now)
                            {
                                return TryCastSpell(spellName, player.Guid, true);
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        protected bool SelectTarget(ITargetProvider targetProvider)
        {
            if (targetProvider.Get(out IEnumerable<IWowUnit> targetToTarget))
            {
                IWowUnit closestUnit = targetToTarget.OrderBy(value => value.Position.GetDistance(Bot.Player.Position)).First();
                ulong guid = closestUnit.Guid;

                if (Bot.Player.TargetGuid != guid)
                {
                    Bot.Wow.WowTargetGuid(guid);
                    Bot.Objects.Player.Update(Bot.Memory, Bot.Wow.Offsets);
                }
            }

            return Bot.Target != null
                && IWowUnit.IsValidUnit(Bot.Target)
                && !Bot.Target.IsDead;
        }

        protected bool TryCastAoeSpell(string spellName, ulong guid, bool needsResource = false, int currentResourceAmount = 0, bool forceTargetSwitch = false)
        {
            if (TryCastSpell(spellName, guid, needsResource, currentResourceAmount, forceTargetSwitch))
            {
                if (GetValidTarget(guid, out IWowUnit target, out bool _))
                {
                    Bot.Wow.WowClickOnTerrain(target.Position);
                    return true;
                }
            }

            return false;
        }

        protected bool TryCastAoeSpellDk(string spellName, ulong guid, bool needsRuneenergy = false, bool needsBloodrune = false, bool needsFrostrune = false, bool needsUnholyrune = false, bool forceTargetSwitch = false)
        {
            if (TryCastSpellDk(spellName, guid, needsRuneenergy, needsBloodrune, needsFrostrune, needsUnholyrune, forceTargetSwitch))
            {
                if (GetValidTarget(guid, out IWowUnit target, out bool _))
                {
                    Bot.Wow.WowClickOnTerrain(target.Position);
                    return true;
                }
            }

            return false;
        }

        protected bool TryCastSpell(string spellName, ulong guid, bool needsResource = false, int currentResourceAmount = 0, bool forceTargetSwitch = false)
        {
            if (!Bot.Character.SpellBook.IsSpellKnown(spellName) || ((guid != 0 && guid != Bot.Wow.PlayerGuid) && !Bot.Objects.IsTargetInLineOfSight)) { return false; }

            if (GetValidTarget(guid, out IWowUnit target, out bool needToSwitchTarget))
            {
                if (currentResourceAmount == 0)
                {
                    currentResourceAmount = Bot.Player.Class switch
                    {
                        WowClass.Deathknight => Bot.Player.Runeenergy,
                        WowClass.Rogue => Bot.Player.Energy,
                        WowClass.Warrior => Bot.Player.Rage,
                        _ => Bot.Player.Mana,
                    };
                }

                bool isTargetMyself = guid == 0;
                Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                if (spell != null
                    && !CooldownManager.IsSpellOnCooldown(spellName)
                    && (!needsResource || spell.Costs < currentResourceAmount)
                    && (target == null || IsInRange(spell, target)))
                {
                    if (!isTargetMyself && (needToSwitchTarget || forceTargetSwitch))
                    {
                        Bot.Wow.WowTargetGuid(guid);
                    }

                    if (spell.CastTime > 0)
                    {
                        // stop pending movement if we cast something
                        Bot.Movement.PreventMovement(TimeSpan.FromMilliseconds(spell.CastTime));
                        CheckFacing(target);
                    }

                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        protected bool TryCastSpellDk(string spellName, ulong guid, bool needsRuneenergy = false, bool needsBloodrune = false, bool needsFrostrune = false, bool needsUnholyrune = false, bool forceTargetSwitch = false)
        {
            if (!Bot.Character.SpellBook.IsSpellKnown(spellName) || ((guid != 0 && guid != Bot.Wow.PlayerGuid) && !Bot.Objects.IsTargetInLineOfSight)) { return false; }

            if (GetValidTarget(guid, out IWowUnit target, out bool needToSwitchTarget))
            {
                bool isTargetMyself = guid == 0;
                Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);
                Dictionary<int, int> runes = Bot.Wow.WowGetRunesReady();

                if (spell != null
                    && !CooldownManager.IsSpellOnCooldown(spellName)
                    && (!needsRuneenergy || spell.Costs < Bot.Player.Runeenergy)
                    && (!needsBloodrune || (runes[(int)WowRuneType.Blood] > 0 || runes[(int)WowRuneType.Death] > 0))
                    && (!needsFrostrune || (runes[(int)WowRuneType.Frost] > 0 || runes[(int)WowRuneType.Death] > 0))
                    && (!needsUnholyrune || (runes[(int)WowRuneType.Unholy] > 0 || runes[(int)WowRuneType.Death] > 0))
                    && (target == null || IsInRange(spell, target)))
                {
                    if (!isTargetMyself && (needToSwitchTarget || forceTargetSwitch))
                    {
                        Bot.Wow.WowTargetGuid(guid);
                    }

                    if (spell.CastTime > 0)
                    {
                        // stop pending movement if we cast something
                        Bot.Movement.PreventMovement(TimeSpan.FromMilliseconds(spell.CastTime));
                        CheckFacing(target);
                    }

                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        protected bool TryCastSpellRogue(string spellName, ulong guid, bool needsEnergy = false, bool needsCombopoints = false, int requiredCombopoints = 1, bool forceTargetSwitch = false)
        {
            if (!Bot.Character.SpellBook.IsSpellKnown(spellName) || ((guid != 0 && guid != Bot.Wow.PlayerGuid) && !Bot.Objects.IsTargetInLineOfSight)) { return false; }

            if (GetValidTarget(guid, out IWowUnit target, out bool needToSwitchTarget))
            {
                bool isTargetMyself = guid == 0;
                Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                if (spell != null
                    && !CooldownManager.IsSpellOnCooldown(spellName)
                    && (!needsEnergy || spell.Costs < Bot.Player.Energy)
                    && (!needsCombopoints || Bot.Player.ComboPoints >= requiredCombopoints)
                    && (target == null || IsInRange(spell, target)))
                {
                    if (!isTargetMyself && (needToSwitchTarget || forceTargetSwitch))
                    {
                        Bot.Wow.WowTargetGuid(guid);
                    }

                    if (spell.CastTime > 0)
                    {
                        // stop pending movement if we cast something
                        Bot.Movement.PreventMovement(TimeSpan.FromMilliseconds(spell.CastTime));
                        CheckFacing(target);
                    }

                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        protected bool TryCastSpellWarrior(string spellName, string requiredStance, ulong guid, bool needsResource = false, int currentResourceAmount = 0, bool forceTargetSwitch = false)
        {
            if (!Bot.Character.SpellBook.IsSpellKnown(spellName) || ((guid != 0 && guid != Bot.Wow.PlayerGuid) && !Bot.Objects.IsTargetInLineOfSight)) { return false; }

            if (GetValidTarget(guid, out IWowUnit target, out bool needToSwitchTarget))
            {
                if (currentResourceAmount == 0)
                {
                    currentResourceAmount = Bot.Player.Rage;
                }

                bool isTargetMyself = guid == 0;
                Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                if (spell != null
                    && !CooldownManager.IsSpellOnCooldown(spellName)
                    && (!needsResource || spell.Costs < currentResourceAmount)
                    && (target == null || IsInRange(spell, target)))
                {
                    if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == requiredStance)
                        && Bot.Character.SpellBook.IsSpellKnown(requiredStance)
                        && !CooldownManager.IsSpellOnCooldown(requiredStance))
                    {
                        CastSpell(requiredStance, true);
                    }

                    if (!isTargetMyself && (needToSwitchTarget || forceTargetSwitch))
                    {
                        Bot.Wow.WowTargetGuid(guid);
                    }

                    if (spell.CastTime > 0)
                    {
                        // stop pending movement if we cast something
                        Bot.Movement.PreventMovement(TimeSpan.FromMilliseconds(spell.CastTime));
                        CheckFacing(target);
                    }

                    return CastSpell(spellName, isTargetMyself);
                }
            }

            return false;
        }

        private bool CastSpell(string spellName, bool castOnSelf)
        {
            // spits out stuff like this "1;300" (1 or 0 whether the cast was successful or not);(the cooldown in ms)
            if (Bot.Wow.WowExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:3}},{{v:4}}=GetSpellCooldown(\"{spellName}\"){{v:2}}=({{v:3}}+{{v:4}}-GetTime())*1000;if {{v:2}}<=0 then {{v:2}}=0;CastSpellByName(\"{spellName}\"{(castOnSelf ? ", \"player\"" : string.Empty)}){{v:5}},{{v:6}}=GetSpellCooldown(\"{spellName}\"){{v:1}}=({{v:5}}+{{v:6}}-GetTime())*1000;{{v:0}}=\"1;\"..{{v:1}} else {{v:0}}=\"0;\"..{{v:2}} end"), out string result))
            {
                if (result.Length < 3)
                {
                    return false;
                }

                string[] parts = result.Split(";", StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 2)
                {
                    return false;
                }

                // replace comma with dot in the cooldown
                if (parts[1].Contains(',', StringComparison.OrdinalIgnoreCase))
                {
                    parts[1] = parts[1].Replace(',', '.');
                }

                if (int.TryParse(parts[0], out int castSuccessful)
                    && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double cooldown))
                {
                    cooldown = Math.Max(cooldown, 0);
                    CooldownManager.SetSpellCooldown(spellName, (int)cooldown);

                    if (castSuccessful == 1)
                    {
                        AmeisenLogger.I.Log("CombatClass", $"[{Displayname}]: Casting Spell \"{spellName}\" on \"{Bot.Target?.Guid}\"", LogLevel.Verbose);
                        IsWanding = IsWanding && spellName == "Shoot";
                        return true;
                    }
                    else
                    {
                        AmeisenLogger.I.Log("CombatClass", $"[{Displayname}]: Spell \"{spellName}\" is on cooldown for \"{cooldown}\"ms", LogLevel.Verbose);
                        return false;
                    }
                }
            }

            return false;
        }

        private void CheckFacing(IWowUnit target)
        {
            if (target == null || target.Guid == Bot.Wow.PlayerGuid)
            {
                return;
            }

            float facingAngle = BotMath.GetFacingAngle(Bot.Player.Position, target.Position);
            float angleDiff = facingAngle - Bot.Player.Rotation;

            if (angleDiff < 0)
            {
                angleDiff += maxAngle;
            }

            if (angleDiff > maxAngle)
            {
                angleDiff -= maxAngle;
            }

            if (angleDiff > 1.0)
            {
                Bot.Wow.WowFacePosition(Bot.Player.BaseAddress, Bot.Player.Position, target.Position);
            }
        }

        private bool GetValidTarget(ulong guid, out IWowUnit target, out bool needToSwitchTargets)
        {
            if (guid == 0)
            {
                target = Bot.Player;
                needToSwitchTargets = false;
                return true;
            }
            else if (guid == Bot.Wow.TargetGuid)
            {
                target = Bot.Target;
                needToSwitchTargets = false;
                return true;
            }
            else
            {
                target = Bot.GetWowObjectByGuid<IWowUnit>(guid);
                needToSwitchTargets = true;
                return target != null;
            }
        }

        private bool IsInRange(Spell spell, IWowUnit wowUnit)
        {
            if ((spell.MinRange == 0 && spell.MaxRange == 0) || spell.MaxRange == 0)
            {
                return Bot.Player.IsInMeleeRange(wowUnit);
            }

            double distance = Bot.Player.Position.GetDistance(wowUnit.Position);
            return distance >= spell.MinRange && distance <= spell.MaxRange;
        }
    }
}