using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using AmeisenBotX.Core.Quest.Profiles;
using AmeisenBotX.Core.Quest.Profiles.StartAreas;
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

            WowInterface.EventHookManager.Subscribe("QUEST_QUERY_COMPLETE", OnGetQuestsCompleted);

            CompletedQuests = new List<int>();
            QueryCompletedQuestsEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
        }

        public void LoadProfile(IQuestProfile questProfile)
        {
            QuestProfile = questProfile;
        }

        public List<int> CompletedQuests { get; private set; }

        public IQuestProfile QuestProfile { get; private set; }

        public bool UpdatedCompletedQuests { get; set; }

        private TimegatedEvent QueryCompletedQuestsEvent { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (QuestProfile == null)
            {
                LoadProfile(new DeathknightStartAreaQuestProfile(WowInterface));
                return;
            }

            if (!UpdatedCompletedQuests)
            {
                if (QueryCompletedQuestsEvent.Run())
                {
                    WowInterface.HookManager.QueryQuestsCompleted();
                }

                return;
            }

            if (QuestProfile.Quests.Count > 0)
            {
                List<BotQuest> quests = QuestProfile.Quests.Peek();
                List<BotQuest> selectedQuests = quests.Where(e => (!e.Returned && !CompletedQuests.Contains(e.Id)) || WowInterface.ObjectManager.Player.GetQuestlogEntries().Any(x => x.Id == e.Id)).ToList();

                if (selectedQuests != null && selectedQuests.Count > 0)
                {
                    BotQuest notAcceptedQuest = selectedQuests.FirstOrDefault(e => !e.Accepted);

                    // make sure we got all quests
                    if (notAcceptedQuest != null)
                    {
                        if (!notAcceptedQuest.Accepted)
                        {
                            notAcceptedQuest.AcceptQuest();
                            return;
                        }
                    }
                    else
                    {
                        // do the quests if not all of them are finished
                        if (selectedQuests.Any(e => !e.Finished))
                        {
                            BotQuest activeQuest = selectedQuests.FirstOrDefault(e => !e.Finished);

                            if (activeQuest != null)
                            {
                                activeQuest.Execute();
                            }
                        }
                        else
                        {
                            // make sure we return all quests
                            BotQuest notReturnedQuest = selectedQuests.FirstOrDefault(e => !e.Accepted);

                            if (notReturnedQuest != null)
                            {
                                notReturnedQuest.CompleteQuest();
                                CompletedQuests.Add(notReturnedQuest.Id);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    CompletedQuests.AddRange(QuestProfile.Quests.Dequeue().Select(e => e.Id));
                    return;
                }
            }

            // filter duplicates
            CompletedQuests = CompletedQuests.Distinct().ToList();
        }

        private void OnGetQuestsCompleted(long timestamp, List<string> args)
        {
            WowInterface.QuestEngine.CompletedQuests.Clear();
            WowInterface.QuestEngine.CompletedQuests.AddRange(WowInterface.HookManager.GetCompletedQuests());

            WowInterface.QuestEngine.UpdatedCompletedQuests = true;
        }
    }
}