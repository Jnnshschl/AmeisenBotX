namespace AmeisenBotX.Wow.Combatlog.Objects
{
    public interface ISubjectBasicCombatlogEntry
    {
        void Notify();

        void Register(IObserverBasicCombatlogEntry observer);

        void Unregister(IObserverBasicCombatlogEntry observer);
    }
}