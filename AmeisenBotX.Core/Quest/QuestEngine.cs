using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest
{
    public class QuestEngine
    {
        public QuestEngine(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            Quests = new Queue<List<BotQuest>>();

            WowInterface.EventHookManager.Subscribe("QUEST_QUERY_COMPLETE", OnGetQuestsCompleted);

            CompletedQuests = new List<int>();
            QueryCompletedQuestsEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));

            Quests.Enqueue(new List<BotQuest>()
            {
                new BotQuest
                (
                    WowInterface, 12593, "In Service of the Lich King", 55, 1,
                    () => WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(24191),
                    () => WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(16582),
                    null
                ),
                new BotQuest
                (
                    WowInterface, 12619, "The Emblazoned Runeblade", 55, 1,
                    () => WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(16582),
                    () => WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(16582),
                    new List<IQuestObjective>()
                    {
                        new QuestObjectiveChain(new List<IQuestObjective>()
                        {
                            new CollectQuestObjective(WowInterface, 38607, 1, 7961, new List<AreaNode>()
                            {
                                new AreaNode(new Vector3(2504, -5563, 421), 32.0)
                            }),
                            new MoveToObjectQuestObjective(WowInterface, 8175, 8.0),
                            new UseItemQuestObjective(WowInterface, 38607, () => WowInterface.CharacterManager.Inventory.Items.Any(e => e.Id == 38631))
                        })
                    }
                ),
                new BotQuest
                (
                    WowInterface, 12842, "Preperation For Battle", 55, 1,
                    () => WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(16582),
                    () => WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(16582),
                    new List<IQuestObjective>()
                    {
                        new QuestObjectiveChain(new List<IQuestObjective>()
                        {
                            new MoveToObjectQuestObjective(WowInterface, 8175, 8.0),
                            new RuneforgingQuestObjective(WowInterface, () => WowInterface.CharacterManager.Equipment.HasEnchantment(EquipmentSlot.INVSLOT_MAINHAND, 3369)
                                                                           || WowInterface.CharacterManager.Equipment.HasEnchantment(EquipmentSlot.INVSLOT_MAINHAND, 3370))
                        })
                    }
                ),
                new BotQuest
                (
                    WowInterface, 12848, "The Endless Hunger", 55, 1,
                    () => WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(16582),
                    () => WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(16582),
                    new List<IQuestObjective>()
                    {
                        new QuestObjectiveChain(new List<IQuestObjective>()
                        {
                            new MoveToObjectQuestObjective(WowInterface, 8115, 4.0),
                            new UseObjectQuestObjective(WowInterface, 8115, () => WowInterface.ObjectManager.Player.GetQuestlogEntries().FirstOrDefault(e=>e.Id == 12848).X == 1)
                        })
                    }
                )
            });
        }

        public List<int> CompletedQuests { get; private set; }

        public Queue<List<BotQuest>> Quests { get; private set; }

        public bool UpdatedCompletedQuests { get; set; }

        private TimegatedEvent QueryCompletedQuestsEvent { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (!UpdatedCompletedQuests)
            {
                if (QueryCompletedQuestsEvent.Run())
                {
                    WowInterface.HookManager.QueryQuestsCompleted();
                }

                return;
            }

            if (Quests.Count > 0)
            {
                List<BotQuest> quests = Quests.Peek();
                BotQuest selectedQuest = quests.FirstOrDefault(e => (!e.Returned && !CompletedQuests.Contains(e.Id)) || WowInterface.ObjectManager.Player.GetQuestlogEntries().Any(x => x.Id == e.Id));

                if (selectedQuest != null)
                {
                    if (!selectedQuest.Accepted)
                    {
                        selectedQuest.AcceptQuest();
                        return;
                    }

                    if (selectedQuest.Finished)
                    {
                        selectedQuest.CompleteQuest();
                        CompletedQuests.Add(selectedQuest.Id);
                        return;
                    }

                    selectedQuest.Execute();
                }
                else
                {
                    CompletedQuests.AddRange(Quests.Dequeue().Select(e => e.Id));
                    return;
                }
            }
        }

        private void OnGetQuestsCompleted(long timestamp, List<string> args)
        {
            WowInterface.QuestEngine.CompletedQuests.Clear();
            WowInterface.QuestEngine.CompletedQuests.AddRange(WowInterface.HookManager.GetCompletedQuests());

            WowInterface.QuestEngine.UpdatedCompletedQuests = true;
        }
    }
}