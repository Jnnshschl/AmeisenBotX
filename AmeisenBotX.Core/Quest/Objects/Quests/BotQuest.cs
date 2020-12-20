using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Quest.Objects.Quests
{
    public delegate (WowObject, Vector3) BotQuestGetPosition();

    public class BotQuest : IBotQuest
    {
        public BotQuest(WowInterface wowInterface, int id, string name, int level, int gossipId, BotQuestGetPosition start, BotQuestGetPosition end, List<IQuestObjective> objectives)
        {
            WowInterface = wowInterface;

            Id = id;
            Name = name;
            Level = level;
            GossipId = gossipId;
            GetStartObject = start;
            GetEndObject = end;
            Objectives = objectives;

            ActionEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(250));
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

        private WowInterface WowInterface { get; }

        private bool CheckedIfAccepted { get; set; } = false;

        public void AcceptQuest()
        {
            if (!CheckedIfAccepted)
            {
                if (WowInterface.HookManager.LuaGetQuestLogIdByTitle(Name, out int _questLogId))
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
                if (WowInterface.ObjectManager.Player.Position.GetDistance(objectPositionCombo.Item1.Position) > 5.0)
                {
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, objectPositionCombo.Item1.Position);
                }
                else if (ActionEvent.Run())
                {
                    if (!ActionToggle)
                    {
                        RightClickQuestgiver(objectPositionCombo.Item1);
                    }
                    else
                    {
                        var acceptGossipId = GossipId;
                        if (WowInterface.HookManager.LuaGetGossipIdByAvailableQuestTitle(Name, out int gossipId))
                        {
                            acceptGossipId = gossipId;
                        }

                        WowInterface.HookManager.LuaSelectGossipAvailableQuest(acceptGossipId);
                        Thread.Sleep(250);
                        WowInterface.HookManager.LuaAcceptQuest();

                        Accepted = true;
                    }

                    ActionToggle = !ActionToggle;
                }
            }
            else if (objectPositionCombo.Item2 != default)
            {
                // move to position
                if (WowInterface.ObjectManager.Player.Position.GetDistance(objectPositionCombo.Item2) > 5.0)
                {
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, objectPositionCombo.Item2);
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
                if (WowInterface.ObjectManager.Player.Position.GetDistance(objectPositionCombo.Item1.Position) > 5.0)
                {
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, objectPositionCombo.Item1.Position);
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
                        var turnInGossipId = GossipId;
                        if (WowInterface.HookManager.LuaGetGossipIdByActiveQuestTitle(Name, out int gossipId))
                        {
                            turnInGossipId = gossipId;
                        }


                        WowInterface.HookManager.LuaSelectGossipActiveQuest(turnInGossipId);
                        Thread.Sleep(250);
                        WowInterface.HookManager.LuaCompleteQuest();
                        Thread.Sleep(250);

                        bool selectedReward = false;
                        if (WowInterface.HookManager.LuaGetGossipActiveQuestTitleById(gossipId,
                            out string selectedQuestTitle))
                        {
                            if (WowInterface.HookManager.LuaGetQuestLogIdByTitle(selectedQuestTitle, out int questLogId))
                            {
                                WowInterface.HookManager.LuaSelectQuestLogEntry(questLogId);
                                for (int i = 1; i <= 10; ++i)
                                {
                                    if (WowInterface.HookManager.LuaGetQuestLogChoiceItemLink(i, out string itemLink))
                                    {
                                        string itemJson = WowInterface.HookManager.LuaGetItemJsonByNameOrLink(itemLink);

                                        WowBasicItem item = ItemFactory.BuildSpecificItem(ItemFactory.ParseItem(itemJson));

                                        if (item.Name == "0" || item.ItemLink == "0")
                                        {
                                            // get the item id and try again
                                            itemJson = WowInterface.HookManager.LuaGetItemJsonByNameOrLink(
                                                itemLink.Split(new string[] { "Hitem:" }, StringSplitOptions.RemoveEmptyEntries)[1]
                                                    .Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0]);

                                            item = ItemFactory.BuildSpecificItem(ItemFactory.ParseItem(itemJson));
                                        }

                                        if (WowInterface.CharacterManager.IsItemAnImprovement(item,
                                            out IWowItem itemToReplace))
                                        {
                                            WowInterface.HookManager.LuaGetQuestReward(i);
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
                            WowInterface.HookManager.LuaGetQuestReward(1);
                        }

                        Returned = true;
                        return true;
                    }

                    ActionToggle = !ActionToggle;
                }
            }
            else if (objectPositionCombo.Item2 != default)
            {
                // move to position
                if (WowInterface.ObjectManager.Player.Position.GetDistance(objectPositionCombo.Item2) > 5.0)
                {
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, objectPositionCombo.Item2);
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
                WowInterface.HookManager.WowObjectRightClick(obj);
            }
            else if (obj.GetType() == typeof(WowUnit))
            {
                WowInterface.HookManager.WowUnitRightClick((WowUnit)obj);
            }
        }
    }
}