﻿namespace AmeisenBotX.Core.Quest.Objects.Objectives
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

        private CastVehicleSpellQuestObjectiveCondition Condition { get; }

        private int SpellId { get; }

        private AmeisenBotInterfaces Bot { get; }

        public void Execute()
        {
            if (Finished || Bot.Objects.Vehicle.IsCasting) { return; }

            Bot.Movement.Reset();
            Bot.Wow.WowStopClickToMove();
            Bot.Wow.LuaCastSpellById(SpellId);
        }
    }
}