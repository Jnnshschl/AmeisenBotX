using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using System.Linq;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Kamel
{
    public abstract class BasicKamelClass : ICombatClass
    {
        private readonly Dictionary<string, DateTime> spellCoolDown = new Dictionary<string, DateTime>();

        //Race (Troll)
        private const string BerserkingSpell = "Berserking";

        //Race (Draenei)
        private const string giftOfTheNaaruSpell = "Gift of the Naaru";

        //Race (Dwarf)
        private const string StoneformSpell = "Stoneform";   
        
        //Race (Human)
        private const string EveryManforHimselfSpell = "Every Man for Himself";

        protected BasicKamelClass()
        {
            //Basic
            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            TargetSelectEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            //Mount check
            getonthemount = new TimegatedEvent(TimeSpan.FromSeconds(4));

            //Race (Troll)
            spellCoolDown.Add(BerserkingSpell, DateTime.Now);

            //Race (Draenei)
            spellCoolDown.Add(giftOfTheNaaruSpell, DateTime.Now);   
            
            //Race (Dwarf)
            spellCoolDown.Add(StoneformSpell, DateTime.Now);  
            
            //Race (Human)
            spellCoolDown.Add(EveryManforHimselfSpell, DateTime.Now);

            PriorityTargetDisplayIds = new List<int>();
        }

        private readonly int[] useableHealingItems = new int[]
        {
            // potions
            118, 929, 1710, 2938, 3928, 4596, 5509, 13446, 22829, 33447,
            // healthstones
            5509, 5510, 5511, 5512, 9421, 19013, 22103, 36889, 36892,
        };

        private readonly int[] useableManaItems = new int[]
        {
            // potions
            2245, 3385, 3827, 6149, 13443, 13444, 33448, 22832,
        };

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

        public IEnumerable<int> PriorityTargetDisplayIds { get; set; }

        public IEnumerable<int> BlacklistedTargetDisplayIds { get; set; }

        public abstract CombatClassRole Role { get; }

        public abstract TalentTree Talents { get; }

        public bool TargetInLineOfSight => WowInterface.ObjectManager.IsTargetInLineOfSight;

        public TimegatedEvent TargetSelectEvent { get; private set; }

        public abstract bool UseAutoAttacks { get; }

        public abstract string Version { get; }

        public abstract bool WalkBehindEnemy { get; }

        public abstract WowClass WowClass { get; }

        public WowInterface WowInterface { get; internal set; }

        public void Execute()
        {
            ExecuteCC();

            if (WowInterface.ObjectManager.Player.Race == WowRace.Human
            && (WowInterface.ObjectManager.Player.IsDazed
                || WowInterface.ObjectManager.Player.IsFleeing
                || WowInterface.ObjectManager.Player.IsInfluenced
                || WowInterface.ObjectManager.Player.IsPossessed))
            {
                if (IsSpellReady(EveryManforHimselfSpell))
                {
                    WowInterface.HookManager.LuaCastSpell(EveryManforHimselfSpell);
                }
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 50.0 
            && (WowInterface.ObjectManager.Player.Race == WowRace.Dwarf ))
            {
                if (IsSpellReady(StoneformSpell))
                {
                    WowInterface.HookManager.LuaCastSpell(StoneformSpell);
                }
            }

            // Useable items, potions, etc.
            // ---------------------------- >

            if (WowInterface.ObjectManager.Player.HealthPercentage < 20)
            {
                IWowItem healthItem = WowInterface.CharacterManager.Inventory.Items.FirstOrDefault(e => useableHealingItems.Contains(e.Id));

                if (healthItem != null)
                {
                    WowInterface.HookManager.LuaUseItemByName(healthItem.Name);
                }
            }

            if (WowInterface.ObjectManager.Player.ManaPercentage < 20)
            {
                IWowItem manaItem = WowInterface.CharacterManager.Inventory.Items.FirstOrDefault(e => useableManaItems.Contains(e.Id));

                if (manaItem != null)
                {
                    WowInterface.HookManager.LuaUseItemByName(manaItem.Name);
                }
            }
        }

        public abstract void ExecuteCC();

        public abstract void OutOfCombatExecute();
        public void AttackTarget()
        {
            WowUnit target = WowInterface.ObjectManager.Target;
            if (target == null)
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.Position.GetDistance(target.Position) <= 3.0)
            {
                WowInterface.HookManager.WowStopClickToMove();
                WowInterface.MovementEngine.Reset();
                WowInterface.HookManager.WowUnitRightClick(target);
            }
            else
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, target.Position);
            }
        }

        private bool IsSpellReady(string spellName)
        {
            if (DateTime.Now > spellCoolDown[spellName])
            {
                spellCoolDown[spellName] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.LuaGetSpellCooldown(spellName));
                return true;
            }

            return false;
        }

        public bool IsTargetAttackable(WowUnit target)
        {
            return true;
        }

        public override string ToString()
        {
            return $"[{WowClass}] [{Role}] {Displayname} ({Author})";
        }
    }
}