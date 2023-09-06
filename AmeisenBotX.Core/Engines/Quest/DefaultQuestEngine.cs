using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;
using AmeisenBotX.Core.Engines.Quest.Profiles;
using AmeisenBotX.Core.Engines.Quest.Profiles.Shino;
using AmeisenBotX.Core.Engines.Quest.Profiles.StartAreas;
using AmeisenBotX.Wow.Hook.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AmeisenBotX.Core.Engines.Quest
{
    public class DefaultQuestEngine : IQuestEngine
    {
        public DefaultQuestEngine(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            Profiles = new List<IQuestProfile>()
            {
                new DeathknightStartAreaQuestProfile(Bot),
                new X5Horde1To80Profile(Bot),
                new Horde1To60GrinderProfile(Bot)
            };

            CompletedQuests = new();
            QueryCompletedQuestsEvent = new(TimeSpan.FromSeconds(2));
        }

        public List<int> CompletedQuests { get; private set; }

        public ICollection<IQuestProfile> Profiles { get; init; }

        public IQuestProfile SelectedProfile { get; set; }

        public bool UpdatedCompletedQuests { get; set; }

        public AmeisenBotInterfaces Bot { get; set; }

        private DateTime LastAbandonQuestTime { get; set; } = DateTime.UtcNow;

        private TimegatedEvent QueryCompletedQuestsEvent { get; }

        public void Enter()
        {
            RegisterEvent("QUEST_ACCEPTED", OnQuestAccepted);
            RegisterEvent("QUEST_POI_UPDATE", OnQuestPoiUpdate);
            RegisterEvent("QUEST_QUERY_COMPLETE", OnGetQuestsCompleted);
            RegisterEvent("GOSSIP_SHOW", OnGossipShow);
            RegisterEvent("QUEST_GREETING", OnQuestGreeting);
            RegisterEvent("QUEST_PROGRESS", OnQuestProgress);
        }

        private void OnQuestProgress(long arg1, List<string> list)
        {
            
        }

        private void OnQuestGreeting(long arg1, List<string> list)
        {
            
        }

        private void OnGossipShow(long arg1, List<string> list)
        {
            
        }

        private void OnQuestPoiUpdate(long arg1, List<string> list)
        {
            // get questlog count
            
            // loop questlog
            
            // get poi icons
            // Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=TableConcat({{QuestPOIGetIconInfo({0})}})"), out string result);

            // get objective updates


        }

        private void RegisterEvent(string eventName, Action<long, List<string>> action)
        {
            if (!Bot.Wow.Events.Events.Any(t => t.Key == eventName))
            {
                Bot.Wow.Events.Subscribe(eventName, action);
            }
        }

        private void OnQuestAccepted(long arg1, List<string> list)
        {

            var questIndex = int.Parse(list.First());
            var questId = int.Parse(list.Skip(1).First());
            Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=\"\";TableConcat=function (t1,t2) local t = \"\"; for k,v in ipairs(t1) do t = t .. tostring(v) .. \";\" end; t = strsub(t, 0, strlen(t)-1); return t; end;"), out _);
            // Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=TableConcat({{QuestPOIGetIconInfo({questId})}})"), out string result);
            //GetQuestLogTitle(questLogIndex)
            GetQuestLogTitle(questIndex);

            Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=GetNumQuestLeaderBoards({questIndex})"), out string strNumberOfObjectives);

            if(int.TryParse(strNumberOfObjectives, out int objectiveCount))
            {
                for(int objectiveIndex = 1; objectiveIndex <= objectiveCount; objectiveIndex++)
                {
                    Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=TableConcat({{GetQuestLogLeaderBoard({objectiveIndex},{questIndex})}})"), out string objective);

                    var objectiveParts = objective.Split(';');
                    var description = objectiveParts.First();
                    var type = objectiveParts.Skip(1).FirstOrDefault();

                    switch (type)
                    {
                        case "item":
                            // KillAndLootQuestObjective
                            break;
                        case "monster":

                            break;
                    }

                    Regex robj = new Regex(@"(.+): (\d+)/(\d+)");
                    var match = robj.Match(description);
                    if(match.Success)
                    {
                        var itemName = match.Groups[1].Value;
                        var numItems = match.Groups[2].Value;
                        var itemsNeeded = match.Groups[3].Value;

                        
                    }
                }
            }
            
            // Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=TableConcat({{QuestPOIGetIconInfo({questId})}})"), out string result);
        }

        public void Execute()
        {
            if (SelectedProfile == null)
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



            if (SelectedProfile.Quests.Count > 0)
            {
                IEnumerable<IBotQuest> selectedQuests = SelectedProfile.Quests.Peek().Where(e => !e.Returned && (!CompletedQuests.Where(t => t > 0).Contains(e.Id)));

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
                    CompletedQuests.AddRange(SelectedProfile.Quests.Dequeue().Select(e => e.Id));
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

        private void GetQuestLogTitle(int questIndex)
        {
            Bot.Wow.ExecuteLuaAndRead(BotUtils.ObfuscateLua($"{{v:0}}=TableConcat({{GetQuestLogTitle({questIndex})}})"), out string result);
            var split = result.Split(';');

        }
    }
}