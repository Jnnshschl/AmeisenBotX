using System;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class CastSpellQuestObjective(AmeisenBotInterfaces bot, int spellId, Func<bool> condition) : IQuestObjective
    {
        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; } = bot;

        private Func<bool> Condition { get; } = condition;

        private int SpellId { get; } = spellId;

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            Bot.Movement.Reset();
            Bot.Wow.StopClickToMove();
            Bot.Wow.CastSpellById(SpellId);
        }
    }
}