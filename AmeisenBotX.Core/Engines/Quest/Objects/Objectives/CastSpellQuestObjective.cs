namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public delegate bool CastSpellQuestObjectiveCondition();

    public class CastSpellQuestObjective : IQuestObjective
    {
        public CastSpellQuestObjective(AmeisenBotInterfaces bot, int spellId, CastSpellQuestObjectiveCondition condition)
        {
            Bot = bot;
            SpellId = spellId;
            Condition = condition;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private CastSpellQuestObjectiveCondition Condition { get; }

        private int SpellId { get; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            Bot.Movement.Reset();
            Bot.Wow.StopClickToMove();
            Bot.Wow.CastSpellById(SpellId);
        }
    }
}