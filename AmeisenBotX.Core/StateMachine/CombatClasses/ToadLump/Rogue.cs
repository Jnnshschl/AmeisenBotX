using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Statemachine;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.ToadLump
{
    public class Rogue : BasicCombatClass
    {
        public Rogue(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            /*
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { battleShoutSpell, () => TryCastSpell(battleShoutSpell, 0, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { hamstringSpell, () => WowInterface.ObjectManager.Target.Type == WowObjectType.Player && TryCastSpell(hamstringSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { rendSpell, () => WowInterface.ObjectManager.Target.Type == WowObjectType.Player && WowInterface.ObjectManager.Player.Rage > 75 && TryCastSpell(rendSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpellWarrior(intimidatingShoutSpell, berserkerStanceSpell, x.Guid, true) },
                { 1, (x) => TryCastSpellWarrior(intimidatingShoutSpell, battleStanceSpell, x.Guid, true) }
            };

            HeroicStrikeEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
            */
        }

        public override string Description => "Control low level rogues for grinding purposes";

        public override string Displayname => "Rogue - Low Level";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.LEATHER }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.DAGGERS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            //todo make these accurate to rogue
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 3) },
                { 3, new Talent(1, 3, 2) },
                { 5, new Talent(1, 5, 2) },
                { 6, new Talent(1, 6, 3) },
                { 9, new Talent(1, 9, 2) },
                { 10, new Talent(1, 10, 3) },
                { 11, new Talent(1, 11, 3) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 3) },
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 6, new Talent(2, 6, 3) },
                { 10, new Talent(2, 10, 5) },
                { 13, new Talent(2, 13, 3) },
                { 14, new Talent(2, 14, 1) },
                { 16, new Talent(2, 16, 1) },
                { 17, new Talent(2, 17, 5) },
                { 18, new Talent(2, 18, 3) },
                { 19, new Talent(2, 19, 1) },
                { 20, new Talent(2, 20, 2) },
                { 22, new Talent(2, 22, 5) },
                { 23, new Talent(2, 23, 1) },
                { 24, new Talent(2, 24, 1) },
                { 25, new Talent(2, 25, 3) },
                { 26, new Talent(2, 26, 5) },
                { 27, new Talent(2, 27, 1) },
            },
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "0.1.0";

        //todo: decide what this value does and whether it should be true
        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Rogue;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetManagerDps))
            {
                if (WowInterface.ObjectManager.Target != null)
                {
                    double distanceToTarget = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                    if ((WowInterface.ObjectManager.Player.IsDazed
                        || WowInterface.ObjectManager.Player.IsConfused
                        || WowInterface.ObjectManager.Player.IsPossessed
                        || WowInterface.ObjectManager.Player.IsFleeing)
                        && TryCastSpell(heroicFurySpell, 0))
                    {
                        return;
                    }

                    if (distanceToTarget > 5.0)
                    {
                        return;
                    }
                    else
                    {
                        if (TryCastSpellRogue(eviscerateSpell, WowInterface.ObjectManager.Target.Guid, needsEnergy: true, needsCombopoints: true, requiredCombopoints: 5))
                        {
                            return;
                        }
                        if (TryCastSpellRogue(sinisterStrikeSpell, WowInterface.ObjectManager.Target.Guid, needsEnergy: true))
                        {
                            return;
                        }

                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
        }
    }
}
