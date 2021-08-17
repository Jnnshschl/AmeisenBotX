using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Logic.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class MageArcane : BasicCombatClass
    {
        public MageArcane(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, arcaneIntellectSpell, () => TryCastSpell(arcaneIntellectSpell, Bot.Wow.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, mageArmorSpell, () => TryCastSpell(mageArmorSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, manaShieldSpell, () => TryCastSpell(manaShieldSpell, 0, true)));

            // TargetAuraManager.DispellBuffs = () => Bot.NewBot.LuaHasUnitStealableBuffs(WowLuaUnit.Target) && TryCastSpell(spellStealSpell, Bot.NewBot.TargetGuid, true);

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(counterspellSpell, x.Guid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((arcaneIntellectSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Arcane Mage spec.";

        public override string Displayname => "Mage Arcane";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicIntellectComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

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
                    if ((Bot.Player.HealthPercentage < 16
                            && TryCastSpell(iceBlockSpell, 0))
                        || (Bot.Player.ManaPercentage < 40
                            && TryCastSpell(evocationSpell, 0, true))
                        || TryCastSpell(mirrorImageSpell, Bot.Wow.TargetGuid, true)
                        || (Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == missileBarrageSpell) && TryCastSpell(arcaneMissilesSpell, Bot.Wow.TargetGuid, true))
                        || TryCastSpell(arcaneBarrageSpell, Bot.Wow.TargetGuid, true)
                        || TryCastSpell(arcaneBlastSpell, Bot.Wow.TargetGuid, true)
                        || TryCastSpell(fireballSpell, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }
    }
}