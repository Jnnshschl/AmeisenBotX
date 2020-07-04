using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.States;
using AmeisenBotX.Core.Statemachine.Utils;
using AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Kamel
{
    public abstract class BasicKamelClass : ICombatClass
    {
        protected BasicKamelClass() 
        {
            MyAuraManager = new AuraManager
            (
                null,
                null,
                TimeSpan.FromSeconds(1),
                () => { if (WowInterface.ObjectManager.Player != null) { return WowInterface.ObjectManager.Player.Auras.Select(e => e.Name).ToList(); } else { return null; } },
                () => { if (WowInterface.ObjectManager.Player != null) { return WowInterface.ObjectManager.Player.Auras.Select(e => e.Name).ToList(); } else { return null; } },
                null,
                DispellDebuffsFunction
            );
            //Basic
            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            //FuryWarrior
            TargetSelectEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
            HeroicStrikeEvent = new TimegatedEvent(TimeSpan.FromSeconds(3));
            RendEvent = new TimegatedEvent(TimeSpan.FromSeconds(6));
            ExecuteEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            //Resto Shaman
            

            //Mount check
            getonthemount = new TimegatedEvent(TimeSpan.FromSeconds(4));
        }

        public abstract string Author { get; }
        //Basic
        public TimegatedEvent AutoAttackEvent { get; private set; }
        //FuryWarrior
        public TimegatedEvent TargetSelectEvent { get; private set; }
        public TimegatedEvent HeroicStrikeEvent { get; private set; }
        public TimegatedEvent RendEvent { get; private set; }
        public TimegatedEvent ExecuteEvent { get; private set; }
        //Resto Shaman
        public TimegatedEvent healingWaveSpellEvent { get; private set; }
        public TimegatedEvent naturesswiftEvent { get; private set; }
        public TimegatedEvent riptideSpellEvent { get; private set; }
        //Mount check
        public TimegatedEvent getonthemount { get; private set; }

        public abstract WowClass Class { get; }

        public abstract Dictionary<string, dynamic> Configureables { get; set; }

        public CooldownManager CooldownManager { get; private set; }

        public abstract string Description { get; }

        public DispellBuffsFunction DispellBuffsFunction { get; private set; }

        public DispellDebuffsFunction DispellDebuffsFunction { get; private set; }

        public abstract string Displayname { get; }

        public GroupAuraManager GroupAuraManager { get; private set; }

        public abstract bool HandlesMovement { get; }

        public abstract bool IsMelee { get; }

        public abstract IWowItemComparator ItemComparator { get; set; }

        public AuraManager MyAuraManager { get; private set; }

        public TimegatedEvent NearInterruptUnitsEvent { get; set; }

        public List<string> PriorityTargets { get => TargetManager.PriorityTargets; set => TargetManager.PriorityTargets = value; }

        public Dictionary<string, DateTime> RessurrectionTargets { get; private set; }

        public abstract CombatClassRole Role { get; }

        public Dictionary<string, Spell> Spells { get; protected set; }

        public abstract TalentTree Talents { get; }

        public AuraManager TargetAuraManager { get; private set; }

        public InterruptManager TargetInterruptManager { get; private set; }

        public TargetManager TargetManager { get; private set; }

        public TimegatedEvent UpdatePriorityUnits { get; set; }

        public abstract bool UseAutoAttacks { get; }

        public bool UseDefaultTargetSelection { get; protected set; } = true;

        public abstract string Version { get; }

        public abstract bool WalkBehindEnemy { get; }

        public WowInterface WowInterface { get; internal set; }

        public bool TargetInLineOfSight { get; set; }

        public void Execute()
        {
            MyAuraManager.Tick();
            ExecuteCC();
        }
        public abstract void ExecuteCC();

        public abstract void OutOfCombatExecute();

        public override string ToString()
        {
            return $"[{Class}] [{Role}] {Displayname} ({Author})";
        }
    }
}
