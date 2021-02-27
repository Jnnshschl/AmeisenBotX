namespace AmeisenBotX.Core.Data.CombatLog.Objects
{
    public interface ISubjectBasicCombatLogEntry
    {
        void Notify();

        void Register(IObserverBasicCombatLogEntry observer);

        void Unregister(IObserverBasicCombatLogEntry observer);
    }
}