using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Quest.Objects.Quests;
using AmeisenBotX.Core.Quest.Profiles;
using AmeisenBotX.Core.Statemachine;
using AmeisenBotX.Core.Statemachine.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AmeisenBotX.Core.Data.Objects.WowObjects.Structs.SubStructs;

namespace AmeisenBotX.Core.Quest
{
    public class QuestEngine
    {
        public QuestEngine(WowInterface wowInterface, AmeisenBotConfig config, AmeisenBotStateMachine stateMachine)
        {
            WowInterface = wowInterface;
            Config = config;
            StateMachine = stateMachine;

            CompletedQuests = new List<int>();
            QueryCompletedQuestsEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
        }

        public List<int> CompletedQuests { get; private set; }

        public IQuestProfile Profile { get; set; }

        public bool UpdatedCompletedQuests { get; set; }

        private AmeisenBotConfig Config { get; }

        private TimegatedEvent QueryCompletedQuestsEvent { get; }

        private AmeisenBotStateMachine StateMachine { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Profile == null)
            {
                return;
            }

            if (!UpdatedCompletedQuests)
            {
                if (WowInterface.EventHookManager.EventDictionary.All(e => e.Key != "QUEST_QUERY_COMPLETE"))
                {
                    WowInterface.EventHookManager.Subscribe("QUEST_QUERY_COMPLETE", OnGetQuestsCompleted);
                }

                if (QueryCompletedQuestsEvent.Run())
                {
                    WowInterface.HookManager.LuaQueryQuestsCompleted();
                }

                return;
            }

            if (Profile.Quests.Count > 0)
            {
                // do i need to recover my hp
                if (WowInterface.ObjectManager.Player.HealthPercentage < Config.EatUntilPercent
                    && WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 60.0).Any())
                {
                    // wait or eat something

                    if (WowInterface.CharacterManager.HasItemTypeInBag<WowFood>() || WowInterface.CharacterManager.HasItemTypeInBag<WowRefreshment>())
                    {
                        StateMachine.SetState(BotState.Eating);
                        return;
                    }
                }

                IEnumerable<BotQuest> selectedQuests = Profile.Quests.Peek().Where(e => (!e.Returned && !CompletedQuests.Contains(e.Id)) || WowInterface.ObjectManager.Player.QuestlogEntries.Any(x => x.Id == e.Id));
                
                // Drop all quest that are not selected
                if (WowInterface.ObjectManager.Player.QuestlogEntries.Any(entry =>
                    selectedQuests.All(q => q.Id != entry.Id)))
                {
                    WowInterface.HookManager.LuaAbandonQuestsNotIn(selectedQuests.Select(q => q.Name));
                }

                if (selectedQuests.Any())
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
                            activeQuest?.Execute();
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
        }

        private void OnGetQuestsCompleted(long timestamp, List<string> args)
        {
            WowInterface.QuestEngine.CompletedQuests.Clear();
            WowInterface.QuestEngine.CompletedQuests.AddRange(WowInterface.HookManager.LuaGetCompletedQuests());

            WowInterface.QuestEngine.UpdatedCompletedQuests = true;
        }
    }
}