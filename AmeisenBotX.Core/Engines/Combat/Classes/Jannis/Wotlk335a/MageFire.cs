using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis.Wotlk335a
{
    public class MageFire : BasicCombatClass
    {
        public MageFire(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.ArcaneIntellect, () => TryCastSpell(Mage335a.ArcaneIntellect, Bot.Wow.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.MoltenArmor, () => TryCastSpell(Mage335a.MoltenArmor, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.ManaShield, () => TryCastSpell(Mage335a.ManaShield, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.Scorch, () => TryCastSpell(Mage335a.Scorch, Bot.Wow.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.LivingBomb, () => TryCastSpell(Mage335a.LivingBomb, Bot.Wow.TargetGuid, true)));

            // TargetAuraManager.DispellBuffs = () =>
            // Bot.NewBot.LuaHasUnitStealableBuffs(WowLuaUnit.Target) && TryCastSpell(spellSteal,
            // Bot.NewBot.TargetGuid, true);

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(Mage335a.Counterspell, x.Guid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((Mage335a.ArcaneIntellect, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Fire Mage spec.";

        public override string DisplayName2 => "Mage Fire";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.Shield }, new() { WowWeaponType.Sword, WowWeaponType.Mace, WowWeaponType.Axe });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 2) },
                { 2, new(1, 2, 3) },
                { 6, new(1, 6, 5) },
                { 8, new(1, 8, 3) },
                { 9, new(1, 9, 1) },
                { 10, new(1, 10, 1) },
                { 14, new(1, 14, 3) },
            },
            Tree2 = new()
            {
                { 3, new(2, 3, 5) },
                { 4, new(2, 4, 5) },
                { 6, new(2, 6, 3) },
                { 7, new(2, 7, 2) },
                { 9, new(2, 9, 1) },
                { 10, new(2, 10, 2) },
                { 11, new(2, 11, 3) },
                { 13, new(2, 13, 3) },
                { 14, new(2, 14, 3) },
                { 15, new(2, 15, 3) },
                { 18, new(2, 18, 5) },
                { 19, new(2, 19, 3) },
                { 20, new(2, 20, 1) },
                { 21, new(2, 21, 2) },
                { 23, new(2, 23, 3) },
                { 27, new(2, 27, 5) },
                { 28, new(2, 28, 1) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Mage;

        public override WowVersion WowVersion => WowVersion.WotLK335a;

        public override void Execute()
        {
            base.Execute();

            if (FindTarget(TargetProviderDps))
            {
                if (Bot.Target != null)
                {
                    if (TryCastSpell(Mage335a.MirrorImage, Bot.Wow.TargetGuid, true)
                        || (Bot.Player.HealthPercentage < 16 && TryCastSpell(Mage335a.IceBlock, 0, true))
                        || (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId).ToLower() == Mage335a.Hotstreak.ToLower()) && TryCastSpell(Mage335a.Pyroblast, Bot.Wow.TargetGuid, true))
                        || (Bot.Player.ManaPercentage < 40 && TryCastSpell(Mage335a.Evocation, Bot.Wow.TargetGuid, true))
                        || TryCastSpell(Mage335a.Fireball, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }
    }
}