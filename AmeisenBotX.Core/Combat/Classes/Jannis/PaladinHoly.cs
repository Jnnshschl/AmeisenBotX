using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class PaladinHoly : BasicCombatClass
    {
        public PaladinHoly(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(blessingOfWisdomSpell, () => TryCastSpell(blessingOfWisdomSpell, WowInterface.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(devotionAuraSpell, () => TryCastSpell(devotionAuraSpell, WowInterface.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(sealOfWisdomSpell, () => WowInterface.CharacterManager.SpellBook.IsSpellKnown(sealOfWisdomSpell) && TryCastSpell(sealOfWisdomSpell, WowInterface.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(sealOfVengeanceSpell, () => !WowInterface.CharacterManager.SpellBook.IsSpellKnown(sealOfWisdomSpell) && TryCastSpell(sealOfVengeanceSpell, WowInterface.PlayerGuid, true)));

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, flashOfLightSpell },
                { 300, holyLightSpell },
                { 2000, holyShockSpell },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((blessingOfWisdomSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Holy Paladin spec.";

        public override string Displayname => "Paladin Holy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicComparator
        (
            null,
            new() { WowWeaponType.TWOHANDED_AXES, WowWeaponType.TWOHANDED_MACES, WowWeaponType.TWOHANDED_SWORDS },
            new Dictionary<string, double>()
            {
                { "ITEM_MOD_CRIT_RATING_SHORT", 0.88 },
                { "ITEM_MOD_INTELLECT_SHORT", 0.2 },
                { "ITEM_MOD_SPELL_POWER_SHORT", 0.68 },
                { "ITEM_MOD_HASTE_RATING_SHORT", 0.71},
            }
        );

        public override WowRole Role => WowRole.Heal;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 5) },
                { 3, new(1, 3, 3) },
                { 4, new(1, 4, 5) },
                { 6, new(1, 6, 1) },
                { 7, new(1, 7, 5) },
                { 8, new(1, 8, 1) },
                { 10, new(1, 10, 2) },
                { 13, new(1, 13, 1) },
                { 14, new(1, 14, 3) },
                { 16, new(1, 16, 5) },
                { 17, new(1, 17, 3) },
                { 18, new(1, 18, 1) },
                { 21, new(1, 21, 5) },
                { 22, new(1, 22, 1) },
                { 23, new(1, 23, 5) },
                { 24, new(1, 24, 2) },
                { 25, new(1, 25, 2) },
                { 26, new(1, 26, 1) },
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 5) },
            },
            Tree3 = new()
            {
                { 2, new(3, 2, 5) },
                { 4, new(3, 4, 3) },
                { 5, new(3, 5, 2) },
                { 7, new(3, 7, 5) },
            },
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Paladin;

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override void Execute()
        {
            base.Execute();

            if (WowInterface.ObjectManager.Partymembers.Any(e => e.Guid != WowInterface.Player.Guid) || WowInterface.Player.HealthPercentage < 65.0)
            {
                if (NeedToHealSomeone())
                {
                    return;
                }
            }
            else if (SelectTarget(TargetManagerDps))
            {
                if ((WowInterface.Player.HasBuffByName(sealOfVengeanceSpell) || WowInterface.Player.HasBuffByName(sealOfWisdomSpell))
                    && TryCastSpell(judgementOfLightSpell, WowInterface.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(exorcismSpell, WowInterface.TargetGuid, true))
                {
                    return;
                }

                if (!WowInterface.Player.IsAutoAttacking
                    && WowInterface.Target.Position.GetDistance(WowInterface.Player.Position) < 3.5
                    && EventAutoAttack.Run())
                {
                    WowInterface.HookManager.LuaStartAutoAttack();
                    return;
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, WowInterface.Target.Position);
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealSomeone())
            {
                return;
            }
        }

        private bool NeedToHealSomeone()
        {
            if (TargetManagerHeal.GetUnitToTarget(out IEnumerable<WowUnit> unitsToHeal))
            {
                WowUnit targetUnit = unitsToHeal.FirstOrDefault(e => !e.HasBuffByName(beaconOfLightSpell));

                if (targetUnit == null)
                {
                    unitsToHeal.FirstOrDefault(e => e != null);

                    if (targetUnit == null)
                    {
                        return false;
                    }
                }

                if (targetUnit.HealthPercentage < 15.0
                    && TryCastSpell(layOnHandsSpell, 0))
                {
                    return true;
                }

                if (unitsToHeal.Count(e => !e.HasBuffByName(beaconOfLightSpell)) > 1
                    && TryCastSpell(beaconOfLightSpell, targetUnit.Guid, true))
                {
                    return true;
                }

                // TODO: bugged need to figure out why cooldown is always wrong
                // if (targetUnit.HealthPercentage < 50
                //     && CastSpellIfPossible(divineFavorSpell, targetUnit.Guid, true))
                // {
                //     LastHealAction = DateTime.Now;
                //     return true;
                // }

                if (WowInterface.Player.ManaPercentage < 50
                   && WowInterface.Player.ManaPercentage > 20
                   && TryCastSpell(divineIlluminationSpell, 0, true))
                {
                    return true;
                }

                if (WowInterface.Player.ManaPercentage < 60
                    && TryCastSpell(divinePleaSpell, 0, true))
                {
                    return true;
                }

                double healthDifference = targetUnit.MaxHealth - targetUnit.Health;
                List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                {
                    if (TryCastSpell(keyValuePair.Value, targetUnit.Guid, true))
                    {
                        break;
                    }
                }

                return true;
            }

            return false;
        }
    }
}