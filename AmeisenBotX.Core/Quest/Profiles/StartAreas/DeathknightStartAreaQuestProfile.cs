using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using AmeisenBotX.Core.Quest.Units.Unitives;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

namespace AmeisenBotX.Core.Quest.Profiles.StartAreas
{
    public class DeathknightStartAreaQuestProfile : IQuestProfile
    {
        public DeathknightStartAreaQuestProfile(WowInterface wowInterface)
        {
            Quests = new Queue<List<BotQuest>>();

            Quests.Enqueue
            (
                new List<BotQuest>()
                {
                    new BotQuest
                    (
                        wowInterface, 12593, "In Service of the Lich King", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 24191 }), default),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 16582 }), default),
                        null
                    )
                }
            );

            Quests.Enqueue
            (
                new List<BotQuest>()
                {
                    new BotQuest
                    (
                        wowInterface, 12619, "The Emblazoned Runeblade", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 16582 }), default),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 16582 }), default),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new CollectQuestObjective(wowInterface, 38607, 1, 7961, new List<AreaNode>()
                                {
                                    new AreaNode(new Vector3(2504, -5563, 421), 32.0)
                                }),
                                new MoveToObjectQuestObjective(wowInterface, 8175, 8.0),
                                new UseItemQuestObjective(wowInterface, 38607, () => wowInterface.CharacterManager.Inventory.Items.Any(e => e.Id == 38631))
                            })
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<BotQuest>()
                {
                    new BotQuest
                    (
                        wowInterface, 12842, "Preperation For Battle", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 16582 }), default),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 16582 }), default),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToObjectQuestObjective(wowInterface, 8175, 8.0),
                                new RuneforgingQuestObjective(wowInterface, () => wowInterface.CharacterManager.Equipment.HasEnchantment(EquipmentSlot.INVSLOT_MAINHAND, 3369)
                                                                               || wowInterface.CharacterManager.Equipment.HasEnchantment(EquipmentSlot.INVSLOT_MAINHAND, 3370))
                            })
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<BotQuest>()
                {
                    new BotQuest
                    (
                        wowInterface, 12848, "The Endless Hunger", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 16582 }), default),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 16582 }), default),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToObjectQuestObjective(wowInterface, 8115, 4.0),
                                new UseObjectQuestObjective(wowInterface, 8115, () => wowInterface.ObjectManager.Player.GetQuestlogEntries().FirstOrDefault(e => e.Id == 12848).Finished == 1)
                            })
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<BotQuest>()
                {
                    new BotQuest
                    (
                        wowInterface, 12636, "The Eye Of Acherus", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 16582 }), default),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 24191 }), default),
                        null
                    )
                }
            );

            Quests.Enqueue
            (
                new List<BotQuest>()
                {
                    new BotQuest
                    (
                        wowInterface, 12641, "Death Comes From On High", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 24191 }), default),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 24191 }), default),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToObjectQuestObjective(wowInterface, 8123, 5.0),
                                new UseObjectQuestObjective(wowInterface, 8123, () => wowInterface.ObjectManager.PetGuid > 0),
                                new WaitUntilQuestObjective(() => wowInterface.ObjectManager.Pet != null && wowInterface.ObjectManager.Pet.Position.GetDistance(new Vector3(1758, -5876, 166)) < 16.0),
                                new MovePetToPositionQuestObjective(wowInterface, new Vector3(1813, -5991, 131), 4.0, MovementAction.DirectMove),
                                new CastPetSpellQuestObjective(wowInterface, 51859, () => CheckEyeCastingState(wowInterface, 0, new Vector3(1813, -5991, 131), 5.0)),
                                new MovePetToPositionQuestObjective(wowInterface, new Vector3(1652, -5996, 151), 4.0, MovementAction.DirectMove),
                                new CastPetSpellQuestObjective(wowInterface, 51859, () => CheckEyeCastingState(wowInterface, 1, new Vector3(1652, -5996, 151), 5.0)),
                                new MovePetToPositionQuestObjective(wowInterface, new Vector3(1601, -5738, 140), 4.0, MovementAction.DirectMove),
                                new CastPetSpellQuestObjective(wowInterface, 51859, () => CheckEyeCastingState(wowInterface, 2, new Vector3(1601, -5738, 140), 5.0)),
                                new MovePetToPositionQuestObjective(wowInterface, new Vector3(1392, -5704, 162), 4.0, MovementAction.DirectMove),
                                new CastPetSpellQuestObjective(wowInterface, 51859, () => CheckEyeCastingState(wowInterface, 3, new Vector3(1392, -5704, 162), 5.0)),
                                new CastPetSpellQuestObjective(wowInterface, 52694, () => CastedSpell[3] == true && wowInterface.ObjectManager.PetGuid == 0)
                            })
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<BotQuest>()
                {
                    new BotQuest
                    (
                        wowInterface, 12657, "The Might Of The Scourge", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 24191 }), default),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 25444 }), default),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToPositionQuestObjective(wowInterface, new Vector3(2385, -5645, 421), 3.0),
                                new WaitUntilQuestObjective(() => wowInterface.ObjectManager.Player.Position.Z < 390.0 && wowInterface.ObjectManager.Player.Position.Z > 350.0)
                            })
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<BotQuest>()
                {
                    new BotQuest
                    (
                        wowInterface, 12850, "Report To Scourge Commander Thalanor", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 25444 }), default),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 25496 }), default),
                        new List<IQuestObjective>()
                        {
                            new MoveToPositionQuestObjective(wowInterface, new Vector3(2348, -5670, 382), 40.0)
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<BotQuest>()
                {
                    new BotQuest
                    (
                        wowInterface, 12670, "The Scarlet Harvest", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 25496 }), default),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 25514 }), default),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToUnitQuestObjective(wowInterface, 26308, 3.0),
                                new UseUnitQuestObjective(wowInterface, 26308, false, () => wowInterface.ObjectManager.Player.Position.GetDistance(new Vector3(2430, -5730, 158)) < 8.0),
                                new WaitUntilQuestObjective(() => wowInterface.ObjectManager.Player.Position.Z < 180.0)
                            })
                        }
                    )
                }
            );

            Quests.Enqueue
            (
                new List<BotQuest>()
                {
                    new BotQuest
                    (
                        wowInterface, 12678, "If Chaos Drives, Let Suffering Hold The Reins", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 25514 }), new Vector3(2340, -5687, 154)),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 25514 }), new Vector3(2340, -5687, 154)),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToPositionQuestObjective(wowInterface, new Vector3(2088, -5744, 100), 60.0),
                                new KillUnitQuestObjective(wowInterface, new Dictionary<int, int> { { 0, 25506 }, { 1, 25509 }, { 2, 25555 }, { 3, 25558 }, { 4, 10311 }, { 5, 24573 }, { 6, 25504 } }, () => wowInterface.ObjectManager.Player.GetQuestlogEntries().FirstOrDefault(e => e.Id == 12678).Finished == 1)
                            })
                        }
                    ),
                    new BotQuest
                    (
                        wowInterface, 12680, "Grand Theft Palomino", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 16416 }), new Vector3(2340, -5687, 154)),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 16416 }), new Vector3(2340, -5687, 154)),
                        new List<IQuestObjective>()
                        {
                            new BotActionQuestObjective(() => wowInterface.Globals.IgnoreCombat = true),
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToPositionQuestObjective(wowInterface, new Vector3(2243, -5834, 101), 48.0),
                                new MoveToUnitQuestObjective(wowInterface, 25571, 32.0),
                                new UseUnitQuestObjective(wowInterface, 25571, false, () => wowInterface.ObjectManager.PetGuid > 0),
                                new MoveToPositionQuestObjective(wowInterface, new Vector3(2353, -5704, 153), 48.0),
                                new MoveToUnitQuestObjective(wowInterface, 16416, 16.0),
                                new CastPetSpellQuestObjective(wowInterface, 52264, () => wowInterface.ObjectManager.Player.GetQuestlogEntries().FirstOrDefault(e => e.Id == 12680).Finished == 1)
                            }),
                            new BotActionQuestObjective(() => wowInterface.Globals.IgnoreCombat = false)
                        }
                    ),
                    new BotQuest
                    (
                        wowInterface, 12679, "Tonight We Dine In Havenshire", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 25589 }), new Vector3(2340, -5687, 154)),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 25589 }), new Vector3(2340, -5687, 154)),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToPositionQuestObjective(wowInterface, new Vector3(2088, -5795, 101), 64.0),
                                new MoveToObjectQuestObjective(wowInterface, 8094, 4.0),
                                new UseObjectQuestObjective(wowInterface, 8094, () => wowInterface.CharacterManager.Inventory.Items.FirstOrDefault(e => e.Id == 39160)?.Count >= 15),
                            })
                        }
                    ),
                    new BotQuest
                    (
                        wowInterface, 12733, "Death's Challenge", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 26762 }), new Vector3(2340, -5687, 154)),
                        () => (wowInterface.ObjectManager.GetClosestWowUnitQuestgiverByDisplayId(new List<int> { 26762 }), new Vector3(2340, -5687, 154)),
                        new List<IQuestObjective>()
                        {
                            new QuestObjectiveChain(new List<IQuestObjective>()
                            {
                                new MoveToPositionQuestObjective(wowInterface, new Vector3(2340, -5687, 154), 40.0),
                                new WaitUntilQuestObjective(() => wowInterface.ObjectManager.Player.HealthPercentage > 50.0),
                                new TalkToUnitQuestObjective(wowInterface, new List<int>(){ 25375, 25412, 25426, 25375 }, new List<int>() { 1 }, false, () => wowInterface.ObjectManager.Player.GetQuestlogEntries().FirstOrDefault(e => e.Id == 12733).Finished == 1)
                            })
                        }
                    ),
                    new BotQuest
                    (
                        wowInterface, 12711, "Abandonned Mail", 55, 1,
                        () => (wowInterface.ObjectManager.GetClosestWowGameobjectQuestgiverByDisplayId(new List<int> { 4851 }), new Vector3(2130, -5799, 99)),
                        () => (wowInterface.ObjectManager.GetClosestWowGameobjectQuestgiverByDisplayId(new List<int> { 4851 }), new Vector3(2130, -5799, 99)),
                        null
                    ),
                }
            );
        }

        public Queue<List<BotQuest>> Quests { get; }

        private bool StolenPalomino { get; set; }

        private bool[] CastedSpell { get; } = new bool[4];

        private bool CheckEyeCastingState(WowInterface wowInterface, int id, Vector3 positionToCast, double distance)
        {
            if (wowInterface.ObjectManager.Pet == null || positionToCast.GetDistance(wowInterface.ObjectManager.Pet.Position) > distance)
            {
                return false;
            }

            if (!CastedSpell[id])
            {
                CastedSpell[id] = wowInterface.ObjectManager.Pet.IsCasting;
                return false;
            }

            return !wowInterface.ObjectManager.Pet.IsCasting && CastedSpell[id];
        }
    }
}