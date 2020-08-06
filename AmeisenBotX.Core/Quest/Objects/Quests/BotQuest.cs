using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Quests
{
    public delegate (WowObject, Vector3) BotQuestGetPosition();

    public class BotQuest
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

        public bool Finished => (Objectives != null && Objectives.Count(e => !e.Finished) == 0) || Progress == 100.0;

        public BotQuestGetPosition GetEndObject { get; set; }

        public BotQuestGetPosition GetStartObject { get; set; }

        public int GossipId { get; set; }

        public bool HasQuest => WowInterface.ObjectManager.Player.GetQuestlogEntries.Any(e => e.Id == Id);

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

                return Math.Round(totalProgress / (double)Objectives.Count, 1);
            }
        }

        public bool Returned { get; set; }

        private WowInterface WowInterface { get; }

        public void AcceptQuest()
        {
            if (HasQuest) { Accepted = true; return; }

            (WowObject, Vector3) objectPositionCombo = GetStartObject();

            if (objectPositionCombo.Item1 != null)
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(objectPositionCombo.Item1.Position) > 3.0)
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
                        WowInterface.HookManager.AcceptQuest(GossipId);
                    }

                    ActionToggle = !ActionToggle;
                }
            }
            else if (objectPositionCombo.Item2 != default)
            {
                // move to position
                if (WowInterface.ObjectManager.Player.Position.GetDistance(objectPositionCombo.Item2) > 3.0)
                {
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, objectPositionCombo.Item2);
                }
            }
        }

        public void CompleteQuest()
        {
            if (!HasQuest || !Finished) { Returned = true; return; }

            (WowObject, Vector3) objectPositionCombo = GetEndObject();

            if (objectPositionCombo.Item1 != null)
            {
                // move to unit / object
                if (WowInterface.ObjectManager.Player.Position.GetDistance(objectPositionCombo.Item1.Position) > 3.0)
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
                        // TODO: get best reward
                        WowInterface.HookManager.CompleteQuestAndGetReward(WowInterface.ObjectManager.Player.GetQuestlogEntries.FindIndex(e => e.Id == Id) + 1, 1, GossipId);
                    }

                    ActionToggle = !ActionToggle;
                }
            }
            else if (objectPositionCombo.Item2 != default)
            {
                // move to position
                if (WowInterface.ObjectManager.Player.Position.GetDistance(objectPositionCombo.Item2) > 3.0)
                {
                    WowInterface.MovementEngine.SetMovementAction(Movement.Enums.MovementAction.Moving, objectPositionCombo.Item2);
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