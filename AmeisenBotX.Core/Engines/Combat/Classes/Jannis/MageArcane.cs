using AmeisenBotX.Core.Logic.Utils.Auras.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class MageArcane : BasicCombatClass
    {
        public MageArcane(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.ArcaneIntellect, () => TryCastSpell(Mage335a.ArcaneIntellect, Bot.Wow.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.MageArmor, () => TryCastSpell(Mage335a.MageArmor, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Mage335a.ManaShield, () => TryCastSpell(Mage335a.ManaShield, 0, true)));

            // TargetAuraManager.DispellBuffs = () => Bot.NewBot.LuaHasUnitStealableBuffs(WowLuaUnit.Target) && TryCastSpell(spellSteal, Bot.NewBot.TargetGuid, true);

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(Mage335a.Counterspell, x.Guid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((Mage335a.ArcaneIntellect, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Arcane Mage spec.";

        public override string DisplayName => "Mage Arcane";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.Shield }, new() { WowWeaponType.Sword, WowWeaponType.Mace, WowWeaponType.Axe });

        public DateTime LastSpellstealCheck { get; private set; }

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 2) },
                { 2, new(1, 2, 3) },
                { 3, new(1, 3, 5) },
                { 6, new(1, 6, 5) },
                { 8, new(1, 8, 2) },
                { 9, new(1, 9, 1) },
                { 10, new(1, 10, 1) },
                { 13, new(1, 13, 2) },
                { 14, new(1, 14, 3) },
                { 16, new(1, 16, 1) },
                { 17, new(1, 17, 5) },
                { 19, new(1, 19, 3) },
                { 20, new(1, 20, 2) },
                { 23, new(1, 23, 3) },
                { 24, new(1, 24, 1) },
                { 25, new(1, 25, 5) },
                { 27, new(1, 27, 5) },
                { 28, new(1, 28, 3) },
                { 29, new(1, 29, 2) },
                { 30, new(1, 30, 1) },
            },
            Tree2 = new()
            {
                { 2, new(2, 2, 3) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 2) },
                { 3, new(3, 3, 3) },
                { 5, new(3, 5, 2) },
                { 6, new(3, 6, 3) },
                { 9, new(3, 9, 1) },
            },
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Mage;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if (Bot.Target != null)
                {
                    if ((Bot.Player.HealthPercentage < 16.0 && TryCastSpell(Mage335a.IceBlock, 0))
                        || (Bot.Player.ManaPercentage < 40.0 && TryCastSpell(Mage335a.Evocation, 0, true))
                        || TryCastSpell(Mage335a.MirrorImage, Bot.Wow.TargetGuid, true)
                        || (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Mage335a.MissileBarrage) && TryCastSpell(Mage335a.ArcaneMissiles, Bot.Wow.TargetGuid, true))
                        || TryCastSpell(Mage335a.ArcaneBarrage, Bot.Wow.TargetGuid, true)
                        || TryCastSpell(Mage335a.ArcaneBlast, Bot.Wow.TargetGuid, true)
                        || TryCastSpell(Mage335a.Fireball, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }
    }
}