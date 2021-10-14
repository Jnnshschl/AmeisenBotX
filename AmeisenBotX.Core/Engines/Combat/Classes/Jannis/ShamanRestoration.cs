using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class ShamanRestoration : BasicCombatClass
    {
        public ShamanRestoration(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Shaman335a.WaterShield, () => TryCastSpell(Shaman335a.WaterShield, 0, true)));

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, Shaman335a.Riptide },
                { 5000, Shaman335a.HealingWave },
            };
        }

        public override string Description => "FCFS based CombatClass for the Restoration Shaman spec.";

        public override string DisplayName => "Shaman Restoration";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new() { WowArmorType.Shield });

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

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Shaman;

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override void Execute()
        {
            base.Execute();

            if (NeedToHealSomeone())
            {
                return;
            }

            if (SelectTarget(TargetProviderDps))
            {
                if (Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Shaman335a.FlameShock)
                    && TryCastSpell(Shaman335a.FlameShock, Bot.Wow.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(Shaman335a.LightningBolt, Bot.Wow.TargetGuid, true))
                {
                    return;
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

            if (CheckForWeaponEnchantment(WowEquipmentSlot.INVSLOT_MAINHAND, Shaman335a.EarthlivingBuff, Shaman335a.EarthlivingWeapon))
            {
                return;
            }
        }

        private bool NeedToHealSomeone()
        {
            if (TargetProviderHeal.Get(out IEnumerable<IWowUnit> unitsToHeal))
            {
                Bot.Wow.ChangeTarget(unitsToHeal.First().Guid);

                if (Bot.Target != null)
                {
                    if (Bot.Target.HealthPercentage < 25
                        && TryCastSpell(Shaman335a.EarthShield, 0, true))
                    {
                        return true;
                    }

                    if (unitsToHeal.Count() > 4
                        && TryCastSpell(Shaman335a.ChainHeal, Bot.Wow.TargetGuid, true))
                    {
                        return true;
                    }

                    if (unitsToHeal.Count() > 6
                        && (TryCastSpell(Shaman335a.NaturesSwiftness, 0, true)
                        || TryCastSpell(Shaman335a.TidalForce, Bot.Wow.TargetGuid, true)))
                    {
                        return true;
                    }

                    double healthDifference = Bot.Target.MaxHealth - Bot.Target.Health;
                    List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                    foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                    {
                        if (TryCastSpell(keyValuePair.Value, Bot.Wow.TargetGuid, true))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}