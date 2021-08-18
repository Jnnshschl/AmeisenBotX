using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class PaladinProtection : BasicCombatClass
    {
        public PaladinProtection(AmeisenBotInterfaces bot, AmeisenBotFsm stateMachine) : base(bot, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, devotionAuraSpell, () => TryCastSpell(devotionAuraSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, blessingOfKingsSpell, () => TryCastSpell(blessingOfKingsSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, sealOfVengeanceSpell, () => TryCastSpell(sealOfVengeanceSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, righteousFurySpell, () => TryCastSpell(righteousFurySpell, 0, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(hammerOfJusticeSpell, x.Guid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((blessingOfKingsSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Protection Paladin spec.";

        public override string DisplayName => "Paladin Protection";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicArmorComparator(null, new() { WowWeaponType.TWOHANDED_SWORDS, WowWeaponType.TWOHANDED_MACES, WowWeaponType.TWOHANDED_AXES });

        public override WowRole Role => WowRole.Tank;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new(),
            Tree2 = new()
            {
                { 2, new(2, 2, 5) },
                { 5, new(2, 5, 5) },
                { 6, new(2, 6, 1) },
                { 7, new(2, 7, 3) },
                { 8, new(2, 8, 5) },
                { 9, new(2, 9, 2) },
                { 11, new(2, 11, 3) },
                { 12, new(2, 12, 1) },
                { 14, new(2, 14, 2) },
                { 15, new(2, 15, 3) },
                { 16, new(2, 16, 1) },
                { 17, new(2, 17, 1) },
                { 18, new(2, 18, 3) },
                { 19, new(2, 19, 3) },
                { 20, new(2, 20, 3) },
                { 21, new(2, 21, 3) },
                { 22, new(2, 22, 1) },
                { 23, new(2, 23, 2) },
                { 24, new(2, 24, 3) },
                { 25, new(2, 25, 2) },
                { 26, new(2, 26, 1) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 5) },
                { 3, new(3, 3, 2) },
                { 4, new(3, 4, 3) },
                { 7, new(3, 7, 5) },
                { 12, new(3, 12, 3) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Paladin;

        private bool Use9SecSpell { get; set; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderTank))
            {
                if (Bot.Player.HealthPercentage < 10.0
                    && TryCastSpell(layOnHandsSpell, 0, true))
                {
                    return;
                }

                if (Bot.Player.HealthPercentage < 20.0
                    && TryCastSpell(flashOfLightSpell, 0, true))
                {
                    return;
                }
                else if (Bot.Player.HealthPercentage < 35.0
                    && TryCastSpell(holyLightSpell, 0, true))
                {
                    return;
                }

                if (TryCastSpell(sacredShieldSpell, 0, true)
                    || TryCastSpell(divinePleaSpell, 0, true))
                {
                    return;
                }

                if (Bot.Target != null)
                {
                    if (Bot.Target.TargetGuid != Bot.Wow.PlayerGuid
                        && TryCastSpell(handOfReckoningSpell, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    if (TryCastSpell(avengersShieldSpell, Bot.Wow.TargetGuid, true)
                        || (Bot.Target.HealthPercentage < 20.0 && TryCastSpell(hammerOfWrathSpell, Bot.Wow.TargetGuid, true)))
                    {
                        return;
                    }

                    if (Use9SecSpell
                        && (((Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == sealOfVengeanceSpell) || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == sealOfWisdomSpell))
                                && TryCastSpell(judgementOfLightSpell, Bot.Wow.TargetGuid, true))
                            || TryCastSpell(consecrationSpell, Bot.Wow.TargetGuid, true)
                            || TryCastSpell(holyShieldSpell, Bot.Wow.TargetGuid, true)))
                    {
                        Use9SecSpell = false;
                        return;
                    }
                    else if (TryCastSpell(shieldOfTheRighteousnessSpell, Bot.Wow.TargetGuid, true)
                             || TryCastSpell(hammerOfTheRighteousSpell, Bot.Wow.TargetGuid, true))
                    {
                        Use9SecSpell = true;
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            Use9SecSpell = true;

            base.OutOfCombatExecute();
        }
    }
}