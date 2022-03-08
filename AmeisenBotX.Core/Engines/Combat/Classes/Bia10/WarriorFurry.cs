using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Bia10
{
    public class WarrirorFurry : BasicCombatClassBia10
    {
        public WarrirorFurry(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Warrior335a.BattleShout, () =>
                Bot.Player.Auras.All(e => Bot.Db.GetSpellName(e.SpellId) != Warrior335a.BattleShout)
                && Bot.Player.Rage > 10.0
                && ValidateSpell(Warrior335a.BattleShout, true)
                && TryCastSpell(Warrior335a.BattleShout, Bot.Player.Guid)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Warrior335a.Rend, () =>
                Bot.Target?.HealthPercentage >= 10
                && ValidateSpell(Warrior335a.Rend, true)
                && TryCastSpell(Warrior335a.Rend, Bot.Wow.TargetGuid)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Warrior335a.ThunderClap, () =>
                Bot.Target.Auras.All(e => Bot.Db.GetSpellName(e.SpellId) != Warrior335a.ThunderClap)
                && Bot.Player.Auras.All(e => Bot.Db.GetSpellName(e.SpellId) == Warrior335a.BattleShout)
                && Bot.Target?.HealthPercentage >= 10 && Bot.Player.Rage > 30
                && ValidateSpell(Warrior335a.ThunderClap, true)
                && TryCastSpell(Warrior335a.ThunderClap, Bot.Wow.TargetGuid)));
        }

        public override string Version => "1.0";
        public override string Description => "CombatClass for the Warrior Furry spec.";
        public override string DisplayName => "Warrior Furry";
        public override bool HandlesMovement => false;
        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } =
            new BasicStrengthComparator(null, new List<WowWeaponType>());

        public override WowClass WowClass => WowClass.Warrior;
        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new Dictionary<int, Talent>(),
            Tree2 = new Dictionary<int, Talent>(),
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override bool UseAutoAttacks => true;
        public override bool WalkBehindEnemy => false;

        public override void Execute()
        {
            base.Execute();

            string spellName = SelectSpell(out ulong targetGuid);
            TryCastSpell(spellName, targetGuid);
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
        }

        private string SelectSpell(out ulong targetGuid)
        {
            // todo: bot doesn't understand the condition
            if (Bot.Player.Rage < 15 && IsInSpellRange(Bot.Target, Warrior335a.VictoryRush)
                && ValidateSpell(Warrior335a.VictoryRush, true))
            {
                targetGuid = Bot.Target.Guid;
                return Warrior335a.VictoryRush;
            }
            if (IsInSpellRange(Bot.Target, Warrior335a.HeroicStrike)
                && ValidateSpell(Warrior335a.HeroicStrike, true))
            {
                targetGuid = Bot.Target.Guid;
                return Warrior335a.HeroicStrike;
            }
            if (IsInSpellRange(Bot.Target, Warrior335a.Charge)
                && ValidateSpell(Warrior335a.Charge, true))
            {
                targetGuid = Bot.Target.Guid;
                return Warrior335a.Charge;
            }

            targetGuid = 9999999;
            return string.Empty;
        }
    }
}