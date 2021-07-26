namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public delegate bool CastVehicleSpellQuestObjectiveCondition();

    public class CastVehicleSpellQuestObjective : IQuestObjective
    {
        public CastVehicleSpellQuestObjective(AmeisenBotInterfaces bot, int spellId, CastVehicleSpellQuestObjectiveCondition condition)
        {
            Bot = bot;
            SpellId = spellId;
            Condition = condition;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private CastVehicleSpellQuestObjectiveCondition Condition { get; }

        private int SpellId { get; }

        public void Execute()
        {
            if (Finished || Bot.Objects.Vehicle.IsCasting) { return; }

            Bot.Movement.Reset();
            Bot.Wow.StopClickToMove();
            Bot.Wow.CastSpellById(SpellId);
        }
    }
}