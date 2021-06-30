using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AmeisenBotX.Core.Quest.Objects.Quests
{
    public delegate (WowObject, Vector3) BotQuestGetPosition();

    public class BotQuest : IBotQuest
    {
        public BotQuest(AmeisenBotInterfaces bot, int id, string name, int level, int gossipId, BotQuestGetPosition start, BotQuestGetPosition end, List<IQuestObjective> objectives)
        {
            Bot = bot;

            Id = id;
            Name = name;
            Level = level;
            GossipId = gossipId;
            GetStartObject = start;
            GetEndObject = end;
            Objectives = objectives;

            ActionEvent = new(TimeSpan.FromMilliseconds(250));
        }

        public bool Accepted { get; set; }

        public TimegatedEvent ActionEvent { get; }

        public bool ActionToggle { get; set; }

        public bool Finished => (Objectives != null && Objectives.All(e => e.Finished)) || Progress == 100.0;

        public BotQuestGetPosition GetEndObject { get; set; }

        public BotQuestGetPosition GetStartObject { get; set; }

        public int GossipId { get; set; }

        public int Id { get; set; }

        public int Level { get; set; }

        public string Name { get; set; }

        public List<IQuestObjective> Objectives { get; set; }

        public double Progress
        {
            get
            {
                if (Objectives == null || Objectives.Count == 0) { return 100.0; }

                double totalProgress = 0;

                for (int i = 0; i < Objectives.Count; ++i)
                {
                    totalProgress += Objectives[i].Progress;
                }

                return Math.Round(totalProgress / Objectives.Count, 1);
            }
        }

        public bool Returned { get; set; }

        private AmeisenBotInterfaces Bot { get; }

        private bool CheckedIfAccepted { get; set; } = false;

        public void AcceptQuest()
        {
            if (!CheckedIfAccepted)
            {
                if (Bot.Wow.LuaGetQuestLogIdByTitle(Name, out int _questLogId))
                {
                    Accepted = true;
                }

                CheckedIfAccepted = true;
            }

            if (Accepted)
            {
                return;
            }

            (WowObject, Vector3) objectPositionCombo = GetStartObject();

            if (objectPositionCombo.Item1 != null)
            {
                if (Bot.Player.Position.GetDistance(objectPositionCombo.Item1.Position) > 5.0)
                {
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, objectPositionCombo.Item1.Position);
                }
                else if (ActionEvent.Run())
                {
                    if (!ActionToggle)
                    {
                        RightClickQuestgiver(objectPositionCombo.Item1);
                    }
                    else
                    {
                        Bot.Wow.LuaSelectQuestByNameOrGossipId(Name, GossipId, true);
                        Thread.Sleep(1000);
                        Bot.Wow.LuaAcceptQuest();
                        Thread.Sleep(250);

                        if (Bot.Wow.LuaGetQuestLogIdByTitle(Name, out int _questLogId))
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
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, objectPositionCombo.Item2);
                }
            }
        }

        public bool CompleteQuest()
        {
            if (Returned)
            {
                return true;
            }

            (WowObject, Vector3) objectPositionCombo = GetEndObject();

            if (objectPositionCombo.Item1 != null)
            {
                // move to unit / object
                if (Bot.Player.Position.GetDistance(objectPositionCombo.Item1.Position) > 5.0)
                {
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, objectPositionCombo.Item1.Position);
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
                        Bot.Wow.LuaSelectQuestByNameOrGossipId(Name, GossipId, false);
                        Thread.Sleep(1000);
                        Bot.Wow.LuaCompleteQuest();
                        Thread.Sleep(1000);

                        bool selectedReward = false;
                        // TODO: This only works for the english locale!
                        if (Bot.Wow.LuaGetQuestLogIdByTitle(Name, out int questLogId))
                        {
                            Bot.Wow.LuaSelectQuestLogEntry(questLogId);

                            if (Bot.Wow.LuaGetNumQuestLogChoices(out int numChoices))
                            {
                                for (int i = 1; i <= numChoices; ++i)
                                {
                                    if (Bot.Wow.LuaGetQuestLogChoiceItemLink(i, out string itemLink))
                                    {
                                        string itemJson = Bot.Wow.LuaGetItemJsonByNameOrLink(itemLink);
                                        WowBasicItem item = ItemFactory.BuildSpecificItem(ItemFactory.ParseItem(itemJson));

                                        if (item == null)
                                        {
                                            break;
                                        }

                                        if (item.Name == "0" || item.ItemLink == "0")
                                        {
                                            // get the item id and try again
                                            itemJson = Bot.Wow.LuaGetItemJsonByNameOrLink(
                                                itemLink.Split(new string[] { "Hitem:" }, StringSplitOptions.RemoveEmptyEntries)[1]
                                                    .Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);

                                            item = ItemFactory.BuildSpecificItem(ItemFactory.ParseItem(itemJson));
                                        }

                                        if (Bot.Character.IsItemAnImprovement(item, out IWowItem itemToReplace))
                                        {
                                            Bot.Wow.LuaGetQuestReward(i);
                                            Bot.Wow.LuaGetQuestReward(i);
                                            Bot.Wow.LuaGetQuestReward(i);
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
                            Bot.Wow.LuaGetQuestReward(1);
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
                    Bot.Movement.SetMovementAction(Movement.Enums.MovementAction.Move, objectPositionCombo.Item2);
                }
            }

            return false;
        }

        public void Execute()
        {
            Objectives.FirstOrDefault(e => !e.Finished)?.Execute();
        }

        private void RightClickQuestgiver(WowObject obj)
        {
            if (obj.GetType() == typeof(WowGameobject))
            {
                Bot.Wow.WowObjectRightClick(obj.BaseAddress);
            }
            else if (obj.GetType() == typeof(WowUnit))
            {
                Bot.Wow.WowUnitRightClick(obj.BaseAddress);
            }
        }
    }
}