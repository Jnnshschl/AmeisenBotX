namespace AmeisenBotX.Core.Quest.Objects.Objectives
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

        private CastSpellQuestObjectiveCondition Condition { get; }

        private int SpellId { get; }

        private AmeisenBotInterfaces Bot { get; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            Bot.Movement.Reset();
            Bot.Wow.WowStopClickToMove();
            Bot.Wow.LuaCastSpellById(SpellId);
        }
    }
}