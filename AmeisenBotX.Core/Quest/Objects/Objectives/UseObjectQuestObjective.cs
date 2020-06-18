using AmeisenBotX.Core.Data.Objects.WowObject;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class UseObjectQuestObjective : IQuestObjective
    {
        public UseObjectQuestObjective(WowInterface wowInterface, int objectDisplayId, UseObjectQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            ObjectDisplayId = objectDisplayId;
            Condition = condition;
        }

        public delegate bool UseObjectQuestObjectiveCondition();

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private UseObjectQuestObjectiveCondition Condition { get; }

        private int ObjectDisplayId { get; }

        private WowGameobject WowGameobject { get; set; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished || WowInterface.ObjectManager.Player.IsCasting) { return; }

            WowGameobject = WowInterface.ObjectManager.GetClosestWowGameobjectByDisplayId(ObjectDisplayId);

            if (WowGameobject != null)
            {
                WowInterface.HookManager.StopClickToMoveIfActive();
                WowInterface.MovementEngine.Reset();

                WowInterface.HookManager.WowObjectOnRightClick(WowGameobject);
            }
        }
    }
}