using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Logic.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class ShamanEnhancement : BasicCombatClass
    {
        public ShamanEnhancement(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, lightningShieldSpell, () => Bot.Player.ManaPercentage > 60.0 && TryCastSpell(lightningShieldSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, waterShieldSpell, () => Bot.Player.ManaPercentage < 20.0 && TryCastSpell(waterShieldSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, flameShockSpell, () => TryCastSpell(flameShockSpell, Bot.Wow.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(windShearSpell, x.Guid, true) },
                { 1, (x) => TryCastSpell(hexSpell, x.Guid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Enhancement Shaman spec.";

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

        private bool HexedTarget { get; set; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if (CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_MAINHAND, flametoungueBuff, flametoungueWeaponSpell)
                    || CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_OFFHAND, windfuryBuff, windfuryWeaponSpell))
                {
                    return;
                }

                if (Bot.Player.HealthPercentage < 30
                    && Bot.Target.Type == WowObjectType.Player
                    && TryCastSpell(hexSpell, Bot.Wow.TargetGuid, true))
                {
                    HexedTarget = true;
                    return;
                }

                if (Bot.Player.HealthPercentage < 60
                    && TryCastSpell(healingWaveSpell, Bot.Wow.PlayerGuid, true))
                {
                    return;
                }

                if (Bot.Target != null)
                {
                    if ((Bot.Target.MaxHealth > 10000000
                            && Bot.Target.HealthPercentage < 25
                            && TryCastSpell(heroismSpell, 0))
                        || TryCastSpell(stormstrikeSpell, Bot.Wow.TargetGuid, true)
                        || TryCastSpell(lavaLashSpell, Bot.Wow.TargetGuid, true)
                        || TryCastSpell(earthShockSpell, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    if (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == maelstromWeaponSpell)
                        && Bot.Player.Auras.FirstOrDefault(e => Bot.Db.GetSpellName(e.SpellId) == maelstromWeaponSpell).StackCount >= 5
                        && TryCastSpell(lightningBoltSpell, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (HandleDeadPartymembers(ancestralSpiritSpell))
            {
                return;
            }

            if (CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_MAINHAND, flametoungueBuff, flametoungueWeaponSpell)
                || CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_OFFHAND, windfuryBuff, windfuryWeaponSpell))
            {
                return;
            }

            HexedTarget = false;
        }
    }
}