namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool CastVehicleSpellQuestObjectiveCondition();

    public class CastVehicleSpellQuestObjective : IQuestObjective
    {
        public CastVehicleSpellQuestObjective(WowInterface wowInterface, int spellId, CastVehicleSpellQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            SpellId = spellId;
            Condition = condition;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private CastVehicleSpellQuestObjectiveCondition Condition { get; }

        private int SpellId { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished || WowInterface.ObjectManager.Vehicle.IsCasting) { return; }

            WowInterface.MovementEngine.Reset();
            WowInterface.HookManager.WowStopClickToMove();
            WowInterface.HookManager.LuaCastSpellById(SpellId);
        }
    }
}