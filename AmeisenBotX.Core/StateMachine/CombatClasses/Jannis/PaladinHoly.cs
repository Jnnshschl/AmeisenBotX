using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class PaladinHoly : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string blessingOfWisdomSpell = "Blessing of Wisdom";
        private const string devotionAuraSpell = "Devotion Aura";
        private const string divineFavorSpell = "Divine Favor";
        private const string divineIlluminationSpell = "Divine Illumination";
        private const string divinePleaSpell = "Divine Plea";
        private const string flashOfLightSpell = "Flash of Light";
        private const string holyLightSpell = "Holy Light";
        private const string holyShockSpell = "Holy Shock";
        private const string layOnHandsSpell = "Lay on Hands";
#pragma warning restore IDE0051

        public PaladinHoly(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            UseDefaultTargetSelection = false;

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { blessingOfWisdomSpell, () => CastSpellIfPossible(blessingOfWisdomSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { devotionAuraSpell, () => CastSpellIfPossible(devotionAuraSpell, WowInterface.ObjectManager.PlayerGuid, true) }
            };

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, flashOfLightSpell },
                { 2000, holyShockSpell },
                { 4000, holyLightSpell }
            };

            FaceEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(4000));

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((blessingOfWisdomSpell, (spellName, guid) => CastSpellIfPossible(spellName, guid, true)));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Paladin;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Holy Paladin spec.";

        public override string Displayname => "Paladin Holy";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => true;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(null, new List<WeaponType>() { WeaponType.TWOHANDED_AXES, WeaponType.TWOHANDED_MACES, WeaponType.TWOHANDED_SWORDS });

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override string Version => "1.0";

        private TimegatedEvent FaceEvent { get; set; }

        private TimegatedEvent AutoAttackEvent { get; set; }

        private DateTime LastHealAction { get; set; }

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override void ExecuteCC()
        {
            // after 1 seconds of no healing and no freidns around us we are going to attack stuff
            if (!NeedToHealSomeone()
                && DateTime.Now - LastHealAction > TimeSpan.FromSeconds(1)
                && WowInterface.ObjectManager.GetNearPartymembers(WowInterface.ObjectManager.Player.Position, 48).Count <= 1)
            {
                // basic auto attack defending
                if (WowInterface.ObjectManager.TargetGuid == 0 || WowInterface.HookManager.GetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target) == WowUnitReaction.Friendly)
                {
                    List<WowUnit> nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 10).Where(e => e.IsInCombat).ToList();

                    if (nearEnemies.Count > 0)
                    {
                        WowUnit target = nearEnemies.FirstOrDefault();
                        if (target != null)
                        {
                            WowInterface.HookManager.TargetGuid(target.Guid);
                            WowInterface.MovementEngine.Reset();
                        }
                    }
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, WowInterface.ObjectManager.Target.Position);

                    if (WowInterface.MovementEngine.IsAtTargetPosition)
                    {
                        if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run())
                        {
                            WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
                        }

                        if (FaceEvent.Run())
                        {
                            WowInterface.HookManager.FacePosition(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target.Position);
                        }
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || GroupAuraManager.Tick()
                || NeedToHealSomeone())
            {
                return;
            }
        }

        private bool NeedToHealSomeone()
        {
            if (TargetManager.GetUnitToTarget(out List<WowUnit> unitsToHeal))
            {
                WowUnit targetUnit = unitsToHeal.First();
                WowInterface.HookManager.TargetGuid(targetUnit.Guid);

                if (targetUnit.HealthPercentage < 12
                    && CastSpellIfPossible(layOnHandsSpell, 0))
                {
                    LastHealAction = DateTime.Now;
                    return true;
                }

                // TODO: bugged need to figure out why cooldown is always wrong
                // if (targetUnit.HealthPercentage < 50
                //     && CastSpellIfPossible(divineFavorSpell, targetUnit.Guid, true))
                // {
                //     LastHealAction = DateTime.Now;
                //     return true;
                // }

                if (WowInterface.ObjectManager.Player.ManaPercentage < 50
                   && WowInterface.ObjectManager.Player.ManaPercentage > 20
                   && CastSpellIfPossible(divineIlluminationSpell, 0, true))
                {
                    LastHealAction = DateTime.Now;
                    return true;
                }

                if (WowInterface.ObjectManager.Player.ManaPercentage < 60
                    && CastSpellIfPossible(divinePleaSpell, 0, true))
                {
                    LastHealAction = DateTime.Now;
                    return true;
                }

                double healthDifference = targetUnit.MaxHealth - targetUnit.Health;
                List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                {
                    if (CastSpellIfPossible(keyValuePair.Value, targetUnit.Guid, true))
                    {
                        break;
                    }
                }

                LastHealAction = DateTime.Now;
                return true;
            }

            return false;
        }
    }
}