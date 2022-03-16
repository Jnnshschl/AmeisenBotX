using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Combat.Helpers.Healing;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow548.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis.Mop548
{
    public class PaladinHoly : BasicCombatClass
    {
        public PaladinHoly(AmeisenBotInterfaces bot) : base(bot)
        {
            Configurables.TryAdd("AttackInGroups", true);
            Configurables.TryAdd("AttackInGroupsUntilManaPercent", 85.0);
            Configurables.TryAdd("AttackInGroupsCloseCombat", false);
            Configurables.TryAdd("BeaconOfLightSelfHealth", 85.0);
            Configurables.TryAdd("BeaconOfLightPartyHealth", 85.0);
            Configurables.TryAdd("DivinePleaMana", 60.0);

            // MyAuraManager.Jobs.Add(new KeepBestActiveAuraJob(bot.Db, new List<(string,
            // Func<bool>)>() { (Paladin548.SealOfInsight, () =>
            // TryCastSpell(Paladin548.SealOfInsight, 0, true)), (Paladin548.SealOfTruth, () =>
            // TryCastSpell(Paladin548.SealOfTruth, 0, true)), }));

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((Paladin548.BlessingOfKings, (spellName, guid) => TryCastSpell(spellName, guid, true)));

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

                if (Bot.Character.SpellBook.TryGetSpellByName(Paladin548.HolyLight, out Spell spellHolyLight))
                {
                    HealingManager.AddSpell(spellHolyLight);
                }

                if (Bot.Character.SpellBook.TryGetSpellByName(Paladin548.DivineLight, out Spell spellDivineLight))
                {
                    HealingManager.AddSpell(spellDivineLight);
                }
            };

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(Paladin548.FistOfJustice, x.Guid, true) },
                { 1, (x) => TryCastSpell(Paladin548.HammerOfJustice, x.Guid, true) },
                { 2, (x) => TryCastSpell(Paladin548.Rebuke, x.Guid, true) },
            };

            // SpellAbortFunctions.Add(HealingManager.ShouldAbortCasting);
            ChangeBeaconEvent = new(TimeSpan.FromSeconds(1));
        }

        public override string Description => "Beta CombatClass for the Holy Paladin spec.";

        public override string DisplayName2 => "Paladin Holy";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicComparator
        (
            null,
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

        public override WowVersion WowVersion => WowVersion.MoP548;

        private TimegatedEvent ChangeBeaconEvent { get; }

        private HealingManager HealingManager { get; }

        public override void Execute()
        {
            base.Execute();

            IWowUnit dyingUnit = Bot.Objects.Partymembers.FirstOrDefault(e => e.HealthPercentage < 14.0 && !e.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Paladin548.Forbearance));

            if (dyingUnit != null)
            {
                if (TryCastSpell(Paladin548.LayOnHands, dyingUnit.Guid, true))
                {
                    return;
                }

                if (TryCastSpell(Paladin548.HandOfProtection, dyingUnit.Guid, true))
                {
                    return;
                }
            }

            IWowUnit shieldWorthyUnit = Bot.Objects.Partymembers.FirstOrDefault(e => e.HealthPercentage < 20.0 && !e.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Paladin548.Forbearance));

            if (shieldWorthyUnit != null)
            {
                if (TryCastSpell(Paladin548.DivineShield, shieldWorthyUnit.Guid, true))
                {
                    return;
                }
            }

            if (HealingManager.Tick())
            {
                return;
            }
            else
            {
                if (!Bot.Objects.Partymembers.Any(e => e.HealthPercentage < 85.0))
                {
                    IEnumerable<IWowUnit> lowHpUnits = Bot.Objects.Partymembers.Where(e => e.HealthPercentage < 95.0);

                    if (lowHpUnits.Any())
                    {
                        if (TryCastSpell(Paladin548.DivineProtection, lowHpUnits.First().Guid, true))
                        {
                            return;
                        }
                    }
                }

                IWowUnit movementImpairedUnit = Bot.Objects.Partymembers.FirstOrDefault(e => e.IsConfused);

                if (movementImpairedUnit != null)
                {
                    if (TryCastSpell(Paladin548.HandOfFreedom, movementImpairedUnit.Guid, true))
                    {
                        return;
                    }
                }

                if (Bot.Player.ManaPercentage < Configurables["DivinePleaMana"]
                    && TryCastSpell(Paladin548.DivinePlea, 0, true))
                {
                    return;
                }

                if (ChangeBeaconEvent.Ready)
                {
                    if (Bot.Player.HealthPercentage < Configurables["BeaconOfLightSelfHealth"])
                    {
                        // keep beacon of light on us to reduce healing ourself
                        if (!Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Paladin548.BeaconOfLight)
                            && TryCastSpell(Paladin548.BeaconOfLight, Bot.Player.Guid, true))
                        {
                            ChangeBeaconEvent.Run();
                            return;
                        }
                    }
                    else
                    {
                        IEnumerable<IWowUnit> healableTargets = Bot.Wow.ObjectProvider.Partymembers.Where(e => e != null && !e.IsDead).OrderBy(e => e.HealthPercentage);

                        if (healableTargets.Count() > 1)
                        {
                            IWowUnit t = healableTargets.Skip(1).FirstOrDefault(e => e.HealthPercentage < Configurables["BeaconOfLightPartyHealth"]);

                            // keep beacon of light on second lowest target
                            if (t != null
                                && !t.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Paladin548.BeaconOfLight)
                                && TryCastSpell(Paladin548.BeaconOfLight, t.Guid, true))
                            {
                                ChangeBeaconEvent.Run();
                                return;
                            }
                        }
                    }
                }

                bool isAlone = !Bot.Objects.Partymembers.Any(e => e.Guid != Bot.Player.Guid);

                if ((isAlone || (Configurables["AttackInGroups"] && Configurables["AttackInGroupsUntilManaPercent"] < Bot.Player.ManaPercentage))
                    && TryFindTarget(TargetProviderDps, out _))
                {
                    if (Bot.Player.HolyPower > 0
                        && Bot.Player.HealthPercentage < 85.0
                        && TryCastAoeSpell(Paladin548.WordOfGlory, Bot.Player.Guid))
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

                            if (TryCastSpell(Paladin548.CrusaderStrike, Bot.Wow.TargetGuid, true))
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (Bot.Target.HealthPercentage < 20.0
                                && TryCastSpell(Paladin548.HammerOfWrath, Bot.Wow.TargetGuid, true))
                            {
                                return;
                            }

                            if (TryCastSpell(Paladin548.Judgment, Bot.Wow.TargetGuid, true))
                            {
                                return;
                            }

                            if (!Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Paladin548.Denounce)
                                && TryCastSpell(Paladin548.Denounce, Bot.Wow.TargetGuid, true))
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

            if (HealingManager.Tick())
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
    }
}