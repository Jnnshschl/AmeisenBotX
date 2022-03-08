using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Combat.Helpers.Healing;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Spells.Objects;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow548.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis548
{
    public class PaladinHoly : BasicCombatClass548
    {
        public PaladinHoly(AmeisenBotInterfaces bot) : base(bot)
        {
            Configurables.TryAdd("AttackInGroups", true);
            Configurables.TryAdd("AttackInGroupsUntilManaPercent", 85.0);
            Configurables.TryAdd("AttackInGroupsCloseCombat", false);

            // HealingManager = new(bot, (string spellName, ulong guid) => { return TryCastSpell(spellName, guid); });
            // 
            // // make sure all new spells get added to the healing manager
            // Bot.Character.SpellBook.OnSpellBookUpdate += () =>
            // {
            //     if (Bot.Character.SpellBook.TryGetSpellByName(Paladin548.FlashOfLight, out Spell spellFlashOfLight))
            //     {
            //         HealingManager.AddSpell(spellFlashOfLight);
            //     }
            // };
            // 
            // SpellAbortFunctions.Add(HealingManager.ShouldAbortCasting);
        }

        public override string Description => "Beta CombatClass for the Holy Paladin spec.";

        public override string DisplayName => "Paladin Holy 5.4.8";

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
                    // either we are alone or allowed to go close combat in groups
                    if (isAlone || Configurables["AttackInGroupsCloseCombat"])
                    {
                        if (Bot.Player.IsInMeleeRange(Bot.Target))
                        {
                            if (!Bot.Player.IsAutoAttacking && EventAutoAttack.Run())
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
                            Bot.Movement.SetMovementAction(MovementAction.Move, Bot.Target.Position);
                        }
                    }
                }
            }
        }

        public override void Load(Dictionary<string, JsonElement> objects)
        {
            base.Load(objects);

            if (objects.ContainsKey("HealingManager"))
            {
                Dictionary<string, JsonElement> s = objects["HealingManager"].To<Dictionary<string, JsonElement>>();

                // if (s.TryGetValue("SpellHealing", out JsonElement j)) { HealingManager.SpellHealing = j.To<Dictionary<string, int>>(); }
                // if (s.TryGetValue("DamageMonitorSeconds", out j)) { HealingManager.DamageMonitorSeconds = j.To<int>(); }
                // if (s.TryGetValue("HealthWeight", out j)) { HealingManager.HealthWeightMod = j.To<float>(); }
                // if (s.TryGetValue("DamageWeight", out j)) { HealingManager.IncomingDamageMod = j.To<float>(); }
                // if (s.TryGetValue("OverhealingStopThreshold", out j)) { HealingManager.OverhealingStopThreshold = j.To<float>(); }
                // if (s.TryGetValue("TargetDyingSeconds", out j)) { HealingManager.TargetDyingSeconds = j.To<int>(); }
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

            // s.Add("HealingManager", new Dictionary<string, object>()
            // {
            //     { "SpellHealing", HealingManager.SpellHealing },
            //     { "DamageMonitorSeconds", HealingManager.DamageMonitorSeconds },
            //     { "HealthWeight", HealingManager.HealthWeightMod },
            //     { "DamageWeight", HealingManager.IncomingDamageMod },
            //     { "OverhealingStopThreshold", HealingManager.OverhealingStopThreshold },
            //     { "TargetDyingSeconds", HealingManager.TargetDyingSeconds },
            // });

            return s;
        }

        private bool NeedToHealSomeone()
        {
            return false; // HealingManager.Tick();
        }
    }
}