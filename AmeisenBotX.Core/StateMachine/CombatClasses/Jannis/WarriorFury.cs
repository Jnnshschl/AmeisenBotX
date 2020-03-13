using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class WarriorFury : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string rendSpell = "Rend";
        private readonly string whirlwindSpell = "Whirlwind";
        private readonly string bloodthirstSpell = "Bloodthirst";
        private readonly string heroicStrikeSpell = "Heroic Strike";
        private readonly string battleStanceSpell = "Battle Stance";
        private readonly string berserkerStanceSpell = "Berserker Stance";
        private readonly string hamstringSpell = "Hamstring";
        private readonly string chargeSpell = "Charge";
        private readonly string interceptSpell = "Intercept";
        private readonly string intimidatingShoutSpell = "Intimidating Shout";
        private readonly string commandingShoutSpell = "Commanding Shout";
        private readonly string disarmSpell = "Disarm";
        private readonly string bladestormSpell = "Bladestorm";
        private readonly string executeSpell = "Execute";
        private readonly string cleaveSpell = "Cleave";

        public WarriorFury(WowInterface wowInterface) : base(wowInterface)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>();

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { hamstringSpell, () => CastSpellIfPossible(hamstringSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { rendSpell, () => CastSpellIfPossible(rendSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(intimidatingShoutSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            WowInterface.CharacterManager.SpellBook.OnSpellBookUpdate += SpellBook_OnSpellBookUpdate;
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Warrior;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Fury Warrior spec.";

        public override string Displayname => "Warrior Fury";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator();

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (WowInterface.ObjectManager.Player.IsCasting
                || WowInterface.ObjectManager.TargetGuid == WowInterface.ObjectManager.PlayerGuid)
            {
                return;
            }

            if (!WowInterface.ObjectManager.Player.IsAutoAttacking)
            {
                WowInterface.HookManager.StartAutoAttack();
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick())
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                double distanceToTarget = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);
                
                if (distanceToTarget > 3)
                {
                    if (CastSpellIfPossible(chargeSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || CastSpellIfPossible(interceptSpell, WowInterface.ObjectManager.Target.Guid, true))
                    {
                        return;
                    }
                }
                else
                {
                    if ((WowInterface.ObjectManager.Target.HealthPercentage < 20)
                       && CastSpellIfPossible(executeSpell, WowInterface.ObjectManager.Target.Guid, true))
                    {
                        return;
                    }

                    if (CastSpellIfPossible(bloodthirstSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || CastSpellIfPossible(whirlwindSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || (WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 5).Count() > 2 && CastSpellIfPossible(cleaveSpell, 0, true))
                        || CastSpellIfPossible(heroicStrikeSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick())
            {
                return;
            }
        }

        private void SpellBook_OnSpellBookUpdate()
        {
            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(berserkerStanceSpell))
            {
                MyAuraManager.BuffsToKeepActive.Add(berserkerStanceSpell, () => CastSpellIfPossible(berserkerStanceSpell, 0, true));
            }
            else if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(battleStanceSpell))
            {
                MyAuraManager.BuffsToKeepActive.Add(battleStanceSpell, () => CastSpellIfPossible(battleStanceSpell, 0, true));
            }
        }
    }
}