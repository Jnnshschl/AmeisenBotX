using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Quest.Objects;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using AmeisenBotX.Core.Engines.Quest.Units.Unitives;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Profiles.StartAreas
{
    public class DeathknightStartAreaQuestProfile : IQuestProfile
    {
        public DeathknightStartAreaQuestProfile(AmeisenBotInterfaces bot)
        {
            Quests = new Queue<List<IBotQuest>>();

            Quests.Enqueue
            (
                new List<IBotQuest>()
                {
                    new BotQuest
                    (
                        bot, 12593, "In Service of the Lich King", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 24191 }), default),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 16582 }), default),
                        null
                    )
                }
            );

            Quests.Enqueue
            (
                new List<IBotQuest>()
                {
                    new BotQuest
                    (
                        bot, 12619, "The Emblazoned Runeblade", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 16582 }), default),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 16582 }), default),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new CollectQuestObjectiveDEPRECATED(bot, 38607, 1, 7961, new List<AreaNode>()
                                {
                                    new AreaNode(new Vector3(2504, -5563, 421), 32.0)
                                }),
                                new MoveToObjectQuestObjective(bot, 8175, 8.0),
                                new UseItemQuestObjective(bot, 38607, () => bot.Character.Inventory.Items.Any(e => e.Id == 38631))
                            })
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<IBotQuest>()
                {
                    new BotQuest
                    (
                        bot, 12842, "Preperation For Battle", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 16582 }), default),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 16582 }), default),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToObjectQuestObjective(bot, 8175, 8.0),
                                new RuneforgingQuestObjective(bot, () => bot.Character.Equipment.HasEnchantment(WowEquipmentSlot.INVSLOT_MAINHAND, 3369)
                                                                      || bot.Character.Equipment.HasEnchantment(WowEquipmentSlot.INVSLOT_MAINHAND, 3370))
                            })
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<IBotQuest>()
                {
                    new BotQuest
                    (
                        bot, 12848, "The Endless Hunger", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 16582 }), default),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 16582 }), default),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToObjectQuestObjective(bot, 8115, 4.0),
                                new UseObjectQuestObjective(bot, 8115, () => bot.Objects.Player.QuestlogEntries.FirstOrDefault(e => e.Id == 12848).Finished == 1)
                            })
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<IBotQuest>()
                {
                    new BotQuest
                    (
                        bot, 12636, "The Eye Of Acherus", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 16582 }), default),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 24191 }), default),
                        null
                    )
                }
            );

            Quests.Enqueue
            (
                new List<IBotQuest>()
                {
                    new BotQuest
                    (
                        bot, 12641, "Death Comes From On High", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 24191 }), default),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 24191 }), default),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToObjectQuestObjective(bot, 8123, 5.0),
                                new UseObjectQuestObjective(bot, 8123, () => bot.Objects.Vehicle != null),
                                new WaitUntilQuestObjective(() => bot.Objects.Vehicle != null && bot.Objects.Vehicle.Position.GetDistance(new Vector3(1758, -5876, 166)) < 32.0),
                                new MoveVehicleToPositionQuestObjective(bot, new Vector3(1813, -5991, 131), 4.0, MovementAction.DirectMove),
                                new CastVehicleSpellQuestObjective(bot, 51859, () => CheckEyeCastingState(bot, 0, new Vector3(1813, -5991, 131), 5.0)),
                                new MoveVehicleToPositionQuestObjective(bot, new Vector3(1652, -5996, 151), 4.0, MovementAction.DirectMove),
                                new CastVehicleSpellQuestObjective(bot, 51859, () => CheckEyeCastingState(bot, 1, new Vector3(1652, -5996, 151), 5.0)),
                                new MoveVehicleToPositionQuestObjective(bot, new Vector3(1601, -5738, 140), 4.0, MovementAction.DirectMove),
                                new CastVehicleSpellQuestObjective(bot, 51859, () => CheckEyeCastingState(bot, 2, new Vector3(1601, -5738, 140), 5.0)),
                                new MoveVehicleToPositionQuestObjective(bot, new Vector3(1392, -5704, 162), 4.0, MovementAction.DirectMove),
                                new CastVehicleSpellQuestObjective(bot, 51859, () => CheckEyeCastingState(bot, 3, new Vector3(1392, -5704, 162), 5.0)),
                                new CastVehicleSpellQuestObjective(bot, 52694, () => CastedSpell[3] == true && bot.Objects.Vehicle == null)
                            })
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<IBotQuest>()
                {
                    new BotQuest
                    (
                        bot, 12657, "The Might Of The Scourge", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 24191 }), default),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 25444 }), default),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToPositionQuestObjective(bot, new Vector3(2385, -5645, 421), 2.5),
                                new WaitUntilQuestObjective(() => bot.Objects.Player.Position.Z < 390.0 && bot.Objects.Player.Position.Z > 350.0)
                            })
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<IBotQuest>()
                {
                    new BotQuest
                    (
                        bot, 12850, "Report To Scourge Commander Thalanor", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 25444 }), default),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 25496 }), default),
                        new List<IQuestObjective>()
                        {
                            new MoveToPositionQuestObjective(bot, new Vector3(2348, -5670, 382), 40.0)
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<IBotQuest>()
                {
                    new BotQuest
                    (
                        bot, 12670, "The Scarlet Harvest", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 25496 }), default),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 25514 }), default),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToUnitQuestObjective(bot, 26308, 3.0),
                                new UseUnitQuestObjective(bot, 26308, false, () => bot.Objects.Player.Position.GetDistance(new Vector3(2430, -5730, 158)) < 8.0),
                                new WaitUntilQuestObjective(() => bot.Objects.Player.Position.Z < 180.0)
                            })
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<IBotQuest>()
                {
                    new BotQuest
                    (
                        bot, 12678, "If Chaos Drives, Let Suffering Hold The Reins", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 25514 }), new Vector3(2340, -5687, 154)),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 25514 }), new Vector3(2340, -5687, 154)),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToPositionQuestObjective(bot, new Vector3(2088, -5744, 100), 60.0),
                                new KillUnitQuestObjective(bot, new Dictionary<int, int> { { 0, 25506 }, { 1, 25509 }, { 2, 25555 }, { 3, 25558 }, { 4, 10311 }, { 5, 24573 }, { 6, 25504 } }, () => bot.Objects.Player.QuestlogEntries.FirstOrDefault(e => e.Id == 12678).Finished == 1)
                            })
                        }
                    ),
                    new BotQuest
                    (
                        bot, 12680, "Grand Theft Palomino", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 16416 }), new Vector3(2340, -5687, 154)),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 16416 }), new Vector3(2340, -5687, 154)),
                        new List<IQuestObjective>()
                        {
                            // new BotActionQuestObjective(() => stateMachine.Get<StateCombat>().Mode = CombatMode.NotAllowed),
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToPositionQuestObjective(bot, new Vector3(2243, -5834, 101), 48.0),
                                new MoveToUnitQuestObjective(bot, 25571, 32.0),
                                new UseUnitQuestObjective(bot, 25571, false, () => bot.Objects.Pet.Guid > 0),
                                new MoveToPositionQuestObjective(bot, new Vector3(2353, -5704, 153), 48.0),
                                new MoveToUnitQuestObjective(bot, 16416, 16.0),
                                new CastVehicleSpellQuestObjective(bot, 52264, () => bot.Objects.Player.QuestlogEntries.FirstOrDefault(e => e.Id == 12680).Finished == 1)
                            }),
                            // new BotActionQuestObjective(() => stateMachine.Get<StateCombat>().Mode = CombatMode.Allowed)
                        }
                    ),
                    new BotQuest
                    (
                        bot, 12679, "Tonight We Dine In Havenshire", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 25589 }), new Vector3(2340, -5687, 154)),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 25589 }), new Vector3(2340, -5687, 154)),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToPositionQuestObjective(bot, new Vector3(2088, -5795, 101), 64.0),
                                new MoveToObjectQuestObjective(bot, 8094, 4.0),
                                new UseObjectQuestObjective(bot, 8094, () => bot.Character.Inventory.Items.FirstOrDefault(e => e.Id == 39160)?.Count >= 15),
                            })
                        }
                    ),
                    new BotQuest
                    (
                        bot, 12733, "Death's Challenge", 55, 1,
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 26762 }), new Vector3(2340, -5687, 154)),
                        () => (bot.GetClosestQuestGiverByDisplayId(bot.Player.Position, new List<int> { 26762 }), new Vector3(2340, -5687, 154)),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToPositionQuestObjective(bot, new Vector3(2340, -5687, 154), 40.0),
                                new WaitUntilQuestObjective(() => bot.Objects.Player.HealthPercentage > 50.0),
                                new TalkToUnitQuestObjective(bot, new List<int>(){ 25375, 25412, 25426, 25375 }, new List<int>() { 1 }, () => bot.Objects.Player.QuestlogEntries.FirstOrDefault(e => e.Id == 12733).Finished == 1)
                            })
                        }
                    ),
                    new BotQuest
                    (
                        bot, 12711, "Abandonned Mail", 55, 1,
                        () => (bot.GetClosestGameObjectByDisplayId(bot.Player.Position, new List<int> { 4851 }), new Vector3(2130, -5799, 99)),
                        () => (bot.GetClosestGameObjectByDisplayId(bot.Player.Position, new List<int> { 4851 }), new Vector3(2130, -5799, 99)),
                        null
                    ),
                }
            );
        }

        public Queue<List<IBotQuest>> Quests { get; }

        private bool[] CastedSpell { get; } = new bool[4];

        private bool[] StartedCasting { get; } = new bool[4];

        public override string ToString()
        {
            return $"[55-59] Deathknight Start Area (Jannis)";
        }

        private bool CheckEyeCastingState(AmeisenBotInterfaces bot, int id, Vector3 positionToCast, double distance)
        {
            if (CastedSpell[id])
            {
                return true;
            }
            else
            {
                if (bot.Objects.Vehicle == null)
                {
                    return false;
                }

                if (!StartedCasting[id])
                {
                    if (positionToCast.GetDistance(bot.Objects.Vehicle.Position) > distance)
                    {
                        return false;
                    }

                    StartedCasting[id] = bot.Objects.Vehicle.IsCasting;
                }
                else if (!bot.Objects.Vehicle.IsCasting)
                {
                    CastedSpell[id] = true;
                }
            }

            return false;
        }
    }
}