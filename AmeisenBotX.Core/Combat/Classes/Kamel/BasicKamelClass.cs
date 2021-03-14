using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Kamel
{
    public abstract class BasicKamelClass : ICombatClass
    {
        #region Race Spells
        //Race (Troll)
        private const string BerserkingSpell = "Berserking";

        //Race (Human)
        private const string EveryManforHimselfSpell = "Every Man for Himself";

        //Race (Draenei)
        private const string giftOfTheNaaruSpell = "Gift of the Naaru";

        //Race (Dwarf)
        private const string StoneformSpell = "Stoneform";
        #endregion

        #region Warrior

        #endregion

        #region Shaman
        public const string ancestralSpiritSpell = "Ancestral Spirit";
        #endregion

        #region Paladin
        public const string redemptionSpell = "Redemption";
        #endregion

        #region Priest
        public const string resurrectionSpell = "Resurrection";
        #endregion

        public readonly Dictionary<string, DateTime> spellCoolDown = new Dictionary<string, DateTime>();

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

        protected BasicKamelClass()
        {
            //Revive Spells
            spellCoolDown.Add(ancestralSpiritSpell, DateTime.Now);
            spellCoolDown.Add(redemptionSpell, DateTime.Now);
            spellCoolDown.Add(resurrectionSpell, DateTime.Now);

            //Basic
            AutoAttackEvent = new(TimeSpan.FromSeconds(1));
            TargetSelectEvent = new(TimeSpan.FromSeconds(1));
            revivePlayerEvent = new(TimeSpan.FromSeconds(4));

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

        public abstract string Author { get; }

        public IEnumerable<int> BlacklistedTargetDisplayIds { get; set; }

        public abstract Dictionary<string, dynamic> C { get; set; }

        public abstract string Description { get; }

        public abstract string Displayname { get; }

        public bool HandlesFacing => false;

        public abstract bool HandlesMovement { get; }

        public abstract bool IsMelee { get; }

        public abstract IItemComparator ItemComparator { get; set; }

        public IEnumerable<int> PriorityTargetDisplayIds { get; set; }

        public abstract WowRole Role { get; }

        public abstract TalentTree Talents { get; }

        public bool TargetInLineOfSight => WowInterface.ObjectManager.IsTargetInLineOfSight;

        public abstract bool UseAutoAttacks { get; }

        public abstract string Version { get; }

        public abstract bool WalkBehindEnemy { get; }

        public abstract WowClass WowClass { get; }

        public WowInterface WowInterface { get; internal set; }
        public TimegatedEvent AutoAttackEvent { get; private set; }
        public TimegatedEvent TargetSelectEvent { get; private set; }
        public TimegatedEvent revivePlayerEvent { get; private set; }

        //follow the target
        public void AttackTarget()
        {
            WowUnit target = WowInterface.Target;
            if (target == null)
            {
                return;
            }

            if (WowInterface.Player.Position.GetDistance(target.Position) <= 3.0)
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
        public void Targetselection()
        {
            if (TargetSelectEvent.Run())
            {
                WowUnit nearTarget = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.Player.Position, 50)
                .Where(e => !e.IsNotAttackable && (e.Type == WowObjectType.Player && (e.IsPvpFlagged && !e.IsFriendyTo(WowInterface, WowInterface.Player)) || (e.IsInCombat)) || (e.IsInCombat && e.Name != "The Lich King" && !(WowInterface.ObjectManager.MapId == WowMapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346)))
                .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                .FirstOrDefault();//&& e.Type(Player)

                if (nearTarget != null)
                {
                    WowInterface.HookManager.WowTargetGuid(nearTarget.Guid);

                    if (!TargetInLineOfSight)
                    {
                        return;
                    }
                }
                else
                {
                    AttackTarget();
                }
            }
        }
        public void TargetselectionTank()
        {
            if (TargetSelectEvent.Run())
            {

                WowUnit nearTargetToTank = WowInterface.ObjectManager.GetEnemiesTargetingPartymembers<WowUnit>(WowInterface.Player.Position, 60)
                .Where(e => e.IsInCombat && !e.IsNotAttackable && e.Type != WowObjectType.Player && e.Name != "The Lich King" && e.Name != "Anub'Rekhan" && !(WowInterface.ObjectManager.MapId == WowMapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346))
                .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                .FirstOrDefault();

                if (nearTargetToTank != null)
                {
                    WowInterface.HookManager.WowTargetGuid(nearTargetToTank.Guid);

                    if (!TargetInLineOfSight)
                    {
                        return;
                    }
                    else
                    {
                        AttackTarget();
                    }
                }
                else
                {
                    WowUnit nearTarget = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.Player.Position, 80)
                    .Where(e => e.IsInCombat && !e.IsNotAttackable && e.Type == WowObjectType.Player)
                    .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                    .FirstOrDefault();//&& e.Type(Player)

                    if (nearTarget != null)
                    {
                        WowInterface.HookManager.WowTargetGuid(nearTarget.Guid);

                        if (!TargetInLineOfSight)
                        {
                            return;
                        }
                    }
                    else
                    {
                        AttackTarget();
                    }
                }
            }
        }
        //Change target if target to far away
        public void ChangeTargetToAttack() 
        {
            IEnumerable<WowPlayer> PlayerNearPlayer = WowInterface.ObjectManager.GetNearEnemies<WowPlayer>(WowInterface.Player.Position, 15);

            WowUnit target = WowInterface.Target;
            if (target == null)
            {
                return;
            }
            if (PlayerNearPlayer.Count() >= 1 && WowInterface.ObjectManager.Target.HealthPercentage >= 60 && WowInterface.Player.Position.GetDistance(target.Position) >= 20)
            {
                WowInterface.HookManager.WowClearTarget();
                return;
            }
        }
        public void Execute()
        {
            ExecuteCC();

            if (WowInterface.Player.Race == WowRace.Human
            && (WowInterface.Player.IsDazed
                || WowInterface.Player.IsFleeing
                || WowInterface.Player.IsInfluenced
                || WowInterface.Player.IsPossessed))
            {
                if (IsSpellReady(EveryManforHimselfSpell))
                {
                    WowInterface.HookManager.LuaCastSpell(EveryManforHimselfSpell);
                }
            }

            if (WowInterface.Player.HealthPercentage < 50.0
            && (WowInterface.Player.Race == WowRace.Dwarf))
            {
                if (IsSpellReady(StoneformSpell))
                {
                    WowInterface.HookManager.LuaCastSpell(StoneformSpell);
                }
            }

            // Useable items, potions, etc.
            // ---------------------------- >

            if (WowInterface.Player.HealthPercentage < 20)
            {
                IWowItem healthItem = WowInterface.CharacterManager.Inventory.Items.FirstOrDefault(e => useableHealingItems.Contains(e.Id));

                if (healthItem != null)
                {
                    WowInterface.HookManager.LuaUseItemByName(healthItem.Name);
                }
            }

            if (WowInterface.Player.ManaPercentage < 20)
            {
                IWowItem manaItem = WowInterface.CharacterManager.Inventory.Items.FirstOrDefault(e => useableManaItems.Contains(e.Id));

                if (manaItem != null)
                {
                    WowInterface.HookManager.LuaUseItemByName(manaItem.Name);
                }
            }
        }

        public abstract void ExecuteCC();

        public bool IsTargetAttackable(WowUnit target)
        {
            return true;
        }

        public abstract void OutOfCombatExecute();

        public override string ToString()
        {
            return $"[{WowClass}] [{Role}] {Displayname} ({Author})";
        }

        public bool IsSpellReady(string spellName)
        {
            if (DateTime.Now > spellCoolDown[spellName])
            {
                spellCoolDown[spellName] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.HookManager.LuaGetSpellCooldown(spellName));
                return true;
            }

            return false;
        }

        public bool CheckForWeaponEnchantment(WowEquipmentSlot slot, string enchantmentName, string spellToCastEnchantment)
        {
            if (WowInterface.CharacterManager.Equipment.Items.ContainsKey(slot))
            {
                int itemId = WowInterface.CharacterManager.Equipment.Items[slot].Id;

                if (itemId > 0)
                {
                    if (slot == WowEquipmentSlot.INVSLOT_MAINHAND)
                    {
                        WowItem item = WowInterface.ObjectManager.WowObjects.OfType<WowItem>().FirstOrDefault(e => e.EntryId == itemId);

                        if (item != null
                            && !item.GetEnchantmentStrings().Any(e => e.Contains(enchantmentName))
                            && CustomCastSpellMana(spellToCastEnchantment))
                        {
                            return true;
                        }
                    }
                    else if (slot == WowEquipmentSlot.INVSLOT_OFFHAND)
                    {
                        WowItem item = WowInterface.ObjectManager.WowObjects.OfType<WowItem>().LastOrDefault(e => e.EntryId == itemId);

                        if (item != null
                            && !item.GetEnchantmentStrings().Any(e => e.Contains(enchantmentName))
                            && CustomCastSpellMana(spellToCastEnchantment))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        //Mana Spells
        public bool CustomCastSpellMana(string spellName, bool castOnSelf = false)
        {
            if (WowInterface.CharacterManager.SpellBook.IsSpellKnown(spellName))
            {
                if (WowInterface.Target != null)
                {
                    Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                    if ((WowInterface.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        double distance = WowInterface.Player.Position.GetDistance(WowInterface.Target.Position);

                        if ((spell.MinRange == 0 && spell.MaxRange == 0) || (spell.MinRange <= distance && spell.MaxRange >= distance))
                        {
                            WowInterface.HookManager.LuaCastSpell(spellName);
                            return true;
                        }
                    }
                }
                else
                {
                    WowInterface.HookManager.WowTargetGuid(WowInterface.PlayerGuid);

                    Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                    if ((WowInterface.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        WowInterface.HookManager.LuaCastSpell(spellName);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool totemItemCheck()
        {
            if (WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Earth Totem", StringComparison.OrdinalIgnoreCase)) &&
             WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Air Totem", StringComparison.OrdinalIgnoreCase)) &&
             WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Water Totem", StringComparison.OrdinalIgnoreCase)) &&
             WowInterface.CharacterManager.Inventory.Items.Any(e => e.Name.Equals("Fire Totem", StringComparison.OrdinalIgnoreCase)) ||
             (WowInterface.CharacterManager.Equipment.Items.ContainsKey(WowEquipmentSlot.INVSLOT_RANGED) &&
             WowInterface.CharacterManager.Equipment.Items[WowEquipmentSlot.INVSLOT_RANGED] != null))
            { 
                return true;
            }

            return false;
        }

        public void revivePartyMember(string reviveSpellName)
        {
            List<WowUnit> partyMemberToHeal = new List<WowUnit>(WowInterface.ObjectManager.Partymembers)
            {
                WowInterface.Player
            };

            partyMemberToHeal = partyMemberToHeal.Where(e => e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

            if (revivePlayerEvent.Run() && partyMemberToHeal.Count > 0)
            {
                WowInterface.HookManager.WowTargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                CustomCastSpellMana(reviveSpellName);
            }
        }
    }
}