using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Quests
{
    public delegate WowObject BotQuestGetWowObject();

    public class BotQuest
    {
        public BotQuest(WowInterface wowInterface, int id, string name, int level, int gossipId, BotQuestGetWowObject start, BotQuestGetWowObject end, List<IQuestObjective> objectives)
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

        public bool Finished => (Objectives != null && Objectives.Count(e => !e.Finished) == 0) || Progress == 100.0;

        public BotQuestGetWowObject GetEndObject { get; set; }

        public BotQuestGetWowObject GetStartObject { get; set; }

        public bool HasQuest => WowInterface.ObjectManager.Player.GetQuestlogEntries().Any(e => e.Id == Id);

        public int Id { get; set; }

        public int GossipId { get; set; }

        public int Level { get; set; }

        public string Name { get; set; }

        public TimegatedEvent ActionEvent { get; }

        public List<IQuestObjective> Objectives { get; set; }

        public double Progress
        {
            get
            {
                if (Objectives == null || Objectives.Count == 0) { return 100.0; }

                double totalProgress = 0;

                foreach (IQuestObjective questObjective in Objectives)
                {
                    totalProgress += questObjective.Progress;
                }

                return Math.Round(totalProgress / (double)Objectives.Count, 1);
            }
        }

        public bool Returned { get; set; }

        public bool ActionToggle { get; set; }

        private WowInterface WowInterface { get; }

        public void AcceptQuest()
        {
            if (HasQuest) { Accepted = true; return; }

            WowObject start = GetStartObject();

            if (start != null)
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(start.Position) > 3.0)
                {
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, start.Position);
                }
                else if(ActionEvent.Run())
                {
                    if (!ActionToggle)
                    {
                        RightClickQuestgiver(start);
                    }
                    else
                    {
                        WowInterface.HookManager.AcceptQuest(GossipId);
                    }

                    ActionToggle = !ActionToggle;
                }
            }
        }

        public void CompleteQuest()
        {
            if (!HasQuest || !Finished) { Returned = true; return; }

            WowObject end = GetEndObject();

            if (end != null)
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(end.Position) > 3.0)
                {
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, end.Position);
                }
                else
                {
                    if (!ActionToggle)
                    {
                        RightClickQuestgiver(end);
                    }
                    else if (ActionEvent.Run())
                    {
                        // TODO: get best reward
                        WowInterface.HookManager.CompleteQuestAndGetReward(WowInterface.ObjectManager.Player.GetQuestlogEntries().FindIndex(e => e.Id == Id) + 1, 1);
                    }

                    ActionToggle = !ActionToggle;
                }
            }
        }

        public void Execute()
        {
            Objectives.FirstOrDefault(e => !e.Finished)?.Execute();
        }

        private void RightClickQuestgiver(WowObject obj)
        {
            if (obj.GetType() == typeof(WowGameobject))
            {
                WowInterface.HookManager.WowObjectOnRightClick(obj);
            }
            else if (obj.GetType() == typeof(WowUnit))
            {
                WowInterface.HookManager.UnitOnRightClick((WowUnit)obj);
            }
        }
    }
}