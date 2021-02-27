namespace AmeisenBotX.Core.Data.CombatLog.Objects
{
    public interface IObserverBasicCombatLogEntry
    {
        void CombatLogChanged(BasicCombatLogEntry entry);
    }
}