namespace AmeisenBotX.Wow.Combatlog.Objects
{
    public interface ISubjectBasicCombatLogEntry
    {
        void Notify();

        void Register(IObserverBasicCombatLogEntry observer);

        void Unregister(IObserverBasicCombatLogEntry observer);
    }
}