using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Engines.Combat.Helpers.Healing;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow548.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis.Mop548
{
    public class PaladinHoly : BasicCombatClass548
    {
        public PaladinHoly(AmeisenBotInterfaces bot) : base(bot)
        {
            Configurables.TryAdd("AttackInGroups", true);
            Configurables.TryAdd("AttackInGroupsUntilManaPercent", 85.0);
            Configurables.TryAdd("AttackInGroupsCloseCombat", false);

            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Paladin548.BlessingOfKings, () => TryCastSpell(Paladin548.BlessingOfKings, Bot.Wow.PlayerGuid, true)));

            MyAuraManager.Jobs.Add(new KeepBestActiveAuraJob(bot.Db, new List<(string, Func<bool>)>()
            {
                (Paladin548.SealOfInsight, () => TryCastSpell(Paladin548.SealOfInsight, 0, true)),
                (Paladin548.SealOfTruth, () => TryCastSpell(Paladin548.SealOfTruth, 0, true)),
            }));

            HealingManager = new(bot, (string spellName, ulong guid) => { return TryCastSpell(spellName, guid); });

            // make sure all new spells get added to the healing manager
            Bot.Character.SpellBook.OnSpellBookUpdate += () =>
            {
                if (Bot.Character.SpellBook.TryGetSpellByName(Paladin548.FlashOfLight, out Spell spellFlashOfLight))
                {
                    HealingManager.AddSpell(spellFlashOfLight);
                }

                if (Bot.Character.SpellBook.TryGetSpellByName(Paladin548.HolyShock, out Spell spellHolyShock))
                {
                    HealingManager.AddSpell(spellHolyShock);
                }

                if (Bot.Character.SpellBook.TryGetSpellByName(Paladin548.LayOnHands, out Spell spellLayOnHands))
                {
                    HealingManager.AddSpell(spellLayOnHands);
                }

                if (Bot.Character.SpellBook.TryGetSpellByName(Paladin548.HolyRadiance, out Spell spellHolyRadiance))
                {
                    HealingManager.AddSpell(spellHolyRadiance);
                }

                if (Bot.Character.SpellBook.TryGetSpellByName(Paladin548.HolyLight, out Spell spellHolyLight))
                {
                    HealingManager.AddSpell(spellHolyLight);
                }
            };

            SpellAbortFunctions.Add(HealingManager.ShouldAbortCasting);
        }

        public override string Description => "Beta CombatClass for the Holy Paladin spec.";

        public override string DisplayName2 => "Paladin Holy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicComparator
        (
            new()
            {
                WowArmorType.Cloth,
                WowArmorType.Leather
            },
            new() { WowWeaponType.AxeTwoHand, WowWeaponType.MaceTwoHand, WowWeaponType.SwordTwoHand },
            new Dictionary<string, double>()
            {
                { "ITEM_MOD_CRIT_RATING_SHORT", 0.88 },
                { "ITEM_MOD_INTELLECT_SHORT", 0.2 },
                { "ITEM_MOD_SPELL_POWER_SHORT", 0.68 },
                { "ITEM_MOD_HASTE_RATING_SHORT", 0.71},
            }
        );

        public override WowRole Role => WowRole.Heal;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
            },
            Tree2 = new()
            {
            },
            Tree3 = new()
            {
            },
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Paladin;

        private HealingManager HealingManager { get; }

        public override void Execute()
        {
            base.Execute();

            if (NeedToHealSomeone())
            {
                return;
            }
            else
            {
                bool isAlone = !Bot.Objects.Partymembers.Any(e => e.Guid != Bot.Player.Guid);

                if ((isAlone || (Configurables["AttackInGroups"] && Configurables["AttackInGroupsUntilManaPercent"] < Bot.Player.ManaPercentage))
                    && SelectTarget(TargetProviderDps))
                {
                    if (Bot.Player.HolyPower > 0
                        && Bot.Player.HealthPercentage < 85.0
                        && TryCastAoeSpell(Paladin548.WordOfGlory, Bot.Player.Guid))
                    {
                        return;
                    }

                    if (Bot.Target.IsCasting
                        && TryCastSpell(Paladin548.FistOfJustice, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }

                    // either we are alone or allowed to go close combat in groups
                    if (isAlone || Configurables["AttackInGroupsCloseCombat"])
                    {
                        if (Bot.Player.IsInMeleeRange(Bot.Target))
                        {
                            if (EventAutoAttack.Run())
                            {
                                Bot.Wow.StartAutoAttack();
                            }

                            if (Bot.Target.IsCasting
                                && TryCastSpell(Paladin548.HammerOfJustice, Bot.Wow.TargetGuid, true))
                            {
                                return;
                            }

                            if (TryCastSpell(Paladin548.CrusaderStrike, Bot.Wow.TargetGuid, true))
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (TryCastSpell(Paladin548.Judgment, Bot.Wow.TargetGuid, true))
                            {
                                return;
                            }

                            if (TryCastSpell(Paladin548.Denounce, Bot.Wow.TargetGuid, true))
                            {
                                return;
                            }

                            Bot.Movement.SetMovementAction(MovementAction.Move, Bot.Target.Position);
                        }
                    }
                }
            }
        }

        public override void Load(Dictionary<string, JsonElement> objects)
        {
            base.Load(objects);

            if (objects.TryGetValue("HealingManager", out JsonElement elementHealingManager))
            {
                HealingManager.Load(elementHealingManager.To<Dictionary<string, JsonElement>>());
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealSomeone())
            {
                return;
            }
        }

        public override Dictionary<string, object> Save()
        {
            Dictionary<string, object> s = base.Save();
            s.Add("HealingManager", HealingManager.Save());
            return s;
        }

        private bool NeedToHealSomeone()
        {
            return HealingManager.Tick();
        }
    }
}