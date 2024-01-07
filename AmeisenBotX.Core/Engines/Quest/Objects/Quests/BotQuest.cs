using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using AmeisenBotX.Core.Managers.Character.Inventory;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Quests
{
    public delegate (IWowObject, Vector3) BotQuestGetPosition();

    public class BotQuest(AmeisenBotInterfaces bot, int id, string name, int level, int gossipId, BotQuestGetPosition start, BotQuestGetPosition end, List<IQuestObjective> objectives) : IBotQuest
    {
        public bool Accepted { get; set; }

        public TimegatedEvent ActionEvent { get; } = new(TimeSpan.FromMilliseconds(250));

        public bool ActionToggle { get; set; }

        public bool Finished => (Objectives != null && Objectives.All(e => e.Finished)) || Progress == 100.0;

        public BotQuestGetPosition GetEndObject { get; set; } = end;

        public BotQuestGetPosition GetStartObject { get; set; } = start;

        public int GossipId { get; set; } = gossipId;

        public int Id { get; set; } = id;

        public int Level { get; set; } = level;

        public string Name { get; set; } = name;

        public List<IQuestObjective> Objectives { get; set; } = objectives;

        public double Progress
        {
            get
            {
                if (Objectives == null || Objectives.Count == 0) { return 100.0; }

                double totalProgress = Objectives.Sum(questObjective => questObjective.Progress);

                return Math.Round(totalProgress / Objectives.Count, 1);
            }
        }

        public bool Returned { get; set; }

        private AmeisenBotInterfaces Bot { get; } = bot;

        private bool CheckedIfAccepted { get; set; } = false;

        public void AcceptQuest()
        {
            if (!CheckedIfAccepted)
            {
                if (Bot.Wow.GetQuestLogIdByTitle(Name, out _))
                {
                    Accepted = true;
                }

                CheckedIfAccepted = true;
            }

            if (Accepted)
            {
                return;
            }

            (IWowObject, Vector3) objectPositionCombo = GetStartObject();

            if (objectPositionCombo.Item1 != null)
            {
                if (Bot.Player.Position.GetDistance(objectPositionCombo.Item1.Position) > 5.0)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, objectPositionCombo.Item1.Position);
                }
                else if (ActionEvent.Run())
                {
                    if (!ActionToggle)
                    {
                        RightClickQuestgiver(objectPositionCombo.Item1);
                    }
                    else
                    {
                        Bot.Wow.SelectQuestByNameOrGossipId(Name, GossipId, true);
                        Thread.Sleep(1000);
                        Bot.Wow.AcceptQuest();
                        Thread.Sleep(250);

                        if (Bot.Wow.GetQuestLogIdByTitle(Name, out _))
                        {
                            Accepted = true;
                        }
                    }

                    ActionToggle = !ActionToggle;
                }
            }
            else if (objectPositionCombo.Item2 != default)
            {
                // move to position
                if (Bot.Player.Position.GetDistance(objectPositionCombo.Item2) > 5.0)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, objectPositionCombo.Item2);
                }
            }
        }

        public bool CompleteQuest()
        {
            if (Returned)
            {
                return true;
            }

            (IWowObject, Vector3) objectPositionCombo = GetEndObject();

            if (objectPositionCombo.Item1 != null)
            {
                // move to unit / object
                if (Bot.Player.Position.GetDistance(objectPositionCombo.Item1.Position) > 5.0)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, objectPositionCombo.Item1.Position);
                }
                else
                {
                    // interact with it
                    if (!ActionToggle)
                    {
                        RightClickQuestgiver(objectPositionCombo.Item1);
                    }
                    else if (ActionEvent.Run())
                    {
                        Bot.Wow.SelectQuestByNameOrGossipId(Name, GossipId, false);
                        Thread.Sleep(1000);
                        Bot.Wow.CompleteQuest();
                        Thread.Sleep(1000);

                        bool selectedReward = false;
                        // TODO: This only works for the english locale!
                        if (Bot.Wow.GetQuestLogIdByTitle(Name, out int questLogId))
                        {
                            Bot.Wow.SelectQuestLogEntry(questLogId);

                            if (Bot.Wow.GetNumQuestLogChoices(out int numChoices))
                            {
                                for (int i = 1; i <= numChoices; ++i)
                                {
                                    if (Bot.Wow.GetQuestLogChoiceItemLink(i, out string itemLink))
                                    {
                                        string itemJson = Bot.Wow.GetItemByNameOrLink(itemLink);
                                        WowBasicItem item = ItemFactory.BuildSpecificItem(ItemFactory.ParseItem(itemJson));

                                        if (item == null)
                                        {
                                            break;
                                        }

                                        if (item.Name == "0" || item.ItemLink == "0")
                                        {
                                            // get the item id and try again
                                            itemJson = Bot.Wow.GetItemByNameOrLink
                                            (
                                                itemLink.Split("Hitem:", StringSplitOptions.RemoveEmptyEntries)[1]
                                                        .Split(":", StringSplitOptions.RemoveEmptyEntries)[0]
                                            );

                                            item = ItemFactory.BuildSpecificItem(ItemFactory.ParseItem(itemJson));
                                        }

                                        if (Bot.Character.IsItemAnImprovement(item, out _))
                                        {
                                            Bot.Wow.SelectQuestReward(i);
                                            Bot.Wow.SelectQuestReward(i);
                                            Bot.Wow.SelectQuestReward(i);
                                            selectedReward = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        if (!selectedReward)
                        {
                            Bot.Wow.SelectQuestReward(1);
                        }

                        Thread.Sleep(250);
                        Returned = true;
                        return true;
                    }

                    ActionToggle = !ActionToggle;
                }
            }
            else if (objectPositionCombo.Item2 != default)
            {
                // move to position
                if (Bot.Player.Position.GetDistance(objectPositionCombo.Item2) > 5.0)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, objectPositionCombo.Item2);
                }
            }

            return false;
        }

        public void Execute()
        {
            Objectives.FirstOrDefault(e => !e.Finished)?.Execute();
        }

        private void RightClickQuestgiver(IWowObject obj)
        {
            if (obj.GetType() == typeof(IWowGameobject))
            {
                Bot.Wow.InteractWithObject(obj);
            }
            else if (obj.GetType() == typeof(IWowUnit))
            {
                Bot.Wow.InteractWithUnit((IWowUnit)obj);
            }
        }
    }
}