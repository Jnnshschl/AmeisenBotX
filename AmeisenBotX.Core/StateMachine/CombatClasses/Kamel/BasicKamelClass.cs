using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.Utils;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Kamel
{
    public abstract class BasicKamelClass : ICombatClass
    {
        protected BasicKamelClass()
        {
            //Basic
            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            //FuryWarrior
            TargetSelectEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            HeroicStrikeEvent = new TimegatedEvent(TimeSpan.FromSeconds(3));
            RendEvent = new TimegatedEvent(TimeSpan.FromSeconds(6));
            ExecuteEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            //Resto Shaman
            revivePlayerEvent = new TimegatedEvent(TimeSpan.FromSeconds(4));
            manaTideTotemEvent = new TimegatedEvent(TimeSpan.FromSeconds(12));
            totemcastEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            //Mount check
            getonthemount = new TimegatedEvent(TimeSpan.FromSeconds(4));

            PriorityTargets = new List<string>();
        }

        public abstract string Author { get; }

        //Basic
        public TimegatedEvent AutoAttackEvent { get; private set; }

        public abstract WowClass Class { get; }

        public abstract Dictionary<string, dynamic> Configureables { get; set; }

        public CooldownManager CooldownManager { get; private set; }

        public abstract string Description { get; }

        public abstract string Displayname { get; }

        public TimegatedEvent ExecuteEvent { get; private set; }

        //Mount check
        public TimegatedEvent getonthemount { get; private set; }

        public abstract bool HandlesMovement { get; }

        public TimegatedEvent HeroicStrikeEvent { get; private set; }

        public abstract bool IsMelee { get; }

        public abstract IWowItemComparator ItemComparator { get; set; }

        public TimegatedEvent manaTideTotemEvent { get; private set; }

        public TimegatedEvent naturesswiftEvent { get; private set; }

        public List<string> PriorityTargets { get; set; }

        public TimegatedEvent RendEvent { get; private set; }

        //Resto Shaman
        public TimegatedEvent revivePlayerEvent { get; private set; }

        public TimegatedEvent riptideSpellEvent { get; private set; }

        public abstract CombatClassRole Role { get; }

        public abstract TalentTree Talents { get; }

        public bool TargetInLineOfSight { get; set; }

        //FuryWarrior
        public TimegatedEvent TargetSelectEvent { get; private set; }

        public TimegatedEvent totemcastEvent { get; private set; }

        public TimegatedEvent UpdatePriorityUnits { get; set; }

        public abstract bool UseAutoAttacks { get; }

        public bool UseDefaultTargetSelection { get; protected set; } = true;

        public abstract string Version { get; }

        public abstract bool WalkBehindEnemy { get; }

        public WowInterface WowInterface { get; internal set; }

        public void Execute()
        {
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