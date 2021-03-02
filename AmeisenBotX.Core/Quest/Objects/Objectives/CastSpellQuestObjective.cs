namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool CastSpellQuestObjectiveCondition();

    public class CastSpellQuestObjective : IQuestObjective
    {
        public CastSpellQuestObjective(WowInterface wowInterface, int spellId, CastSpellQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            SpellId = spellId;
            Condition = condition;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private CastSpellQuestObjectiveCondition Condition { get; }

        private int SpellId { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished || WowInterface.Player.IsCasting) { return; }

            WowInterface.MovementEngine.Reset();
            WowInterface.HookManager.WowStopClickToMove();
            WowInterface.HookManager.LuaCastSpellById(SpellId);
        }
    }
}