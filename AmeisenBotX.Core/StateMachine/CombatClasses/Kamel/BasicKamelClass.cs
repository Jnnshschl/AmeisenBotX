using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
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
            TargetSelectEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            //Mount check
            getonthemount = new TimegatedEvent(TimeSpan.FromSeconds(4));

            PriorityTargets = new List<string>();
        }

        public abstract string Author { get; }

        //Basic
        public TimegatedEvent AutoAttackEvent { get; private set; }

        public abstract Dictionary<string, dynamic> Configureables { get; set; }

        public abstract string Description { get; }

        public abstract string Displayname { get; }

        public TimegatedEvent getonthemount { get; private set; }

        public abstract bool HandlesMovement { get; }

        public abstract bool IsMelee { get; }

        public abstract IWowItemComparator ItemComparator { get; set; }

        public IEnumerable<string> PriorityTargets { get; set; }

        public abstract CombatClassRole Role { get; }

        public abstract TalentTree Talents { get; }

        public bool TargetInLineOfSight { get; set; }

        public TimegatedEvent TargetSelectEvent { get; private set; }

        public abstract bool UseAutoAttacks { get; }

        public abstract string Version { get; }

        public abstract bool WalkBehindEnemy { get; }

        public abstract WowClass WowClass { get; }

        public WowInterface WowInterface { get; internal set; }

        public void Execute()
        {
            ExecuteCC();
        }

        public abstract void ExecuteCC();

        public abstract void OutOfCombatExecute();

        public override string ToString()
        {
            return $"[{WowClass}] [{Role}] {Displayname} ({Author})";
        }
    }
}