﻿using AmeisenBotX.Common.Utils;
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

        public bool TargetInLineOfSight => Bot.Objects.IsTargetInLineOfSight;

        public TimegatedEvent TargetSelectEvent { get; private set; }

        public abstract bool UseAutoAttacks { get; }

        public abstract string Version { get; }

        public abstract bool WalkBehindEnemy { get; }

        public abstract WowClass WowClass { get; }

        public AmeisenBotInterfaces Bot { get; internal set; }

        //follow the target
        public void AttackTarget()
        {
            WowUnit target = Bot.Target;
            if (target == null)
            {
                return;
            }

            if (Bot.Player.Position.GetDistance(target.Position) <= 3.0)
            {
                Bot.Wow.WowStopClickToMove();
                Bot.Movement.Reset();
                Bot.Wow.WowUnitRightClick(target.BaseAddress);
            }
            else
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);
            }
        }

        //Change target if target to far away
        public void ChangeTargetToAttack()
        {
            IEnumerable<WowPlayer> PlayerNearPlayer = Bot.Objects.GetNearEnemies<WowPlayer>(Bot.Db.GetReaction, Bot.Player.Position, 15);

            WowUnit target = Bot.Target;
            if (target == null)
            {
                return;
            }

            if (PlayerNearPlayer.Count() >= 1 && Bot.Objects.Target.HealthPercentage >= 60 && Bot.Player.Position.GetDistance(target.Position) >= 20)
            {
                Bot.Wow.WowClearTarget();
                return;
            }
        }

        public bool CheckForWeaponEnchantment(WowEquipmentSlot slot, string enchantmentName, string spellToCastEnchantment)
        {
            if (Bot.Character.Equipment.Items.ContainsKey(slot))
            {
                int itemId = Bot.Character.Equipment.Items[slot].Id;

                if (itemId > 0)
                {
                    if (slot == WowEquipmentSlot.INVSLOT_MAINHAND)
                    {
                        WowItem item = Bot.Objects.WowObjects.OfType<WowItem>().FirstOrDefault(e => e.EntryId == itemId);

                        if (item != null
                            && !item.GetEnchantmentStrings().Any(e => e.Contains(enchantmentName))
                            && CustomCastSpellMana(spellToCastEnchantment))
                        {
                            return true;
                        }
                    }
                    else if (slot == WowEquipmentSlot.INVSLOT_OFFHAND)
                    {
                        WowItem item = Bot.Objects.WowObjects.OfType<WowItem>().LastOrDefault(e => e.EntryId == itemId);

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
            if (Bot.Character.SpellBook.IsSpellKnown(spellName))
            {
                if (Bot.Target != null)
                {
                    Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                    if ((Bot.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        double distance = Bot.Player.Position.GetDistance(Bot.Target.Position);

                        if ((spell.MinRange == 0 && spell.MaxRange == 0) || (spell.MinRange <= distance && spell.MaxRange >= distance))
                        {
                            Bot.Wow.LuaCastSpell(spellName);
                            return true;
                        }
                    }
                }
                else
                {
                    Bot.Wow.WowTargetGuid(Bot.Wow.PlayerGuid);

                    Spell spell = Bot.Character.SpellBook.GetSpellByName(spellName);

                    if ((Bot.Player.Mana >= spell.Costs && IsSpellReady(spellName)))
                    {
                        Bot.Wow.LuaCastSpell(spellName);
                        return true;
                    }
                }
            }

            return false;
        }

        public void Execute()
        {
            ExecuteCC();

            if (Bot.Player.Race == WowRace.Human
            && (Bot.Player.IsDazed
                || Bot.Player.IsFleeing
                || Bot.Player.IsInfluenced
                || Bot.Player.IsPossessed))
            {
                if (IsSpellReady(EveryManforHimselfSpell))
                {
                    Bot.Wow.LuaCastSpell(EveryManforHimselfSpell);
                }
            }

            if (Bot.Player.HealthPercentage < 50.0
            && (Bot.Player.Race == WowRace.Dwarf))
            {
                if (IsSpellReady(StoneformSpell))
                {
                    Bot.Wow.LuaCastSpell(StoneformSpell);
                }
            }

            // Useable items, potions, etc.
            // ---------------------------- >

            if (Bot.Player.HealthPercentage < 20)
            {
                IWowItem healthItem = Bot.Character.Inventory.Items.FirstOrDefault(e => useableHealingItems.Contains(e.Id));

                if (healthItem != null)
                {
                    Bot.Wow.LuaUseItemByName(healthItem.Name);
                }
            }

            if (Bot.Player.ManaPercentage < 20)
            {
                IWowItem manaItem = Bot.Character.Inventory.Items.FirstOrDefault(e => useableManaItems.Contains(e.Id));

                if (manaItem != null)
                {
                    Bot.Wow.LuaUseItemByName(manaItem.Name);
                }
            }
        }

        public abstract void ExecuteCC();

        public bool IsSpellReady(string spellName)
        {
            if (DateTime.Now > spellCoolDown[spellName])
            {
                spellCoolDown[spellName] = DateTime.Now + TimeSpan.FromMilliseconds(Bot.Wow.LuaGetSpellCooldown(spellName));
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
            List<WowUnit> partyMemberToHeal = new List<WowUnit>(Bot.Objects.Partymembers)
            {
                Bot.Player
            };

            partyMemberToHeal = partyMemberToHeal.Where(e => e.IsDead).OrderBy(e => e.HealthPercentage).ToList();

            if (revivePlayerEvent.Run() && partyMemberToHeal.Count > 0)
            {
                Bot.Wow.WowTargetGuid(partyMemberToHeal.FirstOrDefault().Guid);
                CustomCastSpellMana(reviveSpellName);
            }
        }

        public void Targetselection()
        {
            if (TargetSelectEvent.Run())
            {
                WowUnit nearTarget = Bot.Objects.GetNearEnemies<WowUnit>(Bot.Db.GetReaction, Bot.Player.Position, 50)
                .Where(e => !e.IsNotAttackable && (e.Type == WowObjectType.Player && (e.IsPvpFlagged && Bot.Db.GetReaction(e, Bot.Player) != WowUnitReaction.Friendly) || (e.IsInCombat)) || (e.IsInCombat && Bot.Db.GetUnitName(e, out string name) && name != "The Lich King" && !(Bot.Objects.MapId == WowMapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346)))
                .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                .FirstOrDefault();//&& e.Type(Player)

                if (nearTarget != null)
                {
                    Bot.Wow.WowTargetGuid(nearTarget.Guid);

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
                WowUnit nearTargetToTank = Bot.Objects.GetEnemiesTargetingPartymembers<WowUnit>(Bot.Db.GetReaction, Bot.Player.Position, 60)
                .Where(e => e.IsInCombat && !e.IsNotAttackable && e.Type != WowObjectType.Player && Bot.Db.GetUnitName(Bot.Target, out string name) && name != "The Lich King" && name != "Anub'Rekhan" && !(Bot.Objects.MapId == WowMapId.DrakTharonKeep && e.CurrentlyChannelingSpellId == 47346))
                .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                .FirstOrDefault();

                if (nearTargetToTank != null)
                {
                    Bot.Wow.WowTargetGuid(nearTargetToTank.Guid);

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
                    WowUnit nearTarget = Bot.Objects.GetNearEnemies<WowUnit>(Bot.Db.GetReaction, Bot.Player.Position, 80)
                    .Where(e => e.IsInCombat && !e.IsNotAttackable && e.Type == WowObjectType.Player)
                    .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                    .FirstOrDefault();//&& e.Type(Player)

                    if (nearTarget != null)
                    {
                        Bot.Wow.WowTargetGuid(nearTarget.Guid);

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
            if (Bot.Character.Inventory.Items.Any(e => e.Name.Equals("Earth Totem", StringComparison.OrdinalIgnoreCase)) &&
             Bot.Character.Inventory.Items.Any(e => e.Name.Equals("Air Totem", StringComparison.OrdinalIgnoreCase)) &&
             Bot.Character.Inventory.Items.Any(e => e.Name.Equals("Water Totem", StringComparison.OrdinalIgnoreCase)) &&
             Bot.Character.Inventory.Items.Any(e => e.Name.Equals("Fire Totem", StringComparison.OrdinalIgnoreCase)) ||
             (Bot.Character.Equipment.Items.ContainsKey(WowEquipmentSlot.INVSLOT_RANGED) &&
             Bot.Character.Equipment.Items[WowEquipmentSlot.INVSLOT_RANGED] != null))
            {
                return true;
            }

            return false;
        }
    }
}