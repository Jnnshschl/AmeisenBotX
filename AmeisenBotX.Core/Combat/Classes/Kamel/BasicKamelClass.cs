using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Wow.Objects.Enums;
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

        #endregion Race Spells

        #region Shaman

        public const string ancestralSpiritSpell = "Ancestral Spirit";

        #endregion Shaman

        #region Paladin

        public const string redemptionSpell = "Redemption";

        #endregion Paladin

        #region Priest

        public const string resurrectionSpell = "Resurrection";

        #endregion Priest

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

        public TimegatedEvent AutoAttackEvent { get; private set; }

        public IEnumerable<int> BlacklistedTargetDisplayIds { get; set; }

        public abstract Dictionary<string, dynamic> C { get; set; }

        public abstract string Description { get; }

        public abstract string Displayname { get; }

        public bool HandlesFacing => false;

        public abstract bool HandlesMovement { get; }

        public abstract bool IsMelee { get; }

        public abstract IItemComparator ItemComparator { get; set; }

        public IEnumerable<int> PriorityTargetDisplayIds { get; set; }

        public TimegatedEvent revivePlayerEvent { get; private set; }

        public abstract WowRole Role { get; }

        public abstract TalentTree Talents { get; }

        public bool TargetInLineOfSight => WowInterface.Objects.IsTargetInLineOfSight;

        public TimegatedEvent TargetSelectEvent { get; private set; }

        public abstract bool UseAutoAttacks { get; }

        public abstract string Version { get; }

        public abstract bool WalkBehindEnemy { get; }

        public abstract WowClass WowClass { get; }

        public WowInterface WowInterface { get; internal set; }

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
                WowInterface.NewWowInterface.WowStopClickToMove();
                WowInterface.MovementEngine.Reset();
                WowInterface.NewWowInterface.WowUnitRightClick(target.BaseAddress);
            }
            else
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, target.Position);
            }
        }

        //Change target if target to far away
        public void ChangeTargetToAttack()
        {
            IEnumerable<WowPlayer> PlayerNearPlayer = WowInterface.Objects.GetNearEnemies<WowPlayer>(WowInterface.Db.GetReaction, WowInterface.Player.Position, 15);

            WowUnit target = WowInterface.Target;
            if (target == null)
            {
                return;
            }
            if (PlayerNearPlayer.Count() >= 1 && WowInterface.Objects.Target.HealthPercentage >= 60 && WowInterface.Player.Position.GetDistance(target.Position) >= 20)
            {
                WowInterface.NewWowInterface.WowClearTarget();
                return;
            }
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
                        WowItem item = WowInterface.Objects.WowObjects.OfType<WowItem>().FirstOrDefault(e => e.EntryId == itemId);

                        if (item != null
                            && !item.GetEnchantmentStrings().Any(e => e.Contains(enchantmentName))
                            && CustomCastSpellMana(spellToCastEnchantment))
                        {
                            return true;
                        }
                    }
                    else if (slot == WowEquipmentSlot.INVSLOT_OFFHAND)
                    {
                        WowItem item = WowInterface.Objects.WowObjects.OfType<WowItem>().LastOrDefault(e => e.EntryId == itemId);

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
                            WowInterface.NewWowInterface.LuaCastSpell(spellName);
                            return true;
                        }
                    }
                }
                else
                {
                    WowInterface.NewWowInterface.WowTargetGuid(WowInterface.Player.Guid);

                    Spell spell = WowInterface.CharacterManager.SpellBook.GetSpellByName(spellName);

                    if ((WowInterface.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        WowInterface.NewWowInterface.LuaCastSpell(spellName);
                        return true;
                    }
                }
            }

            return false;
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
                    WowInterface.NewWowInterface.LuaCastSpell(EveryManforHimselfSpell);
                }
            }

            if (WowInterface.Player.HealthPercentage < 50.0
            && (WowInterface.Player.Race == WowRace.Dwarf))
            {
                if (IsSpellReady(StoneformSpell))
                {
                    WowInterface.NewWowInterface.LuaCastSpell(StoneformSpell);
                }
            }

            // Useable items, potions, etc.
            // ---------------------------- >

            if (WowInterface.Player.HealthPercentage < 20)
            {
                IWowItem healthItem = WowInterface.CharacterManager.Inventory.Items.FirstOrDefault(e => useableHealingItems.Contains(e.Id));

                if (healthItem != null)
                {
                    WowInterface.NewWowInterface.LuaUseItemByName(healthItem.Name);
                }
            }

            if (WowInterface.Player.ManaPercentage < 20)
            {
                IWowItem manaItem = WowInterface.CharacterManager.Inventory.Items.FirstOrDefault(e => useableManaItems.Contains(e.Id));

                if (manaItem != null)
                {
                    WowInterface.NewWowInterface.LuaUseItemByName(manaItem.Name);
                }
            }
        }

        public abstract void ExecuteCC();

        public bool IsSpellReady(string spellName)
        {
            if (DateTime.Now > spellCoolDown[spellName])
            {
                spellCoolDown[spellName] = DateTime.Now + TimeSpan.FromMilliseconds(WowInterface.NewWowInterface.LuaGetSpellCooldown(spellName));
                return true;
            }

            return false;
        }

        public bool IsTargetAttackable(WowUnit target)
        {
            return true;
        }

        public abstract void OutOfCombatExecute();

        public void revivePartyMember(string reviveSpellName)
        {
            List<WowUnit> partyMemberToHeal = new List<WowUnit>(WowInterface.Objects.Partymembers)
            {
                WowInterface.Player
            };

            partyMemberToHeal = partyMemberToHeal.Where(e => e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

            if (revivePlayerEvent.Run() && partyMemberToHeal.Count > 0)
            {
                WowInterface.NewWowInterface.WowTargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                CustomCastSpellMana(reviveSpellName);
            }
        }

        public void Targetselection()
        {
            if (TargetSelectEvent.Run())
            {
                WowUnit nearTarget = WowInterface.Objects.GetNearEnemies<WowUnit>(WowInterface.Db.GetReaction, WowInterface.Player.Position, 50)
                .Where(e => !e.IsNotAttackable && (e.Type == WowObjectType.Player && (e.IsPvpFlagged && WowInterface.Db.GetReaction(e, WowInterface.Player) != WowUnitReaction.Friendly) || (e.IsInCombat)) || (e.IsInCombat && WowInterface.Db.GetUnitName(e, out string name) && name != "The Lich King" && !(WowInterface.Objects.MapId == WowMapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346)))
                .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                .FirstOrDefault();//&& e.Type(Player)

                if (nearTarget != null)
                {
                    WowInterface.NewWowInterface.WowTargetGuid(nearTarget.Guid);

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
                WowUnit nearTargetToTank = WowInterface.Objects.GetEnemiesTargetingPartymembers<WowUnit>(WowInterface.Db.GetReaction, WowInterface.Player.Position, 60)
                .Where(e => e.IsInCombat && !e.IsNotAttackable && e.Type != WowObjectType.Player && WowInterface.Db.GetUnitName(WowInterface.Target, out string name) && name != "The Lich King" && name != "Anub'Rekhan" && !(WowInterface.Objects.MapId == WowMapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346))
                .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                .FirstOrDefault();

                if (nearTargetToTank != null)
                {
                    WowInterface.NewWowInterface.WowTargetGuid(nearTargetToTank.Guid);

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
                    WowUnit nearTarget = WowInterface.Objects.GetNearEnemies<WowUnit>(WowInterface.Db.GetReaction, WowInterface.Player.Position, 80)
                    .Where(e => e.IsInCombat && !e.IsNotAttackable && e.Type == WowObjectType.Player)
                    .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                    .FirstOrDefault();//&& e.Type(Player)

                    if (nearTarget != null)
                    {
                        WowInterface.NewWowInterface.WowTargetGuid(nearTarget.Guid);

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

        public override string ToString()
        {
            return $"[{WowClass}] [{Role}] {Displayname} ({Author})";
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
    }
}