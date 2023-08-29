using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using AmeisenBotX.Core.Engines.Quest.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest
{
    public class DefaultQuestEngine : IQuestEngine
    {
        public DefaultQuestEngine(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            CompletedQuests = new();
            QueryCompletedQuestsEvent = new(TimeSpan.FromSeconds(2));
        }

        public List<int> CompletedQuests { get; private set; }

        public IQuestProfile Profile { get; set; }

        public bool UpdatedCompletedQuests { get; set; }

        private AmeisenBotInterfaces Bot { get; }

        private DateTime LastAbandonQuestTime { get; set; } = DateTime.UtcNow;

        private TimegatedEvent QueryCompletedQuestsEvent { get; }

        public void Enter()
        {
        }

        public void Execute()
        {
            if (Profile == null)
            {
                return;
            }

            if (!UpdatedCompletedQuests)
            {
                if (Bot.Wow.Events.Events.All(e => e.Key != "QUEST_QUERY_COMPLETE"))
                {
                    Bot.Wow.Events.Subscribe("QUEST_QUERY_COMPLETE", OnGetQuestsCompleted);
                }

                if (QueryCompletedQuestsEvent.Run())
                {
                    Bot.Wow.QueryQuestsCompleted();
                }

                return;
            }

            if (Profile.Quests.Count > 0)
            {
                IEnumerable<IBotQuest> selectedQuests = Profile.Quests.Peek().Where(e => !e.Returned && !CompletedQuests.Contains(e.Id));

                // drop all quest that are not selected
                if (Bot.Player.QuestlogEntries?.Count() == 25 && DateTime.UtcNow.Subtract(LastAbandonQuestTime).TotalSeconds > 30)
                {
                    Bot.Wow.AbandonQuestsNotIn(selectedQuests.Select(q => q.Name));
                    LastAbandonQuestTime = DateTime.UtcNow;
                }

                if (selectedQuests.Any())
                {
                    IBotQuest notAcceptedQuest = selectedQuests.FirstOrDefault(e => !e.Accepted);

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
                            IBotQuest activeQuest = selectedQuests.FirstOrDefault(e => !e.Finished);
                            activeQuest?.Execute();
                        }
                        else
                        {
                            // make sure we return all quests
                            IBotQuest notReturnedQuest = selectedQuests.FirstOrDefault(e => !e.Returned);

                            if (notReturnedQuest != null)
                            {
                                if (notReturnedQuest.CompleteQuest())
                                {
                                    CompletedQuests.Add(notReturnedQuest.Id);
                                }

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

        private void OnGetQuestsCompleted(long timestamp, List<string> args)
        {
            Bot.Quest.CompletedQuests.Clear();
            Bot.Quest.CompletedQuests.AddRange(Bot.Wow.GetCompletedQuests());

            Bot.Quest.UpdatedCompletedQuests = true;
        }
    }
}