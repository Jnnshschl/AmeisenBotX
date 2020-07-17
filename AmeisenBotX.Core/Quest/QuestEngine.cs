using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Quest.Objects.Quests;
using AmeisenBotX.Core.Quest.Profiles;
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

            CompletedQuests = new List<int>();
            QueryCompletedQuestsEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
        }

        public List<int> CompletedQuests { get; private set; }

        public IQuestProfile Profile { get; set; }

        public bool UpdatedCompletedQuests { get; set; }

        private bool NeedToSetup { get; set; }

        private TimegatedEvent QueryCompletedQuestsEvent { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Profile == null)
            {
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

            if (Profile.Quests.Count > 0)
            {
                List<BotQuest> quests = Profile.Quests.Peek();
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
                            BotQuest notReturnedQuest = selectedQuests.FirstOrDefault(e => !e.Returned);

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
                    CompletedQuests.AddRange(Profile.Quests.Dequeue().Select(e => e.Id));
                    return;
                }
            }

            // filter duplicates
            CompletedQuests = CompletedQuests.Distinct().ToList();
        }

        public void Start()
        {
            if (NeedToSetup)
            {
                WowInterface.EventHookManager.Subscribe("QUEST_QUERY_COMPLETE", OnGetQuestsCompleted);
                NeedToSetup = false;
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