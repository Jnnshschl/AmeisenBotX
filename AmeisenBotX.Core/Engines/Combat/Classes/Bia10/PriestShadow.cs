using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Bia10
{
    public class PriestShadow : BasicCombatClassBia10
    {
        public PriestShadow(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Priest335a.PowerWordFortitude, () =>
                Bot.Player.ManaPercentage > 60.0
                && ValidateSpell(Priest335a.PowerWordFortitude, true)
                && TryCastSpell(Priest335a.PowerWordFortitude, Bot.Player.Guid)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Priest335a.PowerWordShield, () =>
                Bot.Player.Auras.All(e => Bot.Db.GetSpellName(e.SpellId) != "Weakened Soul")
                && Bot.Player.ManaPercentage > 60.0
                && ValidateSpell(Priest335a.PowerWordShield, true)
                && TryCastSpell(Priest335a.PowerWordShield, Bot.Player.Guid)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Priest335a.InnerFire, () =>
                Bot.Player.ManaPercentage > 60.0
                && ValidateSpell(Priest335a.InnerFire, true)
                && TryCastSpell(Priest335a.InnerFire, Bot.Player.Guid)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Priest335a.Renew, () =>
                Bot.Player.Auras.All(e => Bot.Db.GetSpellName(e.SpellId) != Priest335a.Renew)
                && Bot.Player.HealthPercentage < 85 && Bot.Player.ManaPercentage > 65.0
                && ValidateSpell(Priest335a.Renew, true)
                && TryCastSpell(Priest335a.Renew, Bot.Player.Guid)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Priest335a.ShadowWordPain, () =>
                Bot.Target?.HealthPercentage >= 5
                && ValidateSpell(Priest335a.ShadowWordPain, true)
                && TryCastSpell(Priest335a.ShadowWordPain, Bot.Wow.TargetGuid)));
        }

        public override string Description => "CombatClass for the Shadow Priest spec.";

        public override string DisplayName => "Shadow Priest";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } =
            new BasicIntellectComparator(null, new List<WowWeaponType>
            {
                WowWeaponType.AxeTwoHand,
                WowWeaponType.MaceTwoHand,
                WowWeaponType.SwordTwoHand
            });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new(),
            Tree2 = new(),
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Priest;

        public override void Execute()
        {
            base.Execute();

            string spellName = SelectSpell(out ulong targetGuid);
            TryCastSpell(spellName, targetGuid);
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
            HandleDeadPartyMembers(Priest335a.Resurrection);
        }

        private string SelectSpell(out ulong targetGuid)
        {
            if (Bot.Player.HealthPercentage < DataConstants.HealSelfPercentage
                && ValidateSpell(Priest335a.LesserHeal, true))
            {
                targetGuid = Bot.Player.Guid;
                return Priest335a.LesserHeal;
            }
            if (IsInSpellRange(Bot.Target, Priest335a.MindBlast)
                && ValidateSpell(Priest335a.MindBlast, true))
            {
                targetGuid = Bot.Target.Guid;
                return Priest335a.MindBlast;
            }
            if (IsInSpellRange(Bot.Target, Priest335a.Smite)
                && ValidateSpell(Priest335a.Smite, true))
            {
                targetGuid = Bot.Target.Guid;
                return Priest335a.Smite;
            }

            targetGuid = 9999999;
            return string.Empty;
        }
    }
}