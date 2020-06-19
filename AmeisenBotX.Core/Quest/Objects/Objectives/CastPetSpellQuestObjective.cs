namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool CastPetSpellQuestObjectiveCondition();

    public class CastPetSpellQuestObjective : IQuestObjective
    {
        public CastPetSpellQuestObjective(WowInterface wowInterface, int spellId, CastPetSpellQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            SpellId = spellId;
            Condition = condition;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private CastPetSpellQuestObjectiveCondition Condition { get; }

        private int SpellId { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Finished || WowInterface.ObjectManager.Pet.IsCasting) { return; }

            WowInterface.HookManager.CastSpellById(SpellId);
        }
    }
}