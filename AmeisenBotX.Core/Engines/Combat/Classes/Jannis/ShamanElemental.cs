using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Logic.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class ShamanElemental : BasicCombatClass
    {
        public ShamanElemental(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Shaman335a.LightningShield, () => Bot.Player.ManaPercentage > 60.0 && TryCastSpell(Shaman335a.LightningShield, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Shaman335a.WaterShield, () => Bot.Player.ManaPercentage < 20.0 && TryCastSpell(Shaman335a.WaterShield, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Shaman335a.FlameShock, () => TryCastSpell(Shaman335a.FlameShock, Bot.Wow.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(Shaman335a.WindShear, x.Guid, true) },
                { 1, (x) => TryCastSpell(Shaman335a.Hex, x.Guid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Elemental Shaman spec.";

        public override string DisplayName => "Shaman Elemental";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(null, new() { WowWeaponType.AxeTwoHand, WowWeaponType.MaceTwoHand, WowWeaponType.SwordTwoHand });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 3) },
                { 2, new(1, 2, 5) },
                { 3, new(1, 3, 3) },
                { 7, new(1, 7, 1) },
                { 8, new(1, 8, 5) },
                { 9, new(1, 9, 2) },
                { 10, new(1, 10, 3) },
                { 11, new(1, 11, 2) },
                { 12, new(1, 12, 1) },
                { 13, new(1, 13, 3) },
                { 14, new(1, 14, 3) },
                { 15, new(1, 15, 5) },
                { 16, new(1, 16, 1) },
                { 17, new(1, 17, 3) },
                { 18, new(1, 18, 2) },
                { 19, new(1, 19, 2) },
                { 20, new(1, 20, 3) },
                { 22, new(1, 22, 1) },
                { 23, new(1, 23, 3) },
                { 24, new(1, 24, 5) },
                { 25, new(1, 25, 1) },
            },
            Tree2 = new()
            {
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 8, new(2, 8, 3) },
                { 9, new(2, 9, 1) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Shaman;

        private bool HexedTarget { get; set; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if (Bot.Player.HealthPercentage < 30
                && Bot.Target.Type == WowObjectType.Player
                && TryCastSpell(Shaman335a.Hex, Bot.Wow.TargetGuid, true))
                {
                    HexedTarget = true;
                    return;
                }

                if (Bot.Player.HealthPercentage < 60
                    && TryCastSpell(Shaman335a.HealingWave, Bot.Wow.PlayerGuid, true))
                {
                    return;
                }

                if (Bot.Target != null)
                {
                    if ((Bot.Target.Position.GetDistance(Bot.Player.Position) < 6
                            && TryCastSpell(Shaman335a.Thunderstorm, Bot.Wow.TargetGuid, true))
                        || (Bot.Target.MaxHealth > 10000000
                            && Bot.Target.HealthPercentage < 25
                            && TryCastSpell(Shaman335a.Heroism, 0))
                        || TryCastSpell(Shaman335a.LavaBurst, Bot.Wow.TargetGuid, true)
                        || TryCastSpell(Shaman335a.ElementalMastery, 0))
                    {
                        return;
                    }

                    if ((Bot.Objects.WowObjects.OfType<IWowUnit>().Where(e => Bot.Target.Position.GetDistance(e.Position) < 16).Count() > 2 && TryCastSpell(Shaman335a.ChainLightning, Bot.Wow.TargetGuid, true))
                        || TryCastSpell(Shaman335a.LightningBolt, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (HandleDeadPartymembers(Shaman335a.AncestralSpirit))
            {
                return;
            }

            if (CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_MAINHAND, Shaman335a.FlametongueBuff, Shaman335a.FlametongueWeapon))
            {
                return;
            }

            HexedTarget = false;
        }
    }
}